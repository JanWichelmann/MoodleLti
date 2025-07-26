using MoodleLti.Tests.Mocks;

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
