using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace supportsapi.labgenomics.com.Controllers.Sales.CompRegistOrder
{
    public class OSB
    {
        public JArray GetOrder(JObject request)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();
            try
            {
                string sql = string.Empty;

                if (request["RegKind"].ToString() == "W")
                {
                    sql =
                        $"SELECT\r\n" +
                        $"    CONVERT(bit,0) AS ColumnCheck, GETDATE() AS LabRegDate, '' AS LabRegNo,\r\n" +
                        $"    oo.CompCode, CompOrderDate, OsbOrderID AS CompOrderNo, PatientName, CompTestName, CompTestCode, BirthDay,\r\n" +
                        $"    FLOOR(CAST(DATEDIFF(DAY, BirthDay, CompOrderDate) AS INTEGER)/365.2422) AS PatientAge,\r\n" +
                        $"    ltmc.CenterMatchCode AS TestCode,\r\n" +
                        $"    Height, Weight, Gender AS PatientSex, FetusNumber, GestationalAgeWeek, GestationalAgeDay,\r\n" +
                        $"    SampleDrawDate,\r\n" +
                        $"    CASE WHEN ISNULL(ltmc.CenterMatchSampleCode, '') <> '' THEN ltmc.CenterMatchSampleCode\r\n" +
                        $"         ELSE ltc.SampleCode END AS SampleCode," +
                        $"    CONVERT(varchar(6), BirthDay, 12) AS IdentificationNo1\r\n" +
                        $"FROM OsbOrders oo\r\n" +
                        $"LEFT OUTER JOIN LabTransMatchCode AS ltmc\r\n" +
                        $"ON ltmc.CompCode = oo.CompCode\r\n" +
                        $"AND ltmc.CompMatchCode = oo.CompTestCode\r\n" +
                        $"LEFT OUTER JOIN LabTestCode ltc\r\n" +
                        $"ON ltmc.CenterMatchCode = ltc.TestCode\r\n" +
                        $"WHERE oo.CompCode = '{request["CompCode"].ToString()}'\r\n" +
                        $"AND oo.CompOrderDate BETWEEN '{request["BeginDate"].ToString()}' AND '{request["EndDate"].ToString()}'\r\n" +
                        $"AND oo.LabRegDate IS NULL\r\n" +
                        $"ORDER BY oo.RegsitDateTime";

                }
                else
                {
                    sql = 
                        $"SELECT\r\n" +
                        $"    CONVERT(bit, 0) AS ColumnCheck, oo.LabRegDate, oo.LabRegNo,\r\n" +
                        $"    oo.CompCode, CompOrderDate, OsbOrderID AS CompOrderNo, PatientName, CompTestName, CompTestCode, BirthDay,\r\n" +
                        $"    ltmc.CenterMatchCode AS TestCode,\r\n" +
                        $"    Height, Weight, Gender AS PatientSex, FetusNumber, GestationalAgeWeek, GestationalAgeDay,\r\n" +
                        $"    SampleDrawDate,\r\n" +
                        $"    CASE WHEN ISNULL(ltmc.CenterMatchSampleCode, '') <> '' THEN ltmc.CenterMatchSampleCode\r\n" +
                        $"         ELSE ltc.SampleCode END AS SampleCode\r\n" +
                        $"FROM OsbOrders oo\r\n" +
                        $"LEFT OUTER JOIN LabTransMatchCode AS ltmc\r\n" +
                        $"ON ltmc.CompCode = oo.CompCode\r\n" +
                        $"AND ltmc.CompMatchCode = oo.CompTestCode\r\n" +
                        $"LEFT OUTER JOIN LabTestCode ltc\r\n" +
                        $"ON ltmc.CenterMatchCode = ltc.TestCode\r\n" +
                        $"WHERE oo.CompCode = '{request["CompCode"].ToString()}'\r\n" +
                        $"AND oo.LabRegDate BETWEEN '{request["BeginDate"].ToString()}' AND '{request["EndDate"].ToString()}'\r\n" +
                        $"AND oo.LabRegDate IS NOT NULL\r\n" +
                        $"ORDER BY oo.RegsitDateTime";
                }
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                adapter.Fill(dt);

                return JArray.Parse(JsonConvert.SerializeObject(dt));
            }
            finally
            {
                conn.Close();
            }
        }
    }
}