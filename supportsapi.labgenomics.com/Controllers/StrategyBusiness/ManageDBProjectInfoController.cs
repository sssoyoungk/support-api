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
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.StrategyBusiness
{
    [SupportsAuth]
    [Route("api/StrategyBusiness/ManageDBProject")]
    public class ManageDBProjectInfoController : ApiController
    {
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string groupCode)
        {
            string sql;
            sql =
                $"SELECT\n" +
                $"ppi.CompOrderDate, ppi.CompOrderNo, ppi.Gender, ppi.Race, ppi.BirthDay, ppi.PatientName, ppi.ZipCode, ppi.Address, ppi.Address2, ppi.EmailAddress,\n" +
                $"ppi.PhoneNumber, ppi.AgreePrivacyPolicyDateTime, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer, ppi.AgreeThirdPartySensitive,\n" +
                $"ppi.AgreeGeneThirdPartySensitive, ppi.AgreeKeepDataAndFutureAnalysis, ppi.AgreeSendResultEmail, ppi.IsAgreeConsultation,  CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo, lrr.IsReportTransEnd, CONVERT(varchar(19), lrr.ReportTransEndTime, 21) AS ReportTransEndTime,\n" +
                $"ppi.CompCode, pcc.CompName, pcgc.CompGroupName, " +
                $"(CASE WHEN ltcoi.ResultSendState is null then CONVERT(BIT, 0) " +
                $"WHEN ltcoi.ResultSendState != 'Y' then CONVERT(BIT, 0) " +
                $"ELSE CONVERT(BIT, 1) END) AS ResultSendState  \n" +
                $"FROM PGSPatientInfo ppi\n" +
                $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\n" +
                $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\n" +
                $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\n" +
                $"AND ltcoi.CompCode = ppi.CompCode\n" +
                $"JOIN ProgCompCode pcc\n" +
                $"ON ppi.CompCode  = pcc.CompCode\n" +
                $"JOIN ProgCompGroupCode pcgc\n" +
                $"On pcgc.CompGroupCode = pcc.CompGroupCode\n" +
                $"JOIN ProgAuthGroupAccessComp pagac\n" +
                $"on pagac.CompCode = pcc.CompCode\n" +
                $"LEFT OUTER JOIN LabRegReport lrr\n" +
                $"ON ltcoi.LabRegDate = lrr.LabRegDate\n" +
                $"AND ltcoi.LabRegNo = lrr.LabRegNo\n" +
                $"WHERE ppi.CustomerCode = 'GenoCore'\n" +
                $"AND ppi.CompOrderDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\n" +
                $"AND pagac.AuthGroupCode = '{groupCode}'\n";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/StrategyBusiness/ManageDBProject/SendMail")]
        public IHttpActionResult Post([FromBody]JObject request)
        {
            StringBuilder query = new StringBuilder();
            query.Append("select\n");
            query.Append("CONVERT(varchar, lri.LabRegDate, 23) AS LabRegDate, lri.LabRegNo, lri.PatientName, ppi.EmailAddress, lrr.ReportCode,lrr.LabRegReportID, ltcoi.CompOrderNo, lrr.ReportEndTime,\n");
            query.Append("ppi.Barcode, lrr.ReportStateCode, 'https://reports.labgenomics.com/api/downloadreport?reportid=' + CONVERT(varchar(36), lrr.LabRegReportID) + '&filetype=pdf&isusepassword=true' AS ReportUrl\n");
            query.Append("from PGSPatientInfo ppi\n");
            query.Append("join LabTransCompOrderInfo ltcoi\n");
            query.Append("on ppi.CompOrderDate = ltcoi.CompOrderDate\n");
            query.Append("and ppi.CompOrderNo = ltcoi.CompOrderNo\n");
            query.Append("and ppi.CompCode = ltcoi.CompCode\n");
            query.Append("join PGSTestInfo pi2\n");
            query.Append("on pi2.CompOrderDate = ltcoi.CompOrderDate\n");
            query.Append("and pi2.CompOrderNo = ltcoi.CompOrderNo\n");
            query.Append("and pi2.CompCode = ltcoi.CompCode\n");
            query.Append("join LabRegInfo lri\n");
            query.Append("on ltcoi.LabRegDate = lri.LabRegDate\n");
            query.Append("and ltcoi.LabRegNo  = lri.LabRegNo\n");
            query.Append("join LabRegReport lrr \n");
            query.Append("on lri.LabRegDate = lrr.LabRegDate\n");
            query.Append("and lri.LabRegNo  = lrr.LabRegNo\n");
            query.Append("and lrr.ReportStateCode = 'F'\n");
            query.Append("and ppi.CustomerCode = 'genocore'\n");
            query.Append($"and lrr.LabRegDate = '{request["LabRegDate"]}' AND lrr.LabRegNo = '{request["LabRegNo"]}'");

            try
            {
                JObject jObject = LabgeDatabase.SqlToJObject(query.ToString());
                if (jObject.Count == 0)
                {
                    throw new Exception("전송 할 데이터가 없습니다.");
                }

                MailMessage mail = new MailMessage();

                mail.From = new MailAddress("pgs@labgenomics.com", "랩지노믹스 사업부", System.Text.Encoding.UTF8);

                string emailAddress = jObject["EmailAddress"].ToString();


                mail.To.Add(emailAddress);

                mail.Subject = $"[랩지노믹스] {jObject["PatientName"].ToString()}님의 검사 결과 안내 드립니다.";
                mail.Body = $"안녕하세요. {jObject["PatientName"].ToString()}님\r\n\r\n" +
                            $"유전자 검사 분석기관 랩지노믹스입니다.\r\n" +
                            $"검사 결과를 안내드립니다.\r\n" +
                            $"아래 url을 클릭 후 비밀번호 입력하세요.\r\n" +
                            $"{jObject["ReportUrl"]}\r\n" +
                            $"PDF파일의 비밀번호는 생년월일 6자리입니다. (예 : 850505)\r\n" +
                            $"저희 서비스를 이용해주셔서 감사드립니다.\r\n" +
                            $"문의사항은 이메일 또는 유선으로 연락주시면 상담 가능하십니다.\r\n" +
                            $"(주)랩지노믹스 운영관리팀(031-628-0660)\r\n";

                mail.SubjectEncoding = System.Text.Encoding.UTF8;
                mail.BodyEncoding = System.Text.Encoding.UTF8;

                SmtpClient SmtpServer = new SmtpClient("mail.labgenomics.com");
                // smtp 포트
                SmtpServer.Port = 25;
                // smtp 인증
                SmtpServer.Credentials = new NetworkCredential("pgs", "pgs1!");
                // 발송
                SmtpServer.Send(mail);

                mail.Dispose();


                //전송 완료 업데이트
                string sql =
                        $"UPDATE LabRegReport\r\n" +
                        $"SET IsReportTransEnd = 1, ReportTransEndTime = getDate()\r\n" +
                        $"WHERE LabRegDate = '{jObject["LabRegDate"].ToString()}'\r\n" +
                        $"AND LabRegNo = {jObject["LabRegNo"].ToString()}\r\n" +
                        $"AND ReportCode = '{jObject["ReportCode"].ToString()}'";

                LabgeDatabase.ExecuteSql(sql);
                
            }
            catch (Exception ex)
            {

                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }

            return Ok();
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
