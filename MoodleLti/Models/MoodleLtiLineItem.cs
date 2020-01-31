using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoodleLti.Models
{
    /// <summary>
    /// Structure of a line item (i.e., column in the gradebook).
    /// </summary>
    public class MoodleLtiLineItem
    {
        /// <summary>
        /// The URL to access the line item. When an LTI request is sent, the value for this option is derived from the <see cref="IdNumeric"/> property.
        /// </summary>
        [JsonProperty("id")]
        internal string StringId { get; set; }

        /// <summary>
        /// Numeric representation of the line item's ID.
        /// </summary>
        [JsonProperty("idNumeric")]
        public int Id { get; set; }

        /// <summary>
        /// The name of the line item.
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; }

        /// <summary>
        /// The maximum score of the line item.
        /// </summary>
        [JsonProperty("scoreMaximum")]
        public float ScoreMaximum { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        /// <summary>
        /// Tag for automated grade computations.
        /// </summary>
        [JsonProperty("tag")]
        public string Tag { get; set; }

        /// <summary>
        /// ID of the "external tool" instance where this line item belongs to.
        /// </summary>
        [JsonProperty("resourceLinkId")]
        public string ResourceLinkId { get; set; }

        /// <summary>
        /// ? Non standard.
        /// </summary>
        [JsonProperty("ltiLinkId")]
        public string LtiLinkId { get; set; }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        /// <returns></returns>
        public MoodleLtiLineItem Clone()
        {
            // This method generates a shallow copy, which is correct here:
            // All members are either primitive number types, or strings, which are immutable and thus can safely be re-used
            return (MoodleLtiLineItem)MemberwiseClone();
        }
    }
}
