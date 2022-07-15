using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using AresMod.Commands;
using AresMod.Systems;
using AresMod.Utils;
using System.IO;
using System.Reflection;
using Wetstone.API;
using Wetstone.Hooks;

namespace AresMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("xyz.molenzwiebel.wetstone")]
    [Reloadable]
    public class Plugin : BasePlugin, IRunOnInitialized
    {
        private Harmony harmony;

        private CommandHandler cmd;
        private ConfigEntry<string> Prefix;
        private ConfigEntry<string> DisabledCommands;
        private ConfigEntry<float> DelayedCommands;
        private ConfigEntry<int> WaypointLimit;

        private ConfigEntry<bool> EnableVIPSystem;
        private ConfigEntry<bool> EnableVIPWhitelist;
        private ConfigEntry<int> VIP_Permission;

        private ConfigEntry<double> VIP_InCombat_ResYield;
        private ConfigEntry<double> VIP_InCombat_DurabilityLoss;
        private ConfigEntry<double> VIP_InCombat_MoveSpeed;
        private ConfigEntry<double> VIP_InCombat_GarlicResistance;
        private ConfigEntry<double> VIP_InCombat_SilverResistance;

        private ConfigEntry<double> VIP_OutCombat_ResYield;
        private ConfigEntry<double> VIP_OutCombat_DurabilityLoss;
        private ConfigEntry<double> VIP_OutCombat_MoveSpeed;
        private ConfigEntry<double> VIP_OutCombat_GarlicResistance;
        private ConfigEntry<double> VIP_OutCombat_SilverResistance;

        private ConfigEntry<bool> AnnouncePvPKills;
        private ConfigEntry<bool> EnablePvPLadder;
        private ConfigEntry<bool> EnablePvPToggle;

        public static ManualLogSource Logger;

        private void InitConfig()
        {
            Prefix = Config.Bind("Config", "Prefix", ".", "The prefix used for chat commands.");
            DelayedCommands = Config.Bind("Config", "Command Delay", 5f, "The number of seconds user need to wait out before sending another command.\nAdmin will always bypass this.");
            DisabledCommands = Config.Bind("Config", "Disabled Commands", "", "Enter command names to disable them, abbreviation are included automatically. Seperated by commas.\nEx.: save,godmode");
            WaypointLimit = Config.Bind("Config", "Waypoint Limit", 3, "Set a waypoint limit per user.");

            EnableVIPSystem = Config.Bind("VIP", "Enable VIP System", false, "Enable the VIP System.");
            EnableVIPWhitelist = Config.Bind("VIP", "Enable VIP Whitelist", false, "Enable the VIP user to ignore server capacity limit.");
            VIP_Permission = Config.Bind("VIP", "Minimum VIP Permission", 10, "The minimum permission level required for the user to be considered as VIP.");

            VIP_InCombat_DurabilityLoss = Config.Bind("VIP.InCombat", "Durability Loss Multiplier", 0.5, "Multiply durability loss when user is in combat. -1.0 to disable.\nDoes not affect durability loss on death.");
            VIP_InCombat_GarlicResistance = Config.Bind("VIP.InCombat", "Garlic Resistance Multiplier", -1.0, "Multiply garlic resistance when user is in combat. -1.0 to disable.");
            VIP_InCombat_SilverResistance = Config.Bind("VIP.InCombat", "Silver Resistance Multiplier", -1.0, "Multiply silver resistance when user is in combat. -1.0 to disable.");
            VIP_InCombat_MoveSpeed = Config.Bind("VIP.InCombat", "Move Speed Multiplier", -1.0, "Multiply move speed when user is in combat. -1.0 to disable.");
            VIP_InCombat_ResYield = Config.Bind("VIP.InCombat", "Resource Yield Multiplier", 2.0, "Multiply resource yield (not item drop) when user is in combat. -1.0 to disable.");

            VIP_OutCombat_DurabilityLoss = Config.Bind("VIP.OutCombat", "Durability Loss Multiplier", 0.5, "Multiply durability loss when user is out of combat. -1.0 to disable.\nDoes not affect durability loss on death.");
            VIP_OutCombat_GarlicResistance = Config.Bind("VIP.OutCombat", "Garlic Resistance Multiplier", 2.0, "Multiply garlic resistance when user is out of combat. -1.0 to disable.");
            VIP_OutCombat_SilverResistance = Config.Bind("VIP.OutCombat", "Silver Resistance Multiplier", 2.0, "Multiply silver resistance when user is out of combat. -1.0 to disable.");
            VIP_OutCombat_MoveSpeed = Config.Bind("VIP.OutCombat", "Move Speed Multiplier", 1.25, "Multiply move speed when user is out of combat. -1.0 to disable.");
            VIP_OutCombat_ResYield = Config.Bind("VIP.OutCombat", "Resource Yield Multiplier", 2.0, "Multiply resource yield (not item drop) when user is out of combat. -1.0 to disable.");

            AnnouncePvPKills = Config.Bind("PvP", "Announce PvP Kills", true, "Do I really need to explain this...?");
            EnablePvPLadder = Config.Bind("PvP", "Enable PvP Ladder", true, "Enables the PvP Ladder in the PvP command.");
            EnablePvPToggle = Config.Bind("PvP", "Enable PvP Toggle", true, "Enable/disable the pvp toggle feature in the pvp command.");

            if (!Directory.Exists("BepInEx/config/AresMod")) Directory.CreateDirectory("BepInEx/config/AresMod");
            if (!Directory.Exists("BepInEx/config/AresMod/Saves")) Directory.CreateDirectory("BepInEx/config/AresMod/Saves");

            if (File.Exists("BepInEx/config/AresMod/kits.json")) return;
            var stream = File.Create("BepInEx/config/AresMod/kits.json");
            stream.Dispose();
        }
        public override void Load()
        {
            InitConfig();
            Logger = Log;
            cmd = new CommandHandler(Prefix.Value, DisabledCommands.Value);
            Chat.OnChatMessage += HandleChatMessage;
            harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
        public override bool Unload()
        {
            AutoSaveSystem.SaveDatabase();
            Config.Clear();
            Chat.OnChatMessage -= HandleChatMessage;
            harmony.UnpatchSelf();
            return true;
        }
        public void OnGameInitialized()
        {
            //-- Commands Related
            AutoSaveSystem.LoadDatabase();

            //-- Apply configs
            CommandHandler.delay_Cooldown = DelayedCommands.Value;
            Waypoint.WaypointLimit = WaypointLimit.Value;

            PermissionSystem.isVIPSystem = EnableVIPSystem.Value;
            PermissionSystem.isVIPWhitelist = EnableVIPWhitelist.Value;
            PermissionSystem.VIP_Permission = VIP_Permission.Value;

            PermissionSystem.VIP_InCombat_ResYield = VIP_InCombat_ResYield.Value;
            PermissionSystem.VIP_InCombat_DurabilityLoss = VIP_InCombat_DurabilityLoss.Value;
            PermissionSystem.VIP_InCombat_MoveSpeed = VIP_InCombat_MoveSpeed.Value;
            PermissionSystem.VIP_InCombat_GarlicResistance = VIP_InCombat_GarlicResistance.Value;
            PermissionSystem.VIP_InCombat_SilverResistance = VIP_InCombat_SilverResistance.Value;
            
            PermissionSystem.VIP_OutCombat_ResYield = VIP_OutCombat_ResYield.Value;
            PermissionSystem.VIP_OutCombat_DurabilityLoss = VIP_OutCombat_DurabilityLoss.Value;
            PermissionSystem.VIP_OutCombat_MoveSpeed = VIP_OutCombat_MoveSpeed.Value;
            PermissionSystem.VIP_OutCombat_GarlicResistance = VIP_OutCombat_GarlicResistance.Value;
            PermissionSystem.VIP_OutCombat_SilverResistance = VIP_OutCombat_SilverResistance.Value;
            
            PvPSystem.isLadderEnabled = EnablePvPLadder.Value;
            PvPSystem.isPvPToggleEnabled = EnablePvPToggle.Value;
            PvPSystem.announce_kills = AnnouncePvPKills.Value;
        }

        private void HandleChatMessage(VChatEvent ev)
        {
            cmd.HandleCommands(ev, Log, Config);
        }
    }
}
