using Newtonsoft.Json;
using System;

namespace MoodleLti.Models
{
    /// <summary>
    /// Structure of a score entry in the grade book.
    /// </summary>
    public class MoodleLtiScore
    {
        /// <summary>
        /// Recipient of the score.
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// The score value.
        /// </summary>
        [JsonProperty("scoreGiven")]
        public float ScoreGiven { get; set; }

        /// <summary>
        /// The maximum possible score for this result.
        /// </summary>
        [JsonProperty("scoreMaximum")]
        public float ScoreMaximum { get; set; }

        /// <summary>
        /// Comment (feedback) shown alongside the score.
        /// </summary>
        [JsonProperty("comment")]
        public string Comment { get; set; }

        /// <summary>
        /// Date and time when score was last modified.
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Status towards activity completion. This is ignored by Moodle.
        /// </summary>
        [JsonProperty("activityProgress")]
        public ActivityProgressStatus ActivityProgress { get; set; }

        /// <summary>
        /// Status of grading progress. Moodle stores the score only when this is set to <see cref="GradingProgressStatus.FullyGraded"/>.
        /// </summary>
        [JsonProperty("gradingProgress")]
        public GradingProgressStatus GradingProgress { get; set; }
    }

    public enum ActivityProgressStatus
    {
        Initialized,
        Started,
        InProgress,
        Submitted,
        Completed
    }

    public enum GradingProgressStatus
    {
        NotReady,
        Failed,
        Pending,
        PendingManual,
        FullyGraded
    }
}
