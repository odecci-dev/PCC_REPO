using API_PCC.ApplicationModels;
using API_PCC.ApplicationModels.Common;
using API_PCC.Data;
using API_PCC.DtoModels;
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
        string status = "";
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
            public int UserId { get; set; }
            public int HerdId { get; set; }
            public List<FeedingTypeId> FeedingSystemId { get; set; }
            public List<BreedTypeId> BreedTypeId { get; set; }
            public int FarmerAffliation_Id { get; set; }
            public int FarmerClassification_Id { get; set; }
            public int CreatedBy { get; set; }
            public int? Group_Id { get; set; }
            public bool Is_Manager { get; set; }
            public string? Email { get; set; }

        }

        public class FarmerUpdateInfoModel
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Address { get; set; }
            public string? TelephoneNumber { get; set; }
            public string? MobileNumber { get; set; }
            public int HerdId { get; set; }
            public int FarmerAffliation_Id { get; set; }
            public int FarmerClassification_Id { get; set; }
            public string? Group_Id { get; set; }
            public bool Is_Manager { get; set; }
            public string? Email { get; set; }
            public int Updated_By { get; set; }
            public DateTime Updated_At { get; set; }
        }

        public class FarmerView
        {
            public int FarmerId { get; set; }
            public int UserId { get; set; }
            public List<int?> FarmerBreedTypes { get; set; }
            public List<int> FarmerFeedingSystems { get; set; }
            public int FarmerAffiliation_Id { get; set; }
            public int FarmerClassification_Id { get; set; }

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
        public async Task<ActionResult<FarmerView>> view(int id)
        {
            try
            {
                var farmer = await _context.Tbl_Farmers
                    .Where(f => !f.Is_Deleted && f.Id == id)
                    .FirstOrDefaultAsync();

                if (farmer == null)
                {
                    return NotFound($"Farmer with ID {id} not found.");
                }

                var breedTypes = await _context.TblFarmerBreedTypes
                    .Where(b => b.FarmerId == farmer.Id)
                    .Select(b => b.BreedTypeId)
                    .Distinct()
                    .ToListAsync();

                var feedingSystems = await _context.tbl_FarmerFeedingSystem
                    .Where(f => f.Farmer_Id == farmer.Id)
                    .Select(f => f.FeedingSystem_Id)
                    .Distinct()
                    .ToListAsync();

                var farmerView = new FarmerView
                {
                    FarmerId = farmer.Id,
                    UserId = farmer.User_Id,
                    FarmerAffiliation_Id = farmer.FarmerAffliation_Id,
                    FarmerClassification_Id = farmer.FarmerClassification_Id,
                    FarmerBreedTypes = breedTypes,
                    FarmerFeedingSystems = feedingSystems
                };

                return Ok(farmerView);
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

            string herd = $@"SELECT * FROM H_Buff_Herd WHERE id = '{model.HerdId}'";
            DataTable tbl_herd = db.SelectDb(herd).Tables[0];

            string isAffil = $@"SELECT * FROM H_Farmer_Affiliation WHERE Id = '{model.FarmerAffliation_Id}'";
            DataTable tbl_isAffil = db.SelectDb(isAffil).Tables[0];

            string isclassification = $@"SELECT * FROM H_Farmer_Affiliation WHERE Id = '{model.FarmerClassification_Id}'";
            DataTable tbl_isclassificationl = db.SelectDb(isclassification).Tables[0];

            if (tbl_isfarmer.Rows.Count != 0)
            {
                return BadRequest("User is already a Farmer");
            }

            if (tbl_herd.Rows.Count == 0)
            {
                return BadRequest("Herd id does not exist");
            }

            if (tbl_isAffil.Rows.Count == 0)
            {
                return BadRequest("Affiliation System Id does not exist");
            }
            if (tbl_isclassificationl.Rows.Count == 0)
            {
                return BadRequest("Classification System Id does not exist");
            }

            foreach (var feed in model.FeedingSystemId)
            {
                string isfeeding = $@"SELECT * FROM H_Feeding_System WHERE Id = '{feed.FarmerFeedId}'";
                DataTable tbl_isfeeding = db.SelectDb(isfeeding).Tables[0];

                if (tbl_isfeeding.Rows.Count == 0)
                {
                    return BadRequest($"Feeding System Id {feed.FarmerFeedId} does not exist");
                }
            }
            foreach (var breed in model.BreedTypeId)
            {
                string isbreed = $@"SELECT * FROM A_Breed WHERE Id = '{breed.FarmerBreedId}'";
                DataTable tbl_isbreed = db.SelectDb(isbreed).Tables[0];

                if (tbl_isbreed.Rows.Count == 0)
                {
                    return BadRequest($"Breed Type Id {breed.FarmerBreedId} does not exist");
                }
            }

                try
            {
                var farmer = new TblFarmers
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    TelephoneNumber = model.TelephoneNumber,
                    MobileNumber = model.MobileNumber,
                    User_Id = model.UserId,
                    Group_Id = model.Group_Id,
                    Is_Manager = model.Is_Manager,
                    FarmerClassification_Id = model.FarmerClassification_Id,
                    FarmerAffliation_Id = model.FarmerAffliation_Id,
                    Created_By = model.CreatedBy,
                    Created_At = DateTime.Now,
                    Email = model.Email,
                    Deleted_At = DateTime.Now,
                    Is_Deleted = false
                };

                _context.Tbl_Farmers.Add(farmer);
                await _context.SaveChangesAsync();

                generatedFarmerId = farmer.Id;

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

                Insert += $@"INSERT INTO tbl_HerdFarmer (Herd_Id, Farmer_Id) VALUES ({model.HerdId}, {generatedFarmerId});";


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

        [HttpPut("{id}")]
        public async Task<IActionResult> update(int id, FarmerUpdateInfoModel farmerUpdateInfo)
        {


            DataTable farmerTable = db.SelectDb_WithParamAndSorting(QueryBuilder.buildFarmerSearchById(), null, populateSqlParameters(id));

            if (farmerTable.Rows.Count == 0)
            {
                status = "No records matched!";
                return Conflict(status);
            }


            DataTable farmerDuplicateCheck = db.SelectDb_WithParamAndSorting(QueryBuilder.buildFarmerSearchByFirstNameLastNameAddress(), null, populateSqlParameters(id, farmerUpdateInfo));

            if (farmerDuplicateCheck.Rows.Count > 0)
            {
                status = "Entity already exists";
                return Conflict(status);
            }

            var farmer = _context.Tbl_Farmers
                    .Single(x => x.Id == id);

            try
            {

                farmer = populateFarmerDetails(farmer, farmerUpdateInfo);


                _context.Entry(farmer).State = EntityState.Modified;
                _context.SaveChanges();
                status = "Update Successful!";
                dbmet.InsertAuditTrail("Update Farmer Details" + " " + status, DateTime.Now.ToString("yyyy-MM-dd"), "Herd Module", farmer.Updated_By.ToString(), "0");
                return Ok(status);
            }
            catch (Exception ex)
            {

                return Problem(ex.GetBaseException().ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> delete(DeletionModel deletionModel)
        {

            if (_context.Tbl_Farmers == null)
            {
                return Problem("Entity set 'PCC_DEVContext.Tbl_Farmers' is null!");
            }

            var farmer = await _context.Tbl_Farmers.FindAsync(deletionModel.id);
            if (farmer == null || farmer.Is_Deleted)
            {
                return Conflict("No records matched!");
            }

            try
            {
                farmer.Is_Deleted = true;
                farmer.Deleted_At = DateTime.Now;
                farmer.Deleted_By = int.Parse(deletionModel.deletedBy);
                _context.Entry(farmer).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok("Deletion Successful!");
            }
            catch (Exception ex)
            {

                return Problem(ex.GetBaseException().ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> restore(RestorationModel restorationModel)
        {

            if (_context.Tbl_Farmers == null)
            {
                return Problem("Entity set 'PCC_DEVContext.Tbl_Farmers' is null!");
            }

            var farmer = await _context.Tbl_Farmers.FindAsync(restorationModel.id);
            if (farmer == null || !farmer.Is_Deleted)
            {
                return Conflict("No deleted records matched!");
            }

            try
            {
                farmer.Is_Deleted = !farmer.Is_Deleted;
                farmer.Deleted_At = null;
                farmer.Deleted_By = null;
                farmer.Restored_At = DateTime.Now;
                farmer.Restored_By = int.Parse(restorationModel.restoredBy);

                _context.Entry(farmer).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok("Restoration Successful!");
            }
            catch (Exception ex)
            {

                return Problem(ex.GetBaseException().ToString());
            }
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

        private List<TblFarmerVM> convertDataRowListToFarmerlist(List<DataRow> dataRowList)
        {
            var farmerList = new List<TblFarmerVM>();

            foreach (DataRow dataRow in dataRowList)
            {
                var farmerModel = DataRowToObject.ToObject<TblFarmerVM>(dataRow);

                try
                {
                    // Fetch BreedType descriptions
                    string breedTypeQuery = $@"
                        SELECT DISTINCT tbl_FarmerBreedType.BreedType_Id, A_Breed.Breed_Desc 
                        FROM tbl_FarmerBreedType 
                        JOIN A_Breed ON tbl_FarmerBreedType.BreedType_Id = A_Breed.id  
                        WHERE Farmer_Id = '{farmerModel.Id}'";

                    DataTable farmerBreedTypeList = db.SelectDb(breedTypeQuery).Tables[0];
                    var breedTypeCodes = new List<string>();
                    foreach (DataRow row in farmerBreedTypeList.Rows)
                    {
                        breedTypeCodes.Add(row["Breed_Desc"].ToString());
                    }

                    string feedingSystemQuery = $@"
                        SELECT DISTINCT tbl_FarmerFeedingSystem.FeedingSystem_Id, H_Feeding_System.FeedingSystemDesc 
                        FROM tbl_FarmerFeedingSystem
                        JOIN H_Feeding_System ON tbl_FarmerFeedingSystem.FeedingSystem_Id = H_Feeding_System.id 
                        WHERE Farmer_Id = '{farmerModel.Id}'";

                    DataTable farmerFeedingSystemList = db.SelectDb(feedingSystemQuery).Tables[0];
                    var feedingTypeCodes = new List<string>();
                    foreach (DataRow row in farmerFeedingSystemList.Rows)
                    {
                        feedingTypeCodes.Add(row["FeedingSystemDesc"].ToString());
                    }

                    var farmerDetail = _context.TblUsersModels.FirstOrDefault(f => f.Id == (int)dataRow["User_Id"]);
                    if (farmerDetail != null)
                    {
                        farmerModel.FirstName = farmerDetail.Fname;
                        farmerModel.LastName = farmerDetail.Lname;
                        farmerModel.Address = farmerDetail.Address;
                        farmerModel.TelephoneNumber = farmerDetail.Cno;
                        farmerModel.MobileNumber = farmerDetail.Cno;
                        farmerModel.Email = farmerDetail.Email;
                        farmerModel.HerdId = farmerDetail.HerdId;
                        farmerModel.Center = farmerDetail.CenterId;
                    }

                    farmerModel.FarmerBreedTypes = breedTypeCodes;
                    farmerModel.FarmerFeedingSystems = feedingTypeCodes;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing farmer data: " + ex.Message);
                }

                farmerList.Add(farmerModel);
            }

            return farmerList;
        }


        private TblFarmers populateFarmerDetails(TblFarmers farmerModel, FarmerUpdateInfoModel farmerInfo)
        {
            if (!string.IsNullOrEmpty(farmerInfo.FirstName))
            {
                farmerModel.FirstName = farmerInfo.FirstName;
            }
            if (!string.IsNullOrEmpty(farmerInfo.LastName))
            {
                farmerModel.LastName = farmerInfo.LastName;
            }
            if (!string.IsNullOrEmpty(farmerInfo.Address))
            {
                farmerModel.Address = farmerInfo.Address;
            }
            if (!string.IsNullOrEmpty(farmerInfo.TelephoneNumber))
            {
                farmerModel.TelephoneNumber = farmerInfo.TelephoneNumber;
            }
            if (!string.IsNullOrEmpty(farmerInfo.MobileNumber))
            {
                farmerModel.MobileNumber = farmerInfo.MobileNumber;
            }
            if (!string.IsNullOrEmpty(farmerInfo.Group_Id))
            {
                farmerModel.Group_Id = int.Parse(farmerInfo.Group_Id);
            }
            if (!string.IsNullOrEmpty(farmerInfo.Is_Manager.ToString()))
            {
                farmerModel.Is_Manager = (bool)farmerInfo.Is_Manager;
            }
            if (!string.IsNullOrEmpty(farmerInfo.FarmerAffliation_Id.ToString()))
            {
                farmerModel.FarmerAffliation_Id = farmerInfo.FarmerAffliation_Id;
            }
            if (!string.IsNullOrEmpty(farmerInfo.FarmerClassification_Id.ToString()))
            {
                farmerModel.FarmerClassification_Id = farmerInfo.FarmerClassification_Id;
            }
            if (!string.IsNullOrEmpty(farmerInfo.Email))
            {
                farmerModel.Email = farmerInfo.Email;
            }
            if (!string.IsNullOrEmpty(farmerInfo.Updated_By.ToString()))
            {
                farmerModel.Updated_By = farmerInfo.Updated_By;
            }
            if (!string.IsNullOrEmpty(farmerInfo.Updated_At.ToString()))
            {
                farmerModel.Updated_At = farmerInfo.Updated_At;
            }
            return farmerModel;
        }

        private SqlParameter[] populateSqlParameters(FarmerSearchFilterModel searchFilter)
        {
            var sqlParameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(searchFilter.searchValue))
            {
                sqlParameters.Add(new SqlParameter
                {
                    ParameterName = "@SearchParam",
                    Value = searchFilter.searchValue,
                    SqlDbType = System.Data.SqlDbType.VarChar,
                });
            }

            if (!string.IsNullOrEmpty(searchFilter.herdId))
            {
                sqlParameters.Add(new SqlParameter
                {
                    ParameterName = "@HerdId",
                    Value = searchFilter.herdId,
                    SqlDbType = System.Data.SqlDbType.VarChar,
                });
            }

            if (searchFilter.center.HasValue)
            {
                sqlParameters.Add(new SqlParameter
                {
                    ParameterName = "@CenterId",
                    Value = searchFilter.center,
                    SqlDbType = System.Data.SqlDbType.Int,
                });
            }

            if (searchFilter.breedType != null && searchFilter.breedType.Any())
            {
                for (int i = 0; i < searchFilter.breedType.Count; i++)
                {
                    sqlParameters.Add(new SqlParameter
                    {
                        ParameterName = $"@BreedType{i}",
                        Value = int.Parse(searchFilter.breedType[i]),
                        SqlDbType = System.Data.SqlDbType.Int,
                    });
                }
            }

            if (searchFilter.feedingSystem != null && searchFilter.feedingSystem.Any())
            {
                for (int i = 0; i < searchFilter.feedingSystem.Count; i++)
                {
                    sqlParameters.Add(new SqlParameter
                    {
                        ParameterName = $"@FeedingSystem{i}",
                        Value = int.Parse(searchFilter.feedingSystem[i]),
                        SqlDbType = System.Data.SqlDbType.Int,
                    });
                }
            }

            return sqlParameters.ToArray();
        }



        private SqlParameter[] populateSqlParameters(int id)
        {

            var sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "Id",
                Value = id,
                SqlDbType = System.Data.SqlDbType.Int,
            });

            return sqlParameters.ToArray();
        }

        private SqlParameter[] populateSqlParameters(int id, FarmerUpdateInfoModel farmerSaveInfo)
        {

            var sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "Id",
                Value = id,
                SqlDbType = System.Data.SqlDbType.Int,
            });


            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "FirstName",
                Value = farmerSaveInfo.FirstName ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });


            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "LastName",
                Value = farmerSaveInfo.LastName ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });

            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "Address",
                Value = farmerSaveInfo.Address ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });

            return sqlParameters.ToArray();
        }
    }
}
