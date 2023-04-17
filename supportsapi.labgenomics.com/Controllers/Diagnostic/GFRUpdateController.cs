using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    public class GFRUpdateController : ApiController
    {


        [Route("api/Diagnostic/GFRUpdate/eGFR")]
        
        public IHttpActionResult Put(JObject objRequest)
        {
            try
            {
                string sql;

                sql = "UPDATE LabRegResult\n"
                                  + "set TestResultText = char(13) + char(10) + '" + objRequest["TestResultText"].ToString() + "', EditTime = CURRENT_TIMESTAMP, EditorMemberID = '" + objRequest["MemberID"].ToString() + "'\n"
                                  + "where LabRegDate = '" + objRequest["LabRegDate"].ToString() + "' and LabRegNo = '" + objRequest["LabRegNo"].ToString() + "' and TestCode = '11317'";
                LabgeDatabase.ExecuteSql(sql);
                return Ok();
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }

        }


        [Route("api/Diagnostic/GFRUpdate/FIB4")]
        public IHttpActionResult GetFIB4(DateTime beginDate, DateTime endDate, string option)
        {

            string sqlOption = string.Empty;
            string sql = string.Empty;
            if (option == "rabAll")
            {
                sqlOption = "";
            }
            else if (option == "rabNull")
            {
                sqlOption = "where FIB != calc or FIB = ''";
            }
            else if (option == "rabNotNull")
            {
                sqlOption = "where FIB != calc or FIB = ''";
            }
            
            sql = "select case when calc != FIB then '1' else '0' end as 구분\n"
                          + ", LabRegDate as 접수일, LabRegNo as 접수번호, PatientAge as 나이, AST as AST, PLT, ALT as ALT, FIB as 'FIB-4', calc as '계산'\n"
                          + ", etc as 비고\n"
                          + "from\n"
                          + "(select l.LabRegDate, l.LabRegNo, l.PatientAge\n"
                          + ", max(case when seqAST = 1 then l.TestResult01 end) as AST\n"
                          + ", max(case when seqALT = 1 then l.TestResult01 end) as ALT\n"
                          + ", max(case when TestCode in ('12907') then TestResult01 end) as PLT\n"
                          + ", max(case when TestCode in ('15605') then TestResult01 end) as FIB\n"
                          + ", max(seqALT) as maxALT\n"
                          + ", max(seqAST) as maxAST\n"
                          + ", case when max(case when seqAST = 1 then l.TestResult01 end) is null or max(case when seqALT = 1 then l.TestResult01 end) is null or max(case when TestCode in ('12907') then TestResult01 end) is null or PatientAge = 0\n"
                          + "         or ISNUMERIC(max(case when seqAST = 1 then l.TestResult01 end)) != 1 or ISNUMERIC(max(case when seqALT = 1 then l.TestResult01 end)) != 1 or ISNUMERIC(max(case when TestCode in ('12907') then TestResult01 end)) != 1\n"
                          + "       then ''\n"
                          + "	   else convert(varchar, convert(numeric(5, 1), (PatientAge * max(case when seqAST = 1 then l.TestResult01 end)) / (max(case when TestCode in ('12907') then TestResult01 end) * SQRT(max(case when seqALT = 1 then l.TestResult01 end))), 2))\n"
                          + "       end as calc\n"
                          + ", case when max(case when seqAST = 1 then l.TestResult01 end) is null or max(case when seqALT = 1 then l.TestResult01 end) is null or max(case when TestCode in ('12907') then TestResult01 end) is null or PatientAge = 0\n"
                          + "         or ISNUMERIC(max(case when seqAST = 1 then l.TestResult01 end)) != 1 or ISNUMERIC(max(case when seqALT = 1 then l.TestResult01 end)) != 1 or ISNUMERIC(max(case when TestCode in ('12907') then TestResult01 end)) != 1\n"
                          + "       then '항목오류'\n"
                          + "       when max(seqALT) > 1 and max(seqAST) > 1 then 'ALT, AST중복' when max(seqALT) > 1 then 'ALT중복' when max(seqAST) > 1 then 'AST중복' 	   \n"
                          + "	   else '' end as etc\n"
                          + "from\n"
                          + "(select f.LabRegDate, f.LabRegNo, f.PatientAge, c.TestCode, g.TestDisplayName, d.TestResult01\n"
                          + ", case when c.TestCode in ('11033', '33008') then ROW_NUMBER() over(partition by f.LabRegDate, f.LabRegNo order by case when c.TestCode = '11033' then 1 when c.TestCode = '33008' then 2 else 3 end) else '' end as seqALT\n"
                          + ", case when c.TestCode in ('11037', '33007') then ROW_NUMBER() over(partition by f.LabRegDate, f.LabRegNo order by case when c.TestCode = '11037' then 1 when c.TestCode = '33007' then 2 else 3 end) else '' end as seqAST\n"
                          + "from\n"
                          + "(select a.LabRegDate, a.LabRegNo, a.PatientAge, b.TestCode, b.TestStateCode\n"
                          + "from LabRegInfo as a inner join LabRegTest as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                          + "where a.LabRegDate between '2022-06-27' and '2022-06-27'\n"
                          + "and b.TestCode = '15605') as f inner join LabRegTest as c on f.LabRegDate = c.LabRegDate and f.LabRegNo = c.LabRegNo\n"
                          + "inner join LabRegResult as d on f.LabRegDate = d.LabRegDate and f.LabRegNo = d.LabRegNo and c.TestCode = d.TestCode\n"
                          + "inner join LabTestCode as g on c.TestCode = g.TestCode\n"
                          + "where c.TestCode in ('11037', '33007', '11033', '33008', '12907', '15605')) as l\n"
                          + "group by l.LabRegDate, l.LabRegNo, l.PatientAge) as z\n" + sqlOption;
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);

        }

        [Route("api/Diagnostic/GFRUpdate/HOMAB")]
        public IHttpActionResult GetHOMAB(DateTime beginDate, DateTime endDate)
        {
            string sql = "select case when r.Insulin != '' and r.Glucose != '' and isnull(r.HOMA, '') = '' then '1' else '0' end as 구분\n"
                          + ", r.LabRegDate, r.LabRegNo, r.PatientName, 0 as blank, r.Insulin, r.Glucose, r.HOMA\n"
                          + ", case when r.Insulin != '' and r.Glucose != '' then \n"
                          + "  round(360 * (convert(float, r.Insulin) / (convert(float, r.Glucose) - 63)), 0) else '' end as 계산\n"
                          + ", case when r.Insulin = '' and r.Glucose = '' then 'Insulin, Glucose'\n"
                          + "	when r.Insulin = '' then 'Insulin' when r.Glucose = '' then 'Glucose' else '' end as 누락\n"
                          + "from\n"
                          + "(select case when isnull(b.TestResult01, '') = '' then '1' else '0' end as C\n"
                          + ", a.LabRegDate, a.LabRegNo, a.PatientName\n"
                          + ", max(case c.TestCode when '13172' then c.TestResult01 else '' end) as Insulin\n"
                          + ", max(case c.TestCode when '11079' then c.TestResult01 else '' end) as Glucose\n"
                          + ", b.TestResult01 as HOMA\n"
                          + "from LabRegInfo as a \n"
                          + "inner join (select LabRegDate, LabRegNo, TestCode, TestResult01 from LabRegResult where TestCode = '11414') as b \n"
                          + "	on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                          + "left outer join \n"
                          + "	(select d.LabRegDate, d.LabRegNo, e.TestCode, e.TestResult01\n"
                          + "	from LabRegTest as d\n"
                          + "inner join LabRegResult as e on d.LabRegDate = e.LabRegDate and d.LabRegNo = e.LabRegNo\n"
                          + "where d.TestCode in ('13172', '11079')\n"
                          + "and d.TestStateCode = 'F') as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo\n"
                          + "where a.LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'\n"
                          + "group by a.LabRegDate, a.LabRegNo, a.PatientName, b.TestResult01) as r";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);

        }

        [Route("api/Diagnostic/GFRUpdate/HOMA")]
        public IHttpActionResult GetHOMA(DateTime beginDate, DateTime endDate)
        {
            string sql = "select case when r.Insulin != '' and r.Glucose != '' and isnull(r.HOMA, '') = '' then '1' else '0' end as 구분\n"
                          + ", r.LabRegDate, r.LabRegNo, r.PatientName, 0 as blank, r.Insulin, r.Glucose, r.HOMA\n"
                          + ", case when r.Insulin != '' and r.Glucose != '' then \n"
                          + "  round(convert(float, r.Insulin) * convert(float, r.Glucose) / 405.0, 2) else '' end as 계산\n"
                          + ", case when r.Insulin = '' and r.Glucose = '' then 'Insulin, Glucose'\n"
                          + "	when r.Insulin = '' then 'Insulin' when r.Glucose = '' then 'Glucose' else '' end as 누락\n"
                          + "from\n"
                          + "(select case when isnull(b.TestResult01, '') = '' then '1' else '0' end as C\n"
                          + ", a.LabRegDate, a.LabRegNo, a.PatientName\n"
                          + ", max(case c.TestCode when '13172' then c.TestResult01 else '' end) as Insulin\n"
                          + ", max(case c.TestCode when '11079' then c.TestResult01 else '' end) as Glucose\n"
                          + ", b.TestResult01 as HOMA\n"
                          + "from LabRegInfo as a \n"
                          + "inner join (select LabRegDate, LabRegNo, TestCode, TestResult01 from LabRegResult where TestCode = '11413') as b \n"
                          + "	on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                          + "left outer join \n"
                          + "	(select d.LabRegDate, d.LabRegNo, e.TestCode, e.TestResult01\n"
                          + "	from LabRegTest as d\n"
                          + "inner join LabRegResult as e on d.LabRegDate = e.LabRegDate and d.LabRegNo = e.LabRegNo\n"
                          + "where d.TestCode in ('13172', '11079')\n"
                          + "and d.TestStateCode = 'F') as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo\n"
                          + "where a.LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'\n"
                          + "group by a.LabRegDate, a.LabRegNo, a.PatientName, b.TestResult01) as r";


            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);

        }




        [Route("api/Diagnostic/GFRUpdate/EPI")]
        public IHttpActionResult GetEPI(DateTime beginDate, DateTime endDate)
        {
            string sql = "select case when GFR = EPI then '0' else '1' end as 구분, fin.*\n"
                          + "from("
                          + "select f.*\n"
                          + ", case PatientSex \n"
                          + "  when 'M' then CONVERT(numeric(5, 0), 141.0 * power(ValueMin, ValueA) * POWER(ValueMax, -1.209) * power(0.993, PatientAge)) \n"
                          + "  when 'F' then CONVERT(numeric(5, 0), 141.0 * POWER(ValueMin, ValueA) * POWER(ValueMax, -1.209) * POWER(0.993, PatientAge) * 1.018)\n"
                          + "  else '0' end as EPI\n"
                          + "from (\n"
                          + "select m.*, CONVERT(numeric(5, 3), Creatine / ValueK) as CdivK\n"
                          + ", case when CONVERT(numeric(5, 3), Creatine / ValueK) > 1.0 then 1.000 else CONVERT(numeric(5, 3), Creatine / ValueK) end as ValueMin\n"
                          + ", case when CONVERT(numeric(5, 3), Creatine / ValueK) > 1.0 then CONVERT(numeric(5, 3), Creatine / ValueK) else 1.000 end as ValueMax\n"
                          + "from (\n"
                          + "select a.LabRegDate, a.LabRegNo, a.PatientName, a.PatientAge, a.PatientSex\n"
                          + ", max(case when d.TestCode in ('11089', '33020', '33912', '33720') then CONVERT(float, d.TestResult01) else 0.0 end) as GFR\n"
                          + ", max(case when b.TestCode in ('11014', '33010', '33710') then convert(float, b.TestResult01) else 0.0 end) as Creatine\n"
                          + ", case a.PatientSex when 'M' then -0.411 when 'F' then -0.329 else 0.000 end as ValueA\n"
                          + ", case a.PatientSex when 'M' then 0.9 when 'F' then 0.7 else 0.0 end as ValueK\n"
                          + "from LabRegInfo as a\n"
                          + "inner join (select * from LabRegResult where TestCode in ('11089', '33020', '33912', '33720')) as d on a.LabRegDate = d.LabRegDate and a.LabRegNo = d.LabRegNo\n"
                          + "inner join LabRegResult as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo and b.TestCode in ('11014', '33010', '33710')\n"
                          + "inner join LabRegTest as c on b.LabRegDate = c.LabRegDate and b.LabRegNo = c.LabRegNo and b.TestCode = c.TestCode\n"
                          + "where a.LabRegDate between '" + beginDate.ToString("yyyyMMdd") + "' and '" + endDate.ToString("yyyyMMdd") + "'\n"
                          + "and a.CompCode = '01618'\n"
                          + "and c.TestStateCode = 'F'\n"
                          + "group by a.LabRegDate, a.LabRegNo, a.PatientName, a.PatientAge, a.PatientSex) as m) as f) as fin";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);

        }



        [Route("api/Diagnostic/GFRUpdate/eGFR")]
        public IHttpActionResult GeteGFR(DateTime beginDate, DateTime endDate, string option)
        {
            string sqlOption = string.Empty;
            string sql = string.Empty;
            if (option == "rabAll")
            {
                sqlOption = "";
            }
            else if (option == "rabNull")
            {
                sqlOption = "and c.TestStateCode != 'f' and (b.TestResultText is null or b.TestResultText = '')";
            }
            else if (option == "rabNotNull")
            {
                sqlOption = "and isnull(b.TestResultText, '') != ''";
            }

            sql = "select case when ISNULL(a.PatientAge, '') = '' or ISNULL(a.PatientSex, '') = '' or ISNULL(b.TestResultText, '') != '' then '0' else '1' end as 구분\n"
                          + ", a.LabRegDate, a.LabRegNo, e.CompName, a.PatientName, a.PatientAge, a.PatientSex, b.TestCode, d.TestStateShortName, b.TestResult01\n"
                          + ", replace(b.TestResultText, char(13) + char(10), '') as TestResultText\n"
                          + ", case when ISNULL(a.PatientAge, '') = '' or ISNULL(a.PatientSex, '') = '' then '나이/성별 확인'\n"
                          + "  else '*eGFR cysc : ' +\n"
                          + "       CONVERT(varchar, ROUND(133 * power((CONVERT(float, b.TestResult01) / 0.8), case when CONVERT(float, b.TestResult01) <= 0.8 then -0.499 else -1.3289 end)\n"
                          + "       * power(0.996, CONVERT(float, a.PatientAge)) * case when a.PatientSex = 'F' then 0.932 else 1 end, 0))\n"
                          + "       + ' mL/min/1.73m²' + char(13) + char(10) + char(13) + char(10)\n"
                          + "       + '*eGFR은 KDIGO(2012 CKD-EPI)에 따라 계산한 결과입니다.'\n"
                          + "  end as Result\n"
                          + "from LabRegInfo as a inner join LabRegResult as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                          + "inner join LabRegTest as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo and b.TestCode = c.TestCode\n"
                          + "inner join LabTestStateCode as d on c.TestStateCode = d.TestStateCode\n"
                          + "inner join ProgCompCode as e on a.CompCode = e.CompCode "
                          + "where a.LabRegDate between '" + beginDate.ToString("yyyyMMdd") + "' and '" + endDate.ToString("yyyyMMdd") + "'\n"
                          + "and a.CompCode in ('6496', '3719', '6384', '6886')\n"
                          + "and b.TestCode = '11317'\n"
                          + "and isnull(b.TestResult01, '') != ''\n"
                          + sqlOption;


            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);

        }




        [Route("api/Diagnostic/GFRUpdate/CMLDL")]
        public IHttpActionResult GetCMLDL(DateTime beginDate, DateTime endDate)
        {
            string sql = "select case when a.CM_LDL = a.Calc then  '0' else case when a.Calc = '99999' then '0' when a.Calc < 0 then '1' else '1' end\n"
                          + "  end as CK, a.* , case when a.Calc < 0 then '음수' when a.Calc = '99999' then '결과없음' else case when a.CM_LDL = a.Calc then ''\n"
                          + "	   else '불일치' end   end as 구분, '' as 공단\n"
                          + "from (select a.LabRegDate, a.LabRegNo, a.PatientName, a.T_Chol, a.TG, a.HDL_Chol, a.CM_LDL\n"
                          + ", convert(numeric(12, 0), case when a.T_Chol != '' and a.TG != '' and a.HDL_Chol != ''\n"
                          + "  then case when a.TG < 400 then CONVERT(float, a.T_Chol) - CONVERT(float, a.TG) * 0.2 - CONVERT(float, a.HDL_Chol)\n"
                          + "       when a.TG >= 400 then CONVERT(float, a.T_Chol) - CONVERT(float, a.TG) * 0.16 - CONVERT(float, a.HDL_Chol) end\n"
                          + "  else 99999 end) as Calc\n"
                          + "from(select a.LabRegDate, a.LabRegNo, a.PatientName\n"
                          + ", max(case b.TestCode when '11059' then b.TestResult01 else '' end) as T_Chol\n"
                          + ", max(case b.TestCode when '11074' then b.TestResult01 else '' end) as TG\n"
                          + ", max(case b.TestCode when '11064' then b.TestResult01 else '' end) as HDL_Chol\n"
                          + ", max(case b.TestCode when '11702' then b.TestResult01 else '' end) as CM_LDL\n"
                          + "from LabRegInfo as a inner join LabRegResult as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                          + "where a.LabRegDate between '" + beginDate.ToString("yyyyMMdd") + "' and '" + endDate.ToString("yyyyMMdd") + "'\n"
                          + "and b.TestCode in ('11059', '11074', '11064', '11702')\n"
                          + "group by a.LabRegDate, a.LabRegNo, a.PatientName) as a\n"
                          + "where a.CM_LDL != '') as a\n"
                          + "Union all\n"
                          + "select case when a.CM_LDL = a.Calc then  '0' else case when a.Calc = '99999' then '0' when a.Calc < 0 then '1' else '1' end\n"
                          + "  end as CK, a.* , case when a.Calc < 0 then '음수' when a.Calc = '99999' then '결과없음' else case when a.CM_LDL = a.Calc then ''\n"
                          + "	   else '불일치' end   end as 구분, '공단' as 공단\n"
                          + "from (select a.LabRegDate, a.LabRegNo, a.PatientName, a.T_Chol, a.TG, a.HDL_Chol, a.CM_LDL\n"
                          + ", convert(numeric(12, 0), case when a.T_Chol != '' and a.TG != '' and a.HDL_Chol != ''\n"
                          + "  then case when a.TG < 400 then CONVERT(float, a.T_Chol) - CONVERT(float, a.TG) * 0.2 - CONVERT(float, a.HDL_Chol)\n"
                          + "       when a.TG >= 400 then CONVERT(float, a.T_Chol) - CONVERT(float, a.TG) * 0.16 - CONVERT(float, a.HDL_Chol) end\n"
                          + "  else 99999 end) as Calc\n"
                          + "from(select a.LabRegDate, a.LabRegNo, a.PatientName\n"
                          + ", max(case b.TestCode when '33003' then b.TestResult01 else '' end) as T_Chol\n"
                          + ", max(case b.TestCode when '33005' then b.TestResult01 else '' end) as TG\n"
                          + ", max(case b.TestCode when '33004' then b.TestResult01 else '' end) as HDL_Chol\n"
                          + ", max(case b.TestCode when '33019' then b.TestResult01 else '' end) as CM_LDL\n"
                          + "from LabRegInfo as a inner join LabRegResult as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                          + "where a.LabRegDate between '" + beginDate.ToString("yyyyMMdd") + "' and '" + beginDate.ToString("yyyyMMdd") + "'\n"
                          + "and b.TestCode in ('33003', '33005', '33004', '33019')\n"
                          + "group by a.LabRegDate, a.LabRegNo, a.PatientName) as a\n"
                          + "where a.CM_LDL != '') as a order by a.LabRegDate, a.LabRegNo";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }



        [Route("api/Diagnostic/GFRUpdate/GFR")]
        public IHttpActionResult GetGFR(DateTime beginDate, DateTime endDate)
        {
            string sql = "select * from\n"
                          + "(select case when isnull(a.TestResult01, '') = '' and d.PatientAge != 0 then '1' else '0' end as 구분\n"
                          + ", a.LabRegDate, a.LabRegNo, d.PatientName, d.PatientAge, d.PatientSex\n"
                          + ", b.TestResult01 as Creatinine결과, a.TestResult01 as GFR결과\n"
                          + ", case when d.PatientSex = '' or d.PatientAge = 0 then '수치확인'\n"
                          + "       else convert(varchar, ROUND(175.0 * POWER(b.TestResult01, -1.154) * POWER(convert(float, d.PatientAge), -0.203) * case d.PatientSex when 'F' then 0.742 when 'M' then 1 else 0 end, 0)) end as GFR계산\n"
                          + ", ROW_NUMBER() over(partition by a.LabRegDate, a.LabRegNo order by case when(a.TestCode = '11089' and b.TestCode = '11014') or((a.TestCode = '33020' or a.TestCode = '33020') and b.TestCode = '33010') then '1'\n"
                          + "       when(a.TestCode = '11089' and b.TestCode = '33010') or(a.TestCode = '33020' and b.TestCode = '11014') then '2'\n"
                          + "	   else '3' end) as TestMatch\n"
                          + ", case when a.cntTest > 1 and b.cntTest > 1 then '중복(CRE, GFR)'\n"
                          + "       when a.cntTest > 1 then '중복(GFR)'\n"
                          + "       when b.cntTest > 1 then '중복(cre)'\n"
                          + "       when a.TestCode = '33020' then '공단' when a.TestCode = '33912' then '공단(특정)' when a.TestCode = '33720' then '공단(출력)' else '' end as 공단\n"
                          + "from(select LabRegDate, LabRegNo, TestCode, TestResult01\n"
                          + ", ROW_NUMBER() over(partition by LabRegDate, LabRegNo order by case when TestCode = '11089' then 1 when TestCode in ('33020', '33720') then 2 else 3 end) as seq\n"
                          + ", count(TestCode) over(partition by LabRegDate, LabRegNo) as cntTest\n"
                          + "from LabRegResult\n"
                          + "where TestCode in ('11089', '33020', '33912', '33720')\n"
                          + "and LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "') as a\n"
                          + "inner join\n"
                          + "(select LabRegDate, LabRegNo, TestCode, TestResult01\n"
                          + ", ROW_NUMBER() over(partition by LabRegDate, LabRegNo order by case when TestCode = '11014' then 1 when TestCode = '33010' then 2 else 3 end) as seq\n"
                          + ", count(TestCode) over(partition by LabRegDate, LabRegNo) as cntTest\n"
                          + "from LabRegResult\n"
                          + "where TestCode in ('11014', '33010', '33710', '33910', '33710')\n"
                          + "and LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "') as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                          + "inner join LabRegTest as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo and b.TestCode = c.TestCode and c.TestStateCode = 'F'\n"
                          + "inner join LabRegInfo as d on a.LabRegDate = d.LabRegDate and a.LabRegNo = d.LabRegNo) as f\n"
                          + "where f.TestMatch = '1'";
           
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


    }
}