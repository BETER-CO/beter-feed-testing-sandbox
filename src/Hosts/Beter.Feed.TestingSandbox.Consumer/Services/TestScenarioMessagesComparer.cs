using Beter.Feed.TestingSandbox.Common.Serialization;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;

namespace Beter.Feed.TestingSandbox.Consumer.Services
{
    public class TestScenarioMessagesComparer
    {
        private static readonly CompareLogic Comparer = GetComparer();

        public static bool Compare<TValue>(TValue expected, TValue actual) where TValue : class
        {
            return Comparer.Compare(expected, actual).AreEqual;
        }

        private static CompareLogic GetComparer()
        {
            var compare = new CompareLogic();

            compare.Config.IgnoreProperty<SubscriptionsRemovedModel>(x => x.Ids);

            compare.Config.IgnoreProperty<IncidentModel>(x => x.Id);
            compare.Config.IgnoreProperty<IncidentModel>(x => x.Date);
            compare.Config.IgnoreProperty<IncidentModel>(x => x.Offset);

            compare.Config.IgnoreProperty<ScoreBoardModel>(x => x.Id);
            compare.Config.IgnoreProperty<ScoreBoardModel>(x => x.Timestamp);
            compare.Config.IgnoreProperty<ScoreBoardModel>(x => x.Offset);
            compare.Config.IgnoreProperty<TimerModel>(x => x.TimeStamp);

            compare.Config.IgnoreProperty<TradingInfoModel>(x => x.Id);
            compare.Config.IgnoreProperty<TradingInfoModel>(x => x.Timestamp);
            compare.Config.IgnoreProperty<TradingInfoModel>(x => x.Offset);
            compare.Config.CustomPropertyComparer<OutcomeModel>(
                x => x.Prices,
                new CustomComparer<Dictionary<string, object>, Dictionary<string, object>>(
                    (item1, item2) => JsonHubSerializer.Serialize(item1).Equals(JsonHubSerializer.Serialize(item2))));

            compare.Config.IgnoreProperty<TimeTableItemModel>(x => x.Id);
            compare.Config.IgnoreProperty<TimeTableItemModel>(x => x.Timestamp);
            compare.Config.IgnoreProperty<TimeTableItemModel>(x => x.StartDate);
            compare.Config.IgnoreProperty<TimeTableItemModel>(x => x.Offset);

            compare.Config.IgnoreProperty<GlobalMessageModel>(x => x.Id);

            return compare;
        }
    }
}
