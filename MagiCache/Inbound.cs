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

            if (request.HttpMethod == "POST")
            {
                var b_stream = new StreamReader(request.InputStream);
                var body = b_stream.ReadToEnd();

                try
                {
                    var apiReq = JsonSerializer.Deserialize<APIRequest>(body);
                    response.StatusCode = apiReq.ExecuteAndReturnStatus(request.Headers["Origin"], out string res_body);
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
                response.AddHeader("Access-Control-Allow-Headers", "content-type");
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Allow", "OPTIONS,POST");
            }
            else
            {
                response.StatusCode = 405;
                response_stream.Write("Method Not Permitted");
            }

            response.OutputStream.Flush();
            response.OutputStream.Close();
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