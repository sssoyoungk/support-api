using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Controllers.Sales.CompRegistOrder;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [SupportsAuth]
    [EnableCors(origins: "*", headers: "*", methods: "*", PreflightMaxAge = 28800)]
    [Route("api/Sales/CompRegistOrder")]
    public class CompRegistOrderController : ApiController
    {
        public IHttpActionResult GetCompCode(string chartKind, string authGroup)
        {
            string sql;

            sql = "SELECT pcc.CompCode, pcc.CompName, pcc.CompInstitutionNo, rtcs.TransKind\r\n" +
                  "FROM ProgCompCode pcc\r\n" +
                  "JOIN RsltTransCompSet rtcs\r\n" +
                  "ON pcc.CompCode = rtcs.CompCode\r\n" +
                  "AND IsUse = 1\r\n";
            if ((chartKind ?? string.Empty) != string.Empty)
                sql += $"AND rtcs.TransKind = '{chartKind}'\r\n";

            sql += 
                $"WHERE rtcs.CompCode IN\r\n" +
                $"(\r\n" +
                $"    SELECT CompCode\r\n" +
                $"    FROM ProgAuthGroupAccessComp\r\n" +
                $"    WHERE AuthGroupCode = '{authGroup}'\r\n" +
                $")\r\n" +
                $"AND pcc.IsCompUseCode = 1\r\n" +
                $"UNION\r\n" +
                $"SELECT pcc.CompCode, pcc.CompName, pcc.CompInstitutionNo, 'Genocore'\r\n" +
                $"FROM GenocoreCompCode gcc\r\n" +
                $"JOIN ProgCompCode pcc\r\n" +
                $"ON gcc.CompCode = pcc.CompCode\r\n" +
                $"WHERE pcc.CompCode IN\r\n" +
                $"(\r\n" +
                $"    SELECT CompCode\r\n" +
                $"    FROM ProgAuthGroupAccessComp\r\n" +
                $"    WHERE AuthGroupCode = '{authGroup}'\r\n" +
                $")\r\n" +
                $"ORDER BY CompCode";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/Sales/CompRegistOrder/Order")]
        public IHttpActionResult PostOrder(JObject objRequest)
        {
            try
            {
                JArray arrayResponse = new JArray();
                //비즈먼트
                if (objRequest["TransKind"].ToString() == "Bizment")
                {
                    Bizment bizment = new Bizment();
                    arrayResponse = bizment.GetOrder(objRequest);
                }
                //브레인
                else if (objRequest["TransKind"].ToString() == "Brain")
                {
                    Brain brain = new Brain();
                    arrayResponse = brain.GetOrder(objRequest);
                }
                //이지스
                else if (objRequest["TransKind"].ToString() == "Eghis")
                {
                    Eghis eghis = new Eghis();
                    arrayResponse = eghis.GetOrder(objRequest);
                }
                //유비케어 Cloud EMR
                else if (objRequest["TransKind"].ToString() == "CloudEMR")
                {
                    CloudEMR cloudEMR = new CloudEMR();
                    arrayResponse = cloudEMR.GetOrder(objRequest);
                }
                //유비케어 의사랑2000
                else if (objRequest["TransKind"].ToString() == "Ysr2000")
                {
                    Ysr2000 ysr2000 = new Ysr2000();
                    arrayResponse = ysr2000.GetOrder(objRequest);
                }
                //엑셀
                else if (objRequest["TransKind"].ToString() == "Excel")
                {
                    ExcelOrder excelOrder = new ExcelOrder();
                    arrayResponse = excelOrder.GetOrder(objRequest);
                }
                //PGS(보험사제노팩)
                else if (objRequest["TransKind"].ToString() == "PGS")
                {
                    PGS pgs = new PGS();
                    arrayResponse = pgs.GetOrder(objRequest);
                }
                //쥬비스다이어트
                else if (objRequest["TransKind"].ToString() == "Juvis")
                {
                    Juvis juvis = new Juvis();
                    arrayResponse = juvis.GetOrder(objRequest);
                }
                //해외사업팀
                else if (objRequest["TransKind"].ToString() == "OSB")
                {
                    OSB osb = new OSB();
                    arrayResponse = osb.GetOrder(objRequest);
                }
                //뱅크샐러드
                else if (objRequest["TransKind"].ToString() == "Banksalad")
                {
                    BankSalad bankSalad = new BankSalad();
                    arrayResponse = bankSalad.GetOrder(objRequest);
                }
                //아모레 퍼시픽
                else if (objRequest["TransKind"].ToString() == "AmorePacific")
                {
                    AmorePacific amorePacific = new AmorePacific();
                    arrayResponse = amorePacific.GetOrder(objRequest);
                }
                //제노코어
                else if (objRequest["TransKind"].ToString() == "Genocore")
                {
                    GenoCore genoCore = new GenoCore();
                    arrayResponse = genoCore.GetOrder(objRequest);
                }
                //통합 API
                else
                {
                    IntegratedAPI integratedAPI = new IntegratedAPI();
                    arrayResponse = integratedAPI.GetOrder(objRequest);
                }

                return Ok(arrayResponse);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Code", HttpStatusCode.BadRequest.ToString());
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }

        /// <summary>
        /// GET 이미 등록된 정보가 있는지 확인
        /// </summary>
        /// <param name="labRegDate"></param>
        /// <param name="labRegNo"></param>
        /// <returns></returns>
        public IHttpActionResult Get(DateTime labRegDate, int labRegNo)
        {
            string sql;
            sql = $"SELECT TOP 1 LabRegDate, LabRegNo, PatientName, PatientChartNo\r\n" +
                  $"FROM LabRegInfo\r\n" +
                  $"WHERE LabRegDate = '{labRegDate.ToString("yyyy-MM-dd")}'\r\n" +
                  $"AND LabRegNo = {labRegNo}";

            JObject objRequest = LabgeDatabase.SqlToJObject(sql);

            return Ok(objRequest);
        }

        /// <summary>
        /// 기등록 매치
        /// </summary>
        /// <param name="labRegDate"></param>
        /// <param name="labRegNo"></param>
        /// <param name="patientName"></param>
        /// <param name="chartNo"></param>
        /// <param name="orderCode"></param>
        /// <param name="testCode"></param>
        /// <returns></returns>
        public IHttpActionResult Get(string compCode, DateTime labRegDate, string patientName, string chartNo, string orderCode, string testCode)
        {
            string sql;
            string code = (orderCode ?? string.Empty) != string.Empty ? orderCode : testCode;

            sql = $"SELECT LabRegNo\r\n" +
                  $"FROM View_LabgeResult\r\n" +
                  $"WHERE CompCode = '{compCode}'\r\n" +
                  $"AND LabRegDate = '{labRegDate.ToString("yyyy-MM-dd")}'\r\n" +
                  $"AND PatientName = '{patientName}'\r\n" +
                  $"AND PatientChartNo = '{chartNo}'\r\n" +
                  $"AND TestCode = '{code}'\r\n";
            object labRegNo = LabgeDatabase.ExecuteSqlScalar(sql);

            if (labRegNo == null)
            {
                sql = $"SELECT lri.LabRegNo\r\n" +
                      $"FROM LabRegInfo lri\r\n" +
                      $"JOIN LabRegOrder lro\r\n" +
                      $"ON lri.LabRegDate = lro.LabRegDate\r\n" +
                      $"AND lri.LabRegNo = lro.LabRegNo\r\n" +
                      $"WHERE lri.CompCode = '{compCode}'\r\n" +
                      $"AND lri.LabRegDate = '{labRegDate.ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND lri.PatientName = '{patientName}'\r\n" +
                      $"AND lri.PatientChartNo = '{chartNo}'\r\n" +
                      $"AND lro.OrderCode = '{code}'\r\n";
                labRegNo = LabgeDatabase.ExecuteSqlScalar(sql);
            }

            if (labRegNo == null)
            {
                return Content(HttpStatusCode.BadRequest, 0);
            }
            else
            {
                return Ok(Convert.ToInt32(labRegNo));
            }
        }

        /// <summary>
        /// 오더 등록
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHttpActionResult Post([FromBody]JObject request)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();
            //SqlTransaction tran = conn.BeginTransaction();

            RegistOrder registOrder = new RegistOrder();
            try
            {
                //거래처 서브코드가 존재하면 서브코드 정보를 등록해준다.
                if (request["CompSubCode"] != null && request["CompSubCode"].ToString() != string.Empty)
                {
                    registOrder.SetCompSubCode(request["CompCode"].ToString(), request["CompSubCode"].ToString(), request["CompSubName"].ToString(), request["RegistMemberID"].ToString());
                }

                //LabRegInfo 등록
                registOrder.RegistLabRegInfo(request);
                #region 트랜젝션 처리
                //SqlCommand cmdLabRegInfo = registOrder.RegistLabRegInfo(request);
                //cmdLabRegInfo.Connection = conn;
                //cmdLabRegInfo.Transaction = tran;
                //cmdLabRegInfo.ExecuteNonQuery();
                #endregion 트랜젝션 처리

                //LabRegOrder 등록
                registOrder.RegistLabRegOrder(request);
                #region 트랜젝션 처리
                //SqlCommand cmdLabRegOrder = registOrder.RegistLabRegOrder(request);
                //cmdLabRegOrder.Connection = conn;
                //cmdLabRegOrder.Transaction = tran;
                //cmdLabRegOrder.ExecuteNonQuery();
                #endregion 트랜젝션 처리

                //LabRegOrderTest 등록
                registOrder.RegistLabRegOrderTest(request);
                #region 트랜젝션 처리
                //SqlCommand cmdLabRegOrderTest = registOrder.RegistLabRegOrderTest(request);
                //cmdLabRegOrderTest.Connection = conn;
                //cmdLabRegOrderTest.Transaction = tran;
                //cmdLabRegOrderTest.ExecuteNonQuery();
                #endregion 트랜젝션 처리

                //LabRegSample 등록
                registOrder.RegistLabRegSample(request);
                #region 트랜젝션 처리
                //SqlCommand cmdLabRegSample = registOrder.RegistLabRegSample(request);
                //cmdLabRegSample.Connection = conn;
                //cmdLabRegSample.Transaction = tran;
                //cmdLabRegSample.ExecuteNonQuery();
                #endregion 트랜젝션 처리

                //접수가 완료되면 커밋
                //tran.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                //tran.Rollback();

                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 거래처코드 변경
        /// </summary>
        /// <param name="requestData"></param>
        public IHttpActionResult Put([FromBody]JObject request)
        {
            string sql;

            sql = $"SELECT COUNT(*)\r\n" +
                  $"FROM ProgCompCode\r\n" +
                  $"WHERE CompCode = '{request["ChangeCompCode"].ToString()}'";

            int cnt = Convert.ToInt32(LabgeDatabase.ExecuteSqlScalar(sql));
            if (cnt == 0)
            {
                return Content(HttpStatusCode.BadRequest, "변경할 거래처코드가 등록되지 않았습니다.");
            }

            if (request["TransKind"].ToString() == "MCC")
            {
                sql = $"UPDATE RsltTransIntegratedAPIOrder\r\n" +
                      $"SET CompCode = '{request["ChangeCompCode"].ToString()}'\r\n" +
                      $"WHERE CompCode = '{request["CompCode"].ToString()}'\r\n" +
                      $"AND CompOrderDate = '{request["CompOrderDate"].ToString()}'\r\n" +
                      $"AND CompOrderNo = '{request["CompOrderNo"].ToString()}'\r\n" +
                      $"AND CompTestCode = '{request["CompTestCode"].ToString()}'";                
            }
            else if (request["TransKind"].ToString() == "Bizment")
            {
                sql = $"UPDATE RsltTransBizmentOrder\r\n" +
                      $"SET CompCode = '{request["ChangeCompCode"].ToString()}'\r\n" +
                      $"WHERE CompCode = '{request["CompCode"].ToString()}'\r\n" +
                      $"AND WNO = '{request["CompOrderDate"].ToString()}'\r\n" +
                      $"AND ONO = '{request["CompOrderNo"].ToString()}'\r\n" +
                      $"AND BZCODE = '{request["CompTestCode"].ToString()}'";
            }
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        public IHttpActionResult Put(DateTime labRegDate, int labRegNo, string testCode, string sampleNo)
        {
            try
            {
                string sql;

                sql = $"UPDATE LabTransCompOrderInfo\r\n" +
                      $"SET CompSpcNo = '{sampleNo}'\r\n" +
                      $"WHERE LabRegDate = '{labRegDate.ToString("yyyy-MM-dd")}'\r\n" +
                      $"AND LabRegNo = {labRegNo}\r\n" +
                      $"AND TestCode = '{testCode}'";

                LabgeDatabase.ExecuteSql(sql);

                return Ok();
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Code", HttpStatusCode.BadRequest.ToString());
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }

        /// <summary>
        /// 연동접수 정보 등록 테이블 삭제
        /// </summary>
        /// <param name="labRegDate"></param>
        /// <param name="labRegNo"></param>
        /// <returns></returns>
        public IHttpActionResult Delete(DateTime labRegDate, int labRegNo, string orderCode, string testCode)
        {
            string sql;
            sql = $"DELETE FROM LabTransCompOrderInfo\r\n" +
                  $"WHERE LabRegDate = '{labRegDate.ToString("yyyy-MM-dd")}'\r\n" +
                  $"AND LabRegNo = {labRegNo}\r\n" +
                  $"AND ISNULL(OrderCode, '') = '{orderCode}'\r\n" +
                  $"AND TestCode = '{testCode}'";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        [Route("api/Sales/CompRegistOrder/RegistOsbOrders")]
        public IHttpActionResult PutRegistOsbOrders([FromBody]JObject objRequest)
        {
            string sql;
            sql =
                $"UPDATE OsbOrders\r\n" +
                $"SET\r\n" +
                $"    LabRegDate = '{Convert.ToDateTime(objRequest["LabRegDate"]).ToString("yyyy-MM-dd")}',\r\n" +
                $"    LabRegNo = {objRequest["LabRegNo"].ToString()}\r\n" +
                $"WHERE CompOrderDate = '{Convert.ToDateTime(objRequest["CompOrderDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                $"AND OsbOrderID = CONVERT(uniqueidentifier, '{objRequest["CompOrderNo"].ToString()}')";

            LabgeDatabase.ExecuteSql(sql);

            //맘가드는 추가정보 등록한다.
            if (new[] { "22002", "22003", "22004" }.Contains(objRequest["TestCode"].ToString()))
            {
                sql =
                    $"SELECT Height, Weight, FetusNumber, GestationalAgeWeek, GestationalAgeDay\r\n" +
                    $"FROM OsbOrders\r\n" +
                    $"WHERE CompOrderDate = '{Convert.ToDateTime(objRequest["CompOrderDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                    $"AND OsbOrderID = CONVERT(uniqueidentifier, '{objRequest["CompOrderNo"].ToString()}')";
                JObject objCustomCode = LabgeDatabase.SqlToJObject(sql);
                if (objCustomCode.Count > 0)
                {
                    sql = string.Empty;
                    string insertSql =
                        "INSERT INTO LabRegCustom\r\n" +
                        "(LabRegDate, LabRegNo, CustomCode, CustomValue01, CustomValue02, CustomBgColor, CustomFontColor)\r\n" +
                        "VALUES\r\n";
                    //신장
                    sql += insertSql +
                        $"('{Convert.ToDateTime(objRequest["LabRegDate"]).ToString("yyyy-MM-dd")}', {objRequest["LabRegNo"].ToString()}, '0120', '{objCustomCode["Height"].ToString()}', '',\r\n" +
                        $" (SELECT CustomBgColor FROM LabCustomCode WHERE CustomCode = '0120'), (SELECT CustomFontColor FROM LabCustomCode WHERE CustomCode = '0120')\r\n" +
                        $")\r\n";

                    //몸무게
                    sql += insertSql +
                        $"('{Convert.ToDateTime(objRequest["LabRegDate"]).ToString("yyyy-MM-dd")}', {objRequest["LabRegNo"].ToString()}, '0060', '{objCustomCode["Weight"].ToString()}', '',\r\n" +
                        $" (SELECT CustomBgColor FROM LabCustomCode WHERE CustomCode = '0120'), (SELECT CustomFontColor FROM LabCustomCode WHERE CustomCode = '0120')\r\n" +
                        $")\r\n";

                    //임신주수
                    sql += insertSql +
                        $"('{Convert.ToDateTime(objRequest["LabRegDate"]).ToString("yyyy-MM-dd")}', {objRequest["LabRegNo"].ToString()}, '0150',\r\n" +
                        $" '{objCustomCode["GestationalAgeWeek"].ToString()}', '{objCustomCode["GestationalAgeDay"].ToString()}',\r\n" +
                        $" (SELECT CustomBgColor FROM LabCustomCode WHERE CustomCode = '0120'), (SELECT CustomFontColor FROM LabCustomCode WHERE CustomCode = '0120')\r\n" +
                        $")\r\n";

                    //태아수
                    sql += insertSql +
                        $"('{Convert.ToDateTime(objRequest["LabRegDate"]).ToString("yyyy-MM-dd")}', {objRequest["LabRegNo"].ToString()}, '0320',\r\n" +
                        $" '{objCustomCode["FetusNumber"].ToString()}', '',\r\n" +
                        $" (SELECT CustomBgColor FROM LabCustomCode WHERE CustomCode = '0120'), (SELECT CustomFontColor FROM LabCustomCode WHERE CustomCode = '0120')\r\n" +
                        $")\r\n";

                    LabgeDatabase.ExecuteSql(sql);
                }
            }

            return Ok();
        }

        [Route("api/Sales/CompRegistOrder/CheckNotUseTestCode")]
        public IHttpActionResult PostCheckNotUseTestCode(JArray arrRequest)
        {
            string sql;
            sql =
                "CREATE TABLE #TestCodes\r\n" +
                "(\r\n" +
                "    TestCode varchar(50)\r\n" +
                ")\r\n";
            foreach (JObject objRequest in arrRequest)
            {
                sql += $"INSERT INTO #TestCodes ( TestCode ) VALUES ( '{objRequest["TestCode"]}' )\r\n";
            }
            sql +=
                "SELECT TestCode, TestDisplayName\r\n" +
                "FROM LabTestCode\r\n" +
                "WHERE IsTestUse = 0\r\n" +
                "AND TestCode IN (SELECT TestCode FROM #TestCodes)";
            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

            return Ok(arrResponse);
        }

        [Route("api/Sales/CompRegistOrder/NotUseTestCode")]
        public IHttpActionResult PutNotUseTestCode(JObject objRequest)
        {
            string sql;
            sql =
                $"UPDATE LabTransMatchCode\r\n" +
                $"SET CenterMatchCode = '{objRequest["NewTestCode"]}'\r\n" +
                $"WHERE CompCode = '{objRequest["CompCode"]}'\r\n" +
                $"AND CenterMatchCode = '{objRequest["OldTestCode"]}'";
            LabgeDatabase.ExecuteSql(sql);

            return Ok();
        }

        [Route("api/Sales/CompRegistOrder/ReadBarcode")]
        public IHttpActionResult GetReadBarcode(string transKind, string compCode, string barcode)
        {
            try
            {
                string customerCode = string.Empty;
                if (compCode == "BKS01")
                {
                    customerCode = "banksalad";
                }
                else if (compCode == "FET01")
                {
                    customerCode = "fiet";
                }
                else
                {
                    customerCode = "";
                }
                //else if (transKind.Contains("Ju"))
                //{
                //    customerCode = "Juvis";
                //}
                //else if (transKind.Contains("FE"))
                //{
                //    customerCode = "fiet";
                //}
                //else
                //{
                //
                //}


                string sql = string.Empty;
                if (customerCode == "banksalad" || customerCode == "fiet")
                {
                    sql =
                        $"SELECT\r\n" +
                        $"    CONVERT(bit,1) AS ColumnCheck, GETDATE() AS LabRegDate, null AS LabRegNo, ppi.PatientName, ppi.Barcode AS PatientChartNo, \r\n" + //PSG 사업부 요청으로 바코드 번호가 차트 번호로 설정
                        $"    ppi.CompOrderDate, ppi.CompOrderNo, ppi.CompOrderNo , CONVERT(varchar, ppi.SampleDrawDate, 120) SampleDrawDate,\r\n" +
                        $"    Gender AS PatientSex, ppi.OrderStatus,\r\n" +
                        $"    CONVERT(CHAR(6), BirthDay, 12) AS IdentificationNo1, \r\n" +
                        $"    FLOOR(CAST(DATEDIFF(DAY, ppi.BirthDay, ppi.CompOrderDate) AS INTEGER) / 365.2422) AS PatientAge, \r\n " +
                        $"    ppi.Age AS PatientAge, ppi.Barcode,\r\n" +
                        $"    ltmc.CenterMatchCode AS TestCode, ltmc.CenterMatchOrderCode AS OrderCode,\r\n" +
                        $"    pti.CompTestCode, null As CompTestSubCode, pti.CompTestName,\r\n" +
                        $"    CASE" +
                        $"      WHEN ISNULL(ltmc.CenterMatchSampleCode, '') <> '' THEN ltmc.CenterMatchSampleCode\r\n" +
                        $"      ELSE ltc.SampleCode\r\n" +
                        $"    END AS SampleCode," +
                        $"    ltcoi.RegistTime\r\n" +
                        $"FROM PGSPatientInfo ppi\r\n" +
                        $"JOIN PGSTestInfo pti\r\n" +
                        $"ON ppi.CustomerCode = pti.CustomerCode\r\n" +
                        $"AND ppi.CustomerCode = pti.CustomerCode\r\n" +
                        $"AND ppi.CompOrderDate = pti.CompOrderDate\r\n" +
                        $"AND ppi.CompOrderNo = pti.CompOrderNo\r\n" +
                        $"LEFT OUTER JOIN LabTransMatchCode AS ltmc\r\n" +
                        $"ON ltmc.CompCode = pti.CompCode\r\n" +
                        $"AND ltmc.CompMatchCode = pti.CompTestCode\r\n" +
                        $"LEFT OUTER JOIN LabTestCode ltc\r\n" +
                        $"ON ltmc.CenterMatchCode = ltc.TestCode\r\n" +
                        $"LEFT OUTER JOIN LabTransCompOrderInfo ltcoi\r\n" +
                        $"ON ppi.CompOrderDate = ltcoi.CompOrderDate\r\n" +
                        $"AND ppi.CompOrderNo = ltcoi.CompOrderNo\r\n" +
                        $"AND pti.CompTestCode = ltcoi.CompTestCode\r\n" +
                        $"WHERE ppi.CustomerCode = '{customerCode}'\r\n" +
                        $"AND ppi.Barcode = '{barcode}'" +
                        $"AND ltcoi.LabRegNo is null";
                }


                JArray arrResponse = LabgeDatabase.SqlToJArray(sql);


                if (arrResponse[0]["OrderStatus"].ToString() == "Canceled" || arrResponse[0]["OrderStatus"].ToString() == "CanceledBeforeShipment")
                {
                    throw new Exception("해당 수진자는 취소 상태입니다.");
                }

                if (arrResponse.Count <= 0)
                {
                    throw new Exception("등록된 바코드가 없거나 등록되어 있습니다.");
                }
                
                if (arrResponse.Count > 1 )
                {
                    throw new Exception("동일한 바코드가 있습니다.");
                }

                if (arrResponse[0]["LabRegNo"].ToString() != null && arrResponse[0]["LabRegNo"].ToString() != "")
                {
                    throw new Exception("이미 등록된 접수 번호가 있습니다.");
                }

                
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
    }
}