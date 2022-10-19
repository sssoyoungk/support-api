using supportsapi.labgenomics.com.Services;
using Newtonsoft.Json.Linq;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers
{
    [Route("api/Sales/PatientAddress")]
    public class PatientAddressController : ApiController
    {
        public JArray Get(string compCode, string beginDate, string endDate)
        {
            string sql;
            sql = $"SELECT lri.PatientChartNo, '님 귀하' AS Honor, lri.PatientName, lrc.ReportName, lri.PatientZipCode\r\n" +
                  $"     , ISNULL(lri.PatientAddress01, '') + ISNULL(lri.PatientAddress02, '') AS PatientAddress\r\n" +
                  $"FROM LabRegInfo lri\r\n" +
                  $"JOIN LabRegReport lrr\r\n" +
                  $"ON lri.LabRegDate = lrr.LabRegDate\r\n" +
                  $"AND lri.LabRegNo = lrr.LabRegNo\r\n" +
                  $"AND lrr.ReportCode IN ('19', 'LC006', '37', '85', '43', 'LC008')\r\n" +
                  $"JOIN LabReportCode lrc\r\n" +
                  $"ON lrc.ReportCode = lrr.ReportCode\r\n" +
                  $"WHERE lri.CompCode = '{compCode}'\r\n" +
                  $"AND lri.LabRegDate BETWEEN '{beginDate}' AND '{endDate}'\r\n" +
                  $"AND ISNULL(lri.PatientAddress01, '') + ISNULL(lri.PatientAddress02, '') <> ''\r\n" +
                  $"ORDER BY lri.LabRegDate";
            
            JArray array = LabgeDatabase.SqlToJArray(sql);

            return array;
        }
    }
}