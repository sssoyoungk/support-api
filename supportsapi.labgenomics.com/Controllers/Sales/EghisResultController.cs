using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/EghisResult")]
    public class EghisResultController : ApiController
    {
        public IHttpActionResult Get(string centerCode, string sendKind, string dateKind, DateTime beginDate, DateTime endDate, string compCode = "")
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();
            try
            {
                string sql;

                sql = $"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED\r\n" +
                      $"SELECT CompInstitutionNo, LabRegDate, LabRegNo, PatientChartNo, PatientName, CompOrderDate, CompOrderNo, CompSpcNo, CompTestCode\r\n" +
                      $"     , CompTestSubCode, TestCode, TestSubCode, CompTestSampleCode, TestDisplayName, TestEndDate, TestResult\r\n" +
                      $"     , CASE WHEN TestResultText = TestResult THEN '' ELSE TestResultText END AS TestResultText\r\n" +
                      $"     , TestResultAbn, LabRegReportID, ReportCode, TestStartDate, TestRefUnit, TestReferValue, dpa_gb\r\n" +
                      $"     , JuminNo, PatientAge, PatientSex, SampleCode, CompName, CompExpansionField01, CompExpansionField02\r\n" +
                      $"FROM\r\n" +
                      $"(\r\n" +
                      $"    SELECT pcc.CompInstitutionNo, lrr.LabRegDate, lrr.LabRegNo, lri.PatientChartNo, lri.PatientName, CONVERT(varchar, ltcoi.CompOrderDate, 112) AS CompOrderDate\r\n" +
                      $"         , ltcoi.CompOrderNo, ltcoi.CompSpcNo, ltcoi.CompTestCode, ltcoi.CompTestSubCode, lrr.TestCode, lrr.TestSubCode, ltcoi.CompTestSampleCode\r\n" +
                      $"         , ltc.TestDisplayName, lrt.TestEndTime AS TestEndDate\r\n" +
                      $"         , CASE\r\n" +
                      $"               WHEN (LTRIM(RTRIM(ISNULL(lrr.TestResult02, '')))= '') THEN LTRIM(RTRIM(ISNULL(lrr.TestResult01, '')))\r\n" +
                      $"               ELSE LTRIM(RTRIM(ISNULL(lrr.TestResult02,''))+'('+LTRIM(RTRIM(ISNULL(lrr.TestResult01, ''))+')'))\r\n" +
                      $"           END AS TestResult\r\n" +
                      $"         , CASE WHEN ltc.IsTestSub = 1 THEN ''\r\n" +
                      $"                ELSE dbo.FUNC_TRANS_LABRESULT(ltcoi.LabRegDate, ltcoi.LabRegNo, lrc.TestModuleCode, lrt.TestCode, lrt.TestCode, ltc.IsTestHeader, lrr.TestResult01, lrr.TestResult02, lrr.TestResultText)\r\n" +
                      $"                   + CASE WHEN lrt.IsTestOutside = '0' AND ISNULL(lrt.DoctorCode, '') <> '' THEN\r\n" +
                      $"                     CHAR(13) + CHAR(10) + '판독의 : ' + ldc.DoctorPersonName + ' ' + ldc.DoctorLicenseKind + ', 면허번호 : ' + ldc.DoctorLicenseNo\r\n" +
                      $"                     ELSE '' END\r\n" +
                      $"           END AS TestResultText\r\n" +
                      $"         , lrr.TestResultAbn, lrrp.LabRegReportID, ltc.ReportCode, CONVERT(varchar, TestStartTime, 23) AS TestStartDate\r\n" +
                      $"         , dbo.FUNC_TESTREF_UNIT(lrr.LabRegDate, lri.PatientAge, lri.PatientSex, lrr.TestSubCode, lrt.SampleCode) AS TestRefUnit\r\n" +
                      $"         , dbo.FUNC_TESTREF_TEXT(lrr.LabRegDate, lri.PatientAge, lri.PatientSex, lrr.TestSubCode, lrt.SampleCode) AS TestReferValue\r\n" +
                      $"         , CASE WHEN lrr.IsTestResultDelta = 1 THEN 'D'\r\n" +
                      $"                WHEN lrr.IsTestResultPanic = 1 THEN 'P'\r\n" +
                      $"                ELSE ''\r\n" +
                      $"           END AS dpa_gb\r\n" +
                      $"         , lri.PatientJuminNo01 + master.dbo.AES_DecryptFunc(lri.PatientJuminNo02, 'labge$%#!dleorms') AS JuminNo\r\n" +
                      $"         , CONVERT(int, lri.PatientAge) AS PatientAge, lri.PatientSex, lrt.SampleCode\r\n" +
                      $"         , ltc.IsTestHeader, ltc.IsTestSub, lrc.TestModuleCode, pcc.CompName, pcc.CompCode\r\n" +
                      $"         , ltcoi.CompExpansionField01, ltcoi.CompExpansionField02\r\n" +
                      $"    FROM LabRegResult lrr\r\n" +
                      $"    JOIN LabTransCompOrderInfo ltcoi\r\n" +
                      $"    ON lrr.LabRegDate = ltcoi.LabRegDate\r\n" +
                      $"    AND lrr.LabRegNo = ltcoi.LabRegNo\r\n" +
                      $"    AND lrr.TestSubCode = ltcoi.TestCode\r\n" +
                      $"    JOIN LabRegInfo lri\r\n" +
                      $"    ON lrr.LabRegDate = lri.LabRegDate\r\n" +
                      $"    AND lrr.LabRegNo = lri.LabRegNo\r\n" +
                      $"    AND lri.IsTrustOrder = 1\r\n" +
                      $"    AND lri.CenterCode = '{centerCode}' \r\n" +
                      $"    JOIN LabRegTest lrt\r\n" +
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
                    sql += //$"    AND lrr.LabRegDate BETWEEN DATEADD(YEAR, -1, '{endDate.ToString("yyyy-MM-dd")}') AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                           $"    AND lrt.TestEndTime >= '{beginDate.ToString("yyyy-MM-dd")}' AND lrt.TestEndTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')\r\n";
                }
                else
                    sql += $"    AND lrr.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n";

                sql += ")AS Sub1";

                //속도를 위해 정렬을 쿼리문에서 하지 않는다.
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = 120;

                DataTable dt = new DataTable();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                dt.Load(dr);

                //DataTable dt = LabgeDatabase.SqlToDataTable(sql);
                dt.DefaultView.Sort = "LabRegDate, LabRegNo, TestCode";

                if (compCode != string.Empty)
                {
                    dt.DefaultView.RowFilter = $"CompCode = '{compCode}'";
                }

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

        public IHttpActionResult Post([FromBody]JObject request)
        {
            NpgsqlConnection eghisConn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["EghisConnection"].ConnectionString);
            try
            {
                eghisConn.Open();


                string sql = string.Empty;

                sql = $"SELECT COUNT(*)\r\n" +
                      $"FROM labge.interface_result\r\n" +
                      $"WHERE insucode = '{request["insucode"].ToString()}'\r\n" +
                      $"AND chart_no = '{request["chart_no"].ToString()}'\r\n" +
                      $"AND recept_no = '{request["recept_no"].ToString()}'\r\n" +
                      $"AND ord_ymd = '{request["ord_ymd"].ToString()}'\r\n" +
                      $"AND ord_no = '{request["ord_no"].ToString()}'\r\n" +
                      $"AND ord_seq_no = '{request["ord_seq_no"].ToString()}'\r\n" +
                      $"AND h_ord_cd = '{request["h_ord_cd"].ToString()}'\r\n" +
                      $"AND h_spccd = '{request["h_spccd"].ToString()}'";

                DataTable dtEghis = new DataTable();
                NpgsqlCommand eghisCmd = new NpgsqlCommand(sql, eghisConn);

                //쿼리를 실행해서 Count값을 받음.
                int rowCount = Convert.ToInt32(eghisCmd.ExecuteScalar());

                string result_nm = request["result_nm"].ToString();
                string result_txt = request["result_txt"].ToString();

                //단문 결과가 있으면 장문 결과를 넣지 않는다.
                //result_nm = request["result_nm"].ToString();
                //if (request["result_nm"].ToString() == string.Empty && request["result_txt"].ToString() != result_txt)
                //{
                //    result_txt = request["result_txt"].ToString();
                //}

                if (rowCount == 0)
                {
                    sql = $"INSERT INTO labge.interface_result\r\n" +
                          $"SELECT '{request["insucode"].ToString()}'\r\n" +
                          $"     , '{request["chart_no"].ToString()}'\r\n" +
                          $"     , '{request["recept_no"].ToString()}'\r\n" +
                          $"     , '{request["ord_ymd"].ToString()}'\r\n" +
                          $"     , {request["ord_no"].ToString()}\r\n" +
                          $"     , {request["ord_seq_no"].ToString()}\r\n" +
                          $"     , '{request["h_ord_cd"].ToString()}'\r\n" +
                          $"     , '{request["h_spccd"].ToString()}'\r\n" +
                          $"     , '{request["vfy_ymd"].ToString()}'\r\n" +
                          $"     , '{request["vfy_time"].ToString()}'\r\n" +
                          $"     , to_char(now(), 'YYYYMMDDHH24MISS')\r\n" + //cr_date
                          $"     , null\r\n" + //tr_date
                          $"     , null\r\n" + //ok_date
                          $"     , to_char(now(), 'YYYYMMDD')\r\n" + //ent_ymd
                          $"     , '{request["flag"].ToString()}'\r\n" +
                          $"     , '{result_nm}'\r\n" +
                          $"     , '{request["hl_gb"].ToString()}'\r\n" +
                          $"     , '{request["dpa_gb"].ToString()}'\r\n" +
                          $"     , '{request["reference"].ToString()}'\r\n" +
                          $"     , '{result_txt.Replace("'", "''")}'\r\n" +
                          $"     , null\r\n" +
                          $"     , '{request["unit"].ToString()}'";
                }
                else
                {
                    sql = $"UPDATE labge.interface_result\r\n" +
                          $"SET\r\n" +
                          $"    vfy_ymd = '{request["vfy_ymd"].ToString()}',\r\n" +
                          $"    vfy_time = '{request["vfy_time"].ToString()}',\r\n" +
                          $"    cr_date = to_char(now(), 'YYYYMMDDHH24MISS'),\r\n" +
                          $"    ent_ymd = to_char(now(), 'YYYYMMDD'),\r\n" +
                          $"    flag = '{request["flag"].ToString()}',\r\n" +
                          $"    result_nm = '{result_nm}',\r\n" +
                          $"    hl_gb = '{request["hl_gb"].ToString()}',\r\n" +
                          $"    dpa_gb = '{request["dpa_gb"].ToString()}',\r\n" +
                          $"    reference = '{request["reference"].ToString()}',\r\n" +
                          $"    result_txt = '{result_txt.Replace("'", "''")}',\r\n" +
                          $"    unit = '{request["unit"].ToString()}'\r\n" +
                          $"where insucode = '{request["insucode"].ToString()}'\r\n" +
                          $"AND chart_no = '{request["chart_no"].ToString()}'\r\n" +
                          $"AND recept_no = '{request["recept_no"].ToString()}'\r\n" +
                          $"AND ord_ymd = '{request["ord_ymd"].ToString()}'\r\n" +
                          $"AND ord_no = {request["ord_no"].ToString()}\r\n" +
                          $"AND ord_seq_no = {request["ord_seq_no"].ToString()}\r\n" +
                          $"AND h_ord_cd = '{request["h_ord_cd"].ToString()}'\r\n" +
                          $"AND h_spccd = '{request["h_spccd"].ToString()}'";
                }

                eghisCmd.CommandText = sql;
                eghisCmd.ExecuteNonQuery();

                //이지스 등록 완료되면 우리 서버에 전송완료 처리
                RegistOrder registOrder = new RegistOrder();
                registOrder.UpdateLabTransCompOrderInfo(Convert.ToDateTime(request["LabRegDate"]), Convert.ToInt32(request["LabRegNo"]),
                                                        request["TestCode"].ToString(), request["TestSubCode"].ToString(), "Y", "Eghis");


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
                eghisConn.Close();
            }
        }

    }
}