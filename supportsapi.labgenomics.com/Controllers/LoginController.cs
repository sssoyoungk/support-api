using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Cors;

namespace supportsapi.labgenomics.com.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*", PreflightMaxAge = 28800)]
    public class LoginController : ApiController
    {
        //POST방식만 사용 함

        //public IHttpActionResult Get(string loginID, string loginPW)
        //{
        //    JObject objResponse = new JObject();

        //    try
        //    {
        //        string sql;

        //        sql = $"SELECT A.MemberPassword, A.MemberName, A.AuthGroupCode, A.MemberDeptName, A.MemberEmpClass \r\n" +
        //              $"     , A.MemberGroupCode, B.AuthGroupName, A.IsMemberActive\r\n" +
        //              $"     , CONVERT(bit, CASE WHEN A.MemberEndDate < GETDATE() THEN 1 ELSE 0 END) AS IDExpired\r\n" +
        //              $"  FROM ProgMember AS A \r\n" +
        //              $" INNER JOIN ProgAuthGroupCode AS B \r\n" +
        //              $"    ON A.AuthGroupCode = b.AuthGroupCode \r\n" +
        //              $" WHERE A.MemberID = '{loginID}'";

        //        JObject objAccount = LabgeDatabase.SqlToJObject(sql);

        //        //중지계정
        //        if (Convert.ToBoolean(objAccount["IsMemberActive"]) == false)
        //        {
        //            objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
        //            objResponse.Add("Message", "중지된 계정입니다");
        //            return Content(HttpStatusCode.BadRequest, objResponse);
        //        }

        //        //계정만료일자 지남
        //        if (Convert.ToBoolean(objAccount["IDExpired"]) == true)
        //        {
        //            objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
        //            objResponse.Add("Message", "중지된 계정입니다");
        //            return Content(HttpStatusCode.BadRequest, objResponse);
        //        }

        //        if (objAccount["MemberPassword"].ToString() == loginPW)
        //        {
        //            objResponse.Add("MemberName", objAccount["MemberName"].ToString());
        //            objResponse.Add("AuthGroupCode", objAccount["AuthGroupCode"].ToString());
        //            objResponse.Add("MemberDeptName", objAccount["MemberDeptName"].ToString());
        //            objResponse.Add("MemberEmpClass", objAccount["MemberEmpClass"].ToString());
        //            objResponse.Add("MemberGroupCode", objAccount["MemberGroupCode"].ToString());
        //            objResponse.Add("AuthGroupName", objAccount["AuthGroupName"].ToString());
        //            objResponse.Add("SupportConnString", "Data Source=1.237.52.136,14032;Initial Catalog=LC04_LABGE;Persist Security Info=True;User ID=supports;Password=2nd@LabGen[0700]");
        //            objResponse.Add("Token", ManageJwtToken.GenerateToken(loginID, "Supports"));
        //        }
        //        else
        //        {
        //            objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
        //            objResponse.Add("Message", "계정 정보가 일치하지 않습니다.");
        //            return Content(HttpStatusCode.BadRequest, objResponse);
        //        }

        //        return Ok(objResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
        //        objResponse.Add("Message", ex.Message);
        //        return Content(HttpStatusCode.BadRequest, objResponse);
        //    }
        //}


        public IHttpActionResult Post([FromBody]JObject request)
        {
            JObject objResponse = new JObject();

            try
            {
                string sql;

                sql = $"SELECT A.MemberID, A.MemberPassword, A.MemberName, A.AuthGroupCode, A.MemberDeptName, A.MemberEmpClass \r\n" +
                      $"     , A.MemberGroupCode, B.AuthGroupName, A.IsMemberActive\r\n" +
                      $"     , CONVERT(bit, CASE WHEN A.MemberEndDate < GETDATE() THEN 1 ELSE 0 END) AS IDExpired\r\n" +
                      $"  FROM ProgMember AS A \r\n" +
                      $" INNER JOIN ProgAuthGroupCode AS B \r\n" +
                      $"    ON A.AuthGroupCode = b.AuthGroupCode \r\n" +
                      $" WHERE A.MemberID = '{request["LoginID"]}'";

                JObject objAccount = LabgeDatabase.SqlToJObject(sql);

                //중지계정
                if (Convert.ToBoolean(objAccount["IsMemberActive"]) == false)
                {
                    objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                    objResponse.Add("Message", "중지된 계정입니다");
                    return Content(HttpStatusCode.BadRequest, objResponse);
                }

                //계정만료일자 지남
                if (Convert.ToBoolean(objAccount["IDExpired"]) == true)
                {
                    objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                    objResponse.Add("Message", "중지된 계정입니다");
                    return Content(HttpStatusCode.BadRequest, objResponse);
                }

                if (objAccount["MemberPassword"].ToString() == request["LoginPW"].ToString())
                {
                    objResponse.Add("MemberID", objAccount["MemberID"].ToString());
                    objResponse.Add("MemberName", objAccount["MemberName"].ToString());
                    objResponse.Add("AuthGroupCode", objAccount["AuthGroupCode"].ToString());
                    objResponse.Add("MemberDeptName", objAccount["MemberDeptName"].ToString());
                    objResponse.Add("MemberEmpClass", objAccount["MemberEmpClass"].ToString());
                    objResponse.Add("MemberGroupCode", objAccount["MemberGroupCode"].ToString());
                    objResponse.Add("AuthGroupName", objAccount["AuthGroupName"].ToString());
                    objResponse.Add("SupportConnString", "Data Source=1.237.52.136,14032;Initial Catalog=LC04_LABGE;Persist Security Info=True;User ID=supports;Password=2nd@LabGen[0700]");
                    objResponse.Add("Token", ManageJwtToken.GenerateToken(request["LoginID"].ToString(), "Supports"));
                }
                else
                {
                    objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                    objResponse.Add("Message", "계정 정보가 일치하지 않습니다.");
                    return Content(HttpStatusCode.BadRequest, objResponse);
                }

                return Ok(objResponse);
            }
            catch (Exception ex)
            {
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }

        [Route("api/CheckLogin")]
        public IHttpActionResult PostCheckLogin()
        {
            string token = Request.Headers.Authorization.Parameter;
            ClaimsPrincipal principal = ManageJwtToken.VerifyToken(token);
            JObject objResponse = new JObject();
            if (principal.Identity.IsAuthenticated)
            {
                objResponse.Add("IsLogin", true);
            }
            else
            {
                objResponse.Add("IsLogin", false);
            }
            return Ok(objResponse);
        }
    }
}