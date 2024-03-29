﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace supportsapi.labgenomics.com.Controllers.Sales.CompRegistOrder
{
    public class AmorePacific
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
                    sql =
                        $"SELECT\r\n" +
                        $"    CONVERT(bit,0) AS ColumnCheck, GETDATE() AS LabRegDate, null AS LabRegNo, ppi.PatientName, ppi.Barcode AS PatientChartNo\r\n" +
                        $"  , ppi.CompOrderDate, ppi.CompOrderNo, ppi.Barcode, CONVERT(varchar, ppi.SampleDrawDate, 120) SampleDrawDate\r\n" +
                        $"  , CONVERT(CHAR(6), BirthDay, 12) AS IdentificationNo1, Gender AS PatientSex\r\n" +
                        $"  , FLOOR(CAST(DATEDIFF(DAY, ppi.BirthDay, ppi.CompOrderDate) AS INTEGER)/365.2422) AS PatientAge\r\n" +
                        $"  , ltmc.CenterMatchCode AS TestCode, ltmc.CenterMatchOrderCode AS OrderCode\r\n" +
                        $"  , pti.CompTestCode, null As CompTestSubCode, pti.CompTestName\r\n" +
                        $"  , CASE WHEN ISNULL(ltmc.CenterMatchSampleCode, '') <> '' THEN ltmc.CenterMatchSampleCode\r\n" +
                        $"         ELSE ltc.SampleCode END AS SampleCode\r\n" +
                        $"FROM PGSPatientInfo ppi\r\n" +
                        $"JOIN PGSTestInfo pti\r\n" +
                        $"ON ppi.CustomerCode = pti.CustomerCode\r\n" +
                        $"AND ppi.CompOrderDate = pti.CompOrderDate\r\n" +
                        $"AND ppi.CompOrderNo = pti.CompOrderNo\r\n" +
                        $"JOIN ProgCompCode pcc\r\n" +
                        $"ON ppi.CompCode = pcc.CompCode\r\n" +
                        $"LEFT OUTER JOIN LabTransMatchCode AS ltmc\r\n" +
                        $"ON ltmc.CompCode = pti.CompCode\r\n" +
                        $"AND ltmc.CompMatchCode = pti.CompTestCode\r\n" +
                        $"LEFT OUTER JOIN LabTestCode ltc\r\n" +
                        $"ON ltmc.CenterMatchCode = ltc.TestCode\r\n" +
                        $"WHERE ppi.CustomerCode = 'amorepacific'\r\n" +
                        $"AND ppi.CompOrderDate >= '{request["BeginDate"]}'\r\n" +
                        $"AND ppi.CompOrderDate < DATEADD(DAY, 1, '{request["EndDate"]}')\r\n" +
                        $"AND ppi.CompCode = '{request["CompCode"]}'\r\n" +
                        $"AND NOT EXISTS\r\n" +
                        $"(\r\n" +
                        $"    SELECT NULL FROM LabTransCompOrderInfo orderInfo\r\n" +
                        $"    WHERE orderInfo.CompCode = ppi.CompCode\r\n" +
                        $"    AND orderinfo.CompOrderDate = ppi.CompOrderDate\r\n" +
                        $"    AND orderinfo.CompOrderNo = ppi.CompOrderNo\r\n" +
                        $"    AND orderInfo.CompTestCode = pti.CompTestCode\r\n" +
                        $")\r\n" +
                        $"ORDER BY CompOrderDate, CompOrderNo";


                    //sql =
                    //    $"SELECT\r\n" +
                    //    $"    CONVERT(bit,0) AS ColumnCheck, GETDATE() AS LabRegDate, null AS LabRegNo, appi.PatientName, appi.Barcode AS PatientChartNo\r\n" +
                    //    $"  , appi.CompOrderDate, appi.CompOrderNo, appi.Barcode, CONVERT(varchar, appi.SampleDrawDate, 120) SampleDrawDate\r\n" +
                    //    $"  , CONVERT(CHAR(6), BirthDay, 12) AS IdentificationNo1, Gender AS PatientSex\r\n" +
                    //    $"  , FLOOR(CAST(DATEDIFF(DAY, appi.BirthDay, appi.CompOrderDate) AS INTEGER)/365.2422) AS PatientAge\r\n" +
                    //    $"  , ltmc.CenterMatchCode AS TestCode, ltmc.CenterMatchOrderCode AS OrderCode\r\n" +
                    //    $"  , appi.TestCode AS CompTestCode, null AS CompTestSubCode, appi.TestName\r\n" +
                    //    $"  , CASE WHEN ISNULL(ltmc.CenterMatchSampleCode, '') <> '' THEN ltmc.CenterMatchSampleCode\r\n" +
                    //    $"         ELSE ltc.SampleCode END AS SampleCode\r\n" +
                    //    $"FROM AmorePacificPatientInfo appi\r\n" +
                    //    $"JOIN ProgCompCode pcc\r\n" +
                    //    $"ON appi.CompCode = pcc.CompCode\r\n" +
                    //    $"LEFT OUTER JOIN LabTransMatchCode AS ltmc\r\n" +
                    //    $"ON ltmc.CompCode = appi.CompCode\r\n" +
                    //    $"AND ltmc.CompMatchCode = appi.TestCode\r\n" +
                    //    $"LEFT OUTER JOIN LabTestCode ltc\r\n" +
                    //    $"ON ltmc.CenterMatchCode = ltc.TestCode\r\n" +
                    //    $"WHERE appi.CompOrderDate >= '{request["BeginDate"]}'\r\n" +
                    //    $"AND appi.CompOrderDate < DATEADD(DAY, 1, '{request["EndDate"]}')\r\n" +
                    //    $"AND appi.CompCode = '{request["CompCode"]}'\r\n" +
                    //    $"AND NOT EXISTS\r\n" +
                    //    $"(\r\n" +
                    //    $"    SELECT NULL FROM LabTransCompOrderInfo orderInfo\r\n" +
                    //    $"    WHERE orderInfo.CompCode = appi.CompCode\r\n" +
                    //    $"    AND orderinfo.CompOrderDate = appi.CompOrderDate\r\n" +
                    //    $"    AND orderinfo.CompOrderNo = appi.CompOrderNo\r\n" +
                    //    $"    AND orderInfo.CompTestCode = appi.TestCode\r\n" +
                    //    $")\r\n" +
                    //    $"ORDER BY CompOrderDate, CompOrderNo\r\n";                    
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
                          $"WHERE transInfo.CompCode = '{request["CompCode"]}' \r\n" +
                          $"AND transInfo.LabRegDate BETWEEN '{request["BeginDate"]}' AND '{request["EndDate"]}'\r\n" +
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