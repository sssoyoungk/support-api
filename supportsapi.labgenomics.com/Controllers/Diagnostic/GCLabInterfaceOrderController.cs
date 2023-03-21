using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    [Route("api/Diagnostic/GCLabInterfaceOrder")]
    public class GCLabInterfaceOrderController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(string compCode, string sendKind, DateTime beginDate, DateTime endDate)
        {
            try
            {
                string sql;
                sql = $"SELECT A.LabRegDate, A.LabRegNo, A.CompCode\r\n" +
                      $"     , (SELECT CompName FROM ProgCompCode WHERE A.CompCode = CompCode) AS CompName\r\n" +
                      $"     , A.PatientName, A.PatientAge, A.PatientSex\r\n" +
                      $"     , CASE WHEN ISNULL(A.PatientChartNo, '') = '' THEN CONVERT(varchar, A.LabRegDate, 112) + CONVERT(varchar, A.LabRegNo) ELSE A.PatientChartNo END AS PatientChartNo\r\n" +
                      $"     , CASE WHEN ISNULL(A.PatientJuminNo01, '') = '' THEN '******' ELSE A.PatientJuminNo01 END +\r\n" +
                      $"       CASE WHEN CONVERT(varchar, master.dbo.AES_DecryptFunc(A.PatientJuminNo02, 'labge$%#!dleorms')) collate Korean_wansung_CI_AS = '' THEN '*******'\r\n" +
                      $"            ELSE CONVERT(varchar, master.dbo.AES_DecryptFunc(A.PatientJuminNo02, 'labge$%#!dleorms')) collate Korean_wansung_CI_AS END AS PatientJuminNo\r\n" +
                      $"     , B.TestCode AS OrderCode, D.TestSubCode AS TestCode\r\n" +
                      $"     , (SELECT TestDisplayName FROM LabTestCode WHERE D.TestSubCode = TestCode) AS TestDisplayName\r\n" +
                      $"     , B.SampleCode\r\n" +
                      $"     , (SELECT SampleName FROM LabSampleCode WHERE B.SampleCode = SampleCode) AS SampleName\r\n" +
                      $"     , B.IsTestOutside, B.TestOutsideBeginTime, B.TestOutsideEndTime, B.TestOutsideCompCode, B.TestOutsideMemberID\r\n" +
                      $"     , null AS ErrorDescription\r\n" +
                      $"FROM LabRegInfo A\r\n" +
                      $"JOIN LabRegTest B\r\n" +
                      $"ON A.LabRegDate = B.LabRegDate\r\n" +
                      $"AND A.LabRegNo = B.LabRegNo\r\n" +
                      $"AND B.TestStateCode <> 'F'\r\n" +
                      $"JOIN LabOutsideTestCode C\r\n" +
                      $"ON C.OutsideCompCode = '{compCode}'\r\n" +
                      $"AND C.OutsideSampleCode = B.SampleCode\r\n" +
                      $"JOIN LabRegResult D\r\n" +
                      $"ON A.LabRegDate = D.LabRegDate\r\n" +
                      $"AND A.LabRegNo = D.LabRegNo\r\n" +
                      $"AND B.TestCode = D.TestCode\r\n" +
                      $"AND C.OutsideTestCode = D.TestSubCode\r\n" +
                      $"WHERE A.LabRegDate BETWEEN '{beginDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}'\r\n" +
                      $"AND B.IsTestOutSide = {sendKind}\r\n" +
                      $"ORDER BY A.LabRegDate, A.LabRegNo, B.TestCode";

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

        public IHttpActionResult Put([FromBody]JObject request)
        {
            try
            {
                string sql;
                sql = $"UPDATE LabRegTest\r\n" +
                      $"   SET IsTestOutSide = 1\r\n" +
                      $"     , TestOutsideCompCode = '{request["CompCode"]}'\r\n" +
                      $"     , TestStartTime = GETDATE()\r\n" +
                      $"     , TestOutsideBeginTime = GETDATE()\r\n" +
                      $"     , TestOutsideMemberID = '{request["MemberID"]}'\r\n" +
                      $"     , IsWorkCheck = 1\r\n" +
                      $"     , WorkCheckTime = GETDATE()\r\n" +
                      $"     , WorkCheckMemberID = '{request["MemberID"]}'\r\n" +
                      $"     , TestStateCode = 'O'\r\n" +
                      $"     , EditTime = GETDATE()\r\n" +
                      $"     , EditorMemberID = '{request["MemberID"]}'\r\n" +
                      $"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]):yyyy-MM-dd}'\r\n" +
                      $"AND LabRegNo = {request["LabRegNo"]}\r\n" +
                      $"AND TestCode = '{request["TestCode"]}'\r\n" +

                      $"UPDATE LabRegReport\r\n" +
                      $"   SET ReportStateCode = CASE WHEN ReportStateCode = 'W' THEN 'O' ELSE ReportStateCode END\r\n" +
                      $"     , ReportStartTime = CASE WHEN ReportStartTime IS NULL THEN GETDATE() END\r\n" +
                      $"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]):yyyy-MM-dd}'\r\n" +
                      $"AND LabRegNo = {request["LabRegNo"]}\r\n" +
                      $"AND ReportCode = (SELECT ReportCode FROM LabTestCode where TestCode = '{request["TestCode"]}')";

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