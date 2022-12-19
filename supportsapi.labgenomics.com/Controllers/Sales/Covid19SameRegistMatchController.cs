using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [SupportsAuth]
    [Route("api/Sales/SameRegistMatch")]
    public class Covid19SameRegistMatchController : ApiController
    {

        /// <summary>
        /// 질청 중복 확인 컨트롤
        /// </summary>
        /// <param name="beginDate">시작일</param>
        /// <param name="endDate">종료일</param>
        /// <param name="authGroupCode">그룹 코드</param>
        /// <returns></returns>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string authGroupCode)
        {

            StringBuilder query = new StringBuilder();
            query.Append($"select * from\n");
            query.Append($"(select *, count(r.SampleNo) over(partition by r.SampleNo) as cntSampleNo\n");
            query.Append($"from\n");
            query.Append($"(select a.LabRegDate, a.LabRegNo, a.CompCode, e.CompName\n");
            query.Append($", case when g.TestDisplayName like '%그룹%' then f.CustomValue01 else a.PatientName end as PatientName\n");
            query.Append($", case when g.TestDisplayName like '%그룹%' then dbo.FUNC_GetIdxDataLikeSplit(f.CustomValue02, 1, ';') else a.PatientJuminNo01 + '-' + master.dbo.AES_DecryptFunc(a.PatientJuminNo02, 'labge$%#!dleorms') end as PatientJuminNo\n");
            query.Append($", case when g.TestDisplayName like '%그룹%' then dbo.FUNC_GetIdxDataLikeSplit(f.CustomValue02, 2, ';') else a.SystemUniqID end as SampleNo\n");
            query.Append($", c.OrderCode, g.TestDisplayName\n");
            query.Append($", a.IsTrustOrder, a.CenterCode\n");
            query.Append($", d.TestResult01\n");
            query.Append($"from LabRegInfo as a\n");
            query.Append($"inner join LabRegOrder as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n");
            query.Append($"inner join LabRegTest as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo and b.OrderCode = c.OrderCode\n");
            query.Append($"inner join LabRegResult as d on a.LabRegDate = d.LabRegDate and a.LabRegNo = d.LabRegNo and b.OrderCode = d.OrderCode and c.TestCode = d.TestCode\n");
            query.Append($"inner join ProgCompCode as e on a.CompCode = e.CompCode\n");
            query.Append($"inner join LabTestCode as g on d.TestCode = g.TestCode\n");
            query.Append($"left outer join LabRegCustom as f on a.LabRegDate = f.LabRegDate and a.LabRegNo = f.LabRegNo and f.CustomCode in ('5401', '5402', '5403', '5404', '5405')\n");
            query.Append($"where a.LabRegDate between '{beginDate.ToString("yyyy-MM-dd")}' and '{endDate.ToString("yyyy-MM-dd")}'\n");
            query.Append($"and e.CompDemandGroupCode = '24'\n");
            query.Append($"and a.CenterCode != 'AutoReg') as r\n");
            query.Append($"where r.CompCode in (select CompCode from ProgAuthGroupAccessComp where AuthGroupCode = '{authGroupCode}')\n");
            query.Append($"and r.SampleNo != ''\n");
            query.Append($"and (r.SampleNo like 'SC%'\n");
            query.Append($"or r.SampleNo like 'SA%'\n");
            query.Append($"or r.SampleNo like 'TI%'\n");
            query.Append($"or r.SampleNo like 'KP%'\n");
            query.Append($"or r.SampleNo like 'IR%')) as r2\n");
            query.Append($"where r2.cntSampleNo > 1\n");
            query.Append($"order by r2.LabRegDate, r2.LabRegNo\n");

            var arrResponse = LabgeDatabase.SqlToJArray(query.ToString());

            return Ok(arrResponse);
        }

    }
}