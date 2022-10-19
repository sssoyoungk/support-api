﻿using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [SupportsAuth]
    [Route("api/Sales/SungmoPathOrder")]
    public class SungmoPathOrderController : ApiController
    {
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string isTestOutside, int beginNo, int endNo, string compMngCode)
        {
            string sql;
            sql = $"SELECT A.LabRegDate, A.LabRegNo, A.CompCode\r\n" +
                  $"     , (SELECT CompName FROM ProgCompCode WHERE A.CompCode = CompCode) AS CompName\r\n" +
                  $"     , A.PatientName, A.PatientAge, A.PatientSex, A.PatientJuminNo01, A.PatientChartNo\r\n" +
                  $"     , B.OrderCode, B.TestCode\r\n" +
                  $"     , (SELECT TestDisplayName FROM LabTestCode WHERE B.TestCode = TestCode) AS TestDisplayName\r\n" +
                  $"     , B.SampleCode\r\n" +
                  $"     , (SELECT SampleName FROM LabSampleCode WHERE B.SampleCode = SampleCode) AS SampleName\r\n" +
                  $"     , B.IsTestOutside, B.TestOutsideBeginTime, B.TestOutsideEndTime, B.TestOutsideCompCode, B.TestOutsideMemberID\r\n" +
                  $"FROM LabRegInfo A\r\n" +
                  $"JOIN LabRegTest B\r\n" +
                  $"ON A.LabRegDate = B.LabRegDate\r\n" +
                  $"AND A.LabRegNo = B.LabRegNo\r\n" +
                  $"AND A.CompCode IN (SELECT CompCode FROM ProgAuthGroupCompList WHERE AuthGroupCode = 'c000119')\r\n" +
                  $"JOIN LabOutsideTestCode C\r\n" +
                  $"ON C.OutsideCompCode = '000119'\r\n" +
                  $"AND C.OutsideTestCode = B.TestCode\r\n" +
                  $"AND C.OutsideSampleCode = B.SampleCode\r\n" +
                  $"WHERE A.LabRegDate BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'\r\n" +
                  $"AND B.IsTestOutSide = {isTestOutside}\r\n" +
                  $"AND B.TestStateCode <> 'F'";

            if (isTestOutside == "1")
            {
                sql += "AND B.TestOutsideCompCode = '000119'\r\n";
            }
            else
            {
                sql += "AND ISNULL(B.TestOutsideCompCode, '') = ''\r\n";
            }

            sql += "ORDER BY A.LabRegDate, A.LabRegNo, B.OrderCode";

            return Ok(LabgeDatabase.SqlToJArray(sql));
        }

        public IHttpActionResult Post([FromBody]JArray arrRequest)
        {
            try
            {
                foreach (JObject objRequest in arrRequest)
                {
                    string sql;
                    sql =
                        $"UPDATE LabRegTest\r\n" +
                        $"SET TestOutsideCompCode = '000119'\r\n" +
                        $"WHERE LabRegDate = '{Convert.ToDateTime(objRequest["LabRegDate"]).ToString("yyyy-MM-dd")}'\r\n" +
                        $"AND LabRegNo = {objRequest["LabRegNo"]}\r\n" +
                        $"AND TestCode = '{objRequest["TestCode"].ToString()}'";
                    LabgeDatabase.ExecuteSql(sql);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }

        }
    }
}