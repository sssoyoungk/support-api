using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System.Web.Http;
using System.Web.Http.Cors;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    [SupportsAuth]
    [EnableCors(origins: "*", headers: "*", methods: "*", PreflightMaxAge = 28800)]
    [Route("api/Sales/Ysr2000Mapping")]
    public class Ysr2000MappingController : ApiController
    {
        /// <summary>
        /// 보험코드 조회
        /// </summary>
        /// <param name="insureCode"></param>
        /// <returns></returns>
        [Route("api/Sales/Ysr2000Mapping/InsureCode")]
        public IHttpActionResult GetInsureCode(string insureCode)
        {
            string sql;
            sql = $"SELECT *\r\n" +
                  $"FROM View_UBCodeMapping\r\n" +
                  $"WHERE SUBSTRING(InsureCode, 1, 7) = '{insureCode}'\r\n" +
                  $"AND ISNULL(InsureCode, '') <> ''";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/Sales/Ysr2000Mapping/TestCode")]
        public IHttpActionResult GetFindTestCode(string testCode)
        {
            string sql;
            sql =
                $"SELECT\r\n" +
                $"    rtyc.TestCode AS OrderCode, ltc.TestDisplayName AS OrderName, lsc.SampleName AS Sample,\r\n" +
                $"    ltc.InsureCode, rtyc.UBCode AS UBCare\r\n" +
                $"FROM RsltTransYsr2000Code rtyc\r\n" +
                $"JOIN LabTestCode ltc\r\n" +
                $"ON rtyc.TestCode = ltc.TestCode\r\n" +
                $"LEFT OUTER JOIN LabSampleCode lsc\r\n" +
                $"ON rtyc.SampleCode = lsc.SampleCode\r\n" +
                $"WHERE rtyc.TestCode = '{testCode}'" +
                $"ORDER BY rtyc.TestCode\r\n";
            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        /// <summary>
        /// 의사랑 코드 조회
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult GetUBCode()
        {
            string sql;
            sql = "SELECT UBCode, CodeKind, TestName, OrderCode, TestCode, SampleCode, SampleName\r\n" +
                  "     , CASE WHEN ISNULL(LabgeOrderName, '') <> '' THEN LabgeOrderName\r\n" +
                  "            ELSE LabgeTestName\r\n" +
                  "       END AS LabgeTestName\r\n" +
                  "FROM\r\n" +
                  "(\r\n" +
                  "    SELECT rtyc.*\r\n" +
                  "         , (SELECT TestDisplayName FROM LabTestCode WHERE rtyc.OrderCode = TestCode) AS LabgeOrderName\r\n" +
                  "         , (SELECT TestDisplayName FROM LabTestCode WHERE rtyc.TestCode = TestCode) AS LabgeTestName\r\n" +
                  "         , (SELECT SampleName FROM LabSampleCode lsc WHERE rtyc.SampleCode = SampleCode) AS SampleName\r\n" +
                  "    FROM RsltTransYsr2000Code rtyc\r\n" +
                  ") AS Sub1\r\n" +
                  "ORDER BY UBCode";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        /// <summary>
        /// 의사랑 코드 조회
        /// </summary>
        /// <param name="ubCode"></param>
        /// <returns></returns>
        public IHttpActionResult GetUBCode(string ubCode)
        {
            string sql;
            sql = $"SELECT UBCode, CodeKind, TestName, OrderCode, TestCode, SampleCode, SampleName\r\n" +
                  $"     , CASE WHEN ISNULL(LabgeOrderName, '') <> '' THEN LabgeOrderName\r\n" +
                  $"            ELSE LabgeTestName\r\n" +
                  $"       END AS LabgeTestName\r\n" +
                  $"FROM\r\n" +
                  $"(\r\n" +
                  $"    SELECT rtyc.*\r\n" +
                  $"         , (SELECT TestDisplayName FROM LabTestCode WHERE rtyc.OrderCode = TestCode) AS LabgeOrderName\r\n" +
                  $"         , (SELECT TestDisplayName FROM LabTestCode WHERE rtyc.TestCode = TestCode) AS LabgeTestName\r\n" +
                  $"         , (SELECT SampleName FROM LabSampleCode lsc WHERE rtyc.SampleCode = SampleCode) AS SampleName\r\n" +
                  $"    FROM RsltTransYsr2000Code rtyc\r\n" +
                  $"    WHERE UBCode = '{ubCode}'" +
                  $") AS Sub1";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        /// <summary>
        /// 의사랑 코드 등록
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHttpActionResult PostUBCode([FromBody] JObject request)
        {
            string sql;
            sql = $"INSERT INTO RsltTransYsr2000Code\r\n" +
                  $"(UBCode, CodeKind, TestName, OrderCode, TestCode, SampleCode)\r\n" +
                  $"VALUES\r\n" +
                  $"(" +
                  $"    '{request["UBCode"].ToString()}', '{request["CodeKind"].ToString()}', '{request["TestName"].ToString()}'\r\n" +
                  $"  , '{request["OrderCode"].ToString()}', '{request["TestCode"].ToString()}', '{request["SampleCode"].ToString()}'\r\n" +
                  $")";

            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        /// <summary>
        /// 의사랑 코드 수정
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHttpActionResult PutUBCode([FromBody] JObject request)
        {
            string sql;
            sql = $"UPDATE RsltTransYsr2000Code\r\n" +
                  $"SET OrderCode = '{request["OrderCode"].ToString()}'\r\n" +
                  $"  , TestCode = '{request["TestCode"].ToString()}'\r\n" +
                  $"  , TestName = '{request["TestName"].ToString()}'\r\n" +
                  $"  , SampleCode = '{request["SampleCode"].ToString()}'\r\n" +
                  $"WHERE UBCode = '{request["UBCode"].ToString()}'";

            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        /// <summary>
        /// 의사랑 코드 삭제
        /// </summary>
        /// <param name="ubCode"></param>
        /// <returns></returns>
        public IHttpActionResult DeleteUBCode(string ubCode)
        {
                string sql;
                sql = $"DELETE FROM RsltTransYsr2000Code\r\n" +
                      $"WHERE UBCode = '{ubCode}'";

                LabgeDatabase.ExecuteSql(sql);
                return Ok();
        }
    }
}