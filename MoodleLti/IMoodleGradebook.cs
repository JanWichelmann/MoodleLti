using MoodleLti.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoodleLti
{
    public interface IMoodleGradebook
    {
        /// <summary>
        /// Creates a new column in the gradebook.
        /// </summary>
        /// <param name="title">The column title.</param>
        /// <param name="maximumScore">The maximum assignable score.</param>
        /// <param name="tag">Optional. A tag for automatic grading.</param>
        /// <returns></returns>
        Task<MoodleGradebookColumn> CreateColumnAsync(string title, float maximumScore, string tag = null);

        /// <summary>
        /// Deletes the given gradebook column.
        /// </summary>
        /// <param name="columnId">The ID of the column to be deleted.</param>
        /// <returns></returns>
        Task DeleteColumnAsync(int columnId);

        /// <summary>
        /// Returns the gradebook column with the given ID.
        /// </summary>
        /// <param name="id">ID of the column to retrieve.</param>
        /// <returns></returns>
        Task<MoodleGradebookColumn> GetColumnAsync(int id);

        /// <summary>
        /// Returns a list of all gradebook columns.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MoodleGradebookColumn>> GetColumnsAsync();

        /// <summary>
        /// Sets a grade for the given user.
        /// </summary>
        /// <param name="columnId">The gradebook column where the grade should be added.</param>
        /// <param name="userId">The user who is graded.</param>
        /// <param name="grade">The grade.</param>
        /// <returns></returns>
        Task SetGradeAsync(int columnId, int userId, MoodleGradebookGrade grade);

        /// <summary>
        /// Writes the changes in the given grade column to the Moodle database.
        /// </summary>
        /// <param name="column">The column to update.</param>
        Task UpdateColumnAsync(MoodleGradebookColumn column);
    }
}