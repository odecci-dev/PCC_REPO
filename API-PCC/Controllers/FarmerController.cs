using API_PCC.ApplicationModels;
using API_PCC.ApplicationModels.Common;
using API_PCC.Data;
using API_PCC.EntityModels;
using API_PCC.Manager;
using API_PCC.Models;
using API_PCC.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.SqlClient;
using static API_PCC.Controllers.BreedRegistryHerdController;
using static API_PCC.Controllers.UserManagementController;
namespace API_PCC.Controllers
{
    [Authorize("ApiKey")]
    [Route("[controller]/[action]")]
    [ApiController]
    public class FarmerController : ControllerBase
    {
        private readonly PCC_DEVContext _context;
        DbManager db = new DbManager();
        DBMethods dbmet = new DBMethods();
        public FarmerController(PCC_DEVContext context)
        {
            _context = context;
        }

        public class FarmerSaveInfoModel
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Address { get; set; }
            public string? TelephoneNumber { get; set; }
            public string? MobileNumber { get; set; }
            public int? UserId { get; set; }
            public List<FeedingTypeId> FeedingSystemId { get; set; }
            public List<BreedTypeId> BreedTypeId { get; set; }
            public int FarmerAffliation_Id { get; set; }
            public int FarmerClassification_Id { get; set; }
            public int? CreatedBy { get; set; }
            public string? Group_Id { get; set; }
            public string? Is_Manager { get; set; }
            public string? Email { get; set; }

        }

        private void sanitizeInput(CommonSearchFilterModel searchFilter)
        {
            searchFilter.searchParam = StringSanitizer.sanitizeString(searchFilter.searchParam);
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<FarmerPagedModel>>> list(FarmerSearchFilterModel searchFilter)
        {
            try
            {
                DataTable queryResult = db.SelectDb_WithParamAndSorting(QueryBuilder.buildFarmerSearch(searchFilter), null, populateSqlParameters(searchFilter));
                var result = farmersPagedModel(searchFilter, queryResult);
                return Ok(result); ;
            }
            catch (Exception ex)
            {
                return Problem(ex.GetBaseException().ToString());
            }
        }

        [HttpPost]
public async Task<IActionResult> save(FarmerSaveInfoModel model)
{
    int generatedFarmerId = 0;
    string Insert = "";
    string isfarmer = $@"SELECT * FROM tbl_Farmers WHERE User_Id = '{model.UserId}'";

    DataTable tbl_isfarmer = db.SelectDb(isfarmer).Tables[0];
    if (tbl_isfarmer.Rows.Count != 0)
    {
        return BadRequest("User is already a Farmer");
    }

    try
    {
        // Insert Farmer and get the generated ID
        string sqlFarmer = $@"
        INSERT INTO [dbo].[Tbl_Farmers]
               ([FirstName]
               ,[LastName]
               ,[Address]
               ,[TelephoneNumber]
               ,[MobileNumber]
               ,[User_Id]
               ,[Group_Id]
               ,[Is_Manager]
               ,[FarmerClassification_Id]
               ,[FarmerAffliation_Id]
               ,[Created_By]
               ,[Created_At]
               ,[Email]
               ,[Deleted_At]
               ,[Is_Deleted])
         VALUES
           ('{model.FirstName}',
            '{model.LastName}',
            '{model.Address}',
            '{model.TelephoneNumber}',
            '{model.MobileNumber}',
            '{model.UserId}',
            '{model.Group_Id}',
            '{model.Is_Manager}',
            '{model.FarmerClassification_Id}',
            '{model.FarmerAffliation_Id}',
            '{model.CreatedBy}',
            '{DateTime.Now:yyyy-MM-dd}',
            '{model.Email}',
            '{DateTime.Now:yyyy-MM-dd}',
            '0');";

        db.DB_WithParam(sqlFarmer);

        // Retrieve the ID of the newly inserted Farmer
        generatedFarmerId = _context.Tbl_Farmers
            .OrderByDescending(f => f.Created_At)
            .Select(f => f.Id)
            .FirstOrDefault();

        // Check and prepare Feeding System insertions
        foreach (var feed in model.FeedingSystemId)
        {
            string isfeeding = $@"SELECT * FROM H_Feeding_System WHERE Id = '{feed.FarmerFeedId}'";
            DataTable tbl_isfeeding = db.SelectDb(isfeeding).Tables[0];

            if (tbl_isfeeding.Rows.Count == 0)
            {
                return BadRequest($"Feeding System Id {feed.FarmerFeedId} does not exist");
            }

            Insert += $@"
            INSERT INTO [dbo].[tbl_FarmerFeedingSystem]
                   ([Farmer_Id]
                   ,[FeedingSystem_Id]
                   ,[Created_By]
                   ,[Is_Deleted]
                   ,[Created_At])
            VALUES
               ('{generatedFarmerId}',
                '{feed.FarmerFeedId}',
                '{model.CreatedBy}',
                '0',
                '{DateTime.Now:yyyy-MM-dd}');";
        }

        // Check and prepare Breed Type insertions
        foreach (var breed in model.BreedTypeId)
        {
            string isbreed = $@"SELECT * FROM A_Breed WHERE Id = '{breed.FarmerBreedId}'";
            DataTable tbl_isbreed = db.SelectDb(isbreed).Tables[0];

            if (tbl_isbreed.Rows.Count == 0)
            {
                return BadRequest($"Breed Type Id {breed.FarmerBreedId} does not exist");
            }

            Insert += $@"
            INSERT INTO [dbo].[tbl_FarmerBreedType]
                   ([Farmer_Id]
                   ,[BreedType_Id]
                   ,[Created_By]
                   ,[Is_Deleted]
                   ,[Created_At])
            VALUES
               ('{generatedFarmerId}',
                '{breed.FarmerBreedId}',
                '{model.UserId}',
                '0',
                '{DateTime.Now:yyyy-MM-dd}');";
        }

        // Check affiliation and classification validity
        string isAffil = $@"SELECT * FROM H_Farmer_Affiliation WHERE Id = '{model.FarmerAffliation_Id}'";
        DataTable tbl_isAffil = db.SelectDb(isAffil).Tables[0];

        string isclassification = $@"SELECT * FROM H_Farmer_Affiliation WHERE Id = '{model.FarmerClassification_Id}'";
        DataTable tbl_isclassificationl = db.SelectDb(isclassification).Tables[0];

        if (tbl_isAffil.Rows.Count == 0)
        {
            return BadRequest("Affiliation System Id does not exist");
        }
        if (tbl_isclassificationl.Rows.Count == 0)
        {
            return BadRequest("Classification System Id does not exist");
        }

        // Execute all insertions if there are any
        if (!string.IsNullOrEmpty(Insert))
        {
            db.DB_WithParam(Insert);
        }
    }
    catch (Exception ex)
    {
        return Problem(ex.GetBaseException().ToString());
    }

    return Ok("Successfully Saved. Farmer ID: " + generatedFarmerId);
}



        private List<FarmerPagedModel> farmersPagedModel(FarmerSearchFilterModel searchFilter, DataTable dt)
        {

            int pagesize = searchFilter.pageSize == 0 ? 10 : searchFilter.pageSize;
            int page = searchFilter.page == 0 ? 1 : searchFilter.page;
            var items = (dynamic)null;

            int totalItems = dt.Rows.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pagesize);
            items = dt.AsEnumerable().Skip((page - 1) * pagesize).Take(pagesize).ToList();

            var farmerModels = convertDataRowListToFarmerlist(items);

            var result = new List<FarmerPagedModel>();
            var item = new FarmerPagedModel();

            int pages = searchFilter.page == 0 ? 1 : searchFilter.page;
            item.CurrentPage = searchFilter.page == 0 ? "1" : searchFilter.page.ToString();
            int page_prev = pages - 1;

            double t_records = Math.Ceiling(Convert.ToDouble(totalItems) / Convert.ToDouble(pagesize));
            int page_next = searchFilter.page >= t_records ? 0 : pages + 1;
            item.NextPage = items.Count % pagesize >= 0 ? page_next.ToString() : "0";
            item.PrevPage = pages == 1 ? "0" : page_prev.ToString();
            item.TotalPage = t_records.ToString();
            item.PageSize = pagesize.ToString();
            item.TotalRecord = totalItems.ToString();
            item.items = farmerModels;
            result.Add(item);

            return result;
        }

        private List<TblFarmers> convertDataRowListToFarmerlist(List<DataRow> dataRowList)
        {
            var farmerList = new List<TblFarmers>();

            foreach (DataRow dataRow in dataRowList)
            {
                var farmerModel = DataRowToObject.ToObject<TblFarmers>(dataRow);
                farmerList.Add(farmerModel);
            }

            return farmerList;
        }

        private SqlParameter[] populateSqlParameters(FarmerSearchFilterModel searchFilter)
        {

            var sqlParameters = new List<SqlParameter>();

            if (searchFilter.searchValue != null && searchFilter.searchValue != "")
            {
                sqlParameters.Add(new SqlParameter
                {
                    ParameterName = "SearchParam",
                    Value = searchFilter.searchValue ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.VarChar,
                });
            }

            return sqlParameters.ToArray();
        }

    }
}
