using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagiCache
{
    public static class RequestCache
    {
        #region Fields

        private static ConcurrentDictionary<string, CachedRequest> _cache = new ConcurrentDictionary<string, CachedRequest>();
        private static double cacheForMinutes = 5;

        #endregion Fields

        #region Methods

        private static string getKey(APIRequest request)
        {
            var data = request.isPost ? request.data : String.Join('&', request.data.Split("&").Where(x => !x.StartsWith("local") && !x.StartsWith("nd")));

            return $"{request.url}-{request.method}-{request.type}-{data}";
        }

        public static void AddToCache(APIRequest request, string response_body, int response_code)
        {
            var cache_req = new CachedRequest(request, response_body, response_code);
            var key = getKey(request);

            if (_cache.TryGetValue(key, out var old))
            {
                _cache.TryUpdate(key, cache_req, old);
            }
            else
            {
                _cache.TryAdd(key, cache_req);
            }
        }

        public static bool TryGetFromCache(APIRequest request, out CachedRequest cachedRequest)
        {
            var key = getKey(request);

            var inCache = _cache.TryGetValue(key, out cachedRequest);

            if (inCache)
            {
                return (DateTime.Now - cachedRequest.cachedAt).TotalMinutes <= cacheForMinutes;
            }

            return false;
        }

        #endregion Methods

        #region Classes

        public class CachedRequest
        {
            #region Fields

            public DateTime cachedAt = DateTime.Now;

            public APIRequest request;

            public string response_body;

            public int response_code;

            #endregion Fields

            #region Constructors

            public CachedRequest(APIRequest request, string response_body, int response_code)
            {
                this.request = request;
                this.response_code = response_code;
                this.response_body = response_body;
            }

            #endregion Constructors
        }

        #endregion Classes
    }
}