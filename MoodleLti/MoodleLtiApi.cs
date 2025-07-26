using LtiLibrary.NetCore.Clients;
using LtiLibrary.NetCore.OAuth;
using Microsoft.Extensions.Options;
using MoodleLti.Extensions;
using MoodleLti.Models;
using MoodleLti.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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
            (string responseBody, HttpStatusCode _) = await DoGetRequestAsync(url, "application/vnd.ims.lis.v2.lineitemcontainer+json", new[] { HttpStatusCode.OK });
            var lineItems = JsonConvert.DeserializeObject<List<MoodleLtiLineItem>>(responseBody);

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
            (string responseBody, HttpStatusCode _) = await DoPostRequestAsync(url, serializedLineItem, "application/vnd.ims.lis.v2.lineitem+json", new[] { HttpStatusCode.Created });

            // Try to deserialize result
            MoodleLtiLineItem resultLineItem = JsonConvert.DeserializeObject<MoodleLtiLineItem>(responseBody);

            // Extract ID
            return GetNumericLineItemId(resultLineItem.StringId);
        }

        public async Task<MoodleLtiLineItem> GetLineItemAsync(int id)
        {
            // Build URL
            string url = $"{_serviceUrl}/lineitems/{id}/lineitem?{_queryStringPrefix}";

            // Retrieve line item
            (string responseBody, HttpStatusCode _) = await DoGetRequestAsync(url, "application/vnd.ims.lis.v2.lineitem+json", new[] { HttpStatusCode.OK });
            var lineItem = JsonConvert.DeserializeObject<MoodleLtiLineItem>(responseBody);
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
            await DoPutRequestAsync(url, serializedLineItem, "application/vnd.ims.lis.v2.lineitem+json", new[] { HttpStatusCode.OK });
        }

        public async Task DeleteLineItemAsync(int id)
        {
            // Build URL
            string url = $"{_serviceUrl}/lineitems/{id}/lineitem?{_queryStringPrefix}";

            // Delete line item
            await DoDeleteRequestAsync(url, new[] { HttpStatusCode.NoContent });
        }

        public async Task UpdateScoreAsync(int lineItemId, MoodleLtiScore score)
        {
            // Build URL
            string url = $"{_serviceUrl}/lineitems/{lineItemId}/scores?{_queryStringPrefix}";

            // Serialize and send score
            string serializedScore = JsonConvert.SerializeObject(score, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            await DoPostRequestAsync(url, serializedScore, "application/vnd.ims.lis.v1.score+json", new[] { HttpStatusCode.OK });
        }

        /// <summary>
        /// Performs a GET request and returns the response body and its status code.
        /// </summary>
        /// <param name="url">The target URL.</param>
        /// <param name="expectedContentType">The expected response content type.</param>
        /// <param name="expectedStatusCodes">The expected status codes. All other status codes trigger an exception.</param>
        /// <exception cref="MoodleLtiException">Thrown when an unexpected HTTP status code is returned.</exception>
        private async Task<(string responseBody, HttpStatusCode statusCode)> DoGetRequestAsync(string url, string expectedContentType, HttpStatusCode[] expectedStatusCodes)
        {
            // Assign content type
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(expectedContentType));

            // Sign request object
            var reqMessage = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Content = new StringContent(string.Empty)
            };
            await SecuredClient.SignRequest(_httpClient, reqMessage, _consumerKey, _sharedSecret, SignatureMethod.HmacSha1);

            // Send HTTP request and retrieve response
            using var response = await _httpClient.SendAsync(reqMessage);
            string responseBody = await response.ReadBody();
            if(expectedStatusCodes.Contains(response.StatusCode))
                return (responseBody, response.StatusCode);

            // An error occured
            throw CreateExceptionFromFailedRequest(
                response.StatusCode,
                expectedStatusCodes,
                response.RequestMessage.Headers.ToString(),
                string.Empty,
                response.Headers.ToString(),
                responseBody
            );
        }

        /// <summary>
        /// Performs a POST request and returns the response body and its status code.
        /// </summary>
        /// <param name="url">The target URL.</param>
        /// <param name="body">The POST body.</param>
        /// <param name="bodyContentType">The content type of the body.</param>
        /// <param name="expectedStatusCodes">The expected status codes. All other status codes trigger an exception.</param>
        /// <exception cref="MoodleLtiException">Thrown when an unexpected HTTP status code is returned.</exception>
        private async Task<(string responseBody, HttpStatusCode statusCode)> DoPostRequestAsync(string url, string body, string bodyContentType, HttpStatusCode[] expectedStatusCodes)
        {
            // Sign request object
            var encodedBody = new StringContent(body, Encoding.UTF8, bodyContentType);
            var reqMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = encodedBody
            };
            await SecuredClient.SignRequest(_httpClient, reqMessage, _consumerKey, _sharedSecret, SignatureMethod.HmacSha1);

            // Send HTTP request and retrieve response
            using var response = await _httpClient.SendAsync(reqMessage);
            string responseBody = await response.ReadBody();
            if(expectedStatusCodes.Contains(response.StatusCode))
                return (responseBody, response.StatusCode);

            // An error occured
            throw CreateExceptionFromFailedRequest(
                response.StatusCode,
                expectedStatusCodes,
                response.RequestMessage.Headers.ToString(),
                body,
                response.Headers.ToString(),
                responseBody
            );
        }

        /// <summary>
        /// Performs a PUT request and returns the response body and its status code.
        /// </summary>
        /// <param name="url">The target URL.</param>
        /// <param name="body">The PUT body.</param>
        /// <param name="bodyContentType">The content type of the body.</param>
        /// <param name="expectedStatusCodes">The expected status codes. All other status codes trigger an exception.</param>
        /// <exception cref="MoodleLtiException">Thrown when an unexpected HTTP status code is returned.</exception>
        private async Task<(string responseBody, HttpStatusCode statusCode)> DoPutRequestAsync(string url, string body, string bodyContentType, HttpStatusCode[] expectedStatusCodes)
        {
            // Sign request object
            var encodedBody = new StringContent(body, Encoding.UTF8, bodyContentType);
            var reqMessage = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = encodedBody
            };
            await SecuredClient.SignRequest(_httpClient, reqMessage, _consumerKey, _sharedSecret, SignatureMethod.HmacSha1);

            // Send HTTP request and retrieve response
            using var response = await _httpClient.SendAsync(reqMessage);
            string responseBody = await response.ReadBody();
            if(expectedStatusCodes.Contains(response.StatusCode))
                return (responseBody, response.StatusCode);

            // An error occured
            throw CreateExceptionFromFailedRequest(
                response.StatusCode,
                expectedStatusCodes,
                response.RequestMessage.Headers.ToString(),
                body,
                response.Headers.ToString(),
                responseBody
            );
        }

        /// <summary>
        /// Performs a DELETE request and returns the response body and its status code.
        /// </summary>
        /// <param name="url">The target URL.</param>
        /// <param name="expectedStatusCodes">The expected status codes. All other status codes trigger an exception.</param>
        /// <exception cref="MoodleLtiException">Thrown when an unexpected HTTP status code is returned.</exception>
        private async Task<(string responseBody, HttpStatusCode statusCode)> DoDeleteRequestAsync(string url, HttpStatusCode[] expectedStatusCodes)
        {
            // Sign request object
            var reqMessage = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = new StringContent(string.Empty)
            };
            await SecuredClient.SignRequest(_httpClient, reqMessage, _consumerKey, _sharedSecret, SignatureMethod.HmacSha1);

            // Send HTTP request and retrieve response
            using var response = await _httpClient.SendAsync(reqMessage);
            string responseBody = await response.ReadBody();
            if(expectedStatusCodes.Contains(response.StatusCode))
                return (responseBody, response.StatusCode);

            // An error occured
            throw CreateExceptionFromFailedRequest(
                response.StatusCode,
                expectedStatusCodes,
                response.RequestMessage.Headers.ToString(),
                string.Empty,
                response.Headers.ToString(),
                responseBody
            );
        }

        /// <summary>
        /// Creates a new <see cref="MoodleLtiException"/> from the given HTTP request data.
        /// </summary>
        /// <param name="statusCode">Server status code.</param>
        /// <param name="expectedStatusCodes">Expected server status codes.</param>
        /// <param name="requestHeaders">Request headers.</param>
        /// <param name="requestBody">Request body.</param>
        /// <param name="responseHeaders">Response headers.</param>
        /// <param name="responseBody">Response body.</param>
        /// <returns></returns>
        private static MoodleLtiException CreateExceptionFromFailedRequest(
            HttpStatusCode statusCode,
            HttpStatusCode[] expectedStatusCodes,
            string requestHeaders,
            string requestBody,
            string responseHeaders,
            string responseBody)
        {
            string expectedStatusCodesString = string.Join(", ", expectedStatusCodes);
            var exception = new MoodleLtiException($"Unexpected HTTP status: {statusCode} (expected: {expectedStatusCodesString})");
            exception.Data.Add("StatusCode", statusCode);
            exception.Data.Add("StatusCodesExpected", expectedStatusCodesString);
            exception.Data.Add("RequestHeaders", requestHeaders);
            exception.Data.Add("RequestBody", requestBody);
            exception.Data.Add("ResponseHeaders", responseHeaders);
            exception.Data.Add("ResponseBody", responseBody);
            return exception;
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
