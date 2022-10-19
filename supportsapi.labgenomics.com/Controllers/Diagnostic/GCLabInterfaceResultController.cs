using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    [Route("api/Diagnostic/GCLabInterfaceResult")]
    public class GCLabInterfaceResultController : ApiController
    {
        [Route("api/Diagnostic/GCLabInterfaceResult/OutsourcingTestCode")]
        public IHttpActionResult GetOutsourcingTestCode(string compCode)
        {
            string sql;
            sql = $"SELECT DISTINCT OutsideCompCode, OutsideTestCode\r\n" +
                  $"  FROM LabOutsideTestCode\r\n" +
                  $" WHERE OutsideCompCode = '{compCode}'";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        public IHttpActionResult PutGclabResult(JObject request)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();
            try
            {
                string orderCode = string.Empty;

                //OrderCode가 없으면 TestCode에 OrderCode를 넣어준다.
                JToken jtokenOrderCode = request["OrderCode"];
                if (jtokenOrderCode == null || jtokenOrderCode.ToString() == string.Empty)
                {
                    orderCode = request["TestCode"].ToString();
                }
                else
                {
                    orderCode = request["OrderCode"].ToString();
                }

                string sql;

                //데이터 비교(수진자명, 차트번호, 주민번호)g
                sql = $"SELECT @PatientName = A.PatientName\r\n" +
                      $"     , @PatientChartNo = A.PatientChartNo\r\n" +
                      $"     , @TestStateCode = B.TestStateCode\r\n" +
                      $"     , @IsTestHeader = D.IsTestHeader\r\n" +
                      $"FROM LabRegInfo A\r\n" +
                      $"JOIN LabRegTest B\r\n" +
                      $"ON A.LabRegDate = B.LabRegDate\r\n" +
                      $"AND A.LabRegNo = B.LabRegNo\r\n" +
                      $"AND B.TestCode = '{orderCode}'\r\n" +
                      $"JOIN LabRegResult C\r\n" +
                      $"ON A.LabRegDate = C.LabRegDate\r\n" +
                      $"AND A.LabRegNo = C.LabRegNo\r\n" +
                      $"AND C.TestSubCode = '{request["TestCode"].ToString()}'\r\n" +
                      $"JOIN LabTestCode D\r\n" +
                      $"ON D.TestCode = C.TestSubCode\r\n" +
                      $"WHERE A.LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND A.LabRegNo = {request["LabRegNo"].ToString()}";

                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.Add("@PatientName", SqlDbType.VarChar, 50);
                cmd.Parameters["@PatientName"].Direction = ParameterDirection.Output;

                cmd.Parameters.Add("@PatientChartNo", SqlDbType.VarChar, 50);
                cmd.Parameters["@PatientChartNo"].Direction = ParameterDirection.Output;

                cmd.Parameters.Add("@TestStateCode", SqlDbType.VarChar, 1);
                cmd.Parameters["@TestStateCode"].Direction = ParameterDirection.Output;

                cmd.Parameters.Add("@IsTestHeader", SqlDbType.Bit);
                cmd.Parameters["@IsTestHeader"].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                //접수 데이터가 없는 경우
                if ((Object)cmd.Parameters["@PatientName"].Value == DBNull.Value)
                {
                    throw new Exception("접수 데이터 없음");
                }
                //검사결과가 최종인 경우
                else if (cmd.Parameters["@TestStateCode"].Value.ToString() == "F")
                {
                    throw new Exception("검사결과 상태 최종");
                }
                //수진자 정보가 일치하지 않는 경우
                else if (cmd.Parameters["@PatientName"].Value.ToString() != request["PatientName"].ToString())
                {
                    throw new Exception("수진자명 불일치");                    
                }
                else
                {
                    string testCode = request["TestCode"].ToString();
                    
                    if (Convert.ToBoolean(cmd.Parameters["@IsTestHeader"].Value) == true)
                        testCode += "01";

                    string resultField = string.Empty;
                    if (Encoding.Default.GetBytes(request["TestResult"].ToString()).Length <= 50)
                        resultField = "TestResult01";
                    else
                        resultField = "TestResultText";

                    sql = $"UPDATE LabRegResult\r\n" +
                          $"   SET {resultField} = '{request["TestResult"].ToString()}'\r\n" +
                          $"     , TestResultAbn = '{request["TestResultAbn"].ToString()}'\r\n" +
                          $"     , EditTime = GETDATE()\r\n" +
                          $"     , EditorMemberID = '{request["MemberID"].ToString()}'\r\n" +
                          $"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                          $"AND LabRegNo = {request["LabRegNo"].ToString()}\r\n" +
                          $"AND TestSubCode = '{testCode}'\r\n\r\n" +

                          $"UPDATE LabRegTest\r\n" +
                          $"   SET TestOutsideEndTime = '{Convert.ToDateTime(request["TestOutsideEndTime"]).ToString("yyyy-MM-dd")}'\r\n" +
                          $"     , EditTime = GETDATE()\r\n" +
                          $"     , EditorMemberID = '{request["MemberID"].ToString()}'\r\n" +
                          $"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                          $"AND LabRegNo = {request["LabRegNo"].ToString()}\r\n" +
                          $"AND TestCode = '{orderCode}'";

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
    }
}