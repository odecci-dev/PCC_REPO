﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API_PCC.ApplicationModels;
using API_PCC.EntityModels;
using System.Data;
using API_PCC.Utils;
using System.Data.SqlClient;
using API_PCC.Data;
using API_PCC.Manager;
using API_PCC.ApplicationModels.Common;
using API_PCC.Models;
using Microsoft.EntityFrameworkCore;
using System;
using AngouriMath;
using NuGet.Versioning;
using System.Xml.Linq;

namespace API_PCC.Controllers
{
    [Authorize("ApiKey")]
    [Route("[controller]/[action]")]
    [ApiController]
    public class UserTypeController : ControllerBase
    {
        private readonly PCC_DEVContext _context;
        DbManager db = new DbManager();
        DBMethods dbmet = new DBMethods();
        public UserTypeController(PCC_DEVContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<ActionResult<IEnumerable<UserTypePagedModel>>> list(CommonSearchFilterModel searchFilter)
        {
            searchFilter.searchParam = StringSanitizer.sanitizeString(searchFilter.searchParam);

            try
            {
                DataTable queryResult = db.SelectDb_WithParamAndSorting(QueryBuilder.buildUserTypeSearchQuery(searchFilter), null, populateSearchSqlParameters(searchFilter));
                var result = buildUserTypePagedModel(searchFilter, queryResult);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(ex.GetBaseException().ToString());
            }
        }
        private List<UserTypePagedModel> buildUserTypePagedModel(CommonSearchFilterModel searchFilter, DataTable dt)
        {
            int pagesize = searchFilter.pageSize == 0 ? 100 : searchFilter.pageSize;
            int page = searchFilter.page == 0 ? 1 : searchFilter.page;
            var items = (dynamic)null;
            int totalItems = 0;
            int totalPages = 0;

            totalItems = dt.Rows.Count;
            totalPages = (int)Math.Ceiling((double)totalItems / pagesize);
            items = dt.AsEnumerable().Skip((page - 1) * pagesize).Take(pagesize).ToList();

            var userTypes = convertDataRowListToUserTypeList(items);
            
            var result = new List<UserTypePagedModel>();
            var item = new UserTypePagedModel();

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
            item.items = userTypes;
            result.Add(item);

            return result;
        }

        private SqlParameter[] populateSearchSqlParameters(CommonSearchFilterModel searchFilterModel)
        {

            var sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "SearchParam",
                Value = searchFilterModel.searchParam ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });

            return sqlParameters.ToArray();
        }
        private SqlParameter[] populateSqlParameters(string name)
        {

            var sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "Name",
                Value = name ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });

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

        private List<TblUserTypeModel> convertDataRowListToUserTypeList(List<DataRow> dataRowList)
        {
            var userTypeList = new List<TblUserTypeModel>();

            foreach (DataRow dataRow in dataRowList)
            {
                var userType = DataRowToObject.ToObject<TblUserTypeModel>(dataRow);
                userTypeList.Add(userType);
            }

            return userTypeList;
        }

        private TblUserTypeModel convertDataRowToUserType(DataRow dataRow)
        {
            return DataRowToObject.ToObject<TblUserTypeModel>(dataRow);
        }
        public class UserTypeModel
        {
            public int Id { get; set; }
            public string UserTypeName { get; set; }
            public int userAccesasId { get; set; }
        }
        // GET: userType/search/5
        [HttpGet("{name}")]
        public async Task<ActionResult<UserTypeModel>> view(string? name)
        {
            string sql = "";
            //DataTable userTypeRecord = db.SelectDb_WithParamAndSorting(QueryBuilder.buildUserTypeQueryByName(), null, populateSqlParameters(name));

            //if (userTypeRecord.Rows.Count == 0)
            //{
            //    return Conflict("No records found!");
            //}

            //var userTypeModel = convertDataRowToUserType(userTypeRecord.Rows[0]);
            //var userTypeResponseModel = convertUserTypeToResponseModel(userTypeModel);

            //return Ok(userTypeResponseModel);
            if (name == null)
            {
                sql = $@"SELECT [id]
      ,[name]
      ,[userAccesasId]
  FROM [dbo].[tbl_UserTypeModel] where Delete_Flag = 0 ";
            }
      else
            {
                sql = $@"SELECT [id]
      ,[name]
      ,[userAccesasId]
  FROM [dbo].[tbl_UserTypeModel] where Delete_Flag = 0 and name = '" + name + "'";
            }

            DataTable table = db.SelectDb(sql).Tables[0];
            var result = new List<UserTypeModel>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new UserTypeModel();
                item.Id = int.Parse(dr["id"].ToString());
                item.userAccesasId = int.Parse(dr["userAccesasId"].ToString());
                item.UserTypeName = dr["name"].ToString();
                result.Add(item);

            }
            return Ok(result);
        }

        private UserTypeResponseModel convertUserTypeToResponseModel(TblUserTypeModel tblUserTypeModel)
        {
            var userTypeResponseModel = new UserTypeResponseModel()
            {
                code = tblUserTypeModel.code,
                userAccesasId = tblUserTypeModel.userAccesasId,
                name = tblUserTypeModel.name
            };
            return userTypeResponseModel;
        }
        // PUT: userType/update/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> update(int id, UserTypeUpdateModel userTypeUpdateModel)
        {
            DataTable userTypeRecord = db.SelectDb_WithParamAndSorting(QueryBuilder.buildUserTypeQueryById(), null, populateSqlParameters(id));

            if (userTypeRecord.Rows.Count == 0)
            {
                return Conflict("No records matched!");
            }

            DataTable userTypeDuplicateCheck = db.SelectDb_WithParamAndSorting(QueryBuilder.buildUserTypeDuplicateCheckUpdateQuery(), null, populateSqlParameters(id, userTypeUpdateModel));

            // check for duplication
            if (userTypeDuplicateCheck.Rows.Count > 0)
            {
                return Conflict("Entity already exists");
            }

            try
            {
                string usertype_update = $@"UPDATE [dbo].[tbl_UserTypeModel]
                                           SET [code] = '" + userTypeUpdateModel.Code + "'" +
                                          ",[name] = '" + userTypeUpdateModel.Name + "'" +
                                          ",[Updated_By] = '" + userTypeUpdateModel.UpdatedBy + "'" +
                                          ",[userAccesasId] = '" + userTypeUpdateModel.userAccesasId + "'" +
                                          ",[Date_Updated] = '" + DateTime.Now.ToString("yyyy-MM-dd") + "'" +
                                         " WHERE id = '" + id + "' and Delete_Flag = 0 ";
                string result = db.DB_WithParam(usertype_update);
                //var userTypeModel = convertDataRowToUserType(userTypeRecord.Rows[0]);
                //populateUserType(userTypeModel, userTypeUpdateModel);
                //_context.Entry(userTypeModel).State = EntityState.Modified;
                //await _context.SaveChangesAsync();

                return Ok("Update Successful!");
            }
            catch (Exception ex)
            {

                return Problem(ex.GetBaseException().ToString());
            }
        }


        // POST: userType/save
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TblUserTypeModel>> save(UserTypeRegistrationModel userTypeRegistrationModel)
        {
            DataTable userTypeRecord = db.SelectDb_WithParamAndSorting(QueryBuilder.buildUserTypeDuplicateCheckSaveQuery(), null, populateSqlParameters(userTypeRegistrationModel));

            // check for duplication
          

            var userType = buildUserTypeRegistrationModel(userTypeRegistrationModel);
            if (userTypeRecord.Rows.Count > 0)
            {
                return Conflict("Entity already exists");
            }

            try
            {
                int lastInsertedId = 0;
                _context.tblUserTypeModels.Add(userType);
           
                    await _context.SaveChangesAsync();
                    var entry = _context.Entry(userType);
                    lastInsertedId = entry.Property(e => e.Id).CurrentValue;
        
                return Ok(lastInsertedId);
            }
            catch (Exception ex)
            {

                return Problem(ex.GetBaseException().ToString());
            }


        }

        public class Usertypeupdate
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int userAccesasId { get; set; }
            public string UpdatedBy { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteMultiple(List<DeletionModel> deletionModel)
        {
            string status = "";
            if (_context.tblUserTypeModels == null)
            {
                return NotFound();
            }
            for (int x = 0; x < deletionModel.Count; x++)
            {


                var userType = await _context.tblUserTypeModels.FindAsync(deletionModel[x].id);
                if (userType == null || userType.DeleteFlag)
                {
                    return Conflict("No records matched!");
                }

                try
                {
                    userType.DeleteFlag = true;
                    userType.DateDeleted = DateTime.Now;
                    userType.DeletedBy = deletionModel[x].deletedBy;
                    userType.DateRestored = null;
                    userType.RestoredBy = "";
                    _context.Entry(userType).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    dbmet.InsertAuditTrail("Deleted User type ID: " + userType.Id + "", DateTime.Now.ToString("yyyy-MM-dd"), "User Management Module", userType.DeletedBy, "0");

                    status = "Deletion Successful!";
                }
                catch (Exception ex)
                {

                    return Problem(ex.GetBaseException().ToString());
                }
            }
            return Ok(status);
        }

        // POST: userType/delete/5
        [HttpPost]
        public async Task<IActionResult> delete(DeletionModel deletionModel)
        {
            DataTable userTypeRecord = db.SelectDb_WithParamAndSorting(QueryBuilder.buildUserTypeQueryById(), null, populateSqlParameters(deletionModel.id));

            if (userTypeRecord.Rows.Count == 0)
            {
                return Conflict("No records matched!");
            }

            string usertypeChk = $@"SELECT id, UserType FROM tbl_UsersModel WHERE UserType = '" + deletionModel.id + "'";

            DataTable usertypeChk_tbl = db.SelectDb(usertypeChk).Tables[0];

            if(usertypeChk_tbl.Rows.Count != 0)
            {
                return Conflict("User type already assigned to one or more users.");
            }

            var userTypeModel = convertDataRowToUserType(userTypeRecord.Rows[0]);

            try
            {
                userTypeModel.DeleteFlag = true;
                userTypeModel.DateDeleted = DateTime.Now;
                userTypeModel.DeletedBy = deletionModel.deletedBy;
                userTypeModel.DateRestored = null;
                userTypeModel.RestoredBy = "";
                _context.Entry(userTypeModel).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                dbmet.InsertAuditTrail("Deleted Usertype ID: " + deletionModel.id + "", DateTime.Now.ToString("yyyy-MM-dd"), "User Module", deletionModel.deletedBy, "0");
                return Ok("Deletion Successful!");
            }
            catch (Exception ex)
            {
                return Problem(ex.GetBaseException().ToString());
            }
        }

        // POST: userType/restore/
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> restore(RestorationModel restorationModel)
        {
            DataTable userTypeRecord = db.SelectDb_WithParamAndSorting(QueryBuilder.buildUserTypeDeletedSearchQueryById(), null, populateSqlParameters(restorationModel.id));

            if (userTypeRecord.Rows.Count == 0)
            {
                return Conflict("No deleted records matched!");
            }

            var userTypeModel = convertDataRowToUserType(userTypeRecord.Rows[0]);

            try
            {
                userTypeModel.DeleteFlag = !userTypeModel.DeleteFlag;
                userTypeModel.DateDeleted = null;
                userTypeModel.DeletedBy = "";
                userTypeModel.DateRestored = DateTime.Now;
                userTypeModel.RestoredBy = restorationModel.restoredBy;

                _context.Entry(userTypeModel).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                dbmet.InsertAuditTrail("Restore Usertype ID: " + restorationModel.id + "", DateTime.Now.ToString("yyyy-MM-dd"), "User Module", restorationModel.restoredBy, "0");
                return Ok("Restoration Successful!");
            }
            catch (Exception ex)
            {

                return Problem(ex.GetBaseException().ToString());
            }
        }

        private SqlParameter[] populateSqlParameters(int id, UserTypeUpdateModel userTypeUpdateModel)
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
                ParameterName = "Code",
                Value = userTypeUpdateModel.Code ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });

            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "Name",
                Value = userTypeUpdateModel.Name ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });

            return sqlParameters.ToArray();
        }

        private SqlParameter[] populateSqlParameters(UserTypeRegistrationModel userTypeRegistrationModel)
        {
            var sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "Code",
                Value = userTypeRegistrationModel.Code ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });

            sqlParameters.Add(new SqlParameter
            {
                ParameterName = "Name",
                Value = userTypeRegistrationModel.Name ?? Convert.DBNull,
                SqlDbType = System.Data.SqlDbType.VarChar,
            });

            return sqlParameters.ToArray();
        }
        private void populateUserType(TblUserTypeModel userType, UserTypeUpdateModel userTypeUpdateModel)
        {
            userType.code = userTypeUpdateModel.Code;
            userType.name = userTypeUpdateModel.Name;
            userType.DateUpdated = DateTime.Now;
            userType.UpdatedBy = userTypeUpdateModel.UpdatedBy;
        }

        private TblUserTypeModel buildUserTypeRegistrationModel(UserTypeRegistrationModel userTypeRegistrationModel)
        {
            var userType = new TblUserTypeModel()
            {
                code = userTypeRegistrationModel.Code,
                name = userTypeRegistrationModel.Name,
                userAccesasId = userTypeRegistrationModel.userAccesasId,
                DeleteFlag = false,
                CreatedBy = userTypeRegistrationModel.CreatedBy,
                DateCreated = DateTime.Now
            };
            return userType;
        }
    }
}
