using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Web.Http;
using supportsapi.labgenomics.com.Services;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/sales/woorimadiorder")]
    public class WooriMadiOrderController : ApiController
    {
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string isTestOutside, int beginNo, int endNo, string compMngCode)
        {
            string sql;
            sql = $"SELECT A.LabRegDate, A.LabRegNo, A.CompCode\r\n" +
                  $"     , E.CompName, F.CompMngName\r\n" +
                  $"     , A.PatientName, A.PatientAge, A.PatientSex, A.PatientJuminNo01, A.PatientChartNo\r\n" +
                  $"     , B.OrderCode, B.TestCode\r\n" +
                  $"     , D.TestDisplayName, D.ReportCode, lrc.TestModuleCode\r\n" +
                  $"     , B.SampleCode\r\n" +
                  $"     , (SELECT SampleName FROM LabSampleCode WHERE B.SampleCode = SampleCode) AS SampleName\r\n" +
                  $"     , B.IsTestOutside, B.TestOutsideBeginTime, B.TestOutsideEndTime, B.TestOutsideCompCode, B.TestOutsideMemberID\r\n" +
                  $"FROM LabRegInfo A\r\n" +
                  $"JOIN LabRegTest B\r\n" +
                  $"ON A.LabRegDate = B.LabRegDate\r\n" +
                  $"AND A.LabRegNo = B.LabRegNo\r\n";
            if (isTestOutside == "1")
            {
                sql += "AND TestOutsideCompCode = '4289'\r\n";
            }
            sql += $"JOIN LabOutsideTestCode C\r\n" +
                   $"ON C.OutsideCompCode = '4289'\r\n" +
                   $"AND C.OutsideTestCode = B.TestCode\r\n" +
                   $"AND C.OutsideSampleCode = B.SampleCode\r\n" +
                   $"JOIN LabTestCode D\r\n" +
                   $"ON B.TestCode = D.TestCode\r\n" +
                   $"JOIN LabReportCode lrc\r\n" +
                   $"ON D.ReportCode = lrc.ReportCode\r\n" +
                   $"JOIN ProgCompCode E\r\n" +
                   $"ON E.CompCode = A.CompCode\r\n";
            if ((compMngCode ?? string.Empty) != string.Empty)
            {
                sql += $"AND E.CompMngCode = '{compMngCode}'\r\n";
            }
            sql += $"JOIN ProgCompMngCode F\r\n" +
                   $"ON F.CompMngCode = E.CompMngCode\r\n" +
                   $"WHERE A.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                   $"AND B.IsTestOutSide = {isTestOutside}\r\n" +
                   $"AND A.LabRegNo BETWEEN {beginNo} AND {endNo}\r\n" +
                   $"ORDER BY A.LabRegDate, A.LabRegNo";

            return Ok(LabgeDatabase.SqlToJArray(sql));
        }

        public IHttpActionResult Get(string value)
        {
            string sql = string.Empty;
            if (value == "ReportCode")
            {
                sql = "SELECT ReportCode, ReportName\r\n" +
                      "FROM LabReportCode\r\n" +
                      "WHERE ReportCode IN ('02', '03', '19')";
            }
            else if (value == "ProgCompMngCode")
            {
                sql = "SELECT CompMngCode, CompMngName\r\n" +
                      "FROM ProgCompMngCode";
            }
            else if (value == "TestModuleCode")
            {
                sql = "SELECT TestModuleCode, TestModuleName\r\n" +
                      "FROM LabTestModuleCode";
            }
            JArray array = LabgeDatabase.SqlToJArray(sql);
            return Ok(array);
        }

        public IHttpActionResult Post([FromBody]JObject request)
        {
            MySqlConnection wooriMadiConn;
            wooriMadiConn = new MySqlConnection(ConfigurationManager.ConnectionStrings["WooriMadiConnection"].ConnectionString);

            wooriMadiConn.Open();

            try
            {
                string sql;

                sql = $"INSERT INTO uploadmst\r\n" +
                      $"     ( REQDTE, CSTCD, SAMPLENO, SEQ, CSTITEMCD, CSTITEMNM\r\n" +
                      $"     , HOSNO, PATNM, SAMPLECD\r\n" +
                      $"     , SAMPLENM, BIRDTE, SEX, RESULT_DOWN, UP_DATE, ITEMGUBN)\r\n" +
                      $"VALUES\r\n" +
                      $"     ( '{request["REQDTE"].ToString()}', '30000', '{request["SAMPLENO"].ToString()}', '{request["SEQ"].ToString()}', '{request["CSTITEMCD"].ToString()}', '{request["CSTITEMNM"].ToString()}'\r\n" +
                      $"     , '{request["HOSNO"].ToString()}', '{request["PATNM"].ToString()}', '{request["SAMPLECD"].ToString()}'\r\n" +
                      $"     , '{request["SAMPLENM"].ToString()}', '{request["BIRDTE"].ToString()}', '{request["SEX"].ToString()}', 'F', SYSDATE(), 'G')";

                MySqlCommand cmd = new MySqlCommand(sql, wooriMadiConn);
                cmd.ExecuteNonQuery();

                //등록이 완료되면 우리 테이블에도 update해준다.
                sql = $"UPDATE LabRegTest\r\n" +
                      $"SET IsTestOutside = '1'\r\n" +
                      $"  , TestStateCode = 'O'\r\n" +
                      $"  , TestOutSideBeginTime = GETDATE()\r\n" +
                      $"  , TestStartTime = GETDATE()\r\n" +
                      $"  , IsWorkCheck = '1'\r\n" +
                      $"  , WorkCheckMemberID = '{request["RegistMemberID"].ToString()}'\r\n" +
                      $"  , WorkCheckTime  = GETDATE()\r\n" +
                      $"  , TestOutsideCompCode = '4289'\r\n" +
                      $"  , TestOutsideMemberID = '{request["RegistMemberID"].ToString()}'\r\n" +
                      $"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND LabRegNo = {request["LabRegNo"].ToString()}\r\n" +
                      $"AND TestCode = '{request["CSTITEMCD"].ToString()}'\r\n" +
                      $"\r\n" +
                      $"DECLARE @ReportCode varchar(30)\r\n" +
                      $"SELECT @ReportCode = ReportCode\r\n" +
                      $"FROM LabTestCode\r\n" +
                      $"WHERE TestCode = '{request["CSTITEMCD"].ToString()}'\r\n" +
                      $"\r\n" +
                      $"UPDATE LabRegReport\r\n" +
                      $"SET ReportStartTime = GETDATE()\r\n" +
                      $"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND LabRegNo = '{request["LabRegNo"].ToString()}'\r\n" +
                      $"AND ReportCode = @ReportCode";
                LabgeDatabase.ExecuteSql(sql);
                return Ok();
            }
            finally
            {
                wooriMadiConn.Close();
            }
        }
    }
}