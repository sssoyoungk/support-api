using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/sales/WooriMadiResult")]
    public class WooriMadiResultController : ApiController
    {
        /// <summary>
        /// 미수신 결과 조회
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult Get()
        {
            MySqlConnection wooriMadiConn;
            wooriMadiConn = new MySqlConnection(ConfigurationManager.ConnectionStrings["WooriMadiConnection"].ConnectionString);

            wooriMadiConn.Open();
            try
            {
                JArray jArrayResult = new JArray();

                MySqlCommand cmdPrepare = new MySqlCommand("procCenterPrepare3", wooriMadiConn);

                cmdPrepare.CommandTimeout = 1000;
                cmdPrepare.CommandType = CommandType.StoredProcedure;
                cmdPrepare.Parameters.AddWithValue("@vKubun", "G");
                cmdPrepare.Parameters.AddWithValue("@vCenter", "랩지");
                cmdPrepare.Parameters.Add("@vRet", MySqlDbType.VarChar, 50);
                cmdPrepare.Parameters["@vRet"].Direction = ParameterDirection.Output;
                cmdPrepare.ExecuteNonQuery();

                MySqlCommand cmdGetRes = new MySqlCommand("procCenterGetRes3", wooriMadiConn);
                cmdGetRes.CommandTimeout = 1000;
                cmdGetRes.CommandType = CommandType.StoredProcedure;
                cmdGetRes.Parameters.AddWithValue("@vKubun", "G");
                cmdGetRes.Parameters.AddWithValue("@vCenter", "랩지");
                cmdGetRes.Parameters.Add("@vRet", MySqlDbType.VarChar, 50);
                cmdGetRes.Parameters["@vRet"].Direction = ParameterDirection.Output;
                MySqlDataReader dataReader = cmdGetRes.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Load(dataReader);
                jArrayResult = JArray.Parse(JsonConvert.SerializeObject(dt));

                return Ok(jArrayResult);
            }
            finally
            {
                wooriMadiConn.Close();
            }
        }

        /// <summary>
        /// 수신완료 결과 조회
        /// </summary>
        /// <param name="isUpdate"></param>
        /// <param name="dateKind"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public IHttpActionResult Get(string isUpdate, string dateKind, DateTime beginDate, DateTime endDate)
        {
            MySqlConnection wooriMadiConn;

            wooriMadiConn = new MySqlConnection(ConfigurationManager.ConnectionStrings["WooriMadiConnection"].ConnectionString);
            wooriMadiConn.Open();
            try
            {
                string sql;
                sql = $"SELECT *\r\n" +
                      $"FROM everresult\r\n" +
                      $"WHERE comCode = '30000'\r\n" +
                      $"AND itemgubn = 'G'";

                if (dateKind == "E")
                {
                    //보고일
                    sql += $"AND inpdte >= '{beginDate.ToString("yyyyMMdd")}'\r\n" +
                           $"AND inpdte <= '{endDate.ToString("yyyyMMdd")}'\r\n";
                }
                else if (dateKind == "R")
                {
                    //접수일
                    sql += $"AND substring(EMRequestNo, 1, 8) >= '{beginDate.ToString("yyyyMMdd")}'\r\n" +
                           $"AND substring(EMRequestNo, 1, 8) <= '{endDate.ToString("yyyyMMdd")}'\r\n";
                }
                sql += "ORDER BY EMRequestNo";
                MySqlCommand cmd = new MySqlCommand(sql, wooriMadiConn);
                cmd.CommandTimeout = 360;
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                return Ok(JArray.Parse(JsonConvert.SerializeObject(dt)));
            }
            finally
            {
                wooriMadiConn.Close();
            }
        }

        // PUT api/<controller>/5
        public IHttpActionResult Put([FromBody]JArray arrResults)
        {
            MySqlConnection wooriMadiConn;

            wooriMadiConn = new MySqlConnection(ConfigurationManager.ConnectionStrings["WooriMadiConnection"].ConnectionString);
            wooriMadiConn.Open();

            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            sqlConn.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = sqlConn;

            SqlTransaction trans = sqlConn.BeginTransaction();
            cmd.Transaction = trans;
            try
            {
                try
                {
                    foreach (JObject objPatient in arrResults)
                    {
                        string doctorCode = string.Empty;

                        string sql;

                        //수진자 접수 정보 일치 확인, 결과 상태가 최종이 아닌지 체크
                        sql = $"SELECT COUNT(*) AS CNT\r\n " +
                              $"FROM LabRegInfo Info\r\n " +
                              $"JOIN LabRegTest Test\r\n " +
                              $"ON Info.LabRegDate = Test.LabRegDate\r\n " +
                              $"AND Info.LabRegNo = Test.LabRegNo\r\n " +
                              $"AND Test.TestCode = '{objPatient["TestCode"].ToString()}'\r\n" +
                              $"AND Test.TestStateCode <> 'F'\r\n " +
                              $"JOIN LabRegResult Result\r\n " +
                              $"ON Result.LabRegDate = Test.LabRegDate\r\n " +
                              $"AND Result.LabRegNo = Test.LabRegNo\r\n " +
                              $"AND Result.OrderCode = Test.TestCode\r\n " +
                              $"WHERE Info.LabRegDate = '{objPatient["LabRegDate"].ToString()}'\r\n" +
                              $"AND Info.LabRegNo = '{objPatient["LabRegNo"].ToString()}'\r\n" +
                              $"AND Info.PatientName = '{objPatient["PatientName"].ToString()}'\r\n";
                        //$"    AND Info.PatientChartNo = '{objPatient["PatientChartNo"].ToString()}'";

                        cmd.CommandText = sql;
                        int count = Convert.ToInt32(cmd.ExecuteScalar()); //실행되는 쿼리의 첫번째 열의 첫번째 행을 반환
                        int loopCount = 0;

                        if (count > 0)
                        {
                            JToken results = objPatient["Results"];
                            foreach (JObject result in results)
                            {
                                sql = $"UPDATE LabRegResult \n " +
                                      $"SET TestResultText = '{result["TestResultText"].ToString().Replace("'", "''")}'\r\n " +
                                      $"  , EditTime = GETDATE()\r\n " +
                                      $"  , EditorMemberID = 'Admin'\r\n " +
                                      $"WHERE LabRegDate = '{objPatient["LabRegDate"].ToString()}'\r\n " +
                                      $"AND LabRegNo = '{objPatient["LabRegNo"].ToString()}'\r\n " +
                                      $"AND TestSubCode = '{result["TestSubCode"].ToString()}' ";
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();

                                loopCount++;
                            }
                        }

                        if (loopCount >= 3)
                        {
                            //결과 업데이트가 완료 됐다면 LabRegTest, LabRegReport에 상태 업데이트
                            sql = $"UPDATE LabRegTest\r\n " +
                                  $" SET TestStateCode = 'F'\r\n " +
                                  $"   , TestEndTime = GETDATE()\r\n" +
                                  $"   , TestOutsideEndTime = GETDATE()\r\n" +
                                  $"   , EditTime = GETDATE()\r\n" +
                                  $"   , EditorMemberID = 'Admin'\r\n" +
                                  $"   , DoctorCode = '{objPatient["DoctorCode"].ToString()}'\r\n" + //의사 서명을 위한 판독의정보 입력
                                  $"WHERE LabRegDate = '{objPatient["LabRegDate"].ToString()}'\r\n " +
                                  $"AND LabRegNo = '{objPatient["LabRegNo"].ToString()}'\r\n " +
                                  $"UPDATE LabRegReport\r\n " +
                                  $"SET ReportStateCode = 'F'\r\n " +
                                  $"  , ReportEndTime = GETDATE()\r\n" +
                                  $"  , IsReportPrintWait = 1\r\n" +
                                  $"WHERE LabRegDate = '{objPatient["LabRegDate"].ToString()}'\r\n " +
                                  $"AND LabRegNo = '{objPatient["LabRegNo"].ToString()}' ";
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                    } //foreach (JObject objResult in arrResults)
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return Content(HttpStatusCode.BadRequest, ex.Message);
                }


                    //결과 등록이 끝나면 프로시저 실행
                    MySqlCommand cmdPrepare = new MySqlCommand("procCenterFinal", wooriMadiConn);
                    cmdPrepare.CommandType = CommandType.StoredProcedure;
                    cmdPrepare.Parameters.AddWithValue("@vKubun", "G");
                    cmdPrepare.Parameters.AddWithValue("@vCenter", "랩지");
                    cmdPrepare.Parameters.Add("@vRet", MySqlDbType.VarChar, 50);
                    cmdPrepare.Parameters["@vRet"].Direction = ParameterDirection.Output;
                    cmdPrepare.ExecuteNonQuery();

                //int update = arrResults.IndexOf("IsUpdate");
                //
                //if (update >= 0)
                //{
                //    if (bool.TryParse(arrResults["IsUpdate"].ToString(), out bool isUpdate))
                //    {
                //        if (isUpdate)
                //        {
                //            MySqlCommand cmdPrepare = new MySqlCommand("procCenterFinal", wooriMadiConn);
                //            cmdPrepare.CommandType = CommandType.StoredProcedure;
                //            cmdPrepare.Parameters.AddWithValue("@vKubun", "G");
                //            cmdPrepare.Parameters.AddWithValue("@vCenter", "랩지");
                //            cmdPrepare.Parameters.Add("@vRet", MySqlDbType.VarChar, 50);
                //            cmdPrepare.Parameters["@vRet"].Direction = ParameterDirection.Output;
                //            cmdPrepare.ExecuteNonQuery();
                //        }
                //    }
                //}

                return Ok();
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
            finally
            {
                sqlConn.Close();
                wooriMadiConn.Close();
            }
        }
    }
}