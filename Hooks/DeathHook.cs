using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using AresMod.Commands;
using AresMod.Systems;
using AresMod.Utils;
using Unity.Collections;
using Unity.Entities;

namespace AresMod.Hooks;
[HarmonyPatch]
public class DeathEventListenerSystem_Patch
{
    [HarmonyPatch(typeof(DeathEventListenerSystem), "OnUpdate")]
    [HarmonyPostfix]
    public static void Postfix(DeathEventListenerSystem __instance)
    {
        if (__instance._DeathEventQuery != null)
        {
            NativeArray<DeathEvent> deathEvents = __instance._DeathEventQuery.ToComponentDataArray<DeathEvent>(Allocator.Temp);
            foreach (DeathEvent ev in deathEvents)
            {
                

                //-- Auto Respawn & HunterHunted System Begin
                if (__instance.EntityManager.HasComponent<PlayerCharacter>(ev.Died))
                {
                    PlayerCharacter player = __instance.EntityManager.GetComponentData<PlayerCharacter>(ev.Died);
                    Entity userEntity = player.UserEntity._Entity;
                    User user = __instance.EntityManager.GetComponentData<User>(userEntity);
                    ulong SteamID = user.PlatformId;

                    

                    //-- Check for AutoRespawn
                    if (user.IsConnected)
                    {
                        bool isServerWide = Database.autoRespawn.TryGetValue(1, out bool value);
                        bool doRespawn;
                        if (!isServerWide)
                        {
                            doRespawn = Database.autoRespawn.TryGetValue(SteamID, out bool value_);
                        }
                        else { doRespawn = true; }

                        if (doRespawn)
                        {
                            Utils.RespawnCharacter.Respawn(ev.Died, player, userEntity);
                        }
                    }
                    //-- ---------------------
                }
                //-- ----------------------------------------
            }
        }
    }
}