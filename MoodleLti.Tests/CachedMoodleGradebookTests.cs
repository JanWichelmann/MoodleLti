using MoodleLti.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoodleLti.Tests
{
    public class CachedMoodleGradebookTests : MoodleGradebookTests
    {
        protected override MoodleGradebook GetGradebook()
        {
            var mockMoodleLtiApi = new MockMoodleLtiApi();
            var options = Microsoft.Extensions.Options.Options.Create(new Options.MoodleLtiOptions());
            return new CachedMoodleGradebook(mockMoodleLtiApi, options);
        }
    }
}
