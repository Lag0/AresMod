using System;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace AresMod.Hooks;

[HarmonyPatch(typeof(RespawnCharacterSystem), "OnUpdate")]
public class RespawnCharacterSystem_Patch
{
    [HarmonyPostfix]
    public static void Postfix(RespawnCharacterSystem __instance)
    {
        var entityManager = __instance.EntityManager;
        NativeArray<Entity> entityArray = __instance._Query.ToEntityArray(Allocator.Temp);
        foreach (Entity entity in entityArray)
        {
            try
            {
                var userEntity = entityManager.GetComponentData<PlayerCharacter>(entity).UserEntity;
                var user = entityManager.GetComponentData<User>(userEntity._Entity);
                // do things
            }
            catch (Exception e)
            {
                //
            }
        }
    }
}