using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    //[SupportsAuth]
    [Route("api/Sales/Covid19InfoSMSForm")]
    public class Covid19SMSInfoController : ApiController
    {
        /// <summary>
        /// Get TEST
        /// </summary>
        /// <param name="startdatetime"></param>
        /// <param name="enddatetime"></param>
        /// <param name="healthcentercode"></param>
        /// <returns></returns>
        public IHttpActionResult Get(string startdatetime, string enddatetime, string[] healthcentercode)
        {

            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select ");
            sql.AppendLine("Covid19Order.LabRegDate , Covid19Order.IsSendSMS ,Covid19Order.SampleNo,  Covid19Order.CompInstitutionNo , Covid19Order.CompName  ,Covid19Order.BirthDay , Covid19Order.PatientName , Covid19Order.PhoneNo");
            sql.AppendLine("from Covid19Order");
            sql.Append($"where (LabRegDate BETWEEN'{startdatetime}' AND '{enddatetime}') ");
            sql.Append($"AND (CompInstitutionNo in(");
            for (int i = 0; i < healthcentercode.Length; i++)
            {
                sql.Append($"'{healthcentercode}'");
                if (i >= healthcentercode.Length - 1)
                {
                    sql.Append($", ");
                }
            }
            sql.Append($"))");
            sql.AppendLine($"order by LabRegDate, CompInstitutionNo");

            JObject objSMSInfo = LabgeDatabase.SqlToJObject(sql.ToString());

            return Ok(objSMSInfo);
        }


        /// <summary>
        /// Set NumberFlag
        /// </summary>
        /// <param name="objRequest"></param>
        /// <returns></returns>
        [Route("api/Sales/Covid19InfoSMSForm/Number")]
        public IHttpActionResult NumberPut([FromBody]JObject objRequest)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append($"update Covid19Order set IsSendSMS = '0' where SampleNo ='{objRequest["SampleNo"].ToString()}'");
                sql.Append($"and PatientName = '{objRequest["PatientName"].ToString()}' ");

                LabgeDatabase.ExecuteSql(sql.ToString());

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

        /// <summary>
        /// Set NumberFlag
        /// </summary>
        /// <param name="objRequest"></param>
        /// <returns></returns>
        [Route("api/Sales/Covid19InfoSMSForm/SMS")]
        public IHttpActionResult SMSPut([FromBody]JObject objRequest)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append($"update Covid19Order set IsSendSMS = '0' where SampleNo ='{objRequest["SampleNo"].ToString()}'");
                sql.Append($"and PatientName = '{objRequest["PatientName"].ToString()}' ");

                LabgeDatabase.ExecuteSql(sql.ToString());
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