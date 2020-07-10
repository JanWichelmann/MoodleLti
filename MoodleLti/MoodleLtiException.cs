using System;
using System.Collections.Generic;
using System.Text;

namespace MoodleLti
{
    /// <summary>
    /// Thrown when LTI unexpectedly returned an empty result set.
    /// </summary>
    public class MoodleLtiException : Exception
    {
        public MoodleLtiException(string message)
            : base(message) { }

        public MoodleLtiException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
