using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Data;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Diagnostic
{
    public class ResultCopyController : ApiController
    {

        [Route("api/Diagnostic/ResultCopy/ReportCode")]
        public IHttpActionResult GetReportCode()
        {
            string sql;
            sql = "select ReportCode as 코드, ReportName as 보고서 from LabReportCode where IsUseReport = '1' and ReportCode in ('01', '03', '04', '04_1', '12', '12_2', '14', '07', '29', '17', '30', 'LC006', 'L001', 'LC009', 'LC010', 'LC013', 'L004', 'LC017')";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }
        
        [Route("api/Diagnostic/ResultCopy/Source")]
        public IHttpActionResult GetSource(string labRegDate, string labRegNo, string targetLabRegNo, string targetLabRegDate, string editvalue, string whereOption)
        {
            string sql;

            sql = "select a.LabRegDate as 접수일, a.LabRegNo as 접수번호, b.CompName as 거래처명, a.PatientName as 수진자명, a.PatientJuminNo01 as 생년월일, a.PatientAge as 나이, a.PatientSex as 성별, a.PatientChartNo as 차트번호, h.ReportBunjuNo as 병리번호, e.PartName as 파트명, c.OrderCode as 의뢰코드, c.TestCode as 테스트코드, c.TestSubCode as 검사코드, "
                           + "d.TestDisplayName as 검사명, d.IsTestHeader as 헤더, g.TestStateShortName as 상태, c.TestResult01 as 결과1, c.TestResult02 as 결과2, c.TestResultAbn as 판정, c.TestResultText as 서술, isnull(c.IsTestResultPanic, 0) as P, isnull(c.IsTestResultDelta, 0) as D, isnull(c.IsTestResultCritical, 0) as C, f.DoctorCode as 판독의코드, j.DoctorPersonName as 판독의 , k.TestSubCode as T코드 "
                           + "from LabRegInfo as a inner join ProgCompCode as b on a.CompCode = b.CompCode "
                           + "inner join LabRegResult as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo "
                           + "inner join LabTestCode as d on c.TestSubCode = d.TestCode "
                           + "inner join LabPartCode as e on d.PartCode = e.PartCode "
                           + "inner join LabRegTest as f on a.LabRegDate = f.LabRegDate and a.LabRegNo = f.LabRegNo and c.TestCode = f.TestCode "
                           + "inner join LabTestStateCode as g on f.TestStateCode = g.TestStateCode "
                           + "inner join LabTestCode as i on f.TestCode = i.TestCode "
                           + "inner join LabRegReport as h on a.LabRegDate = h.LabRegDate and a.LabRegNo = h.LabRegNo "
                           + "inner join (select distinct RtT.ReportCode, RtT.TestCode "
                           + "from (select ReportCode, TestCode from LabReportTest union all "
                           + "select ReportCode, TestCode from LabTestCode) as RtT) as L on h.ReportCode = L.ReportCode and i.TestCode = L.TestCode "
                           + "left outer join LabDoctorCode as j on f.DoctorCode = j.DoctorCode "
                           + "left outer join (select a.LabRegDate, a.LabRegNo, a.TestSubCode, b.RelatedTestCode from LabRegResult as a inner join LabTestCode as b on a.TestSubCode = b.TestCode) as k on k.LabRegDate = '" + targetLabRegDate + "' and k.LabRegNo = '" + targetLabRegNo + "' and " + whereOption + " in (k.TestSubCode, k.RelatedTestCode) "
                           //+ "left outer join (select a.LabRegDate, a.LabRegNo, a.TestSubCode, b.RelatedTestCode from LabRegResult as a inner join LabTestCode as b on a.TestSubCode = b.TestCode) as k on k.LabRegDate = '" + dateT.Value.ToString("yyyy-MM-dd") + "' and k.LabRegNo = '" + txtTNo.Text.ToString() + "' and d.RelatedTestCode in (k.TestSubCode, k.RelatedTestCode) "
                           + "where a.LabRegDate = '" + labRegDate + "' and a.LabRegNo = '" + labRegNo + "'  and L.ReportCode = '" + editvalue + "' "
                           + "order by e.PartSeqNo, d.TestSeqNo, d.TestCode, d.IsTestHeader desc ";


            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/Diagnostic/ResultCopy/Target")]
        public IHttpActionResult GetTarget(string targetLabRegNo, string targetLabRegDate, string targetReportCode )
        {
            string sql;
            sql = "select a.LabRegDate as 접수일, a.LabRegNo as 접수번호, b.CompName as 거래처명, a.PatientName as 수진자명, a.PatientJuminNo01 as 생년월일, a.PatientAge as 나이, a.PatientSex as 성별, a.PatientChartNo as 차트번호, h.ReportBunjuNo as 병리번호, e.PartName as 파트명, c.OrderCode as 의뢰코드, c.TestCode as 테스트코드, c.TestSubCode as 검사코드, "
                           + "d.TestDisplayName as 검사명, d.IsTestHeader as 헤더, g.TestStateShortName as 상태, c.TestResult01 as 결과1, c.TestResult02 as 결과2, c.TestResultAbn as 판정, c.TestResultText as 서술, isnull(c.IsTestResultPanic, 0) as P, isnull(c.IsTestResultDelta, 0) as D, isnull(c.IsTestResultCritical, 0) as C, f.DoctorCode as 판독의코드, j.DoctorPersonName as 판독의, h.ReportCode  "
                           + "from LabRegInfo as a inner join ProgCompCode as b on a.CompCode = b.CompCode "
                           + "inner join LabRegResult as c on a.LabRegDate = c.LabRegDate and a.LabRegNo = c.LabRegNo "
                           + "inner join LabTestCode as d on c.TestSubCode = d.TestCode "
                           + "inner join LabPartCode as e on d.PartCode = e.PartCode "
                           + "inner join LabRegTest as f on a.LabRegDate = f.LabRegDate and a.LabRegNo = f.LabRegNo and c.TestCode = f.TestCode "
                           + "inner join LabTestStateCode as g on f.TestStateCode = g.TestStateCode "
                           + "inner join LabTestCode as i on f.TestCode = i.TestCode "
                           + "inner join LabRegReport as h on a.LabRegDate = h.LabRegDate and a.LabRegNo = h.LabRegNo "
                           + "inner join (select distinct RtT.ReportCode, RtT.TestCode "
                           + "from (select ReportCode, TestCode from LabReportTest union all "
                           + "select ReportCode, TestCode from LabTestCode) as RtT) as L on h.ReportCode = L.ReportCode and i.TestCode = L.TestCode "
                           + "left outer join LabDoctorCode as j on f.DoctorCode = j.DoctorCode "
                           + "where a.LabRegDate = '" + targetLabRegDate + "' and a.LabRegNo = '" + targetLabRegNo + "'  and L.ReportCode in (" + targetReportCode + ") "
                           + "order by e.PartSeqNo, d.TestSeqNo, d.TestCode, d.IsTestHeader desc ";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }



        [Route("api/Diagnostic/ResultCopy")]
        public IHttpActionResult PutResultCopy([FromBody]JObject request)
        {
            try
            {
                string sql = string.Empty;
                string merge = string.Empty;

                //param
                //TargetLabRegNo
                //TargetLabRegDate
                //SourceLabRegNo
                //SourceLabRegDate
                //MemberName

                JObject param = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(request["param"].ToString());

                DataTable dtSource = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(request["source"].ToString());

                DataTable dtTarget = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(request["target"].ToString());

                if (dtSource.Rows.Count <= 0 && dtTarget.Rows.Count <= 0)
                {
                    throw new Exception("복사할 데이터 없음.");
                }


                if (param["ReportCode"].ToString() != "03")
                {
                    for (int i = 0; i < dtSource.Rows.Count; i++)
                    {
                        if (dtSource.Rows[i]["헤더"].ToString() != "True" && dtSource.Rows[i]["결과1"].ToString() != "")
                        {
                            //MessageBox.Show("Interface_SetPatientResult_Test '" + dateT.Value.ToString("yyyy-MM-dd") + "', '" + txtTNo.Text + "', '" + dtSource.Rows[i]["의뢰코드"].ToString() + "', '" + dtSource.Rows[i]["테스트코드"].ToString() + "', '" + dtSource.Rows[i]["검사코드"].ToString() + "', '" + dtSource.Rows[i]["결과1"].ToString() + "', '" + dtSource.Rows[i]["결과2"].ToString() + "', Null, Null, Null, Null, 'CM099', null");
                            string strSourceOrderCode = dtSource.Rows[i]["검사코드"].ToString(), strTargetOrderCode = "";
                            string strTargetTestCode = dtSource.Rows[i]["테스트코드"].ToString();
                            string strTargetTestSubCode = dtSource.Rows[i]["T코드"].ToString();
                
                
                
                            if (strSourceOrderCode == "22050")
                            {
                                strTargetTestCode = dtTarget.Rows[0]["테스트코드"].ToString();
                                if (strTargetTestCode == "22051")
                                {
                                    strTargetOrderCode = "22051";
                                    strTargetTestCode = "22051";
                                    strTargetTestSubCode = "22051";
                
                                    string strInterface_Result, strInterface_Text;
                                    strInterface_Result = "Interface_SetPatientResult_Test '" + param["TargetLabRegDate"].ToString() + "', '" + param["TargetLabRegNo"] + "', '" + strTargetOrderCode + "', '" + strTargetTestSubCode + "', '" + strTargetTestSubCode + "', '" + dtSource.Rows[i]["결과1"].ToString() + "', '" + dtSource.Rows[i]["결과2"].ToString() + "', Null, Null, Null, Null, 'CM099', null";
                                    strInterface_Text = "Interface_SetPatientResultText '" + param["TargetLabRegDate"].ToString() + "', '" + param["TargetLabRegNo"] + "', '" + strTargetOrderCode + "', '" + strTargetTestCode + "', '" + strTargetTestSubCode + "', '" + dtSource.Rows[i]["서술"].ToString().Replace("본 검사는 취합검사(Pooling test)이며, ", "") + "', 'CM099', null";
                
                                    LabgeDatabase.ExecuteSql(strInterface_Text);
                                    LabgeDatabase.ExecuteSql(strInterface_Result);
                                }
                                else
                                {
                                    LabgeDatabase.ExecuteSql("Interface_SetPatientResult_Test '" + param["TargetLabRegDate"].ToString() + "', '" + param["TargetLabRegNo"] + "'" +
                                        ", '" + strTargetOrderCode + "', '" + strTargetTestSubCode + "', '" + strTargetTestSubCode + "', '" + dtSource.Rows[i]["결과1"].ToString() + "'" +
                                        ", '" + dtSource.Rows[i]["결과2"].ToString() + "', Null, Null, Null, Null, 'CM099', null");

                                    LabgeDatabase.ExecuteSql("Interface_SetPatientResultText '" + param["TargetLabRegDate"].ToString() + "', '" + param["TargetLabRegNo"] + "'" +
                                        ", '" + strTargetOrderCode + "', '" + strTargetTestCode + "', '" + strTargetTestSubCode + "'" +
                                        ", '" + dtSource.Rows[i]["서술"].ToString() + "', 'CM099', null");
                                }
                            }
                            else if (strSourceOrderCode == "22057" || strSourceOrderCode == "22036")
                            {
                                strTargetTestCode = dtTarget.Rows[0]["테스트코드"].ToString();
                                if (strTargetTestCode == "22057" || strTargetTestCode == "22036")
                                {
                                    strTargetOrderCode = strTargetTestCode;
                                    strTargetTestSubCode = strTargetTestCode;
                
                                    string strInterface_Result, strInterface_Text;
                                    strInterface_Result = "Interface_SetPatientResult_Test '" + param["TargetLabRegDate"].ToString() + "', '" + param["TargetLabRegNo"] + "'" +
                                        ", '" + strTargetOrderCode + "', '" + strTargetTestSubCode + "', '" + strTargetTestSubCode + "', '" + dtSource.Rows[i]["결과1"].ToString() + "'" +
                                        ", '" + dtSource.Rows[i]["결과2"].ToString() + "', Null, Null, Null, Null, 'CM099', null";

                                    strInterface_Text = "Interface_SetPatientResultText '" + param["TargetLabRegDate"].ToString() + "', '" + param["TargetLabRegNo"] + "'" +
                                        ", '" + strTargetOrderCode + "', '" + strTargetTestCode + "', '" + strTargetTestSubCode + "'" +
                                        ", '" + dtSource.Rows[i]["서술"].ToString().Replace("본 검사는 취합검사(Pooling test)이며, ", "") + "', 'CM099', null";


                                    LabgeDatabase.ExecuteSql(strInterface_Text);

                                    LabgeDatabase.ExecuteSql(strInterface_Result);
                                }
                            }
                            else
                            {
                                LabgeDatabase.ExecuteSql("Interface_SetPatientResult_Test '" + param["TargetLabRegDate"].ToString() + "', '" + param["TargetLabRegNo"].ToString() + "', '" + dtSource.Rows[i]["의뢰코드"].ToString() + "'" +
                                    ", '" + dtSource.Rows[i]["테스트코드"].ToString() + "', '" + dtSource.Rows[i]["T코드"].ToString() + "', '" + dtSource.Rows[i]["결과1"].ToString() + "', '" + dtSource.Rows[i]["결과2"].ToString() + "', Null, Null, Null, Null, 'CM099', null");

                                if (dtSource.Rows[i]["서술"].ToString() != "")
                                {
                                    LabgeDatabase.ExecuteSql("Interface_SetPatientResultText '" + param["TargetLabRegDate"].ToString() + "', '" + param["TargetLabRegNo"].ToString() + "', '" + dtSource.Rows[i]["의뢰코드"].ToString() + "'" +
                                        ", '" + dtSource.Rows[i]["테스트코드"].ToString() + "', '" + dtSource.Rows[i]["T코드"].ToString() + "', '" + dtSource.Rows[i]["서술"].ToString() + "', 'CM099', null");
                                }
                            }
                        }
                    }


                    merge = "merge LabRegRemarkPart as a "
                          + "using (select LabRegDate, LabRegNo, PartCode,RemarkCode, RemarkText, EditTime, EditMemberID  "
                          + "from LabRegRemarkPart where LabRegDate = '" + param["SourceLabRegDate"] + "' and LabRegNo = '" + param["SourceLabRegNo"] + "') as b "
                          + "on a.LabRegdate = '" + param["TargetLabRegDate"] + "' and a.LabRegNo = '" + param["TargetLabRegNo"] + "' and a.PartCode = b.PartCode "
                          + "when Not Matched then "
                          + "insert values(NewID(), '" + param["TargetLabRegDate"] + "', '" + param["TargetLabRegNo"] + "', b.PartCode, b.RemarkCode, b.RemarkText, b.EditTime, b.EditMemberID) "
                          + "when Matched then "
                          + "update set a.RemarkText = b.RemarkText, a.EditTime = b.EditTime, a.EditMemberID = b.EditMemberID; ";
                    LabgeDatabase.ExecuteSql(merge);

                }


                if (param["ReportCode"].ToString() == "03")
                {
                    for (int i = 0; i < dtSource.Rows.Count; i++)
                    {
                        if (dtSource.Rows[i]["헤더"].ToString() != "True")
                        {
                            if (dtSource.Rows[i]["서술"].ToString() != "")
                            {
                                LabgeDatabase.ExecuteSql("Interface_SetPatientResultText '" + param["TargetLabRegDate"] + "', '" + param["TargetLabRegNo"] + "'" +
                                    ", '" + dtSource.Rows[i]["의뢰코드"].ToString() + "', '" + dtSource.Rows[i]["테스트코드"].ToString() + "', '" + dtSource.Rows[i]["T코드"].ToString() + "'" +
                                    ", '" + dtSource.Rows[i]["서술"].ToString().Replace("'", "''") + "', 'CM099', null");
                            }
                        }
                    }
                
                    string sqlupdate = "update LabRegReport "
                           + "set ReportBunjuNo = '" + dtSource.Rows[0]["병리번호"].ToString() + "' "
                           + "where LabRegDate = '" + param["TargetLabRegDate"] + "' and LabRegNo = '" + param["TargetLabRegNo"] + "' and ReportCode = '" + param["ReportCode"] + "' ";

                    LabgeDatabase.ExecuteSql(sqlupdate);

                    sqlupdate = "update LabRegTest "
                           + "set DoctorCode = '" + dtSource.Rows[0]["판독의코드"].ToString() + "' "
                           + "where LabRegDate = '" + param["TargetLabRegDate"] + "' and LabRegNo = '" + param["TargetLabRegNo"] + "' "
                           + "and TestCode = '" + dtTarget.Rows[0]["테스트코드"] + "' ";

                    LabgeDatabase.ExecuteSql(sqlupdate);
                }

                merge = "merge LabRegRemark as a "
                          + "using (select LabRegDate, LabRegNo, ReportCode, RemarkFormat, RemarkText, RemarkNotice, EditTime, EditorMemberID  "
                          + "from LabRegRemark where LabRegDate = '" + param["SourceLabRegDate"] + "' and LabRegNo = '" + param["SourceLabRegNo"] + "' and ReportCode = '" + param["ReportCode"] + "') as b "
                          + "on a.LabRegDate = '" + param["TargetLabRegDate"] + "' and a.LabRegNo = '" + param["TargetLabRegNo"] + "' and a.ReportCode = '" + param["ReportCode"] + "' "
                          + "when Not Matched then "
                          + "Insert values(Newid(), '" + param["TargetLabRegDate"] + "', '" + param["TargetLabRegNo"] + "', '" + param["ReportCode"] + "', b.RemarkFormat, b.RemarkText, b.RemarkNotice, b.EditTime, b.EditorMemberID) "
                          + "when Matched then "
                          + "Update set a.RemarkFormat = b.RemarkFormat, a.RemarkText = b.RemarkText, a.RemarkNotice = b.RemarkNotice, a.EditTime = b.EditTime, a.EditorMemberID = b.EditorMemberid; ";
                
                LabgeDatabase.ExecuteSql(merge);

                string update = "update LabRegReport\n"
                           + "set ReportMemo =\n"
                           + "case when isnull(a.ReportMemo, '') != '' then a.ReportMemo + CHAR(13) + CHAR(10) else '' end\n"
                           + "+ case when ISNULL(b.ReportMemo, '') != '' then b.ReportMemo + CHAR(13) + CHAR(10) else '' end\n"
                           + "+ '결과이동 : " + dtSource.Rows[0]["접수일"].ToString().Substring(0, 10) + " / " + dtSource.Rows[0]["접수번호"].ToString() + " " + dtSource.Rows[0]["거래처명"].ToString() + " " + dtSource.Rows[0]["수진자명"].ToString() + "' + char(13) + char(10)\n"
                           + "+ '이동일 : " + DateTime.Now.ToString("yyyy-MM-dd") + " " + param["MemberName"] + "'\n"
                           + "from LabRegReport as a inner join\n"
                           + "(select ReportCode, isnull(ReportMemo, '') as ReportMemo from LabRegReport where LabRegDate = '" + param["SourceLabRegDate"] + "' and LabRegNo = '" + param["SourceLabRegNo"] + "' and ReportCode = '" + param["ReportCode"] + "') as b on a.ReportCode = '" + param["ReportCode"] + "'\n"
                           + "where a.LabRegDate = '" + param["TargetLabRegDate"] + "' and a.LabRegNo = '" + param["TargetLabRegNo"]  + "' and a.ReportCode = '" + param["ReportCode"] + "'\n";
                //MessageBox.Show(sql_Update);



                LabgeDatabase.ExecuteSql(update);


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