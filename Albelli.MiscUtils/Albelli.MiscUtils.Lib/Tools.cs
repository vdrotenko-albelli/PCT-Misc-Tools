﻿using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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

        public static void DataTableToCsv(DataTable dt, StringBuilder sbTarget)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine
            };
            using (TextWriter w = new StringWriter(sbTarget))
            using (CsvHelper.CsvWriter csvWriter = new CsvHelper.CsvWriter(w, config))
            {
                csvWriter.WriteRecords(dt.Rows);
            }
        }

        public static void DataTableToCsv(DataTable dt, string outputPath)
        {
            var sb = new StringBuilder();
            DataTableToCsv(dt, sb);
            File.WriteAllText(outputPath, sb.ToString());
        }

        public static void ListToCsv<T>(List<T> dt, StringBuilder sbTarget)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine
            };
            using (TextWriter w = new StringWriter(sbTarget))
            using (CsvHelper.CsvWriter csvWriter = new CsvHelper.CsvWriter(w, config))
            {
                csvWriter.WriteRecords<T>(dt);
            }
        }
        public static void ListToCsv<T>(List<T> dt, string outputPath)
        {
            var sb = new StringBuilder();
            ListToCsv<T>(dt, sb);
            File.WriteAllText(outputPath, sb.ToString());
        }
        public static T ReadXML<T>(string fromFile)
        {
            using (FileStream fs = new FileStream(fromFile, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                object o = serializer.Deserialize(fs);
                return (T)o;
            }
        }
        public static void WriteXML<T>(T obj, string saveAs)
        {
            using (FileStream fs = File.Create(saveAs))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(fs, obj);
            }
        }
    }
}
