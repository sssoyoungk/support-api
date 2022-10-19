using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data.SqlClient;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/SqLabOrder")]
    public class SqLabOrderController : ApiController
    {
        /// <summary>
        /// 전송할 오더 조회
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="isTestOutside"></param>
        /// <param name="beginNo"></param>
        /// <param name="endNo"></param>
        /// <param name="compMngCode"></param>
        /// <param name="authGroupCode"></param>
        /// <returns></returns>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string isTestOutside, int beginNo, int endNo, string compMngCode, string authGroupCode)
        {
            string sql = string.Empty;
            try
            {
                //선제 검사가 아닌 검사들
                sql = $"SELECT lri.LabRegDate, lri.LabRegNo, lri.CompCode\r\n" +
                      $"     , pcc.CompName, pcmc.CompMngName, '' AS OriginalName\r\n" +
                      $"     , lri.PatientName, lri.PatientAge, lri.PatientSex, '' AS PatientJuminNo01, lri.PatientChartNo\r\n" +
                      $"     , lrt.OrderCode, lrt.TestCode\r\n" +
                      $"     , ltc.TestDisplayName, ltc.ReportCode\r\n" +
                      $"     , lrt.SampleCode\r\n" +
                      $"     , (SELECT SampleName FROM LabSampleCode WHERE lrt.SampleCode = SampleCode) AS SampleName\r\n" +
                      $"     , lrt.IsTestOutside, lrt.TestOutsideBeginTime, lrt.TestOutsideEndTime, lrt.TestOutsideCompCode, lrt.TestOutsideMemberID\r\n" +
                      $"     , ROW_NUMBER() OVER(PARTITION BY lri.LabRegDate, lri.LabRegNo ORDER BY lri.LabRegDate, lri.LabRegNo) AS ItemSeq\r\n" +
                      $"FROM LabRegInfo lri\r\n" +
                      $"JOIN LabRegTest lrt\r\n" +
                      $"ON lri.LabRegDate = lrt.LabRegDate\r\n" +
                      $"AND lri.LabRegNo = lrt.LabRegNo\r\n";                      
                if (isTestOutside == "1")
                {
                    sql += "AND lrt.TestOutsideCompCode = '000125'\r\n";
                }
                else
                {
                    sql += "AND lrt.TestStateCode <> 'F'\r\n" +
                           "AND ISNULL(lrt.TestOutsideCompCode, '') = ''\r\n";
                }
                sql += $"JOIN LabOutsideTestCode lotc\r\n" +
                       $"ON lotc.OutsideCompCode = '000125'\r\n" +
                       $"AND lotc.OutsideTestCode = lrt.TestCode\r\n" +
                       $"AND lotc.OutsideSampleCode = lrt.SampleCode\r\n" +
                       $"JOIN LabTestCode ltc\r\n" +
                       $"ON lrt.TestCode = ltc.TestCode\r\n" +
                       $"JOIN ProgCompCode pcc\r\n" +
                       $"ON pcc.CompCode = lri.CompCode\r\n";
                if ((compMngCode ?? string.Empty) != string.Empty)
                {
                    sql += $"AND pcc.CompMngCode = '{compMngCode}'\r\n";
                }
                sql += $"JOIN ProgCompMngCode pcmc\r\n" +
                       $"ON pcmc.CompMngCode = pcc.CompMngCode\r\n" +
                       $"JOIN ProgAuthGroupAccessComp pagac\r\n" +
                       $"ON pcc.CompCode = pagac.CompCode\r\n" +
                       $"AND pagac.AuthGroupCode = '{authGroupCode}'\r\n" +
                       $"WHERE lri.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                       $"AND lrt.OrderCode <> '22039'\r\n" +
                       $"AND lrt.IsTestOutSide = {isTestOutside}\r\n" +
                       $"AND lri.LabRegNo BETWEEN {beginNo} AND {endNo}\r\n";
                       //$"ORDER BY lri.LabRegDate, lri.LabRegNo\r\n";

                sql += "UNION\r\n";

                //코로나 선제 검사
                sql += $"SELECT lri.LabRegDate, lri.LabRegNo, lri.CompCode\r\n" +
                       $"     , pcc.CompName, pcmc.CompMngName, lri.PatientName AS OriginalName\r\n" +
                       $"     , lrc.CustomValue01 AS PatientName, lri.PatientAge, lri.PatientSex, '' AS PatientJuminNo01, lri.PatientChartNo\r\n" +
                       $"     , lrt.OrderCode, lrt.TestCode\r\n" +
                       $"     , ltc.TestDisplayName, ltc.ReportCode\r\n" +
                       $"     , lrt.SampleCode\r\n" +
                       $"     , (SELECT SampleName FROM LabSampleCode WHERE lrt.SampleCode = SampleCode) AS SampleName\r\n" +
                       $"     , lrt.IsTestOutside, lrt.TestOutsideBeginTime, lrt.TestOutsideEndTime, lrt.TestOutsideCompCode, lrt.TestOutsideMemberID\r\n" +
                       $"     , ROW_NUMBER() OVER(PARTITION BY lri.LabRegDate, lri.LabRegNo ORDER BY lri.LabRegDate, lri.LabRegNo) AS ItemSeq\r\n" +
                       $"FROM LabRegInfo lri\r\n" +
                       $"JOIN LabRegCustom lrc\r\n" +
                       $"ON lri.LabRegDate = lrc.LabRegDate\r\n" +
                       $"AND lri.LabRegNo = lrc.LabRegNo\r\n" +
                       $"JOIN LabRegTest lrt\r\n" +
                       $"ON lri.LabRegDate = lrt.LabRegDate\r\n" +
                       $"AND lri.LabRegNo = lrt.LabRegNo\r\n";                                       
                if (isTestOutside == "1")
                {
                    sql += "AND lrt.TestOutsideCompCode = '000125'\r\n";
                }
                else
                {
                    sql += "AND lrt.TestStateCode <> 'F'\r\n" + 
                           "AND ISNULL(lrt.TestOutsideCompCode, '') = ''\r\n";
                }
                sql += $"JOIN LabOutsideTestCode lotc\r\n" +
                       $"ON lotc.OutsideCompCode = '000125'\r\n" +
                       $"AND lotc.OutsideTestCode = lrt.TestCode\r\n" +
                       $"AND lotc.OutsideSampleCode = lrt.SampleCode\r\n" +
                       $"JOIN LabTestCode ltc\r\n" +
                       $"ON lrt.TestCode = ltc.TestCode\r\n" +
                       $"JOIN ProgCompCode pcc\r\n" +
                       $"ON pcc.CompCode = lri.CompCode\r\n";
                if ((compMngCode ?? string.Empty) != string.Empty)
                {
                    sql += $"AND pcc.CompMngCode = '{compMngCode}'\r\n";
                }
                sql += $"JOIN ProgCompMngCode pcmc\r\n" +
                       $"ON pcmc.CompMngCode = pcc.CompMngCode\r\n" +
                       $"JOIN ProgAuthGroupAccessComp pagac\r\n" +
                       $"ON pcc.CompCode = pagac.CompCode\r\n" +
                       $"AND pagac.AuthGroupCode = '{authGroupCode}'\r\n" +
                       $"WHERE lri.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                       $"AND lrt.OrderCode = '22039'\r\n" +
                       $"AND lrt.IsTestOutSide = {isTestOutside}\r\n" +
                       $"AND lri.LabRegNo BETWEEN {beginNo} AND {endNo}\r\n";
                       //$"ORDER BY lri.LabRegDate, lri.LabRegNo\r\n";

                sql = "SELECT LabRegDate, LabRegNo, CompCode, CompName, CompMngName, OriginalName, PatientName, PatientAge, PatientSex\r\n" +
                      "     , PatientJuminNo01, PatientChartNo, OrderCode, TestCode, TestDisplayName, ReportCode, SampleCode, SampleName\r\n" +
                      "     , IsTestOutside, TestOutsideBeginTime, TestOutsideEndTime, TestOutsideCompCode, TestOutsideMemberID\r\n" +
                      "     , dbo.FN_PadLeft(ItemSeq, '0', 2) AS ItemSeq\r\n" +
                      "FROM (\r\n" +
                      sql +
                      "     ) GRP\r\n" +
                      "ORDER BY LabRegDate, LabRegNo, ItemSeq";

                return Ok(LabgeDatabase.SqlToJArray(sql));
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// SqLab으로 오더 전송
        /// </summary>
        /// <param name="arrRequest"></param>
        /// <returns></returns>
        public IHttpActionResult Post([FromBody]JArray arrRequest)
        {
            //에스큐랩 트랜젝션
            OracleConnection sqLabConn = new OracleConnection(ConfigurationManager.ConnectionStrings["SqLabConnection"].ConnectionString);
            sqLabConn.Open();
            OracleTransaction sqLabTransaction = sqLabConn.BeginTransaction();
            
            //랩지노믹스 트랜젝션
            SqlConnection labgeConn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            labgeConn.Open();
            SqlTransaction labgeTransaction = labgeConn.BeginTransaction();

            try
            {
                foreach (JObject objRequest in arrRequest)
                {                    
                    string sql;
                    
                    sql = $"INSERT INTO uploadmst\r\n" +
                          $"( REQ_DATE, CUST_CODE, SAMPLE_NO, PAT_NAME, CHART_NO, SEX, ITEM_SEQ, CUST_ITEM_CODE, CUST_ITEM_NAME\r\n" +
                          $", CUST_SAMPLE_CODE, CUST_SAMPLE_NAME, JUBSUNO1, JUBSUNO2, SEND_DATE)\r\n" +
                          $"VALUES\r\n" +
                          $"( '{Convert.ToDateTime(objRequest["LabRegDate"]).ToString("yyyyMMdd")}'\r\n" +
                          //$"( CASE WHEN TO_CHAR(SYSDATE, 'HH24:MI:SS') >= '04:00:00' THEN TO_CHAR(SYSDATE, 'YYYYMMDD') ELSE TO_CHAR(SYSDATE - 1, 'YYYYMMDD') END\r\n" +
                          $", '03475'\r\n" +
                          $", '{Convert.ToDateTime(objRequest["LabRegDate"]).ToString("yyyyMMdd")}_{objRequest["LabRegNo"].ToString()}'\r\n" +
                          $", '{objRequest["PatientName"].ToString()}', '{objRequest["PatientChartNo"].ToString()}'" +
                          $", '{objRequest["PatientSex"].ToString()}', {objRequest["ItemSeq"].ToString()}\r\n" +
                          $", '{objRequest["TestCode"].ToString()}', '{objRequest["TestDisplayName"].ToString()}'\r\n" +
                          $", '{objRequest["SampleCode"].ToString()}', '{objRequest["SampleName"].ToString()}'\r\n" +
                          $", '{objRequest["LabRegNo"].ToString()}', '{objRequest["OriginalName"].ToString()}', SYSDATE)";

                    OracleCommand sqLabCmd = new OracleCommand
                    {
                        Connection = sqLabConn,
                        Transaction = sqLabTransaction,
                        CommandText = sql
                    };
                    sqLabCmd.ExecuteNonQuery();

                    //등록이 완료되면 우리 테이블에도 update해준다.
                    sql = $"UPDATE LabRegTest\r\n" +
                          $"SET IsTestOutside = '1'\r\n" +
                          $"  , TestStateCode = 'O'\r\n" +
                          $"  , TestOutSideBeginTime = GETDATE()\r\n" +
                          $"  , TestStartTime = GETDATE()\r\n" +
                          $"  , IsWorkCheck = '1'\r\n" +
                          $"  , WorkCheckMemberID = '{objRequest["RegistMemberID"].ToString()}'\r\n" +
                          $"  , WorkCheckTime  = GETDATE()\r\n" +
                          $"  , TestOutsideCompCode = '000125'\r\n" +
                          $"  , TestOutsideMemberID = '{objRequest["RegistMemberID"].ToString()}'\r\n" +
                          $"WHERE LabRegDate = '{Convert.ToDateTime(objRequest["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                          $"AND LabRegNo = {objRequest["LabRegNo"].ToString()}\r\n" +
                          $"AND TestCode = '{objRequest["TestCode"].ToString()}'\r\n" +
                          $"\r\n" +
                          $"DECLARE @ReportCode varchar(30)\r\n" +
                          $"SELECT @ReportCode = ReportCode\r\n" +
                          $"FROM LabTestCode\r\n" +
                          $"WHERE TestCode = '{objRequest["TestCode"].ToString()}'\r\n" +
                          $"\r\n" +
                          $"UPDATE LabRegReport\r\n" +
                          $"SET ReportStartTime = GETDATE()\r\n" +
                          $"  , ReportStateCode = CASE WHEN ReportStateCode = 'W' THEN 'O' ELSE ReportStateCode END\r\n" +
                          $"WHERE LabRegDate = '{Convert.ToDateTime(objRequest["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                          $"AND LabRegNo = '{objRequest["LabRegNo"].ToString()}'\r\n" +
                          $"AND ReportCode = @ReportCode";
                    SqlCommand labgeCmd = new SqlCommand
                    {
                        Connection = labgeConn,
                        Transaction = labgeTransaction,
                        CommandText = sql
                    };
                    labgeCmd.ExecuteNonQuery();
                }
                labgeTransaction.Commit();
                sqLabTransaction.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                labgeTransaction.Rollback();
                sqLabTransaction.Rollback();
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
            finally
            {
                labgeConn.Close();
                sqLabConn.Close();
            }
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

    }
}