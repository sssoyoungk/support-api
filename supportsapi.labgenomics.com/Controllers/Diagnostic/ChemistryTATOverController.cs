using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    public class ChemistryTATOverController : ApiController
    {

        [Route("api/Diagnostic/ChemistryTATOver/LoadRemark")]
        public IHttpActionResult GetLoadRemark(DateTime beginDate, DateTime endDate, string remark)
        {
            string sql = "select a.LabRegDate as 등록일, a.LabRegNo as 등록번호, a.CompName as 거래처명, a.PatientName as 수진자명, a.PatientChartNo as 차트번호, a.PatientJuminNo01 as 생년월일, a.PatientSex as 성별, a.TestCode as 검사코드, a.TestDisplayName as 검사명, a.TestStateShortName as 상태, a.SumRemark as 학부소견, a.PatientMemo as 기타매모, a.TAT as 'TAT(시간)', a.TAT_Hour as '소요(시간)', a.TAT_Min as '소요(분)' "
                        + "from View_ChemistryStatics as a "
                        + "where a.LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "' "
                        + "and a.TestTATCheck = '1' and a.IsTestOutside = '0' and a.CompGroupCode not in ('02', '03') "
                        + "and (TAT_TMin > TAT_M or a.TestStateShortName != '최종') and a.sRemark != 'K' and a.sRemark = '" + remark + "' order by a.LabRegDate, a.LabRegNo";


            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }
        

        [Route("api/Diagnostic/ChemistryTATOver/LoadComment/Total")]
        public IHttpActionResult GetLoadCommentTotal(DateTime beginDate, DateTime endDate, string month)
        {

            string sql = "select 'TAT Over건수' as 구분 , COUNT(a.LabRegDate) as '" + month + "월' "
                         + "from View_ChemistryStatics as a "
                         + "where a.LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and  '" + endDate.ToString("yyyy-MM-dd") + "' "
                         + "and a.TestTATCheck = '1' and a.IsTestOutside = '0' and a.CompGroupCode not in ('02', '03') "
                         + "and (TAT_TMin > TAT_M or a.TestStateShortName != '최종') and a.sRemark != 'K' union all "
                         + "select '총건수' as 구분, COUNT(a.LabRegDate) as '" + month + "월' from View_ChemistryStatics as a where a.LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and  '" + endDate.ToString("yyyy-MM-dd") + "' ";


            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


        [Route("api/Diagnostic/ChemistryTATOver/LoadComment/Count")]
        public IHttpActionResult GetLoadCommentCount(DateTime beginDate, DateTime endDate, string month)
        {
            string sql = "select b.Gubun as 구분, case b.Gubun "
                      + "when '용혈' then sum(case when DATEPART(mm, a.LabRegDate) = " + month + " then case a.sRemark when '용혈' then 1 else 0 end end)  "
                      + "when 'Lipemic' then sum(case when DATEPART(mm, a.LabRegDate) = " + month + " then case a.sRemark when 'Lipemic' then 1 else 0 end end)  "
                      + "when '부족' then sum(case when DATEPART(mm, a.LabRegDate) = " + month + " then case a.sRemark when '부족' then 1 else 0 end end) "
                      + "when '검체추후' then sum(case when DATEPART(mm, a.LabRegDate) = " + month + " then case a.sRemark when '검체추후' then 1 else 0 end end)  "
                      + "when 'No Sample' then sum(case when DATEPART(mm, a.LabRegDate) = " + month + " then case a.sRemark when 'No Sample' then 1 else 0 end end)  "
                      + "when '재검' then sum(case when DATEPART(mm, a.LabRegDate) = " + month + " then case a.sRemark when '재검' then 1 else 0 end end) "
                      + "when '기타' then sum(case when DATEPART(mm, a.LabRegDate) = " + month + " then case a.sRemark when '기타' then 1 else 0 end end) end as '" + month + "월' "
                      + "from View_ChemistryStatics as a "
                      + ", (select '1' as seq, '용혈' as Gubun union all select '2' as seq, 'Lipemic' as Gubun union all select '3' as seq, '부족' as Gubun union all select '4' as seq, '검체추후' as Gubun union all select '5' as seq, 'No sample' as Gubun union all select '6' as seq, '재검' as Gubun union all select '7' as seq, '기타' as Gubun) as b "
                      + "where a.LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and  '" + endDate.ToString("yyyy-MM-dd") + "' and a.TestTATCheck = '1' and a.IsTestOutside = '0' and a.CompGroupCode not in ('02', '03') and (TAT_TMin > TAT_M or a.TestStateShortName != '최종') and a.sRemark != 'K' "
                      + "group by b.Gubun, b.seq order by b.seq ";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


        [Route("api/Diagnostic/ChemistryTATOver/DistinctYear")]
        public IHttpActionResult GetDistinctYear()
        {
            string sql = "select Distinct Year from ProgMonthlystatistics order by Year desc";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/Diagnostic/ChemistryTATOver/LoadDate")]
        public IHttpActionResult GetLoadDate(DateTime beginDate, DateTime endDate, string gubun)
        {
            string sql =       "DECLARE @BeginDate Date\n"
                      + "DECLARE @EndDate Date\n"
                      + "DECLARE @DW Int\n"
                      + "set @BeginDate = '" + beginDate.ToString("yyyy-MM-dd") + "'\n"
                      + "set @EndDate = '" + endDate.ToString("yyyy-MM-dd") + "'\n"
                      + "set @DW = 1\n"
                      + "select a.LabRegDate as 접수일, a.LabRegNo as 접수번호, a.CompName as 거래처명, a.PatientName as 수진자명, a.PartName as 학부명, a.TestCode as 검사코드, a.TestDisplayName as 검사명, a.IsTestOutside as 외주\n"
                      + ", a.TestRegistTime as 의뢰일시, a.TestStartTime as 시작일시, a.TestEndTime as 종료일시, a.TAT, a.RealTestHour as 소요시간, a.RealTestMin as 소요분, a.PatientMemo as 기타메모\n"
                      + "from\n"
                      + "(select a.LabRegDate, a.LabRegNo, d.CompName, c.PatientName, e.PartName, a.TestCode, b.TestDisplayName\n"
                      + ",case a.IsTestOutside when '1' then '외주' else '' end as IsTestOutside,\n"
                      + "a.TestRegistTime, a.TestStartTime, a.TestEndTime\n"
                      + ", TestTATDay * 24 + TestTATHour as TAT\n"
                      + ", DATEDIFF(HOUR, a.TestStartTime, a.TestEndTime) as RealTestHour\n"
                      + ", DATEDIFF(MINUTE, a.TestStartTime, a.TestEndTime) % 60 as RealTestMin\n"
                      + ", case when TestTATDay * 24 + TestTATHour = 0 then a.TestStartTime\n"
                      + "  else DATEADD(HOUR, (TestTATDay * 24) + TestTATHour + (dbo.FUNC_DW_Count(a.TestStartTime, a.TestEndTime, @DW) * 24), a.TestStartTime) end as RegularTime\n"
                      + ", a.WorkStateCode, a.TestModifyTAT, a.IsTestTATMakeGood, c.PatientMemo\n"
                      + ", case when a.IsTestTATMakeGood = '1'\n"
                      + "       or case when TestTATDay * 24 + TestTATHour = 0 then a.TestStartTime else DATEADD(HOUR, (TestTATDay * 24) + TestTATHour + (dbo.FUNC_DW_Count(a.TestStartTime, a.TestEndTime, @DW) * 24), a.TestStartTime) end\n"
                      + "       > a.TestEndTime then '0' else '1' end as gubun\n"
                      + "from LabRegTest as a\n"
                      + "inner join LabTestCode as b on a.TestCode = b.TestCode\n"
                      + "inner join LabRegInfo as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo\n"
                      + "inner join ProgCompCode as d on c.CompCode = d.CompCode\n"
                      + "inner join LabPartCode as e on b.PartCode = e.PartCode\n"
                      + "where a.LabRegDate between @BeginDate and @EndDate\n"
                      + "and b.PartCode in ('11', '28', '17')\n"
                      + "and b.TestTATCheck = '1'\n"
                      //+ "and a.LabRegNo = '526'\n"
                      + "and a.TestStateCode = 'F') as a\n"
                      + "where a.gubun in (" + gubun + ") order by a.LabRegDate, a.LabRegNo";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }
    }
}