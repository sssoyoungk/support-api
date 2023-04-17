using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    public class RemarkPartReviewController : ApiController
    {

        [Route("api/Diagnostic/RemarkPartReview/PartCode")]
        public IHttpActionResult Get()
        {
            string sql;
            sql = "select PartCode as 코드, PartName as 파트명 from LabPartCode where PartSeqNo between '10' and '100' order by PartSeqNo";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }




        [Route("api/Diagnostic/RemarkPartReview/PartOpinion")]

        public IHttpActionResult GetPartOpinion(DateTime beginDate, DateTime endDate, string partCode)
        {
            string sql = string.Empty;
            string sqlTGResult = string.Empty;
            string sqlsub1 = string.Empty;
            string sqlsub2 = string.Empty;
            string sqlsub3 = string.Empty;
            string sqlsub4 = string.Empty;
            string sqlWhere = string.Empty;


            if (partCode != "")
            {
                sqlWhere = "and b.PartCode = '" + partCode + "'  ";
            }

            if (partCode.ToString() == "11" || partCode.ToString() == "28")
            {
                sqlTGResult = "select * into #TG_Result\r\n"
                             + "from(select LabRegDate, LabRegNo, TestResult01, TestResult02, TestResultAbn\r\n"
                             + ", ROW_NUMBER() over(partition by CONVERT(varchar, labregdate) + '_' + CONVERT(varchar, labregno) order by LabRegDate, LabRegno) as seq\r\n"
                             + "from LabRegResult\r\n"
                             + "where TestCode in ('11074', '33005')\r\n"
                             + "and LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'\r\n"
                             + "group by LabRegDate, LabRegNo, TestResult01, TestResult02, TestResultAbn) as a\r\n\r\n";
                sqlsub1 = ", h.TestResult01 as TG결과, h.TestResultAbn as 판정\r\n";
                sqlsub2 = "left outer join #TG_Result as h on a.LabRegDate = h.LabRegDate and a.LabRegNo = h.LabRegNo and h.seq = '1'\r\n";
                sqlsub3 = ", h.TestResult01, h.TestResultAbn\r\n";
                sqlsub4 = "\r\n\r\ndrop Table #TG_Result";
            }
            else
            {
                sqlsub1 = "";
                sqlsub2 = "";
                sqlsub3 = "";
                sqlsub4 = "";
            }

            sql = sqlTGResult + "select case when COUNT(f.TestCode) = 0 then '1' else '0' end as 선택, a.LabRegDate as 접수일, a.LabRegNo as 접수번호, d.CompName as 거래처명, a.PatientName as 수진자명, a.PatientAge as 나이, a.PatientSex as 성별, g.MemberName as 등록자명, c.PartName as 파트명, b.RemarkText as 파트소견 "
                     + sqlsub1
                     + ", COUNT(f.TestCode) as 검사항목수, b.PartCode "
                     + "from LabRegInfo as a inner join LabRegRemarkPart as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo "
                     + "inner join LabPartCode as c on b.PartCode = c.PartCode inner join ProgCompCode as d on a.CompCode = d.CompCode "
                     + "left outer join LabRegTest as e on a.LabRegDate = e.LabRegDate and a.LabRegNo = e.LabRegNo "
                     + "left outer join LabTestCode as f on e.TestCode = f.TestCode and b.PartCode = f.PartCode "
                     + sqlsub2
                     + "inner join ProgMember as g on a.RegistMemberID = g.MemberID "
                     + "where a.LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "' and isnull(b.RemarkText, '') != '' "
                     + sqlWhere
                     + "group by a.LabRegDate, a.LabRegNo, d.CompName, a.PatientName, a.PatientAge, a.PatientSex, g.MemberName, c.PartName, b.RemarkText, b.PartCode  "
                     + sqlsub3
                     + "order by a.LabRegDate, a.LabRegNo " + sqlsub4;

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }




        [Route("api/Diagnostic/RemarkPartReview/ResultText")]

        public IHttpActionResult GetResultText(DateTime beginDate, DateTime endDate, string partCode)
        {

            string sql = string.Empty;
            string sqlWhere = string.Empty;


            if (partCode != "")
            {
                if (partCode == "11" || partCode == "28")
                {
                    sqlWhere = "'11', '28'";
                }
                else
                {
                    sqlWhere = "'" + partCode + "'";
                }
            }
            sql = "select '0' as C, r.LabRegDate as 접수일, r.LabRegNo as 접수번호, r.MachineCode as 장비코드, r.TestResultText as 서술결과, r.cntResultText as 소견수\n"
                     + "from (select a.LabRegDate, a.LabRegNo, a.MachineCode, a.TestResultText, count(a.TestResultText) as cntResultText\n"
                     + "from LabRegResult as a inner join LabTestCode as b on a.TestSubCode = b.TestCode\n"
                     + "where a.LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'\n"
                     + "and b.PartCode in (" + sqlWhere + ")\n"
                     //+ "and a.MachineCode in ('CM061', 'CM062', 'CM084', 'CM085')\n"
                     + "and isnull(a.TestResultText, '') != ''\n"
                     + "group by a.LabRegDate, a.LabRegNo, a.TestResultText, a.MachineCode) as r\n"
                     + "where r.cntResultText > 1\n"
                     + "order by r.LabRegDate, r.LabRegNo";


            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }




        [Route("api/Diagnostic/RemarkPartReview/ResultTextAll")]

        public IHttpActionResult GetResultTextAll(DateTime beginDate, DateTime endDate, string partCode)
        {

            string sql = string.Empty;
            string sqlWhere = string.Empty;

            if (partCode != "")
            {
                if (partCode == "11" || partCode == "28")
                {
                    sqlWhere = "'11', '28'";
                }
                else
                {
                    sqlWhere = "'" + partCode + "'";
                }
            }
            sql = "select case when ResultLength < 14 then '1' else '0' end as C,LabRegDate as 접수일, LabRegNo as 접수번호, TestSubCode as 검사코드, TestDisplayName as 검사명, TestResultText as 서술결과, ResultLength as 길이\n"
                     + "from\n"
                     + "(select a.LabRegDate, a.LabRegNo, a.TestSubCode, b.TestDisplayName, a.TestResultText, len(a.TestResultText) as ResultLength\n"
                     + "from LabRegResult as a inner join LabTestCode as b on a.TestSubCode = b.TestCode\n"
                     + "where LabRegDate between '" + beginDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'\n"
                     + "and PartCode in (" + sqlWhere + ")\n"
                     + "and isnull(a.TestResultText, '') != '') as r\n"
                     + "order by ResultLength, LabRegDate, LabRegNo";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }



        [Route("api/Diagnostic/RemarkPartReview")]
        public IHttpActionResult Put([FromBody]JObject request)
        {
            try
            {
                string sql = string.Empty;

                if (request["Mode"].ToString() == "ResultText")
                {
                    sql = "update LabRegResult set TestResultText = ''\n"
                           + "where LabRegDate = '" + request["LabRegDate"].ToString() + "'\n"
                           + "and LabRegNo = '" + request["LabRegNo"].ToString() + "'\n"
                           + "and MachineCode = '" + request["MachineCode"].ToString() + "'\n"
                           + "and TestResultText != ''";
                }
                else if (request["Mode"].ToString() == "ResultTextAll")
                {
                    sql = "update LabRegResult set TestResultText = ''\n"
                          + "where LabRegDate = '" + request["LabRegDate"].ToString() + "'\n"
                          + "and LabRegNo = '" + request["LabRegNo"].ToString() + "'\n"
                          + "and TestSubCode = '" + request["TestSubCode"].ToString() + "'\n"
                          + "and TestResultText != ''";
                }
                else
                {

                }



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

        [Route("api/Diagnostic/RemarkPartReview")]
        public IHttpActionResult Delete(string labRegDate, string labRegNo, string partCode)
        {
            try
            {
                string sql;
                sql = "delete LabRegRemarkPart " +
                    "where LabRegDate = '" + labRegDate + "' and LabRegNo = '" + labRegNo + "' " +
                    "and PartCode = '" + partCode + "'";
                Services.LabgeDatabase.ExecuteSql(sql);
                return Ok();
            }
            catch (Exception ex)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}