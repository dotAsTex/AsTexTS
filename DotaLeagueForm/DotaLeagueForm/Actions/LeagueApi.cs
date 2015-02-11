using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using SteamKit2.GC.Dota.Internal;
using System.IO;

namespace DotaLeagueForm.Actions
{
    public class MatchResult
    {
        public ulong MatchID { get; set; }
        public int TeamWin { get; set; }
        public List<Data> players { get; set; }
    }
    public static class LeagueApi
    {
        public static void SendMatchResult(MatchResult matchResult)
        {

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(matchResult);
            File.WriteAllText(matchResult.MatchID.ToString(), json);
            Console.WriteLine(json);
        }

        public static List<ulong> GetPlayersID(int matchID)
        {
            return new List<ulong>();
        }

        public static void SendLobbyState(int state, string password)
        {

        }
    }
}
