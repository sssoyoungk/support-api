using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using System;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    [Route("api/Diagnostic/SendSMSTestResult")]
    public class SendSMSTestResultController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string authGroupCode)
        {
            string sql;
            sql = $"SELECT CompName, LabRegDate, LabRegNo, ReportCode, ReportName, PatientName\r\n" +
                  $"     , ReportResult, Abnormal\r\n" +
                  $"     , CASE WHEN Abnormal = '*' THEN AbnormalMessage ELSE NormalMessage END AS Message\r\n" +
                  $"     , DATALENGTH(CASE WHEN Abnormal = '*' THEN AbnormalMessage ELSE NormalMessage END) AS MessageLength\r\n" +
                  $"     , CompPhoneNo, PatientPhoneNo, ReportBgColor, '' AS SendResult\r\n" +
                  $"FROM(\r\n" +
                  $"    SELECT pcc.CompName, lrr.LabRegDate, lrr.LabRegNo, lrr.ReportCode, lrc.ReportName, lri.PatientName\r\n" +
                  $"         , REPLACE(REPLACE(ssd.NormalMessage, '{{수진자명}}', lri.PatientName), '{{거래처명}}', CompName) AS NormalMessage\r\n" +
                  $"         , REPLACE(REPLACE(ssd.AbnormalMessage, '{{수진자명}}', lri.PatientName), '{{거래처명}}', CompName) AS AbnormalMessage\r\n" +
                  $"         , reportResult.ReportResult, reportResult.Abnormal, pcc.CompPhoneNo, lri.PatientPhoneNo, lrc.ReportBgColor\r\n" +
                  $"    FROM LabRegReport lrr\r\n" +
                  $"    JOIN LabRegInfo lri\r\n" +
                  $"    ON lrr.LabRegDate = lri.LabRegDate\r\n" +
                  $"    AND lrr.LabRegNo = lri.LabRegNo\r\n" +
                  $"    JOIN SupportSendSMSSetData ssd\r\n" +
                  $"    ON lri.CompCode = ssd.CompCode\r\n" +
                  $"    AND lrr.ReportCode = ssd.ReportCode\r\n" +
                  $"    JOIN LabReportCode lrc\r\n" +
                  $"    ON lrr.ReportCode = lrc.ReportCode\r\n" +
                  $"    JOIN ProgCompCode pcc\r\n" +
                  $"    ON pcc.CompCode = lri.CompCode\r\n" +
                  $"    OUTER APPLY dbo.FN_CheckAbnormalResultByReportCode(lrr.LabRegDate, lrr.LabRegNo, lrr.ReportCode) reportResult\r\n" +
                  $"    WHERE lrr.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                  $"    AND lrr.ReportStateCode = 'F'\r\n" +
                  $"    AND lri.CompCode IN(SELECT CompCode\r\n" +
                  $"                        FROM ProgAuthGroupAccessComp\r\n" +
                  $"                        WHERE AuthGroupCode = '{authGroupCode}')\r\n" +
                  $"    AND NOT EXISTS(SELECT NULL\r\n" +
                  $"                    FROM SupportSMSSendCheck ssc\r\n" +
                  $"                    WHERE ssc.LabRegDate = lrr.LabRegDate\r\n" +
                  $"                    AND ssc.LabRegNo = lrr.LabRegNo\r\n" +
                  $"                    AND ssc.ReportCode = lrr.ReportCode)\r\n" +
                  $") AS Grp";

            JArray array = Services.LabgeDatabase.SqlToJArray(sql);

            return Ok(array);
        }

        public IHttpActionResult Post([FromBody]JObject requestParameter)
        {
            string sql;
            sql = $"INSERT INTO SupportSMSSendCheck\r\n" +
                  $"(LabRegDate, LabRegNo, ReportCode, Message)\r\n" +
                  $"VALUES\r\n" +
                  $"('{requestParameter["LabRegDate"].ToString()}'\r\n" +
                  $",'{requestParameter["LabRegNo"].ToString()}'\r\n" +
                  $",'{requestParameter["ReportCode"].ToString()}'\r\n" +
                  $",'{requestParameter["Message"].ToString()}'";
            Services.LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }
    }
}