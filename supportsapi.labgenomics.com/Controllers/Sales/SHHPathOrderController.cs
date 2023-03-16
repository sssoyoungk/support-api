using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/sales/SHHPathOrder")]
    public class SHHPathOrderController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string isTestOutside, int beginNo, int endNo, string compMngCode)
        {
            string sql;
            sql = $"SELECT A.LabRegDate, A.LabRegNo, A.CompCode\r\n" +
                  $"     , (SELECT CompName FROM ProgCompCode WHERE A.CompCode = CompCode) AS CompName\r\n" +
                  $"     , A.PatientName, A.PatientAge, A.PatientSex, A.PatientJuminNo01, A.PatientChartNo\r\n" +
                  $"     , B.OrderCode, B.TestCode\r\n" +
                  $"     , (SELECT TestDisplayName FROM LabTestCode WHERE B.TestCode = TestCode) AS TestDisplayName\r\n" +
                  $"     , B.SampleCode\r\n" +
                  $"     , (SELECT SampleName FROM LabSampleCode WHERE B.SampleCode = SampleCode) AS SampleName\r\n" +
                  $"     , B.IsTestOutside, B.TestOutsideBeginTime, B.TestOutsideEndTime, B.TestOutsideCompCode, B.TestOutsideMemberID\r\n" +
                  $"FROM LabRegInfo A\r\n" +
                  $"JOIN LabRegTest B\r\n" +
                  $"ON A.LabRegDate = B.LabRegDate\r\n" +
                  $"AND A.LabRegNo = B.LabRegNo\r\n" +
                  $"AND A.CompCode IN (SELECT CompCode FROM ProgAuthGroupCompList WHERE AuthGroupCode = 'c4130')\r\n" +
                  $"JOIN LabOutsideTestCode C\r\n" +
                  $"ON C.OutsideCompCode = '4130'\r\n" +
                  $"AND C.OutsideTestCode = B.TestCode\r\n" +
                  $"AND C.OutsideSampleCode = B.SampleCode\r\n" +
                  $"WHERE A.LabRegDate BETWEEN '{beginDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}'\r\n" +
                  $"AND B.IsTestOutSide = {isTestOutside}\r\n";

            if (isTestOutside == "1")
            {
                sql += "AND B.TestOutsideCompCode = '4130'\r\n";
            }
            else
            {
                sql += "AND ISNULL(B.TestOutsideCompCode, '') = ''\r\n";
            }

            sql += "ORDER BY A.LabRegDate, A.LabRegNo, B.OrderCode";

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
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
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