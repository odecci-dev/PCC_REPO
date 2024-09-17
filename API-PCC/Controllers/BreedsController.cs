﻿using API_PCC.ApplicationModels;
using API_PCC.ApplicationModels.Common;
using API_PCC.Data;
using API_PCC.Manager;
using API_PCC.Models;
using API_PCC.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using System.Data;
using System.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
namespace API_PCC.Controllers
{
    [Authorize("ApiKey")]
    [Route("[controller]/[action]")]
    [ApiController]
    public class BreedsController : ControllerBase
    {
        private readonly PCC_DEVContext _context;
        DbManager db = new DbManager();
        DBMethods dbmet = new DBMethods();
        public BreedsController(PCC_DEVContext context)
        {
            _context = context;
        }

        // POST: Breeds/list
        [HttpPost]
        public async Task<ActionResult<IEnumerable<ABreed>>> list(CommonSearchFilterModel searchFilter)
        {

            try
            {
                var breedList = await buildBreedSearchQuery(searchFilter).ToListAsync();
                var result = buildHerdClassificationPagedModel(searchFilter, breedList);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(ex.GetBaseException().ToString());
            }
        }
        private IQueryable<ABreed> buildBreedSearchQuery(CommonSearchFilterModel searchFilter)
        {
            IQueryable<ABreed> query = _context.ABreeds;

            query = query.Where(breed => !breed.DeleteFlag);
            // assuming that you return all records when nothing is specified in the filter

            if (!searchFilter.searchParam.IsNullOrEmpty())
                query = query.Where(breed =>
                               breed.BreedCode.Contains(searchFilter.searchParam) ||
                               breed.BreedDesc.Contains(searchFilter.searchParam));

            return query;
        }

        // GET: Breeds/search/5
        [HttpGet("{breedCode}")]
        public async Task<ActionResult<BreedResponseModel>> search(string breedCode)
        {
            DataTable breedRecord = db.SelectDb_WithParamAndSorting(QueryBuilder.buildBreedSearchQueryByBreedCode(), null, populateSqlParameters(breedCode));

            if (breedRecord.Rows.Count == 0)
            {
                return Conflict("No records found!");
            }

            var breedModel = convertDataRowToBreed(breedRecord.Rows[0]);
            var breedResponseModel = convertBreedToResponseModel(breedModel);

            return Ok(breedResponseModel);
        }



        // GET: Breeds/view
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BreedResponseModel>>> view()
        {
            try
            {
                DataTable queryResult = db.SelectDb_WithParamAndSorting(QueryBuilder.buildBreedSearchQueryAll(), null, new SqlParameter[] { });
                if (queryResult.Rows.Count == 0)
                {
                    return Conflict("No records found!");
                }
                var breedModels = convertDataRowListToBreedList(queryResult.AsEnumerable().ToList());
                List<BreedResponseModel> breedResponseList = convertBreedListToResponseModelList(breedModels);

                return Ok(breedResponseList);
            }
            catch (Exception ex)
            {
                return Problem(ex.GetBaseException().ToString());
            }
        }

        // PUT: Breeds/update/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> update(int id, BreedUpdateModel breedUpdateModel)
        {
            //DataTable breedRecord = db.SelectDb_WithParamAndSorting(QueryBuilder.buildBreedSearchQueryById(), null, populateSqlParameters(id));

            //if (breedRecord.Rows.Count == 0)
            //{
            //    return Conflict("No records matched!");
            //}

            //DataTable breedDuplicateCheck = db.SelectDb_WithParamAndSorting(QueryBuilder.buildBreedDuplicateCheckUpdateQuery(), null, populateSqlParameters(id, breedUpdateModel));

            //// check for duplication
            //if (breedDuplicateCheck.Rows.Count > 0)
            //{
            //    return Conflict("Entity already exists");
            //}

            //try
            //{
            //    var breedModel = convertDataRowToBreed(breedRecord.Rows[0]);
            //    populateBreed(breedModel, breedUpdateModel);
            //    _context.Entry(breedModel).State = EntityState.Modified;
            //    await _context.SaveChangesAsync();

            //    return Ok("Update Successful!");
            //}
            //catch (Exception ex)
            //{

            //    return Problem(ex.GetBaseException().ToString());
            //}
            var breedRecord = _context.ABreeds.Where(breed => !breed.DeleteFlag && breed.Id.Equals(id)).FirstOrDefault();
            if (breedRecord == null)
            {
                return Conflict("No records matched!");
            }

            var breedDuplicateCheck = _context.ABreeds.Where(breed => !breed.DeleteFlag &&
                                                                       breed.Id.Equals(id) &&
                                                                       breed.BreedCode.Equals(breedUpdateModel.BreedCode) &&
                                                                       breed.BreedDesc.Equals(breedUpdateModel.BreedDesc)).FirstOrDefault();
            // check for duplication
            if (breedDuplicateCheck != null)
            {
                return Conflict("Entity already exists");
            }

            try
            {
                populateBreed(breedRecord, breedUpdateModel);
                _context.Entry(breedRecord).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                dbmet.InsertAuditTrail("Updated Breed  ID: " + id + "", DateTime.Now.ToString("yyyy-MM-dd"), "Breed Module", breedDuplicateCheck.UpdatedBy, "0");
           
                return Ok("Update Successful!");
            }
            catch (Exception ex)
            {

                return Problem(ex.GetBaseException().ToString());
            }
        }


        // POST: Breeds/save
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ABreed>> save(BreedRegistrationModel breedRegistrationModel)
        {
            //    DataTable breedRecord = db.SelectDb_WithParamAndSorting(QueryBuilder.buildBreedDuplicateCheckSaveQuery(), null, populateSqlParameters(breedRegistrationModel));

            //    // check for duplication
            //    if (breedRecord.Rows.Count > 0)
            //    {
            //        return Conflict("Entity already exists");
            //    }

            //    var breed = buildBreedRegistrationModel(breedRegistrationModel);

            //    try
            //    {
            //        _context.ABreeds.Add(breed);
            //        await _context.SaveChangesAsync();

            //        return Ok("Registration Successful");
            //    }
            //    catch (Exception ex)
            //    {

            //        return Problem(ex.GetBaseException().ToString());
            //    }
            //}

            //private ABreed buildBreedRegistrationModel(BreedRegistrationModel breedRegistrationModel)
            //{
            //    var breed = new ABreed()
            //    {
            //        BreedCode = breedRegistrationModel.BreedCode,
            //        BreedDesc = breedRegistrationModel.BreedDesc,
            //        Status = 1,
            //        CreatedBy = breedRegistrationModel.CreatedBy,
            //        DateCreated = DateTime.Now
            //    };
            //    return breed;
            var breedRecord = _context.ABreeds.Where(breed => !breed.DeleteFlag &&
                                                                 breed.BreedCode.Equals(breedRegistrationModel.BreedCode) &&
                                                                 breed.BreedDesc.Equals(breedRegistrationModel.BreedDesc)).FirstOrDefault();

            // check for duplication
            if (breedRecord != null)
            {
                return Conflict("Entity already exists");
            }

            var breed = buildBreedRegistrationModel(breedRegistrationModel);

            try
            {
                _context.ABreeds.Add(breed);
                await _context.SaveChangesAsync();
                dbmet.InsertAuditTrail("Save Breed  Code : " + breed.BreedCode + "", DateTime.Now.ToString("yyyy-MM-dd"), "Breed Module", breed.CreatedBy, "0");

                return Ok("Registration Successful");
            }
            catch (Exception ex)
            {

                return Problem(ex.GetBaseException().ToString());
            }
        }

        private ABreed buildBreedRegistrationModel(BreedRegistrationModel breedRegistrationModel)
        {
            var breed = new ABreed()
            {
                BreedCode = breedRegistrationModel.BreedCode,
                BreedDesc = breedRegistrationModel.BreedDesc,
                Status = 1,
                CreatedBy = breedRegistrationModel.CreatedBy,
                DateCreated = DateTime.Now
            };
            return breed;
        }

        // POST: Breeds/delete/5
        [HttpPost]
        public async Task<IActionResult> delete(DeletionModel deletionModel)
        {
            //DataTable breedRecord = db.SelectDb_WithParamAndSorting(QueryBuilder.buildBreedSearchQueryById(), null, populateSqlParameters(deletionModel.id));

            //if (breedRecord.Rows.Count == 0)
            //{
            //    return Conflict("No records matched!");
            //}

            //var breedModel = convertDataRowToBreed(breedRecord.Rows[0]);

            //try
            //{
            //    breedModel.DeleteFlag = true;
            //    breedModel.DateDeleted = DateTime.Now;
            //    breedModel.DeletedBy = deletionModel.deletedBy;
            //    breedModel.DateRestored = null;
            //    breedModel.RestoredBy = "";
            //    _context.Entry(breedModel).State = EntityState.Modified;
            //    await _context.SaveChangesAsync();

            //    return Ok("Deletion Successful!");
            //}
            //catch (Exception ex)
            //{

            //    return Problem(ex.GetBaseException().ToString());
            //}
            var breedModel = _context.ABreeds.Where(breed => !breed.DeleteFlag &&
                                                              breed.Id.Equals(deletionModel.id)).FirstOrDefault();
            if (breedModel == null)
            {
                return Conflict("No records matched!");
            }

            try
            {
                breedModel.DeleteFlag = true;
                breedModel.DateDeleted = DateTime.Now;
                breedModel.DeletedBy = deletionModel.deletedBy;
                breedModel.DateRestored = null;
                breedModel.RestoredBy = "";
                _context.Entry(breedModel).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                dbmet.InsertAuditTrail("Deleted Breed  Id : " + breedModel.Id     + "", DateTime.Now.ToString("yyyy-MM-dd"), "Breed Module", breedModel.DeletedBy, "0");
                return Ok("Deletion Successful!");
            }
            catch (Exception ex)
            {

                return Problem(ex.GetBaseException().ToString());
            }
        }

        // POST: Breeds/restore/
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> restore(RestorationModel restorationModel)
        {
            DataTable breedRecord = db.SelectDb_WithParamAndSorting(QueryBuilder.buildBreedDeletedSearchQueryById(), null, populateSqlParameters(restorationModel.id));

            if (breedRecord.Rows.Count == 0)
            {
                return Conflict("No deleted records matched!");
            }

            var breedModel = convertDataRowToBreed(breedRecord.Rows[0]);

            try
            {
                breedModel.DeleteFlag = !breedModel.DeleteFlag;
                breedModel.DateDeleted = null;
                breedModel.DeletedBy = "";
                breedModel.DateRestored = DateTime.Now;
                breedModel.RestoredBy = restorationModel.restoredBy;

                _context.Entry(breedModel).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                dbmet.InsertAuditTrail("Restore Breed  Id : " + breedModel.Id + "", DateTime.Now.ToString("yyyy-MM-dd"), "Breed Module", breedModel.RestoredBy, "0");

                return Ok("Restoration Successful!");
            }
            catch (Exception ex)
            {
                
                return Problem(ex.GetBaseException().ToString());
            }
        }

        private void sanitizeInput(CommonSearchFilterModel searchFilter)
        {
            searchFilter.searchParam = StringSanitizer.sanitizeString(searchFilter.searchParam);
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

        private SqlParameter[] populateSqlParameters(string breedCode)
        {
            var sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "BreedCode",
                Value = breedCode ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });

            return sqlParameters.ToArray();
        }

        private SqlParameter[] populateSqlParameters(BreedRegistrationModel breedRegistrationModel)
        {
            var sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "BreedCode",
                Value = breedRegistrationModel.BreedCode ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });

            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "BreedDesc",
                Value = breedRegistrationModel.BreedDesc ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });

            return sqlParameters.ToArray();
        }

        private SqlParameter[] populateSqlParameters(int id, BreedUpdateModel breedUpdateModel)
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
                ParameterName = "BreedCode",
                Value = breedUpdateModel.BreedCode ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });

            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "BreedDesc",
                Value = breedUpdateModel.BreedDesc ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });

            return sqlParameters.ToArray();
        }

        private List<BreedsPagedModel> buildHerdClassificationPagedModel(CommonSearchFilterModel searchFilter, List<ABreed> breedList)
        {
            int pagesize = searchFilter.pageSize == 0 ? 10 : searchFilter.pageSize;
            int page = searchFilter.page == 0 ? 1 : searchFilter.page;
            var items = (dynamic)null;

            int totalItems = breedList.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pagesize);
            items = breedList.Skip((page - 1) * pagesize).Take(pagesize).ToList();

            List<BreedResponseModel> breedResponseModels = convertBreedListToResponseModelList(items);

            var result = new List<BreedsPagedModel>();
            var item = new BreedsPagedModel();

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
            item.items = breedResponseModels;
            result.Add(item);

            return result;
        }

        private List<ABreed> convertDataRowListToBreedList(List<DataRow> dataRowList)
        {
            var herdClassificationList = new List<ABreed>();

            foreach (DataRow dataRow in dataRowList)
            {
                var herdClassificationModel = DataRowToObject.ToObject<ABreed>(dataRow);
                herdClassificationList.Add(herdClassificationModel);
            }

            return herdClassificationList;
        }

        private List<BreedResponseModel> convertBreedListToResponseModelList(List<ABreed> breedList)
        {
            var breedResponseModels = new List<BreedResponseModel>();

            foreach (ABreed breed in breedList)
            {
                var breedResponseModel = new BreedResponseModel()
                {
                    breedCode = breed.Id.ToString(),
                    breedDesc = breed.BreedDesc
                };
                breedResponseModels.Add(breedResponseModel);
            }
            return breedResponseModels;
        }

        private ABreed convertDataRowToBreed(DataRow dataRow) 
        {
            return DataRowToObject.ToObject<ABreed>(dataRow);
        }

        private BreedResponseModel convertBreedToResponseModel(ABreed breed)
        {
            var breedResponseModel = new BreedResponseModel()
            {
                breedCode = breed.BreedCode,
                breedDesc = breed.BreedDesc
            };
            return breedResponseModel;
        }

        private void populateBreed(ABreed breed, BreedUpdateModel breedUpdateModel)
        {
            breed.BreedCode = breedUpdateModel.BreedCode;
            breed.BreedDesc = breedUpdateModel.BreedDesc;
            breed.DateUpdated = DateTime.Now;
            breed.UpdatedBy = breedUpdateModel.UpdatedBy;
        }

       
    }
}
