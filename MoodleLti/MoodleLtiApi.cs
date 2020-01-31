using LtiLibrary.NetCore.Clients;
using LtiLibrary.NetCore.Common;
using LtiLibrary.NetCore.OAuth;
using Microsoft.Extensions.Options;
using MoodleLti.Extensions;
using MoodleLti.Models;
using MoodleLti.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MoodleLti
{
    /// <summary>
    /// Implements low-level functions to communicate with Moodle via the LTI API.
    /// TODO move documentation to interface
    /// </summary>
    public class MoodleLtiApi : IMoodleLtiApi
    {
        private readonly HttpClient _httpClient;
        private readonly string _serviceUrl;
        private readonly string _queryStringPrefix;
        private readonly string _consumerKey;
        private readonly string _sharedSecret;

        /// <summary>
        /// Creates a new API communication object using dependency injection.
        /// </summary>
        /// <param name="httpClientFactory">HTTP client factory.</param>
        /// <param name="options">LTI configuration.</param>
        public MoodleLtiApi(IHttpClientFactory httpClientFactory, IOptions<MoodleLtiOptions> options)
            : this(httpClientFactory.CreateClient(), options.Value.BaseUrl, options.Value.CourseId, options.Value.ToolTypeId, options.Value.OAuthConsumerKey, options.Value.OAuthSharedSecret)
        { }

        /// <summary>
        /// Creates a new API communication object. This constructor is for internal use and console applications only. Use dependency injection when possible!
        /// </summary>
        /// <param name="httpClient">The HTTP client object to use for the requests.</param>
        /// <param name="baseUrl">The URL of the Moodle instance.</param>
        /// <param name="courseId">The ID of the affected Moodle course.</param>
        /// <param name="toolTypeId">The ID of the external tool definition.</param>
        /// <param name="consumerKey">The OAuth consumer key to use for signing the requests.</param>
        /// <param name="sharedSecret">The OAuth shared secret to use for signing the requests.</param>
        public MoodleLtiApi(HttpClient httpClient, string baseUrl, int courseId, int toolTypeId, string consumerKey, string sharedSecret)
        {
            _httpClient = httpClient;
            _consumerKey = consumerKey;
            _sharedSecret = sharedSecret;

            // Build service URL from base URL and course ID
            _serviceUrl = baseUrl;
            if(!_serviceUrl.EndsWith('/'))
                _serviceUrl += '/';
            _serviceUrl += "mod/lti/services.php/" + courseId;

            // Build query string prefix
            _queryStringPrefix = "type_id=" + toolTypeId;
        }

        public async Task<List<MoodleLtiLineItem>> GetLineItemsAsync()
        {
            // Build URL
            // TODO Paging support
            string url = $"{_serviceUrl}/lineitems?{_queryStringPrefix}";

            // Perform request and parse response body
            string response = await DoGetRequestAsync(url, "application/vnd.ims.lis.v2.lineitemcontainer+json");
            var lineItems = JsonConvert.DeserializeObject<List<MoodleLtiLineItem>>(response);

            // Generate numeric IDs
            foreach(var lineItem in lineItems)
                lineItem.Id = GetNumericLineItemId(lineItem.StringId);

            return lineItems;
        }

        public async Task<int> CreateLineItemAsync(MoodleLtiLineItem lineItem)
        {
            // Build URL
            string url = $"{_serviceUrl}/lineitems?{_queryStringPrefix}";

            // Serialize line item
            string serializedLineItem = JsonConvert.SerializeObject(lineItem, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            string result = await DoPostRequestAsync(url, serializedLineItem, "application/vnd.ims.lis.v2.lineitem+json");

            // Try to deserialize result
            MoodleLtiLineItem resultLineItem = JsonConvert.DeserializeObject<MoodleLtiLineItem>(result);

            // Extract ID
            return GetNumericLineItemId(resultLineItem.StringId);
        }

        public async Task<MoodleLtiLineItem> GetLineItemAsync(int id)
        {
            // Build URL
            string url = $"{_serviceUrl}/lineitems/{id}/lineitem?{_queryStringPrefix}";

            // Retrieve line item
            string response = await DoGetRequestAsync(url, "application/vnd.ims.lis.v2.lineitem+json");
            var lineItem = JsonConvert.DeserializeObject<MoodleLtiLineItem>(response);
            lineItem.Id = GetNumericLineItemId(lineItem.StringId);

            return lineItem;
        }

        public async Task UpdateLineItemAsync(MoodleLtiLineItem lineItem)
        {
            // Build URL
            string url = $"{_serviceUrl}/lineitems/{lineItem.Id}/lineitem?{_queryStringPrefix}";

            // Serialize line item
            lineItem.StringId = url;
            string serializedLineItem = JsonConvert.SerializeObject(lineItem, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            string result = await DoPutRequestAsync(url, serializedLineItem, "application/vnd.ims.lis.v2.lineitem+json");

            // TODO check result
        }

        public async Task DeleteLineItemAsync(int id)
        {
            // Build URL
            string url = $"{_serviceUrl}/lineitems/{id}/lineitem?{_queryStringPrefix}";

            // Delete line item
            string result = await DoDeleteRequestAsync(url);

            // TODO check result
        }

        public async Task UpdateScoreAsync(int lineItemId, MoodleLtiScore score)
        {
            // Build URL
            string url = $"{_serviceUrl}/lineitems/{lineItemId}/scores?{_queryStringPrefix}";

            // Serialize and send score
            string serializedScore = JsonConvert.SerializeObject(score, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            string result = await DoPostRequestAsync(url, serializedScore, "application/vnd.ims.lis.v1.score+json");

            // TODO check result
        }

        /// <summary>
        /// Performs a GET request and returns the response body, if it was successful.
        /// </summary>
        /// <param name="url">The target URL.</param>
        /// <param name="expectedContentType">The expected response content type.</param>
        /// <returns></returns>
        private async Task<string> DoGetRequestAsync(string url, string expectedContentType)
        {
            // Assign content type
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(expectedContentType));

            // Sign request object
            await SecuredClient.SignRequest(_httpClient, HttpMethod.Get, url, new StringContent(string.Empty), _consumerKey, _sharedSecret, SignatureMethod.HmacSha1);

            // Send HTTP request and retrieve response
            // TODO exception handling
            using(var response = await _httpClient.GetAsync(url))
            {
                if(response.StatusCode == HttpStatusCode.OK)
                    return await response.ReadBody();
            }
            return default;
        }

        /// <summary>
        /// Performs a POST request and returns the response body, if it was successful.
        /// </summary>
        /// <param name="url">The target URL.</param>
        /// <param name="body">The POST body.</param>
        /// <param name="bodyContentType">The content type of the body.</param>
        /// <returns></returns>
        private async Task<string> DoPostRequestAsync(string url, string body, string bodyContentType)
        {
            // Sign request object
            var encodedBody = new StringContent(body, Encoding.UTF8, bodyContentType);
            await SecuredClient.SignRequest(_httpClient, HttpMethod.Post, url, encodedBody, _consumerKey, _sharedSecret, SignatureMethod.HmacSha1);

            // Send HTTP request and retrieve response
            // TODO exception handling
            using var response = await _httpClient.PostAsync(url, encodedBody);
            return await response.ReadBody();
        }

        /// <summary>
        /// Performs a PUT request and returns the response body, if it was successful.
        /// </summary>
        /// <param name="url">The target URL.</param>
        /// <param name="body">The PUT body.</param>
        /// <param name="bodyContentType">The content type of the body.</param>
        /// <returns></returns>
        private async Task<string> DoPutRequestAsync(string url, string body, string bodyContentType)
        {
            // Sign request object
            var encodedBody = new StringContent(body, Encoding.UTF8, bodyContentType);
            await SecuredClient.SignRequest(_httpClient, HttpMethod.Put, url, encodedBody, _consumerKey, _sharedSecret, SignatureMethod.HmacSha1);

            // Send HTTP request and retrieve response
            // TODO exception handling
            using var response = await _httpClient.PutAsync(url, encodedBody);
            return await response.ReadBody();
        }

        /// <summary>
        /// Performs a DELETE request and returns the response body, if it was successful.
        /// </summary>
        /// <param name="url">The target URL.</param>
        /// <returns></returns>
        private async Task<string> DoDeleteRequestAsync(string url)
        {
            // Sign request object
            await SecuredClient.SignRequest(_httpClient, HttpMethod.Delete, url, new StringContent(string.Empty), _consumerKey, _sharedSecret, SignatureMethod.HmacSha1);

            // Send HTTP request and retrieve response
            // TODO exception handling
            using(var response = await _httpClient.DeleteAsync(url))
            {
                // Should return 204 NoContent
            }
            return default;
        }

        /// <summary>
        /// Retrieves a numeric ID from the URL-style line item ID returned by LTI.
        /// </summary>
        /// <param name="id">The URL-style line item ID string to parse.</param>
        /// <returns></returns>
        private static int GetNumericLineItemId(string id)
        {
            Regex idRegex = new Regex("services\\.php\\/[0-9]+/lineitems/([0-9]+)/lineitem");
            var match = idRegex.Match(id);
            if(!match.Success)
                return -1; // TODO exception handling
            return int.Parse(match.Groups[1].Value);
        }
    }
}
