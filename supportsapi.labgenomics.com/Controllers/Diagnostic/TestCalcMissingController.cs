using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    public class TestCalcMissingController : ApiController
    {



        [Route("api/Diagnostic/TestCalcMissing")]
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string mode, bool calculator)
        {
            string sql = string.Empty;
            string option = string.Empty;
            string optionH = string.Empty;

            if (mode  == "All")
            {
                option = "'1', '0'";
                optionH = "0";
            }
            else
            {
                option = "'1'";
                optionH = "1";
            }


            if (calculator)
            {
                sql = "select a.LabRegDate as 접수일, a.LabRegNo as 접수번호, a.CompCode as 거래처코드, a.CompName as 거래처명, a.TestCode as 검사코드_결과, a.ResultTestName 검사명_결과, a.CalcCode as 검사코드_계산, a.CalcTestName as 검사명_계산 "
                     + ", case a.OX when 0 then 'O' else 'X' end as OX, a.MemberName as 접수자명, a.MemberPhoneNo as 연락처 "
                     + "from (select b.LabRegDate, b.LabRegNo, f.CompCode, g.CompName, b.TestCode, c.TestDisplayName as ResultTestName, a.CalcCode, d.TestDisplayName as CalcTestName "
                     + ", case ISNULL(e.TestCode, 'X') when 'X' then 1 else 0 end as OX "
                     + ", h.MemberName, h.MemberPhoneNo, c.TestSeqNo "
                     + "from LabCalcTestCode as a inner join LabRegTest as b on a.ResultCode = b.TestCode "
                     + "inner join LabTestCode as c on b.TestCode = c.TestCode "
                     + "inner join LabTestCode as d on a.CalcCode = d.TestCode "
                     + "inner join LabRegInfo as f on b.LabRegDate = f.LabRegDate and b.LabRegNo = f.LabRegNo "
                     + "inner join ProgCompCode as g on f.CompCode = g.CompCode "
                     + "inner join ProgMember as h on f.RegistMemberID = h.MemberID "
                     + "left outer join LabRegTest as e on b.LabRegDate = e.LabRegDate and b.LabRegNo = e.LabRegNo and a.CalcCode = e.TestCode "
                     + "where b.LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "' "
                     + "and b.LabRegNo > 400) as a "
                     + "where a.OX in (" + option + ") "
                     + "order by a.LabRegDate, a.LabRegNo, a.TestSeqNo";
            }
            else
            {
                sql = "select a.LabRegDate as 접수일, a.LabRegNo as 접수번호, a.CompCode as 거래처코드, a.CompName as 거래처명 "
                         + ", replace(replace(a.H11084, 1, 'O'), 0, '') as '11084', replace(replace(a.H11604, 1, 'O'), 0, '') as '11604' "
                         + ", replace(replace(a.H11605, 1, 'O'), 0, '') as '11605', replace(replace(a.H1160501, 1, 'O'), 0, '') as '1160501' "
                         + ", replace(replace(a.H1160401, 1, 'O'), 0, '') as '1160401', a.MemberName as 등록자명, a.MemberPhoneNo as 연락처 "
                         + ", a.H11084 + a.H11604 + a.H11605 + a.H1160501 + a.H1160401 as 갯수 "
                         + "from (select a.LabRegDate, a.LabRegNo, a.CompCode, c.CompName "
                         + ", max(case b.TestCode when '11084' then 1 else 0 end) as H11084, max(case b.TestCode when '11604' then 1 else 0 end) as H11604 "
                         + ", max(case b.TestCode when '11605' then 1 else 0 end) as H11605, max(case b.TestCode when '1160501' then 1 else 0 end) as H1160501 "
                         + ", max(case b.TestCode when '1160401' then 1 else 0 end) as H1160401, d.MemberName, d.MemberPhoneNo "
                         + "from LabRegInfo as a inner join LabRegTest as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo "
                         + "inner join ProgCompCode as c on a.CompCode = c.CompCode "
                         + "inner join ProgMember as d on a.RegistMemberID = d.MemberID "
                         + "where a.LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "' "
                         + "and a.LabRegNo > '400' "
                         + "and b.TestCode in ('11084', '11604', '11605', '1160501', '1160401') "
                         + "group by a.LabRegDate, a.LabRegNo, a.CompCode, c.CompName, d.MemberName, d.MemberPhoneNo) as a "
                         + "where a.H11084 + a.H11604 + a.H11605 + a.H1160501 + a.H1160401 > " + optionH + " "
                         + "order by a.LabRegDate, a.LabRegNo ";
            }
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);

        }


    }
}