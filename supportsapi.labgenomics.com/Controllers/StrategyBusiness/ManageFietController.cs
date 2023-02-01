using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.StrategyBusiness
{
    [SupportsAuth]
    [Route("api/StrategyBusiness/ManageFiet")]
    public class ManageFietController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string mode)
        {
            string sql;
            if (mode == "Ordered")
            {
                sql =
                    $"SELECT\r\n" +
                    $"    ppi.CompOrderDate, ppi.CompOrderNo, ppi.Gender, ppi.PatientRegNo, ppi.PatientName, ppi.ZipCode, ppi.Address, ppi.Address2, ppi.EmailAddress, \r\n" +
                    $"    ppi.PhoneNumber, ppi.AgreeRequestTest, ppi.AgreePrivacyPolicy, ppi.AgreeLabgePrivacyPolicy ,ppi.AgreePrivacyPolicyDateTime, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer, ppi.PrevTrackingNumber, ppi.PrevBarcode, ppi.TrackingNumber, ppi.ReshippedCode , ppi.Barcode, \r\n" +
                    $"    ppi.AgreeGeneThirdPartySensitive, ppi.AgreeKeepDataAndFutureAnalysis, ppi.OrderStatus, CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo\r\n" +
                    $"FROM PGSPatientInfo ppi\r\n" +
                    $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                    $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\r\n" +
                    $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\r\n" +
                    $"AND ltcoi.CompCode = ppi.CompCode\r\n" +
                    $"WHERE ppi.CustomerCode = 'fiet'\r\n" +
                    $"AND ppi.CompOrderDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                    $"AND (ppi.Server <> 'Develop' or ppi.Server is null)\r\n" +
                    $"AND (ppi.OrderStatus is null or ppi.OrderStatus = 'Ordered') ";
            }
            else 
            {
                sql =
                    $"SELECT\r\n" +
                    $"    ppi.CompOrderDate, ppi.CompOrderNo, ppi.Gender, ppi.PatientRegNo, ppi.PatientName, ppi.ZipCode, ppi.Address, ppi.Address2, ppi.EmailAddress, \r\n" +
                    $"    ppi.PhoneNumber, ppi.AgreeRequestTest, ppi.AgreePrivacyPolicy, ppi.AgreeLabgePrivacyPolicy ,ppi.AgreePrivacyPolicyDateTime, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer, ppi.PrevTrackingNumber, ppi.PrevBarcode, ppi.TrackingNumber, ppi.ReshippedCode , ppi.Barcode, \r\n" +
                    $"    ppi.AgreeGeneThirdPartySensitive, ppi.AgreeKeepDataAndFutureAnalysis, ppi.OrderStatus, CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo, CONVERT(varchar(19), lrr.ReportTransEndTime, 21) AS ReportTransEndTime, ISNULL(lrr.IsReportTransEnd, 0) as IsReportTransEnd\r\n" +
                    $"FROM PGSPatientInfo ppi\r\n" +
                    $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                    $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\r\n" +
                    $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\r\n" +
                    $"AND ltcoi.CompCode = ppi.CompCode\r\n" +
                    $"LEFT outer join LabRegReport lrr\n" +
                    $"ON lrr.LabRegDate = ltcoi.LabRegDate\n" +
                    $"AND lrr.LabRegNo  = ltcoi.LabRegNo\n" +
                    $"WHERE ppi.CustomerCode = 'fiet'\r\n" +
                    $"AND ppi.CompOrderDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                    $"AND (ppi.Server <> 'Develop' or ppi.Server is null)\r\n" +
                    $"AND ppi.OrderStatus != 'Ordered'";
            }

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        // POST api/<controller>
        public IHttpActionResult Post(JObject arrRequest)
        {

            return Ok();
        }

        // PUT api/<controller>/5
        public IHttpActionResult Put([FromBody]JArray request)
        {
            try
            {
                foreach (JObject objRequest in request)
                {
                    string sql;
                    sql = $"UPDATE PGSPatientInfo\r\n" +
                          $"SET AgreeRequestTest = '{objRequest["AgreeRequestTest"]}'\r\n" +
                          $"  , ZipCode = '{objRequest["ZipCode"]}'\r\n" +
                          $"  , Address = '{objRequest["Address"]}'\r\n" +
                          $"  , Address2 = '{objRequest["Address2"]}'\r\n" +
                          $"  , PatientRegNo = '{objRequest["PatientRegNo"]}'\r\n" +
                          $"  , BirthDay = '{objRequest["PatientRegNo"]}'\r\n" +
                          $"  , EmailAddress = '{objRequest["EmailAddress"]}'\r\n" +
                          $"  , PhoneNumber = '{objRequest["PhoneNumber"]}'\r\n" +
                          $"  , AgreeGeneTest = '{objRequest["agreeGeneTest"]}'\r\n" +
                          $"  , AgreeLabgePrivacyPolicy = '{objRequest["agreeLabgePrivacyPolicy"]}'\r\n" +
                          $"  , AgreeThirdPartyOffer = '{objRequest["agreeThirdPartyOffer"]}'\r\n" +
                          $"  , AgreeSendResultEmail = '{objRequest["agreeSendResultEmail"]}'\r\n" +
                          $"WHERE CompOrderDate = '{Convert.ToDateTime(objRequest["CompOrderDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                          $"AND CompOrderNo = '{objRequest["CompOrderNo"].ToString()}'\r\n"+
                          $"AND CustomerCode = 'fiet' ";
                    LabgeDatabase.ExecuteSql(sql);
                }
                return Ok();
            }

            catch (HttpException ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", ex.GetHttpCode());
                objResponse.Add("Message", ex.Message);
                HttpStatusCode code = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), ex.GetHttpCode().ToString());
                return Content((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), ex.GetHttpCode().ToString()), objResponse);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }

        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}