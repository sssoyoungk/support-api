using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Molecular
{
    [SupportsAuth]
    [Route("api/molecular/WasteList")]
    public class WasteListController : ApiController
    {

        public IHttpActionResult Get(DateTime beginDate, DateTime endDate)
        {
            StringBuilder query = new StringBuilder();
            query.Append("select ROW_NUMBER() over(order by ID) as 일련번호,  ID as 관리번호, SampleName as 종류, RegDateAdd1 as 연월일_수증내용, SampleCapa as 수증량, CompName as 기관명, '' as 연월일_제공내용, '' as 제공량, '' as 제공기관명\n");
            query.Append(", RegDateAdd14 as 연월일_허가번호\n");
            query.Append(", case SampleCode\n");
            query.Append("when '01' then SampleCapa - 0.2\n");
            query.Append("when '99' then SampleCapa - 2\n");
            query.Append("when '103' then SampleCapa - 2\n");
            query.Append("when '59' then SampleCapa - 2\n");
            query.Append("when '29' then SampleCapa - 10\n");
            query.Append("else 0 end as 폐기량\n");
            query.Append(", 'X' as 자가처리, '메디코연합' as 위탁처리, '실온' as 보관조건, '이현주' as 담당, '김소영' as 관리책임자\n");
            query.Append("from\n");
            query.Append("(select convert(varchar(10), a.LabRegDate, 112) + right('00000' + convert(varchar, a.LabRegNo), 5) as ID, e.SampleName\n");
            query.Append(", DATEADD(dd, 1, a.LabRegDate) as RegDateAdd1, c.SampleCode\n");
            query.Append(", case c.SampleCode\n");
            query.Append("when '01' then floor(rand(checksum(newid())) * (51 - 15) + 15) / 10\n");
            query.Append("when '99' then floor(rand(checksum(newid())) * (11 - 5) + 5)\n");
            query.Append("when '103' then floor(rand(checksum(newid())) * (11 - 5) + 5)\n");
            query.Append("when '59' then floor(rand(checksum(newid())) * (6 - 3) + 3)\n");
            query.Append("when '29' then floor(rand(checksum(newid())) * (16 - 10) + 10)\n");
            query.Append("else 0 end as SampleCapa\n");
            query.Append(", g.CompName, DATEADD(dd, 14, a.LabRegDate) as RegDateAdd14\n");
            query.Append("from LabRegInfo as a\n");
            query.Append("inner join LabRegOrder as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n");
            query.Append("inner join LabRegTest as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo and b.OrderCode = c.OrderCode\n");
            query.Append("inner join LabWorkTest as d on c.TestCode = d.TestCode and d.WorkCode = 'G02'\n");
            query.Append("inner join LabSampleCode as e on c.SampleCode = e.SampleCode\n");
            query.Append("inner join LabTestCode as f on c.TestCode = f.TestCode\n");
            query.Append("inner join ProgCompCode as g on a.CompCode = g.CompCode\n");
            query.Append($"where a.LabRegDate between '{beginDate.ToString("yyyy-MM-dd")}' and '{endDate.ToString("yyyy-MM-dd")}'\n");
            query.Append("and a.CompCode not in ('3268', '3934')) as r\n");
            query.Append("order by r.ID\n");
            JArray arrResponse = LabgeDatabase.SqlToJArray(query.ToString());
            return Ok(arrResponse);

        }
    }
}