using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.ElabLibService;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace supportsapi.labgenomics.com.Controllers.Sales.CompRegistOrder
{
    public class Bizment
    {
        public JArray GetOrder(JObject request)
        {
            string bizOrder = string.Empty;
            //1. 비즈먼트 웹서비스를 호출해서 처방 데이터를 가져옴
            //   현재 사용중인 요양기관번호로 데이터 호출 후 리턴값이 없다면 구 요양기관번호로 재호출
            ELabLibServiceSoapClient bizmentClient = new ELabLibServiceSoapClient();

            //현재 사용중인 요양기관번호
            bizOrder = bizmentClient.SRequestServerReceiveHospital("41355709", request["InstitutionNo"].ToString(), Convert.ToDateTime(request["BeginDate"]).ToString("yyyyMMdd"),
                                                                   Convert.ToDateTime(request["EndDate"]).ToString("yyyyMMdd"), "2");

            //리턴값이 없다면 구 요양기관번호 호출
            if (bizOrder == string.Empty)
            {
                bizOrder = bizmentClient.SRequestServerReceiveHospital("31379010", request["InstitutionNo"].ToString(), Convert.ToDateTime(request["BeginDate"]).ToString("yyyyMMdd"),
                                                                   Convert.ToDateTime(request["EndDate"]).ToString("yyyyMMdd"), "2");
            }

            if (bizOrder != string.Empty)
            {
                InsertBizmentOrder(bizOrder, request["CompCode"].ToString());
            }

            bizmentClient.Close();

            return GetBizmentOrder(request["CompCode"].ToString(), Convert.ToDateTime(request["BeginDate"]).ToString("yyyyMMdd"), Convert.ToDateTime(request["EndDate"]).ToString("yyyyMMdd"), request["RegKind"].ToString());
        }

        /// <summary>
        /// 비즈먼트 오더 xml데이터를 파싱해서 오더 테이블에 등록
        /// </summary>
        /// <param name="bizOrder"></param>
        /// <param name="compCode"></param>
        private void InsertBizmentOrder(string bizOrder, string compCode)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();

            try
            {
                string[] cTextParse = { "^" };

                while (bizOrder.IndexOf("</LAB>") > 0)
                {
                    int lineNum = bizOrder.IndexOf("</LAB>") + 6;

                    #region LAB

                    string cID = string.Empty;
                    string hosNum = string.Empty;
                    string cNo = string.Empty;
                    string wNo = string.Empty;
                    string oNo = string.Empty;

                    #endregion LAB

                    #region CONSIGNEXAM

                    string pName = string.Empty;
                    string pID = string.Empty;
                    string sex = string.Empty;
                    string age = string.Empty;
                    string admopd = string.Empty;

                    #endregion CONSIGNEXAM

                    #region EXAM

                    string cText = string.Empty;
                    string barCode = string.Empty;
                    string emrCode = string.Empty;
                    string confFlag = string.Empty;
                    string jubsuDate = string.Empty;

                    #endregion EXAM

                    #region REQUEST

                    string sCodeSeq = string.Empty;
                    string bzCode = string.Empty;
                    string sbzCode = string.Empty;
                    string ocsName = string.Empty;

                    #endregion REQUEST

                    #region CTEXT에서 파싱할 정보들

                    string ward = string.Empty;
                    string doctor = string.Empty;
                    string dept = string.Empty;

                    #endregion CTEXT에서 파싱할 정보들

                    string compRegDate = string.Empty;

                    XmlDocument xml = new XmlDocument();

                    string order = bizOrder.Substring(0, lineNum);
                    xml.LoadXml(order);

                    XmlNodeList lab = xml.GetElementsByTagName("LAB");
                    foreach (XmlNode node in lab)
                    {
                        cID = node.Attributes["CID"].Value.ToString();
                        hosNum = node.Attributes["HOSNUM"].Value.ToString();
                        cNo = node.Attributes["CNO"].Value.ToString();
                        wNo = node.Attributes["WNO"].Value.ToString();
                        oNo = node.Attributes["ONO"].Value.ToString();
                    }

                    XmlNodeList consignExam = xml.GetElementsByTagName("CONSIGNEXAM");
                    foreach (XmlNode node in consignExam)
                    {
                        pName = node.Attributes["PNAME"].Value.ToString();
                        pID = node.Attributes["PID"].Value.ToString();
                        sex = node.Attributes["SEX"].Value.ToString();
                        age = node.Attributes["AGE"].Value.ToString();
                        admopd = node.Attributes["ADMOPD"].Value.ToString();
                    }

                    XmlNodeList exam = xml.GetElementsByTagName("EXAM");
                    foreach (XmlNode node in exam)
                    {
                        if (node.Attributes["CTEXT"].Value.ToString() != string.Empty)
                        {
                            //CTEXT에서 기타 정보를 파싱
                            string[] cTexts = node.Attributes["CTEXT"].Value.ToString().Split(cTextParse, System.StringSplitOptions.None);
                            ward = cTexts[1];
                            doctor = cTexts[2];
                            dept = cTexts[3];

                            cText = node.Attributes["CTEXT"].Value.ToString();
                            barCode = node.Attributes["BARCODE"].Value.ToString();
                            confFlag = node.Attributes["CONFFLAG"].Value.ToString();
                            jubsuDate = node.Attributes["JUBSUDATE"].Value.ToString();
                        }
                    }

                    //하위 xml값 읽기
                    XmlNodeList list = xml.GetElementsByTagName("REQUEST");

                    foreach (XmlNode node in list)
                    {
                        sCodeSeq = node.Attributes["SCODESEQ"].Value.ToString();
                        bzCode = node.Attributes["BZCODE"].Value.ToString();
                        sbzCode = node.Attributes["SBZCODE"].Value.ToString();
                        compRegDate = DateTime.Parse(node.Attributes["CREDATE"].Value.ToString()).ToString("yyyy-MM-dd");
                        //compRegDate = node.Attributes["CREDATE"].Value.ToString().Substring(0, 10);
                        ocsName = node["OCSNAME"].InnerText;

                        string sql;

                        sql = $"MERGE INTO RsltTransBizmentOrder AS target\r\n" +
                              $"USING (SELECT @HOSNUM AS HOSNUM, @CNO AS CNO, @WNO AS WNO, @ONO AS ONO, @BZCODE AS BZCODE, @SBZCODE AS SBZCODE, @SCODESEQ AS SCODESEQ) AS source\r\n" +
                              $"ON (source.HOSNUM = target.HOSNUM AND source.CNO = target.CNO AND source.WNO = target.WNO AND\r\n" +
                              $"    source.ONO = target.ONO AND source.BZCODE = target.BZCODE AND source.SBZCODE = target.SBZCODE AND\r\n" +
                              $"    source.SCODESEQ = target.SCODESEQ)\r\n" +
                              $"WHEN NOT MATCHED THEN\r\n" +
                              $"    INSERT (CID, CompCode, HOSNUM, CNO, WNO, ONO, CompRegDate, PNAME, PID, SEX, AGE\r\n" +
                              $"         , ADMOPD, CTEXT, BARCODE, EMRCODE, CONFFLAG, JUBSUDATE, SCODESEQ, BZCODE, SBZCODE, OCSNAME, PatientDoctorName\r\n" +
                              $"         , Dept, Ward )\r\n" +
                              $"    VALUES (@CID, @CompCode, @HOSNUM, @CNO, @WNO, @ONO , @CompRegDate, @PNAME, master.dbo.AES_EncryptFunc(@PID, N'labge$%#!dleorms'), @SEX, @AGE\r\n" +
                              $"         , @ADMOPD, @CTEXT, @BARCODE, @EMRCODE, @CONFFLAG, @JUBSUDATE, @SCODESEQ, @BZCODE, @SBZCODE, @OCSNAME, @PatientDoctorName\r\n" +
                              $"         , @Dept, @Ward );";

                        SqlCommand cmd = new SqlCommand(sql, conn);

                        cmd.Parameters.AddWithValue("@CID", cID);
                        cmd.Parameters.AddWithValue("@CompCode", compCode);
                        cmd.Parameters.AddWithValue("@HOSNUM", hosNum);
                        cmd.Parameters.AddWithValue("@CNO", cNo);
                        cmd.Parameters.AddWithValue("@WNO", wNo);
                        cmd.Parameters.AddWithValue("@ONO", oNo);
                        cmd.Parameters.AddWithValue("@CompRegDate", compRegDate);
                        cmd.Parameters.AddWithValue("@PNAME", pName);
                        cmd.Parameters.AddWithValue("@PID", pID);
                        cmd.Parameters.AddWithValue("@SEX", sex);
                        cmd.Parameters.AddWithValue("@AGE", age);
                        cmd.Parameters.AddWithValue("@ADMOPD", admopd);
                        cmd.Parameters.AddWithValue("@CTEXT", cText);
                        cmd.Parameters.AddWithValue("@BARCODE", barCode);
                        cmd.Parameters.AddWithValue("@EMRCODE", emrCode);
                        cmd.Parameters.AddWithValue("@CONFFLAG", confFlag);
                        cmd.Parameters.AddWithValue("@JUBSUDATE", jubsuDate);
                        cmd.Parameters.AddWithValue("@SCODESEQ", sCodeSeq);
                        cmd.Parameters.AddWithValue("@BZCODE", bzCode);
                        cmd.Parameters.AddWithValue("@SBZCODE", sbzCode);
                        cmd.Parameters.AddWithValue("@OCSNAME", ocsName);
                        cmd.Parameters.AddWithValue("@PatientDoctorName", doctor);
                        cmd.Parameters.AddWithValue("@Dept", dept);
                        cmd.Parameters.AddWithValue("@Ward", ward);

                        cmd.ExecuteNonQuery();
                    }

                    bizOrder = bizOrder.Substring(lineNum);
                }
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 비즈먼트 오더 조회
        /// </summary>
        /// <param name="compCode"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="regKind"></param>
        /// <returns></returns>
        private JArray GetBizmentOrder(string compCode, string beginDate, string endDate, string regKind)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();

            try
            {
                string sql = string.Empty;

                if (regKind == "W")
                {
                    sql = $"SELECT CONVERT(bit, 0) AS ColumnCheck, CONVERT(date, GETDATE()) As LabRegDate, '' AS LabRegNo, bizorder.CompCode, bizorder.HOSNUM AS CompInstitutionNo, bizorder.CNO AS PatientChartNo\r\n" +
                          $"     , bizorder.WNO AS CompOrderDate, bizorder.ONO AS CompOrderNo, bizorder.WNO AS SampleDrawDate\r\n" +
                          $"     , bizorder.CompRegDate, bizorder.PNAME AS PatientName, bizorder.SEX AS PatientSex\r\n" +
                          $"     , CASE WHEN bizorder.AGE <= 0 THEN dbo.FN_GetInstitutioNoToAge(master.dbo.AES_DecryptFunc(bizorder.PID, N'labge$%#!dleorms'), bizorder.WNO) ELSE bizorder.AGE END AS PatientAge\r\n" +
                          $"     , SUBSTRING(REPLACE(REPLACE(master.dbo.AES_DecryptFunc(bizorder.PID, N'labge$%#!dleorms') COLLATE Korean_Wansung_CS_AS, '-', ''), '*', ''), 1, 6) AS IdentificationNo1\r\n" +
                          $"     , SUBSTRING(REPLACE(REPLACE(master.dbo.AES_DecryptFunc(bizorder.PID, N'labge$%#!dleorms') COLLATE Korean_Wansung_CS_AS, '-', ''), '*', ''), 7, 7) AS IdentificationNo2\r\n" +
                          $"     , bizorder.SCODESEQ AS SampleNo, bizorder.BZCODE AS CompTestCode, bizorder.SBZCODE AS CompTestSubCode, bizorder.OCSNAME AS CompTestName, bizorder.PatientDoctorName AS DoctorName\r\n" +
                          $"     , bizorder.Dept, bizorder.Ward\r\n" +
                          $"     , CASE WHEN match.CenterMatchCode <> '' THEN match.CenterMatchCode ELSE code.TestCode END AS TestCode\r\n" +
                          $"     , CASE WHEN match.CenterMatchCode <> '' THEN match.CenterMatchOrderCode ELSE code.OrderCode END AS OrderCode\r\n" +
                          $"     , (SELECT TestDisplayName FROM LabTestCode WHERE TestCode = match.CenterMatchCode) AS CenterTestName\r\n" +
                          $"     , (SELECT TestDisplayName FROM LabTestCode WHERE TestCode = match.CenterMatchOrderCode) AS CenterOrderName\r\n" +
                          $"     , code.SampleCode, bizorder.CID, bizorder.ADMOPD, bizorder.CTEXT\r\n" +
                          $"FROM RsltTransBizmentOrder AS bizorder\r\n" +
                          $"LEFT OUTER JOIN RsltTransBizmentCode AS code\r\n" +
                          $"ON code.BZCODE = bizorder.BZCODE\r\n" +
                          $"AND code.SBZCODE = bizorder.SBZCODE\r\n" +
                          $"LEFT OUTER JOIN LabTransMatchCode AS match\r\n" +
                          $"ON match.CompCode = bizorder.CompCode\r\n" +
                          $"AND match.CompMatchCode = bizorder.BZCODE\r\n" +
                          $"AND match.CompMatchSubCode = bizorder.SBZCODE\r\n" +
                          $"WHERE bizorder.CompCode = '{compCode}'\r\n" +
                          $"AND bizorder.CompRegDate BETWEEN '{beginDate}' AND '{endDate}'\r\n" +
                          $"AND NOT EXISTS\r\n" +
                          $"(\r\n" +
                          $"    SELECT NULL\r\n" +
                          $"    FROM LabTransCompOrderInfo orderInfo\r\n" +
                          $"    JOIN LabRegInfo info \r\n " +
                          $"    ON info.LabRegDate = orderInfo.LabRegDate\r\n" +
                          $"    AND info.LabRegNo = orderInfo.LabRegNo\r\n" +
                          $"    AND info.PatientChartNo = bizorder.CNO\r\n" +
                          $"    WHERE orderInfo.CompCode = bizorder.CompCode\r\n" +
                          $"    AND orderinfo.CompOrderDate = bizorder.WNO\r\n" +
                          $"    AND orderinfo.CompOrderNo = bizorder.ONO\r\n" +
                          $"    AND orderInfo.CompTestCode = bizorder.BZCODE\r\n" +
                          $"    AND orderInfo.CompTestSubCode = bizorder.SBZCODE\r\n" +
                          $")\r\n" +
                          $"ORDER BY CNO";
                }
                else
                {
                    sql = $"SELECT CONVERT(bit, 0) AS ColumnCheck, transInfo.LabRegDate, transInfo.LabRegNo, regInfo.PatientName, regInfo.PatientChartNo\r\n" +
                          $"     , regInfo.PatientSex, regInfo.PatientAge, transInfo.CompOrderDate, transinfo.CompOrderNo, transInfo.CompTestCode, transInfo.CompTestName\r\n" +
                          $"     , regInfo.CompDeptCode, regInfo.PatientSickRoom, regInfo.PatientDoctorName, transInfo.TestCode, transInfo.OrderCode\r\n" +
                          $"     , transInfo.CompSpcNo AS SampleNo\r\n" +
                          $"FROM LabTransCompOrderInfo AS transInfo\r\n" +
                          $"LEFT OUTER JOIN LabRegInfo regInfo\r\n" +
                          $"ON regInfo.LabRegDate = transInfo.LabRegDate\r\n" +
                          $"AND regInfo.LabRegNo = transInfo.LabRegNo\r\n" +
                          $"WHERE transInfo.CompCode = '{compCode}'\r\n" +
                          $"AND transInfo.LabRegDate BETWEEN '{beginDate}' AND '{endDate}'\r\n" +
                          $"ORDER BY regInfo.LabRegDate, regInfo.LabRegNo";
                }

                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                adapter.Fill(dt);

                //연동코드 중복을 확인하는 컬럼을 추가
                RegistOrder registOrder = new RegistOrder();
                dt = registOrder.AddColumnDuplicationCodeCheck(dt);

                return JArray.Parse(JsonConvert.SerializeObject(dt));
            }
            finally
            {
                conn.Close();
            }
        }
    }
}