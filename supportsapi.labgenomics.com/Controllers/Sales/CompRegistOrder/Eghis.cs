using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace supportsapi.labgenomics.com.Controllers.Sales.CompRegistOrder
{
    public class Eghis
    {
        public JArray GetOrder(JObject request)
        {
            NpgsqlConnection eghisConn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["EghisConnection"].ConnectionString);
            eghisConn.Open();

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();

            try
            {
                string sql;

                if (request["RegKind"].ToString() == "W") //등록대기
                {                    
                    //if (request["RegistOrder"] != null && Convert.ToBoolean(request["RegistOrder"]))
                    //{
                    //자료를 불러와서 테이블에 넣어준다.
                    sql =
                        $"SELECT hosp_no, hosp_nm, clinic_ymd, ord_ymd, recept_no, ord_cd, ord_no, ord_seq_no\r\n" +
                        $"     , ptnt_no, DECODING(ptnt_nm, hosp_no) AS ptnt_nm, sex, age, ord_nm, spc_cd, spc_nm, ord_type, ord_type_nm, trans_ymd\r\n" +
                        $"     , trans_time, sutak_cd, sts_cd, sts_nm, sutak_ord, sutak_spc, sutak_seq, DECODING(ptnt_prsn_no, hosp_no) AS ptnt_prsn_no\r\n" +
                        $"     , acc_ymd, acc_time, sutak_sts, edi_cd, dept_cd, dept_nm, doct_nm, health_gb\r\n" +
                        $"FROM labge.interface_ord\r\n";

                    if (request["InstitutionNo"].ToString() != string.Empty)
                    {
                        sql += $"WHERE hosp_no = '{request["InstitutionNo"].ToString()}'\r\n";
                    }
                    else
                    {
                        sql += $"WHERE hosp_no <> ''\r\n";
                    }

                    sql +=
                        $"AND ord_ymd BETWEEN '{Convert.ToDateTime(request["BeginDate"]).ToString("yyyyMMdd")}' AND '{Convert.ToDateTime(request["EndDate"]).ToString("yyyyMMdd")}'\r\n" +
                        $"AND sutak_sts is null";

                    DataTable dtOrder = new DataTable();
                    dtOrder.TableName = "EghisOrder";

                    NpgsqlCommand eghisCmd = new NpgsqlCommand(sql, eghisConn);
                    eghisCmd.CommandTimeout = 45;
                    NpgsqlDataAdapter eghisAdapter = new NpgsqlDataAdapter(eghisCmd);

                    eghisAdapter.Fill(dtOrder);


                    sql = string.Empty;
                    foreach (DataRow row in dtOrder.Rows)
                    {
                        //sql += $"MERGE INTO RsltTransEghisOrder AS target\r\n" +
                        //       $"USING (SELECT '{row["hosp_no"]}' AS hosp_no,\r\n" +
                        //       $"              (SELECT pcc.CompCode FROM ProgCompCode pcc\r\n" +
                        //       $"               JOIN RsltTransCompSet rtcs ON rtcs.CompCode = pcc.CompCode AND rtcs.TransKind = 'Eghis'\r\n" +
                        //       $"               WHERE CompInstitutionNo = '{row["hosp_no"]}') AS CompCode,\r\n" +
                        //       $"              '{row["clinic_ymd"]}' AS clinic_ymd, '{row["ord_ymd"]}' AS ord_ymd,\r\n" +
                        //       $"              '{row["recept_no"]}' AS recept_no, '{row["ord_cd"]}' AS ord_cd, '{row["ord_no"]}' AS ord_no, '{row["ord_seq_no"]}' AS ord_seq_no,\r\n" +
                        //       $"              '{row["ptnt_no"]}' AS ptnt_no, '{row["ptnt_nm"]}' AS ptnt_nm) AS source\r\n" +
                        //       $"ON (source.hosp_no = target.hosp_no AND source.CompCode = target.CompCode AND source.clinic_ymd = target.clinic_ymd AND source.ord_ymd = target.ord_ymd AND\r\n" +
                        //       $"    source.recept_no = target.recept_no AND source.ord_cd = target.ord_cd AND source.ord_no = target.ord_no AND\r\n" +
                        //       $"    source.ord_seq_no = target.ord_seq_no AND source.ptnt_no = target.ptnt_no AND source.ptnt_nm = target.ptnt_nm)\r\n" +
                        //       $"WHEN NOT MATCHED THEN\r\n" +
                        //       $"INSERT ( hosp_no, hosp_nm, CompCode, clinic_ymd, ord_ymd, recept_no, ord_cd\r\n" +
                        //       $"       , ord_no, ord_seq_no, ptnt_no, ptnt_nm, sex, age, ord_nm, spc_cd\r\n" +
                        //       $"       , spc_nm, ord_type, ord_type_nm, trans_ymd, trans_time, sutak_cd, sts_cd\r\n" +
                        //       $"       , sts_nm, sutak_ord, sutak_spc, sutak_seq, ptnt_prsn_no\r\n" +
                        //       $"       , acc_ymd, acc_time, sutak_sts, edi_cd, dept_cd, dept_nm, doct_nm, health_gb)\r\n" +
                        //       $"VALUES ( '{row["hosp_no"]}', '{row["hosp_nm"]}',\r\n" +
                        //       $"         (SELECT pcc.CompCode FROM ProgCompCode pcc\r\n" +
                        //       $"          JOIN RsltTransCompSet rtcs ON rtcs.CompCode = pcc.CompCode AND rtcs.TransKind = 'Eghis'\r\n" +
                        //       $"          WHERE CompInstitutionNo = '{row["hosp_no"]}'),\r\n" +
                        //       $"         '{row["clinic_ymd"]}', '{row["ord_ymd"]}', '{row["recept_no"]}', '{row["ord_cd"]}',\r\n" +
                        //       $"         '{row["ord_no"]}', '{row["ord_seq_no"]}', '{row["ptnt_no"]}', '{row["ptnt_nm"]}', '{row["sex"]}', '{row["age"]}', '{row["ord_nm"]}', '{row["spc_cd"]}',\r\n" +
                        //       $"         '{row["spc_nm"]}', '{row["ord_type"]}', '{row["ord_type_nm"]}', '{row["trans_ymd"]}', '{row["trans_time"]}', '{row["sutak_cd"]}', '{row["sts_cd"]}',\r\n" +
                        //       $"         '{row["sts_nm"]}', '{row["sutak_ord"]}', '{row["sutak_spc"]}', '{row["sutak_seq"]}', master.dbo.AES_EncryptFunc('{row["ptnt_prsn_no"]}', N'labge$%#!dleorms'),\r\n" +
                        //       $"         '{row["acc_ymd"]}', '{row["acc_time"]}', '{row["sutak_sts"]}', '{row["edi_cd"]}', '{row["dept_cd"]}', '{row["dept_nm"]}', '{row["doct_nm"]}', '{row["health_gb"]}');\r\n";
                        sql =
                            $"INSERT INTO RsltTransEghisOrder\r\n" +
                            $"(\r\n" +
                            $"    hosp_no, hosp_nm, CompCode, clinic_ymd, ord_ymd, recept_no, ord_cd,\r\n" +
                            $"    ord_no, ord_seq_no, ptnt_no, ptnt_nm, sex, age, ord_nm, spc_cd,\r\n" +
                            $"    spc_nm, ord_type, ord_type_nm, trans_ymd, trans_time, sutak_cd, sts_cd,\r\n" +
                            $"    sts_nm, sutak_ord, sutak_spc, sutak_seq, ptnt_prsn_no\r\n," +
                            $"    acc_ymd, acc_time, sutak_sts, edi_cd, dept_cd, dept_nm, doct_nm, health_gb\r\n" +
                            $")\r\n" +
                            $"VALUES\r\n" +
                            $"(\r\n" +
                            $"    '{row["hosp_no"]}', '{row["hosp_nm"]}',\r\n" +
                            $"    (SELECT pcc.CompCode FROM ProgCompCode pcc\r\n" +
                            $"     JOIN RsltTransCompSet rtcs ON rtcs.CompCode = pcc.CompCode AND rtcs.TransKind = 'Eghis'　AND pcc.IsCompUseCode = 1 AND rtcs.IsUse = 1\r\n" +
                            $"     WHERE CompInstitutionNo = '{row["hosp_no"]}'),\r\n" +
                            $"    '{row["clinic_ymd"]}', '{row["ord_ymd"]}', '{row["recept_no"]}', '{row["ord_cd"]}',\r\n" +
                            $"    '{row["ord_no"]}', '{row["ord_seq_no"]}', '{row["ptnt_no"]}', '{row["ptnt_nm"]}', '{row["sex"]}', '{row["age"]}', '{row["ord_nm"]}', '{row["spc_cd"]}',\r\n" +
                            $"    '{row["spc_nm"]}', '{row["ord_type"]}', '{row["ord_type_nm"]}', '{row["trans_ymd"]}', '{row["trans_time"]}', '{row["sutak_cd"]}', '{row["sts_cd"]}',\r\n" +
                            $"    '{row["sts_nm"]}', '{row["sutak_ord"]}', '{row["sutak_spc"]}', '{row["sutak_seq"]}', master.dbo.AES_EncryptFunc('{row["ptnt_prsn_no"]}', N'labge$%#!dleorms'),\r\n" +
                            $"    '{row["acc_ymd"]}', '{row["acc_time"]}', '{row["sutak_sts"]}', '{row["edi_cd"]}', '{row["dept_cd"]}', '{row["dept_nm"]}', '{row["doct_nm"]}', '{row["health_gb"]}'\r\n" +
                            $")";
                        try
                        {
                            SqlCommand cmd = new SqlCommand(sql, conn);
                            cmd.ExecuteNonQuery();

                            sql =
                                $"UPDATE interface_ord\r\n" +
                                $"SET sutak_sts = 'D'\r\n" +
                                $"WHERE hosp_no = '{row["hosp_no"]}'\r\n" +
                                $"AND clinic_ymd = '{row["clinic_ymd"]}'\r\n" +
                                $"AND recept_no = '{row["recept_no"]}'\r\n" +
                                $"AND ord_cd = '{row["ord_cd"]}'\r\n" +
                                $"AND ord_no = '{row["ord_no"]}'\r\n" +
                                $"AND ord_seq_no = '{row["ord_seq_no"]}'";
                            NpgsqlCommand eghisCmd2 = new NpgsqlCommand(sql, eghisConn);
                            eghisCmd2.ExecuteNonQuery();
                        }
                        catch
                        {

                        }
                    }
                    //}

                    sql =
                        $"SELECT ColumnCheck, LabRegDate, LabRegNo, PatientName, PatientChartNo, IdentificationNo1, IdentificationNo2, PatientSex, PatientAge, CompOrderDate, CompOrderNo\r\n" +
                        $"     , TestCode, OrderCode, CompTestSampleCode, CompTestCode, CompTestSubCode, CompTestName, Dept, DoctorName, CenterTestName, CenterOrderName\r\n" +
                        $"     , InsureCode, CompExpansionField01, CompExpansionField02, Gongdan\r\n" +
                        $"     , CASE WHEN ISNULL(CenterMatchSampleCode, '') = ''\r\n" +
                        $"            THEN (SELECT SampleCode FROM LabTestCode WHERE TestCode = Sub1.TestCode)\r\n" +
                        $"            ELSE CenterMatchSampleCode\r\n" +
                        $"       END AS SampleCode\r\n" +
                        $"     , CONVERT(char(19), TestSendDateTime, 20) AS TestSendDateTime\r\n" +
                        $"FROM\r\n" +
                        $"(\r\n" +
                        $"    SELECT CONVERT(bit,0) AS ColumnCheck, GETDATE() AS LabRegDate, '' AS LabRegNo, eghisOrder.ptnt_nm AS PatientName, eghisOrder.ptnt_no AS PatientChartNo\r\n" +
                        $"         , SUBSTRING(master.dbo.AES_DecryptFunc(eghisOrder.ptnt_prsn_no, N'labge$%#!dleorms'), 1, 6) AS IdentificationNo1\r\n" +
                        $"         , SUBSTRING(master.dbo.AES_DecryptFunc(eghisOrder.ptnt_prsn_no, N'labge$%#!dleorms'), 7, 7) AS IdentificationNo2\r\n" +
                        $"         , eghisOrder.sex AS PatientSex, eghisOrder.age AS PatientAge, CONVERT(Date, eghisOrder.ord_ymd) AS CompOrderDate, eghisOrder.recept_no AS CompOrderNo\r\n" +
                        $"         , CASE WHEN eghisOrder.health_gb = 'Y' THEN match.CenterGongDanCode ELSE match.CenterMatchCode END AS TestCode\r\n" +
                        $"         , CASE WHEN eghisOrder.health_gb = 'Y' THEN '' ELSE match.CenterMatchOrderCode END AS OrderCode\r\n" +
                        $"         , eghisOrder.spc_cd AS CompTestSampleCode\r\n" +
                        $"         , eghisOrder.ord_cd AS CompTestCode, '' AS CompTestSubCode, eghisOrder.ord_nm AS CompTestName\r\n" +
                        $"         , CASE" +
                        $"               WHEN eghisOrder.dept_nm = '진료실' THEN ''\r\n" +
                        $"               ELSE eghisOrder.dept_nm\r\n" +
                        $"           END AS Dept\r\n" +
                        $"         , eghisOrder.doct_nm AS DoctorName\r\n" +
                        $"         , CASE\r\n" +
                        $"               WHEN eghisOrder.health_gb = 'Y'\r\n" +
                        $"               THEN (SELECT TestDisplayName FROM LabTestCode WHERE TestCode = match.CenterGongDanCode)\r\n" +
                        $"               ELSE (SELECT TestDisplayName FROM LabTestCode WHERE TestCode = match.CenterMatchCode)\r\n" +
                        $"           END AS CenterTestName\r\n" +
                        $"         , CASE\r\n" +
                        $"               WHEN eghisOrder.health_gb = 'Y'\r\n" +
                        $"               THEN ''\r\n" +
                        $"               ELSE (SELECT TestDisplayName FROM LabTestCode WHERE TestCode = match.CenterMatchOrderCode)\r\n" +
                        $"           END AS CenterOrderName\r\n" +
                        $"         , eghisOrder.edi_cd AS InsureCode, eghisOrder.ord_no AS CompExpansionField01, eghisOrder.ord_seq_no AS CompExpansionField02\r\n" +
                        $"         , eghisOrder.health_gb AS Gongdan, eghisOrder.clinic_ymd, eghisOrder.sutak_spc\r\n" +
                        $"         , match.CenterMatchSampleCode\r\n" +
                        $"         , CONVERT(DATETIME, STUFF(STUFF(STUFF(trans_ymd + trans_time, 13,0 , ':'), 11, 0, ':'), 9, 0, ' ')) AS TestSendDateTime\r\n" +
                        $"    FROM RsltTransEghisOrder eghisOrder\r\n" +
                        $"    LEFT OUTER JOIN LabTransMatchCode match\r\n" +
                        $"    ON match.CompCode = eghisOrder.CompCode\r\n" +
                        $"    AND match.CompMatchCode = eghisOrder.ord_cd\r\n" +
                        $"    LEFT OUTER JOIN LabTestCode AS testCode\r\n" +
                        $"    ON match.CenterMatchCode = testCode.TestCode\r\n" +
                        $"    WHERE eghisOrder.CompCode = '{request["CompCode"].ToString()}'\r\n" +
                        $"    AND eghisOrder.ord_ymd BETWEEN '{Convert.ToDateTime(request["BeginDate"]).ToString("yyyyMMdd")}' AND '{Convert.ToDateTime(request["EndDate"]).ToString("yyyyMMdd")}'\r\n" +
                        $"    AND NOT EXISTS\r\n" +
                        $"    (\r\n" +
                        $"        SELECT NULL FROM LabTransCompOrderInfo orderInfo\r\n" +
                        $"        WHERE orderInfo.CompCode = eghisOrder.CompCode\r\n" +
                        $"        AND orderinfo.CompOrderDate = CONVERT(DATE, eghisOrder.ord_ymd)\r\n" +
                        $"        AND orderinfo.CompOrderNo = eghisOrder.recept_no\r\n" +
                        $"        AND orderInfo.CompTestCode = eghisOrder.ord_cd\r\n" +
                        $"        AND orderInfo.CompExpansionField01 = eghisOrder.ord_no\r\n" +
                        $"        AND orderInfo.CompExpansionField02 = eghisOrder.ord_seq_no\r\n" +
                        $"    )\r\n" +
                        $") AS Sub1\r\n" +
                        $"ORDER BY clinic_ymd, CompOrderNo, CompOrderDate, CompExpansionField01, CompExpansionField02\r\n" +
                        $"       , CompTestCode, InsureCode, sutak_spc";

                    DataTable dt = new DataTable("EghisOrder");
                    SqlCommand cmdSelect = new SqlCommand(sql, conn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmdSelect);

                    adapter.Fill(dt);

                    RegistOrder registOrder = new RegistOrder();
                    dt = registOrder.AddColumnDuplicationCodeCheck(dt);
                    JArray array = JArray.Parse(JsonConvert.SerializeObject(dt));
                    return array;
                }
                else //등록자료
                {
                    sql = $"SELECT CONVERT(bit, 0) AS ColumnCheck, ltcoi.LabRegDate, ltcoi.LabRegNo, lri.PatientName, lri.PatientChartNo\r\n" +
                          $"     , lri.PatientSex, lri.PatientAge, ltcoi.CompOrderDate, ltcoi.CompOrderNo, ltcoi.CompTestCode, ltcoi.CompTestName\r\n" +
                          $"     , lri.CompDeptCode, lri.PatientSickRoom, lri.PatientDoctorName, ltcoi.TestCode, ltcoi.OrderCode\r\n" +
                          $"     , ltcoi.CompSpcNo AS SampleNo\r\n" +
                          $"FROM LabTransCompOrderInfo AS ltcoi\r\n" +
                          $"LEFT OUTER JOIN LabRegInfo lri\r\n" +
                          $"ON lri.LabRegDate = ltcoi.LabRegDate\r\n" +
                          $"AND lri.LabRegNo = ltcoi.LabRegNo\r\n" +
                          $"WHERE ltcoi.LabRegDate BETWEEN '{Convert.ToDateTime(request["BeginDate"]).ToString("yyyy-MM-dd")}' AND '{Convert.ToDateTime(request["EndDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                    $"AND ltcoi.CompCode = '{request["CompCode"].ToString()}'\r\n" +
                    $"ORDER BY lri.LabRegDate, lri.LabRegNo";

                    JArray array = LabgeDatabase.SqlToJArray(sql);
                    return array;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                conn.Close();
                eghisConn.Close();
            }
        }
    }
}