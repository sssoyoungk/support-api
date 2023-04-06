using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Preference
{
    [Route("api/Preference/ScreenSaver")]
    public class ScreenSaverController : ApiController
    {
        // GET api/<controller>
        [Route("api/Preference/ScreenSaver/adress")]
        public IHttpActionResult Get(string address)
        {
            string sql;
            sql =
                $"SELECT *\r\n" +
                $"FROM LabgeIPAddress\r\n" +
                $"WHERE IPAddresss = '{address}'";
            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/Preference/ScreenSaver")]
        public IHttpActionResult Get()
        {
            string sql;
            sql =
                "SELECT TOP 1 *\r\n" +
                "FROM LabgeScreenSaver\r\n" +  
                "ORDER BY EditTime DESC";
            JObject objResponse = LabgeDatabase.SqlToJObject(sql);
            return Ok(objResponse);
        }
    }
}