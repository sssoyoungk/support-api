﻿using ExcelDataReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Data;
using System.IO;
using System.Linq;
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
        public IHttpActionResult PostUploadFile(JObject objRequest)
        {
            try
            {
                //base64 string을 엑셀파일로 디코딩.
                byte[] dataBytes = Convert.FromBase64String(objRequest["ReceiptFile"].ToString());

                //디코딩된 엑셀파일을 메모리스트림에 올린다.
                MemoryStream stream = new MemoryStream(dataBytes);

                //엑셀파일을 읽어서 table에 저장
                IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                };

                DataSet dataSet = reader.AsDataSet(conf);
                string sql;
                foreach (DataRow dr in dataSet.Tables[0].Rows)
                {
                    try
                    {
                        sql =
                            $"INSERT INTO PGSPatientInfo\r\n" +
                            $"(\r\n" +
                            $"    CustomerCode, CompCode, CompOrderDate, CompOrderNo, PatientName, Address, ZipCode, PhoneNumber,\r\n" +
                            $"    OrderStatus\r\n" +
                            $")\r\n" +
                            $"VALUES\r\n" +
                            $"(\r\n" +
                            $"    'amorepacific', 'AP01',\r\n" +
                            $"    '{Convert.ToDateTime(dr["주문접수완료일시"]):yyyy-MM-dd}', '{dr["주문번호"]}', '{dr["주문자"]}', '{dr["주소"]}', '{dr["우편번호"]}', '{dr["주문자휴대전화번호"]}',\r\n" +
                            $"    'Ordered'\r\n" +
                            $")\r\n" +
                            $"\r\n" +
                            $"INSERT INTO PGSTestInfo\r\n" +
                            $"(\r\n" +
                            $"    CustomerCode, CompCode, CompOrderDate, CompOrderNo, CompTestCode, CompTestName\r\n" +
                            $")\r\n" +
                            $"VALUES\r\n" +
                            $"(\r\n" +
                            $"    'amorepacific', 'AP01', '{Convert.ToDateTime(dr["주문접수완료일시"]):yyyy-MM-dd}', '{dr["주문번호"]}', '60047', '2024위드진69AP'\r\n" +
                            $")\r\n";
                        LabgeDatabase.ExecuteSql(sql);
                    }
                    catch
                    {

                    }
                }

                //메모리스트림 메모리 반환
                stream.Dispose();

                return Ok();
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject
                {
                    { "Status", Convert.ToInt32(HttpStatusCode.BadRequest) },
                    { "Message", ex.Message }
                };
                return Content(HttpStatusCode.BadRequest, objResponse);
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
                    $"    ppi.AgreeGeneThirdPartySensitive,ppi.AgreeSendResultEmail, ppi.AgreeKeepDataAndFutureAnalysis, ppi.OrderStatus, CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo,\r\n" +
                    $"    ppi.DeliveryCompleteDateTime, ppi.TestResultReceiveMethod\r\n" +
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
                    $"    CONVERT(varchar(19), lrr.ReportTransEndTime, 21) AS ReportTransEndTime, ISNULL(lrr.IsReportTransEnd, 0) as IsReportTransEnd,\r\n" +
                    $"    ppi.DeliveryCompleteDateTime, ppi.TestResultReceiveMethod\r\n" +
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
                foreach (JObject objRequest in arrRequest.Cast<JObject>())
                {
                    string birthday = string.Empty;
                    if (DateTime.TryParse(objRequest["BirthDay"].ToString(), out DateTime parseBirthday))
                    {
                        birthday = parseBirthday.ToString("yyyy-MM-dd");
                    }

                    string sql;
                    sql = $"UPDATE PGSPatientInfo\r\n" +
                          $"SET ZipCode = '{objRequest["ZipCode"]}'\r\n" +
                          $"  , Address = '{objRequest["Address"]}'\r\n" +
                          $"  , Address2 = '{objRequest["Address2"]}'\r\n" +
                          $"  , PatientRegNo = '{objRequest["PatientRegNo"]}'\r\n" +
                          $"  , BirthDay = CASE WHEN ISNULL('{birthday}', '') = '' THEN NULL ELSE '{birthday}' END\r\n" +
                          $"  , EmailAddress = '{objRequest["EmailAddress"]}'\r\n" +
                          $"  , PhoneNumber = '{objRequest["PhoneNumber"].ToString().Replace("-", "")}'\r\n" +
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
                JObject objResponse = new JObject
                {
                    { "Status", ex.GetHttpCode() },
                    { "Message", ex.Message }
                };
                HttpStatusCode code = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), ex.GetHttpCode().ToString());
                return Content((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), ex.GetHttpCode().ToString()), objResponse);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject
                {
                    { "Status", Convert.ToInt32(HttpStatusCode.BadRequest) },
                    { "Message", ex.Message }
                };
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }

        public IHttpActionResult Delete(DateTime compOrderDate, string compOrderNo)
        {
            try
            {
                string sql;

                //접수정보가 있는지 확인
                sql =
                    $"SELECT COUNT(*)\r\n" +
                    $"FROM LabTransCompOrderInfo ltcoi\r\n" +
                    $"JOIN PGSPatientInfo ppi\r\n" +
                    $"ON ppi.CompOrderDate = ltcoi.CompOrderDate\r\n" +
                    $"AND ppi.CompOrderNo = ltcoi.CompOrderNo\r\n" +
                    $"AND ppi.CustomerCode = 'amorepacific'\r\n" +
                    $"WHERE ltcoi.CompOrderDate = '{compOrderDate:yyyy-MM-dd}'\r\n" +
                    $"AND ltcoi.CompOrderNo = '{compOrderNo}'";

                int count = Convert.ToInt32(LabgeDatabase.ExecuteSqlScalar(sql));

                //접수정보가 있으면 오류
                if (count > 0)
                {
                    throw new Exception("이미 랩센터에 접수된 고객입니다.");
                }
                //접수정보가 없으면 삭제 진행
                else
                {
                    sql =
                        $"DELETE FROM PGSPatientInfo\r\n" +
                        $"WHERE CustomerCode = 'amorepacific'\r\n" +
                        $"AND CompOrderDate = '{compOrderDate:yyyy-MM-dd}'\r\n" +
                        $"AND CompOrderNo = '{compOrderNo}'";
                    LabgeDatabase.ExecuteSql(sql);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject
                {
                    { "Status", Convert.ToInt32(HttpStatusCode.BadRequest) },
                    { "Message", ex.Message }
                };
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
                JObject objResponse = new JObject
                {
                    { "Status", Convert.ToInt32(HttpStatusCode.BadRequest) },
                    { "Message", ex.Message }
                };
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }

        [Route("api/StrategyBusiness/ManageAmorePacific/DeliveryComplete")]
        public IHttpActionResult PutDeliveryComplete(JObject objRequest)
        {
            try
            {

                string sql;
                sql =
                    $"SELECT\r\n" +
                    $"    ppi.CompOrderDate, ppi.CompOrderNo, ppi.Gender, CONVERT(varchar, ppi.BirthDay, 23) as BirthDay, ppi.PatientName, ppi.ZipCode, ppi.Address, ppi.Address2, ppi.EmailAddress, \r\n" +
                    $"    ppi.PhoneNumber, ppi.AgreeRequestTest, ppi.AgreePrivacyPolicy, ppi.AgreeLabgePrivacyPolicy, ppi.AgreePrivacyPolicyDateTime, ppi.AgreeGeneTest, ppi.AgreeThirdPartyOffer,\r\n" +
                    $"    ppi.PrevTrackingNumber, ppi.PrevBarcode, ppi.TrackingNumber, ppi.ReshippedCode , ppi.Barcode, \r\n" +
                    $"    ppi.AgreeGeneThirdPartySensitive,ppi.AgreeSendResultEmail, ppi.AgreeKeepDataAndFutureAnalysis, ppi.OrderStatus, CONVERT(varchar, ltcoi.LabRegDate, 23) AS LabRegDate, ltcoi.LabRegNo,\r\n" +
                    $"    ppi.DeliveryCompleteDateTime\r\n" +
                    $"FROM PGSPatientInfo ppi\r\n" +
                    $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                    $"ON ltcoi.CompOrderDate = ppi.CompOrderDate\r\n" +
                    $"AND ltcoi.CompOrderNo = ppi.CompOrderNo\r\n" +
                    $"AND ltcoi.CompCode = ppi.CompCode\r\n" +
                    $"WHERE ppi.CustomerCode = 'amorepacific'\r\n" +
                    $"AND ppi.Barcode = '{objRequest["Barcode"]}'";

                JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

                if (arrResponse.Count > 0)
                {
                    sql =
                        $"UPDATE PGSPatientInfo\r\n" +
                        $"SET DeliveryCompleteDateTime = GETDATE()\r\n" +
                        $"WHERE CustomerCode = 'amorepacific'\r\n" +
                        $"AND Barcode = '{objRequest["Barcode"]}'";
                    LabgeDatabase.ExecuteSql(sql);
                }
                return Ok(arrResponse);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject
                {
                    { "Status", Convert.ToInt32(HttpStatusCode.BadRequest) },
                    { "Message", ex.Message }
                };
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }
    }
}