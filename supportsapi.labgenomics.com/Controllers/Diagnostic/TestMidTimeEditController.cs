using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    public class TestMidTimeEditController : ApiController
    {



        [Route("api/Diagnostic/TestMidTimeEdit")]
        public IHttpActionResult Get(DateTime labRegDate, string labRegNo)
        {
            string sql;
            sql = "select a.LabRegDate as 접수일, a.LabRegNo as 접수번호, b.CompCode as 거래처코드, c.CompName as 거래처명, b.PatientName as 수진자명\n"
                         + ", b.PatientChartNo as 차트번호, a.TestCode as 검사코드, d.TestDisplayName as 검사명, convert(varchar(22), a.TestStartTime, 21) as 시작일시, convert(varchar(22), a.TestMidTime, 21) as 중간보고, convert(varchar(22), a.TestEndTime, 21) as 종료일시\n"
                         + "from LabRegTest as a inner join LabRegInfo as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                         + "inner join ProgCompCode as c on b.CompCode = c.CompCode\n"
                         + "inner join LabTestCode as d on a.TestCode = d.TestCode\n"
                         + "where a.LabRegDate = '" + labRegDate.ToString("yyyy-MM-dd") + "'\n"
                      + "and a.LabRegNo = '" + labRegNo + "'\n"
                      + "and a.TestCode in ('21201', '21202', '21009')";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);

        }


        [Route("api/Diagnostic/TestMidTimeEdit")]
        public IHttpActionResult Put([FromBody]JObject request)
        {
            try
            {
                string sql;
                sql = "update LabRegTest\n"
                       + "set TestMidTime = '" + request["TestMidTime"].ToString() + "'\n"
                       + "from LabRegTest as a inner join LabRegInfo as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                       + "inner join ProgCompCode as c on b.CompCode = c.CompCode\n"
                       + "inner join LabTestCode as d on a.TestCode = d.TestCode\n"
                       + "where a.LabRegDate = '" + request["LabRegDate"].ToString() + "'\n"
                          + "and a.LabRegNo = '" + request["LabRegNo"].ToString() + "'\n"
                          + "and a.TestCode in ('21201', '21202', '21009')";

                LabgeDatabase.ExecuteSql(sql);
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