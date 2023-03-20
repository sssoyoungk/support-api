using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace supportsapi.labgenomics.com.Controllers.Sales.CompRegistOrder
{
    public class IntegratedAPI
    {
        public JArray GetOrder(JObject request)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();

            try
            {
                string sql = string.Empty;

                if (request["RegKind"].ToString() == "W") //등록대기
                {
                    sql = $"SELECT CONVERT(bit, 0) AS ColumnCheck, GETDATE() AS LabRegDate, '' AS LabRegNo, PatientName AS PatientName, ChartNo AS PatientChartNo\r\n" +
                          $"     , Gender AS PatientSex\r\n" +
                          $"     , Age AS PatientAge, CompOrderDate, CompOrderNo\r\n" +
                          $"     , CASE WHEN ISNULL(SampleDrawDate, '') = '' THEN CompOrderDate ELSE SampleDrawDate END AS SampleDrawDate\r\n" +
                          $"     , CompTestCode, CompTestName\r\n" +
                          $"     , Dept, Ward, DoctorName\r\n" +
                          $"     , match.CenterMatchCode AS TestCode, match.CenterMatchOrderCode AS OrderCode, SampleNo, testCode.SampleCode\r\n" +
                          $"     , PatientRegNo01 AS IdentificationNo1, master.dbo.AES_DecryptFunc(PatientRegNo02, 'labge$%#!dleorms') AS IdentificationNo2, '' AS CompTestSubCode\r\n" +
                          $"     , (SELECT TestDisplayName FROM LabTestCode WHERE TestCode = match.CenterMatchCode) AS CenterTestName\r\n" +
                          $"     , (SELECT TestDisplayName FROM LabTestCode WHERE TestCode = match.CenterMatchOrderCode) AS CenterOrderName\r\n" +
                          $"     , ZipCode, Address, PhoneNo\r\n" +
                          $"     , PatientImportCustomData01, PatientImportCustomData02, PatientImportCustomData03, Description\r\n" +
                          $"FROM RsltTransIntegratedAPIOrder APIOrder\r\n" +
                          $"LEFT OUTER JOIN LabTransMatchCode AS match\r\n" +
                          $"ON match.CompCode = APIOrder.CompCode\r\n" +
                          $"AND match.CompMatchCode = APIOrder.CompTestCode\r\n" +
                          $"LEFT OUTER JOIN LabTestCode AS testCode\r\n" +
                          $"ON match.CenterMatchCode = testCode.TestCode\r\n" +
                          $"WHERE APIOrder.CompCode = '{request["CompCode"].ToString()}'\r\n" +
                          $"AND APIOrder.CompOrderDate BETWEEN '{Convert.ToDateTime(request["BeginDate"]).ToString("yyyyMMdd")}' AND '{Convert.ToDateTime(request["EndDate"]).ToString("yyyyMMdd")}'\r\n" +
                          $"AND NOT EXISTS\r\n" +
                          $"    (\r\n" +
                          $"    SELECT NULL\r\n" +
                          $"    FROM LabTransCompOrderInfo APIOrderInfo\r\n" +
                          $"    JOIN LabRegInfo regInfo\r\n" +
                          $"    ON APIOrderInfo.LabRegDate = regInfo.LabRegDate\r\n" +
                          $"    AND APIOrderInfo.LabRegNo = regInfo.LabRegNo\r\n" +
                          $"    WHERE APIOrderInfo.CompCode = APIOrder.CompCode\r\n" +
                          $"    AND APIOrderInfo.CompOrderDate = APIOrder.CompOrderDate\r\n" +
                          $"    AND APIOrderInfo.CompOrderNo = APIOrder.CompOrderNo\r\n" +
                          $"    AND APIOrderInfo.CompSpcNo = APIOrder.SampleNo\r\n" +
                          $"    AND APIOrderInfo.CompTestCode = APIOrder.CompTestCode\r\n" +
                          $"    AND regInfo.PatientChartNo = APIOrder.ChartNo\r\n" +
                          $"    )\r\n" +
                          $"ORDER BY ChartNo, CompOrderDate";
                }
                else //등록
                {
                    sql = $"SELECT CONVERT(bit, 0) AS ColumnCheck, transInfo.LabRegDate, transInfo.LabRegNo, regInfo.PatientName, regInfo.PatientChartNo \r\n" +
                          $"     , regInfo.PatientSex, regInfo.PatientAge, transInfo.CompOrderDate, transinfo.CompOrderNo, transInfo.CompTestCode, transInfo.CompTestName \r\n" +
                          $"     , regInfo.CompDeptCode, regInfo.PatientSickRoom, regInfo.PatientDoctorName, transInfo.TestCode, transInfo.OrderCode \r\n" +
                          $"     , regInfo.PatientJuminNo01 AS IdentificationNo1, master.dbo.AES_DecryptFunc(PatientJuminNo02, N'labge$%#!dleorms') AS IdentificationNo2\r\n" +
                          $"     , transInfo.CompSpcNo AS SampleNo\r\n" +
                          $"FROM LabTransCompOrderInfo AS transInfo \r\n" +
                          $"LEFT OUTER JOIN LabRegInfo regInfo \r\n" +
                          $"ON regInfo.LabRegDate = transInfo.LabRegDate \r\n" +
                          $"AND regInfo.LabRegNo = transInfo.LabRegNo \r\n" +
                          $"WHERE transInfo.CompCode = '{request["CompCode"].ToString()}' \r\n" +
                          $"AND transInfo.LabRegDate BETWEEN '{Convert.ToDateTime(request["BeginDate"]).ToString("yyyyMMdd")}' AND '{Convert.ToDateTime(request["EndDate"]).ToString("yyyyMMdd")}' \r\n" +
                          $"ORDER BY transInfo.LabRegDate, transInfo.LabRegNo";
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