using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System.Web.Http;
using System.Web.Http.Cors;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [SupportsAuth]
    [EnableCors(origins: "*", headers: "*", methods: "*", PreflightMaxAge = 28800)]
    [Route("api/Sales/BizmentMapping")]
    public class BizmentMappingController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get()
        {
            string sql;
            sql = "SELECT BzCode, SBzCode, CodeKind, TestName, OrderCode, TestCode, SampleCode, SampleName\r\n" +
                  "     , CASE WHEN ISNULL(LabgeOrderName, '') <> '' THEN LabgeOrderName\r\n" +
                  "            ELSE LabgeTestName\r\n" +
                  "       END AS LabgeTestName\r\n" +
                  "FROM\r\n" +
                  "(\r\n" +
                  "    SELECT rtbc.*\r\n" +
                  "         , (SELECT TestDisplayName FROM LabTestCode WHERE rtbc.OrderCode = TestCode) AS LabgeOrderName\r\n" +
                  "         , (SELECT TestDisplayName FROM LabTestCode WHERE rtbc.TestCode = TestCode) AS LabgeTestName\r\n" +
                  "         , (SELECT SampleName FROM LabSampleCode lsc WHERE rtbc.SampleCode = SampleCode) AS SampleName\r\n" +
                  "    FROM RsltTransBizmentCode rtbc\r\n" +
                  ") AS Sub1\r\n" +
                  "ORDER BY BzCode, SBzCode";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        // POST api/<controller>
        public IHttpActionResult Post([FromBody]JObject request)
        {
            string sql;
            sql = $"INSERT INTO RsltTransBizmentCode\r\n" +
                  $"(BzCode, SBzCode, CodeKind, TestName, OrderCode, TestCode, SampleCode)\r\n" +
                  $"VALUES\r\n" +
                  $"(" +
                  $"    '{request["BzCode"].ToString()}', '{request["SBzCode"].ToString()}', '{request["CodeKind"].ToString()}', '{request["TestName"].ToString()}'\r\n" +
                  $"  , '{request["OrderCode"].ToString()}', '{request["TestCode"].ToString()}', '{request["SampleCode"].ToString()}'\r\n" +
                  $")";

            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        // PUT api/<controller>/5
        public IHttpActionResult Put([FromBody]JObject request)
        {
            string sql;
            sql = $"UPDATE RsltTransBizmentCode\r\n" +
                  $"SET OrderCode = '{request["OrderCode"].ToString()}'\r\n" +
                  $"  , TestCode = '{request["TestCode"].ToString()}'\r\n" +
                  $"  , TestName = '{request["TestName"].ToString()}'\r\n" +
                  $"  , SampleCode = '{request["SampleCode"].ToString()}'\r\n" +
                  $"WHERE BzCode = '{request["BzCode"].ToString()}'\r\n" +
                  $"AND SBzCode = '{request["SBzCode"].ToString()}'";

            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        // DELETE api/<controller>/5
        public IHttpActionResult Delete(string bzCode, string sBzCode)
        {
            string sql;
            sql = $"DELETE FROM RsltTransBizmentCode\r\n" +
                  $"WHERE BzCode = '{bzCode}'\r\n" +
                  $"AND SBzCode = '{sBzCode}'";

            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }
    }
}