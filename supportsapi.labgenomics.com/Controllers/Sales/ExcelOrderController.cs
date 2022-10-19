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
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/CompRegistOrder/ExcelOrder")]
    public class ExcelOrderController : ApiController
    {
        /// <summary>
        /// GET
        /// </summary>
        /// <param name="compCode"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public IHttpActionResult Get(string compCode, DateTime beginDate, DateTime endDate)
        {
            string sql;

            sql = $"SELECT ImportFileID, ImportCode, FileDisplayName + '.' + FileExt AS FileDisplayName, IsDataConvert, IsRegistOrder, FileStoragePath, FileStorageName\r\n" +
                  $"     , CONVERT(varchar, FileStorageSaveTime, 21) AS FileStorageSaveTime\r\n" +
                  $"FROM LabImportFile\r\n" +
                  $"WHERE ImportCompCode = '{compCode}'\r\n" +
                  $"AND FileStorageSaveTime >= '{beginDate.ToString("yyyy-MM-dd")}'\r\n" +
                  $"AND FileStorageSaveTime < '{endDate.AddDays(1).ToString("yyyy-MM-dd")}'\r\n" +
                  $"ORDER BY FileStorageSaveTime DESC";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

            return Ok(arrResponse);
        }

        /// <summary>
        /// POST
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHttpActionResult Post([FromBody]JObject request)
        {
            try
            {
                string sql;
                DataTable dt = new DataTable();
                sql =
                    $"SELECT ImportSkipRowCount\r\n" +
                    $"FROM LabImportCode\r\n" +
                    $"WHERE ImportCode = '{request["ImportCode"].ToString()}'";
                JObject objCodeSetup = LabgeDatabase.SqlToJObject(sql);
                int skipCount = Convert.ToInt32(objCodeSetup["ImportSkipRowCount"]);
                //bool useHeaderRow = Convert.ToBoolean(objCodeSetup["IsImportFirstHeader"]);


                //설정 불러오기
                sql = $"SELECT lis.ImportCode, lis.ImportFieldSeqNo, lis.ImportFieldIndex, lifc.ImportFieldName, lifc.ImportFieldExpression\r\n" +
                      $"FROM LabImportSetup lis\r\n" +
                      $"JOIN LabImportFieldCode lifc\r\n" +
                      $"ON lis.ImportFieldCode = lifc.ImportFieldCode\r\n" +
                      $"WHERE lis.ImportCode = '{request["ImportCode"].ToString()}'\r\n" +
                      $"ORDER BY lis.ImportFieldIndex\r\n";
                JArray arrFieldSetup = LabgeDatabase.SqlToJArray(sql);

                //엑셀 파일을 DataTable로 변환
                string root = HttpContext.Current.Server.MapPath(request["FilePath"].ToString());
                string file = root + request["FileStorageName"].ToString();
                if (File.Exists(file))
                {
                    var dataBytes = File.ReadAllBytes(file);
                    var dataStream = new MemoryStream(dataBytes);
                    //IExcelDataReader reader = ExcelReaderFactory.CreateReader(dataStream);
                    IExcelDataReader reader = ExcelReaderFactory.CreateReader(dataStream, new ExcelReaderConfiguration()
                    {
                        FallbackEncoding = Encoding.GetEncoding(949)
                    });
                    var conf = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = false
                        }
                    };
                    var dataSet = reader.AsDataSet(conf);
                    dt = dataSet.Tables[0];
                }
                else
                {
                    throw new Exception("파일이 서버에 없습니다.");
                }

                if (request["CompCode"].ToString() == "2728433")
                {
                    dt.Columns.Add("TestCode", typeof(string));
                }

                int rowNum = -1;
                //데이터를 테이블에 저장
                foreach (DataRow dr in dt.Rows)
                {
                    rowNum++;
                    if (rowNum < skipCount)
                    {
                        continue;
                    }

                    string sqlColumns = string.Empty;
                    string sqlValues = string.Empty;

                    foreach (JObject objSetup in arrFieldSetup)
                    {
                        sqlColumns += $"  , {objSetup["ImportFieldName"].ToString()}\r\n";

                        string value = string.Empty;
                        if (objSetup["ImportFieldExpression"].ToString() != string.Empty)
                        {
                            value = string.Format(objSetup["ImportFieldExpression"].ToString(), dr[Convert.ToInt32(objSetup["ImportFieldIndex"]) - 1].ToString().Replace("'", "''"));
                        }
                        else
                        {
                            value = $"'{dr[Convert.ToInt32(objSetup["ImportFieldIndex"]) - 1].ToString().Replace("'", "''")}'";
                        }
                        sqlValues += $"  , {value}\r\n";
                    }
                    sqlColumns = $"INSERT INTO LabImportData\r\n" +
                                 $"(\r\n" +
                                 $"    ImportFileID\r\n" +
                                 $"  , ImportCode\r\n" +
                                 $"  , CompCode\r\n" +
                                 $"  , RegistTime\r\n" +
                                 $"  , RegistMemberID\r\n" +
                                 $"{sqlColumns}" +
                                 $")";
                    sqlValues = $"VALUES\r\n" +
                                $"(\r\n" +
                                $"    '{request["ImportFileID"].ToString()}'\r\n" +
                                $"  , '{request["ImportCode"].ToString()}'\r\n" +
                                $"  , '{request["CompCode"].ToString()}'" +
                                $"  , GETDATE()\r\n" +
                                $"  , '{request["MemberID"].ToString()}'\r\n" +
                                $"{sqlValues}" +
                                $")";

                    sql = sqlColumns + sqlValues;

                    int executeRow = LabgeDatabase.ExecuteSql(sql);

                    //엑셀 순서대로 정렬해야 해서 10밀리초 대기한다.
                    Thread.Sleep(10);
                }

                //빈데이터는 삭제한다.
                sql = $"DELETE\r\n" +
                      $"FROM LabImportData\r\n" +
                      $"WHERE ImportFileID = '{request["ImportFileID"].ToString()}'\r\n" +
                      $"AND ISNULL(PatientName , '') = ''\r\n" +
                      $"AND ISNULL(CompOrderCode, '') = ''";
                LabgeDatabase.ExecuteSql(sql);

                //저장이 완료되면 변환 완료 처리 플래그 업데이트
                sql = $"UPDATE LabImportFile\r\n" +
                      $"SET IsDataConvert = 1\r\n" +
                      $"WHERE ImportFileID = '{request["ImportFileID"].ToString()}'";
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

        public IHttpActionResult Put([FromBody]JObject request)
        {
            //string sql;
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($"UPDATE LabImportData");
            sql.AppendLine($"SET LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'");
            sql.AppendLine($"  , LabRegNo = {request["LabRegNo"].ToString()}");
            if (request["CenterOrderCode2"] != null)
            {
                sql.AppendLine($"  , CenterOrderCode2 = '{request["CenterOrderCode2"].ToString()}'");
            }
            sql.AppendLine($"  , CenterOrderCode = '{request["CenterOrderCode"].ToString()}'");
            sql.AppendLine($"  , CenterSampleCode = '{(request["CenterSampleCode"] ?? "").ToString()}'");
            sql.AppendLine($"  , IsCenterRegist = 1");
            sql.AppendLine($"WHERE ImportDataID = '{request["ImportDataID"].ToString()}'");
            sql.AppendLine($"UPDATE LabRegInfo");
            sql.AppendLine($"SET IsTrustOrder = 1");
            sql.AppendLine($"  , CenterCode = 'Excel'");
            sql.AppendLine($"WHERE LabRegDate = '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'");
            sql.AppendLine($"AND LabRegNo = {request["LabRegNo"].ToString()}");
            LabgeDatabase.ExecuteSql(sql.ToString());
            return Ok();
        }

        public IHttpActionResult Delete(string importFileID)
        {
            try
            {
                string sql;
                sql = $"SELECT FileStoragePath, FileStorageName\r\n" +
                      $"FROM LabImportFile\r\n" +
                      $"WHERE ImportFileID = '{importFileID}'";
                JObject objFileInfo = LabgeDatabase.SqlToJObject(sql);

                string filePath = HttpContext.Current.Server.MapPath(objFileInfo["FileStoragePath"].ToString().Replace("\\", "/"));
                string file = filePath + objFileInfo["FileStorageName"].ToString();
                if (File.Exists(file))
                {
                    File.Delete(file);
                    sql = $"DELETE\r\n" +
                          $"FROM LabImportFile\r\n" +
                          $"WHERE ImportFileID = '{importFileID}'\r\n" +
                          $"DELETE\r\n" +
                          $"FROM LabImportData\r\n" +
                          $"WHERE ImportFileID = '{importFileID}'";
                    LabgeDatabase.ExecuteSql(sql);

                    return Ok();
                }
                else
                {
                    throw new HttpException(404, "파일이 없습니다.");
                }
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

        [Route("api/Sales/CompRegistOrder/ExcelOrder/LabImportFile")]
        public IHttpActionResult PutLabImportFile([FromBody]JObject request)
        {
            string sql;
            sql = $"UPDATE LabImportFile\r\n" +
                  $"SET IsRegistOrder = 1\r\n" +
                  $"  , RegistOrderTime = GETDATE()\r\n" +
                  $"  , RegistOrderMemberID = '{request["MemberID"].ToString()}'\r\n" +
                  $"WHERE ImportFileID = '{request["ImportFileID"].ToString()}'";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        /// <summary>
        /// 엑셀 파일 정보 리턴
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileID"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [Route("api/Sales/CompRegistOrder/ExcelOrder/ExcelFile")]
        public IHttpActionResult GetExcelFile(string importCode, string filePath, string storageName, string fileName)
        {
            try
            {
                string sql;
                sql = $"SELECT *\r\n" +
                      $"FROM LabImportCode\r\n" +
                      $"WHERE ImportCode = '{importCode}'";
                JObject objCode = LabgeDatabase.SqlToJObject(sql);

                //실제 파일 경로
                string root = HttpContext.Current.Server.MapPath(filePath);
                string file = root + storageName;

                if (File.Exists(file))
                {
                    var dataBytes = File.ReadAllBytes(file);
                    var dataStream = new MemoryStream(dataBytes);
                    IExcelDataReader reader = ExcelReaderFactory.CreateReader(dataStream, new ExcelReaderConfiguration()
                    {
                        FallbackEncoding = Encoding.GetEncoding(949)
                    });
                    var conf = new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = Convert.ToBoolean(objCode["IsImportFirstHeader"])
                        }
                    };

                    var dataSet = reader.AsDataSet(conf);
                    DataTable dt = dataSet.Tables[0].Clone();
                    foreach (DataColumn column in dt.Columns)
                    {
                        column.DataType = typeof(string);
                    }
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        dt.ImportRow(row);
                    }

                    JArray arrResponse = JArray.Parse(JsonConvert.SerializeObject(dt));
                    return Ok(arrResponse);
                }
                else
                {
                    throw new HttpException(404, "파일이 서버에 없습니다.");
                }
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

        /// <summary>
        /// 파일 업로드
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [Route("api/Sales/CompRegistOrder/ExcelOrder/Upload")]
        public async Task<HttpResponseMessage> PostFileUpload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string root = HttpContext.Current.Server.MapPath("/" + @"LabImportFile");
            Directory.CreateDirectory(root);
            var provider = new MultipartFormDataStreamProvider(root);

            await Request.Content.ReadAsMultipartAsync(provider);
            string importCode = string.Empty;
            string importCompCode = string.Empty;
            string memberID = string.Empty;
            //Key를 읽어온다.            
            foreach (var key in provider.FormData.AllKeys)
            {
                foreach (var val in provider.FormData.GetValues(key))
                {
                    if (key == "ImportCode")
                    {
                        importCode = val;
                    }
                    else if (key == "ImportCompCode")
                    {
                        importCompCode = val;
                    }
                    else if (key == "MemberID")
                    {
                        memberID = val;
                    }
                    //Trace.WriteLine($"{key} : {val}");
                }
            }

            //폴더 생성
            string saveDir = HttpContext.Current.Server.MapPath("/" + $"LabImportFile/{importCode}/{DateTime.Now.ToString("yyyy")}/{DateTime.Now.ToString("MMdd")}");
            Directory.CreateDirectory(saveDir);
            //파일을 이동
            foreach (MultipartFileData file in provider.FileData)
            {
                var info = new FileInfo(file.LocalFileName);
                DateTime createTime = info.CreationTime;
                long fileSize = info.Length;
                string replaceFileName = file.LocalFileName.Replace("BodyPart_", string.Empty);
                File.Move(file.LocalFileName, replaceFileName);
                File.Move(replaceFileName, saveDir + $"\\{Path.GetFileName(replaceFileName)}");

                string fileName = Path.GetFileNameWithoutExtension(file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty));
                string fileExt = Path.GetExtension(file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty)).Replace(".", string.Empty);
                string sql;
                sql = $"INSERT INTO LabImportFile\r\n" +
                      $"(\r\n" +
                      $"    ImportCode, ImportCompCode, FileDisplayName, FileStoragePath, FileStorageName, FileStorageSaveTime, FileStorageSaveMemberID,\r\n" +
                      $"    FileCreateTime, FileExt, FileSize\r\n" +
                      $")\r\n" +
                      $"VALUES\r\n" +
                      $"(\r\n" +
                      $"    '{importCode}', '{importCompCode}', '{fileName}'" +
                      $"  , '\\LabImportFile\\{importCode}\\{DateTime.Now.ToString("yyyy")}\\{DateTime.Now.ToString("MMdd")}\\', '{Path.GetFileName(replaceFileName)}'\r\n" +
                      $"  , GETDATE(), '{memberID}', '{createTime.ToString("yyyy-MM-dd HH:mm:ss")}', '{fileExt}', {fileSize}\r\n" +
                      $")";
                LabgeDatabase.ExecuteSql(sql);
                //Trace.WriteLine(file.LocalFileName);
                //Trace.WriteLine(file.Headers.ContentDisposition.FileName);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// 엑셀파일 다운로드
        /// </summary>
        /// <param name="importFileID"></param>
        /// <returns></returns>
        [Route("api/Sales/CompRegistOrder/ExcelOrder/Download")]
        public IHttpActionResult Get(string importFileID)
        {
            try
            {
                IHttpActionResult actionResult;
                HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

                string sql = $"SELECT *\r\n" +
                             $"FROM LabImportFile\r\n" +
                             $"WHERE ImportFileID = '{importFileID}'";
                JObject objFile = LabgeDatabase.SqlToJObject(sql);

                if (objFile.HasValues)
                {
                    string root = HttpContext.Current.Server.MapPath(objFile["FileStoragePath"].ToString());
                    string file = root + objFile["FileStorageName"].ToString();
                    if (File.Exists(file))
                    {
                        responseMessage.Content = new StreamContent(new FileStream(file, FileMode.Open, FileAccess.Read));
                        responseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                        responseMessage.Content.Headers.ContentDisposition.FileName = Path.GetFileName(file);
                        responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/excel");
                        actionResult = ResponseMessage(responseMessage);
                    }
                    else
                    {
                        throw new HttpException(404, "검색된 파일이 없습니다.");
                    }

                    return actionResult;
                }
                else
                {
                    throw new HttpException(404, "검색된 파일이 없습니다.");
                }
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

        /// <summary>
        /// ImportCode 목록 가져오기
        /// </summary>
        /// <param name="compCode"></param>
        /// <returns></returns>
        [Route("api/Sales/CompRegistOrder/ExcelOrder/ImportCode")]
        public IHttpActionResult GetImportCode(string compCode)
        {
            string sql;
            sql = $"SELECT ImportCode, ImportName\r\n" +
                  $"FROM LabImportCode\r\n" +
                  $"WHERE ImportCompCode = '{compCode}'\r\n" +
                  $"AND IsImportUse = 1";
            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }
    }
}