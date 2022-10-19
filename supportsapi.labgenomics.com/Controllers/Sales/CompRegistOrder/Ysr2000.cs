using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace supportsapi.labgenomics.com.Controllers.Sales.CompRegistOrder
{
    public class Ysr2000
    {
        public JArray GetOrder(JObject request)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();
            try
            {
                string sql;

                if (request["RegKind"].ToString() == "W")
                {
                    //LabTransMatchCode 테이블에 정보가 있으면 해당 항목을 먼저 매칭 진행을 하는 쿼리
                    sql =
                        $"SELECT CONVERT(bit,0) AS ColumnCheck, GETDATE() AS LabRegDate, null AS LabRegNo, ysr2000.PNAME AS PatientName, ysr2000.CNO AS PatientChartNo\r\n" +
                        $"     , ysr2000.ORDERDATE AS CompOrderDate, ysr2000.REQID AS CompOrderNo, ysr2000.ORDERDATE AS SampleDrawDate\r\n" +
                        $"     , SUBSTRING(ysr2000.PID, 1, 6) AS IdentificationNo1, SUBSTRING(ysr2000.PID, 8, 7) AS IdentificationNo2, ysr2000.SEX AS PatientSex\r\n" +
                        $"     , dbo.FN_GetInstitutioNoToAge(ysr2000.PID, ysr2000.ORDERDATE) AS PatientAge\r\n" +
                        $"     , CASE WHEN ISNULL(ltmc.CenterMatchOrderCode, '') <> '' THEN ltmc.CenterMatchOrderCode\r\n" +
                        $"            ELSE ysrCode.OrderCode\r\n" +
                        $"       END AS OrderCode\r\n" +
                        $"     , CASE WHEN ISNULL(ltmc.CenterMatchCode, '') <> '' THEN ltmc.CenterMatchCode\r\n" +
                        $"            ELSE ysrCode.TestCode\r\n" +
                        $"       END AS TestCode\r\n" +
                        $"     , ysr2000.STDCODE AS CompTestCode, null As CompTestSubCode\r\n" +
                        $"     , ysrCode.TestName AS CompTestName\r\n" +
                        $"     , CASE WHEN ysr2000.CompCode IN ('9045', '5752') THEN\r\n" +
                        $"           CASE\r\n" +
                        $"               WHEN ISNULL(rtysc.LabgeSampleCode, '') <> '' THEN rtysc.LabgeSampleCode\r\n" +
                        $"               WHEN ISNULL(ysrCode.SampleCode, '') <> '' THEN ysrCode.SampleCode\r\n" +
                        $"               ELSE ltc.SampleCode\r\n" +
                        $"           END\r\n" +
                        $"       ELSE\r\n" +
                        $"           CASE\r\n" +
                        $"               WHEN ISNULL(ysrCode.SampleCode, '') <> '' THEN ysrCode.SampleCode\r\n" +
                        $"               ELSE ltc.SampleCode\r\n" +
                        $"           END\r\n" +
                        $"       END AS SampleCode\r\n" +
                        $"     , ysr2000.SCODE AS CompTestSampleCode\r\n" +
                        $"     , ysr2000.CGCODE AS InsureCode\r\n" +
                        $"     , ysr2000.SUBJECT AS Dept, ysr2000.DOCTORNAME AS DoctorName\r\n" +
                        $"FROM RsltTransYsr2000Order ysr2000\r\n" +
                        $"LEFT OUTER JOIN RsltTransYsr2000Code ysrCode\r\n" +
                        $"ON ysrCode.UBCode = ysr2000.STDCODE\r\n" +
                        $"LEFT OUTER JOIN LabTestCode ltc\r\n" +
                        $"ON ltc.TestCode = ysrCode.TestCode\r\n" +
                        $"LEFT OUTER JOIN RsltTransYsr2000SampleCode rtysc\r\n" +
                        $"ON ysr2000.SCODE = rtysc.YsrSampleCode\r\n" +
                        $"LEFT OUTER JOIN LabTransMatchCode ltmc\r\n" +
                        $"ON ysr2000.CompCode = ltmc.CompCode\r\n" +
                        $"AND ysr2000.STDCODE = ltmc.CompMatchCode\r\n" +
                        $"WHERE ORDERDATE BETWEEN '{request["BeginDate"].ToString()}' AND '{request["EndDate"].ToString()}'\r\n" +
                        $"AND ysr2000.CompCode = '{request["CompCode"].ToString()}'\r\n" +
                        $"AND NOT EXISTS\r\n" +
                        $"(\r\n" +
                        $"    SELECT NULL FROM LabTransCompOrderInfo orderInfo\r\n" +
                        $"    WHERE orderInfo.CompCode = ysr2000.CompCode\r\n" +
                        $"    AND orderinfo.CompOrderDate = ysr2000.ORDERDATE\r\n" +
                        $"    AND orderinfo.CompOrderNo = ysr2000.REQID\r\n" +
                        $"    AND orderInfo.CompTestCode = ysr2000.STDCODE\r\n" +
                        $")\r\n" +
                        $"ORDER BY REQID\r\n";

                    //기존 쿼리
                    //sql = $"SELECT CONVERT(bit,0) AS ColumnCheck, GETDATE() AS LabRegDate, null AS LabRegNo, ysr2000.PNAME AS PatientName, ysr2000.CNO AS PatientChartNo\r\n" +
                    //      $"     , ysr2000.ORDERDATE AS CompOrderDate, ysr2000.REQID AS CompOrderNo, ysr2000.ORDERDATE AS SampleDrawDate\r\n" +
                    //      $"     , SUBSTRING(ysr2000.PID, 1, 6) AS IdentificationNo1, SUBSTRING(ysr2000.PID, 8, 7) AS IdentificationNo2, ysr2000.SEX AS PatientSex\r\n" +
                    //      $"     , dbo.FN_GetInstitutioNoToAge(ysr2000.PID, ysr2000.ORDERDATE) AS PatientAge, ysrCode.OrderCode AS OrderCode, ysrCode.TestCode AS TestCode\r\n" +
                    //      $"     , ysr2000.STDCODE AS CompTestCode, null As CompTestSubCode\r\n" +
                    //      $"     , ysrCode.TestName AS CompTestName\r\n" +
                    //      $"     , CASE WHEN CompCode = '9045' THEN\r\n" +
                    //      $"           CASE\r\n" +
                    //      $"               WHEN ISNULL(rtysc.LabgeSampleCode, '') <> '' THEN rtysc.LabgeSampleCode\r\n" +
                    //      $"               WHEN ISNULL(ysrCode.SampleCode, '') <> '' THEN ysrCode.SampleCode\r\n" +
                    //      $"               ELSE ltc.SampleCode\r\n" +
                    //      $"           END\r\n" +
                    //      $"       ELSE\r\n" +
                    //      $"           CASE\r\n" +
                    //      $"               WHEN ISNULL(ysrCode.SampleCode, '') <> '' THEN ysrCode.SampleCode\r\n" +
                    //      $"               ELSE ltc.SampleCode\r\n" +
                    //      $"           END\r\n" +
                    //      $"       END AS SampleCode\r\n" +
                    //      $"     , ysr2000.SCODE AS CompTestSampleCode\r\n" +
                    //      $"     , ysr2000.CGCODE AS InsureCode\r\n" +
                    //      $"     , ysr2000.SUBJECT AS Dept, ysr2000.DOCTORNAME AS DoctorName\r\n" +
                    //      $"FROM RsltTransYsr2000Order ysr2000\r\n" +
                    //      $"LEFT OUTER JOIN RsltTransYsr2000Code ysrCode\r\n" +
                    //      $"ON ysrCode.UBCode = ysr2000.STDCODE\r\n" +
                    //      $"LEFT OUTER JOIN LabTestCode ltc\r\n" +
                    //      $"ON ltc.TestCode = ysrCode.TestCode\r\n" +
                    //      $"LEFT OUTER JOIN RsltTransYsr2000SampleCode rtysc\r\n" +
                    //      $"ON ysr2000.SCODE = rtysc.YsrSampleCode\r\n" +
                    //      $"WHERE ORDERDATE BETWEEN '{request["BeginDate"].ToString()}' AND '{request["EndDate"].ToString()}'\r\n" +
                    //      $"AND CompCode = '{request["CompCode"].ToString()}'\r\n" +
                    //      $"AND NOT EXISTS\r\n" +
                    //      $"    (\r\n" +
                    //      $"    SELECT NULL FROM LabTransCompOrderInfo orderInfo\r\n" +
                    //      $"    WHERE orderInfo.CompCode = ysr2000.CompCode\r\n" +
                    //      $"    AND orderinfo.CompOrderDate = ysr2000.ORDERDATE\r\n" +
                    //      $"    AND orderinfo.CompOrderNo = ysr2000.REQID\r\n" +
                    //      $"    AND orderInfo.CompTestCode = ysr2000.STDCODE\r\n" +
                    //      $"    )\r\n" +
                    //      $"ORDER BY REQID";
                }
                else
                {
                    sql = $"SELECT CONVERT(bit, 0) AS ColumnCheck, transInfo.LabRegDate, transInfo.LabRegNo, regInfo.PatientName, regInfo.PatientChartNo \r\n" +
                          $"     , regInfo.PatientSex, regInfo.PatientAge, transInfo.CompOrderDate, transinfo.CompOrderNo, transInfo.CompTestCode, transInfo.CompTestName \r\n" +
                          $"     , regInfo.CompDeptCode, regInfo.PatientSickRoom, regInfo.PatientDoctorName, transInfo.TestCode, transInfo.OrderCode \r\n" +
                          $"FROM LabTransCompOrderInfo AS transInfo \r\n" +
                          $"LEFT OUTER JOIN LabRegInfo regInfo \r\n" +
                          $"ON regInfo.LabRegDate = transInfo.LabRegDate \r\n" +
                          $"AND regInfo.LabRegNo = transInfo.LabRegNo \r\n" +
                          $"WHERE transInfo.CompCode = '{request["CompCode"].ToString()}' \r\n" +
                          $"AND transInfo.LabRegDate BETWEEN '{request["BeginDate"].ToString()}' AND '{request["EndDate"].ToString()}'\r\n" +
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