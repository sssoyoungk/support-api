using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [Route("api/Sales/SendPlsLabOrder")]
    public class SendPlsLabOrderController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string kindReg)
        {
            OracleConnection connComp = new OracleConnection(ConfigurationManager.ConnectionStrings["PlsLabConnection"].ConnectionString);
            OracleCommand cmdComp = connComp.CreateCommand();
            OracleDataAdapter adapterComp = new OracleDataAdapter();

            DataTable dtRtn = new DataTable("ReturnTable");
            try
            {
                //1. 이기은 병리과 DB에 접속해서 연동데이터 불러옴.
                DataTable dtComp = new DataTable("LISConMaster");

                connComp.Open();
                cmdComp.BindByName = true;

                string sql;

                if (kindReg == "W")
                {
                    //Oracle Column 대소문자를 구분하기 위해 ""사이에 컬럼명을 넣어준다.

#if DEBUG
                    sql = $"SELECT TO_DATE(REQDTE, 'YYYY-MM-DD') AS \"CompOrderDate\", REGNO AS \"CompOrderNo\", REQNO AS \"SampleNo\", ITEMCD AS \"CompTestCode\"\r\n" +
                          $"     , ITEMNM AS \"CompTestName\", SAMPLENM AS \"CompSampleCode\" , CHARTNO AS \"ChartNo\", PATNM AS \"PatientName\", BIRDTE AS \"IdentificationNo\"\r\n" +
                          $"     , SUBSTR(BIRDTE, 1, 6) AS \"IdentificationNo1\", SUBSTR(BIRDTE, 8, 7) AS \"IdentificationNo2\"\r\n" +
                          $"     , CASE SEX WHEN '남' THEN 'M' WHEN '여' THEN 'F' END AS \"SexKind\"\r\n" +
                          $"     , NVL(AGE, 0) AS \"Age\", JINRYO AS \"Dept\", HOSDOC AS \"DoctorName\"\r\n " +
                          $"     , CSTCD AS \"CstCd\", CSTNM AS \"CstNm\"\r\n" +
                          $"FROM MESDB.LISConMaster\r\n" +
                          $"WHERE REQDTE BETWEEN '{beginDate.ToString("yyyyMMdd")}' AND '{endDate.ToString("yyyyMMdd")}'\r\n" +
                          $"AND OutCom = '99908'";
#else
                    sql = $"SELECT TO_DATE(REQDTE, 'YYYY-MM-DD') AS \"CompOrderDate\", REGNO AS \"CompOrderNo\", REQNO AS \"SampleNo\", ITEMCD AS \"CompTestCode\"\r\n" +
                          $"     , ITEMNM AS \"CompTestName\", SAMPLENM AS \"CompSampleCode\" , CHARTNO AS \"ChartNo\", PATNM AS \"PatientName\", BIRDTE AS \"IdentificationNo\"\r\n" +
                          $"     , SUBSTR(BIRDTE, 1, 6) AS \"IdentificationNo1\", SUBSTR(BIRDTE, 8, 7) AS \"IdentificationNo2\"\r\n" +
                          $"     , CASE SEX WHEN '남' THEN 'M' WHEN '여' THEN 'F' END AS \"SexKind\"\r\n" +
                          $"     , NVL(AGE, 0) AS \"Age\", JINRYO AS \"Dept\", HOSDOC AS \"DoctorName\"\r\n " +
                          $"     , CSTCD AS \"CstCd\", CSTNM AS \"CstNm\"\r\n" +
                          $"FROM MESDB.LISConMaster\r\n" +
                          $"WHERE REQDTE BETWEEN '{beginDate.ToString("yyyyMMdd")}' AND '{endDate.ToString("yyyyMMdd")}'\r\n" +
                          $"AND OutCom = '99908'\r\n" +
                          $"AND Result_Down = 'J'";
#endif


                    cmdComp.CommandText = sql;

                    adapterComp.SelectCommand = cmdComp;
                    adapterComp.Fill(dtComp);

                    //2-1. 우리 연동 테이블에서 자료를 조회
                    sql = $"SELECT CompOrderDate, CompOrderNo, CompTestCode\r\n" +
                          $"FROM LabTransCompOrderInfo\r\n" +
                          $"WHERE CompOrderDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                          $"AND CompCode = '4236'";
                    DataTable dtLabge = new DataTable("LabTransCompOrderInfo");
                    dtLabge = LabgeDatabase.SqlToDataTable(sql);
#if DEBUG
                    dtLabge.Rows.Clear();
#endif

                    //2-2. 매핑코드를 가져오자                    
                    sql = $"SELECT A.CompMatchCode, A.CompMatchSubCode, A.CenterMatchCode, A.CenterMatchOrderCode\r\n" +
                          $"     , CASE WHEN A.CenterMatchSampleCode <> '' THEN A.CenterMatchSampleCode ELSE B.SampleCode END AS SampleCode\r\n" +
                          $"     , (SELECT TestDisplayName FROM LabTestCode WHERE A.CenterMatchCode = TestCode) AS CenterTestName\r\n " +
                          $"     , (SELECT TestDisplayName FROM LabTestCode WHERE A.CenterMatchOrderCode = TestCode) AS CenterOrderName\r\n " +
                          $"FROM LabTransMatchCode AS A\r\n" +
                          $"JOIN LabTestCode AS B\r\n" +
                          $"ON B.TestCode = A.CenterMatchCode\r\n " +
                          $"WHERE A.CompCode = '4236'";
                    DataTable dtMatchCode = new DataTable("LabTransMatchCode");
                    dtMatchCode = LabgeDatabase.SqlToDataTable(sql);

                    //3. 조회조건에 맞게 Linq JOIN으로 데이터 테이블 생성하여 return하려 했는데 linq JOIN을 하면 데이터 조회가 안되서 원시적으로 처리하자.
                    //데이터를 가져오면서 매치코드도 같이 가져오자.
                    foreach (DataColumn col in dtComp.Columns)
                    {
                        dtRtn.Columns.Add(col.ColumnName, col.DataType);
                    }
                    dtRtn.Columns.Add("LabRegDate", typeof(DateTime));
                    dtRtn.Columns.Add("LabRegNo", typeof(int));
                    dtRtn.Columns.Add("TestCode", typeof(string));
                    dtRtn.Columns.Add("OrderCode", typeof(string));
                    dtRtn.Columns.Add("SampleCode", typeof(string));
                    dtRtn.Columns.Add("CenterTestName", typeof(string));
                    dtRtn.Columns.Add("CenterOrderName", typeof(string));
                    dtRtn.Columns.Add("DuplicationInfo", typeof(bool));

                    foreach (DataRow compRow in dtComp.Rows)
                    {
                        bool checkReg = false;
                        foreach (DataRow labgeRow in dtLabge.Rows)
                        {
                            if (compRow["CompOrderDate"].ToString() == labgeRow["CompOrderDate"].ToString() && compRow["CompOrderNo"].ToString() == labgeRow["CompOrderNo"].ToString() &&
                                compRow["CompTestCode"].ToString() == labgeRow["CompTestCode"].ToString())
                            {
                                checkReg = true;
                                break;
                            }
                        }

                        if (checkReg == false)
                        {
                            DataRow row = dtRtn.NewRow();

                            row["LabRegDate"] = DateTime.Now;
                            row["LabRegNo"] = compRow["CompOrderNo"];
                            row["DuplicationInfo"] = false;

                            foreach (DataColumn col in dtComp.Columns)
                            {
                                row[col.ColumnName] = compRow[col.ColumnName];
                            }

                            foreach (DataRow matchRow in dtMatchCode.Rows)
                            {
                                if (compRow["CompTestCode"].ToString().Trim() == matchRow["CompMatchCode"].ToString().Trim() &&
                                    compRow["CompSampleCode"].ToString().Trim() == matchRow["CompMatchSubCode"].ToString().Trim())
                                {
                                    row["TestCode"] = matchRow["CenterMatchCode"];
                                    row["OrderCode"] = matchRow["CenterMatchOrderCode"];
                                    row["SampleCode"] = matchRow["SampleCode"];
                                    row["CenterTestName"] = matchRow["CenterTestName"];
                                    row["CenterOrderName"] = matchRow["CenterOrderName"];
                                    break;
                                }
                            }

                            dtRtn.Rows.Add(row);
                        }
                    }
                }
                else
                {
                    sql = $"SELECT CONVERT(varchar, A.CompOrderDate, 112) AS CompOrderDate, A.CompOrderNo, A.CompSpcNo As SampleNo, A.CompTestCode\r\n" +
                          $"     , A.CompTestSubCode, A.CompTestName, A.LabRegDate, A.LabRegNo, A.OrderCode, A.TestCode\r\n " +
                          $"     , B.PatientName, B.PatientAge AS Age, B.PatientSex AS SexKind, B.PatientChartNo AS ChartNo\r\n" +
                          $"     , B.PatientJuminNo01 AS IdentificationNo1, master.dbo.AES_DecryptFunc(B.PatientJuminNo02, 'labge$%#!dleorms') AS IdentificationNo2\r\n" +
                          $"FROM LabTransCompOrderInfo AS A\r\n" +
                          $"LEFT OUTER JOIN LabRegInfo AS B\r\n" +
                          $"ON B.LabRegDate = A.LabRegDate\r\n" +
                          $"AND B.LabRegNo = A.LabRegNo\r\n" +
                          $"WHERE A.CompOrderDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                          $"AND A.CompCode = '4236'";
                    dtRtn = LabgeDatabase.SqlToDataTable(sql);
                }

                JArray arrResponse = JArray.Parse(JsonConvert.SerializeObject(dtRtn));
                return Ok(arrResponse);
            }
            finally
            {
                connComp.Close();
            }
        }
    }
}