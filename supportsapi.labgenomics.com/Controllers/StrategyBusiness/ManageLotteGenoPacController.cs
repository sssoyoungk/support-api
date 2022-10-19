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
    [Route("api/StrategyBusiness/ManageLotteGenoPac")]
    public class ManageLotteGenoPacController : ApiController
    {
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate)
        {
            try
            {
                string sql;
                sql = $"SELECT CONVERT(bit,0) AS ColumnCheck, CONVERT(varchar, ppi.RegistDateTime, 23) AS RegistDate, CONVERT(varchar, ppi.CompOrderDate, 23) AS CompOrderDate, ppi.CompOrderNo\r\n" +
                      $"     , ppi.PatientName, ppi.Barcode, ppi.BirthDay, ppi.Age, ppi.Gender, ppi.ReservationCompCode\r\n" +
                      $"     , (SELECT CompName FROM ProgCompCode WHERE CompCode = ReservationCompCode) AS ReservationCompName\r\n" +
                      $"     , ppi.EmailAddress, ppi.AgreeRequestTest, ppi.AgreeLabgePrivacyPolicy, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer, ppi.AgreeSendResultSMS\r\n" +
                      $"     , ppi.PhoneNumber, ppi.Address, ppi.ZipCode, ppi.CustomerEmpCode, pe.EmpName, pe.CompSubCode AS CompCode, pcsc.CompSubName AS CompName\r\n" +
                      $"     , CONVERT(varchar, lri.LabRegDate, 23) AS LabRegDate, lri.LabRegNo\r\n" +
                      $"     , CONVERT(varchar(19), lrr.ReportEndTime, 21) AS ReportEndTime, CheckReservationSMS, CheckReservationSMS2, CheckRegistSMS, CheckSendResultSMS\r\n" +
                      $"     , CONVERT(varchar(19), SMSSendDateTime, 21) AS SMSSendDateTime, CONVERT(date, ppi.ReservationDateTime) AS ReservationDate\r\n" +
                      $"FROM PGSPatientInfo ppi\r\n" +
                      $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                      $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\r\n" +
                      $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\r\n" +
                      $"and ppi.CompCode = ltcoi.CompCode\r\n" +
                      $"LEFT OUTER JOIN LabRegInfo lri\r\n" +
                      $"ON lri.LabRegDate = ltcoi.LabRegDate\r\n" +
                      $"AND lri.LabRegNo = ltcoi.LabRegNo\r\n" +
                      $"LEFT OUTER JOIN LabRegReport lrr\r\n" +
                      $"ON lri.LabRegDate = lrr.LabRegDate\r\n" +
                      $"AND lri.LabRegNo = lrr.LabRegNo\r\n" +
                      $"JOIN PGSCustomerEmployee pe\r\n" +
                      $"ON pe.CustomerCode = 'lotte'\r\n" +
                      $"AND pe.EmpCode = ppi.CustomerEmpCode\r\n" +
                      $"JOIN ProgCompSubCode pcsc\r\n" +
                      $"ON pcsc.CompCode = '70096'\r\n" +
                      $"AND pcsc.CompSubCode = pe.CompSubCode\r\n" +
                      $"WHERE ppi.CustomerCode = 'lotte'\r\n" +
                      $"AND ppi.RegistDateTime >= '{beginDate.ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND ppi.RegistDateTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')\r\n" +
                      $"ORDER BY ppi.RegistDateTime";
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
                      $"  , ReservationDateTime = '{Convert.ToDateTime(request["ReservationDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                      $"  , PatientName = '{request["PatientName"].ToString()}'\r\n" +
                      $"  , BirthDay = '{request["BirthDay"].ToString()}'\r\n" +
                      $"  , Gender = '{request["Gender"].ToString()}'\r\n" +
                      $"  , PhoneNumber = '{request["PhoneNumber"].ToString()}'\r\n" +
                      $"  , EmailAddress = '{request["EmailAddress"].ToString()}'\r\n" +
                      $"  , Address = '{request["Address"].ToString()}'\r\n" +
                      $"WHERE CompOrderDate = '{request["CompOrderDate"].ToString()}'\r\n" +
                      $"AND CompOrderNo = '{request["CompOrderNo"].ToString()}'\r\n" +
                      $"AND CustomerCode = 'lotte'\r\n" +
                      $"\r\n" +
                      $"UPDATE PGSTestInfo\r\n" +
                      $"SET CompTestCode = '{((request["Gender"].ToString() == "M") ? "13596" : "13597")}'\r\n" +
                      $"  , CompTestName = '{((request["Gender"].ToString() == "M") ? "GenoPAC_남성종합 12종 I" : "GenoPAC_여성종합 12종 I")}'\r\n" +
                      $"WHERE CompOrderDate = '{request["CompOrderDate"].ToString()}'\r\n" +
                      $"AND CompOrderNo = '{request["CompOrderNo"].ToString()}'\r\n" +
                      $"AND CustomerCode = 'lotte'\r\n";
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

        public IHttpActionResult Delete(DateTime compOrderDate, string compOrderNo)
        {
            try
            {
                string sql;
                sql = $"DELETE FROM PGSPatientInfo\r\n" +
                      $"WHERE CompOrderDate = '{compOrderDate.ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND CompOrderNo = '{compOrderNo}'\r\n" +
                      $"AND CustomerCode = 'lotte'\r\n" +
                      $"DELETE FROM PGSTestInfo\r\n" +
                      $"WHERE CompOrderDate = '{compOrderDate.ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND CompOrderNo = '{compOrderNo}'\r\n" +
                      $"AND CustomerCode = 'lotte'\r\n";
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

        [Route("api/StrategyBusiness/ManageLotteGenoPac/TestCount")]
        public IHttpActionResult GetTestCount(int year = 0, int week = 0)
        {
            try
            {
                string sql;

                sql = $"SELECT TOP 1 DATEPART(YEAR, ptc.BeginDate) AS Year, DATEPART(WEEK, ptc.BeginDate) AS Week\r\n" +
                      $"FROM PGSTestCount ptc\r\n" +
                      $"WHERE CustomerCode = 'lotte'\r\n";
                if (week == 0)
                {
                    sql += $"AND DATEPART(YEAR, BeginDate) = DATEPART(YEAR, GETDATE())\r\n" +
                           $"AND DATEPART(WEEK, BeginDate) = DATEPART(WEEK, GETDATE())\r\n";                    
                }
                else
                {
                    sql += $"AND DATEPART(YEAR, ptc.BeginDate) = {year}\r\n" +
                           $"AND DATEPART(WEEK, ptc.BeginDate) = {week}\r\n";
                }
                JObject objResponse = LabgeDatabase.SqlToJObject(sql);

                sql = $"SELECT ptc.CompSubCode, pcsc.CompSubName, ptc.BeginDate, ptc.EndDate, ptc.TestCount\r\n" +
                      $"FROM PGSTestCount ptc\r\n" +
                      $"JOIN ProgCompSubCode pcsc\r\n" +
                      $"ON pcsc.CompCode = '70096'\r\n" +
                      $"AND ptc.CompSubCode = pcsc.CompSubCode\r\n";
                if (week == 0)
                {                    
                    sql += $"WHERE DATEPART(YEAR, BeginDate) = DATEPART(YEAR, GETDATE())\r\n" +
                           $"AND DATEPART(WEEK, BeginDate) = DATEPART(WEEK, GETDATE())\r\n";
                }
                else
                {
                    sql += $"WHERE DATEPART(YEAR, ptc.BeginDate) = {year}\r\n" +
                           $"AND DATEPART(WEEK, ptc.BeginDate) = {week}\r\n";
                }

                sql += $"ORDER BY BeginDate\r\n";

                JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

                objResponse.Add("TestCount", arrResponse);

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

        [Route("api/StrategyBusiness/ManageLotteGenoPac/TestCount")]
        public IHttpActionResult PutTestCount([FromBody]JObject request)
        {
            try
            {
                string sql;

                sql = $"UPDATE PGSTestCount\r\n" +
                      $"SET TestCount = {request["TestCount"].ToString()}\r\n" +
                      $"WHERE CompCode = '70096'\r\n" +
                      $"AND CompSubCode = '{request["CompSubCode"].ToString()}'\r\n" +
                      $"AND BeginDate = '{request["BeginDate"].ToString()}'\r\n" +
                      $"AND EndDate = '{request["EndDate"].ToString()}'";
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

        #region RC사번 관리

        /// <summary>
        /// RC 조회
        /// </summary>
        /// <returns></returns>
        [Route("api/StrategyBusiness/ManageLotteGenoPac/RCInfo")]
        public IHttpActionResult GetRCInfo()
        {
            try
            {
                string sql;
                sql = $"SELECT pce.EmpCode, pce.EmpName, pce.Password, pce.CompSubCode\r\n" +
                      $"     , pcsc.CompSubName\r\n" +
                      $"FROM PGSCustomerEmployee pce\r\n" +
                      $"JOIN ProgCompSubCode pcsc\r\n" +
                      $"ON pcsc.CompCode = '70096'\r\n" +
                      $"AND pcsc.CompSubCode = pce.CompSubCode\r\n" +
                      $"WHERE pce.CustomerCode = 'lotte'\r\n" +
                      $"ORDER BY pce.CompSubCode\r\n";

                JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

                return Ok(arrResponse);
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
        /// RC 등록
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("api/StrategyBusiness/ManageLotteGenoPac/RCInfo")]
        public IHttpActionResult PostRCInfo(JObject request)
        {
            try
            {
                string sql;
                sql = $"INSERT INTO PGSCustomerEmployee\r\n" +
                      $"(\r\n" +
                      $"    CustomerCode, EmpCode, EmpName, Password, CompSubCode\r\n" +
                      $")\r\n" +
                      $"VALUES\r\n" +
                      $"(\r\n" +
                      $"    'lotte'\r\n" +
                      $"  , '{request["EmpCode"].ToString()}'\r\n" +
                      $"  , '{request["EmpName"].ToString()}'\r\n" +
                      $"  , '{request["Password"].ToString()}'\r\n" +
                      $"  , '{request["CompSubCode"].ToString()}'\r\n" +
                      $")";
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
        /// 비밀번호 업데이트
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("api/StrategyBusiness/ManageLotteGenoPac/RCInfo")]
        public IHttpActionResult PutRCInfo(JObject request)
        {
            try
            {
                string sql;
                sql = $"UPDATE PGSCustomerEmployee\r\n" +
                      $"SET Password = '{request["Password"].ToString()}'\r\n" +
                      $"WHERE CustomerCode = 'lotte'\r\n" +
                      $"AND EmpCode = '{request["EmpCode"].ToString()}'";

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

        [Route("api/StrategyBusiness/ManageLotteGenoPac/RCInfo")]
        public IHttpActionResult DeleteRCInfo(string empCode)
        {
            try
            {
                string sql;
                sql = $"DELETE FROM PGSCustomerEmployee\r\n" +
                      $"WHERE CustomerCode = 'lotte'\r\n" +
                      $"AND EmpCode = '{empCode}'";
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
        #endregion RC사번 관리

        [Route("api/StrategyBusiness/ManageLotteGenoPac/CompSubCode")]
        public IHttpActionResult GetCompSubCode()
        {
            try
            {
                string sql;
                sql = $"SELECT CompSubCode, CompSubName\r\n" +
                      $"FROM ProgCompSubCode\r\n" +
                      $"WHERE CompCode = '70096'";

                JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

                return Ok(arrResponse);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }

        #region 방문 병원 관리
        [Route("api/StrategyBusiness/ManageLotteGenoPac/CompList")]
        public IHttpActionResult GetCompList()
        {
            string sql;
            sql = "SELECT pl.CompCode, pcc.CompName, pl.State, pl.City, pl.Town, IsUse\r\n" +
                  "FROM PGSCompList pl\r\n" +
                  "JOIN ProgCompCode pcc\r\n" +
                  "ON pl.CompCode = pcc.CompCode\r\n" +
                  "WHERE pl.CustomerCode = 'lotte'";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/StrategyBusiness/ManageLotteGenoPac/CompList")]
        public IHttpActionResult PostCompList(JObject request)
        {
            try
            {
                string sql;
                sql = $"INSERT INTO PGSCompList\r\n" +
                      $"(\r\n" +
                      $"    CustomerCode, CompCode, State, City, Town, IsUse\r\n" +
                      $")\r\n" +
                      $"VALUES\r\n" +
                      $"(\r\n" +
                      $"    'lotte',\r\n" +
                      $"    '{request["CompCode"].ToString()}',\r\n" +
                      $"    '{request["State"].ToString()}',\r\n" +
                      $"    '{request["City"].ToString()}',\r\n" +
                      $"    '{request["Town"].ToString()}',\r\n" +
                      $"    '{request["IsUse"].ToString()}'\r\n" +
                      $")";
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

        [Route("api/StrategyBusiness/ManageLotteGenoPac/CompList")]
        public IHttpActionResult PutCompList(JObject request)
        {
            try
            {
                string sql;
                sql = $"UPDATE PGSCompList\r\n" +
                      $"SET State = '{request["State"].ToString()}'\r\n" +
                      $"  , City = '{request["City"].ToString()}'\r\n" +
                      $"  , Town = '{request["Town"].ToString()}'\r\n" +
                      $"  , IsUse = '{request["IsUse"].ToString()}'\r\n" +
                      $"WHERE CustomerCode = 'lotte'\r\n" +
                      $"AND CompCode = '{request["CompCode"].ToString()}'";
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

        [Route("api/StrategyBusiness/ManageLotteGenoPac/CompList")]
        public IHttpActionResult DeleteCompList(string compCode)
        {
            try
            {
                string sql;
                sql = $"DELETE FROM PGSCompList\r\n" +
                      $"WHERE CompCode = '{compCode}'\r\n" +
                      $"AND CustomerCode = 'lotte'";
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
        #endregion 방문 병원 관리
    }
}