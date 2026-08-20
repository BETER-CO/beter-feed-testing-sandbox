using System.Text.Json.Nodes;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Domain.TestScenarios
{
    /// <summary>
    /// Playback deserializes a message into a typed model and re-serializes it, so any field the
    /// model does not declare is silently dropped. These tests guard the fields against that.
    /// </summary>
    public class ContractFieldRoundTripTests
    {
        [Fact]
        public void Modify_PreservesPlayerProps_OnScoreboardRoundTrip()
        {
            // Arrange
            var scoreboard = JsonNode.Parse("""
                [{
                    "id": "019ff69f-eb7c-7f41-af31-7ed6ec182111",
                    "sportId": 3,
                    "stage": 10,
                    "scores": [],
                    "playerProps": [
                        {
                            "participantId": "019dd76b-5378-77af-bcc0-d61c73d68066",
                            "interval": 11,
                            "results": [{ "result": 19, "value": 4 }, { "result": 40, "value": 2 }]
                        }
                    ],
                    "timestamp": 1786567225460,
                    "msgType": 2,
                    "offset": 9168661
                }]
                """);

            var message = new TestScenarioMessage { Value = scoreboard };

            // Act
            message.Modify<IEnumerable<ScoreBoardModel>>(_ => { });

            // Assert
            var playerProps = message.Value.AsArray()[0]["playerProps"];

            Assert.NotNull(playerProps);
            Assert.Equal("019dd76b-5378-77af-bcc0-d61c73d68066", (string)playerProps[0]["participantId"]);
            Assert.Equal(11, (int)playerProps[0]["interval"]);
            Assert.Equal(19, (int)playerProps[0]["results"][0]["result"]);
            Assert.Equal(4, (int)playerProps[0]["results"][0]["value"]);
            Assert.Equal(40, (int)playerProps[0]["results"][1]["result"]);
            Assert.Equal(2, (int)playerProps[0]["results"][1]["value"]);
        }

        [Fact]
        public void Modify_PreservesParticipantStructure_IncludingNestedComposition()
        {
            // Arrange
            var timeTable = JsonNode.Parse("""
                [{
                    "sport": "CounterStrike",
                    "sportId": 3,
                    "startDate": "2026-08-12T15:38:00Z",
                    "status": 1,
                    "participants": [],
                    "participantStructure": [
                        {
                            "id": "019fa2f5-5238-7644-95d6-7c6c1bc91ace",
                            "participantType": 1,
                            "order": 1,
                            "name": "NewTeam1",
                            "attributes": {},
                            "composition": [
                                {
                                    "id": "019dd76b-5378-77af-bcc0-d61c73d68066",
                                    "participantType": 3,
                                    "order": 1,
                                    "name": "A0 ZywOo",
                                    "attributes": {},
                                    "composition": []
                                }
                            ]
                        }
                    ],
                    "timestamp": 1786549114987,
                    "willBeLive": true,
                    "willBePrematch": false,
                    "msgType": 1,
                    "offset": 9162224,
                    "id": "019ff69f-eb7c-7f41-af31-7ed6ec182111"
                }]
                """);

            var message = new TestScenarioMessage { Value = timeTable };

            // Act
            message.Modify<IEnumerable<TimeTableItemModel>>(_ => { });

            // Assert
            var structure = message.Value.AsArray()[0]["participantStructure"];

            Assert.NotNull(structure);
            Assert.Equal("019fa2f5-5238-7644-95d6-7c6c1bc91ace", (string)structure[0]["id"]);
            Assert.Equal(1, (int)structure[0]["participantType"]);
            Assert.Equal("NewTeam1", (string)structure[0]["name"]);

            var player = structure[0]["composition"][0];

            Assert.Equal("019dd76b-5378-77af-bcc0-d61c73d68066", (string)player["id"]);
            Assert.Equal(3, (int)player["participantType"]);
            Assert.Equal("A0 ZywOo", (string)player["name"]);
        }

        [Fact]
        public void Modify_PreservesExcludedParticipants_IncludingParentParticipantId()
        {
            // Arrange
            var timeTable = JsonNode.Parse("""
                [{
                    "sport": "CounterStrike",
                    "sportId": 3,
                    "startDate": "2026-08-12T15:38:00Z",
                    "status": 1,
                    "participants": [],
                    "participantStructure": [],
                    "excludedParticipants": [
                        {
                            "parentParticipantId": "019fa2f5-5238-7644-95d6-7c6c1bc91ace",
                            "order": 5,
                            "id": "019dd76b-5378-77af-bcc0-d61c73d68066",
                            "participantType": 3,
                            "name": "A0 ZywOo",
                            "attributes": {},
                            "composition": []
                        }
                    ],
                    "timestamp": 1786549114987,
                    "willBeLive": true,
                    "willBePrematch": false,
                    "msgType": 1,
                    "offset": 9162224,
                    "id": "019ff69f-eb7c-7f41-af31-7ed6ec182111"
                }]
                """);

            var message = new TestScenarioMessage { Value = timeTable };

            // Act
            message.Modify<IEnumerable<TimeTableItemModel>>(_ => { });

            // Assert
            var excluded = message.Value.AsArray()[0]["excludedParticipants"];

            Assert.NotNull(excluded);
            Assert.Equal("019fa2f5-5238-7644-95d6-7c6c1bc91ace", (string)excluded[0]["parentParticipantId"]);
            Assert.Equal("019dd76b-5378-77af-bcc0-d61c73d68066", (string)excluded[0]["id"]);
            Assert.Equal(3, (int)excluded[0]["participantType"]);
            Assert.Equal(5, (int)excluded[0]["order"]);
            Assert.Equal("A0 ZywOo", (string)excluded[0]["name"]);
        }
    }
}
