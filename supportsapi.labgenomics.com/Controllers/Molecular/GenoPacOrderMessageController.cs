using Newtonsoft.Json.Linq;
using System;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Molecular
{
    [Route("api/molecular/GenoPacOrderMessage")]
    public class GenoPacOrderMessageController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string groupCode)
        {
            string sql;
            sql = $"SELECT *" +
                  $"FROM (SELECT gom.RegistDate, gom.LabRegDate, gom.LabRegNo, gom.OrderCode\r\n" +
                  $"           , gom.LabMessage, gom.LabRegistID, gom.LabRegistTime\r\n" +
                  $"           , (SELECT MemberName FROM ProgMember WHERE gom.LabRegistID = MemberID) AS LabRegistName\r\n" +
                  $"           , gom.SalesMessage, gom.SalesRegistID, gom.SalesRegistTime\r\n" +
                  $"           , (SELECT MemberName FROM ProgMember WHERE gom.SalesRegistID = MemberID) AS SalesRegistName\r\n" +
                  $"           , lri.PatientName, lri.PatientChartNo, lri.PatientAge, lri.PatientSex\r\n" +
                  $"           , lri.CompCode, pcc.CompName\r\n" +
                  $"           , (SELECT CompMngName FROM ProgCompMngCode WHERE CompMngCode = pcc.CompMngCode) AS CompMngName\r\n" +
                  $"           , (SELECT TestDisplayName FROM LabTestCode WHERE TestCode = gom.OrderCode) AS TestDisplayName\r\n" +
                  $"           , lrt.SampleCode\r\n" +
                  $"           , (SELECT SampleName FROM LabSampleCode WHERE SampleCode = lrt.SampleCode) AS SampleName\r\n" +
                  $"           , gom.IsFinal\r\n" +
                  $"      FROM GenoPacOrderMessage gom\r\n" +
                  $"      LEFT OUTER JOIN LabRegInfo lri\r\n" +
                  $"      ON gom.LabRegDate = lri.LabRegDate\r\n" +
                  $"      AND gom.LabRegNo = lri.LabRegNo\r\n" +
                  $"      LEFT OUTER JOIN ProgCompCode pcc\r\n" +
                  $"      ON pcc.CompCode = lri.CompCode\r\n" +
                  $"      LEFT OUTER JOIN LabRegTest lrt\r\n" +
                  $"      ON gom.LabRegDate = lrt.LabRegDate\r\n" +
                  $"      AND gom.LabRegNo = lrt.LabRegNo\r\n" +
                  $"      AND gom.OrderCode = lrt.OrderCode) AS Grp1\r\n" +
                  $"WHERE LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'";
            if (groupCode.Contains("sales_") || groupCode.Contains("masters_"))
            {
                sql += $"\r\nAND (CompCode IN (SELECT CompCode FROM ProgAuthGroupAccessComp WHERE AuthGroupCode = '{groupCode}') OR CompCode IS null)";
            }

            return Ok(Services.LabgeDatabase.SqlToJArray(sql));
        }

        public IHttpActionResult Get(DateTime labRegDate, int labRegNo)
        {
            string sql;
            sql = $"SELECT gom.RegistDate, lri.LabRegDate, lri.LabRegNo, lrt.OrderCode\r\n" +
                  $"     , gom.LabMessage, gom.LabRegistID, gom.LabRegistTime\r\n" +
                  $"     , (SELECT MemberName FROM ProgMember WHERE gom.LabRegistID = MemberID) AS LabRegistName\r\n" +
                  $"     , gom.SalesMessage, gom.SalesRegistID, gom.SalesRegistTime\r\n" +
                  $"     , (SELECT MemberName FROM ProgMember WHERE gom.SalesRegistID = MemberID) AS SalesRegistName\r\n" +
                  $"     , lri.PatientName, lri.PatientChartNo, lri.PatientAge, lri.PatientSex\r\n" +
                  $"     , (SELECT CompName FROM ProgCompCode WHERE CompCode = lri.CompCode) AS CompName\r\n" +
                  $"     , lrt.OrderCode\r\n" +
                  $"     , (SELECT TestDisplayName FROM LabTestCode WHERE TestCode = gom.OrderCode) AS TestDisplayName\r\n" +
                  $"           , gom.IsFinal\r\n" +
                  $"FROM LabRegInfo lri\r\n" +
                  $"JOIN LabRegTest lrt\r\n" +
                  $"ON lri.LabRegDate = lrt.LabRegDate\r\n" +
                  $"AND lri.LabRegNo = lrt.LabRegNo\r\n" +
                  $"JOIN LabTestCode ltc\r\n" +
                  $"ON lrt.OrderCode = ltc.TestCode\r\n" +
                  $"AND ltc.TestDisplayName LIKE 'GenoPAC%'\r\n" +
                  $"LEFT OUTER JOIN GenoPacOrderMessage gom\r\n" +
                  $"ON lrt.LabRegDate = gom.LabRegDate\r\n" +
                  $"AND lrt.LabRegNo = gom.LabRegNo\r\n" +
                  $"AND lrt.OrderCode = gom.OrderCode\r\n" +
                  $"WHERE lri.LabRegDate = '{labRegDate.ToString("yyyy-MM-dd")}'\r\n" +
                  $"AND lri.LabRegNo = {labRegNo}";

            return Ok(Services.LabgeDatabase.SqlToJArray(sql));
        }

        // POST api/<controller>
        public IHttpActionResult Post([FromBody]JObject value)
        {
            try
            {
                string sql;
                sql = $"MERGE INTO GenoPacOrderMessage AS target\r\n" +
                      $"USING (SELECT '{Convert.ToDateTime(value["LabRegDate"]):yyyy-MM-dd}' AS LabRegDate, '{value["LabRegNo"]}' AS LabRegNo\r\n" +
                      $"            , '{value["OrderCode"]}' AS OrderCode) AS source\r\n" +
                      $"ON (target.LabRegDate = source.LabRegDate AND target.LabRegNo = source.LabRegNo AND target.OrderCode = source.OrderCode)\r\n" +
                      $"WHEN NOT MATCHED THEN\r\n" +
                      $"INSERT (RegistDate, LabRegDate, LabRegNo, OrderCode, LabMessage, LabRegistTime, LabRegistID)\r\n" +
                      $"VALUES ( '{Convert.ToDateTime(value["RegistDate"]):yyyy-MM-dd}', source.LabRegDate, source.LabRegNo, source.OrderCode\r\n" +
                      $"       , '{value["LabMessage"]}', GETDATE(), '{value["LabRegistID"]}' )\r\n" +
                      $"WHEN MATCHED THEN\r\n" +
                      $"UPDATE SET LabMessage = '{value["LabMessage"]}'\r\n" +
                      $"         , SalesMessage = '{value["SalesMessage"]}'\r\n" +
                      $"         , SalesRegistID = '{value["SalesRegistID"]}';";

                Services.LabgeDatabase.ExecuteSql(sql);

                return Ok();
            }
            catch (Exception ex)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("Hello")]
        public IHttpActionResult Hello()
        {
            return Ok("Hello");
        }

        [Route("GoodBye")]
        public IHttpActionResult GoodBye()
        {
            return Ok("GoodBye");
        }

        public IHttpActionResult Delete(DateTime labRegDate, int labRegNo, string orderCode)
        {
            try
            {
                string sql;
                sql = $"DELETE FROM GenoPacOrderMessage\r\n" +
                      $"WHERE LabRegDate = '{labRegDate.ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND LabRegNo = {labRegNo}\r\n" +
                      $"AND OrderCode = '{orderCode}'";
                Services.LabgeDatabase.ExecuteSql(sql);
                return Ok();
            }
            catch (Exception ex)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, ex.Message);
            }
        }

        public IHttpActionResult Patch([FromBody]JArray value)
        {
            foreach (JObject objValue in value)
            {
                string sql;
                sql = $"UPDATE GenoPacOrderMessage\r\n" +
                      $"SET IsFinal = {objValue["IsFinal"]}\r\n" +
                      $"WHERE LabRegDate = '{Convert.ToDateTime(objValue["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND LabRegNo = {objValue["LabRegNo"].ToString()}\r\n" +
                      $"AND OrderCode = '{objValue["OrderCode"].ToString()}'";
                Services.LabgeDatabase.ExecuteSql(sql);
            }
            return Ok();
        }
    }
}