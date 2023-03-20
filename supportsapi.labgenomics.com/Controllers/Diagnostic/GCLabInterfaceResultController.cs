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

        public IHttpActionResult PutGclabResult(JObject objRequest)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();
            try
            {
                string orderCode = string.Empty;

                //OrderCode가 없으면 TestCode에 OrderCode를 넣어준다.
                JToken jtokenOrderCode = objRequest["OrderCode"];
                if (jtokenOrderCode == null || jtokenOrderCode.ToString() == string.Empty)
                {
                    orderCode = objRequest["TestCode"].ToString();
                }
                else
                {
                    orderCode = objRequest["OrderCode"].ToString();
                }

                string sql;

                //데이터 비교(수진자명, 차트번호, 주민번호)g
                sql = 
                    $"SELECT lri.PatientName, lri.PatientChartNo, lrt.TestStateCode, ltc.IsTestHeader\r\n" +
                    $"FROM LabRegInfo lri\r\n" +
                    $"JOIN LabRegTest lrt\r\n" +
                    $"ON lri.LabRegDate = lrt.LabRegDate\r\n" +
                    $"AND lri.LabRegNo = lrt.LabRegNo\r\n" +
                    $"AND lrt.TestCode = '{orderCode}'\r\n" +
                    $"JOIN LabRegResult lrr\r\n" +
                    $"ON lri.LabRegDate = lrr.LabRegDate\r\n" +
                    $"AND lri.LabRegNo = lrr.LabRegNo\r\n" +
                    $"AND lrr.TestSubCode = '{objRequest["TestCode"]}'\r\n" +
                    $"JOIN LabTestCode ltc\r\n" +
                    $"ON ltc.TestCode = lrr.TestSubCode\r\n" +
                    $"WHERE lri.LabRegDate = '{Convert.ToDateTime(objRequest["LabRegDate"]):yyyy-MM-dd}'\r\n" +
                    $"AND lri.LabRegNo = {objRequest["LabRegNo"]}";

                JObject objPatientInfo = LabgeDatabase.SqlToJObject(sql);

                //접수 데이터가 없는 경우
                if (objPatientInfo.Count == 0)
                {
                    throw new Exception("접수 데이터 없음");
                }
                //검사결과가 최종인 경우
                else if (objPatientInfo["TestStateCode"].ToString() == "F")
                {
                    throw new Exception("검사결과 상태 최종");
                }
                //수진자 정보가 일치하지 않는 경우
                else if (objPatientInfo["PatientName"].ToString() != objRequest["PatientName"].ToString())
                {
                    throw new Exception("수진자명 불일치");
                }
                else
                {
                    string testCode = objRequest["TestCode"].ToString();

                    if (Convert.ToBoolean(objPatientInfo["IsTestHeader"]) == true)
                        testCode += "01";

                    string resultField = string.Empty;
                    if (Encoding.Default.GetBytes(objRequest["TestResult"].ToString()).Length <= 50)
                        resultField = "TestResult01";
                    else
                        resultField = "TestResultText";

                    sql = $"UPDATE LabRegResult\r\n" +
                          $"   SET {resultField} = '{objRequest["TestResult"]}'\r\n" +
                          $"     , TestResultAbn = '{objRequest["TestResultAbn"]}'\r\n" +
                          $"     , EditTime = GETDATE()\r\n" +
                          $"     , EditorMemberID = '{objRequest["MemberID"]}'\r\n" +
                          $"WHERE LabRegDate = '{Convert.ToDateTime(objRequest["LabRegDate"]):yyyy-MM-dd}'\r\n" +
                          $"AND LabRegNo = {objRequest["LabRegNo"]}\r\n" +
                          $"AND TestSubCode = '{testCode}'\r\n\r\n" +

                          $"UPDATE LabRegTest\r\n" +
                          $"   SET TestOutsideEndTime = '{Convert.ToDateTime(objRequest["TestOutsideEndTime"]):yyyy-MM-dd}'\r\n" +
                          $"     , EditTime = GETDATE()\r\n" +
                          $"     , EditorMemberID = '{objRequest["MemberID"]}'\r\n" +
                          $"WHERE LabRegDate = '{Convert.ToDateTime(objRequest["LabRegDate"]):yyyy-MM-dd}'\r\n" +
                          $"AND LabRegNo = {objRequest["LabRegNo"]}\r\n" +
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