﻿using ProjectM;
using ProjectM.Network;
using AresMod.Utils;
using Wetstone.API;

namespace AresMod.Commands
{
    [Command("kick", Usage = "kick <playername>", Description = "Kick the specified player out of the server.", ReqPermission = 100)]
    public static class Kick
    {
        public static void Initialize(Context ctx)
        {
            var args = ctx.Args;
            if (args.Length < 1)
            {
                Output.MissingArguments(ctx);
                return;
            }

            var name = args[0];
            if (Helper.FindPlayer(name, true, out _, out var targetUserEntity))
            {
                Helper.KickPlayer(targetUserEntity);
                ctx.Event.User.SendSystemMessage($"Player \"{name}\" has been kicked from server.");
            }
            else
            {
                Output.CustomErrorMessage(ctx, "Specified player not found.");
            }
        }
    }
}
