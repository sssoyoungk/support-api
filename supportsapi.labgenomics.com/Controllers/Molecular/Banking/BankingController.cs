using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Molecular
{
    [Route("api/Molecular/Banking")]
    public class BankingController : ApiController
    {
        /// <summary>
        /// Banking 종류를 불러온다.
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        [Route("api/Molecular/Banking/Kind")]
        public IHttpActionResult GetBankingKind(string memberId)
        {
            try
            {
                JArray arrResponse = new JArray();
                var arrAuth = FindMemberBankingKindAuth(memberId);

                if (arrAuth.Count() == 0)
                {
                    string sql;
                    sql = "SELECT DISTINCT BankingKind \r\n" +
                          "FROM BankingSampleInfo \r\n ";
                    arrResponse = LabgeDatabase.SqlToJArray(sql);
                }
                else
                {
                    foreach (JObject objAuth in arrAuth)
                    {
                        arrResponse.Add(objAuth);
                    }
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

        /// <summary>
        /// Banking 검체 정보 가져옴
        /// </summary>
        /// <param name="bankingKind"></param>
        /// <returns></returns>
        public IHttpActionResult Get(string bankingKind)
        {
            try
            {
                string sql;
                sql = $"SELECT ImportYearKind, ImportDate, SampleCode, ResearchID, CompCode, AdultChildCheck, ChartNo, DestroyCheck, ImportEDTA, ImportSST, ImportUrine\r\n" +
                      $"     , CASE WHEN DestroyCheck = 0 THEN SerumStock ELSE 0 END SerumStock\r\n" +
                      $"     , CASE WHEN DestroyCheck = 0 THEN PlasmaStock ELSE 0 END PlasmaStock\r\n" +
                      $"     , CASE WHEN DestroyCheck = 0 THEN UrineStock ELSE 0 END UrineStock\r\n" +
                      $"     , CASE WHEN DestroyCheck = 0 THEN DNAStock ELSE 0 END DNAStock\r\n" +
                      $"     , ChildSerumVolume, CentrifugeCheck, BloodDrawDate\r\n" +
                      $"     , SampleStockDate, SampleTestDate, SampleDescription, SamplePosition, DNAResult, DNAPurity, DNA260, LeftPlasma, SampleDiscardCheck\r\n" +
                      $"     , CompName, LabgeDescription, BankingInfoID\r\n" +
                      $"FROM\r\n" +
                      $"(\r\n" +
                      $"    SELECT bsi.ImportYearKind\r\n" +
                      $"         , bsi.ImportDate\r\n" +
                      $"         , bsi.SampleCode, bsi.ResearchID, bsi.CompCode, bsi.AdultChildCheck\r\n" +
                      $"         , bsi.ChartNo, bsi.DestroyCheck, bsi.ImportEDTA, bsi.ImportSST, bsi.ImportUrine\r\n" +
                      $"         , (SELECT COUNT(*)\r\n" +
                      $"           FROM\r\n" +
                      $"           (\r\n" +
                      $"               SELECT bss.SampleVolume - ISNULL((SELECT SUM(ExportVolume)\r\n" +
                      $"                                                 FROM BankingSampleExport bse\r\n" +
                      $"                                                 WHERE bss.BankingKind = bse.BankingKind\r\n" +
                      $"                                                 AND bss.SampleCode = bse.SampleCode\r\n" +
                      $"                                                 AND bss.Barcode = bse.Barcode), 0) AS SampleVolume\r\n" +
                      $"               FROM BankingSampleStock bss\r\n" +
                      $"               WHERE bsi.BankingKind = bss.BankingKind\r\n" +
                      $"               AND bsi.SampleCode = bss.SampleCode\r\n" +
                      $"               AND SUBSTRING(bss.BarCode, 8, 1) = 'S'\r\n" +
                      $"           ) AS Sub1\r\n" +
                      $"           WHERE SampleVolume > 0) AS SerumStock\r\n" +
                      $"         , (SELECT COUNT(*)\r\n" +
                      $"           FROM\r\n" +
                      $"           (\r\n" +
                      $"               SELECT bss.SampleVolume - ISNULL((SELECT SUM(ExportVolume)\r\n" +
                      $"                                                 FROM BankingSampleExport bse\r\n" +
                      $"                                                 WHERE bss.BankingKind = bse.BankingKind\r\n" +
                      $"                                                 AND bss.SampleCode = bse.SampleCode\r\n" +
                      $"                                                 AND bss.Barcode = bse.Barcode), 0) AS SampleVolume\r\n" +
                      $"               FROM BankingSampleStock bss\r\n" +
                      $"               WHERE bsi.BankingKind = bss.BankingKind\r\n" +
                      $"               AND bsi.SampleCode = bss.SampleCode\r\n" +
                      $"               AND SUBSTRING(bss.BarCode, 8, 1) = 'P'\r\n" +
                      $"           ) AS Sub1\r\n" +
                      $"           WHERE SampleVolume > 0) AS PlasmaStock\r\n" +
                      $"         , (SELECT COUNT(*)\r\n" +
                      $"           FROM\r\n" +
                      $"           (\r\n" +
                      $"               SELECT bss.SampleVolume - ISNULL((SELECT SUM(ExportVolume)\r\n" +
                      $"                                                 FROM BankingSampleExport bse\r\n" +
                      $"                                                 WHERE bss.BankingKind = bse.BankingKind\r\n" +
                      $"                                                 AND bss.SampleCode = bse.SampleCode\r\n" +
                      $"                                                 AND bss.Barcode = bse.Barcode), 0) AS SampleVolume\r\n" +
                      $"               FROM BankingSampleStock bss\r\n" +
                      $"               WHERE bsi.BankingKind = bss.BankingKind\r\n" +
                      $"               AND bsi.SampleCode = bss.SampleCode\r\n" +
                      $"               AND SUBSTRING(bss.BarCode, 8, 1) = 'U'\r\n" +
                      $"           ) AS Sub1\r\n" +
                      $"           WHERE SampleVolume > 0) AS UrineStock\r\n" +
                      $"         , (SELECT COUNT(*)\r\n" +
                      $"           FROM\r\n" +
                      $"           (\r\n" +
                      $"               SELECT bss.SampleVolume - ISNULL((SELECT SUM(ExportVolume)\r\n" +
                      $"                                                 FROM BankingSampleExport bse\r\n" +
                      $"                                                 WHERE bss.BankingKind = bse.BankingKind\r\n" +
                      $"                                                 AND bss.SampleCode = bse.SampleCode\r\n" +
                      $"                                                 AND bss.Barcode = bse.Barcode), 0) AS SampleVolume\r\n" +
                      $"               FROM BankingSampleStock bss\r\n" +
                      $"               WHERE bsi.BankingKind = bss.BankingKind\r\n" +
                      $"               AND bsi.SampleCode = bss.SampleCode\r\n" +
                      $"               AND SUBSTRING(bss.BarCode, 8, 1) = 'D'\r\n" +
                      $"           ) AS Sub1\r\n" +
                      $"           WHERE SampleVolume > 0) AS DNAStock\r\n" +
                      $"         , CASE WHEN bsi.ChildSerumVolume = 0 THEN NULL ELSE bsi.ChildSerumVolume END AS ChildSerumVolume\r\n" +
                      $"         , bsi.CentrifugeCheck, bsi.BloodDrawDate\r\n" +
                      $"         , bsi.SampleStockDate\r\n" +
                      $"         , bsi.SampleTestDate\r\n" +
                      $"         , bsi.SampleDescription, bsi.SamplePosition\r\n " +
                      $"         , bsi.DNAResult, bsi.DNAPurity, bsi.DNA260, bsi.LeftPlasma , bsi.SampleDiscardCheck\r\n" +
                      $"         , pcc.CompName, bsi.LabgeDescription, bsi.BankingInfoID, bsi.InsertDateTime\r\n" +
                      $"    FROM BankingSampleInfo bsi\r\n" +
                      $"    JOIN ProgCompCode pcc\r\n " +
                      $"    ON bsi.CompCode = pcc.CompCode\r\n" +
                      $"    WHERE BankingKind = '{bankingKind}'\r\n" +
                      $") AS Sub1\r\n" +
                      $"ORDER BY ImportDate ASC, InsertDateTime";

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
        /// 검체 정보 등록
        /// </summary>
        /// <param name="value"></param>
        public IHttpActionResult Post([FromBody]JObject request)
        {
            try
            {

                string sql = string.Empty;

                sql = $"INSERT INTO BankingSampleInfo\r\n" +
                      $"    ( BankingKind, ImportYearKind, ImportDate, SampleCode, CompCode, ResearchID, ChartNo, DestroyCheck, ImportEDTA, ImportSST, ImportUrine\r\n" +
                      $"    , SerumStock, PlasmaStock, UrineStock, DNAStock, CentrifugeCheck, BloodDrawDate, SampleStockDate, SampleTestDate\r\n" +
                      $"    , SamplePosition, SampleDescription, DNAResult, DNAPurity, DNA260, LeftPlasma, LabgeDescription, InsertID, InsertDateTime)\r\n" +
                      $"VALUES\r\n" +
                      $"    ( @BankingKind, @ImportYearKind, @ImportDate, @SampleCode, @CompCode, @ResearchID, @ChartNo, @DestroyCheck, @ImportEDTA, @ImportSST, @ImportUrine\r\n" +
                      $"    , @SerumStock, @PlasmaStock, @UrineStock, @DNAStock, @CentrifugeCheck, @BloodDrawDate, @SampleStockDate, @SampleTestDate\r\n" +
                      $"    , @SamplePosition, @SampleDescription, @DNAResult, @DNAPurity, @DNA260, @LeftPlasma, @LabgeDescription, @MemberID, GETDATE())\r\n";

                SqlCommand cmd = new SqlCommand(sql);
                cmd = SetSqlParameters(cmd, request);

                LabgeDatabase.ExecuteSqlCommand(cmd);

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
            try
            {
                string sql;
                sql = $"UPDATE BankingSampleInfo\r\n" +
                      $"SET\r\n" +
                      $"    ImportDate = @ImportDate,\r\n" +
                      $"    CompCode = @CompCode,\r\n" +
                      $"    ResearchID = @ResearchID,\r\n" +
                      $"    ChartNo = @ChartNo,\r\n" +
                      $"    DestroyCheck = @DestroyCheck,\r\n" +
                      $"    ImportEDTA = @ImportEDTA,\r\n" +
                      $"    ImportSST = @ImportSST,\r\n" +
                      $"    ImportUrine = @ImportUrine,\r\n" +
                      $"    SerumStock = @SerumStock,\r\n" +
                      $"    PlasmaStock = @PlasmaStock,\r\n" +
                      $"    UrineStock = @UrineStock,\r\n" +
                      $"    DNAStock = @DNAStock,\r\n" +
                      $"    CentrifugeCheck = @CentrifugeCheck,\r\n" +
                      $"    SampleStockDate = @SampleStockDate,\r\n" +
                      $"    SampleTestDate = @SampleTestDate,\r\n" +
                      $"    SampleDescription = @SampleDescription,\r\n" +
                      $"    SamplePosition = @SamplePosition,\r\n" +
                      $"    DNAResult = @DNAResult,\r\n" +
                      $"    DNAPurity = @DNAPurity,\r\n" +
                      $"    DNA260 = @DNA260,\r\n" +
                      $"    LeftPlasma = @LeftPlasma,\r\n" +
                      $"    LabgeDescription = @LabgeDescription,\r\n" +
                      $"    ModifyID = @MemberID,\r\n" +
                      $"    ModifyDateTime = GETDATE()\r\n" +
                      $"WHERE SampleCode = @SampleCode\r\n" +
                      $"AND ImportYearKind = @ImportYearKind\r\n" +
                      $"AND BloodDrawDate = @BloodDrawDate";
                SqlCommand cmd = new SqlCommand(sql);
                cmd = SetSqlParameters(cmd, request);

                LabgeDatabase.ExecuteSqlCommand(cmd);

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

        private SqlCommand SetSqlParameters(SqlCommand cmd, JObject request)
        {
            cmd.Parameters.Add("@BankingKind", SqlDbType.VarChar, 50);
            cmd.Parameters["@BankingKind"].Value = request["BankingKind"].ToString();

            cmd.Parameters.Add("@ImportYearKind", SqlDbType.VarChar, 2);
            cmd.Parameters["@ImportYearKind"].Value = request["ImportYearKind"].ToString();

            cmd.Parameters.Add("@ImportDate", SqlDbType.Date);
            cmd.Parameters["@ImportDate"].Value = CheckDateTime(request["ImportDate"].ToString());

            cmd.Parameters.Add("@SampleCode", SqlDbType.VarChar, 30);
            cmd.Parameters["@SampleCode"].Value = request["SampleCode"].ToString();

            cmd.Parameters.Add("@CompCode", SqlDbType.VarChar, 30);
            cmd.Parameters["@CompCode"].Value = request["CompCode"].ToString();

            cmd.Parameters.Add("@ResearchID", SqlDbType.VarChar, 40);
            cmd.Parameters["@ResearchID"].Value = request["ResearchID"].ToString();

            cmd.Parameters.Add("@ChartNo", SqlDbType.VarChar, 20);
            cmd.Parameters["@ChartNo"].Value = request["ChartNo"].ToString();

            cmd.Parameters.Add("@DestroyCheck", SqlDbType.Bit);
            cmd.Parameters["@DestroyCheck"].Value = request["DestroyCheck"].ToString();

            cmd.Parameters.Add("@ImportEDTA", SqlDbType.Int);
            cmd.Parameters["@ImportEDTA"].Value = request["ImportEDTA"].ToString();

            cmd.Parameters.Add("@ImportSST", SqlDbType.Int);
            cmd.Parameters["@ImportSST"].Value = request["ImportSST"].ToString();

            cmd.Parameters.Add("@ImportUrine", SqlDbType.Int);
            cmd.Parameters["@ImportUrine"].Value = request["ImportUrine"].ToString();

            cmd.Parameters.Add("@SerumStock", SqlDbType.Int);
            cmd.Parameters["@SerumStock"].Value = request["SerumStock"].ToString();

            cmd.Parameters.Add("@PlasmaStock", SqlDbType.Int);
            cmd.Parameters["@PlasmaStock"].Value = request["PlasmaStock"].ToString();

            cmd.Parameters.Add("@UrineStock", SqlDbType.Int);
            cmd.Parameters["@UrineStock"].Value = request["UrineStock"].ToString();

            cmd.Parameters.Add("@DNAStock", SqlDbType.Int);
            cmd.Parameters["@DNAStock"].Value = request["DNAStock"].ToString();

            cmd.Parameters.Add("@CentrifugeCheck", SqlDbType.Bit);
            cmd.Parameters["@CentrifugeCheck"].Value = request["CentrifugeCheck"].ToString();

            cmd.Parameters.Add("@BloodDrawDate", SqlDbType.DateTime);
            cmd.Parameters["@BloodDrawDate"].Value = CheckDateTime(request["BloodDrawDate"].ToString());

            cmd.Parameters.Add("@SampleStockDate", SqlDbType.Date);
            cmd.Parameters["@SampleStockDate"].Value = CheckDateTime(request["SampleStockDate"].ToString());

            cmd.Parameters.Add("@SampleTestDate", SqlDbType.Date);
            cmd.Parameters["@SampleTestDate"].Value = CheckDateTime(request["SampleTestDate"].ToString());

            cmd.Parameters.Add("@SamplePosition", SqlDbType.VarChar, 10);
            cmd.Parameters["@SamplePosition"].Value = request["SamplePosition"].ToString();

            cmd.Parameters.Add("@SampleDescription", SqlDbType.VarChar, 1000);
            cmd.Parameters["@SampleDescription"].Value = request["SampleDescription"].ToString();

            cmd.Parameters.Add("@DNAResult", SqlDbType.VarChar, 20);
            cmd.Parameters["@DNAResult"].Value = request["DNAResult"].ToString();

            cmd.Parameters.Add("@DNAPurity", SqlDbType.VarChar, 20);
            cmd.Parameters["@DNAPurity"].Value = request["DNAPurity"].ToString();

            cmd.Parameters.Add("@DNA260", SqlDbType.VarChar, 20);
            cmd.Parameters["@DNA260"].Value = request["DNA260"].ToString();

            cmd.Parameters.Add("@LeftPlasma", SqlDbType.Bit);
            cmd.Parameters["@LeftPlasma"].Value = request["LeftPlasma"].ToString();

            cmd.Parameters.Add("@LabgeDescription", SqlDbType.VarChar, 1000);
            cmd.Parameters["@LabgeDescription"].Value = request["LabgeDescription"].ToString();

            cmd.Parameters.Add("@MemberID", SqlDbType.VarChar, 20);
            cmd.Parameters["@MemberID"].Value = request["MemberID"].ToString();

            return cmd;
        }

        /// <summary>
        /// 날짜 변환이 되면 날짜를 아니면 DBNull을 리턴
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private object CheckDateTime(string value)
        {
            if (!DateTime.TryParse(value, out DateTime date))
            {
                return DBNull.Value;
            }
            else
            {
                return date;
            }
        }

        /// <summary>
        /// 검체 정보 삭제
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>        
        public IHttpActionResult DeleteBankingSampleInfo(string bankingKind, string importYearKind, string sampleCode)
        {
            try
            {
                string sql;
                sql = $"DELETE FROM BankingSampleInfo\r\n" +
                      $"WHERE BankingKind = '{bankingKind}'\r\n" +
                      $"AND ImportYearKind = '{importYearKind}'\r\n" +
                      $"AND SampleCode = '{sampleCode}'";
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
        /// Banking 권한 (몇개 되지 않는데 DB화 하기엔 좀...)
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        private IEnumerable<JObject> FindMemberBankingKindAuth(string memberId)
        {
            JArray arrAuth = new JArray();
            //CKD 2
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"c3985\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"BRMIM\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"CNUIM\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"GUGIM\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"IBPIM\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"KBSIM\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"PNYPE\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"SEUIM\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"SMHIM\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"SNBIM\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"SNUIM\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"SSVIM\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"IMCIM\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"DSHIM\", BankingKind : \"CKD 2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"PNUIM\", BankingKind : \"CKD 2\"}")));

            //CKD2 소아
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"c3985\", BankingKind : \"소아 CKD2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"SNUPE\", BankingKind : \"소아 CKD2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"SSVPE\", BankingKind : \"소아 CKD2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"AMCPE\", BankingKind : \"소아 CKD2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"SNBPE\", BankingKind : \"소아 CKD2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"CNUPE\", BankingKind : \"소아 CKD2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"KNUPE\", BankingKind : \"소아 CKD2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"PNUPE\", BankingKind : \"소아 CKD2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"JNUPE\", BankingKind : \"소아 CKD2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"KSHPE\", BankingKind : \"소아 CKD2\"}")));
            arrAuth.Add(new JObject(JObject.Parse("{MemberId : \"SMCPE\", BankingKind : \"소아 CKD2\"}")));

            //로그인 멤버별 권한 확인
            var arrResponse = arrAuth.Children<JObject>()
                .Where(o => o["MemberId"].ToString().ToUpper() == memberId.ToUpper());

            return arrResponse;
        }

        [Route("api/Molecular/Banking/CheckBarcodeData")]
        public IHttpActionResult GetCheckBarcodeData(string bankingKind)
        {
            string sql;
            sql = $" SELECT *\r\n" +
                $"FROM BankingCheckBarcode\r\n" +
                $"WHERE BankingKind = '{bankingKind}'\r\n" +
                $"ORDER BY CheckDate DESC";
            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);

            return Ok(arrResponse);
        }

        [Route("api/Molecular/Banking/CheckBarcodeData")]
        public IHttpActionResult PostCheckBarcodeData([FromBody]JObject request)
        {
            string sql;
            sql = $"SELECT *\r\n" +
                $"FROM BankingSampleStock\r\n " +
                $"WHERE BankingKind = '{request["BankingKind"].ToString()}'\r\n" +
                $"AND Barcode = '{request["Barcode"].ToString()}'";
            DataTable dt = LabgeDatabase.SqlToDataTable(sql);

            if (dt.Rows.Count <= 0)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", "등록되지 않은 바코드입니다.");
                return Content(HttpStatusCode.BadRequest, objResponse);
            }

            if (dt.Rows[0]["CheckDate"].ToString() != string.Empty)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", "이미 출고된 바코드 입니다.");
                return Content(HttpStatusCode.BadRequest, objResponse);
            }

            sql = $"INSERT INTO BankingCheckBarcode\r\n" +
                  $"    ( BankingKind, Barcode )\r\n" +
                  $"VALUES" +
                  $"    ('{request["BankingKind"].ToString()}', '{request["Barcode"].ToString()}')";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        [Route("api/Molecular/Banking/ChangeSampleCode")]
        public IHttpActionResult PutChangSampleCode([FromBody]JObject request)
        {
            try
            {
                string sql;
                sql = $"UPDATE BankingSampleInfo\r\n" +
                      $"SET SampleCode = '{request["ChangeSampleCode"].ToString()}'\r\n" +
                      $"WHERE SampleCode = '{request["OriginSampleCode"].ToString()}'\r\n" +
                      $"AND BankingInfoID = '{request["BankingInfoID"].ToString()}'\r\n" +
                      $"\r\n" +
                      $"UPDATE BankingSampleStock\r\n" +
                      $"SET Barcode = REPLACE(Barcode, '{request["OriginSampleCode"].ToString().Replace("-", "")}', '{request["ChangeSampleCode"].ToString().Replace("-", "")}')\r\n" +
                      $"  , SampleCode = '{request["ChangeSampleCode"].ToString()}'\r\n" +
                      $"WHERE SampleCode = '{request["OriginSampleCode"].ToString()}'";
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