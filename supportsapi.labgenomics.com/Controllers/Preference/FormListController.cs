using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System.Data;
using System.Linq;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Preference
{
    [SupportsAuth]
    [Route("api/Preference/FormList")]
    public class FormListController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get()
        {
            string sql;
            sql = $"SELECT FormName\r\n" +
                  $"FROM SupportFormList\r\n" +
                  $"WHERE MenuDepth = 1\r\n" +
                  $"ORDER BY FormSequence";
            DataTable dtTitle = LabgeDatabase.SqlToDataTable(sql);

            sql = $"SELECT FormGroup, FormName, FormTitle, FormSequence\r\n" +
                  $"FROM SupportFormList\r\n" +
                  $"WHERE MenuDepth = 2\r\n" +
                  $"ORDER BY FormGroup, FormSequence";
            DataTable dtMenu = LabgeDatabase.SqlToDataTable(sql);

            var objResponse =
                from t1 in dtTitle.AsEnumerable()
                select new JObject(
                    new JProperty("MenuTitle", t1["FormName"]),
                    new JProperty("Forms",
                        new JArray(
                            from t2 in dtMenu.AsEnumerable()
                            where t2["FormGroup"].Equals(t1["FormName"])
                            select new JObject(
                                new JProperty("FormName", t2["FormName"]),
                                new JProperty("FormTitle", t2["FormTitle"]),
                                new JProperty("FormSequence", t2["FormSequence"])
                            )
                        )
                    )
                );

            return Ok(objResponse);
        }
    }
}