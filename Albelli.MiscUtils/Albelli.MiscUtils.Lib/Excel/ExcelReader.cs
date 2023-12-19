using ProductionOrderOperationsApi.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.Excel
{
    public static class ExcelReader
    {
        public static DataTable Read(string excelPath, string sheetName = null) 
        {
            string strConnString = $"Driver={{Microsoft Excel Driver (*.xls, *.xlsx, *.xlsm, *.xlsb)}};Dbq={excelPath};Extensions=xls/xlsx;Persist Security Info=False";
            DataTable rslt;
            string actualSheetName = !string.IsNullOrWhiteSpace(sheetName) ? sheetName : "Sheet1";
            using (OdbcConnection oConn = new OdbcConnection(strConnString))
            {
                using (OdbcCommand oCmd = new OdbcCommand())
                {
                    oCmd.Connection = oConn;

                    oCmd.CommandType = System.Data.CommandType.Text;
                    oCmd.CommandText = $"select * from [{actualSheetName}$]";

                    OdbcDataAdapter oAdap = new OdbcDataAdapter();
                    oAdap.SelectCommand = oCmd;

                    rslt = new DataTable();
                    oAdap.Fill(rslt);
                    oAdap.Dispose();
                }
                return rslt;
            }
        }
    }
}
