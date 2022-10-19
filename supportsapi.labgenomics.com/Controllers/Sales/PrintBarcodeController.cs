using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/PrintBarcode")]
    public class PrintBarcodeController : ApiController
    {
        [Route("api/Sales/PrintBarcode/GroupCode")]
        public IHttpActionResult GetPartCode(string groupCode)
        {
            string sql;
            sql = $"SELECT bpm.PartCode, bp.PartName\r\n" +
                  $"FROM BarcodePartMapping bpm\r\n" +
                  $"JOIN BarcodePart bp\r\n" +
                  $"ON bpm.PartCode = bp.PartCode\r\n" +
                  $"WHERE bpm.GroupCode = '{groupCode}'";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        // GET api/<controller>
        public IHttpActionResult Get(string partCode, DateTime labRegDate)
        {
            string sql;
            sql = $"MERGE INTO BarcodePrintLog AS target\r\n" +
                  $"USING (SELECT '{partCode}' AS PartCode, '{labRegDate.ToString("yyyy-MM-dd")}' AS LabRegDate) AS source\r\n" +
                  $"ON (target.PartCode = source.PartCode AND\r\n" +
                  $"    target.LabRegDate = source.LabRegDate)\r\n" +
                  $"WHEN NOT MATCHED THEN\r\n" +
                  $"    INSERT (PartCode, LabRegDate, PrintBarcodeNo)\r\n" +
                  $"    VALUES (source.PartCode, source.LabRegDate,\r\n" +
                  $"            (SELECT BeginNo - 1 FROM BarcodeRange\r\n" +
                  $"             WHERE PartCode = source.PartCode\r\n" +
                  $"             AND SetDate = (SELECT MAX(SetDate) FROM BarcodeRange WHERE SetDate <= source.LabRegDate))\r\n" +
                  $"           );\r\n" +
                  $"SELECT PrintBarcodeNo + 1 AS PrintBarcodeNo\r\n" +
                  $"FROM BarcodePrintLog\r\n" +
                  $"WHERE PartCode = '{partCode}'\r\n" +
                  $"AND LabRegDate = '{labRegDate.ToString("yyyy-MM-dd")}'";

            object barcodeNo = LabgeDatabase.ExecuteSqlScalar(sql);
            return Ok(barcodeNo);
        }

        public IHttpActionResult Put([FromBody]JObject request)
        {
            string sql;
            sql = $"UPDATE BarcodePrintLog\r\n" +
                  $"SET PrintBarcodeNo = {request["PrintBarcodeNo"].ToString()}\r\n" +
                  $"WHERE PartCode = '{request["PartCode"].ToString()}'\r\n" +
                  $"AND LabRegDate = '{request["LabRegDate"].ToString()}'";

            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }
    }
}