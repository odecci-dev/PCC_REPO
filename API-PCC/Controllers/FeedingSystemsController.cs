﻿using API_PCC.ApplicationModels;
using API_PCC.ApplicationModels.Common;
using API_PCC.Data;
using API_PCC.Manager;
using API_PCC.Models;
using API_PCC.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.SqlClient;

namespace API_PCC.Controllers
{
    [Authorize("ApiKey")]
    [Route("[controller]/[action]")]
    [ApiController]
    public class FeedingSystemsController : ControllerBase
    {

        private readonly PCC_DEVContext _context;
        DbManager db = new DbManager();

        public FeedingSystemsController(PCC_DEVContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<FeedingSystemResponseModel>>> List(CommonSearchFilterModel searchFilter)
        {
            sanitizeInput(searchFilter);
            try
            {
                DataTable queryResult = db.SelectDb_WithParamAndSorting(QueryBuilder.buildFeedingSystemSearchQuery(searchFilter), null, populateSqlParameters(searchFilter));
                var result = buildFeedingSystemPagedModel(searchFilter, queryResult);
                return Ok(result);
            }

            catch (Exception ex)
            {
                return Problem(ex.GetBaseException().ToString());
            }
        }

        private SqlParameter[] populateSqlParameters(CommonSearchFilterModel searchFilter)
        {

            var sqlParameters = new List<SqlParameter>();

            if (searchFilter.searchParam != null && searchFilter.searchParam != "")
            {
                sqlParameters.Add(new SqlParameter
                {
                    ParameterName = "SearchParam",
                    Value = searchFilter.searchParam ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.VarChar,
                });
            }

            return sqlParameters.ToArray();
        }

        private void sanitizeInput(CommonSearchFilterModel searchFilter)
        {
            searchFilter.searchParam = StringSanitizer.sanitizeString(searchFilter.searchParam);
        }

        private List<FeedingSystemPagedModel> buildFeedingSystemPagedModel(CommonSearchFilterModel searchFilter, DataTable dt)
        {

            int pagesize = searchFilter.pageSize == 0 ? 10 : searchFilter.pageSize;
            int page = searchFilter.page == 0 ? 1 : searchFilter.page;
            var items = (dynamic)null;

            int totalItems = dt.Rows.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pagesize);
            items = dt.AsEnumerable().Skip((page - 1) * pagesize).Take(pagesize).ToList();

            var feedingSystemModels = convertDataRowListToFeedingSystemlist(items);
            List<FeedingSystemResponseModel> feedingSystemResponseModels = convertFeedingSystemListToResponseModelList(feedingSystemModels);

            var result = new List<FeedingSystemPagedModel>();
            var item = new FeedingSystemPagedModel();

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
            item.items = feedingSystemResponseModels;
            result.Add(item);

            return result;
        }

        private List<HFeedingSystem> convertDataRowListToFeedingSystemlist(List<DataRow> dataRowList)
        {
            var feedingSystemList = new List<HFeedingSystem>();

            foreach (DataRow dataRow in dataRowList)
            {
                var feedingSystemModel = DataRowToObject.ToObject<HFeedingSystem>(dataRow);
                feedingSystemList.Add(feedingSystemModel);
            }

            return feedingSystemList;
        }

        private List<FeedingSystemResponseModel> convertFeedingSystemListToResponseModelList(List<HFeedingSystem> feedingSystemList)
        {
            var feedingSystemResponseModels = new List<FeedingSystemResponseModel>();

            foreach (HFeedingSystem feedingSystem in feedingSystemList)
            {
                var feedingSystemResponseModel = new FeedingSystemResponseModel()
                {
                    feedingSystemCode = feedingSystem.Id.ToString(),
                    feedingSystemDesc = feedingSystem.FeedingSystemDesc
                };
                feedingSystemResponseModels.Add(feedingSystemResponseModel);
            }
            return feedingSystemResponseModels;
        }

        // GET: FeedingSystems/search/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HFeedingSystem>> Search(int id)
        {
            if (_context.HFeedingSystems == null)
            {
                return Problem("Entity set 'PCC_DEVContext.Feeding Sytem' is null!");
            }
            var hFeedingSystem = await _context.HFeedingSystems.FindAsync(id);

            if (hFeedingSystem == null || hFeedingSystem.DeleteFlag)
            {
                return Conflict("No records found!");
            }

            return hFeedingSystem;
        }

        // PUT: FeedingSystems/update/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, HFeedingSystem hFeedingSystem)
        {
            if (_context.HFeedingSystems == null)
            {
                return Problem("Entity set 'PCC_DEVContext.Feeding System' is null!");
            }

            var feedingSystem = _context.HFeedingSystems.AsNoTracking().Where(feedSys => !feedSys.DeleteFlag && feedSys.Id == id).FirstOrDefault();

            if (feedingSystem == null)
            {
                return Conflict("No records matched!");
            }

            if (id != hFeedingSystem.Id)
            {
                return Conflict("Ids mismatched!");
            }

            bool hasDuplicateOnUpdate = (_context.HFeedingSystems?.Any(fs => !fs.DeleteFlag && fs.FeedingSystemCode == hFeedingSystem.FeedingSystemCode && fs.FeedingSystemDesc == hFeedingSystem.FeedingSystemDesc && fs.Id != id)).GetValueOrDefault();

            // check for duplication
            if (hasDuplicateOnUpdate)
            {
                return Conflict("Entity already exists");
            }

            try
            {
                _context.Entry(hFeedingSystem).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok("Update Successful!");
            }
            catch (Exception ex)
            {
                
                return Problem(ex.GetBaseException().ToString());
            }

        }

        // POST: FeedingSystems/save
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<HFeedingSystem>> save(HFeedingSystem hFeedingSystem)
        {
            if (_context.HFeedingSystems == null)
            {
                return Problem("Entity set 'PCC_DEVContext.Feeding Sytem' is null!");
            }

            bool hasDuplicateOnSave = (_context.HFeedingSystems?.Any(fs => !fs.DeleteFlag && fs.FeedingSystemCode == hFeedingSystem.FeedingSystemCode && fs.FeedingSystemDesc == hFeedingSystem.FeedingSystemDesc)).GetValueOrDefault();

            if (hasDuplicateOnSave)
            {
                return Conflict("Entity already exists");
            }

            try
            {
                _context.HFeedingSystems.Add(hFeedingSystem);
                await _context.SaveChangesAsync();

                return CreatedAtAction("save", new { id = hFeedingSystem.Id }, hFeedingSystem);
            }
            catch (Exception ex)
            {
                
                return Problem(ex.GetBaseException().ToString());
            }
        }

        // POST: FeedingSystems/delete/5
        [HttpPost]
        public async Task<IActionResult> delete(DeletionModel deletionModel)
        {
            if (_context.HFeedingSystems == null)
            {
                return Problem("Entity set 'PCC_DEVContext.Feeding Sytem' is null!");
            }
            var hFeedingSystem = await _context.HFeedingSystems.FindAsync(deletionModel.id);
            if (hFeedingSystem == null || hFeedingSystem.DeleteFlag)
            {
                return Conflict("No records matched!");
            }

            bool FeedingSystemDescExistsInBuffHerd = _context.HBuffHerds.Include(herd => herd.feedingSystem).Any(buffHerd => !buffHerd.DeleteFlag && buffHerd.feedingSystem.Any(fs => fs.FeedingSystemCode.Equals(hFeedingSystem.FeedingSystemCode)));

            if (FeedingSystemDescExistsInBuffHerd)
            {
                return Conflict("Used by other table!");
            }

            try
            {
                hFeedingSystem.DeleteFlag = true;
                hFeedingSystem.DateDeleted = DateTime.Now;
                hFeedingSystem.DeletedBy = deletionModel.deletedBy;
                hFeedingSystem.DateRestored = null;
                hFeedingSystem.RestoredBy = "";
                _context.Entry(hFeedingSystem).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok("Deletion Successful!");
            }
            catch (Exception ex)
            {
                
                return Problem(ex.GetBaseException().ToString());
            }
        }
        

        // GET: FeedingSystems/view
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HFeedingSystem>>> view()
        {
            if (_context.HFeedingSystems == null)
            {
                return Problem("Entity set 'PCC_DEVContext.HFeedingSystem' is null.");
            }
            return await _context.HFeedingSystems.Where(feedingSystem => !feedingSystem.DeleteFlag).ToListAsync();
        }

        // POST: FeedingSystems/restore/
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> restore(RestorationModel restorationModel)
        {

            if (_context.HFeedingSystems == null)
            {
                return Problem("Entity set 'PCC_DEVContext.HFeedingSystem' is null.");
            }

            var feedingSystem = await _context.HFeedingSystems.FindAsync(restorationModel.id);

            if (feedingSystem == null || !feedingSystem.DeleteFlag)
            {
                return Conflict("No deleted records matched!");
            }

            try
            {
                feedingSystem.DeleteFlag = !feedingSystem.DeleteFlag;
                feedingSystem.DateDeleted = null;
                feedingSystem.DeletedBy = "";
                feedingSystem.DateRestored = DateTime.Now;
                feedingSystem.RestoredBy = restorationModel.restoredBy;

                _context.Entry(feedingSystem).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok("Restoration Successful!");
            }
            catch (Exception ex)
            {
                
                return Problem(ex.GetBaseException().ToString());
            }
        }

        private bool HFeedingSystemExists(int id)
        {
            return (_context.HFeedingSystems?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
