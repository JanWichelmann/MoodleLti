using MoodleLti.Tests.Mocks;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MoodleLti.Tests
{
    public class MoodleGradebookTests
    {
        protected virtual MoodleGradebook GetGradebook()
        {
            var mockMoodleLtiApi = new MockMoodleLtiApi();
            var options = Microsoft.Extensions.Options.Options.Create(new Options.MoodleLtiOptions());
            return new MoodleGradebook(mockMoodleLtiApi, options);
        }

        [Fact]
        public async Task ColumnManagementAsync()
        {
            var gradebook = GetGradebook();

            // Gradebook is empty
            Assert.Empty(await gradebook.GetColumnsAsync());

            // Test sanity checks
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await gradebook.CreateColumnAsync(null, 1));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await gradebook.CreateColumnAsync("col", -10));

            // Create column 1
            string col1Title = "col1";
            float col1MaxScore = 100;
            string col1Tag = null;
            var col1 = await gradebook.CreateColumnAsync(col1Title, col1MaxScore, col1Tag);
            Assert.NotNull(col1);
            Assert.Equal(col1Title, col1.Title);
            Assert.Equal(col1MaxScore, col1.MaximumScore);
            Assert.Equal(col1Tag, col1.Tag);

            // Check whether retrieval of column 1 works
            var col1Retrieved = await gradebook.GetColumnAsync(col1.Id);
            Assert.NotNull(col1Retrieved);
            Assert.Equal(col1Title, col1Retrieved.Title);
            Assert.Equal(col1MaxScore, col1Retrieved.MaximumScore);
            Assert.Equal(col1Tag, col1Retrieved.Tag);

            // Create column 2
            string col2Title = "";
            float col2MaxScore = 0;
            string col2Tag = "tag2";
            var col2 = await gradebook.CreateColumnAsync(col2Title, col2MaxScore, col2Tag);
            Assert.NotNull(col2);
            Assert.Equal(col2Title, col2.Title);
            Assert.Equal(col2MaxScore, col2.MaximumScore);
            Assert.Equal(col2Tag, col2.Tag);

            // Check whether retrieval of column 2 works
            var col2Retrieved = await gradebook.GetColumnAsync(col2.Id);
            Assert.NotNull(col2Retrieved);
            Assert.Equal(col2Title, col2Retrieved.Title);
            Assert.Equal(col2MaxScore, col2Retrieved.MaximumScore);
            Assert.Equal(col2Tag, col2Retrieved.Tag);

            // Check column count
            Assert.Equal(2, (await gradebook.GetColumnsAsync()).Count());

            // Delete column and check column count again
            await gradebook.DeleteColumnAsync(col1.Id);
            Assert.Single(await gradebook.GetColumnsAsync());

            // Column 1 should not be retrievable
            await Assert.ThrowsAsync<MoodleLtiException>(async () => await gradebook.GetColumnAsync(col1.Id));
        }
    }
}
