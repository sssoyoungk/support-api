using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/sales/BrainResult")]
    public class BrainResultController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(string compCode, string centerCode, string sendKind, string dateKind, DateTime beginDate, DateTime endDate)
        {
            try
            {
                string sql;

                sql =
                  $"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED\r\n" +
                  $"SELECT lrr.LabRegDate, lrr.LabRegNo, lri.PatientChartNo, lri.PatientName, CONVERT(varchar, ltcoi.CompOrderDate, 112) AS CompOrderDate\r\n" +
                  $"     , ltcoi.CompOrderNo, ltcoi.CompSpcNo, ltcoi.CompTestCode, ltcoi.CompTestSubCode, ltcoi.TestCode\r\n" +
                  $"     , ltc.TestDisplayName, CONVERT(varchar, lrt.TestEndTime, 112) AS TestEndDate\r\n" +
                  $"     , CASE WHEN (LTRIM(RTRIM(ISNULL(lrr.TestResult02, '')))= '') THEN LTRIM(RTRIM(ISNULL(lrr.TestResult01, '')))\r\n" +
                  $"       ELSE LTRIM(RTRIM(ISNULL(lrr.TestResult02,''))+'('+LTRIM(RTRIM(ISNULL(lrr.TestResult01, ''))+')')) END AS TestResult\r\n" +
                  $"     , dbo.FUNC_TRANS_LABRESULT_BRAIN(ltcoi.LabRegDate, ltcoi.LabRegNo, lrc.TestModuleCode, lrt.TestCode, ltc.IsTestHeader, lrr.TestResultText) AS TestResultText\r\n" +
                  $"     , lrr.TestResultAbn, lrp.LabRegReportID, ltc.ReportCode, CONVERT(varchar, TestStartTime, 23) AS TestStartDate\r\n" +
                  $"     , dbo.FUNC_TESTREF_UNIT(lrr.LabRegDate, lri.PatientAge, lri.PatientSex, lrr.TestSubCode, lrt.SampleCode) AS TestRefUnit\r\n" +
                  $"     , dbo.FUNC_TESTREF_TEXT(lrr.LabRegDate, lri.PatientAge, lri.PatientSex, lrr.TestSubCode, lrt.SampleCode) AS TestReferValue\r\n" +
                  $"     , lri.PatientJuminNo01 + master.dbo.AES_DecryptFunc(lri.PatientJuminNo02, 'labge$%#!dleorms') AS JuminNo\r\n" +
                  $"     , lri.PatientAge, lri.PatientSex, lrt.SampleCode, ltcoi.CompTestSampleCode AS CompSampleCode\r\n" +
                  $"     , (SELECT SampleName FROM LabSampleCode WHERE lrt.SampleCode = SampleCode) AS SampleName\r\n" +
                  $"     , ltc.IsTestHeader, ltc.IsTestSub, lrc.TestModuleCode, pcc.CompName, pcc.CompCode, pcc.CompInstitutionNo\r\n" +
                  $"     , dbo.Func_GetReportUrl(lrr.LabRegDate, lrr.LabRegNo, lrr.TestCode) AS ReportURL \r\n" +
                  $"     , lrt.TestCode AS HeaderCode\r\n" +
                  $"FROM LabRegResult AS lrr \r\n" +
                  $"JOIN LabTransCompOrderInfo AS ltcoi \r\n" +
                  $"ON lrr.LabRegDate = ltcoi.LabRegDate \r\n" +
                  $"AND lrr.LabRegNo = ltcoi.LabRegNo \r\n" +
                  $"AND lrr.TestSubCode = ltcoi.TestCode \r\n" +
                  $"JOIN LabRegInfo AS lri \r\n" +
                  $"ON lrr.LabRegDate = lri.LabRegDate \r\n" +
                  $"AND lrr.LabRegNo = lri.LabRegNo \r\n" +
                  $"JOIN LabRegTest AS lrt \r\n" +
                  $"ON lrr.LabRegDate = lrt.LabRegDate \r\n" +
                  $"AND lrr.LabRegNo = lrt.LabRegNo \r\n" +
                  $"AND lrr.TestCode = lrt.TestCode \r\n" +
                  $"JOIN LabTestCode AS ltc \r\n" +
                  $"ON ltc.TestCode = ltcoi.TestCode \r\n" +
                  $"JOIN LabReportCode AS lrc \r\n" +
                  $"ON ltc.ReportCode = lrc.ReportCode \r\n" +
                  $"JOIN LabRegReport AS lrp \r\n" +
                  $"ON lrr.LabRegDate = lrp.LabRegDate \r\n" +
                  $"AND lrr.LabRegNo = lrp.LabRegNo  \r\n" +
                  $"AND ltc.ReportCode = lrp.ReportCode \r\n" +
                  $"JOIN ProgCompCode AS pcc\r\n" +
                  $"ON lri.CompCode = pcc.CompCode\r\n" +
                  $"WHERE lrt.TestStateCode = 'F'\r\n";


                if (dateKind == "E")
                {
                    sql += $"AND lri.LabRegDate >= convert(varchar(10), DATEADD(yy, -1, CURRENT_TIMESTAMP), 126) AND lri.LabRegDate <= convert(varchar(10), CURRENT_TIMESTAMP, 126)\r\n" +
                           $"AND lrt.TestEndTime >= '{beginDate.ToString("yyyy-MM-dd")}' AND lrt.TestEndTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')\r\n";
                }
                else
                {
                    sql += $"AND lrr.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n";
                }

                if (compCode != string.Empty)
                    sql += $"AND lri.CompCode = '{compCode}'\r\n";

                string isTestTransEnd = (sendKind == "N") ? "0" : "1";
                sql += $"AND (ltcoi.ResultSendState = '{sendKind}' OR lrt.IsTestTransEnd = {isTestTransEnd})\r\n" +
                       $"ORDER BY lrr.LabRegDate, lrr.LabRegNo";

                JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

                return Ok(arrResponse);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }

        [Route("api/sales/BrainResult/Interface")]
        public IHttpActionResult PostInterface([FromBody]JArray arrRequest)
        {

            foreach (JObject objOrder in arrRequest)
            {
                try
                {
                    string sql;
                    sql = $"INSERT INTO RsltTransBrainOrder \r\n" +
                          $" (InstitutionNo, CompCode, EXTRDD, CHARTNO, NM, RGSTNO, AGE, SEX, DEPT, DOCTOR, ROOM, EXAMCD, EXAMNM, CONTESTNUM, EXTRNO, SLIPCODE, RegistDateTime) \r\n" +
                          $" VALUES\r\n" +
                          $" ('{objOrder["InstitutionNo"].ToString()}', '{objOrder["CompCode"].ToString()}', '{objOrder["EXTRDD"].ToString()}', '{objOrder["CHARTNO"].ToString()}'\r\n" +
                          $" ,'{objOrder["NM"].ToString()}', '{objOrder["RGSTNO"].ToString()}', '{objOrder["AGE"].ToString()}', '{objOrder["SEX"].ToString()}', '{objOrder["DEPT"].ToString()}'\r\n" +
                          $" ,'{objOrder["DOCTOR"].ToString()}', '{objOrder["ROOM"].ToString()}', '{objOrder["EXAMCD"].ToString()}', '{objOrder["EXAMNM"].ToString()}', {objOrder["CONTESTNUM"].ToString()}\r\n" +
                          $" ,'{objOrder["EXTRNO"].ToString()}', '{objOrder["SLIPCODE"].ToString()}', getdate())";
                    LabgeDatabase.ExecuteSql(sql);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return Ok();
        }

        [Route("api/sales/BrainResult/Config")]
        public IHttpActionResult GetBrainConfig(string compCode)
        {
            string sql;
            sql =
                $"SELECT \r\n" +
                $"    rtcs.CompCode, pcc.CompName, pcc.CompInstitutionNo,\r\n" +
                $"    rtcs.ServerName, rtcs.DatabaseName, rtcs.LoginID, rtcs.Password, rtcs.UseOdbc\r\n" +
                $"FROM RsltTransCompSet rtcs\r\n" +
                $"JOIN ProgCompCode pcc\r\n" +
                $"ON rtcs.CompCode = pcc.CompCode\r\n" +
                $"WHERE rtcs.TransKind = 'Brain'\r\n" +
                $"AND rtcs.CompCode = '{compCode}'";
            JObject objResponse = LabgeDatabase.SqlToJObject(sql);
            return Ok(objResponse);
        }
    }
}