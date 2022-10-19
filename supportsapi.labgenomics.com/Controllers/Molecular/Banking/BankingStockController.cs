using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Molecular.Banking
{
    public class BankingStockController : ApiController
    {
        /// <summary>
        /// 검체 재고
        /// </summary>
        /// <param name="sampleCode"></param>
        /// <returns></returns>
        [Route("api/Molecular/Banking/Stock")]
        public IHttpActionResult GetBankingStock(string sampleCode)
        {
            try
            {
                string sql;
                sql = $"SELECT bss.Barcode\r\n" +
                      $"     , CASE SUBSTRING(bss.Barcode,8,1) WHEN 'S' THEN 'Serum' WHEN 'P' THEN 'Plasma' WHEN 'U' THEN 'Urine' WHEN 'D' THEN 'DNA' END SampleName\r\n" +
                      $"     , bss.SampleVolume - ISNULL((SELECT SUM(bse.ExportVolume)\r\n" +
                      $"                                  FROM BankingSampleExport bse\r\n" +
                      $"                                  WHERE bse.BankingKind = bss.BankingKind\r\n" +
                      $"                                  AND bse.SampleCode = bss.SampleCode\r\n" +
                      $"                                  AND bss.Barcode = bse.Barcode), 0) AS SampleVolume\r\n" +
                      $"FROM BankingSampleStock bss\r\n" +
                      $"WHERE bss.SampleCode = '{sampleCode}'\r\n" +
                      $"ORDER BY bss.SampleCode, Barcode";

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
        /// 검체 재고 등록
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("api/Molecular/Banking/Stock")]
        public IHttpActionResult PostBankingStock([FromBody]JObject request)
        {
            try
            {                
                string sql;
                sql = $"INSERT INTO BankingSampleStock \r\n" +
                  $"     ( BankingKind, SampleCode, Barcode, SampleVolume, EditTime, EditMemberID ) \r\n" +
                  $"VALUES\r\n" +
                  $"     ( '{request["BankingKind"].ToString()}'\r\n" +
                  $"     , '{Services.Banking.BarcodeToSampleCode(request["BankingKind"].ToString(), request["Barcode"].ToString())}'\r\n" +
                  $"     , '{request["Barcode"].ToString()}'\r\n" +
                  $"     , {request["SampleVolume"].ToString()}" +
                  $"     , GETDATE() \r\n" +
                  $"     , '{request["MemberID"].ToString()}')";
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
        /// 검체 재고 삭제
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("api/Molecular/Banking/Stock")]
        //public IHttpActionResult DeleteBankingStock([FromBody]JObject request)
        public IHttpActionResult DeleteBankingStock(string bankingKind, string barcode)
        {
            try
            {
                string sql;
                sql = $"DELETE FROM BankingSampleStock\r\n" +
                      $"WHERE BankingKind = '{bankingKind}'\r\n" +
                      $"AND Barcode = '{barcode}'";
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