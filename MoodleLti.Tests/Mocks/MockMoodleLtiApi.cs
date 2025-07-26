using MoodleLti.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoodleLti.Tests.Mocks
{
    /// <summary>
    /// Simulates the behavior of the remote server.
    /// </summary>
    class MockMoodleLtiApi : IMoodleLtiApi
    {
        private int _nextLineItemId = 1;
        private Dictionary<int, LineItemData> _lineItems = new Dictionary<int, LineItemData>();

        public Task<int> CreateLineItemAsync(MoodleLtiLineItem lineItem)
        {
            LineItemData lineItemData = new LineItemData
            {
                LineItem = new MoodleLtiLineItem
                {
                    Id = _nextLineItemId++,
                    Label = lineItem.Label,
                    LtiLinkId = lineItem.LtiLinkId,
                    ResourceId = lineItem.ResourceId,
                    ResourceLinkId = lineItem.ResourceLinkId,
                    ScoreMaximum = lineItem.ScoreMaximum,
                    Tag = lineItem.Tag
                },
                Scores = new Dictionary<string, MoodleLtiScore>()
            };
            _lineItems.Add(lineItemData.LineItem.Id, lineItemData);
            return Task.FromResult(lineItemData.LineItem.Id);
        }

        public Task DeleteLineItemAsync(int id)
        {
            if(_lineItems.ContainsKey(id))
                _lineItems.Remove(id);
            return Task.CompletedTask;
        }

        public Task<MoodleLtiLineItem> GetLineItemAsync(int id)
        {
            if(_lineItems.ContainsKey(id))
                return Task.FromResult(_lineItems[id].LineItem.Clone());
            throw new MoodleLtiException(nameof(GetLineItemAsync));
        }

        public Task<List<MoodleLtiLineItem>> GetLineItemsAsync()
        {
            var lineItemList = new List<MoodleLtiLineItem>();
            foreach(var lid in _lineItems)
                lineItemList.Add(lid.Value.LineItem.Clone());
            return Task.FromResult(lineItemList);
        }

        public Task UpdateLineItemAsync(MoodleLtiLineItem lineItem)
        {
            if(!_lineItems.ContainsKey(lineItem.Id))
                throw new MoodleLtiException(nameof(UpdateLineItemAsync));
            var lid = _lineItems[lineItem.Id];
            lid.LineItem = lineItem.Clone();
            return Task.CompletedTask;
        }

        public Task UpdateScoreAsync(int lineItemId, MoodleLtiScore score)
        {
            if(!_lineItems.ContainsKey(lineItemId))
                throw new MoodleLtiException(nameof(UpdateScoreAsync));
            var lid = _lineItems[lineItemId];

            if(!lid.Scores.TryGetValue(score.UserId, out var s))
                s = new MoodleLtiScore();
            s.ActivityProgress = score.ActivityProgress;
            s.Comment = score.Comment;
            s.GradingProgress = score.GradingProgress;
            s.ScoreGiven = score.ScoreGiven;
            s.ScoreMaximum = score.ScoreMaximum;
            s.Timestamp = score.Timestamp;

            return Task.CompletedTask;
        }

        private class LineItemData
        {
            public MoodleLtiLineItem LineItem { get; set; }
            public Dictionary<string, MoodleLtiScore> Scores { get; set; }
        }
    }
}
