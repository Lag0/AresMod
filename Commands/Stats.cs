using System;
using System.Collections.Generic;
using AresMod.Utils;
using System.Linq;
using Wetstone.API;

namespace AresMod.Commands;

[Command("status, stats", Usage = "status / stats", Description = "Display your personal status.")]
public static class Stats
{
    private static Dictionary<ulong, int> GetTopKillList() => Database.pvpkills
        .OrderBy<KeyValuePair<ulong, int>, int>((Func<KeyValuePair<ulong, int>, int>)(x => x.Value))
        .Reverse<KeyValuePair<ulong, int>>().ToDictionary<KeyValuePair<ulong, int>, ulong, int>(
            (Func<KeyValuePair<ulong, int>, ulong>)(x => x.Key), 
            (Func<KeyValuePair<ulong, int>, int>)(x => x.Value));

    public static void Initialize(Context ctx)
    {
        var user = ctx.Event.User;
        var userEntity = ctx.Event.SenderUserEntity;
        var charEntity = ctx.Event.SenderCharacterEntity;
        var charName = user.CharacterName.ToString();
        var steamID = user.PlatformId;
        var a = (float)steamID;
        
        Database.pvpkills.TryGetValue(steamID, out var pvpKills);
        Database.pvpdeath.TryGetValue(steamID, out var pvpDeaths);
        Database.pvpkd.TryGetValue(steamID, out var pvpKd);

        user.SendSystemMessage($"-- <color=#FFFFFFFF>{charName}</color> --");
        user.SendSystemMessage($"K/D: <color=#FFFFFFFF>{pvpKd:F}</color>");
        user.SendSystemMessage($"Kills: <color=#75FF33FF>{pvpKills}</color>");
        user.SendSystemMessage($"Deaths: <color=#F00000FF>{pvpDeaths}</color>");
        user.SendSystemMessage($"You are No. <color=#FFFFFFFF>{(GetTopKillList().Keys.ToList<ulong>().IndexOf(ctx.Event.User.PlatformId)+ 1)}</color> in the leaderboard!");
    }
}