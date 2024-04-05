using Albelli.MiscUtils.Lib;
using Albelli.MiscUtils.Lib.AWSCli;
using Albelli.MiscUtils.Lib.CodeMetrics;
using Albelli.MiscUtils.Lib.ESLogs;
using Albelli.MiscUtils.Lib.Excel;
using Albelli.MiscUtils.Lib.PCT10481;
using Albelli.MiscUtils.Lib.PCT10679;
using Albelli.MiscUtils.Lib.PCT9944;
using Albelli.MiscUtils.Lib.PCT9944.v1;
using Albelli.MiscUtils.Lib.SQSUtils;
using Amazon;
using Amazon.Util;
using BusinessLogic.Dtos.ProductOptionsDimensionGenerator;
using Centiro.PromiseEngine.Client;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProductionOrderOperationsApi;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security;
using System.Text;
using System.Xml;
using static Albelli.MiscUtils.Lib.PCT9944.Constants;
using static System.Net.WebRequestMethods;
using JsonFormatting = Newtonsoft.Json.Formatting;

namespace Albelli.MiscUtils.CLI
{
    internal class Program
    {
        #region field(s)
        private static Dictionary<string, CmdHandler> _cmdHandlers;
        private static bool _noBuzz = false;
        #endregion

        #region inner type(s)
        private delegate int CmdHandler(string[] args);
        #endregion

        #region Init CMD handlers
        private static Dictionary<string, CmdHandler> AutoDetectCMDHandlers()
        {
            Dictionary<string, CmdHandler> rslt = new Dictionary<string, CmdHandler>();
            Type thisType = typeof(Program);
            MethodInfo[] methods = thisType.GetMethods();
            foreach (MethodInfo mi in methods)
            {
                if (mi.Name == nameof(Main))
                    continue;
                if (mi.ReturnType != typeof(int))
                    continue;
                ParameterInfo[] args = mi.GetParameters();
                if (args == null || args.Length != 1)
                    continue;
                if (args[0].ToString() != "System.String[] args")
                    continue;
                rslt.Add(mi.Name.ToLower(), (CmdHandler)mi.CreateDelegate(typeof(CmdHandler)));
            }
            return rslt;
        }
        private static void FillCmdHandlers()
        {
            #region auto-detect
            _cmdHandlers = AutoDetectCMDHandlers();
            #endregion

            #region fill necessary CMD aliases (if any)
            _cmdHandlers.Add(string.Empty, ShowUsage);
            #endregion
        }
        #endregion

        #region the sacred Main()
        public static int Main(string[] args)
        {
            FillCmdHandlers();
            string keyArg = args.Length > 0 ? args[0].ToLower() : null;
            if (!string.IsNullOrWhiteSpace(keyArg) && _cmdHandlers.ContainsKey(keyArg))
            {
                List<string> origArgs = new List<string>(args);
                origArgs.RemoveAt(0);
                bool isDebug = false;
                int debugSwitchPos = DetectSpecialSwitch(origArgs, "-Debug");
                if (debugSwitchPos != -1)
                {
                    isDebug = true;
                    origArgs.RemoveAt(debugSwitchPos);
                }

                if (isDebug)
                    Console.Read();

                int noBuzzPos = DetectSpecialSwitch(origArgs, "-SkipLogo");
                if (noBuzzPos != -1)
                {
                    _noBuzz = true;
                    origArgs.RemoveAt(noBuzzPos);
                }

                if (!_noBuzz) LogCmdArgs(_cmdHandlers[keyArg].Method.Name, origArgs.ToArray());
                return _cmdHandlers[keyArg](origArgs.ToArray());
            }
            else
                return _cmdHandlers[string.Empty](args);
        }

        private static int DetectSpecialSwitch(List<string> args, string switchName)
        {
            //const string DebugSwitch = "-Debug";
            return args.FindIndex(a => a == switchName || a.ToLower() == switchName.ToLower());
        }
        #endregion

        #region usage & help
        public static int ShowUsage(string[] args)
        {
            Console.WriteLine("A valid command key must be supplied. See below the list of available commands:");
            foreach (string key in _cmdHandlers.Keys)
                Console.WriteLine("  {0}", key);
            return 1;
        }
        #endregion

        #region applied CMD handlers
        public static int ReadESLogDumpCsv(string[] args)
        {
            string logFilePath = args[0];
            int printRows = int.Parse(args[1]);
            string printColNm = args[2];
            var dt = Tools.Csv2DataTable(logFilePath, ',', 35);
            Console.WriteLine($"{nameof(dt)}.Count: {dt.Rows.Count}");
            var colVals = new List<string>();
            //for (int i = 0; i < printRows; i++)
            //{
            //    string s = dt.Rows[i][printColNm] as string;
            //    Console.WriteLine(s);
            //    colVals.Add(s);
            //}
            int i = 0;
            foreach (DataRow dr in dt.Rows)
            {
                string s = dr[printColNm] as string;
                if (i < printRows)
                    Console.WriteLine(s);
                i++;
                colVals.Add(s);
            }
            Console.WriteLine(new string('-', 33));
            Console.WriteLine($"distinct: {colVals.Distinct().Count()}");
            return 0;
        }

        public static int ESLogDumpCsv2RequestStats(string[] args)
        {
            string logFilePath = args[0];
            var dt = Tools.Csv2DataTable(logFilePath, ',', 35);
            Console.WriteLine($"{nameof(dt)}.Count: {dt.Rows.Count}");
            Dictionary<string, string> logField2Props = new Dictionary<string, string> {
                { "_source.Albelli\\.Correlation\\.Http\\.Server\\.Url", nameof(ESLogEntry.Url) },
                { "_source.Timestamp", nameof(ESLogEntry.ts) }
            };
            int i = 0;
            List<ESLogEntry> eSLogEntries = new List<ESLogEntry>();
            foreach (DataRow dr in dt.Rows)
            {
                var curr = new ESLogEntry();
                bool skipCurr = false;
                foreach (string key in logField2Props.Keys)
                {
                    var currFldStr = dr[key] as string;
                    if (logField2Props[key] == nameof(ESLogEntry.Url))
                    {
                        curr.Url = currFldStr;
                    }
                    else if (logField2Props[key] == nameof(ESLogEntry.ts))
                    {
                        int pos = currFldStr.IndexOf('@');
                        if (pos == -1)
                        {
                            skipCurr = true;
                            continue;
                        }
                        var dtStr = currFldStr.Substring(0, pos).Trim();
                        var tmStr = currFldStr.Substring(pos + 1).Trim();
                        DateTime dtt = DateTime.Parse(dtStr);
                        var fullparsableDtTmStr = $"{dtt:yyyy-MM-dd}T{tmStr}";
                        curr.ts = DateTime.Parse(fullparsableDtTmStr);
                    }
                }
                if (!skipCurr)
                    eSLogEntries.Add(curr);
            }
            Console.WriteLine(new string('-', 33));
            var urlsDistinct = eSLogEntries.Select(e => e.Url).Distinct().Count();
            Console.WriteLine($"distinct: {urlsDistinct}");

            //exploring the 'duplicates' patterns
            /*
            Console.WriteLine(new string('-', 33));
            var duplicates = eSLogEntries.GroupBy(e => e.Url).Where(g => g.Count() > 1).ToList();
            Console.WriteLine($"{nameof(duplicates)}: {JsonConvert.SerializeObject(duplicates, JsonFormatting.Indented)}");
            */
            Console.WriteLine(new string('-', 33));
            var hourly = eSLogEntries.GroupBy(e => e.ts.Hour).Select(grp => new { Hour = grp.Key, Count = grp.Count(), RPM = (decimal)grp.Count() / 60.0M }).ToList();
            Console.WriteLine($"{nameof(hourly)}: {JsonConvert.SerializeObject(hourly, JsonFormatting.Indented)}");
            var maxHourly = hourly.Max(e => e.RPM);
            Console.WriteLine($"{nameof(maxHourly)}: {maxHourly}");
            return 0;
        }

        public static int ESLogDumpCsv2RequestStatsBulk(string[] args)
        {
            const decimal normalBestRPM = 60.0M / 4.57M;
            const int ourNormalVUs = 2;
            string logFileListPath = args[0];
            var dumpAllUrlsPath = args.Length > 1 ? args[1] : string.Empty;
            var logFilePaths = System.IO.File.ReadAllLines(logFileListPath);
            List<ESLogEntry> eSLogEntriesAll = new List<ESLogEntry>();

            Dictionary<string, string> logField2Props = new Dictionary<string, string>
                {
                    { "_source.Albelli\\.Correlation\\.Http\\.Server\\.Url", nameof(ESLogEntry.Url) },
                    { "_source.Timestamp", nameof(ESLogEntry.ts) }
                };

            foreach (var logFilePathRaw in logFilePaths)
            {
                var logFilePath = logFilePathRaw?.Trim();
                if (string.IsNullOrWhiteSpace(logFilePath) || !System.IO.File.Exists(logFilePath))
                    continue;
                var dt = Tools.Csv2DataTable(logFilePath, ',');
                //Console.WriteLine($"{nameof(dt)}.Count: {dt.Rows.Count}");
                if (!AllColumnsPresent(dt, logField2Props))
                    continue;
                int i = 0;
                List<ESLogEntry> eSLogEntriesCurr = new List<ESLogEntry>();
                foreach (DataRow dr in dt.Rows)
                {
                    var curr = new ESLogEntry();
                    bool skipCurr = false;
                    foreach (string key in logField2Props.Keys)
                    {
                        var currFldStr = dr[key] as string;
                        if (logField2Props[key] == nameof(ESLogEntry.Url))
                        {
                            curr.Url = currFldStr;
                        }
                        else if (logField2Props[key] == nameof(ESLogEntry.ts))
                        {
                            int pos = currFldStr.IndexOf('@');
                            if (pos == -1)
                            {
                                skipCurr = true;
                                continue;
                            }
                            var dtStr = currFldStr.Substring(0, pos).Trim();
                            var tmStr = currFldStr.Substring(pos + 1).Trim();
                            DateTime dtt = DateTime.Parse(dtStr);
                            var fullparsableDtTmStr = $"{dtt:yyyy-MM-dd}T{tmStr}";
                            curr.ts = DateTime.Parse(fullparsableDtTmStr);
                        }
                    }
                    if (!skipCurr)
                        eSLogEntriesCurr.Add(curr);
                }
                //Console.WriteLine(new string('-', 33));
                eSLogEntriesAll.AddRange(eSLogEntriesCurr);
                //Console.WriteLine($"distinct: {urlsDistinct}");
            }

            if (!string.IsNullOrWhiteSpace(dumpAllUrlsPath))
            {
                var urlsDistinct = eSLogEntriesAll.Select(e => e.Url).Distinct();//.Count();
                System.IO.File.WriteAllLines(dumpAllUrlsPath, urlsDistinct);
            }

            Console.WriteLine(new string('-', 33));
            var hourly = eSLogEntriesAll.GroupBy(e => e.ts.ToString("yyyy-MM-dd_HH")).Select(grp => new { Hour = grp.Key, Count = grp.Count(), RPM = (decimal)grp.Count() / 60.0M }).ToList();
            var maxHourly = hourly.Max(e => e.RPM);
            Console.WriteLine($"max hourly AVG RPM: {maxHourly}");
            var minutely = eSLogEntriesAll.GroupBy(e => e.ts.ToString("yyyy-MM-dd_HH:mm")).Select(grp => new { Minute = grp.Key, Count = grp.Count(), RPM = grp.Count() }).ToList();
            var maxMinutely = minutely.Max(e => e.RPM);
            Console.WriteLine($"max RPM (by minute): {maxMinutely}");

            var secondly = eSLogEntriesAll.GroupBy(e => e.ts.ToString("yyyy-MM-dd_HH:mm:ss")).Select(grp => new { Second = grp.Key, Count = grp.Count() }).ToList();
            var maxSecondly = secondly.Max(e => e.Count);
            Console.WriteLine($"max parallel requests (by second): {maxSecondly}");

            var aboveOurNormalBestPct_HourlyAvg = Math.Round(100.0M * hourly.Where(h => h.RPM > normalBestRPM).Count() / hourly.Count(), 2);
            var aboveOurNormalBestPct_Minutely = Math.Round(100.0M * minutely.Where(h => h.RPM > normalBestRPM).Count() / minutely.Count(), 2);
            var aboveOurNormalBestPct_Secondly = Math.Round(100.0M * secondly.Where(h => h.Count > ourNormalVUs).Count() / secondly.Count(), 2);
            Console.WriteLine($"Pctg of hourly AVG above our normal best RPM({normalBestRPM}):{aboveOurNormalBestPct_HourlyAvg}");
            Console.WriteLine($"Pctg of minutely AVG above our normal best RPM({normalBestRPM}):{aboveOurNormalBestPct_Minutely}");
            Console.WriteLine($"Pctg of parallel requests at a second above our normal parallel VUs ({ourNormalVUs}):{aboveOurNormalBestPct_Secondly}");
            Console.WriteLine(new string('-', 33));
            var minutelyAboveNormalBest = minutely.Where(m => m.RPM > normalBestRPM).OrderByDescending(h => h.RPM);
            Console.WriteLine($"{nameof(minutely)} above our normal best (count={minutelyAboveNormalBest.Count()} out of {minutely.Count}): {JsonConvert.SerializeObject(minutelyAboveNormalBest, JsonFormatting.Indented)}");
            Console.WriteLine(new string('-', 33));
            var hourlyAboveNormalBest = hourly.Where(m => m.RPM > normalBestRPM).OrderByDescending(h => h.RPM);
            Console.WriteLine($"{nameof(hourly)} above our normal best(count={hourlyAboveNormalBest.Count()} out of {hourly.Count}): {JsonConvert.SerializeObject(hourlyAboveNormalBest, JsonFormatting.Indented)}");

            return 0;
        }

        public static int ApiGetJsons(string[] args)
        {
            const string RequestIdHdr = "Request-Id";
            string inFilePath = args[0];
            string token = args[1];
            List<Tuple<string, string>> urlsPaths = new List<Tuple<string, string>>();
            var lns = System.IO.File.ReadAllLines(inFilePath);
            foreach (var ln in lns)
            {
                if (string.IsNullOrWhiteSpace(ln)) continue;
                var flds = ln.Split('\t');
                if (flds.Length < 2) continue;
                urlsPaths.Add(new Tuple<string, string>(flds[0].Trim(), flds[1].Trim()));
            }
            using (HttpClient wc = new HttpClient())
            {
                //wc.DefaultRequestHeaders.Add("Content-Type", "application/json");
                wc.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                wc.DefaultRequestHeaders.Add("Correlation-Context", "X-Is-Load-Test-Request=True");
                foreach (var pair in urlsPaths)
                {
                    try
                    {
                        if (System.IO.File.Exists(pair.Item2)) continue;
                        using (var reqMsg = new HttpRequestMessage(HttpMethod.Get, pair.Item1))
                        {
                            reqMsg.Headers.Add(RequestIdHdr, Guid.NewGuid().ToString());
                            reqMsg.Headers.Add("Accept", "application/json");
                            //reqMsg.Headers.Authorization = $"Bearer {token}";
                            var resp = wc.SendAsync(reqMsg).ConfigureAwait(false).GetAwaiter().GetResult();
                            if (resp.StatusCode == HttpStatusCode.OK)
                            {
                                var responseContent = resp.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                                System.IO.File.WriteAllText(pair.Item2, responseContent);
                            }
                            else
                            {
                                Console.Error.WriteLine($"{pair.Item1}:{resp.StatusCode}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error getting {pair.Item1}:{ex}");
                    }
                }
            }
            return 0;
        }

        public static int ApiPostJsons(string[] args)
        {
            const string RequestIdHdr = "Request-Id";
            string inFilePath = args[0];
            string token = args[1];
            List<Tuple<string, string, string>> urlsPaths = new List<Tuple<string, string, string>>();
            var lns = System.IO.File.ReadAllLines(inFilePath);
            foreach (var ln in lns)
            {
                if (string.IsNullOrWhiteSpace(ln)) continue;
                var flds = ln.Split('\t');
                if (flds.Length < 3) continue;
                urlsPaths.Add(new Tuple<string, string, string>(flds[0].Trim(), flds[1].Trim(), flds[2]));
            }
            using (HttpClient wc = new HttpClient())
            {
                wc.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                wc.DefaultRequestHeaders.Add("Correlation-Context", "X-Is-Load-Test-Request=True");
                foreach (var pair in urlsPaths)
                {
                    try
                    {
                        if (System.IO.File.Exists(pair.Item2)) continue;
                        using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, pair.Item1))
                        {
                            reqMsg.Headers.Add(RequestIdHdr, Guid.NewGuid().ToString());
                            reqMsg.Headers.Add("Accept", "application/json");
                            //reqMsg.Headers.Add("Content-Type", "application/json");
                            reqMsg.Content = new StringContent(pair.Item3, Encoding.UTF8, "application/json");
                            var resp = wc.SendAsync(reqMsg).ConfigureAwait(false).GetAwaiter().GetResult();
                            if (resp.StatusCode == HttpStatusCode.OK)
                            {
                                var responseContent = resp.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                                System.IO.File.WriteAllText(pair.Item2, responseContent);
                            }
                            else
                            {
                                string respContent = null;
                                try
                                {
                                    respContent = resp.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                                }
                                catch { }
                                Console.Error.WriteLine($"{pair.Item1}:{(int)resp.StatusCode} {resp.StatusCode}/{respContent}/{resp.ReasonPhrase}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error getting {pair.Item1}:{ex}");
                    }
                }
            }
            return 0;
        }

        public static int AlbelliReadSALs(string[] args)
        {
            string inFilePath = args[0];
            List<Tuple<string, string>> urlsPaths = new List<Tuple<string, string>>();
            var lns = System.IO.File.ReadAllLines(inFilePath);
            foreach (var ln in lns)
            {
                if (string.IsNullOrWhiteSpace(ln)) continue;
                var flds = ln.Split('\t');
                if (flds.Length < 2) continue;
                urlsPaths.Add(new Tuple<string, string>(flds[0].Trim(), flds[1].Trim()));
            }

            foreach (var pair in urlsPaths)
            {
                var orderDetails = JsonConvert.DeserializeObject<Albelli.CustomerCare.Backoffice.Models.OrderDetails>(System.IO.File.ReadAllText(pair.Item2));
                Console.WriteLine($"{pair.Item2}:{orderDetails.Products.Count}");
            }

            return 0;
        }

        public static int ExtractAllSALsFromESLogs(string[] args)
        {
            const string SALToken = "/orders/";
            char[] sepChars = new char[] { '?', '/', ']', '"', '\'' };
            string csvPath = args[0];
            var dt = Tools.Csv2DataTable(csvPath, ',', maxBufferSize: 102400);
            List<string> rsltRaw = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    string currCellTxt = dr[col.ColumnName] as string;
                    if (string.IsNullOrWhiteSpace(currCellTxt)) continue;
                    int pos0 = currCellTxt.IndexOf(SALToken);
                    if (pos0 == -1) continue;
                    int pos1 = currCellTxt.IndexOfAny(sepChars, pos0 + SALToken.Length + 1);
                    string currSAL = pos1 == -1 ? currCellTxt.Substring(pos0 + SALToken.Length) : currCellTxt.Substring(pos0 + SALToken.Length, pos1 - (pos0 + SALToken.Length));
                    rsltRaw.Add(currSAL);
                }
            }
            rsltRaw.Distinct().ToList().ForEach(s => Console.WriteLine(s));
            //Console.WriteLine(JsonConvert.SerializeObject());
            return 0;
        }

        public static int DetectFFnPOSALs(string[] args)
        {
            string inFilePath = args[0];
            List<Tuple<string, string>> urlsPaths = new List<Tuple<string, string>>();
            var lns = System.IO.File.ReadAllLines(inFilePath);
            foreach (var ln in lns)
            {
                if (string.IsNullOrWhiteSpace(ln)) continue;
                var flds = ln.Split('\t');
                if (flds.Length < 2) continue;
                urlsPaths.Add(new Tuple<string, string>(flds[0].Trim(), flds[1].Trim()));
            }
            Console.WriteLine("SAL\tProds\tPOs\tFF.Prods\tPO.Prods\tFFProdcPct\tPOProdsPct\tFFNotInPOs\tPONotInFFs\tFFNotInProds\tPONotInProds\tFFinPO\tPOinFF");
            foreach (var pair in urlsPaths)
            {
                var orderDetails = JsonConvert.DeserializeObject<Albelli.CustomerCare.Backoffice.Models.OrderDetails>(System.IO.File.ReadAllText(pair.Item2));
                if (orderDetails == null) continue;
                if (orderDetails.Fulfillment == null || orderDetails.ProductionOrders == null) continue;
                if (orderDetails?.Fulfillment?.Products?.Any() != true || orderDetails.ProductionOrders?.Any() != true) continue;
                int currPOProdsCnt = 0;
                orderDetails.ProductionOrders.ForEach(o => o.Shipments.ForEach(s => currPOProdsCnt += s.Products.Count));
                var currProductsIds = orderDetails.Products.Select(p => p.Id).Distinct().ToList();
                //Console.WriteLine($"{nameof(currProductsIds)}:{JsonConvert.SerializeObject(currProductsIds, JsonFormatting.None)}");
                var currFFProductIds = orderDetails.Fulfillment.Products.Select(p => p.ProductCode).Distinct().ToList();
                //Console.WriteLine($"{nameof(currFFProductIds)}:{JsonConvert.SerializeObject(currFFProductIds, JsonFormatting.None)}");
                var currPOProductIds = new List<string>();
                orderDetails.ProductionOrders.ForEach(po => po.Shipments.ForEach(s => currPOProductIds.AddRange(s.Products.Distinct())));
                //Console.WriteLine($"{nameof(currPOProductIds)}:{JsonConvert.SerializeObject(currPOProductIds, JsonFormatting.None)}");
                Console.WriteLine($"{Path.GetFileNameWithoutExtension(pair.Item2)}\t{orderDetails.Products.Count}\t{orderDetails.ProductionOrders.Count}\t{orderDetails.Fulfillment.Products.Count()}\t{currPOProdsCnt}\t{Pct(currProductsIds.Intersect(currPOProductIds).Count(), currProductsIds.Count)}\t{Pct(currProductsIds.Intersect(currFFProductIds).Count(), currProductsIds.Count)}\t{Pct(currFFProductIds.Except(currPOProductIds).Count(), currFFProductIds.Count)}\t{Pct(currPOProductIds.Except(currFFProductIds).Count(), currPOProductIds.Count)}\t{Pct(currFFProductIds.Except(currProductsIds).Count(), currFFProductIds.Count)}\t{Pct(currPOProductIds.Except(currProductsIds).Count(), currPOProductIds.Count)}\t{Pct(currPOProductIds.Intersect(currFFProductIds).Count(), currPOProductIds.Count)}\t{Pct(currFFProductIds.Intersect(currPOProductIds).Count(), currFFProductIds.Count)}");

            }

            return 0;
        }

        public static int ExtractFFProdsOnly(string[] args)
        {
            string inFilePath = args[0];
            List<Tuple<string, string>> urlsPaths = new List<Tuple<string, string>>();
            var lns = System.IO.File.ReadAllLines(inFilePath);
            foreach (var ln in lns)
            {
                if (string.IsNullOrWhiteSpace(ln)) continue;
                var flds = ln.Split('\t');
                if (flds.Length < 2) continue;
                urlsPaths.Add(new Tuple<string, string>(flds[0].Trim(), flds[1].Trim()));
            }
            Console.WriteLine("SAL\tFF.Prods");
            foreach (var pair in urlsPaths)
            {
                var orderDetails = JsonConvert.DeserializeObject<Albelli.CustomerCare.Backoffice.Models.OrderDetails>(System.IO.File.ReadAllText(pair.Item2));
                if (orderDetails == null) continue;
                if (orderDetails.Fulfillment == null || orderDetails.ProductionOrders == null) continue;
                if (orderDetails?.Fulfillment?.Products?.Any() != true || orderDetails.ProductionOrders?.Any() != true) continue;
                int currPOProdsCnt = 0;
                orderDetails.ProductionOrders.ForEach(o => o.Shipments.ForEach(s => currPOProdsCnt += s.Products.Count));
                var currProductsIds = orderDetails.Products.Select(p => p.Id).Distinct().ToList();
                //Console.WriteLine($"{nameof(currProductsIds)}:{JsonConvert.SerializeObject(currProductsIds, JsonFormatting.None)}");
                var currFFProductIds = orderDetails.Fulfillment.Products.Select(p => p.ProductCode).Distinct().ToList();
                //Console.WriteLine($"{nameof(currFFProductIds)}:{JsonConvert.SerializeObject(currFFProductIds, JsonFormatting.None)}");


                //Console.WriteLine($"{nameof(currPOProductIds)}:{JsonConvert.SerializeObject(currPOProductIds, JsonFormatting.None)}");
                Console.WriteLine($"{Path.GetFileNameWithoutExtension(pair.Item2)}\t{string.Join(",", currFFProductIds)}");

            }

            return 0;
        }

        public static int DetectNoFFnPOSALs(string[] args)
        {
            string inFilePath = args[0];
            List<Tuple<string, string>> urlsPaths = new List<Tuple<string, string>>();
            var lns = System.IO.File.ReadAllLines(inFilePath);
            foreach (var ln in lns)
            {
                if (string.IsNullOrWhiteSpace(ln)) continue;
                var flds = ln.Split('\t');
                if (flds.Length < 2) continue;
                urlsPaths.Add(new Tuple<string, string>(flds[0].Trim(), flds[1].Trim()));
            }
            Console.WriteLine("SAL\tProds\tPOs\tFF.Prods\tPO.Prods\tFFProdcPct\tPOProdsPct\tFFNotInPOs\tPONotInFFs\tFFNotInProds\tPONotInProds\tFFinPO\tPOinFF\tProdsNotInPOs\tProdsNotInFF\tOrphanedProds");
            foreach (var pair in urlsPaths)
            {
                var orderDetails = JsonConvert.DeserializeObject<Albelli.CustomerCare.Backoffice.Models.OrderDetails>(System.IO.File.ReadAllText(pair.Item2));
                if (orderDetails == null) continue;
                int currPOProdsCnt = 0;
                orderDetails.ProductionOrders.ForEach(o => o.Shipments.ForEach(s => currPOProdsCnt += s.Products.Count));
                var currProductsIds = orderDetails.Products.Select(p => p.Id).Distinct().ToList();
                //Console.WriteLine($"{nameof(currProductsIds)}:{JsonConvert.SerializeObject(currProductsIds, JsonFormatting.None)}");
                var currFFProductIds = orderDetails.Fulfillment?.Products?.Select(p => p.ProductCode).Distinct().ToList();
                if (currFFProductIds == null) currFFProductIds = new List<string>();
                //Console.WriteLine($"{nameof(currFFProductIds)}:{JsonConvert.SerializeObject(currFFProductIds, JsonFormatting.None)}");
                var currPOProductIds = new List<string>();
                var currFForPOProductIdsRaw = new List<string>();
                currFForPOProductIdsRaw.AddRange(currPOProductIds);
                currFForPOProductIdsRaw.AddRange(currFFProductIds);
                var currFForPOProductIds = currFForPOProductIdsRaw.Distinct().ToList();
                orderDetails.ProductionOrders?.ForEach(po => po.Shipments.ForEach(s => currPOProductIds.AddRange(s.Products.Distinct())));
                //Console.WriteLine($"{nameof(currPOProductIds)}:{JsonConvert.SerializeObject(currPOProductIds, JsonFormatting.None)}");
                Console.WriteLine($"{Path.GetFileNameWithoutExtension(pair.Item2)}\t{orderDetails.Products.Count}\t{orderDetails.ProductionOrders?.Count}\t{orderDetails.Fulfillment?.Products?.Count()}\t{currPOProdsCnt}\t{Pct(currProductsIds.Intersect(currPOProductIds).Count(), currProductsIds?.Count)}\t{((currFFProductIds != null) ? Pct(currProductsIds.Intersect(currFFProductIds).Count(), currProductsIds?.Count) : 0.0M)}\t{Pct(currFFProductIds?.Except(currPOProductIds).Count(), currFFProductIds?.Count)}\t{Pct(currPOProductIds.Except(currFFProductIds).Count(), currPOProductIds.Count)}\t{Pct(currFFProductIds.Except(currProductsIds).Count(), currFFProductIds.Count)}\t{Pct(currPOProductIds.Except(currProductsIds).Count(), currPOProductIds.Count)}\t{Pct(currPOProductIds.Intersect(currFFProductIds).Count(), currPOProductIds.Count)}\t{Pct(currFFProductIds.Intersect(currPOProductIds).Count(), currFFProductIds.Count)}\t{Pct(currProductsIds.Except(currPOProductIds).Count(), currProductsIds?.Count)}\t{((currFFProductIds != null) ? Pct(currProductsIds.Except(currFFProductIds).Count(), currProductsIds?.Count) : 0.0M)}\t{Pct(currProductsIds.Except(currFForPOProductIds).Count(), currProductsIds?.Count)}");

            }

            return 0;
        }

        #region PCT-10480
        public static int DetectMultiAddressPOs(string[] args)
        {
            string inFilePath = args[0];
            List<Tuple<string, string>> urlsPaths = new List<Tuple<string, string>>();
            var lns = System.IO.File.ReadAllLines(inFilePath);
            foreach (var ln in lns)
            {
                if (string.IsNullOrWhiteSpace(ln)) continue;
                var flds = ln.Split('\t');
                if (flds.Length < 2) continue;
                urlsPaths.Add(new Tuple<string, string>(flds[0].Trim(), flds[1].Trim()));
            }
            Console.WriteLine("SAL\tMax(Shipments.Count)");
            foreach (var pair in urlsPaths)
            {
                var orderDetails = JsonConvert.DeserializeObject<Albelli.CustomerCare.Backoffice.Models.OrderDetails>(System.IO.File.ReadAllText(pair.Item2));
                if (orderDetails == null) continue;
                int maxPoShipmentsCnt = orderDetails.ProductionOrders != null && orderDetails.ProductionOrders.Any() ? orderDetails.ProductionOrders.Max(o => o.Shipments != null && o.Shipments.Any() ? o.Shipments.Count : 0) : 0;
                Console.WriteLine($"{Path.GetFileNameWithoutExtension(pair.Item2)}\t{maxPoShipmentsCnt}");
            }

            return 0;
        }

        private static decimal Pct(int? share, int? total)
        {
            if (total == null || share == null) return 0.0M;
            if ((int)total == 0 || (int)share == 0) return 0.0M;
            return Math.Round(100.0M * (decimal)(int)share / (decimal)(int)total, 2);
        }
        #endregion

        private static bool AllColumnsPresent(DataTable dt, Dictionary<string, string> logField2Props)
        {
            foreach (var key in logField2Props.Keys)
            {
                if (!dt.Columns.Contains(key))
                    return false;
            }
            return true;
        }


        public static int GetAwsQueuMessagesCount(string[] args)
        {
            string queuUrl = args[0];
            var attrsJson = AWSCliRunner.CatchCMDOutput($"sqs get-queue-attributes --queue-url \"{queuUrl}\" --attribute-names All  --output json");
            dynamic attrs = JsonConvert.DeserializeObject(attrsJson);
            Console.WriteLine(attrs.Attributes.ApproximateNumberOfMessages);
            return 0;
        }
        public static int GetAwsDLQMessagesCount(string[] args)
        {

            var sqsListJson = args.Length > 0 ? System.IO.File.ReadAllText(args[0]) :
                AWSCliRunner.CatchCMDOutput($"sqs list-queues --output json");
            Console.WriteLine(sqsListJson);
            dynamic queuesList = JsonConvert.DeserializeObject(sqsListJson);
            //string[] queueUrls = queuesList.QueueUrls as string[];
            foreach (var queuUrl in queuesList.QueueUrls)
            {
                var attrsJson = AWSCliRunner.CatchCMDOutput($"sqs get-queue-attributes --queue-url \"{queuUrl}\" --attribute-names All  --output json --cli-read-timeout 5 --cli-connect-timeout 5");
                try
                {
                    dynamic attrs = JsonConvert.DeserializeObject(attrsJson);
                    Console.WriteLine($"{queuUrl}:{attrs?.Attributes?.ApproximateNumberOfMessages}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
            return 0;
        }
        #region PCT-9944
        public static int AnalyzePCT9944Discrepancies(string[] args)
        {
            var inputCsv = args[0];
            var plantOnly = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : string.Empty;
            var outputTablePath = args.Length > 2 && !string.IsNullOrWhiteSpace(args[2]) ? args[2] : string.Empty;
            Dictionary<string, PCT9944FullDiffInfo> diffs = new();
            Dictionary<string, PCT9944FullDiffInfo> missing = new();
            Dictionary<string, PCT9944FullDiffInfo> excessive = new();
            List<DiscrepancyLogEntry> des = PCT9944DiscrepanciesLogReader.Read(inputCsv);
            foreach (DiscrepancyLogEntry dle in des)
            {
                AccountForPCT9944(diffs, dle.DiffStr, dle.XCorrelationId, dle.Input);
                AccountForPCT9944(missing, dle.Missing, dle.XCorrelationId, dle.Input);
                AccountForPCT9944(excessive, dle.Excessive, dle.XCorrelationId, dle.Input);
            }
            if (!string.IsNullOrWhiteSpace(outputTablePath))
            {
                System.IO.File.WriteAllText(outputTablePath, DiscrepancyLogEntryPrinter.Print(des));
            }
            PrintPCT9944Keys(nameof(diffs), diffs);
            PrintPCT9944Keys(nameof(missing), missing);
            PrintPCT9944Keys(nameof(excessive), excessive);
            PrintPCT9944FullDiffInfo(nameof(diffs), diffs);
            return 0;
        }

        public static int AnalyzePCT9944NoDiscrepancies(string[] args)
        {
            var inputJsonPath = args[0];
            var inputParams = JsonConvert.DeserializeObject<NoOrDiscrepancyLogsAnalyzeRequest>(System.IO.File.ReadAllText(inputJsonPath));
            var outputTablePath = inputParams.OutputTablePath;

            Dictionary<string, PCT9944FullDiffInfo> diffs = new();
            List<NoDiscrepancyLogEntry> des = new();
            foreach (var currLog in inputParams.Logs)
            {
                try
                {

                    string inputCsv = currLog;

                    if (!string.IsNullOrWhiteSpace(inputParams.LogsDir) && Directory.Exists(inputParams.LogsDir))
                        inputCsv = Path.Combine(inputParams.LogsDir, inputCsv);
                    var currNDLEs = PCT9944DiscrepanciesLogReader.ReadNoDiscrepancies(inputCsv);

                    foreach (var ndle in currNDLEs)
                    {
                        AccountForPCT9944(diffs, ndle.Centiro, ndle.XCorrelationId, ndle.Input);
                        des.Add(ndle);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error processing log '{currLog}':{ex}");
                }
            }
            if (!string.IsNullOrWhiteSpace(outputTablePath))
            {
                System.IO.File.WriteAllText(outputTablePath, NoDiscrepancyLogEntryPrinter.Print(des, inputParams.PrintPayloadJson));
            }

            var stats = des.GroupBy(e => e.ParsedInput.PlantCode).Select(grp => new { PlantCode = grp.Key, Count = grp.Count() }).ToList();
            //Console.WriteLine(JsonConvert.SerializeObject(stats, JsonFormatting.Indented));
            Console.WriteLine($"PlantCode\tCount");
            stats.ForEach(s => Console.WriteLine($"{s.PlantCode}\t{s.Count}"));
            var missingPlants = inputParams.AllPlantCodes.Except(stats.Select(s => s.PlantCode).ToList());
            Console.WriteLine($"Missing plant(s): [{string.Join(',', missingPlants)}]");
            PrintPCT9944Keys(nameof(diffs), diffs);
            PrintPCT9944FullDiffInfo(nameof(diffs), diffs);
            return 0;
        }

        #region PCT-9944 discrepancies analyzer stuff
        private static void PrintPCT9944FullDiffInfo(string heading, Dictionary<string, PCT9944FullDiffInfo> diffs)
        {
            Console.WriteLine(new string('-', 33));
            Console.WriteLine($"{heading}:");
            Console.WriteLine(JsonConvert.SerializeObject(diffs, JsonFormatting.Indented));
        }

        private static void PrintPCT9944Keys(string heading, Dictionary<string, PCT9944FullDiffInfo> diffs)
        {
            Console.WriteLine(new string('-', 33));
            Console.WriteLine($"{heading}:");
            foreach (var key in diffs.Keys)
                Console.WriteLine($"{key} ({diffs[key].Count})");
        }

        private static void AccountForPCT9944(Dictionary<string, PCT9944FullDiffInfo> target, string key, string currCorrId, string currInput)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;
            if (!target.ContainsKey(key))
                target.Add(key, new PCT9944FullDiffInfo());
            target[key].Count++;
            if (!target[key].XCorrelationIds.Contains(currCorrId)) target[key].XCorrelationIds.Add(currCorrId);
            if (!target[key].Inputs.Contains(currInput)) target[key].Inputs.Add(currInput);
        }
        #endregion

        public static int QueryExcel(string[] args)
        {
            string excelPath = args[0];
            string query = args[1];
            string orderBy = args[2];
            string strConnString = $"Driver={{Microsoft Excel Driver (*.xls, *.xlsx, *.xlsm, *.xlsb)}};Dbq={excelPath};Extensions=xls/xlsx;Persist Security Info=False";
            DataTable ds;
            using (OdbcConnection oConn = new OdbcConnection(strConnString))
            {
                using (OdbcCommand oCmd = new OdbcCommand())
                {
                    oCmd.Connection = oConn;

                    oCmd.CommandType = System.Data.CommandType.Text;
                    oCmd.CommandText = "select * from [Sheet1$]";

                    OdbcDataAdapter oAdap = new OdbcDataAdapter();
                    oAdap.SelectCommand = oCmd;

                    ds = new DataTable();
                    oAdap.Fill(ds);
                    oAdap.Dispose();



                    // ds.Dispose();
                }
                DataRow[] rows = ds.Select(query, orderBy);
                ExcelUtils.PrintDataRows(rows, ds, Console.Out);
            }
            return 0;
        }

        public static int QueryExcel2(string[] args)
        {
            string excelPath = args[0];
            string query = args[1];
            string orderBy = args.Length > 2 ? args[2] : string.Empty;
            using (DataTable ds = ExcelReader.Read(excelPath))
            {
                DataRow[] rows = string.IsNullOrWhiteSpace(orderBy) ? ds.Select(query) : ds.Select(query, orderBy);
                ExcelUtils.PrintDataRows(rows, ds, Console.Out);

            }
            return 0;
        }

        public static int PCT9944_MatchCandidates(string[] args)
        {
            string ACSSMatrixPath = args[0];
            string ACSSZonesPath = args[1];
            string inputJsonPath = args[2];
            DiscrepancyLogEntry req_DLE = null;
            AvailableCarriersRequestV2 req_API = null;
            try
            {
                req_DLE = JsonConvert.DeserializeObject<DiscrepancyLogEntry>(System.IO.File.ReadAllText(inputJsonPath));
            }
            catch { }
            try
            {
                req_API = JsonConvert.DeserializeObject<AvailableCarriersRequestV2>(System.IO.File.ReadAllText(inputJsonPath));
            }
            catch { }

            using (DataTable matrix = ExcelReader.Read(ACSSMatrixPath))
            using (DataTable zones = ExcelReader.Read(ACSSZonesPath))
            {
                ACSSCandidatesFilterer filterer = new ACSSCandidatesFilterer(matrix, zones);
                //DataRow[] drs = filtererProd.PreFilter(request.ParsedInput);
                CalculateCarrierModel ccm = req_DLE?.ParsedInput?.Package?.WeightInGrams > 0 ? req_DLE.ParsedInput : (CalculateCarrierModel)req_API;
                //PrintDataRows(drs, matrix);
                var resp = filterer.Filter(ccm);
                //Console.WriteLine(JsonConvert.SerializeObject(resp, JsonFormatting.Indented));
                Console.WriteLine($"{nameof(resp.PreFilterQuery)}:{resp.PreFilterQuery}");
                resp.Candidates.ForEach(r => Console.WriteLine($"{r.MatrixRow.PK()}:{r.Verdict.ToString()}({string.Join(", ", r.NonMatchingFields)})\t{r.MatrixRow.Priority}"));
            }
            return 0;
        }

        public static int PCT9944BulkRevalidate(string[] args)
        {
            string srcXlsPath = args[0];
            string srcXlsSheet = args[1];
            string apiEndpointProd = args[2];
            string apiAuthTokenProd = args[3];
            string apiEndpointUat = args[4];
            string apiAuthTokenUat = args[5];
            string ACSSMatrixPath_Prod = args[6];
            string ACSSZonesPath = args[7];
            string ACSSMatrixPath_Uat = args[8];

            Console.WriteLine($"ts\tCentiroV1\tCentiroV2\tmatrixTopCandidate_Prod\tmatrixTopCandidate_Uat\tX-CorrelationId_Prod\tApiRespCandidateProd\tApiResponseCandidateUAT\tCentiroV2Req");
            using (DataTable dtMain = ExcelReader.Read(srcXlsPath, srcXlsSheet))
            using (DataTable dtMatrix_Prod = ExcelReader.Read(ACSSMatrixPath_Prod))
            using (DataTable dtZones_Prod = ExcelReader.Read(ACSSZonesPath))
            using (DataTable dtMatrix_Uat = ExcelReader.Read(ACSSMatrixPath_Uat))
            using (DataTable dtZones_Uat = ExcelReader.Read(ACSSZonesPath))
            {
                int rowIdx = 0;
                foreach (DataRow dr in dtMain.Rows)
                {
                    try
                    {
                        string JSON = dr[nameof(JSON)] as string;
                        string CentiroV1 = dr[nameof(CentiroV1)] as string;
                        string CentiroV2 = dr[nameof(CentiroV2)] as string;
                        if (string.IsNullOrWhiteSpace(JSON) && string.IsNullOrWhiteSpace(CentiroV1) && string.IsNullOrWhiteSpace(CentiroV2))
                            break;
                        DiscrepancyLogEntry dle = JsonConvert.DeserializeObject<DiscrepancyLogEntry>(JSON);
                        ACSSCandidatesFilterer filtererProd = new ACSSCandidatesFilterer(dtMatrix_Prod, dtZones_Prod);
                        var filteredProd = filtererProd.Filter(dle.ParsedInput);
                        ACSSCandidatesFilterer filtererUat = new ACSSCandidatesFilterer(dtMatrix_Uat, dtZones_Uat);
                        var filteredUat = filtererUat.Filter(dle.ParsedInput);
                        var currApiCorrId = Guid.NewGuid().ToString(); //$"PCT-9944-Revalidate-{Guid.NewGuid().ToString()}";
                        var corrIdExreHdrs = new Dictionary<string, string>() { { "X-CorrelationId", currApiCorrId } };
                        AvailableCarriersRequestV2 centiroV2Req = (AvailableCarriersRequestV2)dle.ParsedInput;
                        string centiroV2ReqJson = JsonConvert.SerializeObject(centiroV2Req, JsonFormatting.None);
                        DateTime ts = DateTime.Now;
                        var centiroResponseProd = ApiUtility.Post(apiEndpointProd, apiAuthTokenProd, centiroV2ReqJson, corrIdExreHdrs).ConfigureAwait(false).GetAwaiter().GetResult();///corrIdExreHdrs_Prod
                        List<AvailableCarriersResponse> avCarrsProd = null;
                        if (centiroResponseProd.Item1 == HttpStatusCode.OK)
                        {
                            avCarrsProd = JsonConvert.DeserializeObject<List<AvailableCarriersResponse>>(centiroResponseProd.Item2);
                        }
                        var centiroResponseUat = ApiUtility.Post(apiEndpointUat, apiAuthTokenUat, centiroV2ReqJson, null).ConfigureAwait(false).GetAwaiter().GetResult();///corrIdExreHdrs_Prod
                        List<AvailableCarriersResponse> avCarrsUat = null;
                        if (centiroResponseUat.Item1 == HttpStatusCode.OK)
                        {
                            avCarrsUat = JsonConvert.DeserializeObject<List<AvailableCarriersResponse>>(centiroResponseUat.Item2);
                        }
                        var matrixTopCandidate_Prod = filteredProd.Candidates?.Where(c => c.Verdict == ACSSFilteringVerdict.Served).FirstOrDefault();
                        var matrixTopCandidate_Uat = filteredUat.Candidates?.Where(c => c.Verdict == ACSSFilteringVerdict.Served).FirstOrDefault();
                        Console.WriteLine($"{ts:s}\t{CentiroV1}\t{CentiroV2}\t{matrixTopCandidate_Prod?.MatrixRow?.PK()}\t{matrixTopCandidate_Uat?.MatrixRow?.PK()}\t{currApiCorrId}\t{avCarrsProd?.FirstOrDefault()?.PK()}\t{avCarrsUat?.FirstOrDefault()?.PK()}\t{centiroV2ReqJson}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing row # {rowIdx}\t{ex.Message}\t{ex.StackTrace?.Replace("\n", " ")?.Replace("\r", "")}");
                    }
                    rowIdx++;
                }
            }

            return 0;
        }

        public static int PCT9944BulkRevalidate2(string[] args)
        {
            string srcXlsPath = args[0];
            string srcXlsSheet = args[1];
            string apiEndpointProd = args[2];
            string apiAuthTokenProd = args[3];
            string apiEndpointUat = args[4];
            string apiAuthTokenUat = args[5];
            string inputParamsJsonPath = args[6];
            string debugFilterersArg = args.Length > 7 ? args[7] : string.Empty;

            bool debugFilterers;
            if (!bool.TryParse(debugFilterersArg, out debugFilterers))
                debugFilterers = false;
            //Console.WriteLine($"ts\tCentiroV1\tCentiroV2\tmatrixTopCandidate_Prod\tmatrixTopCandidate_Uat\tX-CorrelationId_Prod\tApiRespCandidateProd\tApiResponseCandidateUAT\tCentiroV2Req");
            Console.WriteLine($"ts\tCentiroV1\tCentiroV2\tCentiroV2Count\tmatrixCandidates_Prod\tmatrixCandidates_Prod_Count\tmatrixCandidates_Uat\tmatrixCandidates_Uat_Count\ttopMatrixCandidate_Prod\ttopMatrixCandidate_Uat\tX-CorrelationId_Prod\tX-CorrelationId_Uat\tApiRespCandidateProd\tApiResponseCandidateUAT\tShipping-Carrier-Api_request\tCentiroV2Req");
            Dictionary<string, Tuple<DataTable, DataTable>> matrixZonesByPlant_Prod = new();
            Dictionary<string, Tuple<DataTable, DataTable>> matrixZonesByPlant_Uat = new();
            var inputParams = JsonConvert.DeserializeObject<NoOrDiscrepancyLogsAnalyzeRequest>(System.IO.File.ReadAllText(inputParamsJsonPath));
            var prodMatrixZones = inputParams.MatrixZonesByPlantPerEnv["prod"];
            var uatMatrixZones = inputParams.MatrixZonesByPlantPerEnv["uat"];
            foreach (string plantKey in prodMatrixZones.MatrixZonesPerPlant?.Keys)
            {
                matrixZonesByPlant_Prod.Add(plantKey, new Tuple<DataTable, DataTable>(ExcelReader.Read(prodMatrixZones.MatrixZonesPerPlant[plantKey].MatrixXlsPath), ExcelReader.Read(prodMatrixZones.MatrixZonesPerPlant[plantKey].ZonesXlsPath)));
            }
            if (uatMatrixZones.MatrixZonesPerPlant != null && true == uatMatrixZones.MatrixZonesPerPlant.Any())
            {
                foreach (string plantKey in uatMatrixZones.MatrixZonesPerPlant?.Keys)
                {
                    matrixZonesByPlant_Uat.Add(plantKey, new Tuple<DataTable, DataTable>(ExcelReader.Read(uatMatrixZones.MatrixZonesPerPlant[plantKey].MatrixXlsPath), ExcelReader.Read(uatMatrixZones.MatrixZonesPerPlant[plantKey].ZonesXlsPath)));
                }
            }

            using (DataTable dtMain = ExcelReader.Read(srcXlsPath, srcXlsSheet))
            {
                int rowIdx = 0;
                foreach (DataRow dr in dtMain.Rows)
                {
                    try
                    {
                        string JSON = dr[nameof(JSON)] as string;
                        string CentiroV1 = dr[nameof(CentiroV1)] as string;
                        string CentiroV2 = dr[nameof(CentiroV2)] as string;
                        string PlantCode = dr[nameof(PlantCode)] as string;
                        if (string.IsNullOrWhiteSpace(JSON) && string.IsNullOrWhiteSpace(CentiroV1) && string.IsNullOrWhiteSpace(CentiroV2))
                            break;
                        DiscrepancyLogEntry dle = JsonConvert.DeserializeObject<DiscrepancyLogEntry>(JSON);
                        ACSSCandidatesFilterer filtererProd = new ACSSCandidatesFilterer(matrixZonesByPlant_Prod[PlantCode].Item1, matrixZonesByPlant_Prod[PlantCode].Item2) { Debug = debugFilterers };
                        var filteredProd = filtererProd.Filter(dle?.ParsedInput);

                        ACSSCandidatesFilteringResponse filteredUat = null;
                        if (true == matrixZonesByPlant_Uat?.Any() && matrixZonesByPlant_Uat.ContainsKey(PlantCode))
                        {
                            ACSSCandidatesFilterer filtererUat = new ACSSCandidatesFilterer(matrixZonesByPlant_Uat[PlantCode].Item1, matrixZonesByPlant_Uat[PlantCode].Item2) { Debug = debugFilterers };
                            filteredUat = filtererUat.Filter(dle?.ParsedInput);
                        }
                        var currApiCorrId_Prod = string.Empty;
                        var currApiCorrId_Uat = string.Empty;
                        List<AvailableCarriersResponse> avCarrsProd = null;
                        List<AvailableCarriersResponse> avCarrsUat = null;
                        AvailableCarriersRequestV2 centiroV2ReqOurs = (AvailableCarriersRequestV2)dle.ParsedInput;
                        string centiroV2ReqJson = JsonConvert.SerializeObject(centiroV2ReqOurs, JsonFormatting.None);
                        DateTime ts = DateTime.Now;
                        GetOptionsRequest trueCentiroV2Req = new GetOptionsRequest
                        {
                            MessageId = Guid.NewGuid(),
                            Deliveries = new List<DeliveryRequest> { dle.ParsedInput.ToDeliveryRequest().ToCentiroModel() }
                        };
                        if (!string.IsNullOrWhiteSpace(apiEndpointProd) && !string.IsNullOrWhiteSpace(apiAuthTokenProd))
                        {
                            currApiCorrId_Prod = Guid.NewGuid().ToString();
                            var corrIdExreHdrs_Prod = new Dictionary<string, string>() { { "X-CorrelationId", currApiCorrId_Prod } };
                            var centiroResponseProd = ApiUtility.Post(apiEndpointProd, apiAuthTokenProd, centiroV2ReqJson, corrIdExreHdrs_Prod).ConfigureAwait(false).GetAwaiter().GetResult();
                            if (centiroResponseProd.Item1 == HttpStatusCode.OK)
                            {
                                avCarrsProd = JsonConvert.DeserializeObject<List<AvailableCarriersResponse>>(centiroResponseProd.Item2);
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(apiEndpointUat) && !string.IsNullOrWhiteSpace(apiAuthTokenUat))
                        {
                            currApiCorrId_Uat = Guid.NewGuid().ToString();
                            var corrIdExreHdrs_Uat = new Dictionary<string, string>() { { "X-CorrelationId", currApiCorrId_Uat } };
                            var centiroResponseUat = ApiUtility.Post(apiEndpointUat, apiAuthTokenUat, centiroV2ReqJson, corrIdExreHdrs_Uat).ConfigureAwait(false).GetAwaiter().GetResult();

                            if (centiroResponseUat.Item1 == HttpStatusCode.OK)
                            {
                                avCarrsUat = JsonConvert.DeserializeObject<List<AvailableCarriersResponse>>(centiroResponseUat.Item2);
                            }
                        }
                        var matrixTopCandidate_Prod = filteredProd?.Candidates?.Where(c => c.Verdict == ACSSFilteringVerdict.Served)?.FirstOrDefault();
                        var matrixCandidates_Prod = filteredProd?.Candidates?.Where(c => c.Verdict == ACSSFilteringVerdict.Served)?.ToList()?.Select(c => c.MatrixRow.PK())?.ToArray();
                        var matrixTopCandidate_Uat = filteredUat?.Candidates?.Where(c => c.Verdict == ACSSFilteringVerdict.Served)?.FirstOrDefault();
                        var matrixCandidates_Uat = filteredUat?.Candidates?.Where(c => c.Verdict == ACSSFilteringVerdict.Served)?.ToList()?.Select(c => c.MatrixRow.PK())?.ToArray();

                        //Console.WriteLine($"{ts:s}\t{CentiroV1}\t{CentiroV2}\t{matrixTopCandidate_Prod?.MatrixRow?.PK()}\t{matrixTopCandidate_Uat?.MatrixRow?.PK()}\t{currApiCorrId_Prod}\t{avCarrsProd?.FirstOrDefault()?.PK()}\t{avCarrsUat?.FirstOrDefault()?.PK()}\t{centiroV2ReqJson}");
                        var trueCentiroV2ReqJson = JsonConvert.SerializeObject(trueCentiroV2Req, JsonFormatting.None);
                        Console.WriteLine($"{ts:s}\t{CentiroV1}\t{CentiroV2}\t{CentiroV2.Split(',').Length}\t{IEnumerableToString(matrixCandidates_Prod)}\t{matrixCandidates_Prod?.Count()}\t{IEnumerableToString(matrixCandidates_Uat)}\t{matrixCandidates_Uat?.Count()}\t{matrixCandidates_Prod?.FirstOrDefault()}\t{matrixCandidates_Uat?.FirstOrDefault()}\t{currApiCorrId_Prod}\t{currApiCorrId_Uat}\t{avCarrsProd?.FirstOrDefault()?.PK()}\t{avCarrsUat?.FirstOrDefault()?.PK()}\t{centiroV2ReqJson}\t{trueCentiroV2ReqJson}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing row # {rowIdx}\t{ex.Message}\t{ex.StackTrace?.Replace("\n", " ")?.Replace("\r", "")}");
                    }
                    rowIdx++;
                }
            }

            return 0;
        }

        private static string IEnumerableToString(IEnumerable<string> list)
        {
            return true != list?.Any() ? string.Empty : string.Join(", ", list);
        }
        public static int PCT9944BulkRevalidateChecker(string[] args)
        {
            string bulkReValidateOutputPath = args[0];
            //string 
            return 0;
        }

        public static int RequestJsonSerializationSample(string[] args)
        {
            string[] envs = { "prod", "uat" };
            NoOrDiscrepancyLogsAnalyzeRequest req = new NoOrDiscrepancyLogsAnalyzeRequest();
            req.MatrixZonesByPlantPerEnv = new();
            foreach (var env in envs)
            {
                req.MatrixZonesByPlantPerEnv.Add(env, new MatrixZonesPerEnvConfig());
                req.MatrixZonesByPlantPerEnv[env].MatrixZonesPerPlant = new();
                req.MatrixZonesByPlantPerEnv[env].MatrixZonesPerPlant.Add("YPB", new MatrixZoneConfig()
                {
                    MatrixXlsPath = $"matrix_nl_{env}.xls",
                    ZonesXlsPath = $"zones_nl_{env}.xls"
                }
                );
                req.MatrixZonesByPlantPerEnv[env].MatrixZonesPerPlant.Add("WFR", new MatrixZoneConfig()
                {
                    MatrixXlsPath = $"matrix_wfr_{env}.xls",
                    ZonesXlsPath = $"zones_wfr_{env}.xls"
                }
                );
            }

            Console.WriteLine(JsonConvert.SerializeObject(req, JsonFormatting.Indented));
            Console.Read();
            return 0;
        }

        public static int IsZipBetween(string[] args)
        {
            Console.WriteLine("97100-97699, 97101" + ACSSCandidatesFilterer.IsZipBetween("97101", "97100", "97699"));
            Console.WriteLine("98400-98499, 98500" + ACSSCandidatesFilterer.IsZipBetween("98500", "98400", "98499"));
            Console.WriteLine("98600-98899, 97000" + ACSSCandidatesFilterer.IsZipBetween("97000", "98600", "98899"));
            Console.WriteLine("00000-97099, 00001" + ACSSCandidatesFilterer.IsZipBetween("00001", "00000", "97099"));
            Console.WriteLine("00000-97099, 99999" + ACSSCandidatesFilterer.IsZipBetween("99999", "00000", "97099"));
            return 0;
        }

        public static int PCT9944CompareDiscrepanciesSets(string[] args)
        {
            string discrLogPath1 = args[0];
            string discrLogPath2 = args[1];
            var dles1 = PCT9944DiscrepanciesLogReader.Read(discrLogPath1);
            var dles2 = PCT9944DiscrepanciesLogReader.Read(discrLogPath2);
            var inputsIntersection = dles1.Where(d => !string.IsNullOrWhiteSpace(d.Input)).Select(d => d.Input).Intersect(dles2.Where(d => !string.IsNullOrWhiteSpace(d.Input)).Select(d => d.Input)).ToList();
            var diffsIntersection = dles1.Where(d => !string.IsNullOrWhiteSpace(d.DiffStr)).Select(d => d.DiffStr).Intersect(dles2.Where(d => !string.IsNullOrWhiteSpace(d.DiffStr)).Select(d => d.DiffStr)).ToList();
            var newInputs = dles2.Where(d => !string.IsNullOrWhiteSpace(d.Input)).Select(d => d.Input).Except(dles1.Where(d => !string.IsNullOrWhiteSpace(d.Input)).Select(d => d.Input)).ToList();
            var newDiffs = dles2.Where(d => !string.IsNullOrWhiteSpace(d.DiffStr)).Select(d => d.DiffStr).Except(dles1.Where(d => !string.IsNullOrWhiteSpace(d.DiffStr)).Select(d => d.DiffStr)).ToList();
            if (inputsIntersection.Any())
            {
                Console.WriteLine(nameof(inputsIntersection));
                inputsIntersection.ForEach(i => Console.WriteLine($"  {i}"));
            }
            if (diffsIntersection.Any())
            {
                Console.WriteLine(nameof(diffsIntersection));
                diffsIntersection.ForEach(i => Console.WriteLine($"  {i}"));
            }
            if (newInputs.Any())
            {
                Console.WriteLine(nameof(newInputs));
                newInputs.ForEach(i => Console.WriteLine($"  {i}"));
            }
            if (newDiffs.Any())
            {
                Console.WriteLine(nameof(newDiffs));
                newDiffs.ForEach(i => Console.WriteLine($"  {i}"));
            }

            return 0;
        }

        public static int PCT10679ExportLeadtimes(string[] args)
        {
            string envName = args[0];
            string authToken = args[1];
            string saveAsPath = args[2];


            string host = string.Empty;
            switch (envName.Trim().ToLower())
            {
                case "prod":
                case "production": host = "leadtime-api.production-scheduling.infra.photos"; break;
                case "test":
                case "sandbox": host = "leadtime-api.tst.production-scheduling.infra.photos"; break;
                case "acc":
                case "acceptance":
                case "non-prod": host = "leadtime-api.acc.production-scheduling.infra.photos"; break;
                default: throw new ArgumentException(nameof(envName));
            }
            string url = $"https://{host}/v2/leadtime";
            var apiRes = ApiUtility.Get(url, authToken);
            if (apiRes.Item1 != HttpStatusCode.OK)
            {
                Console.WriteLine($"{nameof(apiRes)}:{{ {apiRes.Item1}, {apiRes.Item2} }}");
                return 1;
            }
            var leadTimes = JsonConvert.DeserializeObject<GetAllLeadTimesResponse>(apiRes.Item2);

            StringBuilder sbTarget = new StringBuilder();
            //Tools.DataTableToCsv(leadTimes?.ToDataTable(), sbTarget);
            Tools.ListToCsv<FlattenedLeadTime>(leadTimes?.Flatten(), sbTarget);
            if (!string.IsNullOrWhiteSpace(saveAsPath))
                System.IO.File.WriteAllText(saveAsPath, sbTarget.ToString());
            else
                Console.WriteLine(sbTarget.ToString());
            return 0;
        }

        public static int ParseESLogJSContentTest(string[] args)
        {
            string srcCsv = args[0];
            var entries = ESLogsJSContentParser.ReadOut(srcCsv);
            Console.WriteLine(JsonConvert.SerializeObject(entries, JsonFormatting.Indented));
            return 0;
        }

        public static int ReplayJSContent(string[] args)
        {
            string srcLogCsv = args[0];
            string apiUrl = args[1];
            string authToken = args[2];
            string logErrorsOnlyStr = args.Length > 3 ? args[3] : string.Empty;
            bool logErrorsOnly;
            if (!bool.TryParse(logErrorsOnlyStr, out logErrorsOnly))
                logErrorsOnly = true;

            string errorDelim = new string('-', 33);
            int completedCount = 0;
            int errCount = 0;
            var entries = ESLogsJSContentParser.ReadOut(srcLogCsv);
            foreach (var entry in entries)
            {
                if (entry.Message.IndexOf("POST ") != 0)
                    continue;
                try
                {
                    var apiResp = ApiUtility.Post(apiUrl, authToken, entry.JSContent, null).ConfigureAwait(false).GetAwaiter().GetResult();
                    if (apiResp.Item1 != HttpStatusCode.OK)
                        Console.Error.WriteLine($"Error:{apiResp.Item1}|{apiResp.Item2}:\n{entry.JSContent}\n{errorDelim}");
                    else
                    {
                        if (!logErrorsOnly)
                            Console.Error.WriteLine($"Success:{apiResp.Item1}|{apiResp.Item2}:\n{entry.JSContent}\n{errorDelim}");
                    }
                    completedCount++;
                }
                catch (Exception ex)
                {
                    errCount++;
                    Console.Error.WriteLine($"Exception:{entry.XCorrelationId}:{ex}\n===\n{entry.JSContent}\n{errorDelim}");
                }
            }
            Console.WriteLine($"{nameof(completedCount)}:{completedCount}\n{nameof(errCount)}:{errCount}");
            return 0;
        }

        public static int PCT9247ReplayCompareV1vsV2(string[] args)
        {
            string srcLogCsv1 = args[0];
            string srcLogCsv2 = args[1];
            string apiUrlV1 = args[2];
            string authTokenV1 = args[3];
            string apiUrlV2 = args[4];
            string authTokenV2 = args[5];
            string outputPath = args[6];

            string errorDelim = new string('-', 33);
            int completedCount = 0;
            int errCount = 0;
            var entriesV1 = !string.IsNullOrWhiteSpace(srcLogCsv1) && System.IO.File.Exists(srcLogCsv1) ? ESLogsJSContentParser.ReadOut(srcLogCsv1) : new List<ESLogEntryEx>();
            var entriesV2 = !string.IsNullOrWhiteSpace(srcLogCsv2) && System.IO.File.Exists(srcLogCsv2) ? ESLogsJSContentParser.ReadOut(srcLogCsv2) : new List<ESLogEntryEx>();

            var allEntries = new List<Tuple<CalculateCarrierModel, string, string>>();
            entriesV1.ForEach(e =>
            {
                if (e.Message.IndexOf("POST ") == 0)
                {
                    try
                    {
                        AvailableCarriersRequest req = JsonConvert.DeserializeObject<AvailableCarriersRequest>(e.JSContent);
                        allEntries.Add(new Tuple<CalculateCarrierModel, string, string>((CalculateCarrierModel)req, e.JSContent, string.Empty));
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error deserializing or converting an entry(v1):{ex},\n{e.JSContent}");
                    }
                }
            });
            var defaultv1ArticleTypes = new List<string> { "PhotoBook PhotoCover" };
            entriesV2.ForEach(e =>
            {
                if (e.Message.IndexOf("POST ") == 0)
                {
                    try
                    {
                        AvailableCarriersRequestV2 req = JsonConvert.DeserializeObject<AvailableCarriersRequestV2>(e.JSContent);
                        var ccm = (CalculateCarrierModel)req;
                        if (true != ccm.ArticleTypes?.Any())
                            ccm.ArticleTypes = defaultv1ArticleTypes;
                        allEntries.Add(new Tuple<CalculateCarrierModel, string, string>(ccm, string.Empty, e.JSContent));
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error deserializing or converting an entry(v2):{ex},\n{e.JSContent}");
                    }
                }
            });

            ProgressTracker progressTracker = new ProgressTracker();
            progressTracker.Start(allEntries.Count, System.Console.Out, DateTime.Now);
            List<PCT9247ReplayCompareV1vsV2Entry> rslt = new();
            foreach (var entry in allEntries)
            {
                var dr = new PCT9247ReplayCompareV1vsV2Entry();
                var XCorrelationId = Guid.NewGuid().ToString();
                dr.CalculateCarrierModel = JsonConvert.SerializeObject(entry.Item1, JsonFormatting.Indented);
                dr.XCorrelationId = XCorrelationId;
                dr.CalculateCarrierModelShort = entry.Item1.ToString();
                try
                {
                    var v1ReqJson = !string.IsNullOrWhiteSpace(entry.Item2) ? entry.Item2 : JsonConvert.SerializeObject((AvailableCarriersRequest)entry.Item1);
                    dr.v1ReqJson = v1ReqJson;
                    var v2ReqJson = !string.IsNullOrWhiteSpace(entry.Item3) ? entry.Item3 : JsonConvert.SerializeObject((AvailableCarriersRequestV2)entry.Item1);
                    dr.v2ReqJson = v2ReqJson;
                    IEnumerable<AvailableCarriersResponse> availablesV1 = null;
                    IEnumerable<AvailableCarriersResponse> availablesV2 = null;
                    var corrIdHdrs = new Dictionary<string, string>() { { "X-CorrelationId", XCorrelationId } };
                    try
                    {
                        var v1Resp = ApiUtility.Post(apiUrlV1, authTokenV1, v1ReqJson, corrIdHdrs).ConfigureAwait(false).GetAwaiter().GetResult();
                        dr.v1Status = v1Resp.Item1.ToString();
                        dr.v1Resp = v1Resp.Item2;
                        if (v1Resp.Item1 == HttpStatusCode.OK)
                        {
                            availablesV1 = JsonConvert.DeserializeObject<IEnumerable<AvailableCarriersResponse>>(v1Resp.Item2);
                        }
                        dr.v1Count = availablesV1?.Count();
                    }
                    catch (Exception e)
                    {
                        dr.v1Err = e.ToString();
                    }

                    try
                    {
                        var v2Resp = ApiUtility.Post(apiUrlV2, authTokenV2, v2ReqJson, corrIdHdrs).ConfigureAwait(false).GetAwaiter().GetResult();
                        dr.v2Status = v2Resp.Item1.ToString();
                        dr.v2Resp = v2Resp.Item2;
                        if (v2Resp.Item1 == HttpStatusCode.OK)
                        {
                            availablesV2 = JsonConvert.DeserializeObject<IEnumerable<AvailableCarriersResponse>>(v2Resp.Item2);
                        }
                        dr.v2Count = availablesV2?.Count();
                    }
                    catch (Exception e)
                    {
                        dr.v2Err = e.ToString();
                    }
                    string v1PKs = string.Join(",", availablesV1?.Select(a => a.PK()).ToArray());
                    string v2PKs = string.Join(",", availablesV2?.Select(a => a.PK()).ToArray());
                    dr.v1PKs = v1PKs;
                    dr.v2PKs = v2PKs;
                    string Diff = string.Empty;
                    if (availablesV1 == null && availablesV2 == null)
                        Diff = 0.ToString();
                    else if (availablesV1 == null || availablesV2 == null)
                        Diff = "null vs non-null";
                    else if (!availablesV1.Any() && !availablesV2.Any())
                        Diff = 0.ToString();
                    else if (availablesV1?.Count() != availablesV2?.Count())
                        Diff = $"Diff counts({availablesV1?.Count()} vs {availablesV2?.Count()})";
                    else if (v1PKs != v2PKs)
                    {
                        if (availablesV1.Select(a => a.PK()).ToList().Intersect(availablesV2.Select(a => a.PK()).ToList()).Count() == availablesV1.Count())
                            Diff = "Order";
                        else
                            Diff = "Different";
                    }
                    else if (availablesV1?.Count() == availablesV2?.Count() && dr.v1PKs == dr.v2PKs)
                        Diff = 0.ToString();
                    dr.Diff = Diff;
                    completedCount++;
                }
                catch (Exception ex)
                {
                    errCount++;
                    dr.Error = ex.ToString();
                }
                finally
                {
                    progressTracker.ItemComplete();
                }
                rslt.Add(dr);
            }
            Tools.ListToCsv(rslt, outputPath);

            var diffs = rslt.Where(e => e.Diff != 0.ToString()).GroupBy(e => $"{e.v1PKs} vs {e.v2PKs}").Select(grp => new { Diff = grp.Key, Count = grp.Count() }).ToList();
            Console.WriteLine($"{nameof(completedCount)}:{completedCount}\n{nameof(errCount)}:{errCount}");
            diffs.ForEach(d => Console.WriteLine($"{d.Diff}\t{d.Count}"));
            return 0;
        }

        public static int PCT9247Extract500s(string[] args)
        {
            string dir = args[0];
            string filemask = args[1];
            var files = Directory.GetFiles(dir, filemask);
            List<ESLogEntryEx> allEntries = new();
            foreach (var filPath in files)
            {
                allEntries.AddRange(ESLogsJSContentParser.ReadOut(filPath));
            }

            var grouped = allEntries.GroupBy(e => e.XCorrelationId).Select(grp => new { XCorrId = grp.Key, Count = grp.Count() }).Where(s => s.Count > 1);
            var oks = allEntries.GroupBy(e => e.XCorrelationId).Select(grp => new { XCorrId = grp.Key, Count = grp.Count() }).Where(s => s.Count == 1);
            Console.WriteLine($"OKs:{oks.Count()}, 500s: {grouped.Count()}");
            foreach (var entry in grouped)
            {
                Console.WriteLine(entry.XCorrId);
            }
            return 0;
        }
        public static int PCT9247Replay500s(string[] args)
        {
            string dir = args[0];
            string filemask = args[1];
            string apiUrl = args[2];
            string authToken = args[3];
            string http500OnlyStr = args.Length > 4 ? args[4] : string.Empty;

            bool http500Only;
            if (!bool.TryParse(http500OnlyStr, out http500Only))
                http500Only = false;

            var files = Directory.GetFiles(dir, filemask);
            List<ESLogEntryEx> allEntries = new();
            foreach (var filPath in files)
            {
                allEntries.AddRange(ESLogsJSContentParser.ReadOut(filPath));
            }
            Dictionary<string, int> plantStats = new();

            var grouped = allEntries.GroupBy(e => e.XCorrelationId).Select(grp => new { XCorrId = grp.Key, Count = grp.Count() }).Where(s => !http500Only || (http500Only && s.Count > 1));
            Console.WriteLine($"{nameof(ESLogEntryEx.XCorrelationId)}-ProdOriginal\t{nameof(ESLogEntryEx.XCorrelationId)}-Re-Check\t{nameof(HttpStatusCode)}\tError");

            foreach (var grp in grouped)
            {
                string jsContent = allEntries.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.XCorrelationId) && e.XCorrelationId == grp.XCorrId)?.JSContent;
                if (string.IsNullOrWhiteSpace(jsContent))
                    continue;
                #region collection plant stats
                try
                {
                    var pl = JsonConvert.DeserializeObject<AvailableCarriersRequestV2>(jsContent);
                    if (pl != null && !string.IsNullOrWhiteSpace(pl?.PlantCode))
                    {
                        if (!plantStats.ContainsKey(pl.PlantCode))
                            plantStats.Add(pl.PlantCode, 0);
                        plantStats[pl.PlantCode]++;
                    }
                }
                catch
                {
                    Console.WriteLine($"Error collecting plant stats for pl:{jsContent}\r\n{new string('-', 33)}");
                }
                #endregion
                var curNewReqXCorr = Guid.NewGuid().ToString();
                try
                {
                    var corrIdHdrs = new Dictionary<string, string>() { { "X-CorrelationId", curNewReqXCorr } };
                    var v2Resp = ApiUtility.Post(apiUrl, authToken, jsContent, corrIdHdrs).ConfigureAwait(false).GetAwaiter().GetResult();
                    var err = v2Resp.Item1 == HttpStatusCode.OK ? string.Empty : v2Resp.Item2;
                    Console.WriteLine($"{grp.XCorrId}\t{curNewReqXCorr}\t{(int)v2Resp.Item1}\t{err}");
                    if (v2Resp.Item1 == HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine("Token expired, sorry");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{grp.XCorrId}\t{curNewReqXCorr}\t{0}\t{e.Message}");
                }
            }
            Console.WriteLine(new string('=', 33));
            Console.WriteLine(JsonConvert.SerializeObject(plantStats, JsonFormatting.Indented));
            return 0;
        }

        public static int PCT9247Replay500s2(string[] args)
        {
            string dir = args[0];
            string filemask = args[1];
            string apiUrl = args[2];
            string authToken = args[3];
            string http500OnlyStr = args.Length > 4 ? args[4] : string.Empty;
            string plThreadsStr = args.Length > 5 ? args[5] : string.Empty;

            bool http500Only;
            if (!bool.TryParse(http500OnlyStr, out http500Only))
                http500Only = false;


            int plThreads;
            if (!int.TryParse(plThreadsStr, out plThreads))
                plThreads = 10;

            var files = Directory.GetFiles(dir, filemask);
            List<ESLogEntryEx> allEntries = new();
            foreach (var filPath in files)
            {
                allEntries.AddRange(ESLogsJSContentParser.ReadOut(filPath));
            }
            Dictionary<string, int> plantStats = new();

            var grouped = allEntries.GroupBy(e => e.XCorrelationId).Select(grp => new { XCorrId = grp.Key, Count = grp.Count() }).Where(s => !http500Only || (http500Only && s.Count > 1));
            Console.WriteLine($"{nameof(ESLogEntryEx.XCorrelationId)}-ProdOriginal\t{nameof(ESLogEntryEx.XCorrelationId)}-Re-Check\t{nameof(HttpStatusCode)}\tError");
            ConcurrentQueue<Tuple<string, string>> theQueue = new();

            foreach (var grp in grouped)
            {
                string jsContent = allEntries.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.XCorrelationId) && e.XCorrelationId == grp.XCorrId)?.JSContent;
                if (string.IsNullOrWhiteSpace(jsContent))
                    continue;
                theQueue.Enqueue(new Tuple<string, string>(grp.XCorrId, jsContent));
                #region collection plant stats
                try
                {
                    var pl = JsonConvert.DeserializeObject<AvailableCarriersRequestV2>(jsContent);
                    if (pl != null && !string.IsNullOrWhiteSpace(pl?.PlantCode))
                    {
                        if (!plantStats.ContainsKey(pl.PlantCode))
                            plantStats.Add(pl.PlantCode, 0);
                        plantStats[pl.PlantCode]++;
                    }
                }
                catch
                {
                    Console.WriteLine($"Error collecting plant stats for pl:{jsContent}\r\n{new string('-', 33)}");
                }
                #endregion
            }
            List<Task> tasks = new();
            for (int i = 0; i < plThreads; i++)
            {
                tasks.Add(Task.Run(() => PCT9247Replay500s2ThreadWorker(theQueue, apiUrl, authToken).ConfigureAwait(false).GetAwaiter().GetResult()));
            }
            Task.WhenAll(tasks).ConfigureAwait(false).GetAwaiter().GetResult();
            Console.WriteLine(new string('=', 33));
            Console.WriteLine(JsonConvert.SerializeObject(plantStats, JsonFormatting.Indented));
            return 0;
        }

        private static async Task PCT9247Replay500s2ThreadWorker(ConcurrentQueue<Tuple<string, string>> queue, string apiUrl, string authToken)
        {
            do
            {
                Tuple<string, string> curr;
                if (!queue.TryDequeue(out curr))
                    break;
                var curNewReqXCorr = Guid.NewGuid().ToString();
                try
                {
                    var corrIdHdrs = new Dictionary<string, string>() { { "X-CorrelationId", curNewReqXCorr } };
                    var v2Resp = await ApiUtility.Post(apiUrl, authToken, curr.Item2, corrIdHdrs);
                    var err = v2Resp.Item1 == HttpStatusCode.OK ? string.Empty : v2Resp.Item2;
                    Console.WriteLine($"{curr.Item1}\t{curNewReqXCorr}\t{(int)v2Resp.Item1}\t{err}");
                    if (v2Resp.Item1 == HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine("Token expired, sorry");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{curr.Item1}\t{curNewReqXCorr}\t{0}\t{e.Message}");
                }
            } while (true);
        }

        #endregion


        public static int PCT9247PrepareYPBVPLeadtimes(string[] args)
        {
            string jsonPath = args[0];
            Console.WriteLine("plantCode\tcarrierName\tdeliveryType\tinEu\tcountryId\tshippingLeadTime\tprovider\tlegacyCarrierName");

            dynamic attrs = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(jsonPath));
            dynamic carriers = attrs.carriers;
            for (int i = 0; i < carriers.Count; i++)
            {
                dynamic currCarrier = carriers[i];
                if (currCarrier.plantCode != "YPB" || currCarrier.locations.Count == 0 || currCarrier.dealerIds[0] != 2000)
                    continue;
                for (int j = 0; j < currCarrier.locations.Count; j++)
                {
                    dynamic loc = currCarrier.locations[j];
                    if (loc.providerKey != "qj7du2w9")
                        continue;
                    Console.WriteLine($"{currCarrier.plantCode}\t{currCarrier.carrierName}\t{currCarrier.deliveryType}\t{loc.inEu}\t{loc.countryId}\t{loc.shippingLeadTime}\t{loc.provider}\t{loc.legacyCarrierName}");
                }

            }

            return 0;
        }

        public static int PCT10816GetEmails(string[] args)
        {
            string salesOrderIdsListPath = args[0];
            string cboApiUrl = args[1];
            string cboApiToken = args[2];

            List<string> salesOrderIds = new List<string>(System.IO.File.ReadAllLines(salesOrderIdsListPath));
            foreach (var salId in salesOrderIds)
            {
                var salIdPure = salId?.Trim();
                if (string.IsNullOrWhiteSpace(salIdPure))
                {
                    Console.WriteLine($"{salIdPure}\t");
                    continue;
                }
                var currUrl = $"{cboApiUrl}{salIdPure}";
                var apiResp = ApiUtility.Get(currUrl, cboApiToken);
                if (apiResp?.Item1 != HttpStatusCode.OK)
                {
                    Console.WriteLine($"{salIdPure}\t");
                    Console.Error.WriteLine($"{currUrl}\t{apiResp.Item1}\t{apiResp.Item2}");
                    continue;
                }
                dynamic sal = JsonConvert.DeserializeObject(apiResp.Item2);
                dynamic cust = sal?.customer;
                string email = cust?.email;
                Console.WriteLine($"{salIdPure}\t{email}");
            }
            return 0;
        }


        public static int PCT10481ReplayCPDs(string[] args)
        {
            //sample: CMDs\PCT10481ReplayCPDs.prms.sample.json
            dynamic inputArgs = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(args[0]));
            string justPreviewStr = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : string.Empty;
            bool justPreview;
            if (!bool.TryParse(justPreviewStr, out justPreview))
                justPreview = false;
            string centralLogFpath = inputArgs.centralLogFpath;
            string logsFileMask = inputArgs.logsFileMask;
            string apiUrlv1 = inputArgs.apiUrlv1;
            string apiTokenv1 = inputArgs.apiToken;
            string apiUrlv2 = inputArgs.apiUrlv2;
            string apiTokenv2 = inputArgs.apiToken;
            string apiUrlv1Prod = inputArgs.apiUrlv1Prod;
            string apiTokenv1Prod = inputArgs.apiTokenProd;
            string apiUrlv2Prod = inputArgs.apiUrlv2Prod;
            string apiTokenv2Prod = inputArgs.apiTokenProd;
            string outputCsvPath = inputArgs.outputCsvPath;


            string logsDir = Path.GetDirectoryName(centralLogFpath);
            //string centralLogFpath = Path.Combine(logsDir, centralLog);
            List<string> xCorrIds = new();

            var centralEntries = ESLogsJSContentParser.ReadOut(centralLogFpath);
            centralEntries.ForEach(e =>
            {
                if (!xCorrIds.Contains(e.XCorrelationId)) xCorrIds.Add(e.XCorrelationId);
            });
            var payloadsLogs = Directory.GetFiles(logsDir, logsFileMask);
            var allEntries = new List<ESLogEntryEx>();
            var estShipDtLc = "EstimatedShippingDate".ToLower();
            var estShipDtNullLc = "\"EstimatedDeliveryDate\":null".ToLower();
            long totalBruttoCount = 0;
            foreach (var currLogPath in payloadsLogs)
            {
                if (currLogPath.Trim().ToLower() == centralLogFpath.Trim().ToLower())
                    continue;
                try
                {
                    var tmp = ESLogsJSContentParser.ReadOut(currLogPath);
                    if (tmp != null)
                        totalBruttoCount += tmp.Count;
                    allEntries.AddRange(tmp.Where(e => xCorrIds.Contains(e.XCorrelationId) && e.Message.IndexOf("POST ") == 0 && (e.JSContent.ToLower().IndexOf(estShipDtLc) == -1 || e.JSContent.ToLower().IndexOf(estShipDtNullLc) != -1)));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error reading out '{currLogPath}':{ex}");
                }
            }

            Console.WriteLine($"{nameof(allEntries)}.Count = {allEntries.Count} (of {totalBruttoCount} brutto in total)");
            var plantCodes = allEntries.Select(e =>
            {
                dynamic dto = JsonConvert.DeserializeObject(e.JSContent);
                string plantCode = dto.PlantCode;
                if (string.IsNullOrWhiteSpace(plantCode)) plantCode = dto.plantCode;
                return plantCode;
            }).GroupBy(e => e).Select(grp => new { PlantCode = grp.Key, Count = grp.Count() }).ToList();
            Console.WriteLine(JsonConvert.SerializeObject(plantCodes, JsonFormatting.Indented));
            var trtModesCentral = centralEntries.Select(e => new { PlantCode = PCT10481MiscParser.CPDErrMsg_ParsePlant(e.Message), ModeOfTransport = PCT10481MiscParser.CPDErrMsg_ParseModeOfTransport(e.Message) }).GroupBy(e => e.ModeOfTransport).Select(grp => new { ModeOfTransport = grp.Key, Cnt = grp.Count() }).ToList();
            Console.WriteLine(JsonConvert.SerializeObject(trtModesCentral, JsonFormatting.Indented));
            if (justPreview)
            {
                return 0;
            }
            List<PCT10481ReplayResultRecord> rslt = new();
            foreach (var entry in allEntries)
            {
                //todo:
                /*
                 * 1. reverse only
                 * 2. plant code, carrierservice name
                 * 3. Dates: requests' delivery & shipping date, reponse - delivery & shipping date
                 * 4. Play vs both ACC and PROD and compare results.
                 * 5. v1 or v2
                 */
                var curNewReqXCorr = Guid.NewGuid().ToString();
                var currCentralEntry = centralEntries.FirstOrDefault(e => e.XCorrelationId.Equals(entry.XCorrelationId));
                var corrIdHdrs = new Dictionary<string, string>() { { "X-CorrelationId", curNewReqXCorr } };
                bool isV1 = currCentralEntry.RequestPath.IndexOf("/v1/") == 0;
                var uatResp = ApiUtility.Post(isV1 ? apiUrlv1 : apiUrlv2, isV1 ? apiTokenv1 : apiTokenv2, entry.JSContent, corrIdHdrs).ConfigureAwait(false).GetAwaiter().GetResult();
                var prodResp = ApiUtility.Post(isV1 ? apiUrlv1Prod : apiUrlv2Prod, isV1 ? apiTokenv1Prod : apiTokenv2Prod, entry.JSContent, corrIdHdrs).ConfigureAwait(false).GetAwaiter().GetResult();
                var err = uatResp.Item1 == HttpStatusCode.OK ? string.Empty : uatResp.Item2;
                //Console.WriteLine($"{currCentralEntry.Message}\t{currCentralEntry.XCorrelationId}\t{curNewReqXCorr}\t{(int)uatResp.Item1}\t{err}");
                if (uatResp.Item1 == HttpStatusCode.Unauthorized || prodResp.Item1 == HttpStatusCode.Unauthorized)
                {
                    Console.Error.WriteLine("Token(s) expired, sorry");
                    break;
                }
                try
                {
                    PCT10481ResultsPrinter.FillInResults(rslt, entry, curNewReqXCorr, isV1, currCentralEntry, uatResp, prodResp);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"{currCentralEntry.Message}\t{currCentralEntry.XCorrelationId}\tError filling in result\t{ex}");
                }
            }
            //???!
            var trtModesActual = rslt.Select(r => r.carrierServiceKey.Split(':')[1]).GroupBy(e => e).Select(grp => new { ModeOfTransport = grp.Key, Cnt = grp.Count() }).ToList();
            Console.WriteLine(JsonConvert.SerializeObject(trtModesCentral, JsonFormatting.Indented));

            var nonCoveredTrtModes = trtModesCentral.Select(m => m.ModeOfTransport).Except(trtModesActual.Select(m => m.ModeOfTransport)).ToList();
            Console.WriteLine($"{nameof(nonCoveredTrtModes)}");
            Console.WriteLine(JsonConvert.SerializeObject(nonCoveredTrtModes, JsonFormatting.Indented));

            if (!string.IsNullOrWhiteSpace(outputCsvPath))
            {
                Tools.ListToCsv<PCT10481ReplayResultRecord>(rslt, Path.Combine(logsDir, outputCsvPath));
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                Tools.ListToCsv<PCT10481ReplayResultRecord>(rslt, sb);
                Console.WriteLine(sb.ToString());
            }
            return 0;
        }

        public static int PCT10481ReplayCPDs_SelectTopNXCorrIdsByModeOfTransport(string[] args)
        {
            string inputModes = args[0];
            string centralLogFpath = args[1];
            string topStr = args.Length > 2 ? args[2] : string.Empty;
            string exceptPath = args.Length > 3 ? args[3] : string.Empty;
            string asQueryDSLStr = args.Length > 4 ? args[4] : string.Empty;
            string exportQueryDSLsMask = args.Length > 5 ? args[5] : string.Empty;
            int topN;
            if (string.IsNullOrWhiteSpace(topStr) || !int.TryParse(topStr, out topN))
                topN = 10;

            bool asQueryDSL;
            if (string.IsNullOrWhiteSpace(asQueryDSLStr) || !bool.TryParse(asQueryDSLStr, out asQueryDSL))
                asQueryDSL = false;

            List<string> modesOfInterest = new(inputModes.Split(','));
            var centralEntries = ESLogsJSContentParser.ReadOut(centralLogFpath);
            Dictionary<string, List<string>> modeTrtsXCorrIds = new();
            List<string> exceptXCorrIds = new();
            if (!string.IsNullOrWhiteSpace(exceptPath) && System.IO.File.Exists(exceptPath))
                exceptXCorrIds.AddRange(System.IO.File.ReadLines(exceptPath));
            centralEntries.ForEach(e =>
            {
                var modeTrt = PCT10481MiscParser.CPDErrMsg_ParseModeOfTransport(e.Message);
                if (modesOfInterest.Contains(modeTrt))
                {
                    if (!modeTrtsXCorrIds.ContainsKey(modeTrt)) modeTrtsXCorrIds.Add(modeTrt, new List<string>());
                    if (!exceptXCorrIds.Contains(e.XCorrelationId))
                        modeTrtsXCorrIds[modeTrt].Add(e.XCorrelationId);
                }
            });
            List<string> dist = new();
            //if (asQueryDSL && )
            foreach (var key in modeTrtsXCorrIds.Keys)
            {
                var topX = (from x in modeTrtsXCorrIds[key] select x).Take(topN).ToList();
                dist.AddRange(topX);
            }
            if (asQueryDSL)
            {
                Console.WriteLine(KibanaTools.FormatAsOSQueryDSL("X-CorrelationId", dist.Distinct()));
            }
            else
                Console.WriteLine(string.Join(",\n", dist.Distinct()));
            return 0;
        }

        public static int XCorrIds2DSLQuery(string[] args)
        {
            string inPath = args[0];
            string fldName = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : "X-CorrelationId";
            List<string> raw = new(System.IO.File.ReadAllLines(inPath));
            Console.WriteLine(KibanaTools.FormatAsOSQueryDSL(fldName, raw.Select(r => r?.Trim()).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct()));
            return 0;
        }

        public static int FilterLogEntriesByJSContents(string[] args)
        {
            string logPath = args[0];
            string modelPath = args[1];
            string modelTypeName = args[2];
            string modelEntryTypeName = args.Length > 3 && !string.IsNullOrWhiteSpace(args[3]) ? args[3] : string.Empty;

            Type modelType = Type.GetType(modelTypeName);
            Type modelEntryType = !string.IsNullOrWhiteSpace(modelEntryTypeName) ? Type.GetType(modelEntryTypeName) : null;

            object model = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(modelPath), !string.IsNullOrWhiteSpace(modelEntryTypeName) != null ? modelEntryType : modelType);
            var logEntries = ESLogsJSContentParser.ReadOut(logPath);
            var filtered = new List<ESLogEntryEx>();
            foreach (var entry in logEntries)
            {
                if (string.IsNullOrWhiteSpace(entry.JSContent))
                    continue;
                try
                {
                    object payload = JsonConvert.DeserializeObject(entry.JSContent, modelType);
                    if (modelEntryTypeName != null)
                    {
                        System.Collections.IEnumerable ienum = payload as System.Collections.IEnumerable;
                        if (ienum != null)
                        {
                            foreach (object item in ienum)
                            {
                                if (true == item?.Equals(model))
                                {
                                    filtered.Add(entry);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (true == payload?.Equals(model))
                            filtered.Add(entry);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error processing entry:{ex}");
                    Console.Error.WriteLine($"{entry.XCorrelationId}\t{entry.timestamp_cw}\t{entry.Message}");
                    Console.Error.WriteLine(new string('-', 33));
                }
            }

            Console.WriteLine($"{filtered.Count} of {logEntries.Count}");
            StringBuilder sb = new();
            Tools.ListToCsv(filtered, sb);
            Console.WriteLine(sb.ToString());
            return 0;
        }

        public static int ParseCodeQualityMetrics(string[] args)
        {
            string xmlsPath = args[0];
            string xmlsMask = args[1];
            string outputPath = args[2];
            List<ProjectQualityMetrics> rslt = new();
            var files = Directory.GetFiles(xmlsPath, xmlsMask);
            foreach (var xmlPath in files)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                XmlNodeList projs = doc.SelectNodes("//Targets/Target");
                var sln = Path.GetFileName(xmlPath).Replace(".report.xml", "");
                foreach (XmlNode proj in projs)
                {
                    var currRslt = new ProjectQualityMetrics();
                    currRslt.Sln = sln;
                    currRslt.Proj = proj.Attributes["Name"].Value;

                    XmlNodeList metrics = proj.SelectNodes("Assembly/Metrics/Metric");
                    foreach (XmlNode metric in metrics)
                    {
                        string currMetricName = metric.Attributes["Name"].Value;
                        int metricValue = int.Parse(metric.Attributes["Value"].Value);
                        switch (currMetricName)
                        {
                            case nameof(ProjectQualityMetrics.MaintainabilityIndex): currRslt.MaintainabilityIndex = metricValue; break;
                            case nameof(ProjectQualityMetrics.CyclomaticComplexity): currRslt.CyclomaticComplexity = metricValue; break;
                            case nameof(ProjectQualityMetrics.ClassCoupling): currRslt.ClassCoupling = metricValue; break;
                            case nameof(ProjectQualityMetrics.DepthOfInheritance): currRslt.DepthOfInheritance = metricValue; break;
                            case nameof(ProjectQualityMetrics.SourceLines): currRslt.SourceLines = metricValue; break;
                            case nameof(ProjectQualityMetrics.ExecutableLines): currRslt.ExecutableLines = metricValue; break;
                        }
                    }
                    rslt.Add(currRslt);
                }
            }
            Tools.ListToCsv(rslt, outputPath);

            return 0;
        }

        public static int DLQMsgsAnalyze(string[] args)
        {
            string inJsonPath = args[0];
            string keyPropsPaths = args[1];

            string[] keyProps = keyPropsPaths.Split(',');
            JArray jarr = JArray.Parse(System.IO.File.ReadAllText(inJsonPath));
            Dictionary<string, Dictionary<string,int>> grps = new();
            for(int i = 0; i < jarr.Count; i++)
            {
                JObject entry = JObject.Parse(jarr[i].ToString());
                foreach (var keyProp in keyProps)
                {
                    //Console.WriteLine($"[{i}].{keyProp}:'{(string)entry.SelectToken(keyProp)}'");
                    if (!grps.ContainsKey(keyProp))
                        grps.Add(keyProp, new());
                    var grp = grps[keyProp];
                    string currKey = (string)entry.SelectToken(keyProp);
                    if (!grp.ContainsKey(keyProp))
                        grp.Add(currKey,0);
                    grp[currKey]++;
                }
                i++;
            }
            Console.WriteLine(JsonConvert.SerializeObject(grps, JsonFormatting.Indented));

            foreach (var keyProp in grps.Keys)
            {
                var currDupls = grps[keyProp].Where(g => g.Value > 1).Select(kvp => new Tuple<string, int>(kvp.Key, kvp.Value)).ToList();
                Console.WriteLine($"{keyProp} ({currDupls.Count}):");
                currDupls.ForEach(t => Console.WriteLine($"  {t.Item1}:{t.Item2}"));
            }
            return 0;
        }
        public static int PeekSQSMessages(string[] args)
        {
            string awsAcctId = args[0];
            string queueName = args[1];
            string nrOfMessagesStr = TryGetArg(args, 2, string.Empty);
            string visibilityTimeoutStr = TryGetArg(args, 3, string.Empty);
            string attrNames = TryGetArg(args, 4, string.Empty);
            string msgAttrNames = TryGetArg(args, 5, string.Empty);
            string entireQueueStr = TryGetArg(args, 6, string.Empty);

            int nrOfMessages;
            int visibilityTimeout;
            if (!int.TryParse(nrOfMessagesStr, out nrOfMessages)) nrOfMessages = -1;
            if (!int.TryParse(visibilityTimeoutStr, out visibilityTimeout)) visibilityTimeout = -1;
            List<string> lstAttrs = !string.IsNullOrWhiteSpace(attrNames) ? new List<string>(attrNames.Split(',')) : null;
            List<string> lstMsgAttrs = !string.IsNullOrWhiteSpace(msgAttrNames) ? new List<string>(msgAttrNames.Split(',')) : null;
            bool entireQueue;
            if (!bool.TryParse(entireQueueStr, out entireQueue)) entireQueue = false;
            var rargs = new PeekSQSMessagesRequestArgs();
            rargs.awsRegion = RegionEndpoint.EUWest1;
            rargs.queueName = queueName;
            rargs.awsAccId = awsAcctId;
            if (nrOfMessages != -1)
                rargs.nrOfMsgs = nrOfMessages;
            if (visibilityTimeout != -1)
                rargs.visibilityTimeout = visibilityTimeout;
            rargs.attrNames = lstAttrs;
            rargs.msgAttrNames = lstMsgAttrs;
            rargs.entireQueue = entireQueue;
            var func = SQSUtility.PeekSQSMessages(rargs);
            var msgs = func.ConfigureAwait(false).GetAwaiter().GetResult();
            Console.WriteLine(JsonConvert.SerializeObject(msgs, JsonFormatting.Indented));
            return 0;
        }

        public static int ApiTestGets(string[] args)
        {
            string env = args[0];
            string inputXls = args[1];
            string xlsSheet = args[2];
            string baseUrl = args[3];
            string token = args[4];

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.Error.WriteLine($"{nameof(token)} is required.");
                return -1;
            }
            Console.WriteLine("Env\tClient\tProj\tTestEndpoint\tHTTPStatus\tError");
            using (DataTable ds = ExcelReader.Read(inputXls, xlsSheet))
            {
                foreach (DataRow dr in ds.Rows)
                {
                    var client = dr["Client"] as string;
                    var proj = dr["Proj"] as string;
                    var ep = dr["TestEndpoint"] as string;
                    var currUrl = $"{baseUrl}{ep}";
                    int status = 0;
                    var currErr = string.Empty;
                    try
                    {
                        var currRes = ApiUtility.Get(currUrl, token);
                        status = (int)currRes.Item1;
                        //if (currRes.Item1 == HttpStatusCode.Unauthorized)
                        //{
                        //    Console.Error.WriteLine("Token expired, sorry...");
                        //    break;
                        //}
                    }
                    catch (Exception ex)
                    {
                        currErr = ex.Message;
                    }
                    var statusStr = string.Empty;
                    if (status > 0)
                    {
                        statusStr = $"{status}|{(HttpStatusCode)status}";
                    }
                    else
                        statusStr = $"{status}";
                    Console.WriteLine($"{env}\t{client}\t{proj}\t{ep}\t{statusStr}\t{currErr}");
                }
            }

            return 0;
        }

        public static int ReactAppEnableRunLocal(string[] args)
        {
            string packageJsonPath = args[0];
            string configJsonPath = TryGetArg(args, 1, string.Empty);
            string beApiUrl = TryGetArg(args, 2, string.Empty);

            dynamic packageJson = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(packageJsonPath));
            string oldStart = packageJson?.scripts?.start;
            if (oldStart == "react-scripts start -o")
            {
                packageJson.scripts.start = "react-scripts --openssl-legacy-provider start -o";
                System.IO.File.WriteAllText(packageJsonPath, JsonConvert.SerializeObject(packageJson, JsonFormatting.Indented));
                Console.WriteLine("Patched scripts.start");
            }

            if (!string.IsNullOrWhiteSpace(beApiUrl) && !string.IsNullOrWhiteSpace(configJsonPath))
            {
                dynamic configJson = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(configJsonPath));
                configJson.backend.url = beApiUrl;
                System.IO.File.WriteAllText(configJsonPath, JsonConvert.SerializeObject(configJson, JsonFormatting.Indented));
                Console.WriteLine("Patched backend.url");
            }
            
            return 0;
        }

        public static int PCT11312_Export(string[] args)
        {
            string plantsProductTypesPath = args[0];
            string myFactoryApiUrl = args[1];
            string physProdCatApiUrl = args[2];
            string pkgCalcApiUrl = args[3];
            string myFactoryApiToken = args[4];
            string physProdCatApiToken = args[5];
            string pkgCalcApiToken = args[6];
            string saveAsPfx = args[7];

            var myFactoryApiUri = new Uri(myFactoryApiUrl);
            var pkgCalcApiUri = new Uri(pkgCalcApiUrl);
            string myFactoryApiActualToken = pkgCalcApiUri.Host == myFactoryApiUri.Host ? pkgCalcApiToken : myFactoryApiToken;
            List<Tuple<string,List<string>>> plantsProducts = new();
            System.IO.File.ReadAllLines(plantsProductTypesPath).ToList().ForEach(ln => {
                var trimmed = ln?.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed)) 
                {
                    var flds = trimmed.Split('\t');
                    if (flds.Length >= 2)
                    {
                        var prodCat = flds[0]?.Trim();
                        List<string> currPlants = flds[1]?.Trim().Split(',').ToList().Select(s => s?.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
                        plantsProducts.Add(new Tuple<string, List<string>>(prodCat, currPlants));
                    }
                }
            });

            var allPhysProductsResp = ApiUtility.Get(physProdCatApiUrl, physProdCatApiToken);
            if (allPhysProductsResp.Item1 != HttpStatusCode.OK)
            {
                Console.Error.WriteLine($"{DateTime.Now:s}\tFailed to get physical products:{allPhysProductsResp.Item1}");
                return 1;
            }
            PhysicalProductCatalog.Client.Models.GetAllPhysicalProductsResponse allPhysProducts = JsonConvert.DeserializeObject<PhysicalProductCatalog.Client.Models.GetAllPhysicalProductsResponse>(allPhysProductsResp.Item2);

            var plants = plantsProducts.SelectMany(t => t.Item2).Distinct().ToList();
            Dictionary<string, Shipping.PackagingCalculator.Models.GetAllProductsResponse> prodCatsByPlant = new();

            foreach (var plant in plants)
            {
                var currProdsByPlantResp = ApiUtility.Get($"{myFactoryApiUrl}{plant}", myFactoryApiActualToken);
                if (currProdsByPlantResp.Item1 != HttpStatusCode.OK)
                {
                    Console.Error.WriteLine($"{DateTime.Now:s}\tError getting product categories by plant ({plant}):{currProdsByPlantResp.Item1}");
                    continue;
                }
                Shipping.PackagingCalculator.Models.GetAllProductsResponse prods = JsonConvert.DeserializeObject<Shipping.PackagingCalculator.Models.GetAllProductsResponse>(currProdsByPlantResp.Item2);
                prodCatsByPlant.Add(plant, prods);
            }

            // var unsupportedInputs = todo!
            Console.WriteLine("Unsupported product groups(categories) by plant(s):");
            foreach (var plant in plants)
            {
                var currProdGroupsRequested = plantsProducts.Where(pp => pp.Item2.Contains(plant)).Select(pp => pp.Item1).Distinct().ToList();
                var currUnsupported = currProdGroupsRequested.Except(!prodCatsByPlant.ContainsKey(plant) ? Enumerable.Empty<string>() : prodCatsByPlant[plant].ProductCategories.Select(c => c.Name).ToList());
                if (true == currUnsupported?.Any())
                {
                    Console.WriteLine($"{plant}:{string.Join(',', currUnsupported)}");
                }
            }
            Console.WriteLine(new string('-', 33));
            Console.WriteLine($"{nameof(prodCatsByPlant)}:");
            Dictionary<string, List<string>> prodCatsByPlantSimple = new();
            foreach (var key in prodCatsByPlant.Keys) {
                prodCatsByPlantSimple.Add(key, prodCatsByPlant[key].ProductCategories.Select(c => c.Name).ToList());
            }
            Console.WriteLine(JsonConvert.SerializeObject(prodCatsByPlantSimple, JsonFormatting.Indented));
            Console.WriteLine(new string('-', 33));
            foreach (var plantCat in plantsProducts)
            {
                CalculateProductDimensionsRequest req = new();
                if (req.Tasks == null)
                    req.Tasks = new List<CalculateProductDimensionsTask>();
                foreach (var plant in plantCat.Item2)
                {
                    if (!prodCatsByPlant.ContainsKey(plant))
                    {
                        Console.WriteLine($"{DateTime.Now:s}\t!prodCatsByPlant.ContainsKey({plant})");
                        continue;
                    }
                    var paps = prodCatsByPlant[plant].ProductCategories.FirstOrDefault(pc => string.Compare(pc.Name,plantCat.Item1, true) == 0)?.Articles.Select(a => a.ArticleCode.ToUpper()).ToList();
                    var papsPhysProds = allPhysProducts?.PhysicalProducts?.Where(a => !string.IsNullOrWhiteSpace(a?.Code) && true == paps?.Contains(a?.Code?.ToUpper()));
                    req.Tasks.Add(new()
                    {
                        FactoryCode = plant,
                        ProductCodes = paps,
                        Options = papsPhysProds.SelectMany(a => a.Options).Select(o => new ProductOption() { IsRequired = true, OptionName = o.Key, OptionValues = o.Values.Select(v => v.Value).ToList()}).DistinctBy(o => o.OptionName).ToList()
                    }) ;
                }
                //detect incomplete tasks:
                var incompleteTasks = req.Tasks.Where(t => false == t?.ProductCodes?.Any() && false == t.Options?.Any()).ToList();
                incompleteTasks?.ForEach(t => { Console.Error.WriteLine($"Incomplete task:{JsonConvert.SerializeObject(t, JsonFormatting.None)}");
                    req.Tasks.Remove(t);
                });
                
                var reqStr = JsonConvert.SerializeObject(req, JsonFormatting.None);
                var calcResp = ApiUtility.Post(pkgCalcApiUrl, pkgCalcApiToken, reqStr).ConfigureAwait(false).GetAwaiter().GetResult();
                if (calcResp.Item1 != HttpStatusCode.OK)
                {
                    Console.Error.WriteLine($"{DateTime.Now:s}\t{plantCat.Item1}|{string.Join(',',plantCat.Item2)}:Calculate request failed({(int)calcResp.Item1}|{calcResp.Item1}):{calcResp.Item2}\r\n{reqStr}");
                }
                else 
                {
                    //var currSaveAs = Path.Combine(saveAsPfx, string.Format("{0}_{1}.csv", string.Join('_', plantCat.Item2), plantCat.Item1));
                    var currSaveAs = Path.Combine(saveAsPfx, string.Format("{0}_{1}.csv", string.Join('_', req.Tasks.Select(t => t.FactoryCode).ToList()), plantCat.Item1));
                    Console.WriteLine($"{DateTime.Now:s}\t{currSaveAs}::{nameof(req)}:");
                    Console.WriteLine(JsonConvert.SerializeObject(req, JsonFormatting.Indented));
                    try {
                        System.IO.File.WriteAllText(currSaveAs, calcResp.Item2);
                    } catch (Exception ex) {
                        Console.Error.WriteLine($"{DateTime.Now:s}\tError saving export to '{currSaveAs}':{ex}");
                    }
                    
                    Console.WriteLine(new string('-',33));
                }
            }
            return 0;
        }
        #endregion
        #region aux
        public static int ExitWithComplaints(string msg, int ret)
        {
            return ExitWithComplaints(msg, ret, false);
        }
        public static int ExitWithComplaints(string msg, int ret, bool beep)
        {
            Console.WriteLine(msg);
            if (beep)
                Console.Beep();
            return ret;
        }

        private static void LogCmdArgs(string methodName, string[] args)
        {
            StringBuilder sbArgs = new StringBuilder();
            sbArgs.AppendFormat("routed to method {0}(", methodName);
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                    sbArgs.AppendLine(string.Format("[{0}]: '{1}'", i, args[i]));
            }
            sbArgs.AppendLine(")");
            Console.WriteLine(sbArgs.ToString());
        }

        private static string TryGetArg(string[] args, int ordinal, string defaultValue)
        {
            return args.Length > ordinal ? args[ordinal] : defaultValue;
        }
        #endregion
    }
}