using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;

namespace supportsapi.labgenomics.com.Controllers.Sales.CompRegistOrder
{
    public class ExcelOrder
    {
        public JArray GetOrder(JObject request)
        {
            string sql = string.Empty;

            sql = $"SELECT ImportDataID, ColumnCheck, LabRegDate, LabRegNo, PatientName, PatientChartNo\r\n" +
                  $"     , CASE WHEN PatientAge = 0 THEN dbo.FN_GetInstitutioNoToAge(IdentificationNo1 + IdentificationNo2, RegistTime) ELSE PatientAge END PatientAge     \r\n" +
                  $"     , CASE WHEN ISNULL(PatientSex, '') = '' THEN\r\n" +
                  $"           CASE WHEN SUBSTRING(IdentificationNo2, 1, 1) IN('1', '3', '5', '7') THEN 'M'\r\n" +
                  $"                WHEN SUBSTRING(IdentificationNo2, 1, 1) IN('2', '4', '6', '8') THEN 'F'\r\n" +
                  $"           END\r\n" +
                  $"       ELSE PatientSex END AS PatientSex\r\n" +
                  $"     , DeptCode, SampleNo, CompOrderDate, IsCenterRegist, IdentificationNo1, IdentificationNo2\r\n" +
                  $"     , DoctorName, SampleDrawDate, CompTestCode, CompTestName, OrderCode\r\n" +
                  $"     , TestCode, SampleCode, Dept, Ward, PatientRemark\r\n" +
                  $"FROM\r\n" +
                  $"(\r\n" +
                  $"    SELECT ImportDataID\r\n" +
                  $"         , CONVERT(bit,0) AS ColumnCheck, CASE WHEN ISNULL(lid.LabRegDate, '') = '' THEN GETDATE() ELSE lid.LabRegDate END AS LabRegDate\r\n" +
                  $"         , lid.LabRegNo, lid.PatientName, lid.PatientChartNo, lid.PatientSex, lid.PatientAge, lid.CompDeptCode AS DeptCode\r\n" +
                  $"         , lid.CompSystemID AS SampleNo, lid.PatientDate AS CompOrderDate, IsCenterRegist\r\n" +
                  $"         , SUBSTRING(dbo.FN_ConvertJuminNo(lid.PatientJuminNo01, lid.PatientJuminNo02, lid.PatientJuminNo03, lid.PatientJuminNo04, lid.PatientJuminNo05), 1, 6) AS IdentificationNo1\r\n" +
                  $"         , SUBSTRING(dbo.FN_ConvertJuminNo(lid.PatientJuminNo01, lid.PatientJuminNo02, lid.PatientJuminNo03, lid.PatientJuminNo04, lid.PatientJuminNo05), 7, 7) AS IdentificationNo2\r\n" +
                  $"         , lid.PatientDoctorName AS DoctorName, lid.PatientSampleGetTime AS SampleDrawDate, lid.CompOrderCode AS CompTestCode\r\n" +
                  $"         , CompOrderDisplayName AS CompTestName\r\n" +
                  $"         , CASE WHEN ISNULL(lid.CenterOrderCode, '') = '' THEN ltmc.CenterMatchCode ELSE lid.CenterOrderCode END TestCode\r\n" +
                  $"         , CASE WHEN ISNULL(lid.CenterOrderCode2, '') = '' THEN ltmc.CenterMatchOrderCode ELSE lid.CenterOrderCode2 END OrderCode\r\n" +
                  $"         , CASE WHEN ISNULL(lid.CenterSampleCode, '') <> '' THEN lid.CenterSampleCode\r\n" +
                  $"                WHEN ISNULL(lid.CenterSampleCode, '') = '' AND ISNULL(ltmc.CenterMatchSampleCode, '') <> '' THEN ltmc.CenterMatchSampleCode\r\n" +
                  $"                WHEN ISNULL(lid.CenterSampleCode, '') = '' AND ISNULL(ltmc.CenterMatchSampleCode, '') = '' THEN\r\n" +
                  $"                     (SELECT SampleCode FROM LabTestCode WHERE TestCode = ltmc.CenterMatchCode)\r\n" +
                  $"           END AS SampleCode\r\n" +
                  $"         , lid.CompDeptName AS Dept, lid.PatientSickRoom AS Ward, lid.PatientRemark\r\n" +
                  //$"         , CASE WHEN lid.CompDeptName <> '' THEN lid.CompDeptName ELSE lid.PatientSickRoom END AS Dept\r\n" +
                  $"         , lid.RegistTime\r\n" +
                  $"    FROM LabImportData lid\r\n" +
                  $"    LEFT OUTER JOIN LabTransMatchCode ltmc\r\n" +
                  $"    ON ltmc.CompCode = lid.CompCode\r\n" +
                  $"    AND ISNULL(ltmc.CompMatchCode, '') = ISNULL(lid.CompOrderCode, '')\r\n" +
                  $"    WHERE lid.ImportFileID = '{request["ImportFileID"].ToString()}'\r\n" +
                  $"    AND lid.CompCode = '{request["CompCode"].ToString()}'\r\n";
            if (request["RegKind"].ToString() == "W") //등록대기
            {
                sql += $"    AND lid.LabRegNo IS NULL\r\n";
            }
            else
            {
                sql += $"    AND lid.LabRegNo IS NOT NULL\r\n";
            }

            sql += $") AS Sub1\r\n";

            //정렬문제로 엑셀파일을 LabImportData에 저장할때 sleep을 줬음.
            sql += $"ORDER BY RegistTime";


            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return arrResponse;
        }
    }
}