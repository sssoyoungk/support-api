using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.ServiceSMS;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers
{
    [SupportsAuth]
    public class SendSMSController : ApiController
    {
        private string id = "labge";
        private string pw = "labge5096!";
        // PUT api/<controller>/5
        public IHttpActionResult Put([FromBody]JObject jobjRequest)
        {
            ServiceSMSSoapClient client = new ServiceSMSSoapClient();
            if (jobjRequest["Kind"].ToString() == "1") //즉시전송
            {
                string hash = MD5HashFunc(id + pw + jobjRequest["ReceiveNumber"].ToString());
                client.SendSMS(id, hash, jobjRequest["SendNumber"].ToString(), jobjRequest["ReceiveNumber"].ToString(), jobjRequest["Message"].ToString());
            }
            else if (jobjRequest["Kind"].ToString() == "2") //예약전송
            {
                string hash = MD5HashFunc(id + pw + jobjRequest["ReceiveNumber"].ToString());
                //client.SendSMSReserve()
            }
            else if (jobjRequest["KInd"].ToString() == "3") //예약취소
            {
                //ReserveCancle
            }
            return Ok("Put1");
        }

        private string GetSMSCodeDescription(string code)
        {
            string description = string.Empty;
            if (code == "1")
            {
                description = "발송 성공";
            }
            else if (code == "-1")
            {
                description = "ID/비밀번호 이상";
            }
            else if (code == "-2")
            {
                description = "ID 공백";
            }
            else if (code == "-3")
            {
                description = "다중 전송시 모두 실패(수신번호이상)";
            }
            else if (code == "-4")
            {
                description = "해쉬공백";
            }
            else if (code == "-5")
            {
                description = "해쉬이상";
            }
            else if (code == "-6")
            {
                description = "수신자 전화번호 공백";
            }
            else if (code == "-8")
            {
                description = "발신자 전화번호 공백";
            }
            else if (code == "-9")
            {
                description = "전송내용 공백";
            }
            else if (code == "-10")
            {
                description = "예약 날짜 이상";
            }
            else if (code == "-11")
            {
                description = "예약 시간 이상";
            }
            else if (code == "-12")
            {
                description = "예약 가능시간 지남";
            }
            else if (code == "-30")
            {
                description = "등록되지 않은 발신번호";
            }

            return description;
        }

        private string MD5HashFunc(string str)
        {
            StringBuilder MD5Str = new StringBuilder();
            byte[] byteArr = Encoding.ASCII.GetBytes(str);
            byte[] resultArr = (new MD5CryptoServiceProvider()).ComputeHash(byteArr);

            for (int cnti = 0; cnti < resultArr.Length; cnti++)
            {
                MD5Str.Append(resultArr[cnti].ToString("X2"));
            }
            return MD5Str.ToString();
        }
    }
}