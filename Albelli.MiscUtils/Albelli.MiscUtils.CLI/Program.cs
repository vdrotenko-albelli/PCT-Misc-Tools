using Albelli.MiscUtils.Lib;
using Albelli.MiscUtils.Lib.AWSCli;
using Albelli.MiscUtils.Lib.ESLogs;
using Albelli.MiscUtils.Lib.Excel;
using Albelli.MiscUtils.Lib.PCT9944;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
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
            var hourly = eSLogEntries.GroupBy(e => e.ts.Hour).Select(grp => new { Hour = grp.Key, Count = grp.Count(), RPM = (decimal) grp.Count() / 60.0M }).ToList();
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
            var minutely = eSLogEntriesAll.GroupBy(e => e.ts.ToString("yyyy-MM-dd_HH:mm")).Select(grp => new { Minute = grp.Key, Count = grp.Count(), RPM = grp.Count()}).ToList();
            var maxMinutely = minutely.Max(e => e.RPM);
            Console.WriteLine($"max RPM (by minute): {maxMinutely}");

            var secondly = eSLogEntriesAll.GroupBy(e => e.ts.ToString("yyyy-MM-dd_HH:mm:ss")).Select(grp => new { Second = grp.Key, Count = grp.Count()}).ToList();
            var maxSecondly = secondly.Max(e => e.Count);
            Console.WriteLine($"max parallel requests (by second): {maxSecondly}");

            var aboveOurNormalBestPct_HourlyAvg = Math.Round(100.0M * hourly.Where(h => h.RPM > normalBestRPM).Count() / hourly.Count(),2);
            var aboveOurNormalBestPct_Minutely = Math.Round(100.0M * minutely.Where(h => h.RPM > normalBestRPM).Count() / minutely.Count(), 2);
            var aboveOurNormalBestPct_Secondly = Math.Round(100.0M * secondly.Where(h => h.Count> ourNormalVUs).Count() / secondly.Count(), 2);
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
            List<Tuple<string,string>> urlsPaths = new List<Tuple<string,string>>();
            var lns = System.IO.File.ReadAllLines(inFilePath);
            foreach (var ln in lns)
            {
                if (string.IsNullOrWhiteSpace(ln)) continue;
                var flds = ln.Split('\t');
                if (flds.Length < 2) continue;
                urlsPaths.Add(new Tuple<string,string>(flds[0].Trim(), flds[1].Trim()));
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
                            reqMsg.Content = new StringContent(pair.Item3,Encoding.UTF8, "application/json");
                            var resp = wc.SendAsync(reqMsg).ConfigureAwait(false).GetAwaiter().GetResult();
                            if (resp.StatusCode == HttpStatusCode.OK)
                            {
                                var responseContent = resp.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                                System.IO.File.WriteAllText(pair.Item2, responseContent);
                            }
                            else
                            {
                                string respContent = null;
                                try {
                                    respContent = resp.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                                } catch { }
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
            char[] sepChars = new char[] { '?','/',']', '"', '\''};
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
                    int pos1 = currCellTxt.IndexOfAny(sepChars,pos0+SALToken.Length+1);
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
                Console.WriteLine($"{Path.GetFileNameWithoutExtension( pair.Item2)}\t{orderDetails.Products.Count}\t{orderDetails.ProductionOrders.Count}\t{orderDetails.Fulfillment.Products.Count()}\t{currPOProdsCnt}\t{Pct(currProductsIds.Intersect(currPOProductIds).Count(), currProductsIds.Count)}\t{Pct(currProductsIds.Intersect(currFFProductIds).Count(), currProductsIds.Count)}\t{Pct(currFFProductIds.Except(currPOProductIds).Count(), currFFProductIds.Count)}\t{Pct(currPOProductIds.Except(currFFProductIds).Count(), currPOProductIds.Count)}\t{Pct(currFFProductIds.Except(currProductsIds).Count(), currFFProductIds.Count)}\t{Pct(currPOProductIds.Except(currProductsIds).Count(), currPOProductIds.Count)}\t{Pct(currPOProductIds.Intersect(currFFProductIds).Count(), currPOProductIds.Count)}\t{Pct(currFFProductIds.Intersect(currPOProductIds).Count(), currFFProductIds.Count)}");

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
                Console.WriteLine($"{Path.GetFileNameWithoutExtension(pair.Item2)}\t{string.Join(",",currFFProductIds)}");

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
                int maxPoShipmentsCnt = orderDetails.ProductionOrders != null && orderDetails.ProductionOrders.Any() ? orderDetails.ProductionOrders.Max(o => o.Shipments!= null && o.Shipments.Any() ? o.Shipments.Count : 0) : 0;
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
                try {
                    dynamic attrs = JsonConvert.DeserializeObject(attrsJson);
                    Console.WriteLine($"{queuUrl}:{attrs?.Attributes?.ApproximateNumberOfMessages}");
                }
                catch(Exception ex)
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
            var dt = Tools.Csv2DataTable(inputCsv, ',', maxBufferSize: 102400);
            Dictionary<string, PCT9944FullDiffInfo> diffs = new();
            Dictionary<string, PCT9944FullDiffInfo> missing = new();
            Dictionary<string, PCT9944FullDiffInfo> excessive = new();
            List<DiscrepancyLogEntry> des = new();
            foreach (DataRow dr in dt.Rows)
            {
                string currMsg = dr["Message"] as string;
                string currCorrId = dr["X-CorrelationId"] as string;
                var dle = DiscrepancyLogEntryParser.Parse(currMsg);
                dle.XCorrelationId = currCorrId;
                if (dt.Columns.Contains($"@{nameof(dle.timestamp_cw)}"))
                    dle.timestamp_cw = dr[$"@{nameof(dle.timestamp_cw)}"] as string;
                AccountForPCT9944(diffs, dle.DiffStr, currCorrId, dle.Input);
                AccountForPCT9944(missing, dle.Missing, currCorrId, dle.Input);
                AccountForPCT9944(excessive, dle.Excessive, currCorrId, dle.Input);
                des.Add(dle);
            }
            if (!string.IsNullOrWhiteSpace(outputTablePath))
            {
                //System.IO.File.WriteAllText(outputTablePath, JsonConvert.SerializeObject(des, Formatting.Indented));
                System.IO.File.WriteAllText(outputTablePath, DiscrepancyLogEntryPrinter.Print(des));
            }
            PrintPCT9944Keys(nameof(diffs),diffs);
            PrintPCT9944Keys(nameof(missing), missing);
            PrintPCT9944Keys(nameof(excessive), excessive);
            PrintPCT9944FullDiffInfo(nameof(diffs), diffs);
            return 0;
        }

        #region PCT-9944 discrepancies analyzer stuff
        private static void PrintPCT9944FullDiffInfo(string heading, Dictionary<string, PCT9944FullDiffInfo> diffs)
        {
            Console.WriteLine(new string('-', 33));
            Console.WriteLine($"{heading}:");
            Console.WriteLine(JsonConvert.SerializeObject(diffs,Formatting.Indented));
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
                PrintDataRows(rows, ds);
            }
            return 0;
        }

        public static int QueryExcel2(string[] args)
        {
            string excelPath = args[0];
            string query = args[1];
            string orderBy = args.Length > 2 ?  args[2] : string.Empty;
            using (DataTable ds = ExcelReader.Read(excelPath))
            {
                DataRow[] rows = string.IsNullOrWhiteSpace(orderBy) ? ds.Select(query) : ds.Select(query, orderBy);
                PrintDataRows(rows, ds);

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
            try {
                req_API = JsonConvert.DeserializeObject<AvailableCarriersRequestV2>(System.IO.File.ReadAllText(inputJsonPath));
            } catch { }
            
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
                resp.Candidates.ForEach(r => Console.WriteLine($"{r.MatrixRow.PK()}:{r.Verdict.ToString()}({string.Join(", ",r.NonMatchingFields)})\t{r.MatrixRow.Priority}"));
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
                        var centiroResponseProd = ApiUtility.Post(apiEndpointProd, apiAuthTokenProd, centiroV2ReqJson, corrIdExreHdrs);///corrIdExreHdrs
                        List<AvailableCarriersResponse> avCarrsProd = null;
                        if (centiroResponseProd.Item1 == HttpStatusCode.OK)
                        {
                            avCarrsProd = JsonConvert.DeserializeObject<List<AvailableCarriersResponse>>(centiroResponseProd.Item2);
                        }
                        var centiroResponseUat = ApiUtility.Post(apiEndpointUat, apiAuthTokenUat, centiroV2ReqJson, null);///corrIdExreHdrs
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
                        Console.WriteLine($"Error processing row # {rowIdx}\t{ex.Message}\t{ex.StackTrace?.Replace("\n"," ")?.Replace("\r", "")}");
                    }
                    rowIdx++;
                }
            }

            return 0;
        }

        public static int PCT9944BulkRevalidateChecker(string[] args)
        {
            string bulkReValidateOutputPath = args[0];
            //string 
            return 0;
        }
        private static void PrintDataRows(DataRow[] rows, DataTable ds)
        {
            foreach (DataRow dr in rows)
            {
                for(int c=0; c< ds.Columns.Count; c++)
                {
                    if (c > 0)
                        Console.Write('\t');
                    Console.Write(dr[c] as string);

                }
                Console.WriteLine();
            }
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