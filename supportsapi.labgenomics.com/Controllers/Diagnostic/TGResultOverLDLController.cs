using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    public class TGResultOverLDLController : ApiController
    {



        [Route("api/Diagnostic/TGResultOverLDL")]
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate)
        {
            string sql;

            sql = "select r.CompGroup01 as 본부, r.CompGroup02 as 영업소, r.LabRegDate as 접수일, r.LabRegNo as 접수번호, r.CompCode as 거래처코드, r.CompName as 거래처명\r\n"
                                + ", r.TestCode as 검사코드, r.TestDisplayName as 검사명, CONVERT(int, r.TestResult01) as 결과\r\n"
                                + ", t.TestDisplayName as LDL\r\n"
                                + "from \r\n"
                                + "(select e.CompGroup01, e.CompGroup02, a.LabRegDate, a.LabRegNo, a.CompCode, d.CompName, b.TestCode, f.TestDisplayName, c.TestResult01, e.CompGroupSeqNo\r\n"
                                + "from LabRegInfo as a inner join LabRegTest as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\r\n"
                                + "inner join LabRegResult as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo and b.OrderCode = c.OrderCode and b.TestCode = c.TestCode\r\n"
                                + "inner join ProgCompCode as d on a.CompCode = d.CompCode\r\n"
                                + "inner join ProgCompGroupCode as e on d.CompGroupCode = e.CompGroupCode\r\n"
                                + "inner join LabTestCode as f on c.TestCode = f.TestCode\r\n"
                                + "where a.LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'\r\n"
                                + "and b.TestCode in ('33005', '33705')\r\n"
                                + "and b.TestStateCode = 'F') as r \r\n"
                                + "left outer join LabRegTest as l on r.LabRegDate = l.LabRegDate and r.LabRegNo = l.LabRegNo and l.TestCode in ('33006', '11066', '33706')\r\n"
                                + "left outer join LabTestCode as t on l.TestCode = t.TestCode\r\n"
                                //+ "WHERE CONVERT(float, TestResult01) > 400\r\n"
                                + "order by r.CompGroupSeqNo, r.CompCode, r.LabRegDate, r.LabRegNo";
            
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);

        }
    }
}