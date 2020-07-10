using Microsoft.Extensions.Options;
using MoodleLti.Models;
using MoodleLti.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MoodleLti
{
    /// <summary>
    /// Provides abstracted means to read and write to the Moodle gradebook.
    /// </summary>
    public class MoodleGradebook : IMoodleGradebook
    {
        private readonly string _resourceLinkId;
        private readonly IMoodleLtiApi _ltiApi;

        /// <summary>
        /// Creates a new gradebook instance using dependency injection.
        /// </summary>
        /// <param name="ltiApi">LTI API communication object.</param>
        /// <param name="options">Configuration parameters.</param>
        public MoodleGradebook(IMoodleLtiApi ltiApi, IOptions<MoodleLtiOptions> options)
            : this(options.Value.ResourceLinkId)
        {
            _ltiApi = ltiApi;
        }

        /// <summary>
        /// Creates a new gradebook instance using the given configuration paramaters.
        /// </summary>
        /// <param name="httpClient">The HTTP client object to use for the requests.</param>
        /// <param name="baseUrl">The URL of the Moodle instance.</param>
        /// <param name="courseId">The ID of the affected Moodle course.</param>
        /// <param name="toolTypeId">The ID of the external tool definition.</param>
        /// <param name="resourceLinkId">The ID of the external tool instance.</param>
        /// <param name="consumerKey">The OAuth consumer key to use for signing the requests.</param>
        /// <param name="sharedSecret">The OAuth shared secret to use for signing the requests.</param>
        public MoodleGradebook(HttpClient httpClient, string baseUrl, int courseId, int toolTypeId, string resourceLinkId, string consumerKey, string sharedSecret)
            : this(resourceLinkId)
        {
            // Create API connection object
            _ltiApi = new MoodleLtiApi(httpClient, baseUrl, courseId, toolTypeId, consumerKey, sharedSecret);
        }

        /// <summary>
        /// Stores the given configuration parameters.
        /// </summary>
        /// <param name="resourceLinkId">The ID of the external tool instance.</param>
        private MoodleGradebook(string resourceLinkId)
        {
            _resourceLinkId = resourceLinkId;
        }

        public virtual async Task<IEnumerable<MoodleGradebookColumn>> GetColumnsAsync()
        {
            // Retrieve line items and create column objects
            return (await _ltiApi.GetLineItemsAsync()).Select(lineItem => new MoodleGradebookColumn(lineItem));
        }

        public virtual async Task<MoodleGradebookColumn> GetColumnAsync(int id)
        {
            var lineItem = await _ltiApi.GetLineItemAsync(id);
            return new MoodleGradebookColumn(lineItem);
        }

        public virtual async Task UpdateColumnAsync(MoodleGradebookColumn column)
        {
            // Update line item
            await _ltiApi.UpdateLineItemAsync(column.LineItem);
        }

        public virtual async Task<MoodleGradebookColumn> CreateColumnAsync(string title, float maximumScore, string tag = null)
        {
            // Title must not be null
            if(title == null)
                throw new ArgumentNullException(nameof(title));

            // Score must be positive
            if(maximumScore < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumScore), "The maximum score must not be negative.");

            // Create line item
            var lineItem = new MoodleLtiLineItem
            {
                Label = title,
                ResourceLinkId = _resourceLinkId,
                ScoreMaximum = maximumScore,
                Tag = tag
            };
            int id = await _ltiApi.CreateLineItemAsync(lineItem);

            // Retrieve and return created line item
            var createdLineItem = await _ltiApi.GetLineItemAsync(id);
            return new MoodleGradebookColumn(createdLineItem);
        }

        public virtual async Task DeleteColumnAsync(int columnId)
        {
            // Delete column
            await _ltiApi.DeleteLineItemAsync(columnId);
        }

        public virtual async Task SetGradeAsync(int columnId, int userId, MoodleGradebookGrade grade)
        {
            // Sanity check
            if(grade.Score < 0)
                throw new ArgumentOutOfRangeException(nameof(grade.Score), "The score must not be negative.");

            // Retrieve affected column
            var column = await GetColumnAsync(columnId);

            // Prepare score object
            var score = new MoodleLtiScore
            {
                UserId = userId.ToString(),
                ScoreGiven = grade.Score,
                Comment = grade.Comment,
                Timestamp = grade.Timestamp,
                ScoreMaximum = column.MaximumScore,
                ActivityProgress = ActivityProgressStatus.Completed,
                GradingProgress = GradingProgressStatus.FullyGraded
            };

            // Store score
            await _ltiApi.UpdateScoreAsync(columnId, score);
        }
    }
}
