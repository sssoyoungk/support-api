using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    //[SupportsAuth]
    [Route("api/Sales/Covid19SMSForm")]
    public class Covid19SMSFormController : ApiController
    {
        /// <summary>
        /// 개별 반환
        /// </summary>
        /// <param name="sampleNo"></param>
        /// <returns></returns>
        public IHttpActionResult GetObject(string sampleNo)
        {
            //SMS전송문구 불러옴
            string sql;
            sql =
                $"SELECT\r\n" +
                $"    co.PatientName, co.PhoneNo, co.BirthDay," +
                $"    CASE WHEN co.Gender = 'M' THEN '남' WHEN co.Gender = 'F' THEN '여' ELSE '' END AS Gender, lrt.TestCode,\r\n" +
                $"    co.LabRegDate, co.LabRegNo, csf.SendPhoneNo, csf.BirthDayFormat, csf.LabRegDateFormat,\r\n" +
                $"    csf.NegativeMessage, csf.PositiveMessage, csf.InconclusiveMessage,\r\n" +
                $"    CASE co.TestKind WHEN '개별검사' THEN '1' WHEN '취합검사' THEN '2' WHEN '동시진단검사' THEN '3' END TestKind,\r\n" +
                $"    CASE lrr.TestResult01\r\n" +
                $"        WHEN 'Negative' THEN '2'\r\n" +
                $"        WHEN 'Positive' THEN '1'\r\n" +
                $"        WHEN '개별검사시행' THEN\r\n" +
                $"            CASE \r\n" +
                $"                WHEN UPPER(REPLACE(REPLACE(RTRIM(LTRIM(lrp.ReportMemo)), CHAR(13), ''), CHAR(10), '')) = 'POSITIVE' THEN '1'\r\n" +
                $"                WHEN UPPER(REPLACE(REPLACE(RTRIM(LTRIM(lrp.ReportMemo)), CHAR(13), ''), CHAR(10), '')) = 'INCONCLUSIVE' THEN '3'\r\n" +
                $"            END\r\n" +
                $"        ELSE '3'\r\n" +
                $"    END AS Result,\r\n" +
                $"    reTest.Result AS ReTestResult, lrp.LabRegReportID, reTest.LabRegReportID AS ReTestReportID\r\n" +
                $"FROM Covid19Order co\r\n" +
                $"JOIN Covid19SMSForm csf\r\n" +
                $"ON co.CompInstitutionNo = csf.InstitutionNo\r\n" +
                $"JOIN LabRegTest lrt\r\n" +
                $"ON co.LabRegDate = lrt.LabRegDate\r\n" +
                $"AND co.LabRegNo = lrt.LabRegNo\r\n" +
                $"AND lrt.TestStateCode = 'F'\r\n" +
                $"JOIN LabRegResult lrr\r\n" +
                $"ON lrr.LabRegDate = co.LabRegDate\r\n" +
                $"AND lrr.LabRegNo = co.LabRegNo\r\n" +
                $"JOIN LabRegReport lrp\r\n" +
                $"ON lrr.LabRegDate = lrp.LabRegDate\r\n" +
                $"AND lrr.LabRegNo = lrp.LabRegNo\r\n" +
                $"OUTER APPLY dbo.FN_GetCovidReTestResult(co.LabRegDate, co.LabRegNo, co.SampleNo) AS reTest\r\n" +
                $"WHERE SampleNo = '{sampleNo}'\r\n";
            Debug.WriteLine(sql);
            JObject objSMSForm = LabgeDatabase.SqlToJObject(sql);
            string message = GetSMSMessage(objSMSForm);

            JObject objResponse = new JObject();
            objResponse.Add("SendNumber", objSMSForm["SendPhoneNo"].ToString());
            objResponse.Add("Message", message);

            return Ok(objResponse);
        }

        /// <summary>
        /// 배열반환 (Console 호출용)
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="institutionNo"></param>
        /// <returns></returns>
        public IHttpActionResult GetArray(DateTime beginDate, DateTime endDate, string institutionNo, string compCode = "")
        {
            //SMS전송문구 불러옴
            string sql;
            //string message = string.Empty;
            sql =
                $"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED\r\n" +
                $"SELECT ROW_NUMBER() OVER(ORDER BY lri.LabRegDate, lri.LabRegNo) AS RowNum, lri.LabRegDate, lri.LabRegNo, lrp.LabRegReportID, covidOrder.SampleNo, lrt.TestCode\r\n" +
                $"     , covidOrder.PatientName, covidOrder.PhoneNo, covidOrder.CompOrderDate, covidOrder.BirthDay, covidOrder.Gender\r\n" +
                $"     , CASE covidOrder.TestKind WHEN '개별검사' THEN '1' WHEN '취합검사' THEN '2' WHEN '동시진단검사' THEN '3' END TestKind\r\n" +
                $"     , CASE lrr.TestResult01\r\n" +
                $"           WHEN 'Negative' THEN '2'\r\n" +
                $"           WHEN 'Positive' THEN '1'\r\n" +
                $"           WHEN '개별검사시행' THEN\r\n" +
                $"               CASE \r\n" +
                $"                   WHEN UPPER(REPLACE(REPLACE(RTRIM(LTRIM(lrp.ReportMemo)), CHAR(13), ''), CHAR(10), '')) = 'POSITIVE' THEN '1'\r\n" +
                $"                   WHEN UPPER(REPLACE(REPLACE(RTRIM(LTRIM(lrp.ReportMemo)), CHAR(13), ''), CHAR(10), '')) = 'INCONCLUSIVE' THEN '3'\r\n" +
                $"               END\r\n" +
                $"           ELSE '3'\r\n" +
                $"       END AS Result\r\n" +
                $"     , lrc.CustomValue02, lrc.CustomCode, csf.SendPhoneNo, csf.BirthDayFormat, csf.LabRegDateFormat\r\n" +
                $"     , csf.NegativeMessage, csf.PositiveMessage, csf.InconclusiveMessage\r\n" +
                $"     , reTest.Result AS ReTestResult, lrp.LabRegReportID, reTest.LabRegReportID AS ReTestReportID\r\n" +
                $"FROM LabRegInfo lri\r\n" +
                $"JOIN Covid19Order covidOrder\r\n" +
                $"ON covidOrder.LabRegDate = lri.LabRegDate\r\n" +
                $"AND covidOrder.LabRegNo = lri.LabRegNo\r\n" +
                $"JOIN LabRegTest lrt\r\n" +
                $"ON lri.LabRegDate = lrt.LabRegDate\r\n" +
                $"AND lri.LabRegNo = lrt.LabRegNo\r\n" +
                $"AND lrt.TestStateCode = 'F'\r\n" +
                $"AND lrt.TestCode IN ('22036', '22053', '22062', '22063', '22064', '22065')\r\n" +
                $"JOIN LabRegResult lrr\r\n" +
                $"ON lri.LabRegDate = lrr.LabRegDate\r\n" +
                $"AND lri.LabRegNo = lrr.LabRegNo\r\n" +
                $"AND lrt.OrderCode = lrr.OrderCode\r\n" +
                $"AND lrt.TestCode = lrr.TestCode\r\n" +
                $"JOIN LabRegReport lrp\r\n" +
                $"ON lrp.LabRegDate = lri.LabRegDate\r\n" +
                $"AND lrp.LabRegNo = lri.LabRegNo\r\n" +
                $"LEFT OUTER JOIN LabRegCustom lrc\r\n" +
                $"ON lri.LabRegDate = lrc.LabRegDate\r\n" +
                $"AND lri.LabRegNo = lrc.LabRegNo\r\n" +
                $"AND lrc.CustomValue02 LIKE '%' + covidOrder.SampleNo\r\n" +
                $"OUTER APPLY dbo.FN_GetCovidReTestResult(covidOrder.LabRegDate, covidOrder.LabRegNo, covidOrder.SampleNo) AS reTest\r\n" +
                $"JOIN Covid19SMSForm csf\r\n" +
                $"ON covidOrder.CompInstitutionNo = csf.InstitutionNo\r\n" +
                $"WHERE lri.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                $"AND ISNULL(lrr.TestResult01, '') <> ''\r\n" +
                $"AND lri.CenterCode IN ('Covid19Excel', 'Covid19API')\r\n" +
                $"AND covidOrder.CompInstitutionNo = '{institutionNo}' \r\n" +
                $"AND covidOrder.IsSendSMS = 0 \r\n";

            if (compCode != null && compCode != "")
            {
                sql += $"AND lri.CompCode = '{compCode}'";
            }

            JArray arrSMSForm = LabgeDatabase.SqlToJArray(sql);

            JArray arrResponse = new JArray();
            foreach (JObject objSMSForm in arrSMSForm)
            {
                string message = GetSMSMessage(objSMSForm);
                if (message == string.Empty)
                {
                    continue;
                }
                JObject objResponse = new JObject();
                objResponse.Add("SendNumber", objSMSForm["SendPhoneNo"].ToString());
                objResponse.Add("PatientName", objSMSForm["PatientName"].ToString());
                objResponse.Add("PhoneNo", objSMSForm["PhoneNo"].ToString());
                objResponse.Add("SampleNo", objSMSForm["SampleNo"].ToString());
                objResponse.Add("Message", message);

                arrResponse.Add(objResponse);
            }
            return Ok(arrResponse);
        }

        private string GetSMSMessage(JObject objSMSForm)
        {
            string message = string.Empty;
            //개별검사
            if (objSMSForm["TestKind"].ToString() == "1")
            {
                //음성
                if (objSMSForm["Result"].ToString() == "2")
                {
                    message = objSMSForm["NegativeMessage"].ToString();
                }
                //양성
                else if (objSMSForm["Result"].ToString() == "1")
                {
                    message = objSMSForm["PositiveMessage"].ToString();
                }
                //미결정
                else if (objSMSForm["Result"].ToString() == "3")
                {
                    message = objSMSForm["InconclusiveMessage"].ToString();
                }
            }
            //취합검사
            else if (new[] { "2", "3" }.Contains(objSMSForm["TestKind"].ToString()))
            {
                //취합개별검사 예외
                if (objSMSForm["TestCode"].ToString() == "22053")
                {
                    //음성
                    if (objSMSForm["Result"].ToString() == "2")
                    {
                        message = objSMSForm["NegativeMessage"].ToString();
                    }
                    //양성
                    else if (objSMSForm["Result"].ToString() == "1")
                    {
                        message = objSMSForm["PositiveMessage"].ToString();
                    }
                    //미결정
                    else if (objSMSForm["Result"].ToString() == "3")
                    {
                        message = objSMSForm["InconclusiveMessage"].ToString();
                    }
                }
                else
                {
                    //음성
                    if (objSMSForm["Result"].ToString() == "2" || objSMSForm["ReTestResult"].ToString() == "2")
                    {
                        message = objSMSForm["NegativeMessage"].ToString();
                    }
                    //양성
                    else if (new[] { "1", "3" }.Contains(objSMSForm["Result"].ToString()) && objSMSForm["ReTestResult"].ToString() == "1")
                    {
                        message = objSMSForm["PositiveMessage"].ToString();
                    }
                    //미결정
                    else if (new[] { "1", "3" }.Contains(objSMSForm["Result"].ToString()) && objSMSForm["ReTestResult"].ToString() == "3")
                    {
                        message = objSMSForm["InconclusiveMessage"].ToString();
                    }
                }
            }

            string reportId = (objSMSForm["ReTestReportID"].ToString() != string.Empty) ? objSMSForm["ReTestReportID"].ToString() : objSMSForm["LabRegReportID"].ToString();

            message = message.Replace("#수진자명", objSMSForm["PatientName"].ToString());
            message = message.Replace("#성별", objSMSForm["Gender"].ToString());
            message = message.Replace("#생년월일", Convert.ToDateTime(objSMSForm["BirthDay"]).ToString(objSMSForm["BirthDayFormat"].ToString()));
            message = message.Replace("#등록일자", Convert.ToDateTime(objSMSForm["LabRegDate"]).ToString(objSMSForm["LabRegDateFormat"].ToString()));
            message = message.Replace("#요일", Convert.ToDateTime(objSMSForm["LabRegDate"]).ToString("ddd"));
            message = message.Replace("{reportID}", reportId);

            return message;
        }
    }
}