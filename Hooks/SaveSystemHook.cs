using HarmonyLib;
using ProjectM;
using AresMod.Utils;

namespace AresMod.Hooks
{
    [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), nameof(TriggerPersistenceSaveSystem.TriggerSave))]
    public class TriggerPersistenceSaveSystem_Patch
    {
        public static void Prefix()
        {
            AutoSaveSystem.SaveDatabase();
        }
    }
}
