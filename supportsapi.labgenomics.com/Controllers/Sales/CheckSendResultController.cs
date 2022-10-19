using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/CheckSendResult")]
    public class CheckSendResultController : ApiController
    {
        // GET api/<controller>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="compCode">거래처코드</param>
        /// <param name="beginDate">시작일자</param>
        /// <param name="endCode">종료일자</param>
        /// <param name="transKind">연동차트</param>
        /// <param name="sendState">결과전송여부</param>        
        /// <returns></returns>
        public IHttpActionResult Get(string compCode, DateTime beginDate, DateTime endDate, string dateKind, string transKind, string sendState)
        {
            string sql;
            sql = $"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED\r\n" +
                      $"SELECT CONVERT(bit, 0) AS ColumnCheck, CompInstitutionNo, LabRegDate, LabRegNo, PatientChartNo, PatientName, CompOrderDate, CompOrderNo, CompSpcNo, CompTestCode\r\n" +
                      $"     , CompTestSubCode, TestCode, CompTestSampleCode, TestDisplayName, TestEndDate, TestResult\r\n" +
                      $"     , CASE WHEN TestResultText = TestResult THEN '' ELSE TestResultText END AS TestResultText\r\n" +
                      $"     , TestResultAbn, LabRegReportID, ReportCode, TestStartDate, TestRefUnit, TestReferValue, dpa_gb\r\n" +
                      $"     , JuminNo, PatientAge, PatientSex, SampleCode, CompName, CompExpansionField01, CompExpansionField02, ResultSendState, IsTestTransEnd\r\n" +
                      $"FROM\r\n" +
                      $"(\r\n" +
                      $"    SELECT pcc.CompInstitutionNo, lrr.LabRegDate, lrr.LabRegNo, lri.PatientChartNo, lri.PatientName, CONVERT(varchar, ltcoi.CompOrderDate, 112) AS CompOrderDate\r\n" +
                      $"         , ltcoi.CompOrderNo, ltcoi.CompSpcNo, ltcoi.CompTestCode, ltcoi.CompTestSubCode, ltcoi.TestCode, ltcoi.CompTestSampleCode\r\n" +
                      $"         , ltc.TestDisplayName, lrt.TestEndTime AS TestEndDate\r\n" +
                      $"         , CASE\r\n" +
                      $"               WHEN (LTRIM(RTRIM(ISNULL(lrr.TestResult02, '')))= '') THEN LTRIM(RTRIM(ISNULL(lrr.TestResult01, '')))\r\n" +
                      $"               ELSE LTRIM(RTRIM(ISNULL(lrr.TestResult02,''))+'('+LTRIM(RTRIM(ISNULL(lrr.TestResult01, ''))+')'))\r\n" +
                      $"           END AS TestResult\r\n" +
                      $"         , CASE WHEN ltc.IsTestSub = 1 THEN ''\r\n" +
                      $"                ELSE dbo.FUNC_TRANS_LABRESULT(ltcoi.LabRegDate, ltcoi.LabRegNo, lrc.TestModuleCode, lrt.TestCode, lrt.TestCode, ltc.IsTestHeader, lrr.TestResult01, lrr.TestResult02, lrr.TestResultText)\r\n" +
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
                      $"         , ltcoi.CompExpansionField01, ltcoi.CompExpansionField02, ltcoi.ResultSendState, lrt.IsTestTransEnd\r\n" +
                      $"    FROM LabRegResult lrr\r\n" +
                      $"    JOIN LabTransCompOrderInfo ltcoi\r\n" +
                      $"    ON lrr.LabRegDate = ltcoi.LabRegDate\r\n" +
                      $"    AND lrr.LabRegNo = ltcoi.LabRegNo\r\n" +
                      $"    AND lrr.TestSubCode = ltcoi.TestCode\r\n" +                      
                      $"    JOIN LabRegInfo lri\r\n" +
                      $"    ON lrr.LabRegDate = lri.LabRegDate\r\n" +
                      $"    AND lrr.LabRegNo = lri.LabRegNo\r\n" +
                      $"    AND lri.CenterCode = '{transKind}' \r\n" +
                      $"    JOIN LabRegTest lrt\r\n" +
                      $"    ON lrr.LabRegDate = lrt.LabRegDate\r\n" +
                      $"    AND lrr.LabRegNo = lrt.LabRegNo\r\n" +
                      $"    AND lrr.TestCode = lrt.TestCode\r\n" +
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
                      $"    WHERE lrt.TestStateCode = 'F'\r\n" +
                      $"    AND lri.CompCode = '{compCode}'\r\n";
            if (dateKind == "E")
                sql += $"    AND lrt.TestEndTime >= '{beginDate.ToString("yyyy-MM-dd")}' AND lrt.TestEndTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')\r\n";
            else
                sql += $"    AND lrr.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n";

            sql += $"    AND ltcoi.ResultSendState = '{sendState}'\r\n" +
                   $") AS Sub1\r\n";

            //속도를 위해 정렬을 쿼리문에서 하지 않는다.
            JArray array = LabgeDatabase.SqlToJArray(sql);
            return Ok(array);
        }

        public IHttpActionResult Put([FromBody]JObject request)
        {
            try
            {
                string sql;
                sql = $"UPDATE LabTransCompOrderInfo\r\n" +
                      $"SET ResultSendState = 'N'\r\n" +
                      $"WHERE LabRegDate = '{request["LabRegDate"].ToString()}'\r\n" +
                      $"AND LabRegNo = '{request["LabRegNo"].ToString()}'\r\n" +
                      $"AND TestCode = '{request["TestCode"].ToString()}'\r\n" +
                      $"UPDATE LabRegTest\r\n" +
                      $"SET IsTestTransEnd = 0\r\n" +
                      $"WHERE LabRegDate = '{request["LabRegDate"].ToString()}'\r\n" +
                      $"AND LabRegNo = '{request["LabRegNo"].ToString()}'\r\n" +
                      $"AND TestCode = '{request["TestCode"].ToString()}'\r\n";

                LabgeDatabase.ExecuteSql(sql);

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