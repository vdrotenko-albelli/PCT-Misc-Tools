using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.Excel
{
    public static class ExcelUtils
    {
        public static void PrintDataRows(DataRow[] rows, DataTable ds, TextWriter writer)
        {
            foreach (DataRow dr in rows)
            {
                for (int c = 0; c < ds.Columns.Count; c++)
                {
                    if (c > 0)
                        writer.Write('\t');
                    writer.Write(dr[c] as string);

                }
                writer.WriteLine();
            }
        }

    }
}
