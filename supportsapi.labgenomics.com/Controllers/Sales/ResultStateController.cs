using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using supportsapi.labgenomics.com.ElabLibService;
using supportsapi.labgenomics.com.Services;
using System;
using System.Configuration;
using System.Data;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml;

namespace supportsapi.labgenomics.com.Controllers.Sales
{
    //[SupportsAuth]
    [EnableCors(origins: "*", headers: "*", methods: "*", PreflightMaxAge = 28800)]
    [Route("api/sales/ResultState")]
    public class ResultStateController : ApiController
    {
        [Route("api/sales/ResultState/CompList")]
        public IHttpActionResult GetCompList()
        {
            string sql;
            sql =
                "SELECT pcc.CompName + '(' + rtcs.CompCode + ')' AS CompName, rtcs.CompCode, rtcs.TransKind, pcc.CompInstitutionNo\r\n" +
                "FROM RsltTransCompSet rtcs\r\n" +
                "JOIN ProgCompCode pcc\r\n" +
                "ON rtcs.CompCode = pcc.CompCode\r\n" +
                "WHERE TransKind IN ('Bizment', 'Eghis')\r\n";
            JArray arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }

        [Route("api/sales/ResultState/Bizment")]
        public IHttpActionResult GetBizment(string hosNum, DateTime beginDate, DateTime endDate)
        {
            JArray arrResponse = new JArray();

            ELabLibServiceSoapClient elabClient = new ELabLibServiceSoapClient();
            string response = elabClient.HResultServerReceive(hosNum, beginDate.ToString("yyyyMMdd"), endDate.ToString("yyyyMMdd"), "4");
            response = "<results>" + response + "</results>";
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(response);
            XmlNodeList nodeList = xmlDocument.SelectNodes("results/LAB");
            foreach (XmlElement elementLab in nodeList)
            {
                string chartNo = elementLab.GetAttribute("CNO");
                foreach (XmlElement elementConsignExam in elementLab.SelectNodes("CONSIGNEXAM"))
                {
                    string patientName = elementConsignExam.GetAttribute("PNAME");
                    string gender = elementConsignExam.GetAttribute("SEX");
                    string age = elementConsignExam.GetAttribute("AGE");
                    foreach (XmlElement elementExam in elementConsignExam.SelectNodes("EXAM"))
                    {
                        string registDate = elementExam.GetAttribute("JUBSUDATE");
                        foreach (XmlElement elementResult in elementExam.SelectNodes("RESULT"))
                        {
                            string testName = elementResult.GetAttribute("OCSNAME");
                            string bzCode = elementResult.GetAttribute("BZCODE");
                            string sbzCode = elementResult.GetAttribute("SBZCODE");
                            string resultVal = elementResult.SelectSingleNode("VALDATA").ChildNodes[0].InnerText;
                            string resultText = elementResult.SelectSingleNode("DESDATA").ChildNodes[0].InnerText;

                            JObject objResponse = new JObject();
                            objResponse.Add("CompRegistDate", registDate);
                            objResponse.Add("ChartNo", chartNo);
                            objResponse.Add("PatientName", patientName);
                            objResponse.Add("Gender", gender);
                            objResponse.Add("Age", age);
                            objResponse.Add("TestCode", bzCode);
                            objResponse.Add("TestSubCode", sbzCode);
                            objResponse.Add("TestName", testName);
                            objResponse.Add("ResultVal", resultVal);
                            objResponse.Add("ResultText", resultText);

                            arrResponse.Add(objResponse);
                        }
                    }
                }
            }
            return Ok(arrResponse);
        }

        [Route("api/sales/ResultState/Eghis")]
        public IHttpActionResult GetEghis(string hosNum, DateTime beginDate, DateTime endDate)
        {
            NpgsqlConnection eghisConn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["EghisConnection"].ConnectionString);
            eghisConn.Open();

            try
            {
                string sql;
                sql =
                    $"SELECT io.ord_ymd AS \"CompRegistDate\"\r\n" +
                    $"     , io.ptnt_no AS \"ChartNo\", DECODING(ptnt_nm, hosp_no) AS \"PatientName\"\r\n" +
                    $"     , io.sex AS \"Gender\", io.age AS \"Age\"\r\n" +
                    $"     , io.ord_cd AS \"TestCode\", '' AS \"TestSubCode\", io.ord_nm AS \"TestName\"\r\n" +
                    $"     , ir.result_nm AS \"ResultVal\", ir.result_txt AS \"ResultText\"\r\n" +
                    $"FROM interface_ord io\r\n" +
                    $"LEFT outer join interface_result ir\r\n" +
                    $"ON ir.insucode = io.hosp_no \r\n" +
                    $"AND ir.chart_no = io.ptnt_no \r\n" +
                    $"AND ir.ord_ymd = io.ord_ymd\r\n" +
                    $"AND ir.recept_no = io.recept_no \r\n" +
                    $"AND ir.ord_no = io.ord_no \r\n" +
                    $"AND ir.ord_seq_no = io.ord_seq_no \r\n" +
                    $"AND ir.h_ord_cd = io.ord_cd \r\n" +
                    $"WHERE io.ord_ymd BETWEEN '{beginDate.ToString("yyyyMMdd")}' AND '{endDate.ToString("yyyyMMdd")}'\r\n" +
                    $"AND io.hosp_no = '{hosNum}'";

                DataTable dtOrder = new DataTable();
                dtOrder.TableName = "EghisResult";
                
                NpgsqlCommand eghisCmd = new NpgsqlCommand(sql, eghisConn);
                eghisCmd.CommandTimeout = 45;
                NpgsqlDataAdapter eghisAdapter = new NpgsqlDataAdapter(eghisCmd);

                eghisAdapter.Fill(dtOrder);
                JArray arrResponse = JArray.Parse(JsonConvert.SerializeObject(dtOrder));

                return Ok(arrResponse);
            }
            finally
            {
                eghisConn.Close();
            }
        }
    }
}