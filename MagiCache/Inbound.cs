using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Net;
using System.Threading.Tasks;

namespace MagiCache
{
    public static class Inbound
    {
        #region Fields

        private static HttpListener httpListener;

        #endregion Fields

        #region Methods

        private static void ReqStart(IAsyncResult cont)
        {
            var context = httpListener.EndGetContext(cont);
            httpListener.BeginGetContext(ReqStart, null);

            var request = context.Request;
            var response = context.Response;
            var response_stream = new StreamWriter(response.OutputStream);

            response.AppendHeader("Access-Control-Allow-Origin", "*");
            response.AppendHeader("Access-Control-Allow-Headers", "content-type");
            response.AppendHeader("Allow", "OPTIONS,POST");

            if (request.HttpMethod == "POST")
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
            else if (request.HttpMethod == "OPTIONS")
            {
                response_stream.Write("Options Returned");
            }
            else
            {
                response.StatusCode = 405;
                response_stream.Write("Method Not Permitted");
            }

            response_stream.Flush();
            response_stream.Close();
            response.Close();
        }

        public static void Init(int port = 8080)
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:" + port + "/");
            httpListener.Start();
            httpListener.BeginGetContext(ReqStart, null);
        }

        #endregion Methods
    }
}