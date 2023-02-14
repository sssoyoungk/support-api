using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using supportsapi.labgenomics.com.Services;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/Covid19ResultExport")]
    public class Covid19ResultExportController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="beginDate">시작일자</param>
        /// <param name="endDate">종료일자</param>
        /// <param name="isNotExport">true면 생성된 적이 없는 데이터만 생성</param>
        /// <param name="isErrorResult">오류 결과 표시 여부</param>
        /// <param name="compCode">거래처 코드</param>
        /// <param name="compInstitutionNo">거래처 요양기관번호</param>
        /// <param name="isSendSMS">true로 조회하면 SMS를 발송하지 않는 항목만 조회</param>
        /// <returns></returns>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, bool isNotExport = false, bool isErrorResult = true, string compCode = "", string compInstitutionNo = "", bool isSendSMS = false)
        {
            string sql;

            sql = $"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED\r\n" +
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
                  $"     , lrc.CustomValue02, lrc.CustomCode\r\n" +
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
                  $"WHERE lri.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                  $"AND ISNULL(lrr.TestResult01, '') <> ''\r\n" +
                  $"AND lri.CenterCode IN ('Covid19Excel', 'Covid19API')\r\n";

            //울산 김태은 소장 요청으로 6597거래처는 전체조회 조건에서 제외한다.
            if (beginDate.ToString("yyyy-MM-dd") == "2021-07-26")
            {
                sql += "AND lri.CompCode <> '6597'\r\n";
            }
            
            if (Request.Headers.Contains("GroupCode"))
            {
                string groupCode = Request.Headers.GetValues("GroupCode").First();
                sql += 
                    $"AND lri.CompCode IN\r\n" +
                    $"(\r\n" +
                    $"    SELECT CompCode\r\n" +
                    $"    FROM ProgAuthGroupAccessComp\r\n" +
                    $"    WHERE AuthGroupCode = '{groupCode}'\r\n" +
                    $")\r\n";
            }

            if (isNotExport)
            {
                sql += "AND covidOrder.ExportDateTime IS NULL\r\n";
            }

            if (compCode != string.Empty)
            {
                sql += $"AND lri.CompCode = '{compCode}'\r\n";
            }

            if (compInstitutionNo != string.Empty)
            {
                sql += $"AND covidOrder.CompInstitutionNo = '{compInstitutionNo}'\r\n";
            }

            if (isSendSMS)
            {
                sql += $"AND ISNULL(covidOrder.IsSendSMS, 0) = 0\r\n";
            }

            DataTable dt = LabgeDatabase.SqlToDataTable(sql);

            //컬럼추가
            dt.Columns.Add("RdRp", typeof(string));
            dt.Columns.Add("Ngene", typeof(string));
            dt.Columns.Add("ReTestResult", typeof(string));
            dt.Columns.Add("ReTestRdRp", typeof(string));
            dt.Columns.Add("ReTestNgene", typeof(string));
            dt.Columns.Add("ReTestLabRegReportID", typeof(string));

            foreach (DataRow dr in dt.Rows)
            {
                //개별검사면서 Positive나 Inconclusive이면 텍스트 결과 가져옴
                if (new[] { "1", "3" }.Contains(dr["Result"].ToString()))
                {
                    sql = $"SELECT RdRp, Ngene FROM dbo.FN_GetCovidTextResult('{Convert.ToDateTime(dr["LabRegDate"]).ToString("yyyy-MM-dd")}', '{dr["LabRegNo"].ToString()}')";
                    JObject objTestResult = LabgeDatabase.SqlToJObject(sql);
                    if (objTestResult.HasValues)
                    {
                        dr["RdRp"] = objTestResult["RdRp"].ToString();
                        dr["Ngene"] = objTestResult["Ngene"].ToString();
                    }
                }
                //취합검사면서 Positive거나 Inconclusive의 경우는 2차도 가져옴
                if (dr["TestKind"].ToString() == "2" && new[] { "1", "3" }.Contains(dr["Result"].ToString()))
                {
                    sql = $"SELECT LabRegReportID, Result, RdRp, Ngene FROM dbo.FN_GetCovidReTestResult" +
                          $"('{Convert.ToDateTime(dr["LabRegDate"]).ToString("yyyy-MM-dd")}', '{dr["LabRegNo"].ToString()}', '{dr["SampleNo"].ToString()}')";
                    JObject objReTestResult = LabgeDatabase.SqlToJObject(sql);
                    if (objReTestResult.HasValues)
                    {
                        dr["ReTestLabRegReportID"] = objReTestResult["LabRegReportID"].ToString();
                        dr["ReTestResult"] = objReTestResult["Result"].ToString();
                        dr["ReTestRdRp"] = objReTestResult["RdRp"].ToString();
                        dr["ReTestNgene"] = objReTestResult["Ngene"].ToString();
                    }
                }
            }

            //에러 체크
            foreach (DataRow dr in dt.Rows)
            {
                //선제개별검사 + 결과 Positive or Inconclusive + 선제검사의 경우 && dr["Result"].ToString() == "1"
                if (dr["TestCode"].ToString() == "22053" && dr["TestKind"].ToString() == "2" && new[] { "1", "3" }.Contains(dr["Result"].ToString()))
                {
                    dr["ReTestResult"] = dr["Result"].ToString();
                    dr["ReTestRdRp"] = dr["RdRp"].ToString();
                    dr["RdRp"] = string.Empty;
                    dr["ReTestNgene"] = dr["Ngene"].ToString();
                    dr["Ngene"] = string.Empty;
                }
            }

            if (isErrorResult)
            {
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    if (dt.Rows[i]["TestKind"].ToString() == "2" &&
                        new[] { "", "1", "3" }.Contains(dt.Rows[i]["Result"].ToString()) &&
                        dt.Rows[i]["ReTestResult"].ToString() == string.Empty)
                    {
                        dt.Rows[i].Delete();
                        continue;
                    }

                    //선제검사면서 LabRegCustom에 일치하는 값이 없을 경우
                    if (dt.Rows[i]["TestKind"].ToString() == "2" && dt.Rows[i]["CustomValue02"].ToString() == string.Empty)
                    {
                        dt.Rows[i].Delete();
                        continue;
                    }
                }
                dt.AcceptChanges();
            }
            dt.Columns.Remove("CustomValue02");

            JArray arrResponse = JArray.Parse(JsonConvert.SerializeObject(dt));

            return Ok(arrResponse);
        }

        /// <summary>
        /// 결과 생성 후 결과 생성일자 업데이트
        /// </summary>
        /// <param name="objRequest"></param>
        /// <returns></returns>
        public IHttpActionResult Put([FromBody]JObject objRequest)
        {
            try
            {
                string sql;
                //임시 테이블 생성
                sql = "CREATE TABLE #Covid19SampleNo\r\n" +
                      "(\r\n" +
                      "    SampleNo varchar(30) COLLATE Korean_Wansung_CI_AS\r\n" +
                      ")\r\n";

                foreach (JObject objSampleNo in objRequest["Samples"])
                {
                    sql += $"INSERT INTO #Covid19SampleNo SELECT '{objSampleNo["SampleNo"].ToString()}'\r\n";
                }

                sql += $"UPDATE Covid19Order\r\n" +
                       $"SET ExportDateTime = GETDATE()\r\n" +
                       $"  , ExportMemberID = '{objRequest["MemberID"].ToString()}'\r\n" +
                       $"WHERE SampleNo IN (SELECT SampleNo FROM #Covid19SampleNo)\r\n" +
                       $"\r\n" +
                       $"DROP TABLE #Covid19SampleNo";

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

        [Route("api/Sales/Covid19ResultExport/ErrorCheck")]
        public IHttpActionResult GetErrorResult(DateTime beginDate, DateTime endDate)
        {
            string sql = string.Empty;

            sql += $"SELECT \r\n" +
                   $"    lrr.LabRegDate, lrr.LabRegNo, lrr.TestResult01, lrp.ReportMemo, co.SampleNo, co.ExportDateTime\r\n" +
                   $"FROM LabRegResult lrr\r\n" +
                   $"JOIN LabRegReport lrp\r\n" +
                   $"ON lrr.LabRegDate = lrp.LabRegDate\r\n" +
                   $"AND lrr.LabRegNo = lrp.LabRegNo\r\n" +
                   $"AND lrp.ReportMemo IS NULL\r\n" +
                   $"AND lrp.ReportStateCode = 'F'\r\n" +
                   $"JOIN Covid19Order co\r\n" +
                   $"ON co.LabRegDate = lrr.LabRegDate\r\n" +
                   $"AND co.LabRegNo = lrr.LabRegNo\r\n" +
                   $"AND co.TestKind = '취합검사'\r\n" +
                   $"AND co.ExportDateTime IS NULL\r\n" +
                   $"WHERE lrr.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                   $"AND lrr.TestCode IN ('22036', '22053', '22062', '22063', '22064', '22065')\r\n" +
                   $"AND lrr.TestResult01 <> 'Negative'\r\n" +
                   $"ORDER BY LabRegDate, LabRegNo\r\n";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        /// <summary>
        /// 질청 API 결과 등록
        /// </summary>
        /// <param name="objRequest"></param>
        /// <returns></returns>
        [Route("api/Sales/Covid19ResultExport/API")]
        public IHttpActionResult PostAPIRegistResult(JObject objRequest)
        {
            string sql = string.Empty;

            string covidUrl = "https://covid19.kdca.go.kr/api/pi/setIrResultList";

            Covid19 covid19 = new Covid19();
            JObject objResult = covid19.GetAPIKey();
            objResult.Add("irResultList", JArray.Parse(objRequest["irResultList"].ToString()));

            var client = new RestClient(covidUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", objResult, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                JObject objResponse = JObject.Parse(response.Content);
                if (objResponse["resultCd"].ToString() == "R0200")
                {
                    JArray arrFails = JArray.Parse(objResponse["irFailrList"].ToString());
                    //등록 실패는 서버에 업데이트
                    foreach (JObject objFail in arrFails)
                    {
                        sql = $"UPDATE Covid19Order\r\n" +
                              $"SET APIErrorCode = '{objFail["failrCd"].ToString()}'\r\n" +
                              $"  , ExportDateTime = GETDATE()\r\n" +
                              $"  , ExportMemberID = '{objRequest["MemberID"].ToString()}'\r\n" +
                              $"WHERE SampleNo = '{objFail["spmNo"].ToString()}'";
                        LabgeDatabase.ExecuteSql(sql);
                    }

                    //결과 등록 성공하면 결과 확정처리도 한다.
                    covid19.Covid19ResultConfirm(objResult, objResponse, objRequest["MemberID"].ToString());
                }
                //API키 오류 발생하면 API키를 갱신
                else if (objResponse["resultCd"].ToString() == "R0402")
                {
                    covid19.RefreshAPIKey();
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

            return Ok();
        }

        [Route("api/Sales/Covid19ResultExport/CheckSendSMS")]
        public IHttpActionResult PutCheckSendSMS([FromBody]JObject request)
        {
            try
            {
                string sql;
                sql = $"UPDATE Covid19Order\r\n" +
                      $"SET IsSendSMS = 1\r\n" +
                      $"WHERE SampleNo = '{request["SampleNo"].ToString()}'";
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

        [Route("api/Sales/Covid19ResultExport/CompCode")]
        public IHttpActionResult GetCompCode(string groupCode)
        {
            string sql;
            sql =
                $"SELECT pcc.CompCode, pcc.CompName, pcc.CompInstitutionNo\r\n" +
                $"FROM ProgCompCode pcc\r\n" +
                $"JOIN ProgAuthGroupAccessComp pagac\r\n" +
                $"ON pagac.AuthGroupCode = '{groupCode}'\r\n" +
                $"AND pagac.CompCode = pcc.CompCode\r\n" +
                $"WHERE CompInstitutionNo IN\r\n" +
                $"(\r\n" +
                $"    SELECT DISTINCT CompInstitutionNo\r\n" +
                $"    FROM Covid19Order\r\n" +
                $"    WHERE CompInstitutionNo <> ''\r\n" +
                $")\r\n" +
                $"ORDER BY CompCode\r\n";
            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);

        }
    }
}