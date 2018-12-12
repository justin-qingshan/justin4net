using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace just4net.net
{
    public class HttpClient
    {
        private List<Tuple<string, string>> parameters = new List<Tuple<string, string>>();
        private string bodyContent;
        private string method;
        private string url;

        private HttpClient() { }

        public static HttpClient Get(string url)
        {
            return Url(url, HttpMethod.GET);
        }

        public static HttpClient Post(string url)
        {
            return Url(url, HttpMethod.POST);
        }

        public static HttpClient Url(string url, string method)
        {
            HttpClient client = new HttpClient();
            client.url = url;
            client.method = method;
            return client;
        }

        public HttpClient AddParam(string key, object value)
        {
            parameters.Add(Tuple.Create(key, value.ToString()));
            return this;
        }

        public HttpClient Body(string body)
        {
            bodyContent = body;
            return this;
        }
        
        public StringResult StringResult(int timeout = 3000)
        {
            HttpWebResponse response = Run(timeout);
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string content = reader.ReadToEnd();
            reader.Close();
            response.Close();
            return new StringResult
            {
                Status = response.StatusCode,
                Content = content
            };
        }

        private HttpWebResponse Run(int timeout)
        {
            if (parameters.Count != 0)
            {
                url += "?";
                foreach (Tuple<string, string> pair in parameters)
                    url += $"{pair.Item1}={pair.Item2}&";
                url.TrimEnd('&');
            }

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = method;
            if (!string.IsNullOrEmpty(bodyContent))
            {
                byte[] data = Encoding.UTF8.GetBytes(bodyContent);
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            request.Timeout = timeout;
            request.ReadWriteTimeout = 1000;

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            return response;
        }
    }

    public class HttpMethod
    {
        public const string GET = "GET";
        public const string POST = "POST";
        public const string DELETE = "DELETE";
        public const string PUT = "PUT";
    }
}
