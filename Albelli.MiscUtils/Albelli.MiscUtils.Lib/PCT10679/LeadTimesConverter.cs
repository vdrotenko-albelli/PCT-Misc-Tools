using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT10679
{
    public static class LeadTimesConverter
    {
        public static DataTable ToDataTable(this GetAllLeadTimesResponse response)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn(nameof(LeadTimeResponse.PlantCode), typeof(string)));
            dt.Columns.Add(new DataColumn(nameof(LeadTimeResponse.ArticleCode), typeof(string)));
            dt.Columns.Add(new DataColumn(nameof(ProductOptionLeadTime.ProductOptionName), typeof(string)));
            dt.Columns.Add(new DataColumn(nameof(ProductOptionLeadTime.ProductOptionValue), typeof(string)));
            dt.Columns.Add(new DataColumn(nameof(LeadTime.BaseLeadTime), typeof(int)));
            dt.Columns.Add(new DataColumn(nameof(LeadTime.ExtraLeadTime), typeof(int)));
            foreach(var lt in response.LeadTimes)
            {
                DataRow dr = dt.NewRow();
                FillDataRow(dr, lt.PlantCode, lt.ArticleCode, null, null, lt.LeadTime);
                dt.Rows.Add(dr);
                if (true == lt.ProductOptionLeadTimes?.Any())
                foreach(var polt in lt.ProductOptionLeadTimes) 
                {
                    DataRow drpolt = dt.NewRow();
                    FillDataRow(drpolt, lt.PlantCode, lt.ArticleCode, polt.ProductOptionName, polt.ProductOptionValue, polt.LeadTime);
                    dt.Rows.Add(drpolt);
                }
            }
            return dt;
        }

        private static void FillDataRow(DataRow dr, string plant, string pap, string optionName, string optionValue, LeadTime lt)
        {
            dr[nameof(LeadTimeResponse.PlantCode)] = plant;
            dr[nameof(LeadTimeResponse.ArticleCode)] = pap;
            if (!string.IsNullOrWhiteSpace(optionName))
                dr[nameof(ProductOptionLeadTime.ProductOptionName)] = optionName;
            if (!string.IsNullOrWhiteSpace(optionValue))
                dr[nameof(ProductOptionLeadTime.ProductOptionName)] = optionValue;
            dr[nameof(LeadTime.BaseLeadTime)] = lt.BaseLeadTime;
            if (lt.ExtraLeadTime != null)
                dr[nameof(LeadTime.ExtraLeadTime)] = (int)lt.ExtraLeadTime;
        }

        public static List<FlattenedLeadTime> Flatten(this GetAllLeadTimesResponse response)
        {
            List<FlattenedLeadTime> rslt = new();
            foreach (var lt in response.LeadTimes)
            {
                FlattenedLeadTime dr = new();
                FillFlattened(dr, lt.PlantCode, lt.ArticleCode, null, null, lt.LeadTime);
                rslt.Add(dr);
                if (true == lt.ProductOptionLeadTimes?.Any())
                    foreach (var polt in lt.ProductOptionLeadTimes)
                    {
                        FlattenedLeadTime drpolt = new();
                        FillFlattened(drpolt, lt.PlantCode, lt.ArticleCode, polt.ProductOptionName, polt.ProductOptionValue, polt.LeadTime);
                        rslt.Add(drpolt);
                    }
            }
            return rslt;

        }
        private static void FillFlattened(FlattenedLeadTime dr, string plant, string pap, string optionName, string optionValue, LeadTime lt)
        {
            dr.PlantCode = plant;
            dr.ArticleCode = pap;
            if (!string.IsNullOrWhiteSpace(optionName))
                dr.ProductOptionName = optionName;
            if (!string.IsNullOrWhiteSpace(optionValue))
                dr.ProductOptionValue = optionValue;
            dr.BaseLeadTime = lt.BaseLeadTime;
            if (lt.ExtraLeadTime != null)
                dr.ExtraLeadTime = (int)lt.ExtraLeadTime;
        }


    }
}
