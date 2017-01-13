using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace JiGuangLinXin.App.App20Interface.Extension
{
    public class CrosHandler : DelegatingHandler
    {
        private const string _origin = "Origin";
        private const string _accessControlRequestMethod = "Access-Control-Request-Method";
        private const string _accessControlRequestHeaders = "Access-Control-Request-Headers";
        private const string _accessControlAllowOrigin = "Access-Control-Allow-Origin";
        private const string _accessControlAllowMethods = "Access-Control-Allow-Methods";
        private const string _accessControlAllowHeaders = "Access-Control-Allow-Headers";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            bool isCrosRequest = request.Headers.Contains(_origin);
            bool isPreflightRequest = request.Method == HttpMethod.Options;
            if (isCrosRequest)
            {
                Task<HttpResponseMessage> taskResult = null;
                if (isPreflightRequest)
                {
                    taskResult = Task.Factory.StartNew<HttpResponseMessage>(() =>
                    {
                        HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                        response.Headers.Add(_accessControlAllowOrigin, request.Headers.GetValues(_origin).FirstOrDefault());
                        string method = request.Headers.GetValues(_accessControlRequestMethod).FirstOrDefault();
                        if (method != null)
                        {
                            response.Headers.Add(_accessControlAllowMethods, method);
                        }
                        string headers = string.Join(", ", request.Headers.GetValues(_accessControlRequestHeaders));
                        if (!string.IsNullOrEmpty(headers))
                        {
                            response.Headers.Add(_accessControlAllowHeaders, headers);
                        }
                        return response;
                    }, cancellationToken);
                }
                else
                {
                    taskResult = base.SendAsync(request, cancellationToken)
                        .ContinueWith<HttpResponseMessage>(t =>
                        {
                            var response = t.Result;
                            response.Headers.Add(_accessControlAllowOrigin, request.Headers.GetValues(_origin).FirstOrDefault());
                            return response;
                        });
                }
                return taskResult;
                //return base.SendAsync(request, cancellationToken);
            }
            else
            {
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}