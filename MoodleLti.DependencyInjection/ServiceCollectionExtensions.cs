using Microsoft.Extensions.DependencyInjection.Extensions;
using MoodleLti;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a Moodle LTI API service. This services requires presence of <see cref="System.Net.Http.IHttpClientFactory"/> and <see cref="Options.IOptions{MoodleLtiOptions}"/>.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <returns></returns>
        public static IServiceCollection AddMoodleLtiApi(this IServiceCollection services)
        {
            // Add LTI provider
            return services.AddTransient<IMoodleLtiApi, MoodleLtiApi>();
        }

        /// <summary>
        /// Adds a Moodle gradebook service. This services requires presence of <see cref="IMoodleLtiApi"/>, <see cref="System.Net.Http.IHttpClientFactory"/> and <see cref="Options.IOptions{MoodleLtiOptions}"/>.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <returns></returns>
        public static IServiceCollection AddMoodleGradebook(this IServiceCollection services)
        {
            // Add service
            return services.AddTransient<IMoodleGradebook, MoodleGradebook>();
        }

        /// <summary>
        /// Adds a cached Moodle gradebook service. This services requires presence of <see cref="IMoodleLtiApi"/>, <see cref="System.Net.Http.IHttpClientFactory"/> and <see cref="Options.IOptions{MoodleLtiOptions}"/>.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <returns></returns>
        public static IServiceCollection AddCachedMoodleGradebook(this IServiceCollection services)
        {
            // Add service
            return services.AddSingleton<IMoodleGradebook, CachedMoodleGradebook>();
        }
    }
}
