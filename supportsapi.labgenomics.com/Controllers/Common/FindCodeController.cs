using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Common
{
    [SupportsAuth]
    [Route("api/Common/FindCode")]
    public class FindCodeController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult GetFindCode(string findKind, string whereSql = "")
        {
            string sql = string.Empty;

            //거래처 코드
            if (findKind == "CompCode")
            {
                sql = "SELECT CompCode AS Code, CompName AS Name\r\n" +
                      "FROM ProgCompCode\r\n";
            }
            //서포트 폼 리스트
            else if (findKind == "SupportForm")
            {
                sql = "SELECT FormName AS Code, FormTitle AS Name\r\n" +
                      "FROM SupportFormList\r\n";
            }
            //검사코드
            else if (findKind == "TestCode")
            {
                sql = "SELECT TestCode AS Code, TestDisplayName AS Name\r\n " +
                      "FROM LabTestCode\r\n";
            }
            //레포트코드
            else if (findKind == "ReportCode")
            {
                sql = "SELECT ReportCode AS Code, ReportName AS Name\r\n" +
                      "FROM LabReportCode\r\n";
            }
            else if (findKind == "SampleCode")
            {
                sql = "SELECT SampleCode, SampleName\r\n" +
                      "FROM LabSampleCode\r\n";
            }
            sql += whereSql;

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }
    }
}