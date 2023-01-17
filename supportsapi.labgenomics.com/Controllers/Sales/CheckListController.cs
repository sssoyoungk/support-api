using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System.Web.Http;
using System.Web.Http.Cors;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [SupportsAuth]
    [EnableCors(origins: "*", headers: "*", methods: "*", PreflightMaxAge = 28800)]
    [Route("api/Sales/Checklist")]
    public class CheckListController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(string memberId, bool isOwnOrder)
        {
            string sql;

            sql =
                "SELECT\r\n" +
                "    lro.LabRegDate, lro.LabRegNo, lri.CompCode, pcc.CompName, lri.PatientName, lri.PatientAge, lri.PatientSex,\r\n" +
                "    lro.OrderCode, loc.OrderDisplayName, lro.SampleCode, lsc.SampleName, lro.RegistMemberID, pm.MemberName, lro.RegistTime\r\n" +
                "FROM LabRegOrder lro\r\n" +
                "JOIN LabRegInfo lri\r\n" +
                "ON lri.LabRegDate = lro.LabRegDate\r\n" +
                "AND lri.LabRegNo = lro.LabRegNo\r\n" +
                "JOIN LabOrderCode loc\r\n" +
                "ON lro.OrderCode = loc.OrderCode\r\n" +
                "LEFT OUTER JOIN LabSampleCode lsc\r\n" +
                "ON lsc.SampleCode = lro.SampleCode\r\n" +
                "JOIN ProgCompCode pcc\r\n" +
                "ON pcc.CompCode = lri.CompCode\r\n" +
                "JOIN ProgMember pm\r\n" +
                "ON pm.MemberID = lro.RegistMemberID\r\n" +
                "WHERE IsOrderCheck = 0\r\n";
            if (isOwnOrder)
            {
                sql += $"AND lro.RegistMemberID = '{memberId}'\r\n";
            }

            sql += "ORDER BY lro.LabRegDate, lro.LabRegNo, lro.OrderCode";
            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        // POST api/<controller>
        public IHttpActionResult Post([FromBody]JArray arrRequest)
        {
            foreach (JObject objRequest in arrRequest)
            {
                //LabRegTest 등록                

                //LabRegReport 등록

                //LabRegResult 등록
                
                //LabRegOrderTest 삭제

                //LabRegOrder에 체크리스 등록완료 플래그 처리
            }
            return Ok();
        }
    }
}