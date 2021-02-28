using LtiLibrary.AspNetCore.Extensions;
using LtiLibrary.NetCore.Common;
using LtiLibrary.NetCore.Extensions;
using LtiLibrary.NetCore.Lti.v1;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MoodleLti
{
    /// <summary>
    /// Handles authentication via Moodle.
    /// </summary>
    public class MoodleAuthenticationTools
    {
        /// <summary>
        /// Parses the LTI authentication request and returns a matching <see cref="MoodleAuthenticationMessageData"/> object on success.
        /// If the authentication fails, an exception is thrown.
        /// </summary>
        /// <param name="httpRequest">The LTI tool launch HTTP request object.</param>
        /// <param name="consumerKey">The consumer key used for OAuth.</param>
        /// <param name="sharedSecret">The shared secret used for OAuth.</param>
        /// <param name="logger">Optional. Logger for debugging purposes.</param>
        /// <exception cref="LtiException">Thrown when the request does not form a valid LTI request.</exception>
        /// <exception cref="SecurityException">Thrown when the OAuth signature check fails.</exception>
        /// <returns></returns>
        public static async Task<MoodleAuthenticationMessageData> ParseAuthenticationRequestAsync(HttpRequest httpRequest, string consumerKey, string sharedSecret, ILogger<MoodleAuthenticationTools> logger = null)
        {
            // Parse and validate the request
            var ltiRequest = await httpRequest.ParseLtiRequestAsync();
            logger?.LogDebug("Parsed LTI request for URL {Url}, method {Method}", ltiRequest.Url, ltiRequest.HttpMethod);
            ltiRequest.CheckForRequiredLtiParameters();

            // Check OAuth signature
            if(!ltiRequest.ConsumerKey.Equals(consumerKey))
                throw new SecurityException($"The OAuth consumer keys differ: Moodle sent \"{ltiRequest.ConsumerKey}\", but \"{consumerKey}\" was expected.");
            var oauthSignature = ltiRequest.GenerateSignature(sharedSecret);
            if(!oauthSignature.Equals(ltiRequest.Signature))
                throw new SecurityException($"The OAuth signatures differ: Moodle sent \"{ltiRequest.Signature}\", but \"{oauthSignature}\" was expected.");

            // Done
            return new MoodleAuthenticationMessageData(ltiRequest);
        }
    }
}
