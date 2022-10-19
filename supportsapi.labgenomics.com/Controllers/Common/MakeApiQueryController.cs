using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Make
{
    [SupportsAuth]
    [Route("api/Common/MakeApiQuery")]
    public class MakeApiQueryController : ApiController
    {
        /// <summary>
        /// Post
        /// </summary>
        /// <param name="request">JObjcet : query : 쿼리문, Mode : Select OR NotSelect</param>
        /// <returns></returns>
        public IHttpActionResult Post([FromBody]JObject request)
        {
            try
            {
                string sql = request["query"].ToString();
                if (request["Mode"].ToString() == "Select")
                {
                    JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
                    return Ok(arrResponse);
                }
                else
                {
                    int execCount = LabgeDatabase.ExecuteSql(sql);
                    if (execCount <= 0)
                    {
                        throw new HttpException(404, $"없는 자료 이거나 요청 할 수 없는 상태입니다.");
                    }
                    JObject Receive = new JObject();
                    Receive.Add("count", execCount);
                    return Ok(Receive);
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