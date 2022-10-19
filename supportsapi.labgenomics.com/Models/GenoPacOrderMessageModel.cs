using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace supportsapi.labgenomics.com.Models
{
    public class GenoPacOrderMessageModel
    {
        public Nullable<DateTime> RegistDate { get; set; }
        public Nullable<DateTime> LabRegDate { get; set; }
        public Nullable<int> LabRegNo { get; set; }
        public string CompName { get; set; }
        public string PatientName { get; set; }
        public Nullable<int> PatientAge { get; set; }
        public string PatientSex { get; set; }
        public string PatientChartNo { get; set; }
        public string OrderCode { get; set; }
        public string TestDisplayName { get; set; }
        public string SampleCode { get; set; }
        public string SampleName { get; set; }
        public string CompMngName { get; set; }
        public string LabMessage { get; set; }
        public string SalesMessage { get; set; }
    }
}