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
    [Route("api/StrategyBusiness/ManageKyoboGenoPac")]
    public class ManageKyoboGenoPacController : ApiController
    {
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        // GET api/<controller>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate)
        {
            try
            {
                string sql;
                sql = $"SELECT CONVERT(bit,0) AS ColumnCheck, CONVERT(varchar, ppi.RegistDateTime, 23) AS RegistDate, CONVERT(varchar, ppi.CompOrderDate, 23) AS CompOrderDate, ppi.CompOrderNo\r\n" +
                      $"     , ppi.PatientName, ppi.Barcode, ppi.BirthDay, ppi.Age, ppi.Gender, ppi.ReservationCompCode\r\n" +
                      $"     , pcc.CompName AS ReservationCompName\r\n" +
                      $"     , ppi.AgreeRequestTest, ppi.AgreeLabgePrivacyPolicy, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer, ppi.AgreeSendResultSMS\r\n" +
                      $"     , ppi.PhoneNumber, ppi.Address, ppi.ZipCode, ppi.CustomerEmpCode, pcc.CompName\r\n" +
                      $"     , CONVERT(varchar, lri.LabRegDate, 23) AS LabRegDate, lri.LabRegNo\r\n" +
                      $"     , CONVERT(varchar(19), lrr.ReportEndTime, 21) AS ReportEndTime, CheckReservationSMS, CheckReservationSMS2, CheckRegistSMS, CheckSendResultSMS\r\n" +
                      $"     , CONVERT(varchar(19), SMSSendDateTime, 21) AS SMSSendDateTime, CONVERT(date, ppi.ReservationDateTime) AS ReservationDate\r\n" +
                      $"     , CONVERT(varchar(5), ReservationDateTime, 8) AS ReservationTime, ppi.EmailAddress\r\n" +
                      $"FROM PGSPatientInfo ppi\r\n" +
                      $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                      $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\r\n" +
                      $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\r\n" +
                      $"AND ltcoi.CompCode = ppi.CompCode\r\n" +
                      $"LEFT OUTER JOIN LabRegInfo lri\r\n" +
                      $"ON lri.LabRegDate = ltcoi.LabRegDate\r\n" +
                      $"AND lri.LabRegNo = ltcoi.LabRegNo\r\n" +
                      $"LEFT OUTER JOIN LabRegReport lrr\r\n" +
                      $"ON lri.LabRegDate = lrr.LabRegDate\r\n" +
                      $"AND lri.LabRegNo = lrr.LabRegNo\r\n" +
                      $"JOIN ProgCompCode pcc\r\n" +
                      $"ON ppi.CompCode = pcc.CompCode\r\n" +
                      $"AND ppi.RegistDateTime >= '{beginDate.ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND ppi.RegistDateTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')\r\n" +
                      $"WHERE ppi.CustomerCode = 'kyobo'\r\n" +
                      $"ORDER BY RegistDateTime";
                JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

                return Ok(arrResponse);
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

        /// <summary>
        /// 업데이트
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHttpActionResult Put([FromBody]JObject request)
        {
            try
            {
                string sql;
                sql = $"UPDATE PGSPatientInfo\r\n" +
                      $"SET Barcode = '{request["Barcode"].ToString()}'\r\n" +
                      $"  , AgreeRequestTest = {Convert.ToInt32(request["AgreeRequestTest"])}\r\n" +
                      $"  , AgreeGeneTest = {Convert.ToInt32(request["AgreeGeneTest"])}\r\n" +
                      $"  , ReservationCompCode = '{request["ReservationCompCode"].ToString()}'\r\n" +
                      $"  , CompCode = '{request["ReservationCompCode"].ToString()}'\r\n" +
                      $"  , ReservationDateTime = '{Convert.ToDateTime(request["ReservationDateTime"]).ToString("yyyy-MM-dd HH:mm")}'\r\n" +
                      $"  , PatientName = '{request["PatientName"].ToString()}'\r\n" +
                      $"  , Gender = '{request["Gender"].ToString()}'\r\n" +
                      $"  , BirthDay = '{request["BirthDay"].ToString()}'\r\n" +
                      $"  , EmailAddress = '{request["EmailAddress"].ToString()}'\r\n" +
                      $"  , PhoneNumber = '{request["PhoneNumber"].ToString()}'\r\n" +
                      $"  , Address = '{request["Address"].ToString()}'\r\n" +
                      $"  , CustomerEmpCode = '{request["EmpCode"].ToString()}'\r\n" +
                      $"WHERE CompOrderDate = '{request["CompOrderDate"].ToString()}'\r\n" +
                      $"AND CompOrderNo = '{request["CompOrderNo"].ToString()}'\r\n" +
                      $"AND CustomerCode = 'kyobo'\r\n" +
                      $"UPDATE PGSTestInfo\r\n" +
                      $"SET CompCode = '{request["ReservationCompCode"].ToString()}',\r\n" +
                      $"    CompTestCode = '{((request["Gender"].ToString() == "M") ? "13596" : "13597")}',\r\n" +
                      $"    CompTestName = '{((request["Gender"].ToString() == "M") ? "GenoPAC_남성종합 12종 I" : "GenoPAC_여성종합 12종 I")}'\r\n" +
                      $"WHERE CompOrderDate = '{request["CompOrderDate"].ToString()}'\r\n" +
                      $"AND CompOrderNo = '{request["CompOrderNo"].ToString()}'\r\n" +
                      $"AND CustomerCode = 'kyobo'";
                LabgeDatabase.ExecuteSql(sql);

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

        /// <summary>
        /// 예약정보 삭제
        /// </summary>
        /// <param name="compOrderDate"></param>
        /// <param name="compOrderNo"></param>
        /// <returns></returns>
        public IHttpActionResult Delete(DateTime compOrderDate, string compOrderNo)
        {
            try
            {
                string sql;
                sql = $"DELETE FROM PGSPatientInfo\r\n" +
                      $"WHERE CompOrderDate = '{compOrderDate.ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND CompOrderNo = '{compOrderNo}'\r\n" +
                      $"AND CustomerCode = 'kyobo'\r\n" +
                      $"DELETE FROM PGSTestInfo\r\n" +
                      $"WHERE CompOrderDate = '{compOrderDate.ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND CompOrderNo = '{compOrderNo}'\r\n" +
                      $"AND CustomerCode = 'kyobo'\r\n";
                LabgeDatabase.ExecuteSql(sql);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testDate"></param>
        /// <returns></returns>
        [Route("api/StrategyBusiness/ManageKyoboGenoPac/TestCount")]
        public IHttpActionResult GetTestCount(DateTime testDate)
        {
            string sql;
            sql = $"SELECT ptc.CompCode, pcc.CompName, ptc.BeginDate, ptc.TestCount\r\n" +
                  $"FROM PGSTestCount ptc\r\n" +
                  $"JOIN ProgCompCode pcc\r\n" +
                  $"ON ptc.CompCode = pcc.CompCode\r\n" +
                  $"WHERE ptc.CustomerCode = 'kyobo'\r\n" +
                  $"AND CONVERT(date, BeginDate, 23) = '{testDate.ToString("yyyy-MM-dd")}'\r\n";
            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/StrategyBusiness/ManageKyoboGenoPac/TestCount")]
        public IHttpActionResult PostTestCount([FromBody]JArray request)
        {
            return Ok();
        }

        [Route("api/StrategyBusiness/ManageKyoboGenoPac/TestCount")]
        public IHttpActionResult PutTestCount([FromBody]JArray arrRequest)
        {
            foreach (JObject objRequest in arrRequest)
            {
                string sql;
                sql =
                    $"UPDATE PGSTestCount\r\n" +
                    $"SET TestCount = {objRequest["TestCount"]}\r\n" +
                    $"WHERE CustomerCode = 'kyobo'\r\n" +
                    $"AND CompCode = '{objRequest["CompCode"].ToString()}'\r\n" +
                    $"AND BeginDate = '{Convert.ToDateTime(objRequest["BeginDate"]).ToString("yyyy-MM-dd HH:mm:ss.fff")}'";
                LabgeDatabase.ExecuteSql(sql);
            }
            
            return Ok();
        }

        [Route("api/StrategyBusiness/ManageKyoboGenoPac/TestCount")]
        public IHttpActionResult DeleteTestCount(string compCode, DateTime beginDate)
        {
            string sql;
            sql =
                $"DELETE FROM PGSTestCount\r\n" +
                $"WHERE CustomerCode = 'kyobo'\r\n" +
                $"AND CompCode = '{compCode}'\r\n" +
                $"AND BeginDate = '{beginDate.ToString("yyyy-MM-dd HH:mm:ss.fff")}'";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }
    }
}