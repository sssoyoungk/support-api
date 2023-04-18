using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    public class WorkListController : ApiController
    {


        [Route("api/Diagnostic/WorkList/WorkReportCode")]
        public IHttpActionResult GetWorkReportCode(string workReportCode)
        {
            string sql;
            sql = $"select WorkReportCode as 코드, " +
                $"WorkReportName as 형식명 from LabWorkReport " +
                $"where WorkReportCode Like 'L%' OR WorkReportCode = '{workReportCode}' ";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);

        }

        [Route("api/Diagnostic/WorkList/WorkCode")]
        public IHttpActionResult GetWorkCode()
        {
            string sql;
            sql = "select WorkCode as 워크코드, WorkName as 워크리스트명, case when left(WorkCode, 1) != 'L' then 'M' + WorkCode else WorkCode end as seq " +
                "from LabWorkCode " +
                "where WorkCode like 'L%' or WorkName like '%코로나%' or WorkCode = 'm-001-5' order by seq";


            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);

        }



        [Route("api/Diagnostic/WorkList/MSMSList")]
        public IHttpActionResult GetMSMSList(DateTime startLabRegDate, DateTime endLabRegDate,string startLabRegNo, string endLabRegNo, string groupCode, string workCode)
        {

            string option = string.Empty;
            if (startLabRegNo != null && startLabRegNo != string.Empty)
            {
                option = "and a.LabRegNo >= '" + startLabRegNo + "'\n";
            }
            if (endLabRegNo != null && endLabRegNo != string.Empty)
            {
                option = option + "and a.LabRegNo <= '" + endLabRegNo + "'\n";
            }

            string sql;
            sql = "select distinct ROW_NUMBER() over(order by g.CustomValue01, a.LabRegDate, a.LabRegNo) as seq, '' as 번호\n"
                         + ", a.LabRegDate as 접수일, a.LabRegNo as 접수번호, d.CompName as 거래처명, a.PatientName as 수진자명, f.OrderDisplayName as 의뢰항목명, h.CompMngName as 관리그룹\n"
                         //+ ", case when g.CustomValue01 is not null then CONVERT(varchar, CONVERT(date, LEFT(g.CustomValue01, 8))) + ' ' + Right(g.CustomValue01, 5) else '' end as 이전검사일\n"
                         + ", g.CustomValue01 + case when g.CustomValue02 != '' then ' ' + g.CustomValue02 else '' end as 이전검사일\n"
                         + ", '' as 재검, '' as 결과일자, '' as 결과, '' as 재채혈결과, '' as 비고\n"
                         + "from LabRegInfo as a inner join LabRegTest as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                         + "inner join LabWorkTest as c on c.TestCode = b.TestCode\n"
                         + "inner join ProgCompCode as d on a.CompCode = d.CompCode\n"
                         + "inner join LabTestCode as e on b.TestCode = e.TestCode\n"
                         + "inner join LabOrderCode as f on b.OrderCode = f.OrderCode\n"
                         + "inner join ProgCompMngCode as h on d.CompMngCode = h.CompMngCode\n"
                         + "inner join ProgAuthGroupAccessComp as i on a.CompCode = i.CompCode and i.AuthGroupCode = '" + groupCode + "'\n"
                         + "left outer join LabRegCustom as g on a.LabRegDate = g.LabRegDate and a.LabRegNo = g.LabRegNo and g.CustomCode in ('0430', 'LC00601')\n"
                         + "where a.LabRegDate between '" + startLabRegDate.ToString("yyyy-MM-dd") + "' and '" + endLabRegDate.ToString("yyyy-MM-dd") + "'\n"
                         + option
                         + "and c.WorkCode = '" + workCode + "'\n"
                         + "order by seq";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


        [Route("api/Diagnostic/WorkList/NSTList")]
        public IHttpActionResult GetNSTList(DateTime startLabRegDate, DateTime endLabRegDate, string startLabRegNo, string endLabRegNo, string groupCode, string workCode, string reportCode)
        {

            string labRegNoOption = string.Empty;
            string reportOption = string.Empty;
            if (startLabRegNo != null && startLabRegNo != string.Empty)
            {
                labRegNoOption = "and a.LabRegNo >= '" + startLabRegNo + "'\n";
            }
            if (endLabRegNo != null && endLabRegNo != string.Empty)
            {
                labRegNoOption = labRegNoOption + "and a.LabRegNo <= '" + endLabRegNo + "'\n";
            }

            if (reportCode == "LWL01")
            {
                reportOption = "and b.TestStateCode = 'W'\n";
            }
            else
            {
                reportOption = "and b.TestStateCode != 'W'\n";
            }



            string sql = "select '0' as 선택, convert(varchar(10), a.LabRegDate, 23) as 접수일, right('00000' + convert(varchar(5), a.LabRegNo), 5) as 접수번호, d.CompName as 거래처명, a.PatientName as 수진자명\n"
                         + ", h.TestStateShortName as 상태, b.TestCode as 검사코드, f.TestDisplayName as 검사항목명\n"
                         + ", j.TestSubCode as 부속코드, k.TestDisplayName as 부속항목명, j.TestResult01 as 결과\n"
                         //+ ", case when g.CustomValue01 is not null then CONVERT(varchar, CONVERT(date, LEFT(g.CustomValue01, 8))) else '' end as 이전검사일\n"
                         + ", g.CustomValue01 + case when g.CustomValue02 != '' then ' ' + g.CustomValue02 else '' end as 이전검사일\n"
                         + "from LabRegInfo as a inner join LabRegTest as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                         + reportOption
                         + "inner join LabWorkTest as c on c.TestCode = b.TestCode\n"
                         + "inner join LabRegResult as j on a.LabRegDate = j.LabRegDate and a.LabRegNo = j.LabRegNo and b.TestCode = j.TestCode\n"
                         + "inner join LabTestCode as k on j.TestSubCode = k.TestCode\n"
                         + "inner join ProgCompCode as d on a.CompCode = d.CompCode\n"
                         + "inner join LabTestCode as e on b.TestCode = e.TestCode\n"
                         + "inner join LabTestCode as f on b.TestCode = f.TestCode\n"
                         + "inner join LabTestStateCode as h on b.TestStateCode = h.TestStateCode\n"
                         //+ "inner join ProgAuthGroupAccessComp as i on a.CompCode = i.CompCode and i.AuthGroupCode = '" + frmSupportMain.strGroupCode + "'\n"
                         + "left outer join LabRegCustom as g on a.LabRegDate = g.LabRegDate and a.LabRegNo = g.LabRegNo and g.CustomCode in ('0430', 'LC00601')\n"
                         + "where a.LabRegDate between '" + startLabRegDate.ToString("yyyy-MM-dd") + "' and '" + endLabRegDate.ToString("yyyy-MM-dd") + "'\n"
                         + labRegNoOption
                         + "and c.WorkCode = '" + workCode + "'\n"
                         + "and j.TestSubCode in ('11302', '13147', '13094', '13300', '13325', '13326')\n"
                         + "--and b.TestCode != j.TestSubCode\n"
                         + "order by a.LabRegDate, a.LabRegNo, k.TestSeqNo";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


        [Route("api/Diagnostic/WorkList/MicrobeList")]
        public IHttpActionResult GetMicrobeList(DateTime startLabRegDate, DateTime endLabRegDate, string startLabRegNo, string endLabRegNo, string groupCode, string workCode)
        {

            string labRegNoOption = string.Empty;
            string reportOption = string.Empty;
            if (startLabRegNo != null && startLabRegNo != string.Empty)
            {
                labRegNoOption = "and a.LabRegNo >= '" + startLabRegNo + "'\n";
            }
            if (endLabRegNo != null && endLabRegNo != string.Empty)
            {
                labRegNoOption = labRegNoOption + "and a.LabRegNo <= '" + endLabRegNo + "'\n";
            }
            

            string sql = "select '0' as 선택, convert(varchar(10), a.LabRegDate, 23) as 접수일, right('00000' + convert(varchar(5), a.LabRegNo), 5) as 접수번호, d.CompName as 거래처명, l.CompGroupName as 영업소, a.PatientName as 수진자명\n"
                         + ", h.TestStateShortName as 상태, b.TestCode as 검사코드, f.TestDisplayName as 검사항목명\n"
                         + ", j.TestSubCode as 부속코드, k.TestDisplayName as 부속항목명, j.TestResult01 as 결과, i.SampleName as 검체코드, k.TestShortName as 검체타입 \n"
                         //+ ", case when g.CustomValue01 is not null then CONVERT(varchar, CONVERT(date, LEFT(g.CustomValue01, 8))) else '' end as 이전검사일\n"
                         + ", g.CustomValue01 + case when g.CustomValue02 != '' then ' ' + g.CustomValue02 else '' end as 이전검사일\n"
                         + "from LabRegInfo as a inner join LabRegTest as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                         //+ sql_state
                         + "inner join LabWorkTest as c on c.TestCode = b.TestCode\n"
                         + "inner join LabRegResult as j on a.LabRegDate = j.LabRegDate and a.LabRegNo = j.LabRegNo and b.TestCode = j.TestCode\n"
                         + "inner join LabTestCode as k on j.TestSubCode = k.TestCode\n"
                         + "inner join ProgCompCode as d on a.CompCode = d.CompCode\n"
                         + "inner join ProgCompGroupCode l on d.CompGroupCode = l.CompGroupCode\n"
                         + "inner join LabTestCode as e on b.TestCode = e.TestCode\n"
                         + "inner join LabTestCode as f on b.TestCode = f.TestCode\n"
                         + "inner join LabTestStateCode as h on b.TestStateCode = h.TestStateCode\n"
                         + "inner join LabSampleCode as i on b.SampleCode = i.SampleCode \n"
                         + "inner join ProgAuthGroupAccessComp as m on a.CompCode = m.CompCode and m.AuthGroupCode = '" + groupCode + "'\n"
                         + "left outer join LabRegCustom as g on a.LabRegDate = g.LabRegDate and a.LabRegNo = g.LabRegNo and g.CustomCode in ('0430', 'LC00601')\n"
                         + "where a.LabRegDate between '" + startLabRegDate.ToString("yyyy-MM-dd") + "' and '" + endLabRegDate.ToString("yyyy-MM-dd") + "'\n"
                         + labRegNoOption
                         + "and c.WorkCode = '" + workCode + "'\n"
                         + "--and b.TestCode != j.TestSubCode\n"
                         + "order by a.LabRegDate, a.LabRegNo, k.TestSeqNo";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/Diagnostic/WorkList/CovidList")]
        public IHttpActionResult GetCovidList(DateTime startLabRegDate, DateTime endLabRegDate, string startLabRegNo, string endLabRegNo, string groupCode, string workCode, string reportCode, string option, string blank)
        {
            string sql;
            string labRegNoOption = string.Empty;
            string reportOption = string.Empty;
            if (startLabRegNo != null && startLabRegNo != string.Empty)
            {
                labRegNoOption = "and a.LabRegNo >= '" + startLabRegNo + "'\n";
            }
            if (endLabRegNo != null && endLabRegNo != string.Empty)
            {
                labRegNoOption = labRegNoOption + "and a.LabRegNo <= '" + endLabRegNo + "'\n";
            }
            

            string option1 = "", option2 = "", option3 = "";
            
           
            if (option == "No0") //No0
            {
                option1 = "and isnull(b.WorkTestNo, '') != ''\n";
                option2 = "b.WorkTestNo, ";
            }
            else if (option == "No1") //No1
            {
                option1 = "and isnull(b.WorkTestNo, '') = ''\n";
            }
            else //"All"
            {
                option1 = "";
            }

            if (blank == "Blank")//Blank
            {
                option3 = "and (isnull(g.CustomValue01, '') != '' or h.CustomShortName = '1번')\n";
            }
            else
            {
                option3 = "";
            }

            if (reportCode == "LWL03")
            {
                sql = "select '0' as 선택, convert(varchar(10), a.LabRegDate, 23) as 접수일, a.LabRegNo as 접수번호, a.CompCode as 거래처코드, d.CompName as 거래처명, a.PatientName as '수진자명(접수)', a.PatientChartNo as 차트번호, b.TestCode as 검사코드\n"
                         + ", e.TestDisplayName as 검사명, f.TestStateShortName as 상태, h.CustomShortName as 구분\n"
                         + ", Case when h.CustomCode = '5401' and g.CustomValue01 is null then a.PatientName else g.CustomValue01 end as '수진자명(추가)'\n"
                         + ", Case when h.CustomCode = '5401' and g.CustomValue02 is null then a.PatientJuminNo01 + '-'+ master.dbo.AES_DecryptFunc(a.PatientJuminNo02, 'labge$%#!dleorms') else g.CustomValue02 end as 주민번호, k.PhoneNo as 전화번호\n"
                         + ", a.LabRegPatientID, j.CompGroup02 as 영업소, convert(varchar(1), isnull(b.WorkNumName, '')) + isnull(right('00000' + convert(varchar, b.WorkTestNo), 5), '') as WorkNo\n"
                         + "from LabRegInfo as a inner join LabRegTest as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                         + "inner join LabWorkTest as c on c.WorkCode = '" + workCode + "' and b.TestCode = c.TestCode\n"
                         + "inner join ProgCompCode as d on a.CompCode = d.CompCode\n"
                         + "inner join LabTestCode as e on b.TestCode = e.TestCode\n"
                         + "inner join LabTestStateCode as f on b.TestStateCode = f.TestStateCode\n"
                         + "inner join ProgCompGroupCode as j on d.CompGroupCode = j.CompGroupCode\n"
                         //+ "inner join ProgAuthGroupAccessComp as i on a.CompCode = i.CompCode and i.AuthGroupCode = '" + frmSupportMain.strGroupCode + "'\n"
                         //+ "left outer join LabRegCustom as g on a.LabRegDate = g.LabRegDate and a.LabRegNo = g.LabRegNo\n"
                         //+ "left outer join LabCustomCode as h on g.CustomCode = h.CustomCode\n"
                         + "inner join LabCustomCode as h on h.CustomCode in ('5401', '5402', '5403', '5404', '5405')\n"
                         + "left outer join LabRegCustom as g on a.LabRegDate = g.LabRegDate and a.LabRegNo = g.LabRegNo and h.CustomCode = g.CustomCode\n"
                         + "left outer join Covid19Order as k on  dbo.FUNC_GetIdxDataLikeSplit(g.CustomValue02, 2, ';') = k.SampleNo\n"
                         + "where a.LabRegDate between '" + startLabRegDate.ToString("yyyy-MM-dd") + "' and '" + endLabRegDate.ToString("yyyy-MM-dd") + "'\n"
                         + "and a.compcode	in (select CompCode from ProgAuthGroupAccessComp where AuthGroupCode = '" + groupCode + "')\n"
                         + labRegNoOption + option1 + option3
                         + "order by " + option2 + "a.LabRegDate, a.LabRegNo, h.CustomCode";
            }
            else
            {
                sql = "select '0' as 선택, convert(varchar(10), a.LabRegDate, 23) as 접수일, a.LabRegNo as 접수번호, a.CompCode as 거래처코드, d.CompName as 거래처명, a.PatientName as '수진자명(접수)', a.PatientChartNo as 차트번호, b.TestCode as 검사코드\n"
                         + ", e.TestShortName as 검사명, f.TestStateShortName as 상태, '' as 구분, a.PatientName as '수진자명(추가)'\n"
                         + ", a.PatientJuminNo01 + '-'+ master.dbo.AES_DecryptFunc(a.PatientJuminNo02, 'labge$%#!dleorms') as 주민번호, a.LabRegPatientID, convert(varchar(1), isnull(b.WorkNumName, '')) + isnull(right('00000' + convert(varchar, b.WorkTestNo), 5), '') as WorkNo\n"
                         + "from LabRegInfo as a inner join LabRegTest as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
                         + "inner join LabWorkTest as c on c.WorkCode = '" + workCode + "' and b.TestCode = c.TestCode\n"
                         + "inner join ProgCompCode as d on a.CompCode = d.CompCode\n"
                         + "inner join LabTestCode as e on b.TestCode = e.TestCode\n"
                         + "inner join LabTestStateCode as f on b.TestStateCode = f.TestStateCode\n"
                         + "inner join ProgAuthGroupAccessComp as i on a.CompCode = i.CompCode and i.AuthGroupCode = '" + groupCode + "'\n"
                         + "where a.LabRegDate between '" + startLabRegDate.ToString("yyyy-MM-dd") + "' and '" + endLabRegDate.ToString("yyyy-MM-dd") + "'\n"
                         + labRegNoOption + option1
                         + "order by " + option2 + "a.LabRegDate, a.LabRegNo";
            }


            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }



        [Route("api/Diagnostic/WorkList/Covid19")]
        public IHttpActionResult Put([FromBody]JObject request)
        {
            try
            {
                string sql = string.Empty;

                sql = "Select * from LabRegCustom where LabRegDate = '" + request["LabRegDate"] + "' " +
                    "and LabRegNo = '" + request["LabRegNo"] + "' and CustomCode = '" + request["CustomCode"] + "'";

                var dtTemp = LabgeDatabase.SqlToDataTable(sql);


                if (dtTemp.Rows.Count > 0)
                {
                    sql = "update LabRegCustom set CustomValue01 = '" + request["CustomValue01"] + "', CustomValue02 = '" + request["CustomValue02"] + "' where LabRegDate = '" + request["LabRegDate"] + "' and LabRegNo = '" + request["LabRegNo"] + "' and CustomCode = '" + request["CustomCode"] + "'";
                }
                else
                {
                    sql = "insert into LabRegCustom\n"
                           + "select NEWID(), '" + request["LabRegDate"] + "', '" + request["LabRegNo"] + "', '" + request["CustomCode"] + "', '" + request["CustomValue01"] + "', '" + request["CustomValue02"] + "', '16777215', '0', CURRENT_TIMESTAMP, '" + request["MemberID"] + "', CURRENT_TIMESTAMP, '" + request["MemberID"] + "'";
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



        [Route("api/Diagnostic/WorkList/Covid19")]
        public IHttpActionResult Delete(string labRegDate, string labRegNo, string customCode)
        {
            try
            {

                string sql = "delete LabRegCustom where LabRegDate = '" + labRegDate + "' and LabRegNo = '" + labRegNo + 
                    "' and CustomCode = '" + customCode + "'";

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