﻿using Sigma.Core.Domain.Interface;
using Newtonsoft.Json;
using RestSharp;

namespace Sigma.Core.Domain.Service
{
    public class HttpService : IHttpService
    {
        public async Task<RestResponse> PostAsync(string url, Object jsonBody)
        {
            RestClient client = new RestClient();
            RestRequest request = new RestRequest(url, Method.Post);
            string josn = JsonConvert.SerializeObject(jsonBody);
            request.AddJsonBody(jsonBody);
            var result = await client.ExecuteAsync(request);
            return result;
        }
    }
}
