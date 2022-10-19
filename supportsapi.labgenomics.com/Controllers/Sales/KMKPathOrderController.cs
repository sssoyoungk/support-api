using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/sales/KMKPathOrder")]
    public class KMKPathOrderController : ApiController
    {
        // GET api/<controller>
        /// <summary>
        /// Select 프로시저 호출
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="isTestOutside"></param>
        /// <param name="beginNo"></param>
        /// <param name="endNo"></param>
        /// <param name="compMngCode"></param>
        /// <returns></returns>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string isTestOutside, int beginNo, int endNo, string compMngCode)
        {
            
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@BeginDate", System.Data.SqlDbType.DateTime));
            sqlParameters[0].Value = beginDate.ToString("yyyy-MM-dd");
            sqlParameters.Add(new SqlParameter("@EndDate", System.Data.SqlDbType.DateTime));
            sqlParameters[1].Value = endDate.ToString("yyyy-MM-dd");
            sqlParameters.Add(new SqlParameter("@isTestOutside", System.Data.SqlDbType.Bit));
            sqlParameters[2].Value = isTestOutside == "1" ? 0x1 : 0x0;
            sqlParameters.Add(new SqlParameter("@CompCode", System.Data.SqlDbType.VarChar));
            sqlParameters[3].Value = "000113";
            sqlParameters.Add(new SqlParameter("@AuthGroupCode", System.Data.SqlDbType.VarChar));
            sqlParameters[4].Value = "c000113";
            

            //@BeginDate datetime, --시작일
            //@EndDate datetime, --종료일
            //@isTestOutside bit, -- 등록자료 1 OR 대기자료 0
            //@CompCode varchar(10), --거래처 코드
            //@AuthGroupCode varchar(20) --거래처 아이디 코드
            //string temp = "Data Source=1.237.52.187,14032;Initial Catalog=Interface;Persist Security Info=True;User ID=kmkpath;Password=kmkpath2022";
            return Ok(LabgeDatabase.SqlProcedureToJArray("Interface_GetPathOrder", sqlParameters));
        }        

        /// <summary>
        /// 보조 서버에 인서트 후 본서버 업데이트
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHttpActionResult Post([FromBody]JArray request)
        {
            //string temp = "Data Source=1.237.52.187,14032;Initial Catalog=Interface;Persist Security Info=True;User ID=kmkpath;Password=kmkpath2022";
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["InterfaceConnection"].ConnectionString);
            conn.Open();
            try
            {
                foreach (JObject objOrder in request)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("INSERT INTO KMKPath_Interface");
                    sb.AppendLine("(LabRegDate, LabRegNo, CompCode, CompName, ChartNo, PatientName, Age, Gender, TestCode, PartCode, TestDisplayName, StateCode) VALUES");
                    sb.AppendLine($"( '{objOrder["LabRegDate"].ToString()}'");
                    sb.AppendLine($", {objOrder["LabRegNo"].ToString()}");
                    sb.AppendLine($", '{objOrder["CompCode"].ToString()}'");
                    sb.AppendLine($", '{objOrder["CompName"].ToString()}'");
                    sb.AppendLine($", '{objOrder["PatientChartNo"].ToString()}'");
                    sb.AppendLine($", '{objOrder["PatientName"].ToString()}'");
                    sb.AppendLine($", '{objOrder["PatientAge"].ToString()}'");
                    sb.AppendLine($", '{objOrder["PatientSex"].ToString()}'");
                    sb.AppendLine($", '{objOrder["TestCode"].ToString()}'");
                    sb.AppendLine($", '{GetPartCode(objOrder["TestCode"].ToString())}'");
                    sb.AppendLine($", '{objOrder["TestDisplayName"].ToString()}'");
                    sb.AppendLine($", 'O')");
                    Debug.WriteLine(sb.ToString());


                    SqlCommand cmd = new SqlCommand(sb.ToString(), conn);
                    cmd.ExecuteNonQuery();

                    sb.Clear();
                    sb.AppendLine($"UPDATE LabRegTest");
                    sb.AppendLine($"SET IsTestOutside = '1'");
                    sb.AppendLine($"  , TestStateCode = 'O'");
                    sb.AppendLine($"  , TestOutSideBeginTime = GETDATE()");
                    sb.AppendLine($"  , TestStartTime = GETDATE()");
                    sb.AppendLine($"  , IsWorkCheck = '1'");
                    sb.AppendLine($"  , WorkCheckMemberID = '{objOrder["RegistMemberID"].ToString()}'");
                    sb.AppendLine($"  , WorkCheckTime  = GETDATE()");
                    sb.AppendLine($"  , TestOutsideCompCode = '000113'");
                    sb.AppendLine($"  , TestOutsideMemberID = '{objOrder["RegistMemberID"].ToString()}'");
                    sb.AppendLine($"WHERE LabRegDate = '{Convert.ToDateTime(objOrder["LabRegDate"]).ToString("yyyy-MM-dd")}'");
                    sb.AppendLine($"AND LabRegNo = {objOrder["LabRegNo"].ToString()}");
                    sb.AppendLine($"AND TestCode = '{objOrder["TestCode"].ToString()}'");
                    sb.AppendLine();
                    sb.AppendLine($"DECLARE @ReportCode varchar(30)");
                    sb.AppendLine($"SELECT @ReportCode = ReportCode");
                    sb.AppendLine($"FROM LabTestCode");
                    sb.AppendLine($"WHERE TestCode = '{objOrder["TestCode"].ToString()}'");
                    sb.AppendLine();
                    sb.AppendLine($"UPDATE LabRegReport");
                    sb.AppendLine($"SET ReportStartTime = GETDATE()");
                    sb.AppendLine($"WHERE LabRegDate = '{Convert.ToDateTime(objOrder["LabRegDate"]).ToString("yyyy-MM-dd")}'");
                    sb.AppendLine($"AND LabRegNo = '{objOrder["LabRegNo"].ToString()}'");
                    sb.AppendLine($"AND ReportCode = @ReportCode");

                    #region backup
                    //등록이 완료되면 우리 테이블에도 update해준다.
                    //sql = $"UPDATE LabRegTest\r\n" +
                    //      $"SET IsTestOutside = '1'\r\n" +
                    //      $"  , TestStateCode = 'O'\r\n" +
                    //      $"  , TestOutSideBeginTime = GETDATE()\r\n" +
                    //      $"  , TestStartTime = GETDATE()\r\n" +
                    //      $"  , IsWorkCheck = '1'\r\n" +
                    //      $"  , WorkCheckMemberID = '{objOrder["RegistMemberID"].ToString()}'\r\n" +
                    //      $"  , WorkCheckTime  = GETDATE()\r\n" +
                    //      $"  , TestOutsideCompCode = '000113'\r\n" +
                    //      $"  , TestOutsideMemberID = '{objOrder["RegistMemberID"].ToString()}'\r\n" +
                    //      $"WHERE LabRegDate = '{Convert.ToDateTime(objOrder["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                    //      $"AND LabRegNo = {objOrder["LabRegNo"].ToString()}\r\n" +
                    //      $"AND TestCode = '{objOrder["TestCode"].ToString()}'\r\n" +
                    //      $"\r\n" +
                    //      $"DECLARE @ReportCode varchar(30)\r\n" +
                    //      $"SELECT @ReportCode = ReportCode\r\n" +
                    //      $"FROM LabTestCode\r\n" +
                    //      $"WHERE TestCode = '{objOrder["TestCode"].ToString()}'\r\n" +
                    //      $"\r\n" +
                    //      $"UPDATE LabRegReport\r\n" +
                    //      $"SET ReportStartTime = GETDATE()\r\n" +
                    //      $"WHERE LabRegDate = '{Convert.ToDateTime(objOrder["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                    //      $"AND LabRegNo = '{objOrder["LabRegNo"].ToString()}'\r\n" +
                    //      $"AND ReportCode = @ReportCode";
                    #endregion

                    Debug.WriteLine(sb.ToString());

                    LabgeDatabase.ExecuteSql(sb.ToString());
                }

                return Ok();
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);                
            }
            finally
            {
                conn.Close();
            }
        }
        /// <summary>
        /// 파트코드를 불러옴.
        /// </summary>
        /// <param name="testCode"></param>
        /// <returns></returns>
        private string GetPartCode(string testCode)
        {
            string sql;
            sql = $"SELECT PartCode\r\n" +
                  $"FROM LabTestCode\r\n" +
                  $"WHERE TestCode = '{testCode}'";
            return LabgeDatabase.ExecuteSqlScalar(sql).ToString();
        }
    }
}