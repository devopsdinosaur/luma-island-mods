using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public static class PluginInfo {

    public const string TITLE = "Pickup Radius";
    public const string NAME = "pickup_radius";
    public const string SHORT_DESCRIPTION = "Adds the ability to adjust the player's magnetic pickup radius using a configurable setting.";

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
public class PickupRadiusPlugin : DDPlugin {
    private Harmony m_harmony = new Harmony(PluginInfo.GUID);

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

    [HarmonyPatch(typeof(PlayerMagneticLootCollector), "PerformLootCheck")]
	class HarmonyPatch_PlayerMagneticLootCollector_PerformLootCheck {
		private static bool Prefix(PlayerMagneticLootCollector __instance, int ___m_lootLayer) {
			try {
                if (!Settings.m_enabled.Value) {
                    return true;
                }
                Collider[] array = Physics.OverlapSphere(__instance.transform.position, Settings.m_pickup_radius.Value, ___m_lootLayer);
		        for (int i = 0; i < array.Length; i++) {
			        MagneticLoot component = array[i].gameObject.GetComponent<MagneticLoot>();
			        if (component != null && component.IsPickupable && component.AvailableToPickup) {
				        component.OnPlayerNearby(__instance.Player);
			        }
		        }
                return false;
            } catch (Exception e) {
                _error_log("** HarmonyPatch_PlayerMagneticLootCollector_PerformLootCheck ERROR - " + e);
            }
			return true;
		}
	}
}
