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
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Preference
{
    [SupportsAuth]
    [Route("api/Preference/GenocoreCompList")]

    public class GenocoreCompListController : ApiController
    {
#if DEBUG
        private string genocoreUrl = "https://sapi.genocorebs.com";
#else
        private string genocoreUrl = "https://api.genocorebs.com";
#endif
        public IHttpActionResult Get()
        {
            string sql;
            sql =
                "SELECT gc.*, pcc.CompName\r\n" +
                "FROM GenocoreCompCode gc\r\n" +
                "LEFT OUTER JOIN ProgCompCode pcc\r\n" +
                "ON pcc.CompCode = gc.CompCode\r\n" +
                "ORDER BY ClientCustomerIdx";
            JArray objResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(objResponse);
        }

        // POST api/<controller>
        public IHttpActionResult Post()
        {
            try
            {
                //제노코어 토큰 수신
                string token = GetGenocoreToken();
                if (token == string.Empty)
                {
                    throw new Exception("제노코어 토큰발급을 실패했습니다.");
                }
                //거래처 목록 수신                
                var client = new RestClient($"{genocoreUrl}/customers");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", $"Bearer {token}");
                IRestResponse response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    JObject objResponse = JObject.Parse(response.Content);
                    foreach (JObject objCustomer in objResponse["data"])
                    {
                        //수신받은 거래처 목록 등록
                        string sql;
                        sql =
                            $"MERGE INTO GenocoreCompCode AS target\r\n" +
                            $"USING (SELECT '{objCustomer["clientCustomerIdx"]}' AS ClientCustomerIdx ) AS source\r\n" +
                            $"ON (target.ClientCustomerIdx = source.clientCustomerIdx)\r\n" +
                            $"WHEN NOT MATCHED THEN\r\n" +
                            $"INSERT (ClientCustomerIdx, ClientCustomerName)\r\n" +
                            $"VALUES\r\n" +
                            $"(\r\n" +
                            $"    {objCustomer["clientCustomerIdx"]}, '{objCustomer["clientCustomerName"]}'\r\n " +
                            $");\r\n";

                        LabgeDatabase.ExecuteSql(sql);
                    }
                }
                
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

        // PUT api/<controller>/5
        public IHttpActionResult Put([FromBody]JObject request)
        {
            try
            {
                string sql;
                sql =
                    $"UPDATE GenocoreCompCode\r\n" +
                    $"SET CompCode = '{request["CompCode"].ToString()}', RegistMemberID = '{request["MemberID"].ToString()}', RegistDateTime = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' \r\n" +
                    $"WHERE ClientCustomerIdx = '{request["ClientCustomerIdx"].ToString()}'";

                LabgeDatabase.ExecuteSql(sql);

                sql =
                    $"SELECT CompName\r\n" +
                    $"FROM ProgCompCode\r\n" +
                    $"WHERE CompCode = '{request["CompCode"].ToString()}'";
                JObject objResponse = LabgeDatabase.SqlToJObject(sql);
                return Ok(objResponse);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }

        private string GetGenocoreToken()
        {
            var client = new RestClient($"{genocoreUrl}/auth");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "applicatgion/json");
            request.AddParameter("application/json", "{\"x-id\": \"labgenomics\", \"x-pw\": \"Foawlsh#226@\"}", ParameterType.RequestBody);
            var response = client.Execute(request);
            if (response.IsSuccessful)
            {
                JObject objResponse = JObject.Parse(response.Content);
                return objResponse["data"]["token"].ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}