using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.StrategyBusiness
{
    [SupportsAuth]
    [Route("api/StrategyBusiness/ManagePersonalInfo")]
    public class ManagePersonalInfoController : ApiController
    {
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string groupCode, string mode)
        {
            //거래처주소, 우편번호 필요 
            // 오전: 9, 오후:13, 모두: 24 필요 컬럼 추가 완료
            //검사 결과 완료
            StringBuilder sb = new StringBuilder();
            //unreg reg
            if (mode == "Unreg")
            {
                sb.Append("SELECT lri.LabRegDate, lri.LabRegNo, lri.PatientName, lri.PatientJuminNo01, lri.PatientSex,lri.PatientPhoneNo, lri.PatientAge,\n");
                sb.Append("pcc.CompCode, pcc.CompName, pcc.CompAddress01 as address0, pcc.CompAddress02 as address1, pcc.CompZipCode, pcgc.CompGroupName, lrt.TestCode, ltc.TestDisplayName, lri.CompSubCode, lri.CompSubName, lri.PatientEmail, CONVERT(bit, 0) as IsAgreeConsultation, IsPatientInfoChange, \n");
                sb.Append("CONVERT(bit, 0) as AgreeThirdPartyOffer, CONVERT(bit, 0) as AgreeFreeTestBiometircAge,\n");
                sb.AppendLine("(CASE WHEN pi2.AgreePhoneConsultation = '9' THEN '오전' " +
                    "WHEN pi2.AgreePhoneConsultation = '13' THEN '오후'" +
                    "WHEN pi2.AgreePhoneConsultation = '23' THEN '둘다가능'" +
                    "ELSE '선택안됨' END) AS AgreePhoneConsultation");
                sb.Append("FROM  LabRegInfo lri\n");
                sb.Append("Left outer join PGSPersonalInfo pi2\n");
                sb.Append("on pi2.LabRegDate = lri.LabRegDate\n");
                sb.Append("AND pi2.LabRegNo = lri.LabRegNo\n");
                sb.Append("JOIN LabRegTest lrt\n");
                sb.Append("on lrt.LabRegDate = lri.LabRegDate\n");
                sb.Append("AND lrt.LabRegNo = lri.LabRegNo\n");
                sb.Append("join LabTestCode ltc\n");
                sb.Append("on ltc.TestCode = lrt.TestCode\n");
                sb.Append("JOIN ProgCompCode pcc\n");
                sb.Append("ON pcc.CompCode = lri.CompCode\n");
                sb.Append("JOIN ProgCompGroupCode pcgc\n");
                sb.Append("On pcgc.CompGroupCode = pcc.CompGroupCode\n");
                sb.Append("JOIN ProgAuthGroupAccessComp pagac\n");
                sb.Append("on pagac.CompCode = lri.CompCode\n");
                sb.Append($"WHERE (lri.LabRegDate  BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}')\n");
                sb.Append("AND lri.CompCode IN\n");
                sb.Append("(\n");
                sb.Append("select pagcl.CompCode  from ProgAuthGroupCompList pagcl\n");
                sb.Append("WHERE  pagcl.AuthGroupCode = 'Genocore_Bioage'\n");
                sb.Append(")\n");
                sb.Append($"AND pagac.AuthGroupCode ='{groupCode}' AND pi2.AgreeThirdPartyOffer is null AND lri.CompSubCode in('500', '', NUll) \n");
                sb.Append($"order by lri.LabRegDate, lri.LabRegNo");
            }
            else
            {
                sb.AppendLine("SELECT ppi.LabRegDate, ppi.LabRegNo, ppi.PatientName, ppi.PatientSex, ppi.PatientJuminNo01, ppi.PatientPhoneNo, ppi.PatientEmail, ppi.CompCode, ppi.IsPatientInfoChange");
                sb.AppendLine(", (CASE WHEN ppi.IsAgreeConsultation = '1' THEN '20000' ELSE '0' END) as IsAgreeConsultation");
                sb.AppendLine(", ppi.CompName, pcc.CompAddress01 as address0, pcc.CompAddress02 as address1, pcc.CompZipCode, ppi.CompSubCode, ppi.CompSubName, ppi.AgreeThirdPartyOffer, ppi.AgreeFreeTestBiometircAge, ppi.SendDatetime, ppi.IsSendData");
                sb.AppendLine(", pcgc.CompGroupName, lrt.TestCode, ltc.TestDisplayName");
                sb.AppendLine(", (CASE WHEN ppi.AgreePhoneConsultation = '9' THEN '오전' " +
                    "WHEN ppi.AgreePhoneConsultation = '13' THEN '오후'" +
                    "WHEN ppi.AgreePhoneConsultation = '24' THEN '둘다가능'" +
                    "ELSE '선택안됨' END) AS AgreePhoneConsultation");
                sb.AppendLine("FROM PGSPersonalInfo ppi");
                sb.AppendLine("JOIN ProgCompCode pcc");
                sb.AppendLine("ON pcc.CompCode = ppi.CompCode");
                sb.AppendLine("JOIN ProgCompGroupCode pcgc");
                sb.AppendLine("On pcgc.CompGroupCode = pcc.CompGroupCode");
                sb.AppendLine("JOIN ProgAuthGroupAccessComp pagac");
                sb.AppendLine("on pagac.CompCode = ppi.CompCode");
                sb.AppendLine("JOIN LabRegTest lrt");
                sb.AppendLine("on lrt.LabRegDate = ppi.LabRegDate");
                sb.AppendLine("AND lrt.LabRegNo = ppi.LabRegNo");
                sb.AppendLine("join LabTestCode ltc");
                sb.AppendLine("on ltc.TestCode = lrt.TestCode");
                sb.AppendLine($"WHERE (ppi.LabRegDate  BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}')");
                sb.AppendLine($"AND pagac.AuthGroupCode ='{groupCode}'");
            }

            JArray arrResponse = LabgeDatabase.SqlToJArray(sb.ToString());
            return Ok(arrResponse);
        }


        [Route("api/StrategyBusiness/ManagePersonalInfo/ChangeData")]
        public IHttpActionResult Get(DateTime beginDate, DateTime endDate, string groupCode)
        {
            //거래처주소, 우편번호 필요 
            // 오전: 9, 오후:13, 모두: 24 필요 컬럼 추가 완료
            //검사 결과 완료
            StringBuilder sb = new StringBuilder();
            //unreg reg
            sb.AppendLine("SELECT lri.LabRegDate, lri.LabRegNo, lri.PatientName as LabCenterName, lri.PatientJuminNo01 as LabCenterJuminNo, lri.PatientPhoneNo as LabCenterPhoneNo, ppi.PatientEmail as LabCenterEmail, lri.PatientSex as LabCenterSex, ppi.CompCode");
            sb.AppendLine(", ppi.PatientName, ppi.PatientJuminNo01, ppi.PatientPhoneNo, ppi.PatientEmail, ppi.PatientSex");
            sb.AppendLine(", ppi.CompName, pcc.CompAddress01 as address0, pcc.CompAddress02 as address1, pcc.CompZipCode, ppi.CompSubCode, ppi.CompSubName, ppi.AgreeThirdPartyOffer, ppi.AgreeFreeTestBiometircAge, ppi.SendDatetime, ppi.IsSendData, ppi.IsPatientInfoChange");
            sb.AppendLine(", pcgc.CompGroupName, lrt.TestCode, ltc.TestDisplayName");
            sb.AppendLine(", (CASE WHEN ppi.AgreePhoneConsultation = '9' THEN '오전' " +
                "WHEN ppi.AgreePhoneConsultation = '13' THEN '오후'" +
                "WHEN ppi.AgreePhoneConsultation = '24' THEN '둘다가능'" +
                "ELSE '선택안됨' END) AS AgreePhoneConsultation");
            sb.AppendLine("FROM PGSPersonalInfo ppi");
            sb.AppendLine("JOIN ProgCompCode pcc");
            sb.AppendLine("ON pcc.CompCode = ppi.CompCode");
            sb.AppendLine("JOIN ProgCompGroupCode pcgc");
            sb.AppendLine("On pcgc.CompGroupCode = pcc.CompGroupCode");
            sb.AppendLine("JOIN ProgAuthGroupAccessComp pagac");
            sb.AppendLine("on pagac.CompCode = ppi.CompCode");
            sb.AppendLine("JOIN LabRegTest lrt");
            sb.AppendLine("on lrt.LabRegDate = ppi.LabRegDate");
            sb.AppendLine("AND lrt.LabRegNo = ppi.LabRegNo");
            sb.AppendLine("join LabTestCode ltc");
            sb.AppendLine("on ltc.TestCode = lrt.TestCode");
            sb.AppendLine("join LabRegInfo lri");
            sb.AppendLine("On lri.LabRegDate = ppi.LabRegDate");
            sb.AppendLine("AND lri.LabRegNo = ppi.LabRegNo");
            sb.AppendLine($"WHERE (ppi.LabRegDate  BETWEEN '{beginDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}')");
            sb.AppendLine($"AND (lri.PatientJuminNo01 != ppi.PatientJuminNo01 or lri.PatientName != ppi.PatientName or lri.PatientPhoneNo != ppi.PatientPhoneNo or lri.PatientEmail != ppi.PatientEmail or lri.PatientSex != ppi.PatientSex)");
            sb.AppendLine($"AND pagac.AuthGroupCode ='{groupCode}'");


            JArray arrResponse = LabgeDatabase.SqlToJArray(sb.ToString());
            return Ok(arrResponse);
        }


        public IHttpActionResult Put([FromBody]JObject objRequest)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("INSERT INTO PGSPersonalInfo\n");
            sb.AppendLine($"(LabRegDate, LabRegNo, PatientName, PatientSex, PatientJuminNo01, PatientPhoneNo, PatientEmail, " +
                $"CompCode, CompName, CompSubCode, CompSubName, AgreeThirdPartyOffer, AgreeFreeTestBiometircAge, agreePhoneConsultation)");
            sb.AppendLine($"values('{objRequest["LabRegDate"]}', '{objRequest["LabRegNo"]}', '{objRequest["PatientName"]}' , '{objRequest["PatientSex"]}', '{objRequest["PatientJuminNo"]}', '{objRequest["PhoneNumber"]}','{objRequest["PatientEmail"]}'," +
                $"'{objRequest["CompCode"]}','{objRequest["CompName"]}','{objRequest["CompSubCode"]}', '{objRequest["CompSubName"]}', {objRequest["AgreeThirdPartyOffer"]}, {objRequest["AgreeFreeTestBiometircAge"]}, '{objRequest["agreePhoneConsultation"]}')");

            LabgeDatabase.ExecuteSql(sb.ToString());
            return Ok();
        }
        public IHttpActionResult Post([FromBody]JObject objRequest)
        {
            //랩센터 업데이트 구현 필요
            //이름, 전화번호, 생년월일 수정가능하게
            //StringBuilder sb = new StringBuilder();
            //
            //sb.AppendLine("UPDATE PGSPersonalInfo ");
            //sb.AppendLine($"SET {objRequest[""]} ");
            //sb.AppendLine($"WHERE LabRegDate = '{objRequest["LabRegDate"]}' AND LabRegNo = '{objRequest["LabRegNo"]}'");
            //
            //
            //
            //LabgeDatabase.ExecuteSql(sb.ToString());
            return Ok();
        }




        public IHttpActionResult Delete(string LabRegDate, string LabRegNo)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("DELETE FROM PGSPersonalInfo");
            sb.AppendLine($"WHERE LabRegDate = '{LabRegDate}' AND LabRegNo = {LabRegNo}");
            LabgeDatabase.ExecuteSql(sb.ToString());
            return Ok();
        }


    }
}
