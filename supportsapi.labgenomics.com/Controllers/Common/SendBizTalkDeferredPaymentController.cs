using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Common
{
    [Route("api/Common/SendBizTalkSendBizTalkDeferredPayment")]
    public class SendBizTalkDeferredPaymentController : ApiController
    {
        // POST api/<controller>
        public IHttpActionResult Post([FromBody]JObject objRequest)
        {
            const string smsUrl = "https://alimtalk-api.sweettracker.net/v2/7af771f7373258c55f23f6daf969a67bcffdb3ce/sendMessage";
            JArray arrMessage = new JArray();
            JObject objMessage = new JObject();

            string phoneCode = objRequest["PhoneNo"].ToString();
            phoneCode = phoneCode.Substring(3);
            phoneCode = phoneCode.Replace("-", "");

            objMessage.Add("msgid", $"{DateTime.Now.ToString("MMddhhmmssf")}{phoneCode}");
            objMessage.Add("message_type", "at");
            objMessage.Add("sms_kind", "L");
            objMessage.Add("receiver_num", objRequest["PhoneNo"]);
            objMessage.Add("sender_num", objRequest["SendNumber"]);
            objMessage.Add("profile_key", "7af771f7373258c55f23f6daf969a67bcffdb3ce");
            objMessage.Add("template_code", objRequest["template_code"].ToString());
            objMessage.Add("message", objRequest["Message"].ToString());
            objMessage.Add("sms_message", objRequest["Message"].ToString());
            objMessage.Add("sms_only", "N"); //비즈톡
            objMessage.Add("reserved_time", "00000000000000");
            if (objRequest["sms_title"] != null)
            {
                objMessage.Add("sms_title", objRequest["sms_title"].ToString());
            }
            
            arrMessage.Add(objMessage);
            var client = new RestClient(smsUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("userid", "labgenomics");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", arrMessage, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {

                var objResponse = JArray.Parse(response.Content)[0];

                if (objResponse["result"].ToString() == "Y")
                {
                    System.Threading.Thread.Sleep(100);
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            else if (Convert.ToInt32(response.StatusCode) == 0)
            {
                return BadRequest("서버에 연결할 수 없습니다.");
            }
            else
            {
                return BadRequest(JObject.Parse(response.Content)["Message"].ToString());
            }
        }
    }
}