using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [SupportsAuth]
    [EnableCors(origins: "*", headers: "*", methods: "*", PreflightMaxAge = 28800)]
    [Route("api/sales/banksaladorder")]
    public class BankSaladOrderController : ApiController
    {
        // PUT api/<controller>/5
        public IHttpActionResult Put(JObject objRequest)
        {
            try
            {
                string sql;
                sql =
                    $"UPDATE PGSPatientInfo\r\n" +
                    $"SET OrderStatus = '{objRequest["OrderStatus"]}'\r\n" +
                    $"WHERE CompOrderDate = '{Convert.ToDateTime(objRequest["CompOrderDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                    $"AND CompOrderNo = '{objRequest["CompOrderNo"]}'";
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