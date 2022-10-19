using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/SHHPathResult")]
    public class SHHPathResultController : ApiController
    {
        /// <summary>
        /// 신현호 병리과 등록할 결과 조회
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["InterfaceConnection"].ConnectionString);
            conn.Open();
            try
            {
                string sql;
                sql = $"SELECT *\r\n" +
                      $"FROM SHHPath_Interface\r\n" +
                      $"WHERE StateCode = 'R'\r\n" +
                      $"AND ResultSendDateTime >= '{beginDate.ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND ResultSendDateTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')";
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                JArray array = JArray.Parse(JsonConvert.SerializeObject(dt));

                return Ok(array);
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

        /// <summary>
        /// 신현호병리과 결과 등록
        /// </summary>
        /// <param name="arrRequest"></param>
        /// <returns></returns>
        public IHttpActionResult Put([FromBody]JArray arrRequest)
        {
            try
            {
                foreach (JObject objResult in arrRequest)
                {
                    string sql;

                    sql = $"SELECT lrr.LabRegDate, lrr.LabRegNo, lrr.TestCode, lrr.TestSubCode\r\n " +
                          $"FROM LabRegInfo lri\r\n " +
                          $"JOIN LabRegTest lrt\r\n " +
                          $"ON lri.LabRegDate = lrt.LabRegDate\r\n " +
                          $"AND lri.LabRegNo = lrt.LabRegNo\r\n " +
                          $"AND lrt.TestCode = '{objResult["TestCode"].ToString()}'\r\n" +
                          $"AND lrt.TestStateCode <> 'F'\r\n " +
                          $"JOIN LabRegResult lrr\r\n " +
                          $"ON lrr.LabRegDate = lrt.LabRegDate\r\n " +
                          $"AND lrr.LabRegNo = lrt.LabRegNo\r\n " +
                          $"AND lrr.OrderCode = lrt.OrderCode\r\n" +
                          $"AND lrr.TestCode = lrt.TestCode\r\n" +
                          $"JOIN LabTestCode ltc\r\n" +
                          $"ON ltc.TestCode = lrr.TestSubCode\r\n" +
                          $"AND ltc.IsTestHeader <> 1" +
                          $"WHERE lri.LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                          $"AND lri.LabRegNo = '{objResult["LabRegNo"].ToString()}'\r\n" +
                          $"AND lri.PatientName = '{objResult["PatientName"].ToString()}'\r\n";

                    int registCount = 0;
                    DataTable dtLabRegResult = LabgeDatabase.SqlToDataTable(sql);
                    if (dtLabRegResult.Rows.Count > 0)
                    {
                        //부속코드가 하나만 있는 경우 (ex Gimsa Stain)
                        if (dtLabRegResult.Rows.Count == 1)
                        {

                        }
                        //부속코드가 있는 경우
                        else if (dtLabRegResult.Rows.Count > 1)
                        {
                            for (int i = 0; i < dtLabRegResult.Rows.Count; i++)
                            {
                                string resultText = objResult["TestResult" + Convert.ToString(i + 1).PadLeft(2, '0')].ToString().Replace("▒ Gross Description", "")
                                    .Replace("▒ Summary of Sections", "").Replace("▒ Pathological Diagnosis", "").Replace("▒ GENERAL CATEGORIZATION", "").Trim();

                                //TestResult01값에 병리번호 입력한다.
                                if (i == 0 && objResult["ExamNo"].ToString() != string.Empty)
                                {
                                    resultText = $"병리번호 : {objResult["ExamNo"].ToString()}\r\n\r\n{resultText}";
                                }

                                //TestResult04값에는 재위탁 코멘트 입력한다.
                                if (i == dtLabRegResult.Rows.Count - 1)
                                {
                                    resultText += "\r\n" + "이 결과는 랩지노믹스로부터 재위탁받은 신현호병리과 검사결과입니다.";
                                }

                                sql = $"UPDATE LabRegResult\r\n" +
                                      $"SET TestResultText = '{resultText}'\r\n" +
                                      $"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                                      $"AND LabRegNo = {objResult["LabRegNo"].ToString()}\r\n" +
                                      $"AND TestCode = '{objResult["TestCode"].ToString()}'\r\n" +
                                      $"AND TestSubCode = '{dtLabRegResult.Rows[i]["TestSubCode"].ToString()}'";
                                LabgeDatabase.ExecuteSql(sql);
                                registCount++;
                            }
                        }
                    }

                    //결과 등록 카운트가 0 이상이면 최종처리
                    if (registCount > 0)
                    {
                        //결과 등록이 완료되면 최종처리 (조직,세포팀에서 최종처리 해달라고 요청하면 이 소스로 대체)
                        sql = $"UPDATE LabRegTest\r\n" +
                              $"SET TestEndTime = GETDATE()\r\n" +
                              $"  , TestOutsideEndTime = GETDATE()\r\n" +
                              $"  , EditTime = GETDATE()\r\n" +
                              $"  , EditorMemberID = 'Admin' \r\r\n" +
                              $"  , DoctorCode = '{objResult["DoctorCode"].ToString()}'\r\n" +//의사 서명을 위한 판독의정보 입력
                              $"  , TestStateCode = 'F'\r\n" +
                              $"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n " +
                              $"AND LabRegNo = {objResult["LabRegNo"].ToString()}\r\n " +
                              $"UPDATE LabRegReport\r\n" +
                              $"SET ReportEndTime = GETDATE()\r\n" +
                              $"  , IsReportPrintWait = 1\r\n" +
                              $"  , ReportStateCode = 'F'\r\n " +
                              $"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                              $"AND LabRegNo = {objResult["LabRegNo"].ToString()}";

                        //일단 판독 상태로
                        //sql = $"UPDATE LabRegTest\r\n" +
                        //      $"SET TestOutsideEndTime = GETDATE()\r\n" +
                        //      $"  , EditTime = GETDATE()\r\n" +
                        //      $"  , EditorMemberID = 'Admin' \r\r\n" +
                        //      $"  , DoctorCode = '{objResult["DoctorCode"].ToString()}'\r\n" +//의사 서명을 위한 판독의정보 입력
                        //      $"  , TestStateCode = 'PF'\r\n" +
                        //      $"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n " +
                        //      $"AND LabRegNo = {objResult["LabRegNo"].ToString()}\r\n " +
                        //      $"UPDATE LabRegReport\r\n" +
                        //      $"SET ReportStateCode = 'PF'\r\n " +
                        //      $"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                        //      $"AND LabRegNo = {objResult["LabRegNo"].ToString()}";
                        LabgeDatabase.ExecuteSql(sql);

                        //최종처리 완료후 연동 테이블 완료처리
                        sql = $"UPDATE SHHPath_Interface\r\n" +
                              $"SET StateCode = 'F'\r\n" +
                              $"  , ResultRegistDateTime = GETDATE()\r\n" +
                              $"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                              $"AND LabRegNo = {objResult["LabRegNo"].ToString()}\r\n" +
                              $"AND TestCode = '{objResult["TestCode"].ToString()}'";
                        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["InterfaceConnection"].ConnectionString);
                        conn.Open();

                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();

                        conn.Close();
                    }
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
        }
    }
}