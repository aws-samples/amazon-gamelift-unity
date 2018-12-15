using Aws.GameLift.Server.Model;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Aws.GameLift.Server.Model
{
    public class MatchmakerData
    {
        public string MatchId { get; set; }
        public string MatchmakingConfigurationArn { get; set; }
        public IList<Player> Players = new List<Player>();

        // Match fields
        private const string FIELD_MATCH_ID = "matchId";
        private const string FIELD_MATCHMAKING_CONFIG_ARN = "matchmakingConfigurationArn";
        private const string FIELD_TEAMS = "teams";

        // Team fields
        private const string FIELD_NAME = "name";
        private const string FIELD_PLAYERS = "players";

        // Player fields
        private const string FIELD_PLAYER_ID = "playerId";
        private const string FIELD_ATTRIBUTES = "attributes";
        private const string FIELD_LATENCY = "attributes";

        // Attribute fields
        private const string FIELD_ATTRIBUTE_TYPE = "attributeType";
        private const string FIELD_ATTRIBUTE_VALUE = "valueAttribute";

        public static MatchmakerData FromJson(string json)
        {
            JObject obj = JObject.Parse(json);

            MatchmakerData matchmakerData = new MatchmakerData();
            matchmakerData.MatchId = (string)obj[FIELD_MATCH_ID];
            matchmakerData.MatchmakingConfigurationArn = (string)obj[FIELD_MATCHMAKING_CONFIG_ARN];

            JArray parsedTeams = (JArray)obj[FIELD_TEAMS];
            foreach (var parsedTeam in parsedTeams.Children())
            {
                matchmakerData.parseOutPlayersFromTeam((JObject)parsedTeam);
            }

            return matchmakerData;
        }

        public void parseOutPlayersFromTeam(JObject parsedTeam)
        {
            string teamName = (string)parsedTeam[FIELD_NAME];

            JArray parsedPlayers = (JArray)parsedTeam[FIELD_PLAYERS];
            foreach (var parsedPlayer in parsedPlayers.Children())
            {
                Players.Add(PlayerFromJson(teamName, (JObject)parsedPlayer));
            }
        }

        private static Player PlayerFromJson(string teamName, JObject obj)
        {
            Player player = new Player();
            player.Team = teamName;
            player.PlayerId = (string)obj[FIELD_PLAYER_ID];
            player.PlayerAttributes = ParsePlayerAttributes((JObject)obj[FIELD_ATTRIBUTES]);
            player.LatencyInMS = ParseLatency((JObject)obj[FIELD_LATENCY]);

            return player;
        }

        private static Dictionary<string, AttributeValue> ParsePlayerAttributes(JObject parsedAttrs)
        {
            if (parsedAttrs == null)
            {
                return null;
            }

            Dictionary<string, AttributeValue> attrs = new Dictionary<string, AttributeValue>();
            foreach (var oneAttr in parsedAttrs)
            {
                AttributeValue val = ParseOneAttribute((JObject)oneAttr.Value);
                if (val != null)
                {
                    attrs.Add(oneAttr.Key, val);
                }
            }

            return attrs;
        }

        private static AttributeValue ParseOneAttribute(JObject parsedAttr)
        {
            string attributeType = (string)parsedAttr[FIELD_ATTRIBUTE_TYPE];
            switch (attributeType)
            {
                case "DOUBLE":
                    return new AttributeValue((double)parsedAttr[FIELD_ATTRIBUTE_VALUE]);

                case "STRING":
                    return new AttributeValue((string)parsedAttr[FIELD_ATTRIBUTE_VALUE]);

                case "STRING_DOUBLE_MAP":
                    {
                        Dictionary<string, double> values = new Dictionary<string, double>();
                        foreach (var onePair in (JObject)parsedAttr[FIELD_ATTRIBUTE_VALUE])
                        {
                            values.Add(onePair.Key, (double)onePair.Value);
                        }

                        return new AttributeValue(values);
                    }

                case "STRING_LIST":
                    {
                        JArray listValues = (JArray)parsedAttr[FIELD_ATTRIBUTE_VALUE];
                        string[] values = new string[listValues.Count];

                        int x = 0;
                        foreach (var val in listValues.Children())
                        {
                            values[x] = (string)val;
                            x++;
                        }

                        return new AttributeValue(values);
                    }
            }

            return null;
        }

        private static Dictionary<string, int> ParseLatency(JObject parsedLatency)
        {
            if (parsedLatency == null)
            {
                return null;
            }

            Dictionary<string, int> latency = new Dictionary<string, int>();
            // TODO: We currently don't include latency measurements in the matchmaker data.
            //       If we decide we want to we would need to add the parsing for that here.
            //
            // Reasons why we might want to avoid including it:
            //     (1) data could easily be out of date
            //     (2) the game server has been communicating regularly with all the players,
            //         so it's in a better position to know what the current latency is
            //     (3) including latency bulks up the size of the matchmaker data, which
            //         effectively reduces the maximum match size that can be supported
            //         before exceeding the maximum supported match size.

            return latency;
        }
    }
}
