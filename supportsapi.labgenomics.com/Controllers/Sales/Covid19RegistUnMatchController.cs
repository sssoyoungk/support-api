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
    [Route("api/Sales/RegistUnMatch")]
    public class Covid19RegistUnMatchController : ApiController
    {

        /// <summary>
        /// 미매치 리스트 
        /// </summary>
        /// <param name="beginDate">시작일</param>
        /// <param name="endDate">종료일</param>
        /// <param name="authGroupCode">그룹 코드</param>
        /// <returns></returns>
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string authGroupCode, string option)
        {
            string strOption = "";

            if (option == "un")
            {
                strOption = "and r.SampleNo != ''\n"
                      + "and r.SampleNo not like 'SC%'\n"
                      + "and r.SampleNo not like 'SA%'\n"
                      + "and r.SampleNo not like 'TI%'\n"
                      + "and r.SampleNo not like 'KP%'\n"
                      + "and r.SampleNo not like 'IR%'\n";
            }
            

            StringBuilder query = new StringBuilder();
            query.Append("select *\n");
            query.Append("from\n");
            query.Append("(select a.LabRegDate, a.LabRegNo, a.CompCode, b.CompName\n");
            query.Append(", case when d.TestDisplayName like '%그룹%' then e.CustomValue01 else a.PatientName end as PatientName\n");
            query.Append(", case when d.TestDisplayName like '%그룹%' then dbo.FUNC_GetIdxDataLikeSplit(e.CustomValue02, 1, ';') else a.PatientJuminNo01 + '-' + master.dbo.AES_DecryptFunc(a.PatientJuminNo02, 'labge$%#!dleorms') end as PatientJuminNo\n");
            query.Append(", case when d.TestDisplayName like '%그룹%' then dbo.FUNC_GetIdxDataLikeSplit(e.CustomValue02, 2, ';') else a.SystemUniqID end as SampleNo\n");
            query.Append(", c.OrderCode, d.TestDisplayName\n");
            query.Append(", a.IsTrustOrder, a.CenterCode\n");
            query.Append("from LabRegInfo as a\n");
            query.Append("inner join ProgCompCode as b on a.CompCode = b.CompCode\n");
            query.Append("inner join LabRegOrder as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo\n");
            query.Append("inner join LabTestCode as d on c.OrderCode = d.TestCode\n");
            query.Append("left outer join LabRegCustom as e on a.LabRegDate = e.LabRegDate and a.LabRegNo = e.LabRegNo and e.CustomCode in ('5401', '5402', '5403', '5404', '5405')\n");
            query.Append($"where a.LabRegDate between '{beginDate.ToString("yyyy-MM-dd")}' and '{endDate.ToString("yyyy-MM-dd")}'\n");
            query.Append("and b.CompDemandGroupCode = '24'\n");
            query.Append("--and isnull(a.CenterCode, '') = 'Covid19Excel'\n");
            query.Append(") as r\n");
            query.Append($"where r.CompCode in (select CompCode from ProgAuthGroupAccessComp where AuthGroupCode = '{authGroupCode}')\n");
            query.Append(strOption);
            //query.Append("and r.SampleNo != ''\n");
            //query.Append("and r.SampleNo not like 'SC%'\n");
            //query.Append("and r.SampleNo not like 'SA%'\n");
            //query.Append("and r.SampleNo not like 'TI%'\n");
            //query.Append("and r.SampleNo not like 'KP%'\n");
            //query.Append("and r.SampleNo not like 'IR%'\n");
            query.Append("order by r.LabRegDate, r.LabRegNo\n");

            var arrResponse = LabgeDatabase.SqlToJArray(query.ToString());

            return Ok(arrResponse);
        }

    }
}