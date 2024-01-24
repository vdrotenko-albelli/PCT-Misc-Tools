using Albelli.MiscUtils.Lib;
using Albelli.MiscUtils.Lib.AWSCli;
using Albelli.MiscUtils.Lib.ESLogs;
using Albelli.MiscUtils.Lib.Excel;
using Albelli.MiscUtils.Lib.PCT10679;
using Albelli.MiscUtils.Lib.PCT9944;
using Albelli.MiscUtils.Lib.PCT9944.v1;
using Centiro.PromiseEngine.Client;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using ProductionOrderOperationsApi;
using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security;
using System.Text;
using static System.Net.WebRequestMethods;

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
            Console.WriteLine($"{nameof(duplicates)}: {JsonConvert.SerializeObject(duplicates, Formatting.Indented)}");
            */
            Console.WriteLine(new string('-', 33));
            var hourly = eSLogEntries.GroupBy(e => e.ts.Hour).Select(grp => new { Hour = grp.Key, Count = grp.Count(), RPM = (decimal)grp.Count() / 60.0M }).ToList();
            Console.WriteLine($"{nameof(hourly)}: {JsonConvert.SerializeObject(hourly, Formatting.Indented)}");
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
            Console.WriteLine($"{nameof(minutely)} above our normal best (count={minutelyAboveNormalBest.Count()} out of {minutely.Count}): {JsonConvert.SerializeObject(minutelyAboveNormalBest, Formatting.Indented)}");
            Console.WriteLine(new string('-', 33));
            var hourlyAboveNormalBest = hourly.Where(m => m.RPM > normalBestRPM).OrderByDescending(h => h.RPM);
            Console.WriteLine($"{nameof(hourly)} above our normal best(count={hourlyAboveNormalBest.Count()} out of {hourly.Count}): {JsonConvert.SerializeObject(hourlyAboveNormalBest, Formatting.Indented)}");

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
                //Console.WriteLine($"{nameof(currProductsIds)}:{JsonConvert.SerializeObject(currProductsIds, Formatting.None)}");
                var currFFProductIds = orderDetails.Fulfillment.Products.Select(p => p.ProductCode).Distinct().ToList();
                //Console.WriteLine($"{nameof(currFFProductIds)}:{JsonConvert.SerializeObject(currFFProductIds, Formatting.None)}");
                var currPOProductIds = new List<string>();
                orderDetails.ProductionOrders.ForEach(po => po.Shipments.ForEach(s => currPOProductIds.AddRange(s.Products.Distinct())));
                //Console.WriteLine($"{nameof(currPOProductIds)}:{JsonConvert.SerializeObject(currPOProductIds, Formatting.None)}");
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
                //Console.WriteLine($"{nameof(currProductsIds)}:{JsonConvert.SerializeObject(currProductsIds, Formatting.None)}");
                var currFFProductIds = orderDetails.Fulfillment.Products.Select(p => p.ProductCode).Distinct().ToList();
                //Console.WriteLine($"{nameof(currFFProductIds)}:{JsonConvert.SerializeObject(currFFProductIds, Formatting.None)}");


                //Console.WriteLine($"{nameof(currPOProductIds)}:{JsonConvert.SerializeObject(currPOProductIds, Formatting.None)}");
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
                //Console.WriteLine($"{nameof(currProductsIds)}:{JsonConvert.SerializeObject(currProductsIds, Formatting.None)}");
                var currFFProductIds = orderDetails.Fulfillment?.Products?.Select(p => p.ProductCode).Distinct().ToList();
                if (currFFProductIds == null) currFFProductIds = new List<string>();
                //Console.WriteLine($"{nameof(currFFProductIds)}:{JsonConvert.SerializeObject(currFFProductIds, Formatting.None)}");
                var currPOProductIds = new List<string>();
                var currFForPOProductIdsRaw = new List<string>();
                currFForPOProductIdsRaw.AddRange(currPOProductIds);
                currFForPOProductIdsRaw.AddRange(currFFProductIds);
                var currFForPOProductIds = currFForPOProductIdsRaw.Distinct().ToList();
                orderDetails.ProductionOrders?.ForEach(po => po.Shipments.ForEach(s => currPOProductIds.AddRange(s.Products.Distinct())));
                //Console.WriteLine($"{nameof(currPOProductIds)}:{JsonConvert.SerializeObject(currPOProductIds, Formatting.None)}");
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
            //Console.WriteLine(JsonConvert.SerializeObject(stats, Formatting.Indented));
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
            Console.WriteLine(JsonConvert.SerializeObject(diffs, Formatting.Indented));
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
                //Console.WriteLine(JsonConvert.SerializeObject(resp, Formatting.Indented));
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
                        string centiroV2ReqJson = JsonConvert.SerializeObject(centiroV2Req, Formatting.None);
                        DateTime ts = DateTime.Now;
                        var centiroResponseProd = ApiUtility.Post(apiEndpointProd, apiAuthTokenProd, centiroV2ReqJson, corrIdExreHdrs);///corrIdExreHdrs_Prod
                        List<AvailableCarriersResponse> avCarrsProd = null;
                        if (centiroResponseProd.Item1 == HttpStatusCode.OK)
                        {
                            avCarrsProd = JsonConvert.DeserializeObject<List<AvailableCarriersResponse>>(centiroResponseProd.Item2);
                        }
                        var centiroResponseUat = ApiUtility.Post(apiEndpointUat, apiAuthTokenUat, centiroV2ReqJson, null);///corrIdExreHdrs_Prod
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
                        string centiroV2ReqJson = JsonConvert.SerializeObject(centiroV2ReqOurs, Formatting.None);
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
                            var centiroResponseProd = ApiUtility.Post(apiEndpointProd, apiAuthTokenProd, centiroV2ReqJson, corrIdExreHdrs_Prod);
                            if (centiroResponseProd.Item1 == HttpStatusCode.OK)
                            {
                                avCarrsProd = JsonConvert.DeserializeObject<List<AvailableCarriersResponse>>(centiroResponseProd.Item2);
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(apiEndpointUat) && !string.IsNullOrWhiteSpace(apiAuthTokenUat))
                        {
                            currApiCorrId_Uat = Guid.NewGuid().ToString();
                            var corrIdExreHdrs_Uat = new Dictionary<string, string>() { { "X-CorrelationId", currApiCorrId_Uat } };
                            var centiroResponseUat = ApiUtility.Post(apiEndpointUat, apiAuthTokenUat, centiroV2ReqJson, corrIdExreHdrs_Uat);

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
                        var trueCentiroV2ReqJson = JsonConvert.SerializeObject(trueCentiroV2Req, Formatting.None);
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

            Console.WriteLine(JsonConvert.SerializeObject(req, Formatting.Indented));
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
            Console.WriteLine(JsonConvert.SerializeObject(entries, Formatting.Indented));
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
                    var apiResp = ApiUtility.Post(apiUrl, authToken, entry.JSContent, null);
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

            var allEntries = new List<Tuple<CalculateCarrierModel,string,string>>();
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
                    catch(Exception ex)
                    {
                            Console.Error.WriteLine($"Error deserializing or converting an entry(v2):{ex},\n{e.JSContent}");
                    }
                }
            });

            ProgressTracker progressTracker = new ProgressTracker();
            progressTracker.Start(allEntries.Count, System.Console.Out, DateTime.Now);
            List< PCT9247ReplayCompareV1vsV2Entry> rslt = new();
            foreach (var entry in allEntries)
            {
                var dr = new PCT9247ReplayCompareV1vsV2Entry();
                var XCorrelationId = Guid.NewGuid().ToString();
                dr.CalculateCarrierModel = JsonConvert.SerializeObject(entry.Item1, Formatting.Indented);
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
                        var v1Resp = ApiUtility.Post(apiUrlV1, authTokenV1, v1ReqJson, corrIdHdrs);
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
                        var v2Resp = ApiUtility.Post(apiUrlV2, authTokenV2, v2ReqJson, corrIdHdrs);
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
        #endregion

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
        #endregion
    }
}