using AresMod.Utils;
using System.Linq;
using Wetstone.API;

namespace AresMod.Commands
{
    [Command("status, stats", Usage = "status / stats", Description = "Display your personal status.")]
    public static class Stats
    {
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;
            var userEntity = ctx.Event.SenderUserEntity;
            var charEntity = ctx.Event.SenderCharacterEntity;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;

            Database.pvpkills.TryGetValue(SteamID, out var pvp_kills);
            Database.pvpdeath.TryGetValue(SteamID, out var pvp_deaths);
            Database.pvpkd.TryGetValue(SteamID, out var pvp_kd);

            user.SendSystemMessage($"-- <color=#ffffffff>{CharName}</color> --");
            user.SendSystemMessage($"K/D: <color=#ffffffff>{pvp_kd.ToString("F")}</color>");
            user.SendSystemMessage($"Kills: <color=#75FF33FF>{pvp_kills}</color>");
            user.SendSystemMessage($"Deaths: <color=#F00000FF>{pvp_deaths}</color>");
            //user.SendSystemMessage($"You are No. <color=#ffff00>{Database.pvpkills.Keys.ToList().IndexOf(SteamID)}</color>");
        }
    }
}