using LtiLibrary.NetCore.Lti.v1;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace MoodleLti
{
    /// <summary>
    /// Provides access to the data sent in a LTI tool launch request.
    /// </summary>
    public class MoodleAuthenticationMessageData
    {
        /// <summary>
        /// Creates a new data container from the given LTI request/response.
        /// </summary>
        /// <param name="ltiRequest">The LTI request/response object containing the basic launch data.</param>
        internal MoodleAuthenticationMessageData(LtiRequest ltiRequest)
        {
            LtiRequest = ltiRequest;

            // Fill public fields
            // Check for null values, which should not occur for a valid tool configuration
            Email = ltiRequest.LisPersonEmailPrimary ?? throw new ArgumentNullException(nameof(ltiRequest.LisPersonEmailPrimary));
            FullName = ltiRequest.LisPersonNameFull ?? throw new ArgumentNullException(nameof(ltiRequest.LisPersonNameFull));
            CourseId = int.Parse(ltiRequest.ContextId);
            CourseShortName = ltiRequest.ContextLabel ?? throw new ArgumentNullException(nameof(ltiRequest.ContextLabel));
            CourseLongName = ltiRequest.ContextTitle ?? throw new ArgumentNullException(nameof(ltiRequest.ContextTitle));
            UserId = int.Parse(ltiRequest.UserId);
            ExternalToolName = ltiRequest.ResourceLinkTitle ?? throw new ArgumentNullException(nameof(ltiRequest.ResourceLinkTitle));
            ExternalToolDescription = ltiRequest.ResourceLinkDescription;
            ResourceLinkId = ltiRequest.ResourceLinkId ?? throw new ArgumentNullException(nameof(ltiRequest.ResourceLinkId));

            // Try to retrieve login name
            LoginName = LtiRequest.Parameters.FirstOrDefault(p => p.Key == "ext_user_username").Value;
        }

        public string LoginName { get; }

        public string Email { get; }

        public string FullName { get; }

        public int CourseId { get; }

        public string CourseShortName { get; }

        public string CourseLongName { get; }

        public int UserId { get; }

        public string ExternalToolName { get; }

        public string ExternalToolDescription { get; }

        public string ResourceLinkId { get; }

        /// <summary>
        /// Returns the underlying LTI request object. This object is not serialized and thus only available when directly generated from the authentication API.
        /// </summary>
        [JsonIgnore]
        public LtiRequest LtiRequest { get; }
    }
}
