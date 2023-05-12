using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    public class SensiStatController : ApiController
    {



        [Route("api/Diagnostic/SensiStat/CompName")]
        public IHttpActionResult GetCompName(string compCode)
        {
            string sql;
            sql = "select CompName from ProgCompCode where CompCode = '" + compCode + "'";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


        [Route("api/Diagnostic/SensiStat/Gram")]
        public IHttpActionResult GetGram(DateTime beginDate, DateTime endDate)
        {
            string sql;
            sql = "select r.GramDisplayName as 균종, COUNT(r.GramDisplayName) as 건수, round(convert(float, (COUNT(r.GramDisplayName)) / convert(float, SUM(count(r.GramDisplayName)) over())) * 100, 2) as 비율\n"
                     + "from (select distinct a.LabRegDate, a.LabRegNo, c.TestCode, b.TestSubCode, c.SampleCode, d.SampleName, e.GramCode, f.GramDisplayName\n"
                     + "from LabRegInfo as a\n"
                     + "inner join LabRegResult as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                     + "inner join LabRegTest as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo and b.TestCode = c.TestCode\n"
                     + "inner join LabSampleCode as d on c.SampleCode = d.SampleCode\n"
                     + "inner join LabGramResult as e on a.LabRegDate = e.LabRegDate and a.LabRegNo = e.LabRegNo and b.TestSubCode = e.TestSubCode\n"
                     + "inner join LabGramCode as f on e.GramCode = f.GramCode\n"
                     + "where a.LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'\n"
                     + "and a.CompCode = '6496'\n"
                     + "and b.TestSubCode in ('21008', '21036', '2113102', '2113103', '2117102')) as r\n"
                     + "group by r.GramDisplayName\n"
                     + "order by r.GramDisplayName";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


        [Route("api/Diagnostic/SensiStat/Sample")]
        public IHttpActionResult GetSample(DateTime beginDate, DateTime endDate, string compCode)
        {
            string sql;
            sql = "select r.SampleName as 검체명, r.GramDisplayName as 균종, COUNT(r.GramDisplayName) as 건수\n"
                     + "from (select distinct a.LabRegDate, a.LabRegNo, c.TestCode, b.TestSubCode, c.SampleCode, d.SampleName, e.GramCode, f.GramDisplayName\n"
                     + "from LabRegInfo as a\n"
                     + "inner join LabRegResult as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                     + "inner join LabRegTest as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo and b.TestCode = c.TestCode\n"
                     + "inner join LabSampleCode as d on c.SampleCode = d.SampleCode\n"
                     + "inner join LabGramResult as e on a.LabRegDate = e.LabRegDate and a.LabRegNo = e.LabRegNo and b.TestSubCode = e.TestSubCode\n"
                     + "inner join LabGramCode as f on e.GramCode = f.GramCode\n"
                     + "where a.LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'\n"
                     + "and a.CompCode = '" + compCode + "'\n"
                     + "and b.TestSubCode in ('21008', '21036', '2113102', '2113103', '2117102')) as r\n"
                     + "group by r.SampleName, r.GramDisplayName\n"
                     + "order by r.SampleName";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


    }
}