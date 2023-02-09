using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.StrategyBusiness
{
    [SupportsAuth]
    [Route("api/StrategyBusiness/ManageDBProject")]
    public class ManageDBProjectInfoController : ApiController
    {
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate)
        {
#if DEBUG
            string infoTable = "PGSPatientInfo";

#else
            string infoTable = "PGSPatientInfo";
#endif

            string sql;
            sql =
                $"SELECT\r\n" +
                $"    ppi.CompOrderDate, ppi.CompOrderNo, ppi.Gender, ppi.Race, ppi.BirthDay, ppi.PatientName, ppi.ZipCode, ppi.Address, ppi.Address2, ppi.EmailAddress, \r\n" +
                $"    ppi.PhoneNumber, ppi.AgreePrivacyPolicyDateTime, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer, ppi.AgreeThirdPartySensitive, \r\n" +
                $"    ppi.AgreeGeneThirdPartySensitive, ppi.AgreeKeepDataAndFutureAnalysis,  CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo, lrr.ReportTransEndTime, \r\n" +
                $"    ppi.CompCode, pcc.CompName \r\n" +
                $"FROM {infoTable} ppi\r\n" +
                $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\r\n" +
                $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\r\n" +
                $"AND ltcoi.CompCode = ppi.CompCode\r\n" +
                $"JOIN ProgCompCode pcc \r\n" +
                $"ON ppi.CompCode  = pcc.CompCode \r\n" +
                $"LEFT OUTER JOIN LabRegReport lrr\r\n" +
                $"ON ltcoi.LabRegDate = lrr.LabRegDate\r\n" +
                $"AND ltcoi.LabRegNo = lrr.LabRegNo\r\n" +
                $"WHERE ppi.CustomerCode = 'GenoCore'\r\n" +
                $"AND ppi.CompOrderDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }




        public IHttpActionResult Put([FromBody]JObject request)
        {
            //업데이트

            string tokenKey = string.Empty;
            try
            {
#if DEBUG
                string apiUrl = "http://localhost:34306/api/Common/GenoCoreAuthority";
                //string apiUrl = "https://supportsapi.labgenomics.com/api/Common/GenoCoreAuthority";
#else
                string apiUrl = "https://supportsapi.labgenomics.com/api/Common/GenoCoreAuthority";
#endif


                var client = new RestClient($"{apiUrl}");
                client.Timeout = -1;
                var requestKey = new RestRequest(Method.GET);
                requestKey.AddHeader("Content-Type", "application/json");
                //requestKey.AddCookie("Cookie", "PHPSESSID=1ffokj5o56bqtahj5qq3p8hde2"); //저번에는 꼭 사용하여야했음. 혹시 몰라 추가

                JObject jobj = new JObject();
                jobj.Add("x-id", "labgenomics");
                jobj.Add("x-pw", "Foawlsh#226@");

                IRestResponse response = client.Execute(requestKey);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var recvObj = JsonConvert.DeserializeObject<JObject>(response.Content);
                    tokenKey = recvObj["authToken"].ToString();
                }
                else
                {
                    var recvObj = JsonConvert.DeserializeObject<JObject>(response.Content);
                    throw new HttpException($"{recvObj["message"].ToString()}");
                }
            }
            catch (HttpException ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", ex.GetHttpCode());
                objResponse.Add("Message", ex.Message);
                HttpStatusCode code = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), ex.GetHttpCode().ToString());
                return Content((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), ex.GetHttpCode().ToString()), objResponse);
            }

            try
            {
                string sql;

                sql = $"UPDATE PGSPatientInfo\r\n" +
                      $" SET EmailAddress = '{request["EmailAddress"]}' \r\n" +
                      $"WHERE CompOrderNo = '{request["CompOrderNo"].ToString()}' AND CompOrderDate = '{request["CompOrderDate"].ToString()}' AND CustomerCode = 'GenoCore'";

                if (!UpdateGenoCore(tokenKey, request["CompOrderNo"].ToString(), request["EmailAddress"].ToString(), out string status))
                {
                    throw new Exception($"[{status}] GenoCoreSendERROR");
                }
                else
                {

                }
                LabgeDatabase.ExecuteSql(sql);

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

        private bool UpdateGenoCore(string tokenKey, string orderNo, string email, out string status)
        {
            string genoCoreUrl = $"https://api.genocorebs.com/users/{orderNo}";
            string authToken = string.Empty;
            var client = new RestClient(genoCoreUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Authorization", $"Bearer {tokenKey}");
            request.AddHeader("Content-Type", "application/json");

            JObject jobj = new JObject();
            jobj.Add("email", email);


            request.AddParameter("application/json", jobj.ToString(), ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            try
            {

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    status = "OK";
                    var recvObj = JsonConvert.DeserializeObject<JObject>(response.Content);
                    uint code = uint.Parse(recvObj["code"].ToString());
                    if (code == 200 || code == 201)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else if (Convert.ToInt32(response.StatusCode) == 0)
                {
                    throw new Exception("서버에 연결할 수 없습니다.");
                }
                else
                {
                    throw new Exception(JObject.Parse(response.Content)["Message"].ToString());
                }
            }
            catch (Exception ex)
            {
                status = ex.Message;
                return false;
            }

        }

        public IHttpActionResult Delete(string LabRegDate, string LabRegNo)
        {
            return Ok();
        }


    }
}
