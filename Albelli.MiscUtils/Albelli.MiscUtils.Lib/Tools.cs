using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib
{
    public static class Tools
    {
        public static DataTable Csv2DataTable(string path, char delimiter, int? expectedColumnCount = null, int? maxBufferSize = null)
        {
            DataTable dt = null;
            using (GenericParsing.GenericParserAdapter p = new GenericParsing.GenericParserAdapter(path))
            {
                p.ColumnDelimiter = delimiter;
                p.FirstRowHasHeader = true;
                //Console.WriteLine($"{nameof(p.MaxBufferSize)}:{p.MaxBufferSize}");
                if (maxBufferSize != null)
                    p.MaxBufferSize = (int)maxBufferSize;
                if (expectedColumnCount != null)
                    p.ExpectedColumnCount = (int)expectedColumnCount;
                dt = p.GetDataTable();
            }

            return dt;
        }

    }
}
