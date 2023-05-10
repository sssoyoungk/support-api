using Newtonsoft.Json.Linq;
using supportsapi.labgenomics.com.Attributes;
using supportsapi.labgenomics.com.Services;
using System;
using System.Net;
using System.Web.Http;

namespace supportsapi.labgenomics.com.Controllers.Manage
{
    public class AdjustFeeController : ApiController
    {


        [Route("api/Diagnostic/AdjustFee/Year")]
        public IHttpActionResult GetYear()
        {
            string sql;
            sql = "select distinct Year from ProgMonthlystatistics order by Year desc";

            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


        [Route("api/Diagnostic/AdjustFee/CompNoticeText")]
        public IHttpActionResult GetCompNoticeText()
        {
            string sql;

            sql = "select distinct CompNoticeText from ProgCompNotice where CompNoticeKind = '13'";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


        [Route("api/Diagnostic/AdjustFee/CompGroup02")]
        public IHttpActionResult GetCompGroup02()
        {
            string sql;
            sql = "select distinct a.CompGroup02, b.CompCode from ProgCompGroupCode as a inner join ProgCompCode as b on a.CompGroup02 = b.CompName where a.CompGroup01 = '지사' union all select 'HKS', '000801'";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


        [Route("api/Manage/AdjustFee/Marketing")]
        public IHttpActionResult GetMarketing(string branch)
        {
            string sql;

            sql = "";
            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);
        }


        [Route("api/Manage/AdjustFee/SearchBranch")]
        public IHttpActionResult GetSearchBranch(string branch, string[] compCodeEone)
        {
            string sql = "";
            string groupCode = string.Empty;
            string whereOption = string.Empty;


            //if (branch == "HKS")
            //{
            //    groupCode = "000801";
            //    whereOption = "and h.CompNoticeText = 'HKS'";
            //}
            //else
            //{
            //    DataRow[] dr = dtBranch.Select("CompGroup02 = '" + cbeBranch.Text + "'");
            //    //MessageBox.Show(dr[0][1].ToString());
            //    strGcode = dr[0][1].ToString();
            //    //strWhere = "and i.CompGroup02 = '" + cbeBranch.Text + "'";
            //    strWhere = "and (i.CompGroup02 = '" + cbeBranch.Text + "' or h.CompNoticeText = '" + cbeBranch.Text + "')";
            //}
            //
            //if (Array.IndexOf(strCompCode_eone, strGcode) < 0)
            //{
            //    sql_AdjustFee = "select a.CompCode as 거래처코드, a.CompName as 거래처명, a.OrderCode as 의뢰코드, a.OrderDisplayName as 항목명, COUNT(a.OrderCode) as 건수, convert(numeric(12, 0), a.OrderDefaultPrice) as 기준수가, COUNT(a.OrderCode) * convert(numeric(12, 0), a.OrderDefaultPrice) as 기준수가합\n"
            //              + ", convert(numeric(12, 0), a.CompDemandPrice) as 청구수가, COUNT(a.OrderCode) * convert(numeric(12, 0), a.CompDemandPrice) as 청구수가합\n"
            //              + ", convert(numeric(12, 1), case when a.OrderDefaultPrice > 0 then  (1 - a.CompDemandPrice / a.OrderDefaultPrice) * 100 else 0 end) as 거래처할인율\n"
            //              + ", CONVERT(numeric(12, 0), a.AdditivePrice) as 질가산료, COUNT(a.OrderCode) * CONVERT(numeric(12, 0), a.AdditivePrice) as 질가산료합\n"
            //              + ", a.지사청구가 as 계약수가\n"
            //              + ", case when a.CompDemandPrice = '0' then '0'\n"
            //              + "  else COUNT(a.OrderCode) * a.지사청구가 + case when ISNULL(a.OutsideCompName, '') != '' then COUNT(a.OrderCode) * CONVERT(numeric(12, 0), a.AdditivePrice) else COUNT(a.OrderCode) * CONVERT(numeric(12, 0), a.AdditivePrice) / 2 end\n"
            //              + "  end as 계약수가합\n"
            //              + ", case when a.CompDemandPrice = '0' then '0'\n"
            //              + "  else case when a.OrderDisplayName like '%차감%' then '0' else COUNT(a.OrderCode) * convert(numeric(12, 1), a.CompDemandPrice) - (COUNT(a.OrderCode) * a.지사청구가 + case when ISNULL(a.OutsideCompName, '') != '' then COUNT(a.OrderCode) * CONVERT(numeric(12, 0), a.AdditivePrice) else COUNT(a.OrderCode) * CONVERT(numeric(12, 0), a.AdditivePrice) / 2 end) end\n"
            //              + "  end as 수수료\n"
            //              + ", convert(numeric(12, 1), case when a.OrderDefaultPrice > 0 then  (1 - a.지사청구가 / a.OrderDefaultPrice) * 100 else 0 end) as 할인율\n"
            //              + ", a.구분, ISNULL(a.OutsideCompName, '') as 외주처, a.OutsidePrice as 외주수가\n"
            //              + "from (select a.CompCode, c.CompName, b.DemandCode as OrderCode, d.OrderDisplayName, e.IsTestOutside, e.TestOutsideCompCode, g.CompName as OutsideCompName, b.OrderDefaultPrice, b.CompDemandPrice, b.AdditivePrice\n"
            //              + ", convert(numeric(12, 1), case when e.IsTestOutside = '1'\n"
            //              + "  then case when dbo.FN_GetOrderPrice('" + strGcode + "', a.LabRegDate, a.LabRegNo, b.DemandCode) > isnull(case when f.IsCompPriceExtraContractPrice = '1' then f.CompPriceExtraContractPrice else b.OrderDefaultPrice * (100 - f.CompPriceExtraDiscountRate) * 0.01 end, '0')\n"
            //              + "       then dbo.FN_GetOrderPrice('" + strGcode + "', a.LabRegDate, a.LabRegNo, b.DemandCode)\n"
            //              + "       else case when f.IsCompPriceExtraContractPrice = '1' then f.CompPriceExtraContractPrice else b.OrderDefaultPrice * (100 - f.CompPriceExtraDiscountRate) * 0.01 end\n"
            //              + "       end\n"
            //              + "  else dbo.FN_GetOrderPrice('" + strGcode + "', a.LabRegDate, a.LabRegNo, b.DemandCode) end) as 지사청구가\n"
            //              + ", case when e.IsTestOutside = '1'\n"
            //              + "  then case when dbo.FN_GetOrderPrice('" + strGcode + "', a.LabRegDate, a.LabRegNo, b.DemandCode) > isnull(case when f.IsCompPriceExtraContractPrice = '1' then f.CompPriceExtraContractPrice else b.OrderDefaultPrice * (100 - f.CompPriceExtraDiscountRate) * 0.01 end, '0')\n"
            //              + "       then dbo.FN_GetOrderPriceKind('" + strGcode + "', a.LabRegDate, a.LabRegNo, b.DemandCode)\n"
            //              + "       else '외주' end\n"
            //              + "  else dbo.FN_GetOrderPriceKind('" + strGcode + "', a.LabRegDate, a.LabRegNo, b.DemandCode) end as 구분\n"
            //              + ", isnull(case when f.IsCompPriceExtraContractPrice = '1' then f.CompPriceExtraContractPrice else b.OrderDefaultPrice * (100 - f.CompPriceExtraDiscountRate) * 0.01 end, '0') as OutsidePrice \n"
            //              + "from LabRegInfo as a inner join LabRegOrder as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
            //              + "inner join ProgCompCode as c on a.CompCode = c.CompCode\n"
            //              + "inner join LabOrderCode as d on b.DemandCode = d.OrderCode\n"
            //              + "inner join ProgCompGroupCode as i on c.CompGroupCode = i.CompGroupCode\n"
            //              + "left outer join LabRegTest as e on a.LabRegDate = e.LabRegDate and a.LabRegNo = e.LabRegNo and b.DemandCode = e.TestCode\n"
            //              + "left outer join ProgOutsidePrice as f on e.TestOutsideCompCode = f.CompCode and e.TestCode = f.OrderCode and a.LabRegDate between f.CompPriceExtraBeginDate and f.CompPriceExtraEndDate\n"
            //              + "left outer join ProgCompCode as g on e.TestOutsideCompCode = g.CompCode\n"
            //              + "left outer join ProgCompNotice as h on a.CompCode = h.CompCode and h.CompNoticeKind = '9'\n"
            //              + "where a.LabRegDate between '" + dateBegin.Value.ToString("yyyy-MM-dd") + "' and '" + dateEnd.Value.ToString("yyyy-MM-dd") + "'\n"
            //              + strWhere + ") as a\n"
            //              + "group by a.CompCode, a.CompName, a.OrderCode, a.OrderDisplayName, a.OrderDefaultPrice, a.CompDemandPrice, a.지사청구가, a.구분, a.OutsideCompName, a.AdditivePrice, a.OutsidePrice  ";
            //}
            //else
            //{
            //    strWhere = "and (g.CompGroup02 = '" + cbeBranch.Text + "' or l.CompNoticeText = '" + cbeBranch.Text + "')";
            //    sql_AdjustFee = "select CompCode as 거래처코드, CompName as 거래처명, TestCode as 의뢰코드, TestDisplayName as 항목명, count(TestCode) as 건수, OrderDefaultPrice as 기준수가, convert(numeric(12,0), sum(OrderDefaultPrice)) as 기준수가합\n"
            //                  + ", convert(numeric(12, 0), TestDemandPrice) as 청구수가, convert(numeric(12, 0), sum(TestDemandPrice)) as 청구수가합, convert(numeric(12, 1), CompDiscountRatio) as 거래처할인율, convert(numeric(12, 0), AdditivePrice) as 질가산료, convert(numeric(12, 0), sum(AdditivePrice)) as 질가산료합\n"
            //                  + ", convert(numeric(12, 0), case when TestDisplayName like '%UniCAP%' then OrderDefaultPrice * 50 / 100 when 지사수가 > OutsidePrice then 지사수가 else OutsidePrice end) as 계약수가\n"
            //                  + ", sum(case when TestDisplayName like '%UniCAP%' then OrderDefaultPrice * 50 / 100 when 지사수가 > OutsidePrice then 지사수가 else OutsidePrice end) as 계약수가합\n"
            //                  + ", sum(TestDemandPrice - case when TestDisplayName like '%UniCAP%' then OrderDefaultPrice * 50 / 100 when 지사수가 > OutsidePrice then 지사수가 else OutsidePrice end)--수수수료일반\n"
            //                  + "+ case when case when TestDisplayName like '%UniCAP%' then 'UniCAp' when 지사수가 > OutsidePrice then PriceKind else case when IsTestOutside = '1' then '외주' else '' end end = '특별'--지사특별수가일때 질가산료 추가\n"
            //                  + "then sum(AdditivePrice) / 2 else 0 end as 수수료\n"
            //                  + ", convert(numeric(12, 1), case when OrderDefaultPrice > 0 then(1 - case when TestDisplayName like '%UniCAP%' then OrderDefaultPrice * 50 / 100 when 지사수가 > OutsidePrice then 지사수가 else OutsidePrice end / OrderDefaultPrice) * 100 else 0 end) as 할인율\n"
            //                  + ", case when TestDisplayName like '%UniCAP%' then 'UniCAp' when 지사수가 > OutsidePrice then PriceKind else case when IsTestOutside = '1' then '외주' else '' end end as 구분\n"
            //                  + ", OutsideCompName as 외주처, OutsidePrice as 외주수가\n"
            //                  + "from\n"
            //                  + "(select *,\n"
            //                  + "case when PriceKind != '' then ExtraPrice\n"
            //                  + "when IsTestOutside = 0 and TestKind = '일반'\n"
            //                  + "then case when CompDiscountRatio <= 50.0 then TestDemandPrice * 50 / 100\n"
            //                  + "when CompDiscountRatio <= 55.0 then TestDemandPrice * 55 / 100\n"
            //                  + "when CompDiscountRatio <= 60.0 then TestDemandPrice * 60 / 100\n"
            //                  + "when CompDiscountRatio <= 65.0 then TestDemandPrice * 65 / 100\n"
            //                  + "when CompDiscountRatio <= 70.0 then TestDemandPrice * 70 / 100\n"
            //                  + "when CompDiscountRatio > 70.0 then TestDemandPrice * 75 / 100\n"
            //                  + "end\n"
            //                  + "when(IsTestOutside = 0 and TestKind = '병리') or(IsTestOutside = 1 and TestOutsideCompCode not in ('000110', '3957', '000113'))\n"
            //                  + "then case when CompDiscountRatio = 0.0 then TestDemandPrice * 50 / 100\n"
            //                  + "when CompDiscountRatio <= 10.0 then TestDemandPrice * 55 / 100\n"
            //                  + "when CompDiscountRatio <= 20.0 then TestDemandPrice * 60 / 100\n"
            //                  + "when CompDiscountRatio <= 30.0 then TestDemandPrice * 65 / 100\n"
            //                  + "when CompDiscountRatio > 30.0 then TestDemandPrice * 70 / 100\n"
            //                  + "end\n"
            //                  + "when TestKind like '%미생물%' then OrderDefaultPrice * (100 - 73) / 100\n"
            //                  + "when IsTestOutside = 1 and TestKind like '%일반%' then OrderDefaultPrice * (100 - 65) / 100\n"
            //                  + "when IsTestOutside = 1 and TestKind like '%병리%' then OrderDefaultPrice * (100 - 40) / 100\n"
            //                  + "else '0'\n"
            //                  + "end as 지사수가\n"
            //                  + "from\n"
            //                  + "(select i.CompCode, i.CompName, i.TestCode, i.TestDisplayName, i.IsTestOutside, i.TestOutsideCompCode, h.CompName as OutsideCompName\n"
            //                  + ", case when j.OrderPriceGroupCode = '03' then case i.IsTestOutside when '1' then '병리(외주)' else '병리' end\n"
            //                  + "when j.OrderPriceGroupCode = '04' then case i.IsTestOutside when '1' then '미생물(외주)' else '미생물' end\n"
            //                  + "else case i.IsTestOutside when '1' then '일반(외주)' else '일반' end end as TestKind\n"
            //                  + ", j.OrderPriceGroupCode, j.OrderDefaultPrice, i.TestDemandPrice\n"
            //                  + ", case when i.AdditivePrice > 0 then j.AdditivePrice else 0 end as AdditivePrice\n"
            //                  + ", convert(numeric(12, 1), case when i.TestDefaultPrice > 0 then(1 - (i.TestDemandPrice - case when i.AdditivePrice > 0 then j.AdditivePrice else 0 end) / j.OrderDefaultPrice) * 100.0 else 0 end) as CompDiscountRatio\n"
            //                  + ", isnull(case when k.IsCompPriceExtraContractPrice = '1' then k.CompPriceExtraContractPrice else i.TestDefaultPrice * (100 - k.CompPriceExtraDiscountRate) * 0.01 end, '0') as OutsidePrice\n"
            //                  + ", dbo.FN_GetOrderPriceKind('" + strGcode + "', i.LabRegDate, i.LabRegNo, i.TestCode) as PriceKind\n"
            //                  + ", case when dbo.FN_GetOrderPriceKind('" + strGcode + "', i.LabRegDate, i.LabRegNo, i.TestCode) != '' then dbo.FN_GetOrderPrice('" + strGcode + "', i.LabRegDate, i.LabRegNo, i.TestCode) else 0 end as ExtraPrice\n"
            //                  + "from\n"
            //                  + "(select distinct a.CompCode, a.LabRegDate, a.LabRegNo, case when c.OrderDisplayName like '%(연동)%' then b.OrderCode else d.TestCode end as TestCode\n"
            //                  + ", case when c.OrderDisplayName like '%(연동)%' then c.OrderDisplayName else e.TestDisplayName end as TestDisplayName\n"
            //                  + ", case when c.OrderDisplayName like '%(연동)%' then b.CompDemandPrice else d.TestDemandPrice end as TestDemandPrice\n"
            //                  + ", case when c.OrderDisplayName like '%(연동)%' then b.OrderDefaultPrice else d.TestDefaultPrice end as TestDefaultPrice\n"
            //                  + ", d.IsTestOutside, d.TestOutsideCompCode, b.AdditivePrice, f.CompName\n"
            //                  + "from\n"
            //                  + "LabRegInfo as a\n"
            //                  + "inner join LabRegOrder as b on a.LabRegDate = b.LabRegDate and a.LabRegNo = b.LabRegNo\n"
            //                  + "inner join LabOrderCode as c on b.OrderCode = c.OrderCode\n"
            //                  + "inner join LabRegTest as d on a.LabRegDate = d.LabRegDate and a.LabRegNo = d.LabRegNo and b.OrderCode = d.OrderCode\n"
            //                  + "inner join LabTestCode as e on d.TestCode = e.TestCode\n"
            //                  + "inner join ProgCompCode as f on a.CompCode = f.CompCode\n"
            //                  + "inner join ProgCompGroupCode as g on f.CompGroupCode = g.CompGroupCode\n"
            //                  + "left outer join ProgCompNotice as l on a.CompCode = l.CompCode and l.CompNoticeKind = '9'\n"
            //                  + "where a.LabRegDate between '" + dateBegin.Value.ToString("yyyy-MM-dd") + "' and '" + dateEnd.Value.ToString("yyyy-MM-dd") + "'\n"
            //                  //+ "and(g.CompGroup02 = '동부지사' or l.CompNoticeText = '동부지사')\n"
            //                  + strWhere
            //                  + "--and a.CompCode = '83010'\n"
            //                  + ") as i\n"
            //                  + "inner join LabOrderPrice as j on i.TestCode = j.OrderCode and i.LabRegDate between j.OrderPriceBeginDate and j.OrderPriceEndDate\n"
            //                  + "left outer join ProgCompCode  h on i.TestOutsideCompCode = h.CompCode\n"
            //                  + "left outer join ProgOutsidePrice as k on i.TestOutsideCompCode = k.CompCode and i.TestCode = k.OrderCode and i.LabRegDate between k.CompPriceExtraBeginDate and k.CompPriceExtraEndDate) as r) as r2\n"
            //                  + "group by CompCode, CompName, TestCode, TestDisplayName, OrderDefaultPrice, TestDemandPrice, CompDiscountRatio, AdditivePrice, 지사수가, OutsidePrice, IsTestOutside, PriceKind, OutsideCompName\n"
            //                  + "order by r2.CompCode";
            //
            //}


            var arrResponse = LabgeDatabase.SqlToJArray(sql);
            return Ok(arrResponse);

        }
        
    }
}