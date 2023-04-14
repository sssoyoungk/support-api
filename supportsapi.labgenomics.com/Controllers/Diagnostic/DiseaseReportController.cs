using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    public class DiseaseReportController : ApiController
    {

        [Route("api/Diagnostic/DiseaseReport")]
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string groupCode)
        {
            string sql;
            sql = "select d.CompInstitutionNo as 의뢰기관코드, d.CompName as 의뢰기관명, case when a.PatientDoctorName != '' then a.PatientDoctorName else d.CompDoctorName end as 담당자\n"
                     + ", a.PatientName as 성명, a.PatientSex as 성별\n"
                     + ", case when PatientJuminNo02 = '' then PatientJuminNo01\n"
                     + "  when left(master.dbo.AES_DecryptFunc(PatientJuminNo02, 'labge$%#!dleorms'), 1)  in ('1', '2', '5', '6') then '19' + a.PatientJuminNo01\n"
                     + "  when left(master.dbo.AES_DecryptFunc(PatientJuminNo02, 'labge$%#!dleorms'), 1)  in ('9', '0') then '18' + a.PatientJuminNo01\n"
                     + "  else '20' + a.PatientJuminNo01 end as 생년월일\n"
                     + ", convert(varchar(8), a.LabRegDate, 112) + '-' + right('00000' + convert(varchar, a.LabRegNo), 5) as 등록번호,a.PatientChartNo as 차트번호, '' as 진료과명\n"
                     + ", case when b.SampleCode in ('115', '23') then '11' when b.SampleCode in ('128', '04', '040', '83') then '12' when b.SampleCode in ('127', '125', '90') then '13'\n"
                     + "       when b.SampleCode in ('124', '09') then '14' when b.SampleCode in ('25') then '15' else '99' end as 검체종류\n"
                     + ", case when b.SampleCode not in ('115', '23', '128', '04', '040', '127', '125', '90', '124', '09', '25') then e.SampleName else '' end as '검체종류(기타)'\n"
                     + ", case when g.MethodName like '%culture%' then '01' when g.MethodName like '%PCR%' then  '02' else '99' end as 검사방법\n"
                     + ", case when g.MethodName like '%culture%' or g.MethodName like '%PCR%' then  '' else g.MethodName end as '검사방법(기타)'\n"
                     + ", '135' as 병원체코드, convert(varchar(8), a.LabRegDate, 112) as 의뢰일, convert(varchar(8), b.TestEndTime, 112) as 진단일, '41355709' as 기관번호, '서동희' as 진단의\n"
                     + ", '' as '비고(특이사항)'\n"
                     + ", a.LabRegDate as 접수일, a.LabRegNo as 접수번호, b.TestCode as 검사코드, f.TestDisplayName as 검사명, a.PatientJuminNo01 as 주민번호앞\n"
                     + ", left(master.dbo.AES_DecryptFunc(PatientJuminNo02, 'labge$%#!dleorms'), 1) as 주민번호뒤\n"
                     + ", e.SampleName as 검체\n"
                     + "from LabRegInfo as a inner join LabRegTest as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                     + "inner join LabRegResult as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo and b.TestCode = c.TestCode\n"
                     + "inner join ProgCompCode as d on a.CompCode = d.CompCode\n"
                     + "inner join LabSampleCode as e on b.SampleCode = e.SampleCode\n"
                     + "inner join LabTestCode as f on b.TestCode = f.TestCode\n"
                     + "inner join LabMethodCode as g on f.MethodCode = g.MethodCode\n"
                     + "inner join ProgCompGroupCode as h on d.CompGroupCode = h.CompGroupCode\n"
                     + "inner join ProgAuthGroupAccessComp as i on d.CompCode = i.CompCode\n"
                    + "where a.LabRegDate between dateadd(mm, -1, dateadd(dd, -7, '" + beginDate.ToString("yyyy-MM-dd") + "')) and dateadd(dd, -1, '" + endDate.ToString("yyyy-MM-dd") + "')\n"
                     + "and convert(varchar(10), b.TestEndTime, 126) between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'\n"
                     + "and a.CompCode not in ('4224', '5447', '5431')\n"
                     + "and c.TestSubCode in ('2113102', '2113103')\n"
                     + "and b.TestStateCode = 'F'\n"
                     + $"and i.AuthGroupCode = '{groupCode}'\n"
                     + "and replace(c.TestResultText, ' ' , '') like replace('%CRE(Carbapenem-Resistant Enterobacteriaceae) : 양성%', ' ', '')\n"
                     + "order by a.LabRegDate, a.LabRegNo\n";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


    }
}