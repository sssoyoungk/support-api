using ExcelDataReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.StrategyBusiness
{
    [SupportsAuth]
    [Route("api/StrategyBusiness/ManageAmorePacific")]
    public class ManageAmorePacificController : ApiController
    {
        /// <summary>
        /// 접수 파일 업로드
        /// </summary>
        /// <returns></returns>
        [Route("api/StrategyBusiness/ManageAmorePacific/UploadFile")]
        public async Task<HttpResponseMessage> PostUploadFile()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                string root = HttpContext.Current.Server.MapPath("/" + @"LabImportFile/AmorePacificFiles");
                Directory.CreateDirectory(root);
                var provider = new MultipartFormDataStreamProvider(root);

                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (MultipartFileData file in provider.FileData)
                {
                    var dataBytes = File.ReadAllBytes(file.LocalFileName);
                    var dataStream = new MemoryStream(dataBytes);
                    IExcelDataReader reader = ExcelReaderFactory.CreateReader(dataStream);
                    var conf = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    };
                    var dataSet = reader.AsDataSet(conf);
                    DataTable dt = dataSet.Tables[0];
                    JArray arrOrders = JArray.Parse(JsonConvert.SerializeObject(dt));
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, objResponse.ToString());
            }
        }
        /// <summary>
        /// 조회
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string mode)
        {
            string sql;
            if (mode == "Ordered")
            {
                sql =
                    $"SELECT\r\n" +
                    $"    ppi.CompOrderDate, ppi.CompOrderNo, ppi.Gender, CONVERT(varchar, ppi.BirthDay, 23) as BirthDay, ppi.PatientName, ppi.ZipCode, ppi.Address, ppi.Address2, ppi.EmailAddress, \r\n" +
                    $"    ppi.PhoneNumber, ppi.AgreeRequestTest, ppi.AgreePrivacyPolicy, ppi.AgreeLabgePrivacyPolicy, ppi.AgreePrivacyPolicyDateTime, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer,\r\n" +
                    $"    ppi.PrevTrackingNumber, ppi.PrevBarcode, ppi.TrackingNumber, ppi.ReshippedCode , ppi.Barcode, \r\n" +
                    $"    ppi.AgreeGeneThirdPartySensitive,ppi.AgreeSendResultEmail, ppi.AgreeKeepDataAndFutureAnalysis, ppi.OrderStatus, CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo\r\n" +
                    $"FROM PGSPatientInfo ppi\r\n" +
                    $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                    $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\r\n" +
                    $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\r\n" +
                    $"AND ltcoi.CompCode = ppi.CompCode\r\n" +
                    $"WHERE ppi.CustomerCode = 'amorepacific'\r\n" +
                    $"AND ppi.CompOrderDate BETWEEN '{beginDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}'\r\n" +
                    $"AND (ppi.Server <> 'Develop' or ppi.Server is null)\r\n" +
                    $"AND (ppi.OrderStatus is null or ppi.OrderStatus = 'Ordered') ";
            }
            else
            {
                sql =
                    $"SELECT\r\n" +
                    $"    ppi.CompOrderDate, ppi.CompOrderNo, ppi.Gender, CONVERT(varchar, ppi.BirthDay, 23) as BirthDay, ppi.PatientName, ppi.ZipCode, ppi.Address, ppi.Address2, ppi.EmailAddress,\r\n" +
                    $"    ppi.PhoneNumber, ppi.AgreeRequestTest, ppi.AgreePrivacyPolicy, ppi.AgreeLabgePrivacyPolicy ,ppi.AgreePrivacyPolicyDateTime, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer,\r\n" +
                    $"    ppi.PrevTrackingNumber, ppi.PrevBarcode, ppi.TrackingNumber, ppi.ReshippedCode , ppi.Barcode, \r\n" +
                    $"    ppi.AgreeGeneThirdPartySensitive,AgreeSendResultEmail, ppi.AgreeKeepDataAndFutureAnalysis, ppi.OrderStatus, CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo,\r\n" +
                    $"    CONVERT(varchar(19), lrr.ReportTransEndTime, 21) AS ReportTransEndTime, ISNULL(lrr.IsReportTransEnd, 0) as IsReportTransEnd\r\n" +
                    $"FROM PGSPatientInfo ppi\r\n" +
                    $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                    $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\r\n" +
                    $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\r\n" +
                    $"AND ltcoi.CompCode = ppi.CompCode\r\n" +
                    $"LEFT outer join LabRegReport lrr\n" +
                    $"ON lrr.LabRegDate = ltcoi.LabRegDate\n" +
                    $"AND lrr.LabRegNo  = ltcoi.LabRegNo\n" +
                    $"WHERE ppi.CustomerCode = 'amorepacific'\r\n" +
                    $"AND ppi.CompOrderDate BETWEEN '{beginDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}'\r\n" +
                    $"AND (ppi.Server <> 'Develop' or ppi.Server is null)\r\n" +
                    $"AND ppi.OrderStatus != 'Ordered'";
            }

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        public IHttpActionResult Put([FromBody] JArray arrRequest)
        {
            try
            {
                foreach (JObject objRequest in arrRequest)
                {
                    string sql;
                    sql = $"UPDATE PGSPatientInfo\r\n" +
                          $"SET ZipCode = '{objRequest["ZipCode"]}'\r\n" +
                          $"  , Address = '{objRequest["Address"]}'\r\n" +
                          $"  , Address2 = '{objRequest["Address2"]}'\r\n" +
                          $"  , PatientRegNo = '{objRequest["PatientRegNo"]}'\r\n" +
                          $"  , BirthDay = '{objRequest["BirthDay"]}'\r\n" +
                          $"  , EmailAddress = '{objRequest["EmailAddress"]}'\r\n" +
                          $"  , PhoneNumber = '{objRequest["PhoneNumber"]}'\r\n" +
                          $"  , AgreeGeneTest = '{objRequest["agreeGeneTest"]}'\r\n" +
                          $"  , AgreeRequestTest = '{objRequest["agreeRequestTest"]}'\r\n" +
                          $"  , AgreeLabgePrivacyPolicy = '{objRequest["agreeLabgePrivacyPolicy"]}'\r\n" +
                          $"  , AgreeThirdPartyOffer = '{objRequest["agreeThirdPartyOffer"]}'\r\n" +
                          $"  , AgreeSendResultEmail = '{objRequest["agreeSendResultEmail"]}'\r\n" +
                          $"WHERE CompOrderDate = '{Convert.ToDateTime(objRequest["CompOrderDate"]):yyyy-MM-dd}'\r\n" +
                          $"AND CompOrderNo = '{objRequest["CompOrderNo"]}'\r\n" +
                          $"AND CustomerCode = 'amorepacific' ";
                    LabgeDatabase.ExecuteSql(sql);
                }
                return Ok();
            }
            catch (HttpException ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", ex.GetHttpCode());
                objResponse.Add("Message", ex.Message);
                HttpStatusCode code = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), ex.GetHttpCode().ToString());
                return Content((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), ex.GetHttpCode().ToString()), objResponse);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }

        }

        [Route("api/StrategyBusiness/ManageAmorePacific/CancelOrder")]
        public IHttpActionResult PutCancelOrder(JObject objRequest)
        {
            try
            {
                string sql;
                sql =
                    $"UPDATE PGSPatientInfo\r\n" +
                    $"SET OrderStatus = 'Canceled'\r\n" +
                    $"WHERE CompOrderDate = '{Convert.ToDateTime(objRequest["CompOrderDate"]):yyyy-MM-dd}'\r\n" +
                    $"AND CompOrderNo = '{objRequest["CompOrderNo"]}'\r\n" +
                    $"AND CustomerCode = 'amorepacific'";
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