using MoodleLti.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoodleLti
{
    public interface IMoodleLtiApi
    {
        /// <summary>
        /// Creates a new line item and returns its ID.
        /// </summary>
        /// <param name="lineItem">Line item object describing the line item to be created. This object is _not_ updated, use <see cref="GetLineItemAsync(int)"/> for retrieving the full line item instead.</param>
        /// <returns></returns>
        /// <exception cref="MoodleLtiException">Thrown when an unexpected HTTP status code is returned.</exception>
        Task<int> CreateLineItemAsync(MoodleLtiLineItem lineItem);

        /// <summary>
        /// Deletes the given line item.
        /// </summary>
        /// <param name="id">The ID of the line item to delete.</param>
        /// <returns></returns>
        /// <exception cref="MoodleLtiException">Thrown when an unexpected HTTP status code is returned.</exception>
        Task DeleteLineItemAsync(int id);

        /// <summary>
        /// Retrieves the line item with the given ID.
        /// </summary>
        /// <param name="id">The ID of the line item to retrieve.</param>
        /// <returns></returns>
        /// <exception cref="MoodleLtiException">Thrown when an unexpected HTTP status code is returned.</exception>
        Task<MoodleLtiLineItem> GetLineItemAsync(int id);

        /// <summary>
        /// Returns a list of line items.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MoodleLtiException">Thrown when an unexpected HTTP status code is returned.</exception>
        Task<List<MoodleLtiLineItem>> GetLineItemsAsync();

        /// <summary>
        /// Updates the given line item on the server.
        /// </summary>
        /// <param name="lineItem">The line item to update.</param>
        /// <returns></returns>
        /// <exception cref="MoodleLtiException">Thrown when an unexpected HTTP status code is returned.</exception>
        Task UpdateLineItemAsync(MoodleLtiLineItem lineItem);

        /// <summary>
        /// Sets a user's score for the given line item.
        /// </summary>
        /// <param name="lineItemId">The line item to assign the score to.</param>
        /// <param name="score">Score data.</param>
        /// <returns></returns>
        Task UpdateScoreAsync(int lineItemId, MoodleLtiScore score);
    }
}