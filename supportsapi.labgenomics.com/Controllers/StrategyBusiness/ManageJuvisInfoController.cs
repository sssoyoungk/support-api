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
    [Route("api/StrategyBusiness/ManageJuvisInfo")]
    public class ManageJuvisInfoController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate)
        {
            string sql;

            sql = $"SELECT CONVERT(varchar, ppi.CompOrderDate, 23) AS CompOrderDate, ppi.CompOrderNo, ppi.PatientName, ppi.ChartNo, ppi.BirthDay, ppi.Age, ppi.Gender\r\n" +
                  $"     , ppi.AgreeRequestTest, ppi.AgreeLabgePrivacyPolicy, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer, ppi.AgreeSendResultEmail\r\n" +
                  $"     , ppi.EmailAddress, ppi.LabgeEmailAddress, ppi.PhoneNumber, ppi.PatientRegNo\r\n" +
                  $"     , ppi.CheckSendResultEmail, CONVERT(varchar(19), ppi.EmailSendDateTime, 21) AS EmailSendDateTime\r\n" +
                  $"     , CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo\r\n" +
                  $"     , CONVERT(varchar(19), lrr.ReportEndTime, 21) AS ReportEndTime\r\n" +
                  $"     , ppi.CompCode, pcc.CompName\r\n" +
                  $"FROM PGSPatientInfo ppi\r\n" +
                  $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                  $"ON ppi.CompOrderDate = ltcoi.CompOrderDate\r\n" +
                  $"AND ppi.CompOrderNo = ltcoi.CompOrderNo\r\n" +
                  $"JOIN ProgCompCode pcc\r\n" +
                  $"ON ppi.CompCode = pcc.CompCode\r\n" +
                  $"LEFT OUTER JOIN LabRegReport lrr\r\n" +
                  $"ON ltcoi.LabRegDate = lrr.LabRegDate\r\n" +
                  $"AND ltcoi.LabRegNo = lrr.LabRegNo\r\n" +
                  $"WHERE ppi.CompOrderDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                  $"AND ppi.CustomerCode = 'Juvis'";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
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
                          $"SET LabgeEmailAddress = '{objRequest["LabgeEmailAddress"].ToString()}'\r\n" +
                          $"  , AgreeRequestTest = '{objRequest["AgreeRequestTest"]}'\r\n" +
                          $"  , AgreeGeneTest = '{objRequest["AgreeGeneTest"]}'\r\n" +
                          $"  , AgreeLabgePrivacyPolicy = '{objRequest["AgreeLabgePrivacyPolicy"]}'\r\n" +
                          $"  , AgreeThirdPartyOffer = '{objRequest["AgreeThirdPartyOffer"]}'\r\n" +
                          $"  , AgreeSendResultEmail = '{objRequest["AgreeSendResultEmail"]}'\r\n" +
                          $"WHERE CompOrderDate = '{Convert.ToDateTime(objRequest["CompOrderDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                          $"AND CompOrderNo = '{objRequest["CompOrderNo"].ToString()}'";
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

        public IHttpActionResult Delete(DateTime compOrderDate, string compOrderNo)
        {
            try
            {
                string sql;
                sql = $"DELETE FROM PGSPatientInfo\r\n" +
                      $"WHERE CompOrderDate = '{compOrderDate.ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND CompOrderNo = '{compOrderNo}'\r\n" +
                      $"AND NOT EXISTS \r\n" + 
                      $"(\r\n" +
                      $"    SELECT NULL\r\n" +
                      $"    FROM LabTransCompOrderInfo ltcoi\r\n" +
                      $"    WHERE ltcoi.CompOrderDate = PGSPatientInfo.CompOrderDate\r\n" +
                      $"    AND ltcoi.CompOrderNo = PGSPatientInfo.CompOrderNo\r\n" +
                      $")\r\n" +
                      $"DELETE FROM PGSTestInfo\r\n" +
                      $"WHERE CompOrderDate = '{compOrderDate.ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND CompOrderNo = '{compOrderNo}'\r\n" +
                      $"AND NOT EXISTS \r\n" + 
                      $"(\r\n" +
                      $"    SELECT NULL\r\n" +
                      $"    FROM LabTransCompOrderInfo ltcoi\r\n" +
                      $"    WHERE ltcoi.CompOrderDate = PGSTestInfo.CompOrderDate\r\n" +
                      $"    AND ltcoi.CompOrderNo = PGSTestInfo.CompOrderNo\r\n" +
                      $")\r\n";

                int execCount =LabgeDatabase.ExecuteSql(sql);

                if (execCount == 0)
                {
                    throw new HttpException(404, "자료가 없거나 접수된 상태입니다.");
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
    }
}