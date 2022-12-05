using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.StrategyBusiness
{
    [SupportsAuth]
    [Route("api/StrategyBusiness/ManageOsb")]
    public class ManageOsbController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@BeginDate", System.Data.SqlDbType.DateTime));
            sqlParameters[0].Value = beginDate.ToString("yyyy-MM-dd");
            sqlParameters.Add(new SqlParameter("@EndDate", System.Data.SqlDbType.DateTime));
            sqlParameters[1].Value = endDate.ToString("yyyy-MM-dd");

            JArray arrResponse = LabgeDatabase.SqlProcedureToJArray("USP_LST_ManageInfoOsb", sqlParameters);
            sqlParameters.Clear();


            //string sql;
            //sql =
            //    $"SELECT oo.CompCode, oo.CompOrderDate, pcc.CompName " +
            //    $", oo.CompOrderDate, oo.PatientName, oo.CompTestName, oo.CompTestCode, oo.BirthDay, oo.Height, oo.Weight, oo.Gender, oo.FetusNumber" +
            //    $", oo.GestationalAgeWeek, oo.GestationalAgeDay, oo.SampleDrawDate, CONVERT(varchar, oo.LabRegDate, 23) AS LabRegDate, oo.LabRegNo, oo.InvoiceFileName, oo.Comment\r\n" +
            //    $"FROM OsbOrders oo\r\n" +
            //    $"JOIN ProgCompCode pcc\r\n" +
            //    $"ON pcc.CompCode = oo.CompCode\r\n" +
            //    $"WHERE oo.CompOrderDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
            //    $"ORDER BY oo.RegsitDateTime";
            //
            //JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

            return Ok(arrResponse);
        }

        /// <summary>
        /// 정보 수정
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult Put([FromBody]JObject objRequest)
        {
            string sql;
            sql =
                $"UPDATE OsbOrders\r\n" +
                $"SET\r\n" +
                $"    PatientName = '{objRequest["PatientName"].ToString()}',\r\n" +
                $"    BirthDay = '{objRequest["BirthDay"].ToString()}',\r\n" +
                $"    Height = {objRequest["Height"].ToString()},\r\n" +
                $"    Weight = {objRequest["Weight"].ToString()},\r\n" +
                $"    Gender = '{objRequest["Gender"].ToString()}',\r\n" +
                $"    FetusNumber = {objRequest["FetusNumber"].ToString()},\r\n" +
                $"    GestationalAgeWeek = {objRequest["GestationalAgeWeek"].ToString()},\r\n" +
                $"    GestationalAgeDay = {objRequest["GestationalAgeDay"].ToString()},\r\n" +
                $"    SampleDrawDate = '{objRequest["SampleDrawDate"].ToString()}',\r\n" +
                $"    Comment = '{objRequest["Comment"].ToString()}'\r\n" +
                $"WHERE OsbOrderID = '{objRequest["OsbOrderID"].ToString()}'";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        public IHttpActionResult Delete(string osbOrderId)
        {
            string sql;
            sql =
                $"DELETE FROM OsbOrders\r\n" +
                $"WHERE OsbOrderId = '{osbOrderId}'";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        #region Invoice 업로드
        [Route("api/StrategyBusiness/ManageOsb/UploadInvoice")]
        public async Task<HttpResponseMessage> PostUploadInvoice()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                string root = HttpContext.Current.Server.MapPath("/" + @"Files/Invoice");
                Directory.CreateDirectory(root);
                var provider = new MultipartFormDataStreamProvider(root);

                await Request.Content.ReadAsMultipartAsync(provider);
                //provider.Contents.
                foreach (var data in provider.FormData)
                {
                    var type = data.GetType();
                }

                foreach (MultipartFileData file in provider.FileData)
                {
                    File.Move(file.LocalFileName, Path.GetDirectoryName(file.LocalFileName) + "\\" + file.Headers.ContentDisposition.FileNameStar);

                    var dataBytes = File.ReadAllBytes(file.LocalFileName);
                    var dataStream = new MemoryStream(dataBytes);
                    //IExcelDataReader reader = ExcelReaderFactory.CreateReader(dataStream);
                    //var conf = new ExcelDataSetConfiguration
                    //{
                    //    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    //    {
                    //        UseHeaderRow = true
                    //    }
                    //};
                    //var dataSet = reader.AsDataSet(conf);
                    //DataTable dt = dataSet.Tables[0];

                    //foreach (DataRow dr in dt.Rows)
                    //{
                    //    string sql;
                    //    sql = $"MERGE INTO Covid19Order AS target\r\n" +
                    //          $"USING (SELECT '{dr[8].ToString()}' AS SampleNo) AS source\r\n" +
                    //          $"ON target.SampleNo = source.SampleNo\r\n" +
                    //          $"WHEN NOT MATCHED THEN\r\n" +
                    //          $"    INSERT\r\n" +
                    //          $"    (\r\n" +
                    //          $"        FileName, CompOrderDate, CompOrderNo, SampleDrawDate, CompName,\r\n" +
                    //          $"        CompInstitutionNo, SampleNo, PatientName, BirthDay, Gender, RegistKind, TestTargetKind, TestKind, PrintDateTime,\r\n" +
                    //          $"        PhoneNo, Description, InterfaceKind\r\n" +
                    //          $"    )\r\n" +
                    //          $"    VALUES\r\n" +
                    //          $"    (\r\n" +
                    //          $"        '{Path.GetFileName(file.LocalFileName)}', '{dr[1].ToString()}', '{dr[2].ToString()}', '{dr[3].ToString()}',\r\n" +
                    //          $"        '{dr[4].ToString()}', '{dr[5].ToString()}', '{dr[8].ToString()}', '{dr[9].ToString()}',\r\n" +
                    //          $"        '{dr[10].ToString()}',\r\n" +
                    //          $"        CASE WHEN '{dr[11].ToString()}' = '남' THEN 'M' WHEN '{dr[11].ToString()}' = '여' THEN 'F' ELSE '' END,\r\n" +
                    //          $"        '{dr[12].ToString()}', '{dr[13].ToString()}', '{dr[14].ToString()}', '{dr[15].ToString()}',\r\n" +
                    //          $"        '{dr[16].ToString()}', '{dr[18].ToString().Replace("'", "''")}', 'Covid19Excel'\r\n" +
                    //          $"    )\r\n" +
                    //          $"WHEN MATCHED THEN\r\n" +
                    //          $"    UPDATE\r\n" +
                    //          $"    SET TestKind = '{dr[14].ToString()}'\r\n" +
                    //          $";";

                    //    LabgeDatabase.ExecuteSql(sql);
                    //}
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

        [Route("api/StrategyBusiness/ManageOsb/Invoice")]
        public IHttpActionResult DeleteInvoice()
        {
            return Ok();
        }
        #endregion Invoice 업로드

        #region CompList||CompListEmail
        /// <summary>
        /// 담당자 정보
        /// </summary>
        /// <returns></returns>
        #region ContactInfo
        [Route("api/StrategyBusiness/ManageOsb/ManagerInfo/CompList")]
        public IHttpActionResult GetManagerInfoCompList()
        {
            string sql;
            sql =
                $"SELECT 'Default' AS CompCode, 'Default' AS CompName\r\n" +
                $"UNION\r\n" +
                $"SELECT CompCode, CompName\r\n" +
                $"FROM ProgCompCode\r\n" +
                $"WHERE CompMngCode = 'GB'\r\n" +
                $"ORDER BY CompCode";
            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

            return Ok(arrResponse);
        }
        /// <summary>
        /// 거래처 별 담당자 정보
        /// </summary>
        /// <param name="compCode">거래처 코드</param>
        /// <returns></returns>
        [Route("api/StrategyBusiness/ManageOsb/ManagerInfo")]
        public IHttpActionResult GetManagerInfo(string compCode)
        {
            string sql;
            sql =
                $"SELECT *\r\n" +
                $"FROM OsbCompManagerInfo\r\n" +
                $"WHERE CompCode = '{compCode}'\r\n" +
                $"ORDER BY CompCode, ManageKind, ManageSeq";
            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

            return Ok(arrResponse);
        }
        /// <summary>
        /// 거래처 별 담당자 정보 추가
        /// </summary>
        /// <param name="objRequest"></param>
        /// <returns></returns>
        [Route("api/StrategyBusiness/ManageOsb/ManagerInfo")]
        public IHttpActionResult PostManagerInfo(JObject objRequest)
        {
            string sql;
            sql =
                $"INSERT INTO OsbCompManagerInfo\r\n" +
                $"(\r\n" +
                $"    CompCode, ManageKind, ManageSeq, ManagerName, Email, TelNumber\r\n" +
                $")\r\n" +
                $"VALUES\r\n" +
                $"(\r\n" +
                $"    '{objRequest["CompCode"].ToString()}', '{objRequest["ManageKind"].ToString()}', '{objRequest["ManageSeq"].ToString()}',\r\n" +
                $"    '{objRequest["ManagerName"].ToString()}', '{objRequest["Email"].ToString()}', '{objRequest["TelNumber"].ToString()}'\r\n" +
                $")";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }
        /// <summary>
        /// 담당자 정보 업데이트
        /// </summary>
        /// <param name="objRequest"></param>
        /// <returns></returns>
        [Route("api/StrategyBusiness/ManageOsb/ManagerInfo")]
        public IHttpActionResult PutConteactInfo(JObject objRequest)
        {
            string sql;
            sql =
                $"UPDATE OsbCompManagerInfo\r\n" +
                $"SET" +
                $"    ManagerName = '{objRequest["ManagerName"].ToString()}',\r\n" +
                $"    Email = '{objRequest["Email"].ToString()}',\r\n" +
                $"    TelNumber = '{objRequest["TelNumber"].ToString()}'\r\n" +
                $"WHERE CompCode = '{objRequest["CompCode"].ToString()}'\r\n" +
                $"AND ManageKind = '{objRequest["ManageKind"].ToString()}'\r\n" +
                $"AND ManageSeq = {objRequest["ManageSeq"].ToString()}";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        /// <summary>
        /// 담당자 정보 삭제
        /// </summary>
        /// <param name="compCode">거래처 코드</param>
        /// <param name="manageKind"></param>
        /// <param name="manageSeq"></param>
        /// <returns></returns>
        [Route("api/StrategyBusiness/ManageOsb/ManagerInfo")]
        public IHttpActionResult DeleteManagerInfo(string compCode, string manageKind, int manageSeq)
        {
            string sql;
            sql =
                $"DELETE OsbCompManagerInfo\r\n" +
                $"WHERE CompCode = '{compCode}'\r\n" +
                $"AND ManageKind = '{manageKind}'\r\n" +
                $"AND ManageSeq = {manageSeq}";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        [Route("api/StrategyBusiness/ManageOsb/ManagerInfo/SetDefault")]
        public IHttpActionResult PostSetDefault([FromBody]JObject request)
        {
            string sql;
            sql =
                $"DELETE FROM OsbCompManagerInfo\r\n" +
                $"WHERE CompCode = '{request["CompCode"].ToString()}'\r\n" +
                $"INSERT INTO OsbCompManagerInfo\r\n" +
                $"(\r\n" +
                $"    CompCode, ManageKind, ManageSeq, ManagerName, Email, TelNumber\r\n" +
                $")\r\n" +
                $"SELECT '{request["CompCode"].ToString()}', ManageKind, ManageSeq, ManagerName, Email, TelNumber\r\n" +
                $"FROM OsbCompManagerInfo\r\n" +
                $"WHERE CompCode = 'Default'";

            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }
        #endregion ContactInfo


        #region NotiMailList

        [Route("api/StrategyBusiness/ManageOsb/ManagerInfo/NotiMailList")]
        public IHttpActionResult GetNotiMailList()
        {

            string sql;
            sql = "SELECT \r\n" +
            "OsbNotiMailList.CompCode, OsbNotiMailList.RegistMemberID, OsbNotiMailList.RegistTime \r\n" +
            "FROM OsbNotiMailList";
            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/StrategyBusiness/ManageOsb/ManagerInfo/NotiMailList")]
        public IHttpActionResult PostNotiMailList(JObject objRequest)
        {
            string sql;
            sql =
                $"SELECT COUNT(*) \r\n" +
                $"FROM ProgCompCode pcc \r\n" +
                $"where pcc.CompCode = '{objRequest["CompCode"].ToString()}'";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

            if (arrResponse.Count < 0)
            {
                return BadRequest("등록되지 않은 코드");
            }

            sql =
                $"INSERT into OsbNotiMailList\r\n" +
                $"(\r\n" +
                $"    CompCode, RegistMemberID \r\n" +
                $")\r\n" +
                $"VALUES\r\n" +
                $"(\r\n" +
                $"    '{objRequest["CompCode"].ToString()}', '{ objRequest["RegistMemberID"].ToString()}'\r\n" +
                $")";

            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }
        [Route("api/StrategyBusiness/ManageOsb/ManagerInfo/NotiMailList")]
        public IHttpActionResult DeleteNotiMailList(string compCode)
        {
            string sql;
            sql =
                $"DELETE OsbNotiMailList\r\n" +
                $"WHERE CompCode = '{compCode}'\r\n";

            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }



        #endregion


        #endregion
    }
}