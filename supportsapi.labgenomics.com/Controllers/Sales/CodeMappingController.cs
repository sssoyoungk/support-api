using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [SupportsAuth]
    [Route("api/Sales/CodeMapping")]
    public class CodeMappingController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="compCode"></param>
        /// <returns></returns>
        public IHttpActionResult Get(string compCode)
        {
            string sql;
            sql = 
                $"SELECT " +
                $"    CompMatchCode, CompMatchSubCode, CompMatchName, CenterMatchCode, CenterMatchOrderCode, OrderDisplayName AS TestDisplayName,\r\n" +
                $"    CenterMatchSampleCode\r\n" +
                $"FROM LabTransMatchCode\r\n" +
                $"LEFT OUTER JOIN LabOrderCode\r\n" +
                $"ON CenterMatchCode = OrderCode\r\n" +
                $"WHERE CompCode = '{compCode}'";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

            return Ok(arrResponse);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHttpActionResult Post([FromBody]JObject request)
        {
            try
            {
                //파라메터가 변경되면서 오류가 날때를 대비한 처리
                if (!request.ContainsKey("Action"))
                    request.Add("Action", string.Empty);

                if (!request.ContainsKey("CenterMatchSampleCode"))
                    request.Add("CenterMatchSampleCode", string.Empty);

                if (!request.ContainsKey("Gongdan"))
                    request.Add("Gongdan", string.Empty);

                string sql = string.Empty;

                //중지코드 확인
                sql =
                    $"SELECT IsTestUse\r\n" +
                    $"FROM LabTestCode\r\n" +
                    $"WHERE TestCode = '{request["CenterMatchCode"]}'\r\n";
                JObject objTestCode = LabgeDatabase.SqlToJObject(sql);

                //중지코드면 에러 리턴
                if (!Convert.ToBoolean(objTestCode["IsTestUse"]))
                {
                    throw new Exception($"{request["CenterMatchCode"]}는 중지된 검사코드 입니다.");
                }


                sql = 
                    $"MERGE INTO LabTransMatchCode AS target\r\n" +
                    $"USING (SELECT '{request["CompCode"]}' AS CompCode, '{request["CompMatchCode"]}' AS CompMatchCode\r\n" +
                    $"            , '{request["CompMatchSubCode"]}' AS CompMatchSubCode\r\n" +
                    $"            , '{request["Gongdan"]}' AS Gongdan) AS source\r\n" +
                    $"ON (target.CompCode = source.CompCode AND target.CompMatchCode = source.CompMatchCode AND target.CompMatchSubCode = source.CompMatchSubCode)\r\n" +
                    $"WHEN MATCHED THEN\r\n" +
                    $"    UPDATE SET CenterGongDanCode = (CASE WHEN source.Gongdan = 'Y' THEN '{request["CenterMatchCode"]}' ELSE target.CenterGongDanCode END)\r\n" +
                    $"             , CenterMatchCode = (CASE WHEN source.Gongdan <> 'Y' THEN '{request["CenterMatchCode"]}' ELSE target.CenterMatchCode END)\r\n" +
                    $"             , CenterMatchOrderCode = '{request["CenterMatchOrderCode"]}'\r\n" +
                    $"             , CenterMatchSampleCode = '{request["CenterMatchSampleCode"]}'\r\n" +
                    $"             , EditTime = GETDATE()\r\n" +
                    $"             , EditormemberID = '{request["RegistMemberID"]}'\r\n" +
                    $"WHEN NOT MATCHED THEN\r\n" +
                    $"    INSERT ( CompCode, CompMatchCode\r\n" +
                    $"           , CompMatchSubCode, CompMatchName\r\n" +
                    $"           , CenterMatchCode, CenterMatchOrderCode\r\n" +
                    $"           , CenterMatchSampleCode\r\n" +
                    $"           , RegistTime, RegistMemberID)\r\n" +
                    $"    VALUES ( source.CompCode, source.CompMatchCode\r\n" +
                    $"           , source.CompMatchSubCode, '{request["CompMatchName"].ToString().Replace("'", "''")}'\r\n" +
                    $"           , '{request["CenterMatchCode"]}', '{request["CenterMatchOrderCode"]}'\r\n" +
                    $"           , '{request["CenterMatchSampleCode"]}'\r\n" +
                    $"           , GETDATE(), '{request["RegistMemberID"]}');";

                LabgeDatabase.ExecuteSql(sql);

                return Ok();
            }
            catch (Exception ex)
            {                
                string logPath = HttpContext.Current.Server.MapPath("/");
                logPath += $"Log\\CodeMapping\\{DateTime.Now:yyyy}\\{DateTime.Now:MMdd}";
                DirectoryInfo logDirInfo = new DirectoryInfo(logPath);
                if (!logDirInfo.Exists)
                {
                    logDirInfo.Create();
                }
                File.WriteAllText(logDirInfo + "\\" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss_fff") + ".json", request.ToString(), Encoding.UTF8);

                JObject objResponse = new JObject
                {
                    { "Status", Convert.ToInt32(HttpStatusCode.BadRequest) },
                    { "Message", ex.Message }
                };
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compCode"></param>
        /// <param name="compMatchCode"></param>
        /// <param name="compMatchSubCode"></param>
        /// <returns></returns>
        public IHttpActionResult Delete(string compCode, string compMatchCode, string compMatchSubCode)
        {
            try
            {
                string sql;
                sql = $"DELETE FROM LabTransMatchCode\r\n" +
                      $"WHERE CompCode = '{compCode}'\r\n" +
                      $"AND CompMatchCode = '{compMatchCode}'\r\n" +
                      $"AND CompMatchSubCode = '{compMatchSubCode}'";

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