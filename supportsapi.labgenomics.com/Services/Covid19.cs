using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Linq;
using System.Net;

namespace supportsapi.labgenomics.com.Services
{
    public class Covid19
    {
        /// <summary>
        /// API키 가져옴
        /// </summary>
        /// <returns></returns>
        public JObject GetAPIKey()
        {
            string sql;
            sql = "SELECT TOP 1 ClientId AS clientId, APIKey AS authKey\r\n" +
                  "FROM Covid19APIKey\r\n" +
                  "ORDER BY RegistDateTime DESC";
            JObject objApiKey = LabgeDatabase.SqlToJObject(sql);

            return objApiKey;
        }

        /// <summary>
        /// API키갱신
        /// </summary>
        /// <returns></returns>
        public string RefreshAPIKey()
        {
            JObject objAPIKey = GetAPIKey();

            JObject objRequest = new JObject();
            objRequest.Add("clientId", objAPIKey["clientId"].ToString());
            objRequest.Add("authKey", objAPIKey["authKey"].ToString());
            string apiUrl = "https://covid19.kdca.go.kr/api/cm/uptAuthKey";

            var client = new RestClient(apiUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", objRequest, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                JObject objResponse = JObject.Parse(response.Content);

                if (objResponse["resultCd"].ToString() != "R0200")
                {
                    throw new Exception(objResponse["resultCd"].ToString());
                }

                string sql;

                sql = "SELECT TOP 1 *\r\n" +
                      "FROM Covid19APIKey\r\n" +
                      "ORDER BY RegistDateTime DESC";
                JObject objKey = LabgeDatabase.SqlToJObject(sql);

                if (objKey["APIKey"].ToString() != objResponse["apiKey"].ToString())
                {
                    sql = $"INSERT INTO Covid19APIKey\r\n" +
                          $"SELECT 'sjpark_labge',  '{objResponse["apiKey"].ToString()}', GETDATE()";
                    LabgeDatabase.ExecuteSql(sql);
                }

                return objResponse["apiKey"].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        //질청 코로나 최종 처리
        public void Covid19ResultConfirm(JObject objResult, JObject objFail, string memberID = "")
        {
            string sql = string.Empty;
            string apiUrl = "https://covid19.kdca.go.kr/api/pi/cfmIrResultList";

            JArray arrSpmList = new JArray();
            JArray arrSendResults = JArray.Parse(objResult["irResultList"].ToString());
            JArray arrFailResults = JArray.Parse(objFail["irFailrList"].ToString());
            foreach (JObject objSendResult in arrSendResults)
            {
                bool isFail = false;
                foreach (JObject objFailResult in arrFailResults)
                {
                    if (objSendResult["spmNo"].ToString() == objFailResult["spmNo"].ToString())
                    {
                        isFail = true;
                        continue;
                    }
                }

                if (!isFail)
                {
                    JObject objSpm = new JObject();
                    objSpm.Add("spmNo", objSendResult["spmNo"].ToString());
                    arrSpmList.Add(objSpm);
                }
            }

            if (arrSpmList.Count == 0)
                return;

            JObject objRequest = GetAPIKey();
            objRequest.Add("irResultList", arrSpmList);

            var client = new RestClient(apiUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", objRequest, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                JObject objResponse = JObject.Parse(response.Content);
                JArray arrFailLists = JArray.Parse(objResponse["irFailrList"].ToString());
                foreach (JObject objSample in arrSpmList)
                {
                    bool isFail = false;
                    foreach (JObject objFailSample in arrFailLists)
                    {
                        if (objSample["spmNo"].ToString() == objFailSample["spmNo"].ToString())
                        {
                            isFail = true;
                        }                        
                    }

                    if (!isFail)
                    {
                        sql = $"UPDATE Covid19Order\r\n" +
                              $"SET ExportDateTime = GETDATE()\r\n" +
                              $"  , APIErrorCode = '200'\r\n" +
                              $"  , ExportMemberID = '{memberID}'" +
                              $"WHERE SampleNo = '{objSample["spmNo"].ToString()}'";
                        LabgeDatabase.ExecuteSql(sql);
                    }
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
    }
}
