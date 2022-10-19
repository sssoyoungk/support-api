using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/LabTransCompOrderInfo")]
    public class LabTransCompOrderInfoController : ApiController
    {
        // POST api/<controller>
        public IHttpActionResult Post([FromBody]JObject request)
        {
            try
            {
                string sql;

                sql = $"INSERT INTO LabTransCompOrderInfo\r\n" +
                      $"    ( CompCode\r\n" +
                      $"    , CompOrderDate\r\n" +
                      $"    , CompOrderNo\r\n" +
                      $"    , CompOrderSeq\r\n" +
                      $"    , CompSpcNo\r\n" +
                      $"    , CompTestCode\r\n" +
                      $"    , CompTestSubCode\r\n" +
                      $"    , CompTestSampleCode\r\n" +
                      $"    , CompTestName\r\n" +
                      $"    , LabRegDate\r\n" +
                      $"    , LabRegNo\r\n" +
                      $"    , OrderCode\r\n" +
                      $"    , TestCode\r\n" +
                      $"    , CompExpansionField01\r\n" +
                      $"    , CompExpansionField02\r\n" +
                      $"    , CID\r\n" +
                      $"    , ADMOPD\r\n" +
                      $"    , CTEXT\r\n" +
                      $"    , RegistTime\r\n" +
                      $"    , RegistID)\r\n" +
                      $"VALUES\r\n" +
                      $"    ( '{request["CompCode"].ToString()}'\r\n" +
                      $"    , '{Convert.ToDateTime(request["CompOrderDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                      $"    , '{request["CompOrderNo"].ToString()}'\r\n" +
                      $"    , '{request["CompOrderSeq"].ToString()}'\r\n" +
                      $"    , '{request["CompSpcNo"].ToString()}'\r\n" +
                      $"    , '{request["CompTestCode"].ToString()}'\r\n" +
                      $"    , '{request["CompTestSubCode"].ToString()}'\r\n" +
                      $"    , '{request["CompTestSampleCode"].ToString()}'\r\n" +
                      $"    , '{request["CompTestName"].ToString().Replace("'", "''")}'\r\n" +
                      $"    , '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                      $"    , {request["LabRegNo"].ToString()}\r\n" +
                      $"    , '{request["OrderCode"].ToString()}'\r\n" +
                      $"    , '{request["TestCode"].ToString()}'\r\n" +
                      $"    , '{(request["CompExpansionField01"] ?? string.Empty).ToString()}'\r\n" +
                      $"    , '{(request["CompExpansionField02"] ?? string.Empty).ToString()}'\r\n" +
                      $"    , '{(request["CID"] ?? string.Empty).ToString()}'\r\n" +
                      $"    , '{(request["ADMOPD"] ?? string.Empty).ToString()}'\r\n" +
                      $"    , '{(request["CTEXT"] ?? string.Empty).ToString()}'\r\n" +
                      $"    , GETDATE()\r\n" +
                      $"    , '{request["RegistID"].ToString()}') ";

                LabgeDatabase.ExecuteSql(sql);

                if ((request["ChartKind"].ToString() ?? string.Empty) != string.Empty)
                {
                    sql = $"UPDATE LabRegInfo\r\n" +
                          $"SET CenterCode = '{request["ChartKind"].ToString()}'\r\n" +
                          $"  , IsTrustOrder = 1\r\n" +
                          $"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                          $"AND LabRegNo = {request["LabRegNo"]}";
                    LabgeDatabase.ExecuteSql(sql);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", HttpStatusCode.BadRequest.ToString());
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }

        public IHttpActionResult Put([FromBody]JObject request)
        {
            try
            {
                string sql;

                int isTestTransEnd = (request["StateCode"].ToString() == "Y") ? 1 : 0;

                sql = $"UPDATE LabTransCompOrderInfo\r\n" +
                      $"SET ResultSendState = '{request["StateCode"].ToString()}'\r\n" +
                      $"  , ResultSendTime = GETDATE()\r\n" +
                      $"  , EditTime = GETDATE()\r\n" +
                      $"  , EditID = '{request["EditID"].ToString()}'\r\n" +
                      $"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND LabRegNo = {request["LabRegNo"].ToString()}\r\n" +
                      $"AND TestCode = '{request["TestSubCode"].ToString()}'\r\n" +
                      $"\r\n" +
                      $"UPDATE LabRegTest\r\n" +
                      $"SET IsTestTransEnd = {isTestTransEnd}\r\n" +                      
                      $"  , EditTime = GETDATE()\r\n" +
                      $"  , EditorMemberID = '{request["EditID"].ToString()}'\r\n" +
                      $"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND LabRegNo = {request["LabRegNo"].ToString()}\r\n" +
                      $"AND TestCode = '{request["TestCode"].ToString()}'";
                LabgeDatabase.ExecuteSql(sql);

                return Ok();
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }
    }
}