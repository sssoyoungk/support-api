using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace supportsapi.labgenomics.com.Controllers.Sales.CompRegistOrder
{
    public class Juvis
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
                    sql = $"SELECT CONVERT(bit,0) AS ColumnCheck, GETDATE() AS LabRegDate, null AS LabRegNo, ppi.PatientName, ppi.ChartNo AS PatientChartNo\r\n" +
                          $"     , ppi.CompOrderDate, ppi.CompOrderNo, CONVERT(varchar, ppi.SampleDrawDate, 120) SampleDrawDate\r\n" +
                          $"     , SUBSTRING(PatientRegNo, 1, 6) AS IdentificationNo1, Gender AS PatientSex\r\n" +
                          $"     , ppi.Age AS PatientAge\r\n" +
                          $"     , ltmc.CenterMatchCode AS TestCode, ltmc.CenterMatchOrderCode AS OrderCode\r\n" +
                          $"     , pti.CompTestCode, null As CompTestSubCode, pti.CompTestName\r\n" +
                          $"     , CASE WHEN ISNULL(ltmc.CenterMatchSampleCode, '') <> '' THEN ltmc.CenterMatchSampleCode\r\n" +
                          $"            ELSE ltc.SampleCode END AS SampleCode\r\n" +
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
                          $"WHERE ppi.CompOrderDate BETWEEN '{request["BeginDate"].ToString()}' AND '{request["EndDate"].ToString()}'\r\n" +
                          $"AND ppi.CompCode = '{request["CompCode"].ToString()}'\r\n" +
                          $"AND NOT EXISTS\r\n" +
                          $"(\r\n" +
                          $"    SELECT NULL FROM LabTransCompOrderInfo orderInfo\r\n" +
                          $"    WHERE orderInfo.CompCode = ppi.CompCode\r\n" +
                          $"    AND orderinfo.CompOrderDate = ppi.CompOrderDate\r\n" +
                          $"    AND orderinfo.CompOrderNo = ppi.CompOrderNo\r\n" +
                          $"    AND orderInfo.CompTestCode = pti.CompTestCode\r\n" +
                          $")\r\n" +
                          $"ORDER BY CompOrderDate, CompOrderNo";
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