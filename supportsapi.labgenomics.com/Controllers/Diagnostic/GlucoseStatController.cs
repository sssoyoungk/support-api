using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    public class GlucoseStatController : ApiController
    {

        [Route("api/Diagnostic/GlucoseStat")]
        public IHttpActionResult Get(string year)
        {
            string sql = "select f.CompGroup03 as 영업소, f.CompCode as 거래처코드, f.CompName as 거래처명, g.구분 as '" + year + "년' "
                     + ", max(case when f.RegMonth = '1' then case g.구분 when '건수' then isnull(f.건수, 0) when '평균' then isnull(f.평균, 0) when '표준편차' then isnull(f.표준편차, 0) end else 0 end) as '1월' "
                     + ", max(case when f.RegMonth = '2' then case g.구분 when '건수' then isnull(f.건수, 0) when '평균' then isnull(f.평균, 0) when '표준편차' then isnull(f.표준편차, 0) end else 0 end) as '2월' "
                     + ", max(case when f.RegMonth = '3' then case g.구분 when '건수' then isnull(f.건수, 0) when '평균' then isnull(f.평균, 0) when '표준편차' then isnull(f.표준편차, 0) end else 0 end) as '3월' "
                     + ", max(case when f.RegMonth = '4' then case g.구분 when '건수' then isnull(f.건수, 0) when '평균' then isnull(f.평균, 0) when '표준편차' then isnull(f.표준편차, 0) end else 0 end) as '4월' "
                     + ", max(case when f.RegMonth = '5' then case g.구분 when '건수' then isnull(f.건수, 0) when '평균' then isnull(f.평균, 0) when '표준편차' then isnull(f.표준편차, 0) end else 0 end) as '5월' "
                     + ", max(case when f.RegMonth = '6' then case g.구분 when '건수' then isnull(f.건수, 0) when '평균' then isnull(f.평균, 0) when '표준편차' then isnull(f.표준편차, 0) end else 0 end) as '6월' "
                     + ", max(case when f.RegMonth = '7' then case g.구분 when '건수' then isnull(f.건수, 0) when '평균' then isnull(f.평균, 0) when '표준편차' then isnull(f.표준편차, 0) end else 0 end) as '7월' "
                     + ", max(case when f.RegMonth = '8' then case g.구분 when '건수' then isnull(f.건수, 0) when '평균' then isnull(f.평균, 0) when '표준편차' then isnull(f.표준편차, 0) end else 0 end) as '8월' "
                     + ", max(case when f.RegMonth = '9' then case g.구분 when '건수' then isnull(f.건수, 0) when '평균' then isnull(f.평균, 0) when '표준편차' then isnull(f.표준편차, 0) end else 0 end) as '9월' "
                     + ", max(case when f.RegMonth = '10' then case g.구분 when '건수' then isnull(f.건수, 0) when '평균' then isnull(f.평균, 0) when '표준편차' then isnull(f.표준편차, 0) end else 0 end) as '10월' "
                     + ", max(case when f.RegMonth = '11' then case g.구분 when '건수' then isnull(f.건수, 0) when '평균' then isnull(f.평균, 0) when '표준편차' then isnull(f.표준편차, 0) end else 0 end) as '11월' "
                     + ", max(case when f.RegMonth = '12' then case g.구분 when '건수' then isnull(f.건수, 0) when '평균' then isnull(f.평균, 0) when '표준편차' then isnull(f.표준편차, 0) end else 0 end) as '12월' "
                     + ", case g.구분 when '건수' then convert(numeric(12, 1), isnull(AVG(f.건수 * 1.0), 0)) when '평균' then convert(numeric(12, 1), isnull(avg(f.평균), 0)) when '표준편차' then convert(numeric(12, 1), isnull(avg(f.표준편차), 0)) end as 평균 "
                     + "from(select e.CompGroup03, a.CompCode, d.CompName, DATEPART(mm, a.LabRegDate) as RegMonth, COUNT(b.TestCode) as 건수, convert(numeric(12, 1), AVG(convert(float, c.TestResult01))) as 평균, CONVERT(numeric(12, 1), STDEV(convert(float, c.TestResult01))) as 표준편차 "
                     + "from LabRegInfo as a "
                     + "inner join LabRegTest as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo "
                     + "inner join LabRegResult as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo and b.TestCode = c.TestCode "
                     + "inner join ProgCompCode as d on a.CompCode = d.CompCode "
                     + "inner join ProgCompGroupCode as e on d.CompGroupCode = e.CompGroupCode "
                     + "where a.LabRegDate between '" + year + "-01-01' and '" + year + "-12-31' and a.LabRegNo > '400' "
                     + "and b.TestCode = '11079' and b.TestStateCode = 'F' "
                     + "and c.TestResult01 not in ('.', '..') and convert(float, c.TestResult01) != 0 and c.TestResult01 is not null and ISNUMERIC(c.TestResult01) = 1 "
                     + "group by a.CompCode, d.CompName, e.CompGroup03, DATEPART(mm, a.LabRegDate)) as f "
                     + ", (select '1' as seq, '건수' as 구분 union all select '2' as seq, '평균' as 구분 union all select '3' as seq, '표준편차' as 구분) as g "
                     + "group by f.CompGroup03, f.CompCode, f.CompName, g.구분, g.seq "
                     + "order by f.CompGroup03, f.CompCode, g.seq ";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


    }
}