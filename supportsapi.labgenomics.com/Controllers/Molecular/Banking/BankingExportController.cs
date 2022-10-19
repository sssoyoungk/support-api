using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Molecular.Banking
{
    [Route("api/Molecular/Banking/Export")]
    public class BankingExportController : ApiController
    {
        /// <summary>
        /// 출고 검체 조회
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        
        public IHttpActionResult Get(string sampleCode)
        {
            try
            {
                string sql;
                sql = $"SELECT *\r\n" +
                      $"FROM BankingSampleExport\r\n" +
                      $"WHERE SampleCode = '{sampleCode}'\r\n" +
                      $"ORDER BY ExportDate";

                JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
                return Ok(arrResponse);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }

        /// <summary>
        /// 출고 검체 등록
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        public IHttpActionResult Post([FromBody]JObject request)
        {
            try
            {
                string sql;
                sql = $"INSERT INTO BankingSampleExport\r\n" +
                      $"    (BankingKind, SampleCode, Barcode, ExportDate, ExportVolume,\r\n" +
                      $"     Description, InsertTime, InsertMemberID)\r\n" +
                      $"VALUES\r\n" +
                      $"    ('{request["BankingKind"].ToString()}'\r\n" +
                      $"    , '{Services.Banking.BarcodeToSampleCode(request["BankingKind"].ToString(), request["Barcode"].ToString())}'\r\n" +
                      $"    , '{request["Barcode"].ToString()}', '{request["ExportDate"].ToString()}', {request["ExportVolume"].ToString()}\r\n" +
                      $"    , '{request["Description"].ToString()}', GETDATE(), '{request["MemberID"].ToString()}')";
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

        /// <summary>
        /// 출고 검체 수정
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        public IHttpActionResult Put([FromBody]JObject request)
        {
            try
            {
                string sql;
                sql = $"UPDATE BankingSampleExport\r\n" +
                      $"SET\r\n" +
                      $"    ExportDate = '{request["ExportDate"].ToString()}',\r\n" +
                      $"    ExportVolume = {request["ExportVolume"].ToString()},\r\n" +
                      $"    Description = '{request["Description"].ToString()}'\r\n" +
                      $"WHERE BankingKind = '{request["BankingKind"].ToString()}'\r\n" +
                      $"AND Barcode = '{request["Barcode"].ToString()}'";
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

        /// <summary>
        /// 출고검체 삭제
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        public IHttpActionResult Delete(string exportID)
        {
            string sql;
            sql = $"DELETE FROM BankingSampleExport\r\n" +
                  $"WHERE ExportID = '{exportID}'";
            LabgeDatabase.ExecuteSql(sql);

            return Ok();
        }
    }
}