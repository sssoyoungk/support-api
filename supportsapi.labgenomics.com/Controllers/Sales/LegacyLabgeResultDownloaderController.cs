using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [EnableCors(origins: "*", headers: "*", methods: "*", PreflightMaxAge = 28800)]
    [Route("api/Sales/LegacyLabgeResultDownloader")]
    public class LegacyLabgeResultDownloaderController : ApiController
    {
        /// <summary>
        /// 청라여성병원
        /// </summary>
        /// <param name="dateKind"></param>
        /// <param name="compCode"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [Route("api/Sales/LegacyLabgeResultDownloader/KChartResult")]
        public IHttpActionResult GetKChartResult(string dateKind, string compCode, DateTime beginDate, DateTime endDate)
        {
            string sql;
            sql =
                "SELECT 검사ID, 접수ID, 접수일자, 시간, 환자이름, 차트번호, 성별, 나이, 생년월일, 유형, 진료실, 담당의, 병실명, 코드 \r\n " +
                "     , 명칭, 의뢰일자, 검사결과 \r\n " +
                "     , CASE WHEN ISNULL(검사결과, '') IN ('', '별지보고') THEN 서술결과 \r\n " +
                "            ELSE NULL END AS 서술결과 \r\n " +
                "     , 하이로우, 단위, 참고치, 결과주소 \r\n " +
                "  FROM ( \r\n " +
                "        SELECT B.CompSystemID AS 검사ID, B.CustomData01 AS 접수ID, CONVERT(varchar, B.PatientSampleGetTime, 23) AS 접수일자, '' AS 시간 \r\n " +
                "             , C.PatientName AS 환자이름, B.PatientChartNo AS 차트번호, B.PatientSex AS 성별, B.PatientAge AS 나이 \r\n " +
                "             , '' AS 생년월일 \r\n" +
                "             , B.CustomData02 AS 유형, B.CompDeptName AS 진료실, B.PatientDoctorName AS 담당의, B.PatientSickRoom AS 병실명 \r\n " +
                "             , B.CompOrderCode AS 코드, B.CompOrderDisplayName AS 명칭, CONVERT(varchar, B.PatientSampleGetTime, 23) AS 의뢰일자 \r\n " +
                "             , CASE WHEN A.OrderCode IN ('30168') THEN '' \r\n " +
                "                    ELSE CASE WHEN G.ReportCode IN ('20', '21', '26') THEN A.TestResult01 WHEN ISNULL(A.TestResult02, '') = '' THEN A.TestResult01 ELSE A.TestResult02 + ' (' + A.TestResult01 + ')' \r\n " +
                "                         END \r\n " +
                "               END AS 검사결과 \r\n " +
                "             , CASE WHEN A.OrderCode IN ('30168') THEN dbo.Func_Profile_TextResult(A.LabRegDate, A.LabRegNo, A.OrderCode) \r\n " +
                "                    ELSE REPLACE(dbo.FUNC_TRANS_LABRESULT_KCHART(A.LabRegDate, A.LabRegNo, G.TestModuleCode, A.TestCode, A.TestSubCode, D.IsTestHeader, A.TestResult01, A.TestResult02, A.TestResultText), '()', '') \r\n " +
                "                       + CASE WHEN G.ReportCode IN ('02', '03', '19', '28') THEN CHAR(13) + CHAR(10) + '결과보고일 : ' + CONVERT(varchar, F.ReportEndTime, 23) ELSE '' \r\n " +
                "                         END \r\n " +
                "               END AS 서술결과 \r\n " +
                "             , CASE WHEN D.IsTestHeader = '1' THEN dbo.Func_GetSetCodeCheckAbnormal(B.LabRegDate, B.LabRegNo, B.CenterOrderCode) WHEN A.TestResultAbn NOT IN ('L', 'H', '*') THEN '' ELSE A.TestResultAbn END AS 하이로우 \r\n " +
                "             , dbo.FUNC_TESTREF_UNIT(A.LabRegDate, C.PatientAge, C.PatientSex, A.TestSubCode, E.SampleCode) AS 단위 \r\n " +
                "             , dbo.FUNC_TESTREF_TEXT(A.LabRegDate, C.PatientAge, C.PatientSex, A.TestSubCode, E.SampleCode) AS 참고치 \r\n " +
                "             , CASE WHEN D.IsTestSub = '1' AND G.ReportCode IN ('20', '21', '26', '45', '46') THEN '' ELSE dbo.Func_GetReportUrl_KChart(A.LabRegDate, A.LabRegNo, A.TestCode) END AS 결과주소 \r\n " +
                "          FROM LabRegResult A \r\n " +
                "          JOIN LabImportData B \r\n " +
                "            ON B.LabRegDate = A.LabRegDate \r\n " +
                "           AND B.LabRegNo = A.LabRegNo \r\n " +
                "           AND B.CenterOrderCode = A.TestSubCode \r\n " +
                "          JOIN LabRegInfo C \r\n " +
                "            ON C.LabRegDate = A.LabRegDate \r\n " +
                "           AND C.LabRegNo = A.LabRegNo \r\n " +
                "          JOIN LabTestCode D \r\n " +
                "            ON D.TestCode = A.TestSubCode \r\n " +
                "          JOIN LabRegTest E \r\n " +
                "            ON E.LabRegDate = A.LabRegDate \r\n " +
                "           AND E.LabRegNo = A.LabRegNo \r\n " +
                "           AND E.OrderCode = A.OrderCode \r\n " +
                "           AND E.TestCode = A.TestCode \r\n " +
                "           AND E.TestStateCode = 'F' \r\n " +
                "          JOIN LabRegReport F \r\n " +
                "            ON F.LabRegDate = A.LabRegDate \r\n " +
                "           AND F.LabRegNo = A.LabRegNo \r\n " +
                "           AND F.ReportCode = D.ReportCode \r\n " +
                "          JOIN LabReportCode G \r\n " +
                "            ON G.ReportCode = F.ReportCode \r\n ";

            if (dateKind == "1")
            {
                sql += $"         WHERE A.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n";
            }
            else if (dateKind == "2")
            {
                sql += $"         WHERE E.TestEndTime >= '{beginDate.ToString("yyyy-MM-dd")}' AND E.TestEndTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')\r\n";
            }

            sql +=
                $"           AND B.CompCode = '{compCode}' \r\n " +
                $"        UNION \r\n " +
                $"         \r\n " +
                $"        SELECT DISTINCT \r\n " +
                $"               B.CompSystemID AS 검사ID, B.CustomData01 AS 접수ID, CONVERT(varchar, B.PatientSampleGetTime, 23) AS 접수일자, '' AS 시간 \r\n " +
                $"             , C.PatientName AS 환자이름, B.PatientChartNo AS 차트번호, B.PatientSex AS 성별, B.PatientAge AS 나이 \r\n " +
                $"             , '' AS 생년월일 \r\n" +
                $"             , B.CustomData02 AS 유형, B.CompDeptName AS 진료실, B.PatientDoctorName AS 담당의, B.PatientSickRoom AS 병실명 \r\n " +
                $"             , B.CompOrderCode AS 코드, B.CompOrderDisplayName AS 명칭, CONVERT(varchar, B.PatientSampleGetTime, 23) AS 의뢰일자 \r\n " +
                $"             , '' \r\n " +
                $"             , '' \r\n " +
                $"             , '' \r\n " +
                $"             , '' \r\n " +
                $"             , '' \r\n " +
                $"             , dbo.Func_GetReportUrl_KChart(A.LabRegDate, A.LabRegNo, E.TestCode) AS 결과주소 \r\n " +
                $"          FROM LabRegOrder A \r\n " +
                $"          JOIN LabImportData B \r\n " +
                $"            ON B.LabRegDate = A.LabRegDate \r\n " +
                $"           AND B.LabRegNo = A.LabRegNo \r\n " +
                $"           AND B.CenterOrderCode = A.OrderCode \r\n " +
                $"          JOIN LabRegInfo C \r\n " +
                $"            ON C.LabRegDate = A.LabRegDate \r\n " +
                $"           AND C.LabRegNo = A.LabRegNo \r\n " +
                $"          JOIN LabRegReport D \r\n " +
                $"            ON D.LabRegDate = A.LabRegDate \r\n " +
                $"           AND D.LabRegNo = A.LabRegNo \r\n " +
                $"          JOIN LabRegTest E \r\n " +
                $"            ON E.LabRegDate = A.LabRegDate \r\n " +
                $"           AND E.LabRegNo = A.LabRegNo \r\n " +
                $"           AND E.OrderCode = A.OrderCode \r\n " +
                $"          JOIN LabRegResult F \r\n " +
                $"            ON F.LabRegDate = A.LabRegDate \r\n " +
                $"           AND F.LabRegNo = A.LabRegNo \r\n " +
                $"           AND F.OrderCode = A.OrderCode \r\n ";

            if (dateKind == "1")
            {
                sql += $"         WHERE A.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n ";
            }
            else if (dateKind == "2")
            {
                sql += $"         WHERE E.TestEndTime >= '{beginDate.ToString("yyyy-MM-dd")}' AND E.TestEndTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}') \r\n ";
            }

            sql +=
                $"           AND B.CompCode = '{compCode}'\r\n" +
                $"           AND A.OrderCode IN ('19980', '19981', '30176', '30177')\r\n" +
                $"        ) GRP1 \r\n " +
                $"ORDER BY 검사ID";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }
        /// <summary>
        /// 아름제일 여성병원
        /// </summary>
        /// <param name="dateKind"></param>
        /// <param name="compCode"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [Route("api/Sales/LegacyLabgeResultDownloader/KChart5771Result")]
        public IHttpActionResult GetKChart5771Result(string dateKind, string compCode, DateTime beginDate, DateTime endDate)
        {
            string sql;

            sql =
                $"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED\r\n" +
                $"SELECT B.CompSystemID AS 검사ID, B.CustomData01 AS 접수ID, CONVERT(varchar, B.PatientSampleGetTime, 23) AS 접수일자, '' AS 시간\r\n" +
                $"     , C.PatientName AS 환자이름, B.PatientChartNo AS 차트번호, B.PatientSex AS 성별, B.PatientAge AS 나이\r\n" +
                $"     , dbo.FUNC_GetPaitentBirthDay(C.PatientJuminNo01 + master.dbo.AES_DecryptFunc(C.PatientJuminNo02, 'labge$%#!dleorms')) AS 생년월일 \r\n" +
                $"     , B.CustomData02 AS 유형, B.CompDeptName AS 진료실, B.PatientDoctorName AS 담당의, B.PatientSickRoom AS 병실명 \r\n" +
                $"     , B.CompOrderCode AS 코드, B.CompOrderDisplayName AS 명칭, CONVERT(varchar, B.PatientSampleGetTime, 23) AS 의뢰일자 \r\n" +
                $"     , CASE WHEN (LTRIM(RTRIM(ISNULL(A.TestResult02, '')))= '') THEN LTRIM(RTRIM(ISNULL(A.TestResult01, '')))\r\n" +
                $"       ELSE LTRIM(RTRIM(ISNULL(A.TestResult02,''))+'('+LTRIM(RTRIM(ISNULL(A.TestResult01, ''))+')')) END AS 검사결과\r\n" +
                $"     , CASE F.IsTestHeader WHEN 1 THEN dbo.FUNC_TRANS_LABRESULT(B.LabRegDate, B.LabRegNo, G.TestModuleCode, D.TestCode, D.TestCode, 1, A.TestResult01, A.TestResult02, A.TestResultText)\r\n" +
                $"       ELSE '' END AS 서술결과\r\n" +
                $"     , A.TestResultAbn AS 하이로우\r\n" +
                $"     , dbo.FUNC_TESTREF_UNIT(A.LabRegDate, C.PatientAge, C.PatientSex, A.TestSubCode, D.SampleCode) AS 단위 \r\n" +
                $"     , dbo.FUNC_TESTREF_TEXT(A.LabRegDate, C.PatientAge, C.PatientSex, A.TestSubCode, D.SampleCode) AS 참고치 \r\n" +
                $"     , H.LabRegReportID, F.ReportCode, G.ReportName, H.IsReportTransEnd, A.LabRegDate, A.LabRegNo\r\n" +
                $"FROM LabRegResult AS A\r\n" +
                $"JOIN LabImportData AS B\r\n" +
                $"ON A.LabRegDate = B.LabRegDate\r\n" +
                $"AND A.LabRegNo = B.LabRegNo\r\n" +
                $"JOIN LabRegInfo AS C\r\n" +
                $"ON A.LabRegDate = C.LabRegDate\r\n" +
                $"AND A.LabRegNo = C.LabRegNo\r\n" +
                $"JOIN LabRegTest AS D\r\n" +
                $"ON A.LabRegDate = D.LabRegDate\r\n" +
                $"AND A.LabRegNo = D.LabRegNo\r\n" +
                $"AND A.TestCode = D.TestCode\r\n" +
                $"JOIN LabTestCode F\r\n" +
                $"ON F.TestCode = A.TestSubCode\r\n" +
                $"JOIN LabReportCode G\r\n" +
                $"ON F.ReportCode = G.ReportCode\r\n" +
                $"JOIN LabRegReport H\r\n" +
                $"ON A.LabRegDate = H.LabRegDate\r\n" +
                $"AND A.LabRegNo = H.LabRegNo \r\n" +
                $"AND F.ReportCode = H.ReportCode\r\n" +
                $"JOIN LabTransMatchCode I\r\n" +
                $"ON B.CompOrderCode = I.CompMatchCode\r\n" +
                $"AND I.CompCode = C.CompCode\r\n" +
                $"AND A.TestSubCode = I.CenterMatchCode\r\n" +
                $"WHERE D.TestStateCode = 'F'\r\n";

            if (dateKind == "1")
            {
                sql += $"AND A.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n";
            }
            else if (dateKind == "2")
            {
                sql += $"AND D.TestEndTime >= '{beginDate.ToString("yyyy-MM-dd")}' AND D.TestEndTime < DATEADD(DAY, 1, '{endDate.ToString("yyyy-MM-dd")}')\r\n";
            }

            sql += $"AND C.CompCode = '{compCode}'\r\n";

            sql += " ORDER BY B.PatientChartNo";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/Sales/LegacyLabgeResultDownloader/BusanGuchisoResult")]
        public IHttpActionResult GetBusanGuchisoResult(string dateKind, string compCode, string beginDate, string endDate)
        {
            string sql, sql_where = "";

            if (dateKind == "1")
            {
                sql_where = "a.LabRegDate between '" + beginDate + "' and '" + endDate + "'\n";
            }
            else if (dateKind == "2")
            {
                sql_where = "e.TestEndTime between '" + beginDate + " 00:00' and '" + endDate + " 23:59:59'\n";
            }

            sql = "select a.PatientChartNo as 칭호번호, a.PatientName as 성명, a.PatientJuminNo01 as 생년월일, '' as 교정번호, '' as 확인, '' as '관비/본인'\n"
                + ", '' as '관비/공단', '' as '자비/본인', '' as '자비/공단', f.CompMatchCode as 검사코드, f.CompMatchName as 검사명\n"
                + ", dbo.FUNC_TRANS_LABRESULT(a.LabRegDate, a.LabRegNo, d.TestModuleCode, b.TestCode, b.TestSubCode, c.IsTestHeader, b.TestResult01, b.TestResult02, b.TestResultText) as 결과\n"
                + ", dbo.FUNC_TESTREF_TEXT(a.LabRegDate, a.PatientAge, a.PatientSex, b.TestSubCode, e.SampleCode) as 정상범위\n"
                + ", b.TestResultAbn as 이상여부\n"
                + "from LabRegInfo as a \n"
                + "inner join LabRegResult as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                + "inner join LabTestCode as c on b.TestSubCode = c.TestCode\n"
                + "inner join LabReportCode as d on c.ReportCode = d.ReportCode\n"
                + "inner join LabRegTest as e on a.LabRegDate = e.LabRegDate and a.LabRegNo = e.LabRegNo and b.TestCode = e.TestCode\n"
                + "left outer join LabTransMatchCode as f on b.TestSubCode = f.CenterMatchCode and a.CompCode = f.CompCode\n"
                + "where " + sql_where
                + "and a.CompCode = '6726'\n"
                + "and e.TestStateCode = 'F'\n"
                + "and c.IsTestHeader !='1'\n"
                + "order by a.LabRegDate, a.LabRegNo, c.TestSeqNo";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/Sales/LegacyLabgeResultDownloader/6574Result")]
        public IHttpActionResult Get6574Result(string dateKind, string compCode, string beginDate, string endDate, string dataKind, string TestTrans, string ChartNo, string PatientName)
        {
            string sql, sql_where = "", sql_where2 = "", sql_where3 = "", sql_SelectResult = "";

            if (dateKind == "1")
            {
                sql_where = "a.LabRegDate between '" + beginDate + "' and '" + endDate + "'\n";
            }
            else if (dateKind == "2")
            {
                //sql_where = "d.TestEndTime between '" + beginDate + " 00:00' and '" + endDate + " 23:59:59' \n";
                sql_where = "(d.TestEndTime between '" + beginDate + " 00:00' and '" + endDate + " 23:59:59' or d.TestMidTime between '" + beginDate + " 00:00' and '" + endDate + " 23:59:59')\n";
            }
            if (dataKind == "1")
            {
                sql_where2 = "and e.PartCode not in ('16', '32')\n";
                sql_SelectResult = ", dbo.FUNC_TRANS_LABRESULT(c.LabRegDate, c.LabRegNo, f.TestModuleCode, c.TestCode, c.TestSubCode, g.IsTestHeader, c.TestResult01, c.TestResult02, c.TestResultText)\n";
            }
            else if (dataKind == "2")
            {
                sql_where2 = "and e.PartCode in ('16', '32')\n";
                sql_SelectResult = ", replace(replace(replace(dbo.FUNC_TRANS_LABRESULT_DoctorCode(c.LabRegDate, c.LabRegNo, f.TestModuleCode, c.TestCode, c.TestSubCode, g.IsTestHeader, c.TestResult01, c.TestResult02, c.TestResultText), CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) + '<', '|<'), CHAR(13) + CHAR(10) + '<', '|<'), '|', CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10))\n";
            }
            if ((ChartNo ?? "") != "")
            {
                sql_where3 = "and a.PatientChartNo = '" + ChartNo + "' ";
            }
            if ((PatientName ?? "") != "")
            {
                sql_where3 = sql_where3 + "and a.PatientName = '" + PatientName + "' ";
            }

            sql = "select ROW_NUMBER() over(order by a.LabRegDate, a.LabRegNo, e.PartCode, e.TestSeqNo) as No\n"
                + ", CONVERT(varchar(8), a.LabRegDate, 112) as 접수일자, b.CustomData01 as 병원검체번호, a.PatientName as 환자명\n"
                + ", a.PatientChartNo as 차트번호, b.CompOrderCode as 병원검사코드, b.CompOrderDisplayName as 검사명\n"
                + sql_SelectResult
                + "  + case When dbo.FUNC_TESTREF_TEXT(c.LabRegDate, a.PatientAge, a.PatientSex, c.TestSubCode, d.SampleCode) != '' then  CHAR(13) + CHAR(10) + '[참고치]' + CHAR(13) + CHAR(10)\n"
                + "  + dbo.FUNC_TESTREF_TEXT(c.LabRegDate, a.PatientAge, a.PatientSex, c.TestSubCode, d.SampleCode) else '' end\n"
                + "  + case When d.TestCode = '13912' then CHAR(13) + CHAR(10) + j.RemarkText else '' end\n"
                + "  as 결과\n"
                + ", dbo.FUNC_TESTREF_TEXT(c.LabRegDate, a.PatientAge, a.PatientSex, c.TestSubCode, d.SampleCode) as 참고치\n"
                + ", CONVERT(varchar(8), d.TestEndTime, 112) as 결과입력일자\n"
                + ", replace(CONVERT(varchar(8), d.TestEndTime, 114), ':', '') as 결과입력시간\n"
                + ", e.InsureCode as 보험코드\n"
                //+ ", 'http://labge.labcenter.kr/webclient/LabReportImage.aspx?ReportID=' + convert(varchar(50), h.LabRegReportID) + '&IsWaterMarkUse=true' as 이미지\n"
                + ", case when isnull(j.RemarkText, '') != '' and j.RemarkText like '%http%' then j.RemarkText \n"
                + "else 'http://labge.labcenter.kr/webclient/LabReportImage.aspx?ReportID=' + convert(varchar(50), h.LabRegReportID) + '&IsWaterMarkUse=true' end as 이미지\n"
                + ", d.LabRegDate, d.LabRegNo, d.TestCode\n"
                + "from LabRegInfo as a\n"
                + "inner join LabImportData as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo and a.PatientChartNo = b.PatientChartNo\n"
                + "inner join LabRegResult as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo and b.CenterOrderCode = c.TestSubCode\n"
                + "inner join LabRegTest as d on a.LabRegDate = d.LabRegDate and a.LabRegNo = d.LabRegNo and d.OrderCode = c.OrderCode and d.TestCode = c.TestCode\n"
                + "inner join LabTestCode as e on c.TestCode = e.TestCode\n"
                + "inner join LabReportCode as f on e.ReportCode = f.ReportCode\n"
                + "inner join LabTestCode as g on c.TestSubCode = g.TestCode\n"
                + "inner join LabRegReport as h on a.LabRegDate = h.LabRegDate and a.LabRegNo = h.LabRegNo and e.ReportCode = h.ReportCode\n"
                + "left outer join LabTransMatchCode as i on b.CompOrderCode = i.CompMatchCode and a.CompCode = i.CompCode\n"
                + "left outer join LabRegRemark as j on a.LabRegDate = j.LabRegDate and a.LabRegNo = j.LabRegNo and e.ReportCode = j.ReportCode\n"
                + "where " + sql_where + sql_where2
                + "and d.IsTestTransEnd in (" + TestTrans + ")"
                + "and a.CompCode = '6574'\n"
                + sql_where3
                + "and (d.TestStateCode = 'F' or (c.TestCode in ('21038') and d.TestStateCode = 'M'))";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/Sales/LegacyLabgeResultDownloader/TestTransEnd_Log")]
        public IHttpActionResult PutTestTransEnd_Log([FromBody]JObject request)
        {
            try
            {
                string sql;
                sql =
                    "UPDATE LabRegTest \r\n" +
                    "   SET IsTestTransEnd = '" + request["setValue"].ToString() + "' \r\n" +
                    "     , TestTransEndTime = CURRENT_TIMESTAMP\r\n" +
                    " WHERE LabRegDate = '" + request["labRegDate"].ToString() + "'\r\n" +
                    "   AND LabRegNo = '" + request["labRegNo"].ToString() + "'\r\n" +
                    "   AND TestCode = '" + request["testCode"].ToString() + "'\r\n" +

                    "INSERT INTO LabgeTransLog \r\n" +
                    "SELECT NEWID(), '" + request["labRegDate"].ToString() + "' as LabRegDate, '" + request["labRegNo"].ToString() + "' as LabRegNo, '" + request["compCode"].ToString() + "' as CompCode, " +
                    " '" + request["testCode"].ToString() + "' as TestCode, " +
                    " '" + request["accessIP"].ToString() + "' as AccessIPAddress, '" + request["transKind"].ToString() + "' as TransKind, CURRENT_TIMESTAMP as TransTime";

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

        [Route("api/Sales/LegacyLabgeResultDownloader/ImageResultSendFlag")]
        //public IHttpActionResult PutImageResultSendFlag(DateTime labRegDate, int labRegNo, string reportCode, string transKind, string ftpCode)
        public IHttpActionResult PutImageResultSendFlag([FromBody]JObject request)
        {
            string sql;
            sql =
                $"UPDATE LabRegReport\r\n" +
                $"   SET IsReportTransEnd = '{request["TransKind"].ToString()}'\r\n" +
                $"     , ReportTransEndTime = GETDATE()\r\n" +
                $" WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                $"   AND LabRegNo = {request["LabRegNo"].ToString()}\r\n" +
                $"   AND ReportCode = '{request["ReportCode"].ToString()}'";
            LabgeDatabase.ExecuteSql(sql);

            if ((request["ftpCode"] ?? "").ToString() != string.Empty)
            {
                sql =
                    $"UPDATE LabTransReportFtp\r\n" +
                    $"   SET LastSendTime = GETDATE()\r\n" +
                    $" WHERE FtpCode = '{request["FtpCode"].ToString()}'";
                LabgeDatabase.ExecuteSql(sql);
            }

            return Ok();
        }
    }
}