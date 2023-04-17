using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    public class ResultCopyController : ApiController
    {

        [Route("api/Diagnostic/ResultCopy/ReportCode")]
        public IHttpActionResult GetReportCode()
        {
            string sql;
            sql = "select ReportCode as 코드, ReportName as 보고서 from LabReportCode where IsUseReport = '1' and ReportCode in ('01', '03', '04', '04_1', '12', '12_2', '14', '07', '29', '17', '30', 'LC006', 'L001', 'LC009', 'LC010', 'LC013', 'L004', 'LC017')";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }
        
        [Route("api/Diagnostic/ResultCopy/Source")]
        public IHttpActionResult GetSource(string labRegDate, string labRegNo, string targetLabRegNo, string targetLabRegDate, string editvalue, string whereOption)
        {
            string sql;

            sql = "select a.LabRegDate as 접수일, a.LabRegNo as 접수번호, b.CompName as 거래처명, a.PatientName as 수진자명, a.PatientJuminNo01 as 생년월일, a.PatientAge as 나이, a.PatientSex as 성별, a.PatientChartNo as 차트번호, h.ReportBunjuNo as 병리번호, e.PartName as 파트명, c.OrderCode as 의뢰코드, c.TestCode as 테스트코드, c.TestSubCode as 검사코드, "
                           + "d.TestDisplayName as 검사명, d.IsTestHeader as 헤더, g.TestStateShortName as 상태, c.TestResult01 as 결과1, c.TestResult02 as 결과2, c.TestResultAbn as 판정, c.TestResultText as 서술, isnull(c.IsTestResultPanic, 0) as P, isnull(c.IsTestResultDelta, 0) as D, isnull(c.IsTestResultCritical, 0) as C, f.DoctorCode as 판독의코드, j.DoctorPersonName as 판독의 , k.TestSubCode as T코드 "
                           + "from LabRegInfo as a inner join ProgCompCode as b on a.CompCode = b.CompCode "
                           + "inner join LabRegResult as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo "
                           + "inner join LabTestCode as d on c.TestSubCode = d.TestCode "
                           + "inner join LabPartCode as e on d.PartCode = e.PartCode "
                           + "inner join LabRegTest as f on a.LabRegDate = f.LabRegDate and a.LabRegNo = f.LabRegNo and c.TestCode = f.TestCode "
                           + "inner join LabTestStateCode as g on f.TestStateCode = g.TestStateCode "
                           + "inner join LabTestCode as i on f.TestCode = i.TestCode "
                           + "inner join LabRegReport as h on a.LabRegDate = h.LabRegDate and a.LabRegNo = h.LabRegNo "
                           + "inner join (select distinct RtT.ReportCode, RtT.TestCode "
                           + "from (select ReportCode, TestCode from LabReportTest union all "
                           + "select ReportCode, TestCode from LabTestCode) as RtT) as L on h.ReportCode = L.ReportCode and i.TestCode = L.TestCode "
                           + "left outer join LabDoctorCode as j on f.DoctorCode = j.DoctorCode "
                           + "left outer join (select a.LabRegDate, a.LabRegNo, a.TestSubCode, b.RelatedTestCode from LabRegResult as a inner join LabTestCode as b on a.TestSubCode = b.TestCode) as k on k.LabRegDate = '" + targetLabRegDate + "' and k.LabRegNo = '" + targetLabRegNo + "' and " + whereOption + " in (k.TestSubCode, k.RelatedTestCode) "
                           //+ "left outer join (select a.LabRegDate, a.LabRegNo, a.TestSubCode, b.RelatedTestCode from LabRegResult as a inner join LabTestCode as b on a.TestSubCode = b.TestCode) as k on k.LabRegDate = '" + dateT.Value.ToString("yyyy-MM-dd") + "' and k.LabRegNo = '" + txtTNo.Text.ToString() + "' and d.RelatedTestCode in (k.TestSubCode, k.RelatedTestCode) "
                           + "where a.LabRegDate = '" + labRegDate + "' and a.LabRegNo = '" + labRegNo + "'  and L.ReportCode = '" + editvalue + "' "
                           + "order by e.PartSeqNo, d.TestSeqNo, d.TestCode, d.IsTestHeader desc ";


            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/Diagnostic/ResultCopy/Target")]
        public IHttpActionResult GetTarget(string targetLabRegNo, string targetLabRegDate, string targetReportCode )
        {
            string sql;
            sql = "select a.LabRegDate as 접수일, a.LabRegNo as 접수번호, b.CompName as 거래처명, a.PatientName as 수진자명, a.PatientJuminNo01 as 생년월일, a.PatientAge as 나이, a.PatientSex as 성별, a.PatientChartNo as 차트번호, h.ReportBunjuNo as 병리번호, e.PartName as 파트명, c.OrderCode as 의뢰코드, c.TestCode as 테스트코드, c.TestSubCode as 검사코드, "
                           + "d.TestDisplayName as 검사명, d.IsTestHeader as 헤더, g.TestStateShortName as 상태, c.TestResult01 as 결과1, c.TestResult02 as 결과2, c.TestResultAbn as 판정, c.TestResultText as 서술, isnull(c.IsTestResultPanic, 0) as P, isnull(c.IsTestResultDelta, 0) as D, isnull(c.IsTestResultCritical, 0) as C, f.DoctorCode as 판독의코드, j.DoctorPersonName as 판독의, h.ReportCode  "
                           + "from LabRegInfo as a inner join ProgCompCode as b on a.CompCode = b.CompCode "
                           + "inner join LabRegResult as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo "
                           + "inner join LabTestCode as d on c.TestSubCode = d.TestCode "
                           + "inner join LabPartCode as e on d.PartCode = e.PartCode "
                           + "inner join LabRegTest as f on a.LabRegDate = f.LabRegDate and a.LabRegNo = f.LabRegNo and c.TestCode = f.TestCode "
                           + "inner join LabTestStateCode as g on f.TestStateCode = g.TestStateCode "
                           + "inner join LabTestCode as i on f.TestCode = i.TestCode "
                           + "inner join LabRegReport as h on a.LabRegDate = h.LabRegDate and a.LabRegNo = h.LabRegNo "
                           + "inner join (select distinct RtT.ReportCode, RtT.TestCode "
                           + "from (select ReportCode, TestCode from LabReportTest union all "
                           + "select ReportCode, TestCode from LabTestCode) as RtT) as L on h.ReportCode = L.ReportCode and i.TestCode = L.TestCode "
                           + "left outer join LabDoctorCode as j on f.DoctorCode = j.DoctorCode "
                           + "where a.LabRegDate = '" + targetLabRegDate + "' and a.LabRegNo = '" + targetLabRegNo + "'  and L.ReportCode in (" + targetReportCode + ") "
                           + "order by e.PartSeqNo, d.TestSeqNo, d.TestCode, d.IsTestHeader desc ";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }
    }
}