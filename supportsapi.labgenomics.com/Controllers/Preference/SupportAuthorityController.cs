using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Preference
{
#if !DEBUG
    [SupportsAuth]
#endif
    public class SupportAuthorityController : ApiController
    {
        /// <summary>
        /// 로그인 계정의 메뉴 권한 조회
        /// </summary>
        /// <param name="groupCode"></param>
        /// <param name="memberID"></param>
        /// <returns></returns>
        public IHttpActionResult Get(string groupCode, string memberID)
        {
            string sql;
            sql = $"SELECT A.FormName, B.FormTitle, B.FormGroup \r\n" +
                  $"  FROM SupportAuthGroup A  \r\n" +
                  $"  JOIN SupportFormList B \r\n" +
                  $"    ON B.FormName = A.FormName \r\n" +
                  $" WHERE A.AuthGroupCode = '{groupCode}' \r\n" +
                  $"UNION \r\n" +
                  $"SELECT A.FormName, B.FormTitle, B.FormGroup \r\n" +
                  $"  FROM SupportAuthMember A \r\n" +
                  $"  JOIN SupportFormList B \r\n" +
                  $"    ON B.FormName = A.FormName \r\n" +
                  $" WHERE A.AuthMemberID = '{memberID}' \r\n" +
                  $" AND FormVisible = 1 ";

            JArray objResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(objResponse);
        }

        /// <summary>
        /// 권한 그룹 계정
        /// </summary>
        /// <returns></returns>
        [Route("api/Preference/Authority/Group")]
        public IHttpActionResult GetGroupAuthority()
        {
            string sql;
            sql = "SELECT AuthGroupCode, AuthGroupName\r\n" +
                  "FROM ProgAuthGroupCode";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        /// <summary>
        /// 그룹별 권한 목록 조회
        /// </summary>
        /// <param name="groupCode"></param>
        /// <returns></returns>
        [Route("api/Preference/Authority/GroupMenu")]
        public IHttpActionResult GetAuthGrouphMenu(string groupCode)
        {
            string sql;
            sql = $"SELECT sag.AuthGroupCode, sag.FormName, sfl.FormTitle, sfl.FormGroup\r\n" +
                  $"FROM SupportAuthGroup sag\r\n" +
                  $"JOIN SupportFormList sfl\r\n" +
                  $"ON sfl.FormName = sag.FormName\r\n" +
                  $"WHERE sag.AuthGroupCode = '{groupCode}'";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        /// <summary>
        /// 그룹별 권한 등록
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("api/Preference/Authority/GroupMenu")]
        public IHttpActionResult PostAuthGroupMenu([FromBody] JObject request)
        {
            string sql;

            sql = $"MERGE INTO SupportAuthGroup AS target\r\n" +
                  $"USING (SELECT '{request["GroupCode"].ToString()}' AS AuthGroupCode, '{request["FormName"].ToString()}' AS FormName) AS source\r\n" +
                  $"ON (target.AuthGroupCode = source.AuthGroupCode AND target.FormName = source.FormName)\r\n" +
                  $"WHEN NOT MATCHED THEN\r\n" +
                  $"    INSERT (AuthGroupCode, FormName)\r\n" +
                  $"    VALUES (source.AuthGroupCode, source.FormName);";

            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        /// <summary>
        /// 그룹별 권한 삭제
        /// </summary>
        /// <param name="groupCode"></param>
        /// <param name="formName"></param>
        /// <returns></returns>
        [Route("api/Preference/Authority/GroupMenu")]
        public IHttpActionResult DeleteAuthGroupMenu(string groupCode, string formName)
        {
            string sql;
            sql = $"DELETE FROM SupportAuthGroup\r\n" +
                  $"WHERE AuthGroupCode = '{groupCode}'\r\n" +
                  $"AND FormName = '{formName}'";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        /// <summary>
        /// 사원 목록 조회
        /// </summary>
        [Route("api/Preference/Authority/Employee")]
        public IHttpActionResult GetEmpList()
        {
            string sql;

            sql = "SELECT MemberID, MemberName, MemberDeptName, IsMemberEmployee\r\n" +
                  "FROM ProgMember\r\n" +
                  "WHERE IsMemberActive = '1'";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        /// <summary>
        /// 사원별 권한 조회
        /// </summary>
        /// <param name="memberID"></param>
        /// <returns></returns>
        [Route("api/Preference/Authority/PersonalMenu")]
        public IHttpActionResult GetAuthPersonMenu(string memberID)
        {
            string sql;

            sql = $"SELECT sam.AuthMemberID, sam.FormName, sfl.FormTitle, sfl.FormGroup, sam.FormVisible\r\n" +
                  $"FROM SupportAuthMember sam\n" +
                  $"JOIN SupportFormList sfl\n" +
                  $"ON sfl.FormName = sam.FormName\r\n" +
                  $"WHERE sam.AuthMemberID = '{memberID}'";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        /// <summary>
        /// 사원별 권한 등록
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("api/Preference/Authority/PersonalMenu")]
        public IHttpActionResult PostAuthPersonMenu([FromBody]JObject request)
        {
            string sql;
            sql = $"MERGE INTO SupportAuthMember AS target\r\n" +
                  $"USING (SELECT '{request["MemberID"].ToString()}' AS AuthMemberID, '{request["FormName"].ToString()}' AS FormName) AS source\r\n" +
                  $"ON (target.AuthMemberID = source.AuthMemberID AND target.FormName = source.FormName)\r\n" +
                  $"WHEN NOT MATCHED THEN\r\n" +
                  $"    INSERT (AuthMemberID, FormName)\r\n" +
                  $"    VALUES (source.AuthMemberID, source.FormName);";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        /// <summary>
        /// 사원별 권한 수정
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("api/Preference/Authority/PersonalMenu")]
        public IHttpActionResult PutAuthPersonMenu([FromBody]JObject request)
        {
            string sql;
            sql = $"UPDATE SupportAuthMember\r\n" +
                  $"SET FormVisible = {Convert.ToInt32(request["FormVisible"])}\r\n" +
                  $"WHERE AuthMemberID = '{request["MemberID"].ToString()}'\r\n" +
                  $"AND FormName = '{request["FormName"].ToString()}'";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        /// <summary>
        /// 사원별 권한 삭제
        /// </summary>
        /// <param name="memberID"></param>
        /// <param name="formName"></param>
        /// <returns></returns>
        [Route("api/Preference/Authority/PersonalMenu")]
        public IHttpActionResult DeleteAuthPersonMenu(string memberID, string formName)
        {
            string sql;
            sql = $"DELETE FROM SupportAuthMember\r\n" +
                  $"WHERE AuthMemberID = '{memberID}'\r\n" +
                  $"AND FormName = '{formName}'";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        /// <summary>
        /// Form 정보 조회
        /// </summary>
        /// <returns></returns>
        [Route("api/Preference/Authority/Form")]
        public IHttpActionResult GetForms()
        {
            string sql;
            sql = "SELECT Depth2.* \r\n" +
                  "FROM SupportFormList AS Depth1 \r\n" +
                  "LEFT OUTER JOIN SupportFormList AS Depth2 \r\n" +
                  "ON Depth1.FormGroup = Depth2.FormGroup \r\n" +
                  "AND Depth2.MenuDepth = 2 \r\n" +
                  "WHERE Depth1.MenuDepth = 1 \r\n" +
                  "ORDER BY Depth1.FormSequence, Depth2.FormSequence";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        /// <summary>
        /// Form 정보 저장
        /// </summary>
        [Route("api/Preference/Authority/Form")]
        public IHttpActionResult PostForms([FromBody]JObject request)
        {
            string sql;
            sql = $"DECLARE @Count int \r\n" +
                  $"SELECT @Count = COUNT(*) \r\n" +
                  $"FROM SupportFormList \r\n" +
                  $"WHERE FormName = '{request["FormName"].ToString()}' \r\n" +

                  $"IF @Count = 0 \r\n" +
                  $"BEGIN \r\n" +
                  $"    DECLARE @FormSequence int \r\n" +
                  $"    SELECT @FormSequence = MAX(FormSequence) + 1 \r\n " +
                  $"    FROM SupportFormList \r\n" +
                  $"    WHERE FormGroup = '{request["FormGroup"].ToString()}' \r\n" +
                  $"    AND MenuDepth = 2 \r\n" +

                  $"    INSERT INTO SupportFormList \r\n" +
                  $"    (FormName, FormGroup, MenuDepth, FormTitle, FormSequence, IsUse) \r\n" +
                  $"    VALUES \r\n" +
                  $"    ( '{request["FormName"].ToString()}', '{request["FormGroup"].ToString()}', '{request["MenuDepth"].ToString()}'" +
                  $"    , '{request["FormTitle"].ToString()}', @FormSequence, {Convert.ToInt32(request["IsUse"])}) \r\n" +
                  $"END \r\n" +
                  $"ELSE \r\n" +
                  $"BEGIN \r\n" +
                  $"    UPDATE SupportFormList \r\n" +
                  $"    SET IsUse = {Convert.ToInt32(request["IsUse"])} \r\n" +
                  $"    WHERE FormName = '{request["FormName"].ToString()}' \r\n" +
                  $"END";
            LabgeDatabase.ExecuteSql(sql);
            return Ok();
        }

        [Route("api/Preference/Authority/RegOrderComp")]
        public IHttpActionResult GetRegOrderComp()
        {
            string sql;
            sql = "SELECT A.*, B.CompName\r\n" +
                  "FROM RsltTransCompSet A\r\n" +
                  "JOIN ProgCompCode B\r\n" +
                  "ON A.CompCode = B.CompCode\r\n" +
                  "ORDER BY A.CompCode";

            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/Preference/Authority/RegOrderComp")]
        public IHttpActionResult PostRegOrderComp([FromBody]JObject request)
        {
            try
            {
                string sql;
                sql = $"MERGE INTO RsltTransCompSet AS target\r\n" +
                      $"USING\r\n" +
                      $"(\r\n" +
                      $"    SELECT\r\n" +
                      $"        '{request["CompCode"].ToString()}' AS CompCode, '{request["TransKind"].ToString()}' AS TransKind,\r\n" +
                      $"        '{(request["ChartName"] ?? "").ToString()}' AS ChartName, {Convert.ToInt32(request["IsUse"])} AS IsUse,\r\n" +
                      $"        '{(request["ServerName"] ?? "").ToString()}' AS ServerName, '{(request["DatabaseName"] ?? "").ToString()}' AS DatabaseName,\r\n" +
                      $"        '{(request["LoginID"] ?? "").ToString()}' AS LoginID, '{(request["Password"] ?? "").ToString()}' AS Password,\r\n" +
                      $"        {Convert.ToInt32(request["UseOdbc"])} AS UseOdbc\r\n" +
                      $") AS source\r\n" +                      
                      $"ON (target.CompCode = source.CompCode)\r\n" +                      
                      $"WHEN MATCHED THEN\r\n" +
                      $"    UPDATE SET IsUse = source.IsUse\r\n" +
                      $"             , TransKind = source.TransKind\r\n" +
                      $"             , ChartName = source.ChartName\r\n" +
                      $"             , ServerName = source.ServerName\r\n" +
                      $"             , DatabaseName = source.DatabaseName\r\n" +
                      $"             , LoginID = source.LoginID\r\n" +
                      $"             , Password = source.Password\r\n" +
                      $"             , UseOdbc = source.UseOdbc\r\n" +
                      $"WHEN NOT MATCHED THEN\r\n" +
                      $"    INSERT (CompCode, TransKind, ChartName, IsUse, ServerName, DatabaseName, LoginID, Password, UseOdbc)\r\n" +
                      $"    VALUES (source.CompCode, source.TransKind, source.ChartName, source.IsUse, source.ServerName, source.DatabaseName, source.LoginID, source.Password, source.UseOdbc);";
                LabgeDatabase.ExecuteSql(sql);

                sql = $"SELECT CompName\r\n" +
                      $"FROM ProgCompCode\r\n" +
                      $"WHERE CompCode = '{request["CompCode"].ToString()}'";
                string compName = Convert.ToString(LabgeDatabase.ExecuteSqlScalar(sql));
                JObject objResponse = new JObject();
                objResponse.Add("CompName", compName);
                return Ok(objResponse);
            }
            catch (Exception ex)
            {
                JObject objResponse = new JObject();
                objResponse.Add("Status", Convert.ToInt32(HttpStatusCode.BadRequest));
                objResponse.Add("Message", ex.Message);
                return Content(HttpStatusCode.BadRequest, objResponse);
            }
        }

        [Route("api/Preference/Authority/RegOrderComp")]
        public IHttpActionResult DeleteRegOrderComp(string compCode)
        {
            try
            {
                string sql;
                sql = $"DELETE FROM RsltTransCompSet\r\n" +
                      $"WHERE CompCode = '{compCode}'";
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
    }
}