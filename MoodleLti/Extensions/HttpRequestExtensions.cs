﻿/*
This file is copied from LtiLibrary.AspNetCore: https://github.com/LtiLibrary/LtiLibrary/blob/master/src/LtiLibrary.AspNetCore/Extensions/HttpRequestExtensions.cs
LtiLibrary is licensed under Apache License 2.0.
This is a temporary workaround for https://github.com/LtiLibrary/LtiLibrary/issues/124, to avoid including the LtiLibrary.AspNetCore package.
TODO remove this once issue is fixed
 */

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using LtiLibrary.NetCore.Common;
using LtiLibrary.NetCore.Lti.v1;
using LtiLibrary.NetCore.OAuth;
using Microsoft.AspNetCore.Http;

namespace LtiLibrary.AspNetCore.Extensions
{
    /// <summary>
    /// <see cref="HttpRequest"/> extension methods.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Get a value indicating whether the current request is authenticated
        /// using LTI.
        /// </summary>
        public static bool IsAuthenticatedWithLti(this HttpRequest request)
        {
            // Normal LTI launch with form parameters
            if (request.HasFormContentType)
            {
                var messageTypeArray = request.Form[LtiConstants.LtiMessageTypeParameter];
                var messageType = messageTypeArray.Count > 0 ? messageTypeArray[0] : string.Empty;
                return request.Method.Equals("POST")
                       && (
                           messageType.Equals(LtiConstants.BasicLaunchLtiMessageType,
                               StringComparison.OrdinalIgnoreCase)
                           || messageType.Equals(LtiConstants.ContentItemSelectionRequestLtiMessageType,
                               StringComparison.OrdinalIgnoreCase)
                           || messageType.Equals(LtiConstants.ContentItemSelectionLtiMessageType,
                               StringComparison.OrdinalIgnoreCase)
                       );
            }

            // Otherwise look for an OAuth Authorization header
            return request.ParseAuthenticationHeader().HasKeys();
        }

        /// <summary>
        /// Parse the Authentication header into a <see cref="NameValueCollection"/> of OAuth parameters.
        /// Non-OAuth parameters are ignored.
        /// </summary>
        /// <returns>A <see cref="NameValueCollection"/> with each OAuth parameter in the header.</returns>
        private static NameValueCollection ParseAuthenticationHeader(this HttpRequest request)
        {
            var parameters = new NameValueCollection();
            var headerValues = request.Headers[OAuthConstants.AuthorizationHeader];
            if (headerValues.Count == 0) return parameters;

            var header = headerValues[0];
            var schemeSeparatorIndex = header.IndexOf(' ');
            var scheme = header.Substring(0, schemeSeparatorIndex);
            if (!scheme.Equals(OAuthConstants.AuthScheme)) return parameters;

            var headerParameter = header.Substring(schemeSeparatorIndex + 1);
            foreach (var pair in headerParameter.Split(','))
            {
                var keyValue = pair.Split('=');
                var key = keyValue[0].Trim();

                // Ignore unknown parameters
                if (!OAuthConstants.OAuthParameters.Any(p => p.Equals(key)))
                    continue;

                var value = System.Net.WebUtility.UrlDecode(keyValue[1].Trim('"'));
                parameters.Set(key, value);
            }
            return parameters;
        }

        /// <summary>
        /// Parse the <see cref="HttpRequest"/> into an <see cref="LtiRequest"/> object.
        /// </summary>
        /// <returns>The <see cref="LtiRequest"/> with every LTI value found in the request filled in.</returns>
        public static async Task<LtiRequest> ParseLtiRequestAsync(this HttpRequest request)
        {
            var ltiRequest = new LtiRequest(null)
            {
                Url = request.GetUri(),
                HttpMethod = request.Method
            };

            // LTI launch and content item requests are sent as form posts
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync().ConfigureAwait(false);
                foreach (var pair in form)
                {
                    foreach (var stringValue in pair.Value)
                    {
                        ltiRequest.AddParameter(pair.Key, stringValue);
                    }
                }
                return ltiRequest;
            }

            // All other LTI requests pass the authentication parameters in an
            // Authorization header
            var headerParameters = request.ParseAuthenticationHeader();
            foreach (var name in headerParameters.AllKeys)
            {
                var values = headerParameters.GetValues(name);
                if (values != null)
                {
                    foreach (var value in values)
                    {
                        ltiRequest.AddParameter(name, value);
                    }
                }
            }
            // Save the BodyHash calculated by the AddBodyHashHeaderAttribute
            if (request.Headers["BodyHash"].Any())
            {
                ltiRequest.BodyHashReceived = request.Headers["BodyHash"].First();
            }
            return ltiRequest;
        }

        /// <summary>
        /// Get the <see cref="Uri"/> from the <see cref="HttpRequest"/>.
        /// </summary>
        /// <returns>The fully formed <see cref="Uri"/> of the request.</returns>
        public static Uri GetUri(this HttpRequest request)
        {
            var hostComponents = request.Host.ToUriComponent().Split(':');

            var builder = new UriBuilder
            {
                Scheme = request.Scheme,
                Host = hostComponents[0],
                Path = request.PathBase + request.Path,
                Query = request.QueryString.ToUriComponent()
            };

            if (hostComponents.Length == 2)
            {
                builder.Port = Convert.ToInt32(hostComponents[1]);
            }

            return builder.Uri;
        }
    }
}