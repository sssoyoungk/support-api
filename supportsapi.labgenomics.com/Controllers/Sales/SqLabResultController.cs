using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/sales/SqLabResult")]
    public class SqLabResultController : ApiController
    {
        /// <summary>
        /// 결과 조회
        /// </summary>
        /// <param name="beginDate">시작일자</param>
        /// <param name="endDate">종료일자</param>
        /// <param name="isUpdate">업데이트 여부</param>
        /// <returns></returns>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string isUpdate)
        {
            OracleConnection sqLabConn = new OracleConnection(ConfigurationManager.ConnectionStrings["SqLabConnection"].ConnectionString);
            sqLabConn.Open();
            try
            {
                UpdateUploadMst(sqLabConn);

                DataTable dt = new DataTable();
                string sql;
                sql = $"SELECT *\r\n" +
                      $"FROM V_R010103_LABGENOMICS\r\n" +
                      $"WHERE LAB_YN_DATE BETWEEN '{Convert.ToDateTime(beginDate).ToString("yyyy-MM-dd")}' AND TO_DATE('{Convert.ToDateTime(endDate).ToString("yyyy-MM-dd")}') + 1\r\n" +
                      $"AND CUST_CODE = '03475'\r\n" +
                      $"AND HOSPI_DOWN_YN = '{isUpdate}'\r\n" +
                      $"ORDER BY CUST_SAMPLENO";

                OracleCommand sqLabCmd = new OracleCommand(sql, sqLabConn);
                OracleDataAdapter sqLabAdapter = new OracleDataAdapter(sqLabCmd);
                sqLabAdapter.Fill(dt);

                JArray arrResponse = JArray.Parse(JsonConvert.SerializeObject(dt));

                return Ok(arrResponse);
            }
            finally
            {
                sqLabConn.Close();
            }
        }

        /// <summary>
        /// 결과 조회 전 완료된 결과 업데이트
        /// </summary>
        /// <param name="sqLabConn"></param>
        private void UpdateUploadMst(OracleConnection sqLabConn)
        {
            string sql;
            sql = "UPDATE UPLOADMST\r\n" +
                  "SET HOSPI_DOWN_YN = 'P'\r\n" +
                  "WHERE(REQ_NO, ITEM_CODE)\r\n" +
                  "    IN(SELECT REQ_NO, ITEM_CODE\r\n" +
                  "       FROM V_R010103_LABGENOMICS\r\n" +
                  "       WHERE LAB_YN_DATE BETWEEN(SYSDATE - 14) AND(SYSDATE + 1)\r\n" +
                  "       AND CUST_CODE = '03475'\r\n" +
                  "       AND HOSPI_DOWN_YN = 'N')";

            OracleCommand sqLabCmd = new OracleCommand(sql, sqLabConn);
            sqLabCmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 결과 등록
        /// </summary>
        /// <param name="arrResults"></param>
        /// <returns></returns>
        public IHttpActionResult Put([FromBody]JArray arrResults)
        {
            OracleConnection sqLabConn = new OracleConnection(ConfigurationManager.ConnectionStrings["SqLabConnection"].ConnectionString);
            sqLabConn.Open();

            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            sqlConn.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = sqlConn;

            SqlTransaction trans = sqlConn.BeginTransaction();
            cmd.Transaction = trans;

            string sql;
            try
            {
                foreach (JObject objResult in arrResults)
                {
                    string doctorCode = string.Empty;

                    if (objResult["TestResultText"].ToString().Contains("◆ 중 간 결 과 보 고 ◆"))
                    {
                        continue;
                    }

                    //수진자 접수 정보 일치 확인, 결과 상태가 최종이 아닌지 체크
                    sql = $"SELECT COUNT(*) AS CNT\r\n " +
                          $"FROM LabRegInfo Info\r\n " +
                          $"JOIN LabRegTest Test\r\n " +
                          $"ON Info.LabRegDate = Test.LabRegDate\r\n " +
                          $"AND Info.LabRegNo = Test.LabRegNo\r\n " +
                          $"AND Test.TestCode = '{objResult["TestCode"].ToString()}'\r\n" +
                          $"AND Test.TestStateCode <> 'F'\r\n " +
                          $"JOIN LabRegResult Result\r\n " +
                          $"ON Result.LabRegDate = Test.LabRegDate\r\n " +
                          $"AND Result.LabRegNo = Test.LabRegNo\r\n " +
                          $"AND Result.OrderCode = Test.TestCode\r\n " +
                          $"WHERE Info.LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"].ToString()).ToString("yyyy-MM-dd")}'\r\n" +
                          $"AND Info.LabRegNo = '{objResult["LabRegNo"].ToString()}'\r\n" +
                          $"AND Info.PatientName = '{objResult["PatientName"].ToString()}'\r\n" +
                          $"AND Info.PatientChartNo = '{objResult["PatientChartNo"].ToString()}'\r\n" +
                          $"AND Test.TestCode = '{objResult["TestCode"].ToString()}'";

                    cmd.CommandText = sql;
                    int count = Convert.ToInt32(cmd.ExecuteScalar()); //실행되는 쿼리의 첫번째 열의 첫번째 행을 반환

                    if (count > 0)
                    {
                        sql = $"UPDATE LabRegResult\r\n" +
                              $"   SET TestResult01 = '{objResult["TestResult01"].ToString().Trim()}'\r\n" +
                              $"     , TestResultText = '{objResult["TestResultText"].ToString().Trim()}'\r\n" +
                              $"     , TestResultAbn = '{objResult["TestResultAbn"].ToString()}'\r\n" +
                              $"     , EditTime = GETDATE()\r\n" +
                              $"     , EditorMemberID = 'Admin'\r\n" +
                              $"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"].ToString()).ToString("yyyy-MM-dd")}'\r\n" +
                              $"AND LabRegNo = '{objResult["LabRegNo"].ToString()}'\r\n" +
                              $"AND TestSubCode = '{objResult["TestSubCode"].ToString()}'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        //결과 업데이트가 완료 됐다면 LabRegTest, LabRegReport에 상태 업데이트
                        sql = $"UPDATE LabRegTest\r\n " +
                              $"   SET TestStateCode = 'S1'\r\n " +
                              $"     , TestEndTime = GETDATE()\r\n" +
                              $"     , TestOutsideEndTime = GETDATE()\r\n" +
                              $"     , EditTime = GETDATE()\r\n" +
                              $"     , EditorMemberID = 'Admin'\r\n" +
                              $"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"].ToString()).ToString("yyyy-MM-dd")}'\r\n " +
                              $"AND LabRegNo = '{objResult["LabRegNo"].ToString()}'\r\n" +
                              $"AND TestCode = '{objResult["TestCode"].ToString()}'\r\n" +
                              $"UPDATE LabRegReport\r\n " +
                              $"   SET ReportStateCode = 'S1'\r\n " +
                              $"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"].ToString()).ToString("yyyy-MM-dd")}'\r\n " +
                              $"AND LabRegNo = '{objResult["LabRegNo"].ToString()}'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        //sql = $"DECLARE @Cnt int\r\n" +
                        //      $"SELECT @Cnt = COUNT(*)\r\n" +
                        //      $"FROM LabRegTest\r\n " +                              
                        //      $"WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"].ToString()).ToString("yyyy-MM-dd")}'\r\n " +
                        //      $"AND LabRegNo = '{objResult["LabRegNo"].ToString()}'\r\n" +                              
                        //      $"AND TestStateCode <> 'F'\r\n" +
                        //      $"IF @Cnt = 0\r\n" +
                        //      $"BEGIN\r\n" +
                        //      $"    UPDATE LabRegReport\r\n " +
                        //      $"       SET ReportStateCode = 'F'\r\n " +
                        //      $"         , ReportEndTime = GETDATE()\r\n" +
                        //      $"         , IsReportPrintWait = 1\r\n" +
                        //      $"    WHERE LabRegDate = '{Convert.ToDateTime(objResult["LabRegDate"].ToString()).ToString("yyyy-MM-dd")}'\r\n " +
                        //      $"    AND LabRegNo = '{objResult["LabRegNo"].ToString()}'\r\n" +
                        //      $"END";
                        //cmd.CommandText = sql;
                        //cmd.ExecuteNonQuery();
                    }
                }
                trans.Commit();

                //결과 등록이 끝나면 결과전송 완료처리
                sql = "UPDATE UPLOADMST\r\n" +
                      "   SET HOSPI_DOWN_YN = 'Y'\r\n" +
                      "     , DOWN_DATE = SYSDATE\r\n" +
                      "WHERE (REQ_NO, ITEM_CODE) " +
                      "   IN (SELECT REQ_NO, ITEM_CODE\r\n" +
                      "       FROM V_R010103_LABGENOMICS\r\n" +
                      "       WHERE LAB_YN_DATE BETWEEN(SYSDATE - 5) AND(SYSDATE + 1)\r\n" +
                      "       AND CUST_CODE = '03475'\r\n" +
                      "       AND HOSPI_DOWN_YN = 'P')";
                OracleCommand sqLabCmd = new OracleCommand(sql, sqLabConn);
                sqLabCmd.ExecuteNonQuery();

                return Ok();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
            finally
            {
                sqlConn.Close();
                sqLabConn.Close();
            }
        }
    }
}