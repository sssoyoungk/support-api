using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/sales/ReceivePlsLabOrder")]
    public class ReceivePlsLabOrderController : ApiController
    {
        /// <summary>
        /// 이기은으로 전송할 오더 조회
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="kindSearch"></param>
        /// <returns></returns>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, bool kindSearch, string groupCode = "")
        {
            string sql;
            string isTestOutSide;
            if (kindSearch)
                isTestOutSide = "1";
            else
                isTestOutSide = "0";

            sql = $"SELECT lri.LabRegDate, lri.LabRegNo, lri.CompCode\r\n" +
                  $"     , (SELECT CompName FROM ProgCompCode WHERE lri.CompCode = CompCode) AS CompName\r\n" +
                  $"     , lri.PatientName, lri.PatientAge, lri.PatientSex, lri.PatientJuminNo01, lri.PatientChartNo\r\n" +
                  $"     , lrt.OrderCode, lrt.TestCode\r\n" +
                  $"     , ltc.TestDisplayName, lpc.PartName\r\n" +
                  $"     , lrt.SampleCode\r\n" +
                  $"     , (SELECT SampleName FROM LabSampleCode WHERE lrt.SampleCode = SampleCode) AS SampleName\r\n" +
                  $"     , lrt.IsTestOutside, lrt.TestOutsideBeginTime, lrt.TestOutsideEndTime, lrt.TestOutsideCompCode, lrt.TestOutsideMemberID\r\n" +
                  $"FROM LabRegInfo lri\r\n" +
                  $"JOIN LabRegTest lrt\r\n" +
                  $"ON lri.LabRegDate = lrt.LabRegDate\r\n" +
                  $"AND lri.LabRegNo = lrt.LabRegNo\r\n" +
                  $"AND lri.CompCode IN (SELECT CompCode FROM ProgAuthGroupCompList WHERE AuthGroupCode = 'c4236')\r\n" + //전주 사무소 요청으로 등록된 거래처만 조회되도록 함.            
                  $"JOIN LabOutsideTestCode lotc\r\n" +
                  $"ON lotc.OutsideCompCode = '4236'\r\n" +
                  $"AND lotc.OutsideTestCode = lrt.TestCode\r\n" +
                  $"AND lotc.OutsideSampleCode = lrt.SampleCode\r\n" +
                  $"JOIN LabTestCode ltc\r\n" +
                  $"ON ltc.TestCode = lrt.TestCode\r\n" +
                  $"JOIN LabPartCode lpc\r\n" +
                  $"ON lpc.PartCode = ltc.PartCode\r\n" +
                  $"WHERE lri.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                  $"AND lrt.IsTestOutSide = {isTestOutSide}\r\n";

            if (kindSearch)
            {
                sql += "AND lrt.TestOutsideCompCode = '4236'\r\n";
            }
            else
            {
                sql += "AND ISNULL(lrt.TestOutsideCompCode, '') = ''\r\n";
            }

            if (groupCode != string.Empty)
            {
                sql += $"AND lri.CompCode IN ( SELECT CompCode FROM ProgAuthGroupAccessComp WHERE AuthGroupCode = '{groupCode}' )\r\n";
            }

            sql += "ORDER BY lri.LabRegDate, lri.LabRegNo, lrt.OrderCode";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        /// <summary>
        /// 이기은진단에 오더 전송
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHttpActionResult Post([FromBody]JObject request)
        {
            //이기은 검사센터 테이블에 Insert
            OracleConnection connComp = new OracleConnection(ConfigurationManager.ConnectionStrings["PlsLabConnection"].ConnectionString);
            connComp.Open();
            OracleTransaction tranComp = connComp.BeginTransaction();
            try
            {
                OracleCommand cmdComp = new OracleCommand();
                cmdComp.Connection = connComp;
                cmdComp.Transaction = tranComp;

                string sqlComp;

                sqlComp = "INSERT INTO uploadmst\r\n" +
                          "     ( REQDTE, CSTCD, SAMPLENO, SEQ, CSTITEMCD, CSTITEMNM\r\n" +
                          "     , HOSNO, PATNM, SAMPLECD, SAMPLENM, BIRDTE, SEX, RESULT_DOWN, UP_DATE)\r\n" +
                          "VALUES\r\n" +
                          "     ( :REQDTE, :CSTCD, :SAMPLENO, :SEQ, :CSTITEMCD, :CSTITEMNM\r\n" +
                          "     , :HOSNO, :PATNM, :SAMPLECD, :SAMPLENM, :BIRDTE, :SEX, 'F', SYSDATE) ";


                cmdComp.CommandText = sqlComp;

                OracleParameter paramReqDte = new OracleParameter("REQDTE", request["ReqDte"].ToString());
                cmdComp.Parameters.Add(paramReqDte);

                OracleParameter paramCstCd = new OracleParameter("CSTCD", request["CstCd"].ToString());
                cmdComp.Parameters.Add(paramCstCd);

                OracleParameter paramSampleNo = new OracleParameter("SAMPLENO", request["SampleNo"].ToString());
                cmdComp.Parameters.Add(paramSampleNo);

                OracleParameter paramSeq = new OracleParameter("SEQ", request["Seq"].ToString());
                cmdComp.Parameters.Add(paramSeq);

                OracleParameter paramCstItemCd = new OracleParameter("CSTITEMCD", request["CstItemCd"].ToString());
                cmdComp.Parameters.Add(paramCstItemCd);

                OracleParameter paramCstItemNm = new OracleParameter("CSTITEMNM", request["CstItemNm"].ToString());
                cmdComp.Parameters.Add(paramCstItemNm);

                OracleParameter paramHosNo = new OracleParameter("HOSNO", request["HosNo"].ToString());
                cmdComp.Parameters.Add(paramHosNo);

                OracleParameter paramPatNm = new OracleParameter("PATNM", request["PatNm"].ToString());
                cmdComp.Parameters.Add(paramPatNm);

                OracleParameter paramSampleCd = new OracleParameter("SAMPLECD", request["SampleCd"].ToString());
                cmdComp.Parameters.Add(paramSampleCd);

                OracleParameter paramSampleNm = new OracleParameter("SAMPLENM", request["SampleNm"].ToString());
                cmdComp.Parameters.Add(paramSampleNm);

                OracleParameter paramBirDte = new OracleParameter("BIRDTE", request["BirDte"].ToString());
                cmdComp.Parameters.Add(paramBirDte);

                OracleParameter paramSex = new OracleParameter("SEX", request["Sex"].ToString());
                cmdComp.Parameters.Add(paramSex);

                cmdComp.ExecuteNonQuery();

                tranComp.Commit();

                //이기은 검사센터에 등록이 완료되면 우리 테이블에도 update해준다.
                string sql;

                sql = $"UPDATE LabRegTest\r\n" +
                      $"SET IsTestOutside = '1'\r\n" +
                      $"  , IsWorkCheck = '1'\r\n" +
                      $"  , TestStateCode = 'O'\r\n" +
                      $"  , TestOutSideBeginTime = GETDATE()\r\n" +
                      $"  , TestStartTime = GETDATE()\r\n" +                      
                      $"  , WorkCheckMemberID = '{request["RegistMemberID"].ToString()}'\r\n" +
                      $"  , WorkCheckTime  = GETDATE()\r\n" +
                      $"  , TestOutsideCompCode = '4236'\r\n" +
                      $"  , TestOutsideMemberID = '{request["RegistMemberID"].ToString()}'\r\n" +
                      $"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND LabRegNo = '{request["LabRegNo"].ToString()}'\r\n" +
                      $"AND TestCode = '{request["CstItemCd"].ToString()}'\r\n" +
                      $"\r\n" +
                      $"DECLARE @ReportCode varchar(30)\r\n" +
                      $"SELECT @ReportCode = ReportCode\r\n" +
                      $"FROM LabTestCode\r\n" +
                      $"WHERE TestCode = '{request["CstItemCd"].ToString()}'\r\n" +
                      $"\r\n" +
                      $"UPDATE LabRegReport\r\n" +
                      $"SET ReportStartTime = GETDATE()\r\n" +
                      $"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND LabRegNo = '{request["LabRegNo"].ToString()}'\r\n" +
                      $"AND ReportCode = @ReportCode";

                LabgeDatabase.ExecuteSql(sql);

                return Ok();
            }
            catch (Exception ex)
            {
                tranComp.Rollback();
                JObject objResponse = new JObject();
                objResponse.Add("Message", ex.Message);
                return Content(System.Net.HttpStatusCode.BadRequest, objResponse);
            }
            finally
            {
                connComp.Close();
            }
        }
    }
}