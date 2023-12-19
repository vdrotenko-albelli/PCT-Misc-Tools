using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib
{
    public class ApiUtility
    {

        public static Tuple<HttpStatusCode,string> Post(string url, string authToken, string jsonContent, Dictionary<string, string> customHeaders = null)
        {
            const string RequestIdHdr = "Request-Id";
            List<Tuple<string, string, string>> urlsPaths = new List<Tuple<string, string, string>>();

            using (HttpClient wc = new HttpClient())
            {
                wc.DefaultRequestHeaders.Add("authorization", $"Bearer {authToken}");
                if (true != customHeaders?.Any())
                    wc.DefaultRequestHeaders.Add("Correlation-Context", "X-Is-Load-Test-Request=True");
                else
                {
                    foreach (string key in customHeaders.Keys)
                    {
                        wc.DefaultRequestHeaders.Add(key, customHeaders[key]);
                    }
                }
                using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    reqMsg.Headers.Add(RequestIdHdr, Guid.NewGuid().ToString());
                    reqMsg.Headers.Add("Accept", "application/json");
                    reqMsg.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    var resp = wc.SendAsync(reqMsg).ConfigureAwait(false).GetAwaiter().GetResult();
                    string respContent = null;
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        respContent = resp.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    else
                    {
                        
                        try
                        {
                            respContent = resp.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                        catch { }
                        respContent = $"{url}:{(int)resp.StatusCode} {resp.StatusCode}/{respContent}/{resp.ReasonPhrase}";
                    }
                    return new Tuple<HttpStatusCode,string>(resp.StatusCode, respContent);
                }
            }
        }
    }
}
