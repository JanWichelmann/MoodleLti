using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MoodleLti.Extensions
{
    internal static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Returns the response body of the given HTTP response message.
        /// </summary>
        /// <param name="response">The HTTP response to read</param>
        /// <returns></returns>
        public static async Task<string> ReadBody(this HttpResponseMessage response)
        {
            // Read response body
            await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            return await streamReader.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}
