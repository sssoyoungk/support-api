using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [SupportsAuth]
    [EnableCors(origins: "*", headers: "*", methods: "*", PreflightMaxAge = 28800)]
    [Route("api/Sales/Covid19MatchingRetest")]
    public class Covid19MatchingRetestController : ApiController
    {
        [Route("api/Sales/Covid19MatchingRetest/UnmatchingList")]
        public IHttpActionResult GetUnmatchingList(DateTime beginDate, DateTime endDate, string groupCode)
        {
            try
            {
                string sql; 
                sql =
                    $"SELECT CONVERT(varchar, lri.LabRegdate, 23) AS LabRegDate, lri.LabRegNo, lri.PatientName, lri.PatientJuminNo01, lri.PatientChartNo, lri.SystemUniqID, lri.CenterCode,\r\n" +
                    $"       lri.CompCode, pcc.CompName, pcc.CompInstitutionNo, '' AS SampleNo\r\n" +
                    $"FROM LabRegInfo lri\r\n" +
                    $"JOIN LabRegTest lrt\r\n" +
                    $"ON lri.LabRegDate = lrt.LabRegDate\r\n" +
                    $"AND lri.LabRegNo = lrt.LabRegNo\r\n" +
                    $"JOIN ProgCompCode pcc\r\n" +
                    $"ON pcc.CompCode = lri.CompCode\r\n" +
                    $"WHERE lri.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                    $"AND lrt.OrderCode = '22053'\r\n" +
                    $"AND LEN(lri.SystemUniqID) <> 25\r\n" +
                    $"AND lri.CompCode IN\r\n" +
                    $"(\r\n" +
                    $"    SELECT CompCode\r\n" +
                    $"    FROM ProgAuthGroupAccessComp\r\n" +
                    $"    WHERE AuthGroupCode = '{groupCode}'\r\n" +
                    $")\r\n" +
                    $"ORDER BY lri.LabRegDate, lri.LabRegNo";
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

        [Route("api/Sales/Covid19MatchingRetest/FindFirstTest")]
        public IHttpActionResult GetFindFirstTest(string compCode, DateTime labRegDate, string patientName)
        {
            try
            {
                string sql;
                sql =
                    $"SELECT\r\n" +
                    $"    lri.LabRegDate, lri.LabRegNo, lri.PatientName, lrc.CustomValue01, lrc.CustomValue02, lri.CompCode, pcc.CompInstitutionNo, lrr.TestResult01,\r\n" +
                    $"    REPLACE(SUBSTRING(CustomValue02, 1, CHARINDEX(';', CustomValue02)), ';', '') AS BirthDay,\r\n" +
                    $"    REPLACE(SUBSTRING(CustomValue02, CHARINDEX(';', CustomValue02), 25), ';', '') AS SampleNo\r\n" +
                    $"FROM LabRegInfo lri\r\n" +
                    $"JOIN LabRegCustom lrc\r\n" +
                    $"ON lri.LabRegDate = lrc.LabRegDate\r\n" +
                    $"AND lri.LabRegNo = lrc.LabRegNo\r\n" +
                    $"JOIN LabRegTest lrt\r\n" +
                    $"ON lrt.LabRegDate = lri.LabRegDate\r\n" +
                    $"AND lrt.LabRegNo = lri.LabRegNo\r\n" +
                    $"AND lrt.OrderCode IN ('22062', '22063', '22064', '22065')\r\n" +
                    $"JOIN LabRegResult lrr\r\n" +
                    $"ON lrr.LabRegDate = lri.LabRegDate\r\n" +
                    $"AND lrr.LabRegNo = lri.LabRegNo\r\n" +
                    $"AND lrt.OrderCode = lrr.OrderCode\r\n" +
                    $"AND lrt.TestCode = lrr.TestCode\r\n" +
                    $"JOIN ProgCompCode pcc\r\n" +
                    $"ON pcc.CompCode = lri.CompCode\r\n" +
                    $"WHERE lrr.TestResult01 <> 'Negative'\r\n" +
                    $"AND lri.CompCode = '{compCode}'\r\n" +
                    $"AND lri.LabRegDate BETWEEN DATEADD(day, -1, '{labRegDate.ToString("yyyy-MM-dd")}') AND '{labRegDate.ToString("yyyy-MM-dd")}'\r\n" +
                    $"AND lrc.CustomValue01 = '{patientName}'" +
                    $"AND LEN(lrc.CustomValue02) > 25";
                var arrResponse = LabgeDatabase.SqlToJArray(sql);

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

        // PUT api/<controller>/5
        public IHttpActionResult Put(JObject objRequest)
        {
            try
            {
                string sql;

                sql =
                    $"UPDATE LabRegInfo\r\n" +
                    $"SET PatientJuminNo01 = '{objRequest["PatientJuminNo01"].ToString()}',\r\n" +
                    $"    SystemUniqID = '{objRequest["SampleNo"].ToString()}',\r\n" +
                    $"    IsTrustOrder = 1,\r\n" +
                    $"    CenterCode = 'Covid19Excel\r\n'" +
                    $"WHERE LabRegDate = '{objRequest["LabRegDate"].ToString()}'\r\n" +
                    $"AND LabRegNo = {objRequest["LabRegNo"].ToString()}\r\n" +
                    $"\r\n" +
                    $"UPDATE Covid19Order\r\n" +
                    $"SET ExportDateTime = null\r\n" +
                    $"WHERE SampleNo = '{objRequest["SampleNo"].ToString()}'";

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