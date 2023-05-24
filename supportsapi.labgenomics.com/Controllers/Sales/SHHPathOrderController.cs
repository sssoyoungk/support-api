using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    /// <summary>
    /// 신현호 병리과 오더 전송
    /// </summary>
    [Route("api/sales/SHHPathOrder")]
    public class SHHPathOrderController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string isTestOutside, int beginNo, int endNo, string compMngCode)
        {
            string sql;
            sql = $"SELECT lri.LabRegDate, lri.LabRegNo, lri.CompCode\r\n" +
                  $"     , (SELECT CompName FROM ProgCompCode WHERE lri.CompCode = CompCode) AS CompName\r\n" +
                  $"     , lri.PatientName, lri.PatientAge, lri.PatientSex, lri.PatientJuminNo01, lri.PatientChartNo\r\n" +
                  $"     , lrt.OrderCode, lrt.TestCode\r\n" +
                  $"     , (SELECT TestDisplayName FROM LabTestCode WHERE lrt.TestCode = TestCode) AS TestDisplayName, ltc.ReportCode\r\n" +
                  $"     , lrt.SampleCode\r\n" +
                  $"     , (SELECT SampleName FROM LabSampleCode WHERE lrt.SampleCode = SampleCode) AS SampleName\r\n" +
                  $"     , lrt.IsTestOutside, lrt.TestOutsideBeginTime, lrt.TestOutsideEndTime, lrt.TestOutsideCompCode, lrt.TestOutsideMemberID\r\n" +
                  $"FROM LabRegInfo lri\r\n" +
                  $"JOIN LabRegTest lrt\r\n" +
                  $"ON lri.LabRegDate = lrt.LabRegDate\r\n" +
                  $"AND lri.LabRegNo = lrt.LabRegNo\r\n" +
                  $"AND lri.CompCode IN (SELECT CompCode FROM ProgAuthGroupCompList WHERE AuthGroupCode = 'c4130')\r\n" +
                  $"JOIN LabOutsideTestCode lotc\r\n" +
                  $"ON lotc.OutsideCompCode = '4130'\r\n" +
                  $"AND lotc.OutsideTestCode = lrt.TestCode\r\n" +
                  $"AND lotc.OutsideSampleCode = lrt.SampleCode\r\n" +
                  $"JOIN LabTestCode ltc\r\n" +
                  $"ON lrt.TestCode = ltc.TestCode\r\n" +
                  $"JOIN ProgCompCode pcc\r\n" +
                  $"ON pcc.CompCode = lri.CompCode\r\n";

            if ((compMngCode ?? string.Empty) != string.Empty)
            {
                sql += $"AND pcc.CompMngCode = '{compMngCode}'\r\n";
            }
            sql += $"WHERE lri.LabRegDate BETWEEN '{beginDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}'\r\n" +
                   $"AND lrt.IsTestOutSide = {isTestOutside}\r\n"+
                   $"AND lri.LabRegNo BETWEEN {beginNo} AND {endNo}\r\n";

            if (isTestOutside == "1")
            {
                sql += "AND lrt.TestOutsideCompCode = '4130'\r\n";
            }
            else
            {
                sql += "AND ISNULL(lrt.TestOutsideCompCode, '') = ''\r\n";
            }

            sql += "ORDER BY lri.LabRegDate, lri.LabRegNo, lrt.OrderCode";

            return Ok(LabgeDatabase.SqlToJArray(sql));
        }

        public IHttpActionResult Post([FromBody]JArray request)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["InterfaceConnection"].ConnectionString);
            conn.Open();
            try
            {
                foreach (JObject objOrder in request)
                {
                    string sql;
                    sql = $"INSERT INTO SHHPath_Interface\r\n" +
                          $"(LabRegDate, LabRegNo, CompCode, CompName, ChartNo, PatientName, Age, Gender, TestCode, PartCode, TestDisplayName, StateCode)\r\n" +
                          $"VALUES\r\n" +
                          $"( '{objOrder["LabRegDate"]}'\r\n" +
                          $", {objOrder["LabRegNo"]}\r\n" +
                          $", '{objOrder["CompCode"]}'\r\n" +
                          $", '{objOrder["CompName"]}'\r\n" +
                          $", '{objOrder["PatientChartNo"]}'\r\n" +
                          $", '{objOrder["PatientName"]}'\r\n" +
                          $", '{objOrder["PatientAge"]}'\r\n" +
                          $", '{objOrder["PatientSex"]}'\r\n" +
                          $", '{objOrder["TestCode"]}'\r\n" +
                          $", '{GetPartCode(objOrder["TestCode"].ToString())}'" +
                          $", '{objOrder["TestDisplayName"]}'\r\n" +
                          $", 'O')";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();

                    //등록이 완료되면 우리 테이블에도 update해준다.
                    sql = $"UPDATE LabRegTest\r\n" +
                          $"SET IsTestOutside = '1'\r\n" +
                          $"  , TestStateCode = 'O'\r\n" +
                          $"  , TestOutSideBeginTime = GETDATE()\r\n" +
                          $"  , TestStartTime = GETDATE()\r\n" +
                          $"  , IsWorkCheck = '1'\r\n" +
                          $"  , WorkCheckMemberID = '{objOrder["RegistMemberID"]}'\r\n" +
                          $"  , WorkCheckTime  = GETDATE()\r\n" +
                          $"  , TestOutsideCompCode = '4130'\r\n" +
                          $"  , TestOutsideMemberID = '{objOrder["RegistMemberID"]}'\r\n" +
                          $"WHERE LabRegDate = '{Convert.ToDateTime(objOrder["LabRegDate"]):yyyy-MM-dd}'\r\n" +
                          $"AND LabRegNo = {objOrder["LabRegNo"]}\r\n" +
                          $"AND TestCode = '{objOrder["TestCode"]}'\r\n" +
                          $"\r\n" +
                          $"DECLARE @ReportCode varchar(30)\r\n" +
                          $"SELECT @ReportCode = ReportCode\r\n" +
                          $"FROM LabTestCode\r\n" +
                          $"WHERE TestCode = '{objOrder["TestCode"]}'\r\n" +
                          $"\r\n" +
                          $"UPDATE LabRegReport\r\n" +
                          $"SET ReportStartTime = GETDATE()\r\n" +
                          $"WHERE LabRegDate = '{Convert.ToDateTime(objOrder["LabRegDate"]):yyyy-MM-dd}'\r\n" +
                          $"AND LabRegNo = '{objOrder["LabRegNo"]}'\r\n" +
                          $"AND ReportCode = @ReportCode";
                    LabgeDatabase.ExecuteSql(sql);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject
                {
                    { "Status", Convert.ToInt32(HttpStatusCode.BadRequest) },
                    { "Message", ex.Message }
                };
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
            finally
            {
                conn.Close();
            }
        }

        private string GetPartCode(string testCode)
        {
            string sql;
            sql = $"SELECT PartCode\r\n" +
                  $"FROM LabTestCode\r\n" +
                  $"WHERE TestCode = '{testCode}'";
            return LabgeDatabase.ExecuteSqlScalar(sql).ToString();
        }
    }
}