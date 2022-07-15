using AresMod.Systems;
using AresMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Wetstone.API;

namespace AresMod.Commands
{
    [Command("leaderboard, lb, pvp", Usage = "leaderboard / lb / pvp", Description = "Show the current leaderboard ranking.", ReqPermission = 0)]
    public static class Leaderboard
    {
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;

            List<KeyValuePair<ulong, int>> list1 = Database.pvpkills.ToList<KeyValuePair<ulong, int>>();
            list1.Sort((Comparison<KeyValuePair<ulong, int>>)((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)));
            var kills = list1.Take<KeyValuePair<ulong, int>>(10);
            List<KeyValuePair<ulong, double>> list2 = Database.pvpkd.ToList<KeyValuePair<ulong, double>>();
            list2.Sort((Comparison<KeyValuePair<ulong, double>>)((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)));
            var kd = list2.Take<KeyValuePair<ulong, double>>(1);
            
            if (ctx.Args.Length == 0)
            {
                if (PvPSystem.isLadderEnabled)
                {
                    user.SendSystemMessage("==========<color=#ffffffff>TOP PvP Players</color>==========");
                    int num4 = 0;
                    foreach (KeyValuePair<ulong, int> keyValuePair in kills)
                    {
                        foreach (KeyValuePair<ulong, double> keyValuePair2 in kd)
                        {
                            num4++;
                            user.SendSystemMessage(($"{num4}. <color=#ffffffff>{(object)Helper.GetNameFromSteamID(keyValuePair.Key)}:</color> <color=#75FF33FF>{keyValuePair.Value}</color> Kills・<color=#FFFFFFFF>{keyValuePair2.Value:0.0}</color> KDA"));
                        }
                    }
                    if (num4 == 0) user.SendSystemMessage("<color=#ffffffff>No result.</color>");
                    user.SendSystemMessage("===================================");
                }
            }
        }
    }
}
