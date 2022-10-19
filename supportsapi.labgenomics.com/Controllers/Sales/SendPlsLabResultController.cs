using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    /// <summary>
    /// 이기은 진단에서 랩지노믹스로 외주 의뢰
    /// </summary>
    [Route("api/Sales/SendPlsLabResult")]
    public class SendPlsLabResultController : ApiController
    {
        private const string plsLabConnString = "User Id=jint;Password=1234;Data Source=plslab.co.kr:1521/ORCL;Min Pool Size=0;Connection Lifetime=180;Max Pool Size=50;Incr Pool Size=5;";

        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string searchKind, string resultSendState)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();
            try
            {
                string sql;
                
                sql = $"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED\r\n" +
                      $"SELECT LabRegDate, LabRegNo, PatientChartNo, PatientName, CompOrderDate, CompOrderNo, CompSpcNo, CompTestCode, TestCode, TestSubCode, TestDisplayName, TestEndDate\r\n" +
                      $"     , CASE WHEN LEN(LTRIM(RTRIM(TestResult))) > 20 THEN '' ELSE TestResult END AS TestResult\r\n" +
                      $"     , CASE\r\n" +
                      $"           WHEN LEN(LTRIM(RTRIM(TestResult))) > 20 THEN TestResult + CHAR(13) + CHAR(10) + TestResultText\r\n" +
                      $"           WHEN TestResult = TestResultText THEN ''\r\n" +
                      $"           ELSE TestResultText\r\n" +
                      $"       END TestResultText\r\n" +
                      $"     , TestResultAbn, ReportCode, TestRefUnit, TestReferValue, JuminNo, IsTestHeader, TestModuleCode\r\n" +
                      $"     , (SELECT LabRegReportID FROM LabRegReport WHERE Sub1.LabRegDate = LabRegDate AND Sub1.LabRegNo = LabRegNo AND Sub1.ReportCode = ReportCode) AS LabRegReportID\r\n" +
                      $"FROM\r\n" +
                      $"(\r\n" +
                      $"    SELECT lrt.LabRegDate, lrt.LabRegNo, lri.PatientChartNo, lri.PatientName, CONVERT(varchar, ltcoi.CompOrderDate, 112) AS CompOrderDate\r\n" +
                      $"         , ltcoi.CompOrderNo, ltcoi.CompSpcNo, ltcoi.CompTestCode , lrt.TestCode, ltcoi.TestCode AS TestSubCode\r\n" +
                      $"         , ltc.TestDisplayName, CONVERT(varchar, lrt.TestEndTime, 112) AS TestEndDate\r\n" +
                      $"         , CASE WHEN (LTRIM(RTRIM(ISNULL(lrr.TestResult02, ''))) = '') THEN LTRIM(RTRIM(ISNULL(lrr.TestResult01, '')))\r\n" +
                      $"                ELSE LTRIM(RTRIM(ISNULL(lrr.TestResult02,'')) + '('+LTRIM(RTRIM(ISNULL(lrr.TestResult01, ''))+')')) END AS TestResult\r\n" +
                      $"         , CASE WHEN ltc.IsTestSub = 1 THEN ''\r\n" +
                      $"                ELSE dbo.FUNC_TRANS_LABRESULT(lrt.LabRegDate, lrt.LabRegNo, lrc.TestModuleCode, lrt.TestCode, lrt.TestCode, ltc.IsTestHeader, lrr.TestResult01, lrr.TestResult02, lrr.TestResultText)\r\n" +
                      $"           END AS TestResultText\r\n" +
                      $"         , lrr.TestResultAbn\r\n" +
                      $"         , dbo.FUNC_TESTREF_UNIT(ltcoi.LabRegDate, lri.PatientAge, lri.PatientSex, lrr.TestSubCode, lrt.SampleCode) AS TestRefUnit\r\n" +
                      $"         , CASE WHEN ltc.ReportCode NOT IN ('04_1')" +
                      $"                THEN dbo.FUNC_TESTREF_TEXT_PlsLab(ltcoi.LabRegDate, lri.PatientAge, lri.PatientSex, lrr.TestSubCode, lrt.SampleCode)\r\n" +
                      $"           END AS TestReferValue\r\n" +
                      $"         , lri.PatientJuminNo01 + master.dbo.AES_DecryptFunc(lri.PatientJuminNo02, 'labge$%#!dleorms') AS JuminNo\r\n" +
                      $"         , ltc.IsTestHeader, lrc.TestModuleCode\r\n" +
                      $"         , CASE WHEN ltcoi.TestCode = '13630' AND (SELECT COUNT(*) FROM LabRegResult WHERE LabRegDate = ltcoi.LabRegDate AND LabRegNo = ltcoi.LabRegNo\r\n" +
                      $"                                                   AND TestSubCode IN('13630', '13928', '13929', '13930')) = 4 THEN '07'\r\n" +
                      $"                ELSE ltc.ReportCode\r\n" +
                      $"           END AS ReportCode\r\n" +
                      $"    FROM LabTransCompOrderInfo ltcoi\r\n" +
                      $"    JOIN LabRegInfo lri\r\n" +
                      $"    ON ltcoi.LabRegDate = lri.LabRegDate\r\n" +
                      $"    AND ltcoi.LabRegNo = lri.LabRegNo\r\n" +
                      $"    AND CenterCode = 'PlsLab'\r\n" +
                      $"    JOIN LabTestCode ltc\r\n" +
                      $"    ON ltc.TestCode = ltcoi.TestCode  \r\n" +
                      $"    JOIN LabReportCode lrc\r\n" +
                      $"    ON ltc.ReportCode = lrc.ReportCode\r\n" +
                      $"    JOIN LabRegResult lrr\r\n" +
                      $"    ON ltcoi.LabRegDate = lrr.LabRegDate\r\n" +
                      $"    AND ltcoi.LabRegNo = lrr.LabRegNo\r\n" +
                      $"    AND ltcoi.TestCode = lrr.TestSubCode\r\n" +
                      $"    JOIN LabRegTest lrt\r\n" +
                      $"    ON ltcoi.LabRegDate = lrt.LabRegDate\r\n" +
                      $"    AND ltcoi.LabRegNo = lrt.LabRegNo\r\n" +
                      $"    AND lrr.TestCode = lrt.TestCode\r\n" +
                      $"    AND lrt.IsTestTransEnd = {((resultSendState == "N") ? 0 : 1)}\r\n" +
                      $"    AND lrt.TestStateCode = 'F'\r\n" +
                      $"    JOIN LabRegReport lrrpt\r\n" +
                      $"    ON ltcoi.LabRegDate = lrrpt.LabRegDate\r\n" +
                      $"    AND ltcoi.LabRegNo = lrrpt.LabRegNo\r\n" +
                      $"    AND ltc.ReportCode = lrrpt.ReportCode\r\n";

                //결과 송신 상태를 조회할 때만 날짜 조건을 타도록 처리함(2021-06-29)
                if (resultSendState == "Y")
                {
                    if (searchKind == "E")
                        sql += $"    WHERE lrt.TestEndTime >= '{beginDate.ToString("yyyy-MM-dd")}' AND lrt.TestEndTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')\r\n";
                    else
                        sql += $"    WHERE lrt.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n";
                }

                sql += ") AS Sub1\r\n";
                sql += "ORDER BY LabRegDate, LabRegNo";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = 120;

                DataTable dt = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                adapter.Fill(dt);

                JArray array = new JArray();
                if (dt.Rows.Count > 0)
                {
                    array = JArray.Parse(JsonConvert.SerializeObject(dt));
                }
                
                //array = LabgeDatabase.SqlToJArray(sql);
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

        public IHttpActionResult PUT([FromBody]JObject request)
        {
            if (request == null)
            {
                throw new Exception("Request값이 null입니다.");
            }

            OracleConnection connPlsLab = new OracleConnection(plsLabConnString);
            try
            {
                connPlsLab.Open();

                string sql;

                sql = $"SELECT COUNT(*)\r\n" +
                      $"FROM MESDB.LisConMaster\r\n" +
                      $"WHERE reqdte = '{request["ReqDte"].ToString()}'\r\n" +
                      $"AND regno = '{request["Seq"].ToString()}'\r\n" +
                      $"AND reqno = '{request["SampleNo"].ToString()}'\r\n" +
                      $"AND itemcd = '{request["CstItemCd"].ToString()}'\r\n" +
                      $"AND patnm = '{request["PatNm"].ToString()}'\r\n" +
                      $"AND NVL(chartno, ' ') = '{(request["HosNo"].ToString() == string.Empty ? " " : request["HosNo"].ToString())}'\r\n" +
                      $"AND OutCom = '99908'";
                OracleCommand cmdPlsLab = connPlsLab.CreateCommand();
                cmdPlsLab.CommandText = sql;

                int dataExist = Convert.ToInt32(cmdPlsLab.ExecuteScalar());
                int scalarCount = 0;
                if (dataExist > 0)
                {
                    sql = $"UPDATE MESDB.LisConMaster\r\n" +
                          $"   SET result = '{(request["TestResult"].ToString() == string.Empty ? "*" : request["TestResult"].ToString()).Replace("**", "*")}'\r\n" +
                          $"     , result_text = '{request["TestResultText"].ToString().Replace("'", "''")}'\r\n" +
                          $"     , imagepath = '{(request["ImagePath"] ?? string.Empty).ToString()}'\r\n" +
                          $"     , compdate = TO_CHAR(SYSDATE, 'YYYYMMDD-HH24:MI:SS')\r\n" +
                          $"     , result_down = 'F'\r\n" +
                          $"     , decision = '{(request["Decision"] ?? string.Empty).ToString()}'\r\n" +
                          $"     , refervalue = '{(request["ReferValue"] ?? string.Empty).ToString()}'\r\n" +
                          $"WHERE reqdte = '{request["ReqDte"].ToString()}'\r\n" +
                          $"AND regno = '{request["Seq"].ToString()}'\r\n" +
                          $"AND reqno = '{request["SampleNo"].ToString()}'\r\n" +
                          $"AND itemcd = '{request["CstItemCd"].ToString()}'\r\n" +
                          $"AND OutCom = '99908'";

                    cmdPlsLab.CommandText = sql;
                    scalarCount = cmdPlsLab.ExecuteNonQuery();
                }

                RegistOrder registOrder = new RegistOrder();
                registOrder.UpdateLabTransCompOrderInfo(Convert.ToDateTime(request["LabRegDate"]), Convert.ToInt32(request["LabRegNo"]),
                                                        request["TestCode"].ToString(), request["TestSubCode"].ToString(), "Y", "PlsLab");
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
                connPlsLab.Close();
            }
        }
    }
}

//sql = "SELECT LabRegDate, LabRegNo, PatientChartNo, PatientName, CompOrderDate, CompOrderNo, CompSpcNo, CompTestCode, TestCode, TestDisplayName, TestEndDate\r\n" +
//      "     , CASE WHEN LEN(LTRIM(RTRIM(TestResult))) > 20 THEN '' ELSE TestResult END AS TestResult\r\n" +
//      "     , CASE\r\n" +
//      "           WHEN LEN(LTRIM(RTRIM(TestResult))) > 20 THEN TestResult + CHAR(13) + CHAR(10) + TestResultText\r\n" +
//      "           WHEN TestResult = TestResultText THEN ''\r\n" +
//      "           ELSE TestResultText\r\n" +
//      "       END TestResultText\r\n" +
//      "     , TestResultAbn, ReportCode, TestRefUnit, TestReferValue, JuminNo, IsTestHeader, TestModuleCode\r\n" +
//      "     , (SELECT LabRegReportID FROM LabRegReport WHERE Sub1.LabRegDate = LabRegDate AND Sub1.LabRegNo = LabRegNo AND Sub1.ReportCode = ReportCode) AS LabRegReportID\r\n" +
//      "FROM\r\n" +
//      "(\r\n" +
//      "    SELECT lrt.LabRegDate, lrt.LabRegNo, lri.PatientChartNo, lri.PatientName, CONVERT(varchar, ltcoi.CompOrderDate, 112) AS CompOrderDate\r\n" +
//      "         , ltcoi.CompOrderNo, ltcoi.CompSpcNo, ltcoi.CompTestCode , ltcoi.TestCode\r\n" +
//      "         , ltc.TestDisplayName, CONVERT(varchar, lrt.TestEndTime, 112) AS TestEndDate\r\n" +
//      "         , CASE WHEN (LTRIM(RTRIM(ISNULL(lrr.TestResult02, ''))) = '') THEN LTRIM(RTRIM(ISNULL(lrr.TestResult01, '')))\r\n" +
//      "                ELSE LTRIM(RTRIM(ISNULL(lrr.TestResult02,'')) + '('+LTRIM(RTRIM(ISNULL(lrr.TestResult01, ''))+')')) END AS TestResult\r\n" +
//      "         , CASE WHEN ltc.IsTestSub = 1 THEN ''\r\n" +
//      "                ELSE dbo.FUNC_TRANS_LABRESULT(lrt.LabRegDate, lrt.LabRegNo, lrc.TestModuleCode, lrt.TestCode, lrt.TestCode, ltc.IsTestHeader, lrr.TestResult01, lrr.TestResult02, lrr.TestResultText)\r\n" +
//      "           END AS TestResultText\r\n" +
//      "         , lrr.TestResultAbn\r\n" +
//      "         , dbo.FUNC_TESTREF_UNIT(ltcoi.LabRegDate, lri.PatientAge, lri.PatientSex, lrr.TestSubCode, lrt.SampleCode) AS TestRefUnit\r\n" +
//      "         , CASE WHEN ltc.ReportCode NOT IN ('04_1')" +
//      "                THEN dbo.FUNC_TESTREF_TEXT_PlsLab(ltcoi.LabRegDate, lri.PatientAge, lri.PatientSex, lrr.TestSubCode, lrt.SampleCode)\r\n" +
//      "           END AS TestReferValue\r\n" +
//      "         , lri.PatientJuminNo01 + master.dbo.AES_DecryptFunc(lri.PatientJuminNo02, 'labge$%#!dleorms') AS JuminNo\r\n" +
//      "         , ltc.IsTestHeader, lrc.TestModuleCode\r\n" +
//      "         , CASE WHEN ltcoi.TestCode = '13630' AND (SELECT COUNT(*) FROM LabRegResult WHERE LabRegDate = ltcoi.LabRegDate AND LabRegNo = ltcoi.LabRegNo\r\n" +
//      "                                                   AND TestSubCode IN('13630', '13928', '13929', '13930')) = 4 THEN '07'\r\n" +
//      "                ELSE ltc.ReportCode\r\n" +
//      "           END AS ReportCode\r\n" +
//      "    FROM LabTransCompOrderInfo ltcoi\r\n" +                  
//      "    JOIN LabRegInfo lri\r\n" +
//      "    ON ltcoi.LabRegDate = lri.LabRegDate\r\n" +
//      "    AND ltcoi.LabRegNo = lri.LabRegNo\r\n" +                  
//      "    AND CenterCode = 'PlsLab'\r\n" +
//      "    JOIN LabTestCode ltc\r\n" +
//      "    ON ltc.TestCode = ltcoi.TestCode  \r\n" +
//      "    JOIN LabReportCode lrc\r\n" +
//      "    ON (ltc.ReportCode = lrc.ReportCode)\r\n" +
//      "    JOIN LabRegResult lrr\r\n" +
//      "    ON ltcoi.LabRegDate = lrr.LabRegDate\r\n" +
//      "    AND ltcoi.LabRegNo = lrr.LabRegNo\r\n" +
//      "    AND ltcoi.TestCode = lrr.TestSubCode\r\n" +
//      "    JOIN LabRegTest lrt\r\n" +
//      "    ON ltcoi.LabRegDate = lrt.LabRegDate\r\n" +
//      "    AND ltcoi.LabRegNo = lrt.LabRegNo\r\n" +
//      "    AND lrr.TestCode = lrt.TestCode\r\n" +
//      "    AND ISNULL(lrt.TestEndTime, '') <> ''\r\n" +
//      "    JOIN LabRegReport lrrpt\r\n" +
//      "    ON ltcoi.LabRegDate = lrrpt.LabRegDate\r\n" +
//      "    AND ltcoi.LabRegNo = lrrpt.LabRegNo\r\n" +
//      "    AND ltc.ReportCode = lrrpt.ReportCode\r\n";

////결과 송신 상태를 조회할 때만 날짜 조건을 타도록 처리함(2021-06-29)
//if (resultSendState == "Y")
//{
//    if (searchKind == "E")
//        sql += $"    WHERE lrt.TestEndTime >= '{beginDate.ToString("yyyy-MM-dd")}' AND lrt.TestEndTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')\r\n";
//    else
//        sql += $"    WHERE lrt.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n";
//}