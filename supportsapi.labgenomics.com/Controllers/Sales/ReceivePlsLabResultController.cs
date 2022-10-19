using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/ReceivePlsLabResult")]
    public class ReceivePlsLabResultController : ApiController
    {
        /// <summary>
        /// 결과 수신할 데이터 조회
        /// </summary>
        /// <param name="transKind"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public IHttpActionResult Get(string transKind, DateTime beginDate, DateTime endDate)
        {
            OracleConnection plsConn = new OracleConnection(ConfigurationManager.ConnectionStrings["PlsLabConnection"].ConnectionString);
            plsConn.Open();

            string sql;
            try
            {
                if (transKind == "N")
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = plsConn;
                    cmd.CommandTimeout = 300;

                    sql = "MERGE INTO JJLAB.uploadmst u\r\n" +
                          "  USING(select a.reqno, a.itemcd from Customers @NEWJJLAB c, JJLAB.uploadmst a, EverResult_M@NEWJJLAB b where c.LCSCon = 'L' and\r\n" +
                          "             (c.cstcd = a.cstcd OR c.COMPANYCODE = a.cstcd) and a.result_down = 'F' and nvl(a.reqno, ' ') <> ' '\r\n" +
                          "             AND TRIM(b.reqno) = TRIM(a.reqno) and TRIM(a.itemcd) = TRIM(b.item_id)\r\n" +
                          "             AND b.req_date between to_char(sysdate - 15, 'yyyymmdd') and to_char(sysdate, 'yyyymmdd') and nvl(b.input_date, ' ') <> ' ') g\r\n" +
                          "  ON(u.reqno = g.reqno AND u.itemcd = g.itemcd) WHEN matched THEN\r\n" +
                          "UPDATE SET u.result_down = 'L'";
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    sql =
                        "select to_char(to_date(substr(b.CUST_FRONT_NO,1,8)), 'YYYY-MM-DD') AS LabRegDate, substr(b.CUST_FRONT_NO,9) AS LabRegNo\r\n" +
                        " , b.CUST_ID,b.TEST_DIV, a.SAMDTE, b.CUST_FRONT_NO, a.PATNM AS PATIENT_NAME, b.PATIENT_ID\r\n" +
                        " , b.ID_NO,a.REQNO,b.INSURE_CODE,b.ITEM_ID,b.ITEM_NAME,b.RESULT, b.DECISION, b.INPUT_DATE\r\n" +
                        " , b.CUST_ITEM_ID,a.NO, b.IMGFILE_YN, b.CUST_ITEM_NAME, a.BIRDTE, a.ITEMCD, b.REQ_DATE, b.SLIDENO\r\n" +
                        "  from Customers @NEWJJLAB c, JJLAB.uploadmst a, EverResult_M@NEWJJLAB b\r\n" +
                        "   where c.LCSCon = 'L' and(c.cstcd = a.cstcd OR c.COMPANYCODE = a.cstcd) and a.result_down = 'L' and nvl(a.reqno,' ') <> ' '\r\n" +
                        "   AND TRIM(b.reqno) = TRIM(a.reqno) and Trim(a.itemcd)= b.item_id\r\n" +
                        "   AND b.req_date between  to_char(sysdate - 30, 'yyyymmdd') and to_char(sysdate,'yyyymmdd')\r\n" +
                        "   and nvl(b.input_date,' ')<> ' ' and b.CANCEL_YN = 'N' and nvl(b.RESULT, ' ')<> ' '";
                    cmd.CommandText = sql;
                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    JArray array = JArray.Parse(JsonConvert.SerializeObject(dt));

                    return Ok(array);
                }
                else
                {
                    sql =                    
                        $"select to_char(to_date(substr(b.CUST_FRONT_NO,1,8)), 'YYYY-MM-DD') AS LabRegDate, substr(b.CUST_FRONT_NO,9) AS LabRegNo\r\n" +
                        $" , b.CUST_ID,b.TEST_DIV, a.SAMDTE, b.CUST_FRONT_NO, a.PATNM AS PATIENT_NAME, b.PATIENT_ID\r\n" +
                        $" , b.ID_NO,a.REQNO,b.INSURE_CODE,b.ITEM_ID,b.ITEM_NAME,b.RESULT, b.DECISION, b.INPUT_DATE\r\n" +
                        $" , b.CUST_ITEM_ID,a.NO, b.IMGFILE_YN, b.CUST_ITEM_NAME, a.BIRDTE, a.ITEMCD, b.REQ_DATE, b.SLIDENO\r\n" +
                        $"  from Customers @NEWJJLAB c, JJLAB.uploadmst a, EverResult_M@NEWJJLAB b\r\n" +
                        $"   where c.LCSCon = 'L' and(c.cstcd = a.cstcd OR c.COMPANYCODE = a.cstcd) and a.result_down = 'T' and nvl(a.reqno,' ') <> ' '\r\n" +
                        $"   AND TRIM(b.reqno) = TRIM(a.reqno) and Trim(a.itemcd)= b.item_id\r\n" +
                        $"   AND b.req_date between  '{beginDate.ToString("yyyyMMdd")}' and '{endDate.ToString("yyyyMMdd")}'\r\n" +
                        $"   and nvl(b.input_date,' ')<> ' ' and b.CANCEL_YN = 'N' and nvl(b.RESULT, ' ')<> ' '";
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = plsConn;
                    cmd.CommandTimeout = 300;

                    cmd.CommandText = sql;
                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    JArray array = JArray.Parse(JsonConvert.SerializeObject(dt));

                    return Ok(array);
                }


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
                plsConn.Close();
            }
        }

        /// <summary>
        /// 결과등록
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHttpActionResult Put([FromBody]JObject request)
        {
            OracleConnection plsConn = new OracleConnection(ConfigurationManager.ConnectionStrings["PlsLabConnection"].ConnectionString);
            plsConn.Open();

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();
            SqlTransaction trans = conn.BeginTransaction("PlsLabTransaction");

            string doctorCode = string.Empty;
            try
            {
                //수진자 접수 정보 일치 확인, 결과 상태가 최종이 아닌지 체크
                string sql;
                sql = $"SELECT COUNT(*) AS CNT\r\n" +
                      $"FROM LabRegInfo Info\r\n" +
                      $"JOIN LabRegTest Test\r\n" +
                      $"ON Info.LabRegDate = Test.LabRegDate\r\n" +
                      $"AND Info.LabRegNo = Test.LabRegNo\r\n" +
                      $"AND Test.TestCode = '{request["TestCode"].ToString()}'\r\n" +
                      $"AND Test.TestStateCode <> 'F'\r\n" +
                      $"JOIN LabRegResult Result\r\n" +
                      $"ON Result.LabRegDate = Test.LabRegDate\r\n" +
                      $"AND Result.LabRegNo = Test.LabRegNo\r\n" +
                      $"AND Result.OrderCode = Test.TestCode\r\n" +
                      $"WHERE Info.LabRegDate = '{request["LabRegDate"].ToString()}'\r\n" +
                      $"AND Info.LabRegNo = '{request["LabRegNo"].ToString()}'\r\n" +
                      $"AND Info.PatientName = '{request["PatientName"].ToString()}'\r\n" +
                      $"AND Info.PatientChartNo = '{request["PatientChartNo"].ToString()}'\r\n" +
                      $"AND Info.PatientJuminNo01 = '{request["PatientJuminNo01"].ToString()}'";
                int count = Convert.ToInt32(LabgeDatabase.ExecuteSqlScalar(sql));

                if (count > 0)
                {
                    foreach (JObject objResult in request["Results"])
                    {

                        if (objResult["TestResultText"].ToString().Contains("김요나"))
                        {
                            doctorCode = "plslab1";
                        }
                        else if (objResult["TestResultText"].ToString().Contains("유택균"))
                        {
                            doctorCode = "plslab2";
                        }
                        else
                        {
                            doctorCode = string.Empty;
                        }

                        sql = $"UPDATE LabRegResult\r\n" +
                              $"SET" +
                              $"    TestResultText = '{objResult["TestResultText"].ToString().Replace("'", "''") }'\r\n" +
                              $"  , TestResult01 = '{objResult["TestResult"].ToString()}'\r\n" +
                              $"  , TestResultAbn = '{objResult["TestResultAbn"].ToString()}'\r\n" +
                              $"  , EditTime = GETDATE()\r\n" +
                              $"  , EditorMemberID = 'Admin'\r\n" +
                              $"WHERE LabRegDate = '{objResult["LabRegDate"].ToString()}'\r\n" +
                              $"AND LabRegNo = '{objResult["LabRegNo"].ToString()}'\r\n" +
                              $"AND TestSubCode = '{objResult["TestSubCode"].ToString() }'";

                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Transaction = trans;

                        cmd.ExecuteNonQuery();
                    }

                    //결과 업데이트가 완료 됐다면 LabRegTest, LabRegReport에 상태 업데이트
                    sql = $"UPDATE LabRegTest\r\n" +
                          $"SET TestEndTime = GETDATE()\r\n" +
                          $"  , TestOutsideEndTime = GETDATE()\r\n" +
                          $"  , EditTime = GETDATE()\r\n" +
                          $"  , EditorMemberID = 'Admin' \r\r\n" +
                          $"  , DoctorCode = '{doctorCode}'\r\n" +//의사 서명을 위한 판독의정보 입력
                          $"  , TestStateCode = 'F'\r\n" +//최종으로 변경해달라고 전주영업소에서 요청 (2020-01-16 이호진 요청)
                          $"WHERE LabRegDate = '{request["LabRegDate"].ToString()}'\r\n " +
                          $"AND LabRegNo = '{request["LabRegNo"].ToString()}'\r\n " +
                          $"UPDATE LabRegReport\r\n" +
                          $"SET ReportEndTime = GETDATE()\r\n" +
                          $"  , IsReportPrintWait = 1\r\n" +
                          $"  , ReportStateCode = 'F'\r\n " +
                          $"WHERE LabRegDate = '{request["LabRegDate"].ToString()}'\r\n" +
                          $"AND LabRegNo = '{request["LabRegNo"].ToString()}' ";

                    SqlCommand cmdTest = new SqlCommand(sql, conn);
                    cmdTest.Transaction = trans;
                    cmdTest.ExecuteNonQuery();

                    sql = $"UPDATE JJLAB.uploadmst\r\n" +
                          $"SET RESULT_DOWN = 'T'\r\n" +
                          $"  , DOWN_DATE = SYSDATE\r\n" +
                          $"WHERE REQDTE = '{request["REQDTE"].ToString()}'\r\n" +
                          $"AND REQNO = '{request["REQNO"].ToString()}'\r\n" +
                          $"AND ITEMCD = '{request["ITEMCD"].ToString()}'";
                    OracleCommand cmdPlsLab = new OracleCommand(sql, plsConn);
                    cmdPlsLab.ExecuteNonQuery();

                    trans.Commit();
                }
                else
                {
                    //롤백
                    trans.Rollback();

                    string message = $"{request["LabRegDate"].ToString()} {request["LabRegNo"].ToString()} {request["PatientName"].ToString()}" +
                        $" 일치하는 정보가 없거나, 검사가 최종처리 상태입니다.";
                    JObject objResponse = new JObject();
                    objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.NoContent));
                    objResponse.Add("Message", message);
                    return Content(HttpStatusCode.BadRequest, objResponse);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                //롤백
                trans.Rollback();

                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
            finally
            {
                conn.Close();
                plsConn.Close();
            }
        }

        [Route("api/Sales/ReceivePlsLabResult/ResultState")]
        public IHttpActionResult PutPlsLabResultState([FromBody]JObject objRequest)
        {
            OracleConnection plsConn = new OracleConnection(ConfigurationManager.ConnectionStrings["PlsLabConnection"].ConnectionString);
            plsConn.Open();

            try
            {
                string sql;
                sql = $"UPDATE JJLAB.UPLOADMST SET RESULT_DOWN = '{objRequest["Code"].ToString()}'\r\n" +
                      $"WHERE RESULT_DOWN = 'L'";
                OracleCommand cmd = new OracleCommand(sql, plsConn);
                cmd.ExecuteNonQuery();
                return Ok();
            }
            finally
            {
                plsConn.Close();
            }
        }
    }
}