using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    public class ResultPivotController : ApiController
    {

        [Route("api/Diagnostic/ResultPivot/OrderHotCode")]
        public IHttpActionResult GetOrderHotCode(string memberID)
        {
            string sql;
            sql = "select OrderHotCode as 코드, OrderHotName as 코드명 from LabOrderHotCode where MemberID = '" + memberID + "'";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/Diagnostic/ResultPivot")]
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string labRegNo, string targetLabRegNo, string editvalue, string memberID)
        {
            string sql;
            string sqlRegNoOption = string.Empty;
            if (labRegNo != null && labRegNo != string.Empty)
            {
                //sql_RegNo = "and a.LabRegNo >= ''" + txtSNo.Text + "'' ";
                sqlRegNoOption = sqlRegNoOption + "and a.LabRegNo >= ''" + labRegNo + "'' ";
            }

            if (targetLabRegNo != null && targetLabRegNo != string.Empty)
            {
                //sql_RegNo = sql_RegNo + "and a.LabRegNo <= ''" + txtTNo.Text + "''";
                sqlRegNoOption = sqlRegNoOption + "and a.LabRegNo <= ''" + targetLabRegNo + "''";
            }

            sql = "declare @pvCul as varchar(max)\n"
                     + "declare @fQuery as varchar(max)\n"
                     + "set @pvCul = ''\n"
                     + "select @pvCul = @pvCul + '[' + c.TestDisplayName + '], '\n"
                     + "from (select OrderCode from LabOrderHotProfile\n"
                     + "where MemberID = '" + memberID + "' and OrderHotCode = '" + editvalue + "') as a inner join View_LabOrderTestSub as b on a.OrderCode = b.OrderCode\n"
                     + "inner join LabTestCode as c on b.TestSubCode = c.TestCode\n"
                     + "order by c.TestSeqNo\n"
                     + "set @pvCul = LEFT(@pvCul, LEN(@pvCul) - 1)\n"
                     + "set @fQuery = 'select * from\n"
                     + "(select a.LabRegDate as 접수일, a.LabRegNo as 접수번호, d.CompCode as 거래처코드, e.CompName as 거래처명, d.PatientName as 수진자명, d.PatientAge as 나이, d.PatientSex as 성별, c.TestDisplayName, a.TestResult01\n"
                     + "from LabRegResult as a \n"
                     + "inner join (select OrderCode from LabOrderHotProfile\n"
                     + "where MemberID = ''" + memberID + "'' and OrderHotCode = ''" + editvalue + "'') as b on a.TestCode = b.OrderCode\n"
                     + "inner join LabTestCode as c on a.TestSubCode = c.TestCode\n"
                     + "inner join LabRegInfo as d on a.LabRegDate = d.LabRegDate and a.LabRegNo = d.LabRegNo\n"
                     + "inner join ProgCompCode as e on d.CompCode = e.CompCode\n"
                     + "where a.LabRegDate between ''" + beginDate.ToString("yyyy-MM-dd") + "'' and ''" + endDate.ToString("yyyy-MM-dd") + "''\n"
                     + sqlRegNoOption
                     + ") as PV\n"
                     + "pivot\n"
                     + "(\n"
                     + "max(PV.TestResult01)\n"
                     + "for TestDisplayName\n"
                     + "in (' + @pvCul + ')\n"
                     + ") as Result'\n"
                     + "exec(@fQuery)";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

    }
}