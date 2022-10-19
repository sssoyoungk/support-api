using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Common
{
    [SupportsAuth]
    [Route("api/Common/CompCodeInfo")]
    public class CompCodeInfoController : ApiController
    {
        public IHttpActionResult Get(string compCode = "")
        {
            string sql;
            sql = $"SELECT *\r\n" +
                  $"FROM ProgCompCode\r\n";
            if (compCode != "")
                sql += $"WHERE CompCode = '{compCode}'";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

            return Ok(arrResponse);
        }
    }
}