using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Common
{
    [SupportsAuth]
    [Route("api/Common/TestCodeInfo")]
    public class TestCodeInfoController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(string testCode)
        {
            string sql;
            sql = $"SELECT TOP 1 TestDisplayName, TestPrintName, TestShortName, SampleCode, IsTestUse\r\n" +
                  $"FROM LabTestCode\r\n" +
                  $"WHERE TestCode = '{testCode}'";

            JObject objResponse = LabgeDatabase.SqlToJObject(sql);
            return Ok(objResponse);
        }
    }
}