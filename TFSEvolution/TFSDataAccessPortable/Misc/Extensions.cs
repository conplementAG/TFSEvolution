#region License and Terms

// /***************************************************************************
// Copyright (c) 2015 Conplement AG
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  
// ***************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TFSDataAccessPortable
{
    internal static class Helper
    {
        internal static void AddQueryValue(ref string origQuery, string queryParam, string queryValue)
        {
            if (origQuery.Contains("?"))
            {
                origQuery += "&";
            }
            else
            {
                origQuery += "?";
            }

            origQuery += string.Format("{0}={1}", queryParam, queryValue);
        }

        internal static List<List<JToken>> SplitList(this List<JToken> tokens, int nSize = 200)
        {
            List<List<JToken>> list = new List<List<JToken>>();

            for (int i = 0; i < tokens.Count; i += nSize)
            {
                list.Add(tokens.GetRange(i, Math.Min(nSize, tokens.Count - i)));
            }

            return list;
        }

        internal static async Task<dynamic> GetDynamicAsync(this HttpClient httpClient, Uri uri)
        {
            var strResponse = await httpClient.GetStringAsync(uri);

            if (strResponse != "null")
            {
                return JObject.Parse(strResponse);
            }

            return null;
        }

        internal static async Task<dynamic> GetDynamicArrayAsync(this HttpClient httpClient, Uri uri)
        {
            var strResponse = await httpClient.GetStringAsync(uri);
            if (strResponse != "null")
            {
                return JArray.Parse(strResponse);
            }

            return null;
        }

        internal static async Task<dynamic> PushAndGetDynamicAsync(this HttpClient httpClient, JObject pushObject,
            Uri uri)
        {
            string postBody = pushObject.ToString();

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response =
                await httpClient.PostAsync(uri, new StringContent(postBody, Encoding.UTF8, "application/json"));
            var strResponse = await response.Content.ReadAsStringAsync();
            return JObject.Parse(strResponse);
        }

        internal static async Task PatchAsync(this HttpClient httpClient, JObject pushObject, Uri uri)
        {
            string patchBody = pushObject.ToString();

            var content = new StringContent(patchBody);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                RequestUri = uri,
                Content = content
            };

            using (HttpResponseMessage response = httpClient.SendAsync(request).Result)
            {
                await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }
        }

        internal static async Task<dynamic> PatchArrayAsync(this HttpClient httpClient, JArray pushObject, Uri uri)
        {
            string patchBody = pushObject.ToString();

            var content = new StringContent(patchBody);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json-patch+json"));

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                RequestUri = uri,
                Content = content
            };

            using (HttpResponseMessage response = httpClient.SendAsync(request).Result)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                return JObject.Parse(responseBody);
            }
        }

        internal static WorkItemData GetWorkItemByID(this List<WorkItemData> workitems, int id)
        {
            return workitems.Find(x => x.ID == id);
        }
    }
}