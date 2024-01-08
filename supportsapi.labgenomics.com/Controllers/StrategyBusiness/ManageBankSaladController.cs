using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.StrategyBusiness
{
    [SupportsAuth]
    public class ManageBankSaladController : ApiController
    {
        // GET api/<controller>
        [Route("api/StrategyBusiness/ManageBankSalad")]
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string mode, string testCode = "")
        {
            string sql;
            string subQuery = "";
            if (testCode != null && testCode != string.Empty)
            {
                if(testCode == "60028")
                {
                    subQuery = $"AND pi2.CompTestCode in ('60028', '60040') \r\n";
                }
                else
                {
                    subQuery = $"AND pi2.CompTestCode = '{testCode}' \r\n";
                }
            }

            if (mode == "Ordered")
            {
                sql =
                    $"SELECT pi2.CompTestCode, pi2.CompTestName, \r\n" +
                    $"    ppi.CompOrderDate, ppi.CompOrderNo, ppi.Gender, ppi.Race, ppi.BirthDay, ppi.PatientName, ppi.ZipCode, ppi.Address, ppi.Address2, ppi.EmailAddress, \r\n" +
                    $"    ppi.PhoneNumber, ppi.AgreePrivacyPolicyDateTime, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer, ppi.PrevTrackingNumber, ppi.PrevBarcode, ppi.TrackingNumber, ppi.ReshippedCode , ppi.AgreeThirdPartySensitive, ppi.Barcode, \r\n" +
                    $"    ppi.AgreeGeneThirdPartySensitive, ppi.AgreeKeepDataAndFutureAnalysis, ppi.OrderStatus, CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo\r\n" +
                    $"FROM PGSPatientInfo ppi\r\n" +
                    $"join PGSTestInfo pi2\n" +
                    $"on pi2.CompOrderDate  = ppi.CompOrderDate\n" +
                    $"and pi2.CompOrderNo  = ppi.CompOrderNo\n" +
                    $"and pi2.CustomerCode = ppi.CustomerCode\n" +
                    $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                    $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\r\n" +
                    $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\r\n" +
                    $"AND ltcoi.CompCode = ppi.CompCode\r\n" +
                    $"WHERE ppi.CustomerCode = 'banksalad'\r\n" +
                    $"{subQuery}" +
                    $"AND ppi.CompOrderDate BETWEEN '{beginDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}'\r\n" +
                    $"AND (ppi.Server <> 'Develop' or ppi.Server is null)\r\n" +
                    $"AND ppi.OrderStatus = 'Ordered'";
            }
            else if (mode == "remainder")
            {
                sql =
                    "SET ARITHABORT ON\r\n" +
                    $"SELECT pi2.CompTestCode, pi2.CompTestName,\r\n" +
                    $"    ppi.CompOrderDate, ppi.CompOrderNo, ppi.Gender, ppi.Race, ppi.BirthDay, ppi.PatientName, ppi.ZipCode, ppi.Address, ppi.Address2,  ppi.EmailAddress, \r\n" +
                    $"    ppi.PhoneNumber, ppi.AgreePrivacyPolicyDateTime, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer, ppi.AgreeThirdPartySensitive, ppi.ReshippedCode, ppi.PrevTrackingNumber, ppi.PrevBarcode, ppi.TrackingNumber, ppi.Barcode,\r\n" +
                    $"    ppi.AgreeGeneThirdPartySensitive, ppi.AgreeKeepDataAndFutureAnalysis, ppi.OrderStatus, CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo, CONVERT(varchar(19), lrr.ReportTransEndTime, 21) AS ReportTransEndTime, CONVERT(varchar(19), lrr.ReportEndTime, 21) AS ReportEndTime, ISNULL(lrr.IsReportTransEnd, 0) as IsReportTransEnd\r\n" +
                    $"FROM PGSPatientInfo ppi\r\n" +
                    $"join PGSTestInfo pi2\r\n" +
                    $"on pi2.CompOrderDate  = ppi.CompOrderDate\r\n" +
                    $"and pi2.CompOrderNo  = ppi.CompOrderNo\r\n" +
                    $"and pi2.CustomerCode = ppi.CustomerCode\r\n" +
                    $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                    $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\r\n" +
                    $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\r\n" +
                    $"AND ltcoi.CompCode = ppi.CompCode\r\n" +
                    $"LEFT outer join LabRegReport lrr\r\n" +
                    $"ON lrr.LabRegDate = ltcoi.LabRegDate\r\n" +
                    $"AND lrr.LabRegNo  = ltcoi.LabRegNo\r\n" +
                    $"WHERE ppi.CustomerCode = 'banksalad'\r\n" +
                    $"{subQuery}" +
                    $"AND ppi.CompOrderDate BETWEEN '{beginDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}'\r\n" +
                    $"AND (ppi.Server <> 'Develop' or ppi.Server is null)\r\n" +
                    $"AND ppi.OrderStatus != 'Ordered'\r\n" +
                    $"order by ppi.CompOrderDate ";
            }
            else
            {
                sql =
                    $"SELECT pi2.CompTestCode, pi2.CompTestName,\r\n" +
                    $"    ppi.CompOrderDate, ppi.CompOrderNo, ppi.Gender, ppi.Race, ppi.BirthDay, ppi.PatientName, ppi.ZipCode, ppi.Address, ppi.Address2, ppi.CheckSendCollectReSample, ppi.EmailAddress,\r\n" +
                    $"    ppi.PhoneNumber, ppi.AgreePrivacyPolicyDateTime, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer, ppi.AgreeThirdPartySensitive, ppi.ReshippedCode, ppi.PrevBarcode, ppi.PrevTrackingNumber, ppi.TrackingNumber, ppi.Barcode,\r\n" +
                    $"    ppi.AgreeGeneThirdPartySensitive, ppi.AgreeKeepDataAndFutureAnalysis, ppi.OrderStatus, CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo\r\n" +
                    $"FROM PGSPatientInfo ppi\r\n" +
                    $"join PGSTestInfo pi2\r\n" +
                    $"on pi2.CompOrderDate  = ppi.CompOrderDate\n" +
                    $"and pi2.CompOrderNo  = ppi.CompOrderNo\n" +
                    $"and pi2.CustomerCode = ppi.CustomerCode\n" +
                    $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                    $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\r\n" +
                    $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\r\n" +
                    $"AND ltcoi.CompCode = ppi.CompCode\r\n" +
                    $"WHERE ppi.CustomerCode = 'banksalad'\r\n" +
                    $"{subQuery}" +
                    $"AND ppi.CompOrderDate BETWEEN '{beginDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}'\r\n" +
                    $"AND (ppi.Server <> 'Develop' or ppi.Server is null)\r\n" +
                    $"AND ppi.OrderStatus in('Returned', 'Reshipped') AND ppi.PrevBarcode != '' \r\n" +
                    $"AND ppi.CheckSendCollectReSample = '0' " +
                    $"order by ppi.CompOrderDate ";
            }
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/StrategyBusiness/ManageBankSalad/FindPatient")]
        public IHttpActionResult GetFindPatient(string patientName, string phoneNumber, string birthDay, string testCode = "")
        {
            string sql;
            string subQuery = "";

            if (patientName != null && patientName != string.Empty)
            {
                subQuery += $"AND ppi.PatientName = '{patientName}' \r\n";
            }

            if (phoneNumber != null && phoneNumber != string.Empty)
            {
                subQuery += $"AND ppi.PhoneNumber = '{phoneNumber}' \r\n";
            }

            if (birthDay != null && birthDay != string.Empty)
            {
                subQuery += $"AND ppi.BirthDay = '{birthDay}' \r\n";
            }

            if (testCode != null && testCode != string.Empty)
            {
                subQuery += $"AND pti.CompTestCode = '{testCode}' \r\n";
            }


            sql =
                $"SELECT pti.CompTestCode, pti.CompTestName, \r\n" +
                $"    ppi.CompOrderDate, ppi.CompOrderNo, ppi.BirthDay, ppi.PatientName,  ppi.CheckSendCollectReSample, ppi.EmailAddress, \r\n" +
                $"    ppi.PhoneNumber,  ppi.PrevBarcode, ppi.PrevTrackingNumber, ppi.TrackingNumber, ppi.Barcode, \r\n" +
                $"    ppi.OrderStatus, CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo\r\n" +
                $"FROM PGSPatientInfo ppi\r\n" +
                $"join PGSTestInfo pti\n" +
                $"on pti.CompOrderDate  = ppi.CompOrderDate\n" +
                $"and pti.CompOrderNo  = ppi.CompOrderNo\n" +
                $"and pti.CustomerCode = ppi.CustomerCode\n" +
                $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\r\n" +
                $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\r\n" +
                $"AND ltcoi.CompCode = ppi.CompCode\r\n" +
                $"WHERE ppi.CustomerCode = 'banksalad'\r\n" +
                $"{subQuery}" +
                $"order by ppi.CompOrderDate ";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }
    }
}