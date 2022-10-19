using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace supportsapi.labgenomics.com.Controllers.Sales.CompRegistOrder
{
    public class CloudEMR
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
                    sql = $"SELECT CONVERT(bit, 0) AS ColumnCheck, GETDATE() AS LabRegDate, '' AS LabRegNo, PatientName AS PatientName, PatientNo AS PatientChartNo\r\n" +
                          $"     , Sex AS PatientSex\r\n" +
                          $"     , dbo.FN_GetInstitutioNoToAge(PatientRegNo, OrderDateTime) AS PatientAge, CONVERT(DATE, OrderDateTime) AS CompOrderDate\r\n" +
                          $"     , ReqID AS CompOrderNo, uCode AS CompTestCode, UserExamName AS CompTestName\r\n" +
                          $"     , Dept AS Dept, DoctorRoomNo AS Ward, DoctorName AS DoctorName\r\n" +
                          $"     , match.CenterMatchCode AS TestCode, match.CenterMatchOrderCode AS OrderCode, '' AS SampleNo, testCode.SampleCode\r\n" +
                          $"     , '' AS IdentificationNo1, '' AS IdentificationNo2, '' AS CompTestSubCode\r\n" +
                          $"     , (SELECT TestDisplayName FROM LabTestCode WHERE TestCode = match.CenterMatchCode) AS CenterTestName\r\n" +
                          $"     , (SELECT TestDisplayName FROM LabTestCode WHERE TestCode = match.CenterMatchOrderCode) AS CenterOrderName\r\n" +
                          $"FROM RsltTransCloudEMROrder cloudEMROrder\r\n" +
                          $"LEFT OUTER JOIN LabTransMatchCode AS match\r\n" +
                          $"ON match.CompCode = cloudEMROrder.CompCode\r\n" +
                          $"AND match.CompMatchCode = cloudEMROrder.uCode\r\n" +
                          $"LEFT OUTER JOIN LabTestCode AS testCode\r\n" +
                          $"ON match.CenterMatchCode = testCode.TestCode\r\n" +
                          $"WHERE cloudEMROrder.CompCode = '{request["CompCode"].ToString()}'\r\n" +
                          $"AND cloudEMROrder.OrderDateTime BETWEEN '{Convert.ToDateTime(request["BeginDate"]).ToString("yyyyMMdd")}' AND DATEADD(DAY, 1, '{Convert.ToDateTime(request["EndDate"]).ToString("yyyyMMdd")}')\r\n" +
                          $"AND NOT EXISTS\r\n" +
                          $"    (\r\n" +
                          $"    SELECT NULL\r\n" +
                          $"    FROM LabTransCompOrderInfo orderInfo\r\n" +
                          $"    JOIN LabRegInfo regInfo\r\n" +
                          $"    ON orderInfo.LabRegDate = regInfo.LabRegDate\r\n" +
                          $"    AND orderInfo.LabRegNo = regInfo.LabRegNo\r\n" +
                          $"    WHERE orderInfo.CompCode = cloudEMROrder.CompCode\r\n" +
                          $"    AND orderinfo.CompOrderDate = CONVERT(DATE, cloudEMROrder.OrderDateTime)\r\n" +
                          $"    AND orderinfo.CompOrderNo = cloudEMROrder.ReqID\r\n" +
                          $"    AND orderInfo.CompTestCode = cloudEMROrder.uCode\r\n" +
                          $"    AND regInfo.PatientChartNo = cloudEMROrder.PatientNo\r\n" +
                          $"    )\r\n" +
                          $"ORDER BY PatientNo";
                }
                else //등록
                {
                    sql = $"SELECT CONVERT(bit, 0) AS ColumnCheck, transInfo.LabRegDate, transInfo.LabRegNo, regInfo.PatientName, regInfo.PatientChartNo\r\n" +
                          $"     , regInfo.PatientSex, regInfo.PatientAge, transInfo.CompOrderDate, transinfo.CompOrderNo, transInfo.CompTestCode, transInfo.CompTestName\r\n" +
                          $"     , regInfo.CompDeptCode, regInfo.PatientSickRoom, regInfo.PatientDoctorName, transInfo.TestCode, transInfo.OrderCode\r\n" +
                          $"     , transInfo.CompSpcNo AS SampleNo\r\n" +
                          $"FROM LabTransCompOrderInfo AS transInfo\r\n" +
                          $"LEFT OUTER JOIN LabRegInfo regInfo\r\n" +
                          $"ON regInfo.LabRegDate = transInfo.LabRegDate\r\n" +
                          $"AND regInfo.LabRegNo = transInfo.LabRegNo\r\n" +
                          $"WHERE transInfo.CompCode = '{request["CompCode"].ToString()}'\r\n" +
                          $"AND transInfo.LabRegDate BETWEEN '{Convert.ToDateTime(request["BeginDate"]).ToString("yyyyMMdd")}' AND '{Convert.ToDateTime(request["EndDate"]).ToString("yyyyMMdd")}'\r\n" +
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