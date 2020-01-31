using System;
using System.Collections.Generic;
using System.Text;

namespace MoodleLti.Models
{
    /// <summary>
    /// Represents a column (aka exercise) in the Moodle gradebook.
    /// Modifications at this object do *not* directly translate to the Moodle database; instead the modifications have to be explicitly submitted using the <see cref="MoodleGradebook.UpdateColumnAsync(MoodleGradebookColumn)"/> method.
    /// </summary>
    public class MoodleGradebookColumn
    {
        /// <summary>
        /// Creates a new gradebook column object from the given line item.
        /// </summary>
        /// <param name="lineItem">The associated line item.</param>
        internal MoodleGradebookColumn(MoodleLtiLineItem lineItem)
        {
            LineItem = lineItem;
        }

        /// <summary>
        /// The associated line item.
        /// </summary>
        internal MoodleLtiLineItem LineItem { get; set; }

        /// <summary>
        /// The column ID.
        /// </summary>
        public int Id
        {
            get => LineItem.Id;
            set => LineItem.Id = value;
        }

        /// <summary>
        /// The column title.
        /// </summary>
        public string Title
        {
            get => LineItem.Label;
            set => LineItem.Label = value;
        }

        /// <summary>
        /// The maximum possible score for this column.
        /// </summary>
        public float MaximumScore
        {
            get => LineItem.ScoreMaximum;
            set => LineItem.ScoreMaximum = value;
        }

        /// <summary>
        /// Tag for automated grade computations.
        /// </summary>
        public string Tag
        {
            get => LineItem.Tag;
            set => LineItem.Tag = value;
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        /// <returns></returns>
        public MoodleGradebookColumn Clone()
        {
            // Create copy
            return new MoodleGradebookColumn(LineItem.Clone());
        }
    }
}
