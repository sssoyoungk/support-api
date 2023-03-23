using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using supportsapi.labgenomics.com.ServiceSMS;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers
{
    [SupportsAuth]
    public class OfficeSupplyController : ApiController
    {

        #region SupplyList

        [Route("api/OfficeSupply/PartCode")]
        public IHttpActionResult GetOfficeSupplyPartCode()
        {
            try
            {
                string sql = " SELECT PartCode, PartName\n" +
                         "   FROM OfficeSupplyPartCode \n"+
                         "order by PartCode ";

                Debug.WriteLine(sql);
                JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
                return Ok(arrResponse);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }


        /// <summary>
        /// 고정자산 가져오기
        /// </summary>
        /// <param name="supplyCode"></param>
        /// <returns></returns>
        /// 
        [Route("api/OfficeSupply/SupplyList")]
        public IHttpActionResult GetSupplyList(string supplyCode, string supplyPlace = "")
        {
            try
            {
                string sql;
                sql = " SELECT A.*, B.MemberGroupName AS PartName \n" +
                      "   FROM OfficeSupplyList A";
                sql += "   LEFT OUTER JOIN ProgMemberGroupCode B " +
                       "     ON B.MemberGroupCode = A.PartCode ";
                if (supplyCode != null && supplyCode != string.Empty)
                {
                    sql += " WHERE SupplyCode LIKE '"+supplyCode + "%' \n";
                    if (supplyPlace != null && supplyPlace != string.Empty)
                    {
                        sql += $" AND SupplyPlace LIKE '{supplyPlace}%' \n";
                    }
                }

                sql += "order by A.SupplyCode";
                Debug.WriteLine(sql);
                JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

                return Ok(arrResponse);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }

        /// <summary>
        /// SupplyCode 등록 가능 여부
        /// </summary>
        /// <param name="supplyCode"></param>
        /// <returns></returns>
        [Route("api/OfficeSupply/SupplyListCount")]
        public IHttpActionResult GetSupplyListCount(string supplyCode)
        {
            try
            {
                string sql;
                sql = $"SELECT COUNT(ofsl.SupplyCode) \n" +
                      $"   FROM OfficeSupplyList AS ofsl \n" +
                      $" Where ofsl.SupplyCode = '{supplyCode}'";
                
                Debug.WriteLine(sql);
                int count = (int)LabgeDatabase.ExecuteSqlScalar(sql);
                if (count >= 1)
                {
                    throw new Exception();
                }
                return Ok(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, (ex.Message));
            }
        }

        /// <summary>
        /// 고정자산 삭제
        /// </summary>
        /// <param name="supplyData">Jobject</param>
        /// <returns></returns>
        [Route("api/OfficeSupply/DeleteOfficeSupply")]
        public IHttpActionResult PutDeleteOfficeSupply([FromBody]JObject supplyData)
        {
            try
            {
                string sql = " DELETE FROM OfficeSupplyList \n" +
                         "  WHERE SupplyCode = '" + supplyData["SupplyCode"].ToString() + "' ";

                LabgeDatabase.ExecuteSql(sql);
                return Ok(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, (ex.Message));
            }
        }

        /// <summary>
        /// 고정자산 추가
        /// </summary>
        /// <param name="supplyData">Jobject</param>
        /// <returns></returns>
        [Route("api/OfficeSupply/SupplyList")]
        public IHttpActionResult PutSupplyList([FromBody]JObject supplyData)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Clear();
                if (supplyData["setKind"].ToString() == "INSERT")
                {
                    query.AppendLine($"SELECT COUNT(*)\r\n");
                    query.AppendLine($"FROM OfficeSupplyList\r\n");
                    query.AppendLine($"WHERE SupplyCode = '{supplyData["SupplyCode"].ToString()}'");
                    int count = (int)LabgeDatabase.ExecuteSqlScalar(query.ToString());
                    query.Clear();
                    
                    if (count >= 1)
                    {
                        throw new Exception("동일한 데이터가 있습니다.");
                    }
                    query.AppendLine($"INSERT INTO OfficeSupplyList (SupplyCode, SupplyName, SupplyPartName, PartCode, " +
                        $" ImportDate, DiscardDate ,SupplyPlace,SupplyUse, SupplyUser,  SupplyPrice, SupplyBrandName,  SupplyAddress, SupplyDescription )");
                    query.Append($" VALUES('{supplyData["SupplyCode"]}'");
                    query.Append($" , '{supplyData["SupplyName"]}'");
                    query.Append($" , '{supplyData["SupplyPartName"]}'");
                    query.Append($" , '{supplyData["PartCode"]}'");
                    query.Append($" , '{supplyData["ImportDate"]}'");
                    query.Append($" , '{supplyData["DiscardDate"]}'");
                    query.Append($" , '{supplyData["SupplyPlace"]}'");
                    query.Append($" , '{supplyData["SupplyUse"]}'");
                    query.Append($" , '{supplyData["SupplyUser"]}'");
                    query.Append($" , '{supplyData["SupplyPrice"]}'");
                    query.Append($" , '{supplyData["SupplyBrandName"]}'");
                    query.Append($" , '{supplyData["SupplyAddress"]}'");
                    query.AppendLine($" , '{supplyData["SupplyDescription"]}')");
                }
                else
                {
                    query = new StringBuilder();
                    query.AppendLine("UPDATE OfficeSupplyList");
                    query.Append($"SET SupplyName = '{supplyData["SupplyName"]}'");
                    query.Append($" , SupplyPartName = '{supplyData["SupplyPartName"]}'");
                    query.Append($" , PartCode = '{supplyData["PartCode"]}'");
                    query.Append($" , ImportDate = '{supplyData["ImportDate"]}'");
                    query.Append($" , DiscardDate = '{supplyData["DiscardDate"]}'");
                    query.Append($" , SupplyPlace = '{supplyData["SupplyPlace"]}'");
                    query.Append($" , SupplyUse = '{supplyData["SupplyUse"]}'");
                    query.Append($" , SupplyUser = '{supplyData["SupplyUser"]}'");
                    query.Append($" , SupplyPrice = '{supplyData["SupplyPrice"]}'");
                    query.Append($" , SupplyBrandName = '{supplyData["SupplyBrandName"]}'");
                    query.Append($" , SupplyAddress = '{supplyData["SupplyAddress"]}'");
                    query.AppendLine($" , SupplyDescription = '{supplyData["SupplyDescription"]}'");
                    query.AppendLine($" WHERE SupplyCode = '{supplyData["SupplyCode"]}'\n");
                }
                LabgeDatabase.ExecuteSql(query.ToString());
                return Ok(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        #endregion

        #region RepairHistroy
        /// <summary>
        /// 수리이력 불러오기
        /// </summary>
        /// <param name="supplyCode"></param>
        /// <returns></returns>
        [Route("api/OfficeSupply/SelectRepairHistory")]
        public IHttpActionResult GetOfficeSupplyRepairHistory(string supplyCode)
        {
            try
            {
                string sql = " SELECT * \n" +
                         "   FROM OfficeSupplyRepairHistory \n" +
                         "  WHERE SupplyCode = '" + supplyCode + "' ";

                Debug.WriteLine(sql);
                JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
                return Ok(arrResponse);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }
        /// <summary>
        /// 수리이력 추가
        /// </summary>
        /// <param name="supplyData"></param>
        /// <returns></returns>
        [Route("api/OfficeSupply/InsertRepairHistory")]
        public IHttpActionResult PutOfficeSupplyRepairHistory([FromBody]JObject supplyData)
        {
            try
            {
                string sql;
                if (supplyData["setKind"].ToString() == "WRITE")
                {
                    sql = $"INSERT INTO OfficeSupplyRepairHistory \n" +
                          $"          ( SupplyCode \n" +
                          $"          , RepairDate \n" +
                          $"          , RepairPrice \n" +
                          $"          , RepairDescription \n" +
                          $"          , RepairHistory \n" +
                          $"          , InsertID \n" +
                          $"          , InsertDateTime) \n" +
                          $"     VALUES " + "\n" +
                          $"          ( '{supplyData["SupplyCode"]}' \n" +
                          $"          , '{supplyData["RepairDate"]}' \n" +
                          $"          , '{supplyData["RepairPrice"]}' \n" +
                          $"          , '{supplyData["RepairDescription"]}' \n" +
                          $"          , '{supplyData["RepairHistory"]}' \n" +
                          $"          , '{supplyData["InsertID"]}' \n" +
                          $"          , GETDATE() )";
                }
                else
                {
                    sql = $"UPDATE OfficeSupplyRepairHistory \n" +
                          $"   SET RepairDate = '{supplyData["RepairDate"]}' \n" +
                          $"     , RepairPrice = '{supplyData["RepairPrice"]}' \n" +
                          $"     , RepairDescription = '{supplyData["RepairDescription"]}' \n" +
                          $"     , RepairHistory = '{supplyData["RepairHistory"]}' \n" +
                          $"     , ModifyID = '{supplyData["ModifyID"]}' \n" +
                          $"     , ModifyDateTime = GETDATE() \n" +
                          $" WHERE RepairHistoryID = '{supplyData["RepairHistoryID"]}'";
                }

                Debug.WriteLine(sql);
                LabgeDatabase.ExecuteSqlScalar(sql);
                return Ok(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        /// <summary>
        /// 수리이력 삭제
        /// </summary>
        /// <param name="supplyData"></param>
        /// <returns></returns>
        [Route("api/OfficeSupply/DeleteRepairHistory")]
        public IHttpActionResult PutOfficeSupplyDeleteRepairHistory([FromBody]JObject supplyData)
        {
            try
            {
                string sql;
                sql = " DELETE FROM OfficeSupplyRepairHistory \n" +
                      "  WHERE RepairHistoryID = '" + supplyData["RepairHistoryID"] + "' ";

                Debug.WriteLine(sql);
                LabgeDatabase.ExecuteSql(sql);
                
                return Ok(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        #endregion

        #region BarCode
        /// <summary>
        /// 바코드 이력 추가
        /// </summary>
        /// <param name="supplyData">Jobject</param>
        /// <returns></returns>
        [Route("api/OfficeSupply/CheckBarcode")]
        public IHttpActionResult PutSupplyCheckBarcode([FromBody]JObject supplyData)
        {
            try
            {
                string sql = $"SELECT COUNT(ofsl.SupplyCode) \n" +
                      $"   FROM OfficeSupplyList AS ofsl \n" +
                      $" Where ofsl.SupplyCode = '{supplyData["SupplyCode"]}'";

                int count = (int)LabgeDatabase.ExecuteSqlScalar(sql);
                if (count <= 0)
                {
                    return Content(HttpStatusCode.NotFound , "데이터 없음.");
                }

                sql = "INSERT INTO OfficeSupplyCheckBarcode \n" +
                      "     ( SupplyCode, RegistMemberID ) \n" +
                      "VALUES \n" +
                      $"     ('{supplyData["SupplyCode"]}', '{supplyData["RegistMemberID"]}') ";
                LabgeDatabase.ExecuteSql(sql);
                return Ok(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, (ex.Message));
            }
        }

        /// <summary>
        /// 바코드 이력 불러오기
        /// </summary>
        /// <returns>Jarray</returns>
        [Route("api/OfficeSupply/CheckBarcode")]
        public IHttpActionResult GetSupplyCheckBarcode()
        {
            try
            {
                string sql = " SELECT CONVERT(varchar, A.CheckDateTime, 120) AS CheckDateTime, A.SupplyCode, A.RegistMemberID, B.MemberName, C.SupplyName \n" +
                         "   FROM OfficeSupplyCheckBarcode A \n" +
                         "   JOIN ProgMember B \n" +
                         "     ON B.MemberID = A.RegistMemberID \n " +
                         "   JOIN OfficeSupplyList C \n" +
                         "     ON C.SupplyCode = A.SupplyCode";
                JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

                return Ok(arrResponse);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, (ex.Message));
            }
        }
        #endregion

    }
}