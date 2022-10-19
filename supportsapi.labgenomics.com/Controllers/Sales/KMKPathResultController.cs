using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/KMKPathResult")]
    public class KMKPathResultController : ApiController
    {
        /// <summary>
        /// 김민경 병리과 등록할 결과 조회
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
                StringBuilder sb = new StringBuilder();
                sb.Append($"SELECT *");
                sb.AppendLine($"FROM KMKPath_Interface");
                sb.AppendLine($"WHERE StateCode = 'R'");
                sb.AppendLine($"AND ResultSendDateTime >= '{beginDate.ToString("yyyy-MM-dd")}'");
                sb.AppendLine($"AND ResultSendDateTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')");

                //string sql;
                //sql = $"SELECT *\r\n" +
                //      $"FROM KMKPath_Interface\r\n" +
                //      $"WHERE StateCode = 'R'\r\n" +
                //      $"AND ResultSendDateTime >= '{beginDate.ToString("yyyy-MM-dd")}'\r\n" +
                //      $"AND ResultSendDateTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')";
                Debug.WriteLine(sb.ToString());
                SqlCommand cmd = new SqlCommand(sb.ToString(), conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                sb.Clear();
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
        /// 김민경병리과 결과 등록
        /// </summary>
        /// <param name="arrRequest"></param>
        /// <returns></returns>
        public IHttpActionResult Put([FromBody]JArray arrRequest)
        {
            try
            {
                foreach (JObject objResult in arrRequest)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"SELECT lrr.LabRegDate, lrr.LabRegNo, lrr.TestCode, lrr.TestSubCode");
                    sb.AppendLine($"FROM LabRegInfo lri");
                    sb.AppendLine($"JOIN LabRegTest lrt");
                    sb.AppendLine($"ON lri.LabRegDate = lrt.LabRegDate");
                    sb.AppendLine($"AND lri.LabRegNo = lrt.LabRegNo");
                    sb.AppendLine($"AND lrt.TestCode = '{objResult["TestCode"].ToString()}'");
                    sb.AppendLine($"AND lrt.TestStateCode <> 'F'");
                    sb.AppendLine($"JOIN LabRegResult lrr");
                    sb.AppendLine($"ON lrr.LabRegDate = lrt.LabRegDate");
                    sb.AppendLine($"AND lrr.LabRegNo = lrt.LabRegNo");
                    sb.AppendLine($"AND lrr.OrderCode = lrt.OrderCode");
                    sb.AppendLine($"AND lrr.TestCode = lrt.TestCode");
                    sb.AppendLine($"JOIN LabTestCode ltc");
                    sb.AppendLine($"ON ltc.TestCode = lrr.TestSubCode");
                    sb.AppendLine($"AND ltc.IsTestHeader <> 1");
                    sb.AppendLine($"WHERE lri.LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"]).ToString("yyyy-MM-dd")}'");
                    sb.AppendLine($"AND lri.LabRegNo = '{objResult["LabRegNo"].ToString()}'");
                    sb.AppendLine($"AND lri.PatientName = '{objResult["PatientName"].ToString()}'");

                    Debug.WriteLine(sb.ToString()); //데이터 확인까지는 진행 완료
                    int registCount = 0;
                    DataTable dtLabRegResult = LabgeDatabase.SqlToDataTable(sb.ToString());

                    //부속코드가 있는 경우  ★수정 필요★(데이터 들어오는 부분을 확인 후 진행하여야함.
                    if (dtLabRegResult.Rows.Count > 0)
                    {
                        //부속코드가 하나만 있는 경우 (ex Gimsa Stain)
                        if (dtLabRegResult.Rows.Count == 1)
                        {

                        }
                        
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
                                    resultText += "\r\n" + "이 결과는 랩지노믹스로부터 재위탁받은 김민경병리과 검사결과입니다.";
                                }

                                sb.Clear();
                                sb.AppendLine($"UPDATE LabRegResult");
                                sb.AppendLine($"SET TestResultText = '{resultText}'");
                                sb.AppendLine($"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"]).ToString("yyyy-MM-dd")}'");
                                sb.AppendLine($"AND LabRegNo = {objResult["LabRegNo"].ToString()}");
                                sb.AppendLine($"AND TestCode = '{objResult["TestCode"].ToString()}'");
                                sb.AppendLine($"AND TestSubCode = '{dtLabRegResult.Rows[i]["TestSubCode"].ToString()}'");
                                LabgeDatabase.ExecuteSql(sb.ToString());

                                registCount++;
                            }
                        }
                    }

                    //결과 등록 카운트가 0 이상이면 최종처리
                    if (registCount > 0)
                    {
                        sb.Clear();
                        //결과 등록이 완료되면 최종처리 (조직,세포팀에서 최종처리 해달라고 요청하면 이 소스로 대체)
                        sb.AppendLine($"UPDATE LabRegTest"); 
                        sb.AppendLine($"SET TestEndTime = GETDATE()");
                        sb.AppendLine($"  , TestOutsideEndTime = GETDATE()");
                        sb.AppendLine($"  , EditTime = GETDATE()");
                        sb.AppendLine($"  , EditorMemberID = 'Admin' ");
                        sb.AppendLine($"  , DoctorCode = '{objResult["DoctorCode"].ToString()}'");//의사 서명을 위한 판독의정보 입력
                        sb.AppendLine($"  , TestStateCode = 'F'");
                        sb.AppendLine($"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"]).ToString("yyyy-MM-dd")}'");
                        sb.AppendLine($"AND LabRegNo = {objResult["LabRegNo"].ToString()}");
                        sb.AppendLine($"UPDATE LabRegReport");
                        sb.AppendLine($"SET ReportEndTime = GETDATE()");
                        sb.AppendLine($"  , IsReportPrintWait = 1");
                        sb.AppendLine($"  , ReportStateCode = 'F'");
                        sb.AppendLine($"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"]).ToString("yyyy-MM-dd")}'");
                        sb.AppendLine($"AND LabRegNo = {objResult["LabRegNo"].ToString()}");

                        #region backup
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
                        #endregion

                        LabgeDatabase.ExecuteSql(sb.ToString());

                        sb.Clear();
                        //최종처리 완료후 연동 테이블 완료처리
                        sb.AppendLine($"UPDATE KMKPath_Interface");
                        sb.AppendLine($"SET StateCode = 'F'");
                        sb.AppendLine($"  , ResultRegistDateTime = GETDATE()");
                        sb.AppendLine($"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"]).ToString("yyyy-MM-dd")}'");
                        sb.AppendLine($"AND LabRegNo = {objResult["LabRegNo"].ToString()}");
                        sb.AppendLine($"AND TestCode = '{objResult["TestCode"].ToString()}'");
                        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["InterfaceConnection"].ConnectionString);
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(sb.ToString(), conn);
                        cmd.ExecuteNonQuery();
                        sb.Clear();
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