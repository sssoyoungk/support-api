using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;

namespace supportsapi.labgenomics.com.Services
{
    public class RegistOrder
    {
        public void UpdateLabTransCompOrderInfo(DateTime labRegDate, int labRegNo, string testCode, string testSubCode, string stateCode, string editID = "")
        {
            int isTestTransEnd = (stateCode == "Y") ? 1 : 0;
            string sql;
            sql = $"UPDATE LabTransCompOrderInfo\r\n" +
                  $"SET ResultSendState = '{stateCode}'\r\n" +
                  $"  , ResultSendTime = GETDATE()\r\n" +
                  $"  , EditTime = GETDATE()\r\n" +
                  $"  , EditID = '{editID}'\r\n" +
                  $"WHERE LabRegDate = '{labRegDate.ToString("yyyy-MM-dd")}'\r\n" +
                  $"AND LabRegNo = {labRegNo}\r\n" +
                  $"AND TestCode = '{testSubCode}'\r\n" +
                  $"\r\n" +
                  $"UPDATE LabRegTest\r\n" +
                  $"SET IsTestTransEnd = {isTestTransEnd}\r\n" +
                  $"  , EditTime = GETDATE()\r\n" +
                  $"  , EditorMemberID = '{editID}'\r\n" +
                  $"WHERE LabRegDate = '{labRegDate.ToString("yyyy-MM-dd")}'\r\n" +
                  $"AND LabRegNo = {labRegNo}\r\n" +
                  $"AND TestCode = '{testCode}'";

            LabgeDatabase.ExecuteSql(sql);
        }

        /// <summary>
        /// 부속거래처 등록
        /// </summary>
        /// <param name="compCode"></param>
        /// <param name="compSubCode"></param>
        /// <param name="compSubName"></param>
        public void SetCompSubCode(string compCode, string compSubCode, string compSubName, string registID)
        {
            string sql = $"MERGE INTO ProgCompSubCode AS target \r\n" +
                         $"USING (SELECT '{compCode}' AS CompCode, '{compSubCode}' AS CompSubCode) AS source \r\n" +
                         $"ON (target.CompCode = source.CompCode AND target.CompSubCode = source.CompSubCode) \r\n" +
                         $"WHEN NOT MATCHED THEN \r\n" +
                         $"    INSERT (CompCode, CompSubCode, IsCompUseSubCode, CompSubName, EditTime, EditorMemberID) \r\n" +
                         $"    VALUES ('{compCode}', '{compSubCode}', 1, '{compSubName}', GETDATE(), '{registID}') \r\n" +
                         $"OUTPUT $action;";

            LabgeDatabase.ExecuteSql(sql);
        }

        /// <summary>
        /// LabRegInfo 등록
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public void RegistLabRegInfo(JObject request)
        {
            string sql;
            sql = "MERGE INTO LabregInfo AS target\r\n" +
                  "USING (SELECT @LabRegDate AS LabRegDate, @LabRegNo AS LabRegNo ) AS source\r\n" +
                  "ON (target.LabRegDate = source.LabRegDate and target.LabRegNo = source.LabRegNo)\r\n" +
                  "WHEN NOT MATCHED BY TARGET THEN\r\n" +
                  "    INSERT ( LabRegDate, LabRegNo, CompCode, CompSubCode\r\n" +
                  "           , CompSubName, CompDeptCode, CompDeptName, IsPatientEmergency\r\n" +
                  "           , PatientName, PatientAge, PatientSex, PatientJuminNo01\r\n" +
                  "           , PatientChartNo, PatientDoctorName, PatientSampleGetTime, EditTime, EditorMemberID\r\n" +
                  "           , IsPatientJonggumFinish, CenterCode, IsJonggumReport, IsTrustOrder, RegistTime\r\n" +
                  "           , RegistMemberID, IsJonggumPrintEnd, PatientJuminNo02, PatientSickRoom\r\n" +
                  "           , PatientZipCode, PatientAddress01, PatientPhoneNo\r\n" +
                  "           , PatientImportCustomData01, PatientImportCustomData02, PatientImportCustomData03, SystemUniqID, PatientRemark)\r\n" +
                  "    VALUES ( source.LabRegDate, source.LabRegNo, @CompCode, @CompSubCode\r\n" +
                  "           , @CompSubName, @CompDeptCode, @CompDeptName, '0'\r\n" +
                  "           , @PatientName, @PatientAge, @PatientSex, @PatientJuminNo01\r\n" +
                  "           , @PatientChartNo, @PatientDoctorName, @PatientSampleGetTime, GETDATE(), @RegistMemberID\r\n" +
                  "           , '0', @ChartKind, '0', '1', GETDATE()\r\n" +
                  "           , @RegistMemberID, '0', master.dbo.AES_EncryptFunc(@PatientJuminNo02, 'labge$%#!dleorms'), @PatientSickRoom\r\n" +
                  "           , @PatientZipCode, @PatientAddress01, @PatientPhoneNo\r\n" +
                  "           , @PatientImportCustomData01, @PatientImportCustomData02, @PatientImportCustomData03, @SystemUniqID, @PatientRemark);";

            SqlCommand cmd = new SqlCommand(sql);

            cmd.Parameters.AddWithValue("@LabRegDate", Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@LabRegNo", Convert.ToInt32(request["LabRegNo"]));
            cmd.Parameters.AddWithValue("@CompCode", request["CompCode"].ToString());
            cmd.Parameters.AddWithValue("@CompSubCode", request["CompSubCode"].ToString());
            cmd.Parameters.AddWithValue("@CompSubName", request["CompSubName"].ToString());
            //cmd.Parameters.AddWithValue("@CompDeptCode", request["CompDeptCode"] == null ? string.Empty : request["CompDeptCode"].ToString());
            cmd.Parameters.AddWithValue("@CompDeptCode", request["CompDeptName"] == null ? string.Empty : request["CompDeptName"].ToString());
            cmd.Parameters.AddWithValue("@CompDeptName", request["CompDeptName"] == null ? string.Empty : request["CompDeptName"].ToString());
            cmd.Parameters.AddWithValue("@PatientName", request["PatientName"].ToString());
            cmd.Parameters.AddWithValue("@PatientAge", request["PatientAge"].ToString());
            cmd.Parameters.AddWithValue("@PatientSex", request["PatientSex"].ToString());
            cmd.Parameters.AddWithValue("@PatientJuminNo01", request["PatientJuminNo01"] == null ? string.Empty : request["PatientJuminNo01"].ToString());
            cmd.Parameters.AddWithValue("@PatientChartNo", request["PatientChartNo"] == null ? string.Empty : request["PatientChartNo"].ToString());
            cmd.Parameters.AddWithValue("@PatientDoctorName", request["PatientDoctorName"] == null ? string.Empty : request["PatientDoctorName"].ToString());
            if (request["PatientSampleGetTime"].ToString() != string.Empty)
                cmd.Parameters.AddWithValue("@PatientSampleGetTime", Convert.ToDateTime(request["PatientSampleGetTime"]).ToString("yyyy-MM-dd HH:mm:ss"));
            else
                cmd.Parameters.AddWithValue("@PatientSampleGetTime", Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ChartKind", request["ChartKind"].ToString());
            cmd.Parameters.AddWithValue("@PatientJuminNo02", request["PatientJuminNo02"] == null ? string.Empty : request["PatientJuminNo02"].ToString());
            cmd.Parameters.AddWithValue("@RegistMemberID", request["RegistMemberID"].ToString());
            cmd.Parameters.AddWithValue("@PatientSickRoom", request["CompWard"] == null ? string.Empty : request["CompWard"].ToString());
            cmd.Parameters.AddWithValue("@PatientZipCode", request["ZipCode"] == null ? string.Empty : request["ZipCode"].ToString());
            cmd.Parameters.AddWithValue("@PatientAddress01", request["Address"] == null ? string.Empty : request["Address"].ToString());
            cmd.Parameters.AddWithValue("@PatientPhoneNo", request["PhoneNo"] == null ? string.Empty : request["PhoneNo"].ToString());
            cmd.Parameters.AddWithValue("@PatientImportCustomData01", request["PatientImportCustomData01"] == null ? string.Empty : request["PatientImportCustomData01"].ToString());
            cmd.Parameters.AddWithValue("@PatientImportCustomData02", request["PatientImportCustomData02"] == null ? string.Empty : request["PatientImportCustomData02"].ToString());
            cmd.Parameters.AddWithValue("@PatientImportCustomData03", request["PatientImportCustomData03"] == null ? string.Empty : request["PatientImportCustomData03"].ToString());
            cmd.Parameters.AddWithValue("@SystemUniqID", request["SystemUniqID"] == null ? string.Empty : request["SystemUniqID"].ToString());
            cmd.Parameters.AddWithValue("@PatientRemark", request["PatientRemark"] == null ? string.Empty : request["PatientRemark"].ToString());


            LabgeDatabase.ExecuteSqlCommand(cmd);
        }

        /// <summary>
        /// LabRegOrder 등록
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public void RegistLabRegOrder(JObject request)
        {
            string sql;
            sql = $"MERGE INTO LabRegOrder AS A\r\n" +
                  $"USING\r\n" +
                  $"(\r\n" +
                  $"    SELECT '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}' AS LabRegDate\r\n" +
                  $"         , {Convert.ToInt32(request["LabRegNo"])} AS LabRegNo, '{request["OrderCode"].ToString()}' AS OrderCode\r\n" +
                  $"         , '{request["OrderCode"].ToString()}' AS DemandCode, '{request["SampleCode"].ToString()}' AS SampleCode\r\n" +
                  $"         , '0' AS IsOrderCheck, F.OrderDefaultPrice, '0' AS IsCompdemand, '0' AS IsCompdemandPriceChange\r\n" +
                  $"         , dbo.FN_GetOrderPrice_LabG(CompCode, '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}', '{request["OrderCode"].ToString()}') AS CompdemandPrice\r\n" +
                  $"         , '0' AS IsOfficedemand, '0' AS IsOfficedemandPriceChange\r\n" +
                  $"         , '0.00' AS OfficedemandPrice, GetDate() AS RegistTime, '{request["RegistMemberID"].ToString()}' AS RegistMemberID, '0' AS IsTrustOrder, F.OrderCost\r\n" +
                  $"         , F.InsurePrice, F.AdditivePrice\r\n" +
                  $"    FROM\r\n" +
                  $"    (\r\n" +
                  $"        SELECT D.CompCode, C.OrderCode, C.OrderDisplayName, C.OrderPriceBeginDate, C.OrderPriceEndDate, C.OrderPriceGroupCode\r\n" +
                  $"             , C.OrderDefaultPrice, C.AdditivePrice, C.InsurePrice, C.OrderLimitPrice, C.OrderCost\r\n" +
                  $"             , D.IsUseLimitPrice, D.IsUseAdditivePrice AS IsUseAdditiveDiscount, D.CompPriceDiscountRate\r\n" +
                  $"             , C.OrderDefaultPrice * (100.0 - D.CompPriceDiscountRate) / 100 AS DisCountPrice\r\n" +
                  $"             , E.IsCompPriceExtraContractPrice, E.IsUseAdditivePrice AS IsUseAdditiveExtra, E.CompPriceExtraContractPrice, E.CompPriceExtraDiscountRate\r\n" +
                  $"             , CASE E.IsCompPriceExtraContractPrice\r\n" +
                  $"                   WHEN '1' THEN E.CompPriceExtraContractPrice\r\n" +
                  $"                   ELSE C.OrderDefaultPrice * (100.0 - E.CompPriceExtraDiscountRate) / 100\r\n" +
                  $"               END AS ExtraPrice\r\n" +
                  $"        FROM (SELECT A.OrderCode, A.OrderDisplayName, B.OrderPriceBeginDate, B.OrderPriceEndDate, B.OrderPriceGroupCode\r\n" +
                  $"                   , B.OrderDefaultPrice, B.AdditivePrice, B.InsurePrice, B.OrderLimitPrice, B.OrderCost\r\n" +
                  $"              FROM LabOrderCode AS A\r\n" +
                  $"              JOIN LabOrderPrice AS B\r\n" +
                  $"              ON A.OrderCode = B.OrderCode) AS C\r\n" +
                  $"        JOIN ProgCompPriceDiscount AS D\r\n" +
                  $"        ON C.OrderPriceGroupCode = D.OrderPriceGroupCode\r\n" +
                  $"        LEFT OUTER JOIN ProgCompPriceExtra AS E\r\n" +
                  $"        ON D.CompCode = E.CompCode\r\n" +
                  $"        AND C.OrderCode = E.OrderCode\r\n" +
                  $"        AND E.CompPriceExtraBeginDate <= '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                  $"        AND E.CompPriceExtraEndDate >= '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                  $"        WHERE C.OrderCode = '{request["OrderCode"].ToString()}'\r\n" +
                  $"        AND D.CompCode = '{request["CompCode"].ToString()}'\r\n" +
                  $"        AND '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}' BETWEEN C.OrderPriceBeginDate AND C.OrderPriceEndDate\r\n" +
                  $"        AND '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}' BETWEEN d.CompPriceGroupBeginDate AND d.CompPriceGroupEndDate\r\n" +
                  $"    ) AS F\r\n" +
                  $") AS B\r\n" +
                  $"ON (a.LabRegDate = B.LabRegdate AND a.LabRegNo = B.LabRegNo AND a.OrderCode = B.OrderCode )\r\n" +
                  $"WHEN NOT MATCHED THEN\r\n" +
                  $"    INSERT\r\n" +
                  $"    (\r\n" +
                  $"         LabRegDate, LabRegNo, OrderCode, DemandCode, SampleCode, IsOrderCheck, OrderDefaultPrice\r\n" +
                  $"       , IsCompdemand, IsCompdemandPriceChange, CompdemandPrice, IsOfficedemand, IsOfficedemandPriceChange\r\n" +
                  $"       , OfficedemandPrice, RegistTime, RegistMemberID, EditTime, EditorMemberID, IsTrustOrder, OrderCost, OrderInsurePrice, AdditivePrice\r\n" +
                  $"    )\r\n" +
                  $"    VALUES\r\n" +
                  $"    (\r\n" +
                  $"        B.LabRegDate, B.LabRegNo, B.OrderCode, B.DemandCode, B.SampleCode, B.IsOrderCheck, B.OrderDefaultPrice\r\n" +
                  $"      , B.IsCompdemand, B.IsCompdemandPriceChange, B.CompdemandPrice, B.IsOfficedemand, B.IsOfficedemandPriceChange\r\n" +
                  $"      , B.OfficedemandPrice, B.RegistTime, B.RegistMemberID, B.RegistTime, B.RegistMemberID, B.IsTrustOrder, B.OrderCost, B.InsurePrice, B.AdditivePrice\r\n" +
                  $"    )\r\n" +
                  $";";

            LabgeDatabase.ExecuteSql(sql);
        }

        /// <summary>
        /// LabRegOrderTest 등록
        /// </summary>
        /// <param name="paramData"></param>
        /// <returns></returns>
        public void RegistLabRegOrderTest(JObject request)
        {
            string sql;
            //접수된 항목이 프로파일인지 확인.
            sql = $"SELECT ProfileCode, TestCode, SampleCode\r\n" +
                  $"FROM LabProfileTest\r\n" +
                  $"WHERE ProfileCode = '{request["TestCode"].ToString()}'";
            JArray arrProfile = LabgeDatabase.SqlToJArray(sql);

            //프로파일이면
            if (arrProfile.Count > 0)
            {
                foreach (JObject objProfile in arrProfile)
                {
                    sql = $"MERGE INTO LabRegOrderTest AS target\r\n" +
                          $"USING\r\n" +
                          $"( SELECT '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}' AS LabRegDate, {request["LabRegNo"].ToString()} AS LabRegNo\r\n" +
                          $"       , '{request["OrderCode"].ToString()}' AS OrderCode, '{objProfile["TestCode"].ToString()}' AS TestCode, '{objProfile["SampleCode"].ToString()}' AS SampleCode\r\n" +
                          $") AS source\r\n" +
                          $"ON (target.LabRegDate = source.LabRegDate AND target.LabRegNo = source.LabRegNo AND\r\n" +
                          $"    target.OrderCode = source.OrderCode AND target.TestCode = source.TestCode)\r\n" +
                          $"WHEN NOT MATCHED BY TARGET THEN\r\n" +
                          $"INSERT (LabRegDate, LabRegNo, OrderCode, TestCode, SampleCode)\r\n" +
                          $"VALUES (source.LabRegDate, source.LabRegNo, source.OrderCode, source.TestCode, source.SampleCode);\r\n";
                    LabgeDatabase.ExecuteSql(sql);
                }
            }
            else
            {
                //프로파일이 아니면
                sql = $"MERGE INTO LabRegOrderTest AS target\r\n" +
                      $"USING\r\n" +
                      $"( SELECT '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}' AS LabRegDate, {request["LabRegNo"].ToString()} AS LabRegNo\r\n" +
                      $"       , '{request["OrderCode"].ToString()}' AS OrderCode, '{request["TestCode"].ToString()}' AS TestCode, '{request["SampleCode"].ToString()}' AS SampleCode\r\n" +
                      $") AS source\r\n" +
                      $"ON (target.LabRegDate = source.LabRegDate and target.LabRegNo = source.LabRegNo and target.OrderCode = source.OrderCode)\r\n" +
                      $"WHEN NOT MATCHED BY TARGET THEN\r\n" +
                      $"INSERT (LabRegDate, LabRegNo, OrderCode, TestCode, SampleCode)\r\n" +
                      $"VALUES (source.LabRegDate, source.LabRegNo, source.OrderCode, source.TestCode, source.SampleCode);\r\n";

                LabgeDatabase.ExecuteSql(sql);
            }
        }

        /// <summary>
        /// LabRegSample 코드 등록
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public void RegistLabRegSample(JObject request)
        {
            string sql;
            sql = $"MERGE LabRegSample AS a\r\n" +
                  $"USING\r\n" +
                  $"(\r\n" +
                  $"    SELECT '{Convert.ToDateTime(request["LabRegDate"]).ToString("yyyy-MM-dd")}' AS LabRegDate" +
                  $"         , {Convert.ToInt32(request["LabRegNo"])} AS LabRegNo, '{request["SampleCode"].ToString()}' AS SampleCode, 'S10' AS SampleStateCode\r\n" +
                  $"         , GetDate() AS SampleGetTime, '{request["RegistMemberID"].ToString()}' AS EditorMemberID, '0001-01-01' AS SampleReceivedDate\r\n" +
                  $") AS b\r\n" +
                  $"ON (a.LabRegDate = b.LabRegdate AND a.LabRegNo = b.LabRegNo AND a.SampleCode = b.SampleCode)\r\n" +
                  $"WHEN NOT MATCHED BY TARGET THEN\r\n" +
                  $"INSERT (LabRegDate, LabRegNo, SampleCode, SampleStateCode, SampleGetTime, EditTime, EditorMemberID, SampleReceivedDate)\r\n" +
                  $"VALUES (b.LabRegDate, b.LabRegNo, b.SampleCode, b.SampleStateCode, b.SampleGetTime, b.SampleGetTime, b.EditorMemberID, b.SampleReceivedDate); ";
            LabgeDatabase.ExecuteSql(sql);
        }

        /// <summary>
        /// 중복코드 색상 처리를 위한 부분
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public DataTable AddColumnDuplicationCodeCheck(DataTable dt)
        {
            dt.Columns.Add("DuplicationCodeCheck", typeof(bool));
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    if (dt.Rows[i]["PatientChartNo"].ToString() == dt.Rows[j]["PatientChartNo"].ToString() &&
                        //dt.Rows[i]["CompOrderNo"].ToString() == dt.Rows[j]["CompOrderNo"].ToString() &&
                        dt.Rows[i]["CompOrderDate"].ToString() == dt.Rows[j]["CompOrderDate"].ToString() &&
                        dt.Rows[i]["TestCode"].ToString() == dt.Rows[j]["TestCode"].ToString() &&
                        dt.Rows[i]["TestCode"].ToString() != string.Empty)
                    {
                        dt.Rows[i]["DuplicationCodeCheck"] = true;
                    }
                }
            }
            return dt;
        }
    }
}