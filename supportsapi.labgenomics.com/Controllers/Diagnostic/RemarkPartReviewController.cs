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
        




    }
}