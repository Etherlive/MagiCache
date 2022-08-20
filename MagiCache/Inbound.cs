using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MagiCache
{
    public static class Inbound
    {
        #region Fields

        private static HttpListener httpListener;

        #endregion Fields

        #region Methods

        private static void MagiCacheReq(HttpListenerRequest request, HttpListenerResponse response, StreamWriter response_stream)
        {
            var b_stream = new StreamReader(request.InputStream);
            var body = b_stream.ReadToEnd();

            try
            {
                var apiReq = JsonSerializer.Deserialize<APIRequest>(body);

                string res_body;
                if (apiReq.CheckInCache(out res_body, out int status_code))
                {
                    response.StatusCode = status_code;
                    response.AppendHeader("cache-hit", "true");
                }
                else
                {
                    response.StatusCode = apiReq.ExecuteRequest(request.Headers["Origin"], out res_body);
                    response.AppendHeader("cache-hit", "false");
                }
                response_stream.Write(res_body);

                Logger.Log(apiReq, response.StatusCode, res_body);
            }
            catch (JsonException e)
            {
                response.StatusCode = 400;
                response_stream.Write("Json Parse Failure");
            }
            catch (Exception e)
            {
                response.StatusCode = 500;
                response_stream.Write(e.ToString());
            }
        }

        private static void ReqBegin(IAsyncResult cont)
        {
            var context = httpListener.EndGetContext(cont);
            httpListener.BeginGetContext(ReqBegin, null);

            var request = context.Request;
            var response = context.Response;
            var response_stream = new StreamWriter(response.OutputStream);

            response.AppendHeader("Access-Control-Allow-Origin", "*");
            response.AppendHeader("Access-Control-Allow-Headers", "content-type");
            response.AppendHeader("Allow", "OPTIONS,POST");

            switch (request.HttpMethod)
            {
                case "GET":
                    if (request.Url.AbsolutePath == "/logs")
                    {
                        response_stream.Write(String.Join("\r\n\r\n", Logger.GetLog()));
                    }
                    break;

                case "POST":
                    MagiCacheReq(request, response, response_stream);
                    break;

                case "OPTIONS":
                    response_stream.Write("Options Returned");
                    break;

                default:
                    response.StatusCode = 405;
                    response_stream.Write("Method Not Permitted");
                    break;
            }

            response_stream.Flush();
            response_stream.Close();
            response.Close();
        }

        public static void Init()
        {
            httpListener = new HttpListener();

            if (Debugger.IsAttached)
            {
                httpListener.Prefixes.Add("http://localhost:8080/");
            }
            else
            {
                httpListener.Prefixes.Add("http://+:80/");
            }

            httpListener.Start();
            httpListener.BeginGetContext(ReqBegin, null);
        }

        #endregion Methods
    }
}