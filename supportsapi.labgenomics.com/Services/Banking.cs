using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace supportsapi.labgenomics.com.Services
{
    public static class Banking
    {
        public static string BarcodeToSampleCode(string bankingKind, string barcode)
        {
            string sampleCode = string.Empty;
            if (bankingKind == "CKD 2" || bankingKind == "소아 CKD2")
            {
                sampleCode = barcode.Substring(0, 1) + "-" + barcode.Substring(1, 4) + "-" + barcode.Substring(5, 2);
            }

            return sampleCode;
        }
    }
}