using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodleLti.Options
{
    /// <summary>
    /// Defines configuration options needed for LTI communication.
    /// </summary>
    public class MoodleLtiOptions
    {
        /// <summary>
        /// The URL of the Moodle instance.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// The ID of the affected Moodle course.
        /// </summary>
        public int CourseId { get; set; }

        /// <summary>
        /// The ID of the external tool definition.
        /// </summary>
        public int ToolTypeId { get; set; }

        /// <summary>
        /// The ID used to distinguish line items from different instances of the same external tool.
        /// </summary>
        public string ResourceLinkId { get; set; }

        /// <summary>
        /// The OAuth consumer key to use for signing the requests.
        /// </summary>
        public string OAuthConsumerKey { get; set; }

        /// <summary>
        /// The OAuth shared secret to use for signing the requests.
        /// </summary>
        public string OAuthSharedSecret { get; set; }
    }
}
