using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;

public static class PluginInfo {

    public const string TITLE = "Movement Speed";
    public const string NAME = "movement_speed";
    public const string SHORT_DESCRIPTION = "Adds a configurable multiplier to increase movement speed.";

    public const string VERSION = "0.0.1";

    public const string AUTHOR = "devopsdinosaur";
    public const string GAME_TITLE = "Luma Island";
    public const string GAME = "luma-island";
    public const string GUID = AUTHOR + "." + GAME + "." + NAME;
    public const string REPO = "luma-island-mods";

    public static Dictionary<string, string> to_dict() {
        Dictionary<string, string> info = new Dictionary<string, string>();
        foreach (FieldInfo field in typeof(PluginInfo).GetFields((BindingFlags) 0xFFFFFFF)) {
            info[field.Name.ToLower()] = (string) field.GetValue(null);
        }
        return info;
    }
}

[BepInPlugin(PluginInfo.GUID, PluginInfo.TITLE, PluginInfo.VERSION)]
public class MovementSpeedPlugin : DDPlugin {
    private Harmony m_harmony = new Harmony(PluginInfo.GUID);
    private static float m_prev_multiplier = -1;

    private void Awake() {
        logger = this.Logger;
        try {
            this.m_plugin_info = PluginInfo.to_dict();
            Settings.Instance.load(this);
            DDPlugin.set_log_level(Settings.m_log_level.Value);
            this.create_nexus_page();
            this.m_harmony.PatchAll();
            logger.LogInfo($"{PluginInfo.GUID} v{PluginInfo.VERSION} loaded.");
        } catch (Exception e) {
            logger.LogError("** Awake FATAL - " + e);
        }
    }

    [HarmonyPatch(typeof(PlayerMovementSpeed), "Update")]
    class HarmonyPatch_PlayerMovementSpeed_Update {
        private static bool Prefix(ref float ___m_defaultMovementSpeed) {
        	try {
				if (Settings.m_enabled.Value && Settings.m_speed_multiplier.Value > 0 && Settings.m_speed_multiplier.Value != m_prev_multiplier) {
                    ___m_defaultMovementSpeed *= (m_prev_multiplier = Settings.m_speed_multiplier.Value);
                }
                return true;
			} catch (Exception e) {
				logger.LogError("** HarmonyPatch_PlayerMovementSpeed_Update.Prefix ERROR - " + e);
			}
            return true;
		}
	}
}
