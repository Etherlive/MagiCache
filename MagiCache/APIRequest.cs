using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace MagiCache
{
    public class APIRequest
    {
        #region Properties

        public string cookie { get; set; }

        public string data { get; set; }

        public bool isPost
        {
            get { return this.method.ToUpper() == "POST"; }
        }

        public string method { get; set; }
        public string type { get; set; }
        public string url { get; set; }

        #endregion Properties

        #region Methods

        public bool CheckInCache(out string response, out int status_code)
        {
            response = "Missed Cache";
            status_code = 110;

            var inCache = RequestCache.TryGetFromCache(this, out var cachedRequest);

            if (inCache)
            {
                response = cachedRequest.response_body;
                status_code = cachedRequest.response_code;
            }
            return inCache;
        }

        public int ExecuteRequest(string origin, out string response)
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.UseCookies = true;

                    var trimmedOrigin = origin.Split("/").Last();

                    foreach (var cook in cookie.Split("; "))
                    {
                        var cook_seg = cook.Split("=");
                        handler.CookieContainer.Add(new System.Net.Cookie(cook_seg[0], cook_seg[1], "/", trimmedOrigin));
                    }

                    var wholeURL = this.isPost ? this.url : this.url + "?" + this.data;

                    if (wholeURL.StartsWith("/") || origin.EndsWith("/"))
                    {
                        wholeURL = origin + wholeURL;
                    }
                    else
                    {
                        wholeURL = origin + "/" + wholeURL;
                    }

                    using (var cli = new HttpClient(handler))
                    {
                        var req = new HttpRequestMessage(HttpMethod.Post, wholeURL);

                        req.Headers.Add("origin", origin);

                        if (isPost)
                        {
                            req.Content = new StringContent(this.data, Encoding.UTF8, this.type.Split(";")[0]);
                        }

                        var res = cli.Send(req);

                        var res_stream = new StreamReader(res.Content.ReadAsStream());
                        var res_body = res_stream.ReadToEnd();

                        RequestCache.AddToCache(this, res_body, (int)res.StatusCode);

                        response = res_body;
                        return (int)res.StatusCode;
                    }
                }
            }
            catch (Exception e)
            {
                response = e.ToString();
                return 400;
            }
        }

        #endregion Methods
    }
}