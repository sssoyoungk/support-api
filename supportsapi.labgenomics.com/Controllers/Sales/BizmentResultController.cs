using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/BizmentResult")]
    public class BizmentResultController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(string centerCode, string sendKind, string dateKind, DateTime beginDate, DateTime endDate)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();

            try
            {
                string sql;

                sql = $"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED\r\n" +
                      $"SELECT LabRegDate, LabRegNo, PatientChartNo, PatientName, CompOrderDate, CompOrderNo, CompSpcNo, CompTestCode, CompTestSubCode\r\n" +
                      $"     , TestCode, TestSubCode, TestDisplayName, TestEndDate, TestResult\r\n" +
                      $"     , CASE WHEN TestResultText = TestResult THEN '' ELSE TestResultText END AS TestResultText\r\n" +
                      $"     , TestResultAbn, LabRegReportID, ReportCode, TestStartDate, TestRefUnit, TestReferValue\r\n" +
                      $"     , JuminNo, PatientAge, PatientSex, SampleCode, CompSampleCode, IsTestHeader, IsTestSub, TestModuleCode, CompName, CompCode\r\n" +
                      $"     , CompInstitutionNo, ReportURL, PID, CID, HOSNUM, ADMOPD, SCODESEQ, CTEXT, BARCODE\r\n" +
                      $"FROM\r\n" +
                      $"(\r\n" +
                      $"    SELECT lrr.LabRegDate, lrr.LabRegNo, lri.PatientChartNo, lri.PatientName, CONVERT(varchar, ltcoi.CompOrderDate, 112) AS CompOrderDate\r\n" +
                      $"         , ltcoi.CompOrderNo, ltcoi.CompSpcNo, ltcoi.CompTestCode, ltcoi.CompTestSubCode, lrt.TestCode, lrr.TestSubCode\r\n" +
                      $"         , ltc.TestDisplayName, CONVERT(varchar(19), lrt.TestEndTime, 120) AS TestEndDate\r\n" +
                      $"         , CASE WHEN (LTRIM(RTRIM(ISNULL(lrr.TestResult02, '')))= '') THEN LTRIM(RTRIM(ISNULL(lrr.TestResult01, '')))\r\n" +
                      $"           ELSE LTRIM(RTRIM(ISNULL(lrr.TestResult02,''))+'('+LTRIM(RTRIM(ISNULL(lrr.TestResult01, ''))+')')) END AS TestResult \r\n" +
                      $"         , dbo.FUNC_TRANS_LABRESULT(ltcoi.LabRegDate, ltcoi.LabRegNo, lrc.TestModuleCode, lrt.TestCode, lrt.TestCode, ltc.IsTestHeader, lrr.TestResult01, lrr.TestResult02, lrr.TestResultText)\r\n" +
                      $"         + CASE WHEN lrt.IsTestOutside = '0' AND ISNULL(lrt.DoctorCode, '') <> '' THEN\r\n" +
                      $"                     CHAR(13) + CHAR(10) + '판독의 : ' + ldc.DoctorPersonName + ' ' + ldc.DoctorLicenseKind + ', 면허번호 : ' + ldc.DoctorLicenseNo\r\n" +
                      $"                ELSE '' END AS TestResultText\r\n" +
                      $"         , lrr.TestResultAbn, lrrp.LabRegReportID, ltc.ReportCode, CONVERT(varchar, TestStartTime, 23) AS TestStartDate\r\n" +
                      $"         , dbo.FUNC_TESTREF_UNIT(lrr.LabRegDate, lri.PatientAge, lri.PatientSex, lrr.TestSubCode, lrt.SampleCode) AS TestRefUnit\r\n" +
                      $"         , dbo.FUNC_TESTREF_TEXT(lrr.LabRegDate, lri.PatientAge, lri.PatientSex, lrr.TestSubCode, lrt.SampleCode) AS TestReferValue \r\n" +
                      $"         , lri.PatientJuminNo01 + master.dbo.AES_DecryptFunc(lri.PatientJuminNo02, 'labge$%#!dleorms') AS JuminNo\r\n" +
                      $"         , CONVERT(int, lri.PatientAge) AS PatientAge, lri.PatientSex, lrt.SampleCode, ltcoi.CompTestSampleCode AS CompSampleCode\r\n" +
                      $"         , ltc.IsTestHeader, ltc.IsTestSub, lrc.TestModuleCode, pcc.CompName, pcc.CompCode, pcc.CompInstitutionNo\r\n" +
                      $"         , dbo.Func_GetReportUrl(lrr.LabRegDate, lrr.LabRegNo, lrr.TestCode) AS ReportURL\r\n" +
                      $"         , PatientJuminNo01 + master.dbo.AES_DecryptFunc(PatientJuminNo02, N'labge$%#!dleorms') AS PID\r\n" +
                      $"         , ltcoi.CID, pcc.CompInstitutionNo AS HOSNUM , ltcoi.ADMOPD, ltcoi.CompSpcNo AS SCODESEQ, ltcoi.CTEXT, '' AS BARCODE\r\n" +
                      $"    FROM LabRegResult AS lrr\r\n" +
                      $"    JOIN LabTransCompOrderInfo AS ltcoi\r\n" +
                      $"    ON lrr.LabRegDate = ltcoi.LabRegDate\r\n" +
                      $"    AND lrr.LabRegNo = ltcoi.LabRegNo\r\n" +
                      $"    AND lrr.TestSubCode = ltcoi.TestCode\r\n" +
                      $"    JOIN LabRegInfo AS lri\r\n" +
                      $"    ON lrr.LabRegDate = lri.LabRegDate\r\n" +
                      $"    AND lrr.LabRegNo = lri.LabRegNo\r\n" +
                      $"    AND lri.CenterCode = '{centerCode}'\r\n" +
                      $"    JOIN LabRegTest AS lrt\r\n" +
                      $"    ON lrr.LabRegDate = lrt.LabRegDate\r\n" +
                      $"    AND lrr.LabRegNo = lrt.LabRegNo\r\n" +
                      $"    AND lrr.TestCode = lrt.TestCode\r\n" +
                      $"    AND lrt.IsTestTransEnd = {((sendKind == "N") ? 0 : 1)}\r\n" +
                      $"    JOIN LabTestCode ltc\r\n" +
                      $"    ON ltc.TestCode = ltcoi.TestCode\r\n" +
                      $"    JOIN LabReportCode lrc\r\n" +
                      $"    ON ltc.ReportCode = lrc.ReportCode\r\n" +
                      $"    JOIN LabRegReport lrrp\r\n" +
                      $"    ON lrr.LabRegDate = lrrp.LabRegDate\r\n" +
                      $"    AND lrr.LabRegNo = lrrp.LabRegNo \r\n" +
                      $"    AND ltc.ReportCode = lrrp.ReportCode\r\n" +
                      $"    JOIN ProgCompCode pcc\r\n" +
                      $"    ON lri.CompCode = pcc.CompCode\r\n" +
                      $"    LEFT OUTER JOIN LabDoctorCode AS ldc\r\n" +
                      $"    ON lrt.DoctorCode = ldc.DoctorCode\r\n" +
                      $"    WHERE lrt.TestStateCode = 'F'\r\n";

                if (dateKind == "E")
                {
                    sql += $"    AND lrr.LabRegDate BETWEEN DATEADD(YEAR, -1, '{endDate.ToString("yyyy-MM-dd")}') AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                           $"    AND lrt.TestEndTime >= '{beginDate.ToString("yyyy-MM-dd")}' AND lrt.TestEndTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')\r\n";
                }
                else
                    sql += $"    AND lrr.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n";

                sql += ") AS Sub1";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = 120;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable("TransResult");
                adapter.Fill(dt);

                //비즈먼트의 부속코드 처리
                DataTable subTotal = new DataTable("subTotal");
                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["IsTestHeader"]) == true && row["ReportCode"].ToString() == "01")
                    {
                        string age = row["PatientAge"].ToString();
                        string sex = row["PatientSex"].ToString();
                        string sampleCode = row["SampleCode"].ToString();
                        string subSql = $"SELECT A.LabRegDate, A.LabRegNo, '{row["PatientChartNo"].ToString()}' AS PatientChartNo, '{row["PatientName"].ToString()}' AS PatientName,  A.TestSubCode AS TestCode\r\n" +
                                        $"     , C.TestDisplayName\r\n" +
                                        $"     , CASE WHEN (LTRIM(RTRIM(ISNULL(A.TestResult02, '')))= '') THEN LTRIM(RTRIM(ISNULL(A.TestResult01, '')))\r\n" +
                                        $"       ELSE LTRIM(RTRIM(ISNULL(A.TestResult02,''))+'('+LTRIM(RTRIM(ISNULL(A.TestResult01, ''))+')')) END AS TestResult\r\n" +
                                        $"     , A.TestResultAbn\r\n" +
                                        $"     , B.BzCode AS CompTestCode, B.SBzCode AS CompTestSubCode\r\n" +
                                        $"     , dbo.FUNC_TESTREF_UNIT(A.LabRegDate, {age}, '{sex}', A.TestSubCode, '{sampleCode}') AS TestRefUnit\r\n" +
                                        $"     , dbo.FUNC_TESTREF_TEXT(A.LabRegDate, {age}, '{sex}', A.TestSubCode, '{sampleCode}') AS TestReferValue \r\n" +
                                        $"     , '{row["CompOrderDate"].ToString()}' AS CompOrderDate\r\n" +
                                        $"     , '{row["CompOrderNo"].ToString()}' AS CompOrderNo\r\n" +
                                        $"     , '{row["CompSpcNo"].ToString()}' AS CompSpcNo\r\n" +
                                        $"     , '{row["HOSNUM"].ToString()}' AS HOSNUM\r\n" +
                                        $"     , '{row["CID"].ToString()}' AS CID\r\n" +
                                        $"     , '{row["ADMOPD"].ToString()}' AS ADMOPD\r\n" +
                                        $"     , '{row["ReportURL"].ToString()}' AS ReportURL\r\n" +
                                        $"     , '{row["PID"].ToString()}' AS PID\r\n" +
                                        $"     , '{row["CompName"].ToString()}' AS CompName\r\n" +
                                        $"     , '{row["JuminNo"].ToString()}' AS JuminNo\r\n" +
                                        $"     , '{row["PatientSex"].ToString()}' AS PatientSex\r\n" +
                                        $"     , CONVERT(int, '{row["PatientAge"].ToString()}') AS PatientAge\r\n" +
                                        $"     , '{row["CTEXT"].ToString()}' AS CTEXT\r\n" +
                                        $"     , '{row["BARCODE"].ToString()}' AS BARCODE\r\n" +
                                        $"     , C.IsTestSub\r\n" +
                                        $"FROM LabRegResult A\r\n" +
                                        $"JOIN RsltTransBizmentCode B\r\n" +
                                        $"ON B.BzCode = '{row["CompTestCode"].ToString()}'\r\n" +
                                        $"AND B.SBzCode <> '00'\r\n" +
                                        $"AND A.TestSubCode = B.TestCode\r\n" +
                                        $"JOIN LabTestCode C\r\n" +
                                        $"ON C.TestCode = A.TestSubCode\r\n" +
                                        $"WHERE A.LabRegDate = '{Convert.ToDateTime(row["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                                        $"AND A.LabRegNo = {row["LabRegNo"].ToString()}";

                        SqlCommand cmdSubTable = new SqlCommand(subSql, conn);
                        SqlDataAdapter adapterSubTable = new SqlDataAdapter(cmdSubTable);
                        DataTable subTable = new DataTable("SubTable");
                        adapterSubTable.Fill(subTable);

                        subTotal.Merge(subTable);
                    }
                }
                dt.Merge(subTotal);

                dt.DefaultView.Sort = "LabRegDate, LabRegNo, TestCode";
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

        [Route("api/Sales/BizmentResult/Test")]
        public IHttpActionResult GetTestBizmentResult(string centerCode, string sendKind, string dateKind, DateTime beginDate, DateTime endDate)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();
            try
            {
                string sql;

                sql = $"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED\r\n" +
                      $"SELECT LabRegDate, LabRegNo, PatientChartNo, PatientName, CompOrderDate, CompOrderNo, CompSpcNo, CompTestCode, CompTestSubCode\r\n" +
                      $"     , TestCode, TestSubCode, TestDisplayName, TestEndDate, TestResult\r\n" +
                      $"     , CASE WHEN TestResultText = TestResult THEN '' ELSE TestResultText END AS TestResultText\r\n" +
                      $"     , TestResultAbn, LabRegReportID, ReportCode, TestStartDate, TestRefUnit, TestReferValue\r\n" +
                      $"     , JuminNo, PatientAge, PatientSex, SampleCode, CompSampleCode, IsTestHeader, IsTestSub, TestModuleCode, CompName, CompCode\r\n" +
                      $"     , CompInstitutionNo, ReportURL, PID, CID, HOSNUM, ADMOPD, SCODESEQ, CTEXT, BARCODE\r\n" +
                      $"FROM\r\n" +
                      $"(\r\n" +
                      $"    SELECT lrr.LabRegDate, lrr.LabRegNo, lri.PatientChartNo, lri.PatientName, CONVERT(varchar, ltcoi.CompOrderDate, 112) AS CompOrderDate\r\n" +
                      $"         , ltcoi.CompOrderNo, ltcoi.CompSpcNo, ltcoi.CompTestCode, ltcoi.CompTestSubCode, lrt.TestCode, lrr.TestSubCode\r\n" +
                      $"         , ltc.TestDisplayName, CONVERT(varchar, lrt.TestEndTime, 112) AS TestEndDate\r\n" +
                      $"         , CASE WHEN (LTRIM(RTRIM(ISNULL(lrr.TestResult02, '')))= '') THEN LTRIM(RTRIM(ISNULL(lrr.TestResult01, '')))\r\n" +
                      $"           ELSE LTRIM(RTRIM(ISNULL(lrr.TestResult02,''))+'('+LTRIM(RTRIM(ISNULL(lrr.TestResult01, ''))+')')) END AS TestResult \r\n" +
                      $"         , dbo.FUNC_TRANS_LABRESULT(ltcoi.LabRegDate, ltcoi.LabRegNo, lrc.TestModuleCode, lrt.TestCode, lrt.TestCode, ltc.IsTestHeader, lrr.TestResult01, lrr.TestResult02, lrr.TestResultText)\r\n" +
                      $"         + CASE WHEN lrt.IsTestOutside = '0' AND ISNULL(lrt.DoctorCode, '') <> '' THEN\r\n" +
                      $"                     CHAR(13) + CHAR(10) + '판독의 : ' + ldc.DoctorPersonName + ' ' + ldc.DoctorLicenseKind + ', 면허번호 : ' + ldc.DoctorLicenseNo\r\n" +
                      $"                ELSE '' END AS TestResultText\r\n" +
                      $"         , lrr.TestResultAbn, lrrp.LabRegReportID, ltc.ReportCode, CONVERT(varchar, TestStartTime, 23) AS TestStartDate\r\n" +
                      $"         , dbo.FUNC_TESTREF_UNIT(lrr.LabRegDate, lri.PatientAge, lri.PatientSex, lrr.TestSubCode, lrt.SampleCode) AS TestRefUnit\r\n" +
                      $"         , dbo.FUNC_TESTREF_TEXT(lrr.LabRegDate, lri.PatientAge, lri.PatientSex, lrr.TestSubCode, lrt.SampleCode) AS TestReferValue \r\n" +
                      $"         , lri.PatientJuminNo01 + master.dbo.AES_DecryptFunc(lri.PatientJuminNo02, 'labge$%#!dleorms') AS JuminNo\r\n" +
                      $"         , CONVERT(int, lri.PatientAge) AS PatientAge, lri.PatientSex, lrt.SampleCode, ltcoi.CompTestSampleCode AS CompSampleCode\r\n" +
                      $"         , ltc.IsTestHeader, ltc.IsTestSub, lrc.TestModuleCode, pcc.CompName, pcc.CompCode, pcc.CompInstitutionNo\r\n" +
                      $"         , dbo.Func_GetReportUrl(lrr.LabRegDate, lrr.LabRegNo, lrr.TestCode) AS ReportURL\r\n" +
                      $"         , master.dbo.AES_DecryptFunc(rtbo.PID, N'labge$%#!dleorms') COLLATE Korean_Wansung_CS_AS AS PID\r\n" +
                      $"         , rtbo.CID, rtbo.HOSNUM , rtbo.ADMOPD, rtbo.SCODESEQ, rtbo.CTEXT, rtbo.BARCODE\r\n" +
                      $"    FROM LabRegResult AS lrr\r\n" +
                      $"    JOIN LabTransCompOrderInfo AS ltcoi\r\n" +
                      $"    ON lrr.LabRegDate = ltcoi.LabRegDate\r\n" +
                      $"    AND lrr.LabRegNo = ltcoi.LabRegNo\r\n" +
                      $"    AND lrr.TestSubCode = ltcoi.TestCode\r\n" +
                      $"    JOIN LabRegInfo AS lri\r\n" +
                      $"    ON lrr.LabRegDate = lri.LabRegDate\r\n" +
                      $"    AND lrr.LabRegNo = lri.LabRegNo\r\n" +
                      $"    AND lri.CenterCode = '{centerCode}'\r\n" +
                      $"    JOIN LabRegTest AS lrt\r\n" +
                      $"    ON lrr.LabRegDate = lrt.LabRegDate\r\n" +
                      $"    AND lrr.LabRegNo = lrt.LabRegNo\r\n" +
                      $"    AND lrr.TestCode = lrt.TestCode\r\n" +
                      $"    AND lrt.IsTestTransEnd = {((sendKind == "N") ? 0 : 1)}" +
                      $"    JOIN LabTestCode ltc\r\n" +
                      $"    ON ltc.TestCode = ltcoi.TestCode\r\n" +
                      $"    JOIN LabReportCode lrc\r\n" +
                      $"    ON ltc.ReportCode = lrc.ReportCode\r\n" +
                      $"    JOIN LabRegReport lrrp\r\n" +
                      $"    ON lrr.LabRegDate = lrrp.LabRegDate\r\n" +
                      $"    AND lrr.LabRegNo = lrrp.LabRegNo \r\n" +
                      $"    AND ltc.ReportCode = lrrp.ReportCode\r\n" +
                      $"    JOIN ProgCompCode pcc\r\n" +
                      $"    ON lri.CompCode = pcc.CompCode\r\n" +
                      $"    JOIN RsltTransBizmentOrder rtbo\r\n" +
                      $"    ON ltcoi.CompCode = rtbo.CompCode\r\n" +
                      $"    AND ltcoi.CompOrderDate = rtbo.WNO\r\n" +
                      $"    AND ltcoi.CompOrderNo = rtbo.ONO\r\n" +
                      $"    AND lri.PatientChartNo = rtbo.CNO\r\n" +
                      $"    AND ltcoi.CompSpcNo = rtbo.SCODESEQ\r\n" +
                      $"    AND ltcoi.CompTestCode = rtbo.BZCODE\r\n" +
                      $"    AND ltcoi.CompTestSubCode = rtbo.SBZCODE\r\n" +
                      $"    LEFT OUTER JOIN LabDoctorCode AS ldc\r\n" +
                      $"    ON lrt.DoctorCode = ldc.DoctorCode\r\n" +
                      $"    WHERE lrt.TestStateCode = 'F'\r\n";

                if (dateKind == "E")
                {
                    sql += $"    AND lrr.LabRegDate BETWEEN DATEADD(YEAR, -1, '{endDate.ToString("yyyy-MM-dd")}') AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                           $"    AND lrt.TestEndTime >= '{beginDate.ToString("yyyy-MM-dd")}' AND lrt.TestEndTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')\r\n";
                }
                else
                    sql += $"    AND lrr.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n";

                string isTestTransEnd = (sendKind == "N") ? "0" : "1";
                sql += $"    AND ltcoi.ResultSendState = '{sendKind}'\r\n" +
                       $") AS Sub1";

                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable("TransResult");
                adapter.Fill(dt);

                //비즈먼트의 부속코드 처리
                DataTable subTotal = new DataTable("subTotal");
                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["IsTestHeader"]) == true && row["ReportCode"].ToString() == "01")
                    {
                        string age = row["PatientAge"].ToString();
                        string sex = row["PatientSex"].ToString();
                        string sampleCode = row["SampleCode"].ToString();
                        string subSql = $"SELECT A.LabRegDate, A.LabRegNo, '{row["PatientChartNo"].ToString()}' AS PatientChartNo, '{row["PatientName"].ToString()}' AS PatientName,  A.TestSubCode AS TestCode\r\n" +
                                        $"     , C.TestDisplayName\r\n" +
                                        $"     , CASE WHEN (LTRIM(RTRIM(ISNULL(A.TestResult02, '')))= '') THEN LTRIM(RTRIM(ISNULL(A.TestResult01, '')))\r\n" +
                                        $"       ELSE LTRIM(RTRIM(ISNULL(A.TestResult02,''))+'('+LTRIM(RTRIM(ISNULL(A.TestResult01, ''))+')')) END AS TestResult\r\n" +
                                        $"     , A.TestResultAbn\r\n" +
                                        $"     , B.BzCode AS CompTestCode, B.SBzCode AS CompTestSubCode\r\n" +
                                        $"     , dbo.FUNC_TESTREF_UNIT(A.LabRegDate, {age}, '{sex}', A.TestSubCode, '{sampleCode}') AS TestRefUnit\r\n" +
                                        $"     , dbo.FUNC_TESTREF_TEXT(A.LabRegDate, {age}, '{sex}', A.TestSubCode, '{sampleCode}') AS TestReferValue \r\n" +
                                        $"     , '{row["CompOrderDate"].ToString()}' AS CompOrderDate\r\n" +
                                        $"     , '{row["CompOrderNo"].ToString()}' AS CompOrderNo\r\n" +
                                        $"     , '{row["CompSpcNo"].ToString()}' AS CompSpcNo\r\n" +
                                        $"     , '{row["HOSNUM"].ToString()}' AS HOSNUM\r\n" +
                                        $"     , '{row["CID"].ToString()}' AS CID\r\n" +
                                        $"     , {row["ADMOPD"].ToString()} AS ADMOPD\r\n" +
                                        $"     , '{row["ReportURL"].ToString()}' AS ReportURL\r\n" +
                                        $"     , '{row["PID"].ToString()}' AS PID\r\n" +
                                        $"     , '{row["CompName"].ToString()}' AS CompName\r\n" +
                                        $"     , '{row["JuminNo"].ToString()}' AS JuminNo\r\n" +
                                        $"     , '{row["PatientSex"].ToString()}' AS PatientSex\r\n" +
                                        $"     , CONVERT(int, '{row["PatientAge"].ToString()}') AS PatientAge\r\n" +
                                        $"     , '{row["CTEXT"].ToString()}' AS CTEXT\r\n" +
                                        $"     , '{row["BARCODE"].ToString()}' AS BARCODE\r\n" +
                                        $"     , C.IsTestSub\r\n" +
                                        $"FROM LabRegResult A\r\n" +
                                        $"JOIN RsltTransBizmentCode B\r\n" +
                                        $"ON B.BzCode = '{row["CompTestCode"].ToString()}'\r\n" +
                                        $"AND B.SBzCode <> '00'\r\n" +
                                        $"AND A.TestSubCode = B.TestCode\r\n" +
                                        $"JOIN LabTestCode C\r\n" +
                                        $"ON C.TestCode = A.TestSubCode\r\n" +
                                        $"WHERE A.LabRegDate = '{Convert.ToDateTime(row["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                                        $"AND A.LabRegNo = {row["LabRegNo"].ToString()}";

                        SqlCommand cmdSubTable = new SqlCommand(subSql, conn);
                        SqlDataAdapter adapterSubTable = new SqlDataAdapter(cmdSubTable);
                        DataTable subTable = new DataTable("SubTable");
                        adapterSubTable.Fill(subTable);

                        subTotal.Merge(subTable);
                    }
                }
                dt.Merge(subTotal);

                dt.DefaultView.Sort = "LabRegDate, LabRegNo, TestCode";
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
    }
}