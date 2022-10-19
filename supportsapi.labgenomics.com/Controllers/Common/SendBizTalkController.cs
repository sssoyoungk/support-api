using Newtonsoft.Json.Linq;
using RestSharp;
using supportsapi.labgenomics.com.Attributes;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Common
{
    [Route("api/Common/SendBizTalk")]
    public class SendBizTalkController : ApiController
    {
        private const string smsUrl = "https://alimtalk-api.bizmsg.kr/v2/sender/send";
        // POST api/<controller>
        public IHttpActionResult Post([FromBody]JArray arrRequest)
        {
            try
            {
                JArray arrMessage = new JArray();
                foreach (JObject objRequest in arrRequest)
                {
                    JObject objMessage = new JObject();

                    objMessage.Add("message_type", "at");
                    objMessage.Add("phn", objRequest["PhoneNumber"].ToString());
                    objMessage.Add("profile", "7af771f7373258c55f23f6daf969a67bcffdb3ce");
                    objMessage.Add("msg", objRequest["Message"].ToString());
                    objMessage.Add("tmplId", objRequest["tmplId"].ToString());
                    objMessage.Add("smsKind", objRequest["smsKind"].ToString());
                    objMessage.Add("msgSms", objRequest["Message"].ToString());
                    objMessage.Add("smsSender", objRequest["smsSender"].ToString());
                    //objMessage.Add("smsLmsTit", "랩지노믹스 유전자검사 안내");
                    arrMessage.Add(objMessage);
                }

                var client = new RestClient(smsUrl);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("userid", "labgenomics");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", arrMessage, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok();
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

                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }

        }
    }
}