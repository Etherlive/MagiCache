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
        public string method { get; set; }
        public string type { get; set; }
        public string url { get; set; }

        #endregion Properties

        #region Methods

        public int ExecuteAndReturnStatus(string origin, out string response)
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

                    using (var cli = new HttpClient(handler))
                    {
                        var req = new HttpRequestMessage(this.method.ToUpper() == "POST" ? HttpMethod.Post : HttpMethod.Get, origin + "/" + this.url);

                        req.Headers.Add("origin", origin);

                        req.Content = new StringContent(this.data, Encoding.UTF8, this.type.Split(";")[0]);

                        var res = cli.Send(req);

                        var res_stream = new StreamReader(res.Content.ReadAsStream());
                        var res_body = res_stream.ReadToEnd();

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