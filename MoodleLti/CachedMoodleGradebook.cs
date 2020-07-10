using Microsoft.Extensions.Options;
using MoodleLti.Models;
using MoodleLti.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MoodleLti
{
    /// <summary>
    /// Provides abstracted means to read and write to the Moodle gradebook. Retrieved data is cached to reduce expensive LTI API queries. This class is thread-safe.
    /// </summary>
    public class CachedMoodleGradebook : MoodleGradebook
    {
        /// <summary>
        /// Caches the list of gradebook columns.
        /// </summary>
        private Dictionary<int, MoodleGradebookColumn> _gradebookColumns = null;

        /// <summary>
        /// Locking object.
        /// </summary>
        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Creates a new gradebook instance using dependency injection.
        /// </summary>
        /// <param name="ltiApi">LTI API communication object.</param>
        /// <param name="options">Configuration parameters.</param>
        public CachedMoodleGradebook(IMoodleLtiApi ltiApi, IOptions<MoodleLtiOptions> options)
            : base(ltiApi, options)
        {
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
        public CachedMoodleGradebook(HttpClient httpClient, string baseUrl, int courseId, int toolTypeId, string resourceLinkId, string consumerKey, string sharedSecret)
            : base(httpClient, baseUrl, courseId, toolTypeId, resourceLinkId, consumerKey, sharedSecret)
        {
        }

        /// <summary>
        /// Fills the internal cache. This method does not do additional locking, but assumes that the caller acquired the relevant locks.
        /// </summary>
        /// <returns></returns>
        private async Task FillCacheAsync()
        {
            // Fill cache
            var columns = await base.GetColumnsAsync();
            _gradebookColumns = columns.ToDictionary(col => col.Id, col => col.Clone());
        }

        public override async Task<IEnumerable<MoodleGradebookColumn>> GetColumnsAsync()
        {
            await _lock.WaitAsync();
            try
            {
                // Fill cache first, if necessary
                if(_gradebookColumns == null)
                    await FillCacheAsync();
                return _gradebookColumns.Values.AsEnumerable().Select(col => col.Clone()).ToList(); // ToList() ensures thread safety here, since Select() is lazy
            }
            finally
            {
                _lock.Release();
            }
        }

        public override async Task<MoodleGradebookColumn> GetColumnAsync(int id)
        {
            await _lock.WaitAsync();
            try
            {
                // Fill cache first, if necessary
                if(_gradebookColumns == null)
                    await FillCacheAsync();

                // Does the column exist?
                if(!_gradebookColumns.ContainsKey(id))
                    throw new MoodleLtiException("The requested column does not exist.");

                // Copy column from cache
                return _gradebookColumns[id].Clone();
            }
            finally
            {
                _lock.Release();
            }
        }

        public override async Task UpdateColumnAsync(MoodleGradebookColumn column)
        {
            await _lock.WaitAsync();
            try
            {
                // Fill cache first, if necessary
                if(_gradebookColumns == null)
                    await FillCacheAsync();

                // Check whether column exists
                if(!_gradebookColumns.ContainsKey(column.Id))
                    throw new MoodleLtiException("The requested column does not exist.");

                // Send update to server
                await base.UpdateColumnAsync(column);

                // Store updated column
                _gradebookColumns[column.Id] = column.Clone();
            }
            finally
            {
                _lock.Release();
            }
        }

        public override async Task<MoodleGradebookColumn> CreateColumnAsync(string title, float maximumScore, string tag = null)
        {
            await _lock.WaitAsync();
            try
            {
                // Fill cache first, if necessary
                if(_gradebookColumns == null)
                    await FillCacheAsync();

                // Create column and store a copy of it
                var column = await base.CreateColumnAsync(title, maximumScore, tag);
                _gradebookColumns.Add(column.Id, column.Clone());
                return column;
            }
            finally
            {
                _lock.Release();
            }
        }

        public override async Task DeleteColumnAsync(int columnId)
        {
            await _lock.WaitAsync();
            try
            {
                // Fill cache first, if necessary
                if(_gradebookColumns == null)
                    await FillCacheAsync();

                // Check whether column exists
                if(!_gradebookColumns.ContainsKey(columnId))
                    throw new MoodleLtiException("The requested column does not exist.");

                // Delete column
                await base.DeleteColumnAsync(columnId);
                _gradebookColumns.Remove(columnId);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
