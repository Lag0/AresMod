﻿using ProjectM;
using ProjectM.Network;
using AresMod.Utils;
using Unity.Collections;
using Unity.Entities;
using Wetstone.API;

namespace AresMod.Commands
{
    [Command("ready, r", Usage = "ready [<Player Name>]", Description = "Set your status ready for pvp")]
    public static class ResetCooldown
    {
        public static bool status_ready = true;
        public static void Initialize(Context ctx)
        {
            Entity PlayerCharacter = ctx.Event.SenderCharacterEntity;
            string CharName = ctx.Event.User.CharacterName.ToString();
            EntityManager entityManager = VWorld.Server.EntityManager;
            var component = ctx.EntityManager.GetComponentData<ProjectM.Health>(ctx.Event.SenderCharacterEntity);
            var UserIndex = ctx.Event.User.Index;
            int Value = 100;

            if (ctx.Args.Length >= 1)
            {
                string name = string.Join(' ', ctx.Args);
                if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                {
                    PlayerCharacter = targetEntity;
                    CharName = name;
                }
                else
                {
                    Utils.Output.CustomErrorMessage(ctx, $"Could not find the specified player \"{name}\".");
                    return;
                }
            }

            var AbilityBuffer = entityManager.GetBuffer<AbilityGroupSlotBuffer>(PlayerCharacter);
            for (int i = 0; i < AbilityBuffer.Length; i++)
            {
                var AbilitySlot = AbilityBuffer[i].GroupSlotEntity._Entity;
                var ActiveAbility = entityManager.GetComponentData<AbilityGroupSlot>(AbilitySlot);
                var ActiveAbility_Entity = ActiveAbility.StateEntity._Entity;

                var b = Helper.GetPrefabGUID(ActiveAbility_Entity);
                if (b.GuidHash == 0) continue;

                var AbilityStateBuffer = entityManager.GetBuffer<AbilityStateBuffer>(ActiveAbility_Entity);
                for (int c_i = 0; c_i < AbilityStateBuffer.Length; c_i++)
                {
                    var abilityState = AbilityStateBuffer[c_i].StateEntity._Entity;
                    var abilityCooldownState = entityManager.GetComponentData<AbilityCooldownState>(abilityState);
                    abilityCooldownState.CooldownEndTime = 0;
                    entityManager.SetComponentData(abilityState, abilityCooldownState);
                }
            float restore_hp = ((component.MaxHealth / 100) * Value) - component.Value;
            var HealthEvent = new ChangeHealthDebugEvent()
            {
                Amount = (int)restore_hp
            };
            VWorld.Server.GetExistingSystem<DebugEventsSystem>().ChangeHealthEvent(UserIndex, ref HealthEvent);
            }
            if (status_ready) ServerChatUtils.SendSystemMessageToAllClients(entityManager, $"Player \"{CharName}\" is ready!.");
        }
    }
}