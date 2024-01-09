using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/SendImageResult")]
    public class SendImageResultController : ApiController
    {
        /// <summary>
        /// FTP 접속 정보
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult Get()
        {
            string sql;

            sql = "SELECT ftp.FtpCode, ftp.FtpName, ftp.CompCode, comp.CompName, ftp.ServerIP, ftp.ServerPort \r\n" +
                  "     , ftp.ServerLoginID, ftp.ServerLoginPW, ftp.CreateFilePath, ftp.CreateFileName, ftp.LastSendTime \r\n" +
                  "     , ftp.CreateFileExt \r\n" +
                  "FROM LabTransReportFtp AS ftp \r\n" +
                  "JOIN ProgCompCode AS comp \r\n" +
                  "ON comp.CompCode = ftp.CompCode \r\n" +
                  "WHERE ftp.IsFtpUse = 1 ";

            return Ok(Services.LabgeDatabase.SqlToJArray(sql));
        }

        /// <summary>
        /// 그룹코드 추가해서 담당 거래처만 보이도록 처리
        /// </summary>
        /// <param name="authGroupCode"></param>
        /// <returns></returns>
        public IHttpActionResult Get(string authGroupCode)
        {
            string sql;

            sql = $"SELECT ftp.FtpCode, ftp.FtpName, ftp.CompCode, comp.CompName, ftp.ServerIP, ftp.ServerPort\r\n" +
                  $"     , ftp.ServerLoginID, ftp.ServerLoginPW, ftp.CreateFilePath, ftp.CreateFileName, ftp.LastSendTime\r\n" +
                  $"     , ftp.CreateFileExt\r\n" +
                  $"FROM LabTransReportFtp AS ftp\r\n" +
                  $"JOIN ProgCompCode AS comp\r\n" +
                  $"ON comp.CompCode = ftp.CompCode\r\n" +
                  $"WHERE ftp.IsFtpUse = 1\r\n" +
                  $"AND comp.CompCode IN\r\n" +
                  $"(\r\n" +
                  $"    SELECT CompCode\r\n" +
                  $"    FROM ProgAuthGroupAccessComp\r\n" +
                  $"    WHERE AuthGroupCode = '{authGroupCode}'\r\n" +
                  $")";

            return Ok(Services.LabgeDatabase.SqlToJArray(sql));
        }

        /// <summary>
        /// 전송할 결과 조회
        /// </summary>
        /// <param name="compCode"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="dateKind"></param>
        /// <param name="transKind"></param>
        /// <returns></returns>
        // GET api/<controller>
        public IHttpActionResult Get(string compCode, DateTime beginDate, DateTime endDate, string dateKind, string transKind)
        {
            try
            {
                string sql = string.Empty;

                //서울여성병원, 서울의원은 검사항목별로 이미지를 전송해야해서 쿼리문을 별도로 분리.
                if (compCode == "2928" || compCode == "M2928" || compCode == "9538" || compCode == "M9538")
                {
                    sql =
                        $"SELECT\r\n" +
                        $"    lri.LabRegDate, lri.LabRegNo, lri.PatientChartNo, lri.PatientName, ltcoi.CompSpcNo, ltcoi.CompTestCode,\r\n" +
                        $"    lrr.TestSubCode, lrp.ReportCode, lrp.LabRegReportID, lrp.IsReportTransEnd, lri.PatientSampleGetTime, lri.PatientImportCustomData01,\r\n" +
                        $"    lrc.ReportName, pclr.ReportMatchCode, lri.CompDeptCode, lri.LabRegPatientID\r\n" +
                        $"FROM LabRegInfo AS lri\r\n" +
                        $"JOIN LabRegResult AS lrr \r\n" +
                        $"ON lri.LabRegDate = lrr.LabRegDate \r\n" +
                        $"AND lri.LabRegNo = lrr.LabRegNo\r\n" +
                        $"JOIN LabTestCode AS ltc \r\n" +
                        $"ON lrr.TestCode = ltc.TestCode\r\n" +
                        $"JOIN LabRegReport AS lrp \r\n" +
                        $"ON lri.LabRegDate = lrp.LabRegDate \r\n" +
                        $"AND lri.LabRegNo = lrp.LabRegNo \r\n" +
                        $"AND ltc.ReportCode = lrp.ReportCode\r\n" +
                        $"JOIN ProgCompLabReport AS pclr \r\n" +
                        $"ON lri.CompCode = pclr.CompCode \r\n" +
                        $"AND lrp.ReportCode = pclr.ReportCode \r\n" +
                        $"AND pclr.IsReportTransFtp = 1\r\n" +
                        $"JOIN LabReportCode AS lrc \r\n" +
                        $"ON lrp.ReportCode = lrc.ReportCode\r\n" +
                        $"JOIN LabTransCompOrderInfo AS ltcoi \r\n" +
                        $"ON lri.LabRegDate = ltcoi.LabRegDate \r\n" +
                        $"AND lri.LabRegNo = ltcoi.LabRegNo \r\n" +
                        $"AND lrr.TestSubCode = ltcoi.TestCode\r\n" +
                        $"WHERE lri.CompCode = '{compCode}'\r\n" +
                        $"AND lrp.ReportStateCode = 'F'\r\n" +
                        $"AND lrp.IsReportTransEnd = {transKind}\r\n" +
                        $"AND lrp.ReportEndTime != ''\r\n";
                        

                    if (dateKind == "E") //보고일
                    {
                        sql += $"AND lrp.ReportEndTime >= '{beginDate:yyyy-MM-dd}' AND lrp.ReportEndTime < DATEADD(DAY, 1, '{endDate:yyyy-MM-dd}')\r\n";
                    }
                    else //접수일
                    {
                        sql += $"AND lrp.LabRegDate BETWEEN '{beginDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}' ";
                    }

                    sql += "ORDER BY lri.LabRegDate, lri.PatientChartNo, lri.PatientName\r\n";
                }
                //아이앤젤산부인과 네오차트(메디차트) 엑셀 연동
                else if (compCode == "22005")
                {
                    sql = $"SELECT lrr.LabRegReportID, lrt.LabRegDate, lrt.LabRegNo, lri.PatientChartNo, lri.PatientName, lid.CompOrderCode AS CompTestCode, lid.CompSystemID AS CompSpcNo\r\n" +
                          $"     , lrr.ReportCode\r\n" +
                          $"FROM LabRegTest lrt\r\n" +
                          $"JOIN LabImportData lid\r\n" +
                          $"ON lrt.LabRegDate = lid.LabRegDate\r\n" +
                          $"AND lrt.LabRegNo = lid.LabRegNo\r\n" +
                          $"AND lrt.TestCode = lid.CenterOrderCode\r\n" +
                          $"JOIN LabRegInfo lri\r\n" +
                          $"ON lrt.LabRegDate = lri.LabRegDate\r\n" +
                          $"AND lrt.LabRegNo = lri.LabRegNo\r\n" +
                          $"AND lri.CompCode = '{compCode}'\r\n" +
                          $"JOIN LabTestCode ltc\r\n" +
                          $"ON lrt.TestCode = ltc.TestCode\r\n" +
                          $"JOIN LabRegReport lrr\r\n" +
                          $"ON lrt.LabRegDate = lrr.LabRegDate\r\n" +
                          $"AND lrt.LabRegNo = lrr.LabRegNo\r\n" +
                          $"AND lrr.ReportCode = ltc.ReportCode\r\n" +
                          $"WHERE lrr.ReportEndTime <> ''\r\n" +
                          $"AND lrr.IsReportTransEnd = {transKind}";

                    if (dateKind == "E") //보고일
                    {
                        sql += $"AND lrr.ReportEndTime >= '{beginDate:yyyy-MM-dd} ' AND lrr.ReportEndTime < DATEADD(DAY, 1, ' {endDate:yyyy-MM-dd}')";
                    }
                    else //접수일
                    {
                        sql += $"AND lrr.LabRegDate BETWEEN '{beginDate:yyyy-MM-dd} ' AND ' {endDate:yyyy-MM-dd}' ";
                    }

                    sql += "ORDER BY lrr.LabRegDate, lrr.LabRegNo, lri.PatientChartNo, lri.PatientName";
                }
                else
                {
                    sql = $"SELECT LabRegReportID, report.LabRegDate, report.LabRegNo, info.PatientChartNo, info.PatientName, report.ReportCode, report.IsReportTransEnd \r\n" +
                          $"     , info.PatientImportCustomData01, info.PatientSampleGetTime, info.CompDeptCode, reportCode.ReportName \r\n" +
                          $"     , setting.IsReportTransFtp, setting.ReportMatchCode, info.LabRegPatientID \r\n" +
                          $"FROM LabRegReport AS report \r\n" +
                          $"JOIN LabRegInfo AS info \r\n" +
                          $"ON info.LabRegDate = report.LabRegDate \r\n" +
                          $"AND info.LabRegNo = report.LabRegNo \r\n" +
                          $"AND info.CompCode = '{compCode}' \r\n" +
                          $"JOIN ProgCompLabReport AS setting \r\n" +
                          $"ON setting.CompCode = info.CompCode \r\n" +
                          $"AND setting.ReportCode = report.ReportCode \r\n" +
                          $"AND setting.IsReportTransFtp = 1 \r\n" +
                          $"JOIN LabReportCode AS reportCode \r\n" +
                          $"ON reportCode.ReportCode = report.ReportCode \r\n" +
                          $"WHERE report.ReportEndTime <> '' \r\n" +
                          $"AND report.ReportStateCode = 'F' \r\n" +
                          $"AND report.IsReportTransEnd = {transKind} \r\n";

                    if (dateKind == "E") //보고일
                    {
                        sql += $"AND report.ReportEndTime >= '{beginDate:yyyy-MM-dd} ' AND report.ReportEndTime < DATEADD(DAY, 1, ' {endDate:yyyy-MM-dd}')\r\n";
                    }
                    else //접수일
                    {
                        sql += $"AND report.LabRegDate BETWEEN '{beginDate:yyyy-MM-dd} ' AND ' {endDate:yyyy-MM-dd}'\r\n";
                    }

                    sql += "ORDER BY report.LabRegDate, info.PatientChartNo, info.PatientName";
                }

                JArray array = Services.LabgeDatabase.SqlToJArray(sql);

                return Ok(array);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }

        public IHttpActionResult Patch([FromBody]JObject value)
        {
            try
            {
                string sql;
                sql = $"UPDATE LabRegReport\r\n" +
                      $"   SET IsReportTransEnd = '{value["IsReportTransEnd"]}'\r\n" +
                      $"     , ReportTransEndTime = GETDATE()\r\n" +
                      $" WHERE LabRegDate = '{Convert.ToDateTime(value["LabRegDate"]):yyyy-MM-dd}'\r\n" +
                      $"   AND LabRegNo = {value["LabRegNo"]}\r\n" +
                      $"   AND ReportCode = '{value["ReportCode"]}'";
                Services.LabgeDatabase.ExecuteSql(sql);

                if (value["FtpCode"].ToString() != string.Empty)
                {
                    sql = $"UPDATE LabTransReportFtp\r\n" +
                          $"   SET LastSendTime = GETDATE()\r\n" +
                          $" WHERE FtpCode = '{value["FtpCode"]}'";

                    Services.LabgeDatabase.ExecuteSql(sql);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}