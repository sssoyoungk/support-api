using ExcelDataReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using supportsapi.labgenomics.com.Services;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/Covid19Regist")]
    public class Covid19RegistController : ApiController
    {
        // GET api/<controller>/5
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string testKind, string institutionNo = "", string regKind = "W", string compCode = "")
        {
            string sql = string.Empty;

            if (regKind == "W")
            {
                sql = $"SELECT CONVERT(bit, 0) AS ColumnCheck, GETDATE() AS LabRegDate, '' AS LabRegNo, covidOrder.CompOrderDate, covidOrder.CompOrderNo\r\n" +
                      $"     , covidOrder.CompName, covidOrder.CompName, covidOrder.CompInstitutionNo, covidOrder.SampleNo, covidOrder.PatientName, covidOrder.SampleDrawDate\r\n" +
                      $"     , covidOrder.BirthDay, covidOrder.Gender, covidOrder.TestKind, '' AS CustomCode, '' AS GroupName, CONVERT(varchar(19), covidOrder.PrintDateTime, 20) AS PrintDateTime\r\n" +
                      $"     , FLOOR(CAST(DATEDIFF(DAY, covidOrder.BirthDay, GETDATE()) AS Integer) / 365.2422) AS Age\r\n" +
                      $"     , covidOrder.PhoneNo, '' AS PatientChartNo, covidOrder.CheckDateTime\r\n" +
                      $"FROM Covid19Order AS covidOrder\r\n" +
                      $"WHERE covidOrder.CompOrderDate BETWEEN '{beginDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}'\r\n" +
                      $"AND covidOrder.TestKind = '{testKind}'\r\n";
                if (institutionNo != string.Empty)
                {
                    sql += $"AND covidOrder.CompInstitutionNo = '{institutionNo}'\r\n";
                }
                sql += $"AND covidOrder.LabRegDate IS NULL\r\n" +
                       $"AND covidOrder.LabRegNo IS NULL\r\n" +
                       $"ORDER BY PrintDateTime";
            }
            else if (regKind == "R")
            {
                sql = $"SELECT CONVERT(bit, 0) AS ColumnCheck, covidOrder.LabRegDate, covidOrder.LabRegNo, covidOrder.CompOrderDate, covidOrder.CompOrderNo\r\n" +
                      $"     , covidOrder.CompName, covidOrder.CompName, covidOrder.CompInstitutionNo, covidOrder.SampleNo, covidOrder.PatientName, covidOrder.SampleDrawDate\r\n" +
                      $"     , covidOrder.BirthDay, covidOrder.Gender, covidOrder.TestKind, '' AS CustomCode, '' AS GroupName, CONVERT(varchar(19)\r\n" +
                      $"     , covidOrder.PrintDateTime, 20) AS PrintDateTime\r\n" +
                      $"     , FLOOR(CAST(DATEDIFF(DAY, covidOrder.BirthDay, GETDATE()) AS Integer) / 365.2422) AS Age\r\n" +
                      $"     , covidOrder.PhoneNo, '' AS PatientChartNo\r\n" +
                      $"FROM Covid19Order covidOrder\r\n" +
                      $"JOIN LabRegInfo lri\r\n" +
                      $"ON covidOrder.LabRegDate = lri.LabRegDate\r\n" +
                      $"AND covidOrder.LabRegNo = lri.LabRegNo\r\n";
                if (institutionNo != string.Empty)
                {
                    sql += $"AND covidOrder.CompInstitutionNo = '{institutionNo}'\r\n";
                }
                sql += $"AND lri.CompCode = '{compCode}'\r\n" +
                       $"WHERE covidOrder.CompOrderDate BETWEEN '{beginDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}'\r\n" +
                       $"AND covidOrder.LabRegDate IS NOT NULL\r\n" +
                       $"AND covidOrder.LabRegNo IS NOT NULL\r\n" +
                       $"AND covidOrder.TestKind = '{testKind}'\r\n";
            }

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        public IHttpActionResult Post([FromBody] JObject request)
        {
            string sql;
            sql = $"UPDATE Covid19Order\r\n" +
                  $"SET LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]):yyyy-MM-dd}'\r\n" +
                  $"  , LabRegNo = '{request["LabRegNo"]}'\r\n" +
                  $"WHERE SampleNo = '{request["SampleNo"]}'";
            LabgeDatabase.ExecuteSql(sql);


            if (request["TestKind"].ToString() == "개별검사")
            {
                sql = $"UPDATE LabRegInfo\r\n" +
                      $"SET CenterCode = 'Covid19Excel'\r\n" +
                      $"  , IsTrustOrder = 1\r\n" +
                      $"  , SystemUniqID = '{request["SampleNo"]}'\r\n" +
                      $"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]):yyyy-MM-dd}'\r\n" +
                      $"AND LabRegNo = {request["LabRegNo"]}";
                LabgeDatabase.ExecuteSql(sql);
            }
            else if (request["TestKind"].ToString() == "취합검사")
            {
                sql = $"UPDATE LabRegInfo\r\n" +
                      $"SET CenterCode = 'Covid19Excel'\r\n" +
                      $"  , IsTrustOrder = 1\r\n" +
                      $"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]):yyyy-MM-dd}'\r\n" +
                      $"AND LabRegNo = {request["LabRegNo"]}";
                LabgeDatabase.ExecuteSql(sql);

                sql = $"MERGE INTO LabRegCustom AS target\r\n" +
                      $"USING (SELECT '{Convert.ToDateTime(request["LabRegDate"]):yyyy-MM-dd}' AS LabRegDate,\r\n" +
                      $"              {request["LabRegNo"]} AS LabRegNo, '{request["CustomCode"]}' AS CustomCode) AS source\r\n" +
                      $"ON target.LabRegDate = source.LabRegDate AND target.LabRegNo = source.LabRegNo AND target.CustomCode = source.CustomCode\r\n" +
                      $"WHEN NOT MATCHED THEN\r\n" +
                      $"    INSERT\r\n" +
                      $"    (\r\n" +
                      $"        LabRegDate, LabRegNo, CustomCode, CustomValue01, CustomValue02,\r\n" +
                      $"        CustomBgColor, CustomFontColor, RegistMemberID\r\n" +
                      $"    )\r\n" +
                      $"    VALUES\r\n" +
                      $"    (\r\n" +
                      $"        '{Convert.ToDateTime(request["LabRegDate"]):yyyy-MM-dd}', '{request["LabRegNo"]}',\r\n" +
                      $"        '{request["CustomCode"]}', '{request["CustomValue01"]}',\r\n" +
                      $"        '{request["CustomValue02"]}', (SELECT CustomBgColor FROM LabCustomCode WHERE CustomCode = '{request["CustomCode"]}'),\r\n" +
                      $"         (SELECT CustomFontColor FROM LabCustomCode WHERE CustomCode = '{request["CustomCode"]}'), '{request["MemberID"]}'\r\n" +
                      $"    )\r\n" +
                      $"WHEN MATCHED THEN\r\n" +
                      $"    UPDATE\r\n" +
                      $"    SET CustomValue01 = '{request["CustomValue01"]}'\r\n" +
                      $"      , CustomValue02 = '{request["CustomValue02"]}'\r\n" +
                      $";\r\n";

                LabgeDatabase.ExecuteSql(sql);
            }

            return Ok();
        }

        /// <summary>
        /// 보건소 목록
        /// </summary>
        /// <returns></returns>
        [Route("api/Sales/Covid19Regist/PublicHealthCenter")]
        public IHttpActionResult GetPublicHealthCenter(string authGroup)
        {
            string sql;
            sql = $"SELECT DISTINCT CompName, CompInstitutionNo\r\n" +
                  $"FROM Covid19Order\r\n" +
                  $"WHERE CompInstitutionNo IN\r\n" +
                  $"(\r\n" +
                  $"    SELECT CompInstitutionNo\r\n" +
                  $"    FROM ProgCompCode pcc\r\n" +
                  $"    JOIN ProgAuthGroupAccessComp pagac\r\n" +
                  $"    ON pcc.CompCode = pagac.CompCode\r\n" +
                  $"    WHERE pagac.AuthGroupCode = '{authGroup}'\r\n" +
                  $")\r\n" +
                  $"AND CompInstitutionNo <> ''\r\n" +
                  $"ORDER BY CompInstitutionNo\r\n";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);

            return Ok(arrResponse);
        }

        [Route("api/Sales/Covid19Regist/CompCode")]
        public IHttpActionResult GetCompCode(string authGroup, string institutionNo = "")
        {
            string sql;

            if (institutionNo != string.Empty)
            {
                sql = "SELECT CompCode, CompName, CompInstitutionNo\r\n" +
                      $"FROM ProgCompCode\r\n" +
                      $"WHERE CompInstitutionNo = '{institutionNo}'\r\n" +
                      $"ORDER BY CompCode";
            }
            else
            {
                sql = $"SELECT CompCode, CompName, CompInstitutionNo\r\n" +
                      $"FROM ProgCompCode\r\n" +
                      $"WHERE CompCode IN\r\n" +
                      $"(\r\n" +
                      $"    SELECT CompCode\r\n" +
                      $"    FROM ProgAuthGroupAccessComp\r\n" +
                      $"    WHERE AuthGroupCode = '{authGroup}'\r\n" +
                      $")\r\n" +
                      $"AND IsCompUseCode = 1\r\n" +
                      $"AND CompName LIKE '%보건소%'\r\n" +
                      $"ORDER BY CompCode\r\n";
            }

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        /// <summary>
        /// 기등록 매치
        /// </summary>
        /// <param name="testKind"></param>
        /// <param name="compCode"></param>
        /// <param name="labRegDate"></param>
        /// <param name="birthDay"></param>
        /// <param name="patientName"></param>
        /// <returns></returns>
        [Route("api/Sales/Covid19Regist/PersonMatch")]
        public IHttpActionResult GetPersonMatch(string testKind, string compCode, DateTime labRegDate, DateTime birthDay, string patientName)
        {
            string sql ;

            JArray arrResponse = new JArray();
            if (testKind == "개별검사")
            {
                sql = 
                    $"SELECT LabRegNo\r\n" +
                    $"FROM LabRegInfo\r\n" +
                    $"WHERE LabRegDate = '{labRegDate:yyyy-MM-dd}'\r\n" +
                    $"AND PatientName = '{patientName}'\r\n" +
                    $"AND PatientJuminNo01 = '{birthDay:yyMMdd}'\r\n" +
                    $"AND CompCode = '{compCode}'";
                arrResponse = LabgeDatabase.SqlToJArray(sql);
            }
            else if (testKind == "취합검사")
            {
                sql =
                    $"SELECT lri.LabRegNo, lrc.CustomCode\r\n" +
                    $"FROM LabRegInfo lri\r\n" +
                    $"JOIN LabRegCustom lrc\r\n" +
                    $"ON lrc.LabRegDate = lri.LabRegDate\r\n" +
                    $"AND lrc.LabRegNo = lri.LabRegNo\r\n" +
                    $"AND lrc.CustomValue01 = '{patientName}'\r\n" +
                    $"AND SUBSTRING(lrc.CustomValue02, 1, 6) = '{birthDay:yyMMdd}'\r\n" +
                    $"WHERE lri.LabRegDate = '{labRegDate:yyyy-MM-dd}'\r\n" +
                    $"AND CompCode = '{compCode}'";
                arrResponse = LabgeDatabase.SqlToJArray(sql);

                //취합 개별의 경우로 개별접수 건을 확인한다.
                if (arrResponse.Count == 0)
                {
                    sql = 
                        $"SELECT LabRegNo, '' AS CustomCode\r\n" +
                        $"FROM LabRegInfo\r\n" +
                        $"WHERE LabRegDate = '{labRegDate:yyyy-MM-dd}'\r\n" +
                        $"AND PatientName = '{patientName}'\r\n" +
                        $"AND PatientJuminNo01 = '{birthDay:yyMMdd}'\r\n" +
                        $"AND CompCode = '{compCode}'";
                    arrResponse = LabgeDatabase.SqlToJArray(sql);
                }
            }

            return Ok(arrResponse);
        }

        [Route("api/Sales/Covid19Regist/PersonMatchUsingPhone")]
        public IHttpActionResult GetPersonMatchUsingPhone(string testKind, string compCode, DateTime labRegDate, string phoneNumber, string patientName)
        {
            string sql;

            JArray arrResponse = new JArray();
            if (testKind == "개별검사")
            {
                sql = $"SELECT LabRegNo\r\n" +
                      $"FROM LabRegInfo\r\n" +
                      $"WHERE LabRegDate = '{labRegDate:yyyy-MM-dd}'\r\n" +
                      $"AND PatientName = '{patientName}'\r\n" +
                      $"AND SystemUniqID = '{phoneNumber.Replace("-", "")}'\r\n" +
                      $"AND CompCode = '{compCode}'";
                arrResponse = LabgeDatabase.SqlToJArray(sql);
            }
            else if (testKind == "취합검사")
            {
                sql = $"SELECT lri.LabRegNo, lrc.CustomCode\r\n" +
                      $"FROM LabRegInfo lri\r\n" +
                      $"JOIN LabRegCustom lrc\r\n" +
                      $"ON lrc.LabRegDate = lri.LabRegDate\r\n" +
                      $"AND lrc.LabRegNo = lri.LabRegNo\r\n" +
                      $"AND lrc.CustomValue01 = '{patientName}'\r\n" +
                      $"AND lrc.CustomValue02 LIKE '%{phoneNumber.Replace("-", "")}%'\r\n" +
                      $"WHERE lri.LabRegDate = '{labRegDate:yyyy-MM-dd}'\r\n" +
                      $"AND CompCode = '{compCode}'";
                arrResponse = LabgeDatabase.SqlToJArray(sql);

                //취합 개별의 경우로 개별접수 건을 확인한다.
                if (arrResponse.Count == 0)
                {
                    sql = $"SELECT LabRegNo, '' AS CustomCode\r\n" +
                          $"FROM LabRegInfo\r\n" +
                          $"WHERE LabRegDate = '{labRegDate:yyyy-MM-dd}'\r\n" +
                          $"AND PatientName = '{patientName}'\r\n" +
                          $"AND SystemUniqID = '{phoneNumber.Replace("-", "")}'\r\n" +
                          $"AND CompCode = '{compCode}'";
                    arrResponse = LabgeDatabase.SqlToJArray(sql);
                }
            }

            return Ok(arrResponse);
        }

        /// <summary>
        /// 기등록 저장할 때 CustomCode 비어있으면 찾기
        /// </summary>
        /// <param name="labregDate"></param>
        /// <param name="labRegNo"></param>
        /// <returns></returns>
        [Route("api/Sales/Covid19Regist/MatchCustomCode")]
        public IHttpActionResult GetMatchCustomCode(DateTime labregDate, int labRegNo, string patientName)
        {
            string sql;
            sql =
                $"SELECT CustomCode\r\n" +
                $"FROM LabRegCustom\r\n" +
                $"WHERE LabRegDate = '{labregDate:yyyy-MM-dd}'\r\n" +
                $"AND LabRegNo = {labRegNo}\r\n" +
                $"AND CustomValue01 = '{patientName}'\r\n" +
                $"AND ISNULL(CustomCode, '') <> ''";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);

            return Ok(arrResponse);
        }

        [Route("api/Sales/Covid19Regist/Excel")]
        public async Task<HttpResponseMessage> PostUploadFile()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                string root = HttpContext.Current.Server.MapPath("/" + @"LabImportFile/Covid19RegistFiles");
                Directory.CreateDirectory(root);
                var provider = new MultipartFormDataStreamProvider(root);

                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (MultipartFileData file in provider.FileData)
                {
                    var dataBytes = File.ReadAllBytes(file.LocalFileName);
                    var dataStream = new MemoryStream(dataBytes);
                    IExcelDataReader reader = ExcelReaderFactory.CreateReader(dataStream);
                    var conf = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    };
                    var dataSet = reader.AsDataSet(conf);
                    DataTable dt = dataSet.Tables[0];
                    JArray arrOrders = JArray.Parse(JsonConvert.SerializeObject(dt));
                    foreach (JObject objOrder in arrOrders)
                    {
                        string sql;                        
                        sql = $"MERGE INTO Covid19Order AS target\r\n" +
                              $"USING (SELECT '{objOrder["검체번호"]}' AS SampleNo) AS source\r\n" +
                              $"ON target.SampleNo = source.SampleNo\r\n" +
                              $"WHEN NOT MATCHED THEN\r\n" +
                              $"    INSERT\r\n" +
                              $"    (\r\n" +
                              $"        FileName, CompOrderDate, CompOrderNo, SampleDrawDate, CompName,\r\n" +
                              $"        CompInstitutionNo, SampleNo, PatientName, BirthDay, Gender, RegistKind, TestTargetKind, TestKind, PrintDateTime,\r\n" +
                              $"        PhoneNo, CheckDateTime, Description, InterfaceKind\r\n" +
                              $"    )\r\n" +
                              $"    VALUES\r\n" +
                              $"    (\r\n" +
                              $"        '{Path.GetFileName(file.LocalFileName)}', '{Convert.ToDateTime(objOrder["의뢰일시"]):yyyy-MM-dd}',\r\n" +
                              $"        '{objOrder["접수번호"]}', '{objOrder["검체채취일"]}',\r\n" +
                              $"        '{objOrder["의뢰기관 이름"]}', '{objOrder["의뢰기관 아이디"]}', '{objOrder["검체번호"]}',\r\n" +
                              $"        '{objOrder["검사대상자 이름"]}', '{objOrder["생년월일"]}',\r\n" +
                              $"        CASE WHEN '{objOrder["성별"]}' = '남' THEN 'M' WHEN '{objOrder["성별"]}' = '여' THEN 'F' ELSE '' END,\r\n" +
                              $"        '{objOrder["등록구분"]}', '{objOrder["선제검사\n대상유형"]}', '{objOrder["검사구분"]}',\r\n" +
                              $"        '{objOrder["검사의뢰서 출력일시"]}',\r\n" +
                              $"        '{objOrder["환자 연락처"]}', '{objOrder["확인일시"]}', '{(objOrder["비고"] ?? string.Empty).ToString().Replace("'", "''")}',\r\n" +
                              $"        'Covid19Excel'\r\n" +
                              $"    )\r\n" +
                              $"WHEN MATCHED THEN\r\n" +
                              $"    UPDATE\r\n" +
                              $"    SET TestKind = '{objOrder["검사구분"]}'\r\n" +
                              $";";

                        LabgeDatabase.ExecuteSql(sql);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, objResponse.ToString());
            }
        }

        /// <summary>
        /// API 오더 등록
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [Route("api/Sales/Covid19Regist/API")]
        public IHttpActionResult PostAPIOrder([FromBody] JObject request)
        {
            Covid19 covid19 = new Covid19();
            covid19.RefreshAPIKey();

            DateTime beginDate = Convert.ToDateTime(request["BeginDate"]);
            DateTime endDate = Convert.ToDateTime(request["EndDate"]);
            string institutionNo = request["InstitutionNo"].ToString();

            var objResponse = GetOrder(beginDate, endDate, institutionNo);
            if (objResponse["resultCd"].ToString() == "R0200")
            {
                JArray arrRequest = JArray.Parse(objResponse["irList"].ToString());
                InsertTable(arrRequest);
                int restCnt = Convert.ToInt32(objResponse["restCnt"]);

                while (restCnt > 0)
                {
                    objResponse = GetOrder(beginDate, endDate, institutionNo);
                    arrRequest = JArray.Parse(objResponse["irList"].ToString());
                    restCnt = Convert.ToInt32(objResponse["restCnt"]);
                    InsertTable(arrRequest);
                }
            }
            else
            {
                throw new Exception(objResponse["apiErrCd"].ToString());
            }
            return Ok();
        }

        private JObject GetOrder(DateTime beginDate, DateTime endDate, string institutionNo)
        {
            Covid19 covid19 = new Covid19();
            JObject objRequest = covid19.GetAPIKey();

            objRequest.Add("reqestBeginDe", beginDate.ToString("yyyyMMdd"));
            objRequest.Add("reqestEndDe", endDate.ToString("yyyyMMdd"));
            objRequest.Add("inspctSe", "");
            objRequest.Add("registSe", "");
            objRequest.Add("reqestInsttCd", institutionNo);
            string apiUrl = "https://covid19.kdca.go.kr/api/pi/getIrListRqed";
            var client = new RestClient(apiUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", objRequest, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JObject.Parse(response.Content);
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

        private void InsertTable(JArray arrRequest)
        {

            foreach (JObject request in arrRequest)
            {
                try
                {
                    string registSe = string.Empty;
                    if (request["registSe"].ToString() == "IR")
                        registSe = "환자감시/보고";
                    else if (request["registSe"].ToString() == "SC")
                        registSe = "진료,검사소/선별진료소";
                    else if (request["registSe"].ToString() == "TI")
                        registSe = "진료,검사소/임시선별검사소";
                    else if (request["registSe"].ToString() == "PI")
                        registSe = "진료,검사소/선제검사소";
                    else
                        registSe = request["registSe"].ToString();

                    string inspctSe = string.Empty;
                    if (request["inspctSe"].ToString() == "1")
                        inspctSe = "개별검사";
                    else if (request["inspctSe"].ToString() == "2")
                        inspctSe = "취합검사";
                    else if (request["inspctSe"].ToString() == "3")
                        inspctSe = "동시진단검사";
                    else
                        inspctSe = request["inspctSe"].ToString();

                    string sql;
                    sql = $"INSERT INTO Covid19Order\r\n";
                    string strFormDt = !DateTime.TryParseExact(request["reqestFormDt"].ToString(), "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime formDt) ? "" : formDt.ToString();
                    string strAcceptDt = !DateTime.TryParseExact(request["acceptDt"].ToString(), "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime acceptDt) ? "" : acceptDt.ToString();

                    sql += $"(\r\n";
                    sql += $"    CompOrderDate, CompOrderNo, SampleDrawDate, CompName, CompInstitutionNo, SampleNo, PatientName, BirthDay, Gender\r\n";
                    sql += $"  , RegistKind, TestKind, PrintDateTime, InterfaceKind, PhoneNo, Description, CompSubName, CompAcceptDateTime\r\n";
                    sql += $")\r\n";
                    sql += $"VALUES\r\n";
                    sql += $"(\r\n";
                    sql += $"    '{Convert.ToDateTime(request["reqestDt"]):yyyy-MM-dd}'\r\n";
                    sql += $"  , '{request["spmNo"]}'\r\n";
                    sql += $"  , '{DateTime.ParseExact(request["spmPickDe"].ToString(), "yyyyMMdd", null):yyyy-MM-dd}'\r\n";
                    sql += $"  , '{request["reqestInsttNm"]}'\r\n";
                    sql += $"  , '{request["reqestInsttCd"]}'\r\n";
                    sql += $"  , '{request["spmNo"]}'\r\n";
                    sql += $"  , '{request["patntNm"]}'\r\n";
                    sql += $"  , '{DateTime.ParseExact(request["brthdy"].ToString(), "yyyyMMdd", null):yyyy-MM-dd}'\r\n";
                    sql += $"  , '{((request["sexdstn"].ToString() == "1") ? "M" : "F")}'\r\n";
                    sql += $"  , '{registSe}'\r\n";
                    sql += $"  , '{inspctSe}'\r\n";
                    sql += $"  , '{strFormDt}'\r\n";
                    sql += $"  , 'Covid19API'\r\n";
                    sql += $"  , '{request["patntTelno"]}'\r\n";
                    sql += $"  , '{request["medicalQuestions"]}'\r\n";
                    sql += $"  , '{request["insttSubNm"]}'\r\n";
                    sql += $"  , '{strAcceptDt}'\r\n";
                    sql += $")";

                    LabgeDatabase.ExecuteSql(sql);
                }
                catch (Exception)
                {

                }
            }
        }

        [Route("api/Sales/Covid19Regist/ChangeInfo")]
        public IHttpActionResult PutChangeInfo(JObject objRequest)
        {
            try
            {
                string sql;
                sql =
                    $"UPDATE Covid19Order\r\n" +
                    $"SET PhoneNo = '{objRequest["PhoneNo"]}'\r\n" +
                    $"WHERE SampleNo = '{objRequest["SampleNo"]}'";
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
    }
}