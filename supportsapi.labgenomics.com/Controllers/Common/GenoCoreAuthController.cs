using Newtonsoft.Json.Linq;
using RestSharp;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Common
{
    [Route("api/Common/GenoCoreAuthority")]
    public class GenoCoreAuthorityController : ApiController
    {
        public IHttpActionResult Get()
        {
            string genoCoreUrl = "http://api.genocorebs.com/auth";
            string authToken = string.Empty;
            var client = new RestClient(genoCoreUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddCookie("Cookie", "PHPSESSID=1ffokj5o56bqtahj5qq3p8hde2"); //저번에는 꼭 사용하여야했음. 혹시 몰라 추가

            JObject jobj = new JObject();
            jobj.Add("x-id", "labgenomics");
            jobj.Add("x-pw", "Foawlsh#226");


            request.AddParameter("application/json", jobj, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            try
            {

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JObject receive = new JObject();
                    var recvjboj = JObject.Parse(response.Content);

                    if (recvjboj["code"].ToString() != "201")
                    {
                        receive.Add("message", recvjboj["message"].ToString());
                        receive.Add("code", recvjboj["code"].ToString());
                        return Ok(receive);
                    }
                    

                    authToken = recvjboj["token"].ToString();
                    if (authToken == string.Empty)
                    {
                        receive.Add("message", recvjboj["message"].ToString());
                        receive.Add("code", recvjboj["code"].ToString());
                        return Ok(receive);
                    }

                    DateTime dteTokenExpriedTime = DateTime.Parse(recvjboj["expiredTime"].ToString());

                    if (dteTokenExpriedTime <= DateTime.Now)
                    {
                        receive.Add("message", recvjboj["message"].ToString());
                        receive.Add("code", recvjboj["code"].ToString());
                        receive.Add("authToken", recvjboj["token"].ToString());
                        return Ok(receive);
                    }

                    receive.Add("authToken", authToken);
                    return Ok(receive);


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
                authToken = string.Empty;
                return Ok(ex.Message);
            }
        }
    }
}