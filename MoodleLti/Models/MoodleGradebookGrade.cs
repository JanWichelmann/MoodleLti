using System;
using System.Collections.Generic;
using System.Text;

namespace MoodleLti.Models
{
    /// <summary>
    /// Represents a grade in the Moodle gradebook.
    /// </summary>
    public class MoodleGradebookGrade
    {
        /// <summary>
        /// The score.
        /// </summary>
        public float Score { get; set; } = 0;

        /// <summary>
        /// Comment with details regarding the grade.
        /// </summary>
        public string Comment { get; set; } = "";

        /// <summary>
        /// Date and time of the last update of the score.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
