using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.StrategyBusiness
{
    [SupportsAuth]
    [Route("api/StrategyBusiness/ManageDBProject")]
    public class ManageDBProjectInfoController : ApiController
    {
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate)
        {
#if DEBUG
            string infoTable = "PGSPatientInfo_Test";

#else
            string infoTable = "PGSPatientInfo";
#endif

            string sql;
            sql =
                $"SELECT\r\n" +
                $"    ppi.CompOrderDate, ppi.CompOrderNo, ppi.Gender, ppi.Race, ppi.BirthDay, ppi.PatientName, ppi.ZipCode, ppi.Address, ppi.Address2, ppi.EmailAddress, \r\n" +
                $"    ppi.PhoneNumber, ppi.AgreePrivacyPolicyDateTime, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer, ppi.PrevTrakingNumber, ppi.PrevBarcode, ppi.TrakingNumber, ppi.ReshippedCode , ppi.AgreeThirdPartySensitive, ppi.Barcode, \r\n" +
                $"    ppi.AgreeGeneThirdPartySensitive, ppi.AgreeKeepDataAndFutureAnalysis, ppi.OrderStatus, CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo\r\n" +
                $"FROM {infoTable} ppi\r\n" +
                $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\r\n" +
                $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\r\n" +
                $"AND ltcoi.CompCode = ppi.CompCode\r\n" +
                $"WHERE ppi.CustomerCode = 'GenoCore'\r\n" +
                $"AND ppi.CompOrderDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }




        public IHttpActionResult Put([FromBody]JObject objRequest)
        {
            //업데이트

            return Ok();
        }




        public IHttpActionResult Delete(string LabRegDate, string LabRegNo)
        {
            return Ok();
        }


    }
}
