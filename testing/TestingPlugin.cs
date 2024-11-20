using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public static class PluginInfo {

    public const string TITLE = "Testing";
    public const string NAME = "testing";
    public const string SHORT_DESCRIPTION = "For testing purposes only";

    public const string VERSION = "0.0.0";

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
public class TestingPlugin : DDPlugin {
    private Harmony m_harmony = new Harmony(PluginInfo.GUID);

    private void Awake() {
        logger = this.Logger;
        try {
            this.m_plugin_info = PluginInfo.to_dict();
            Settings.Instance.load(this);
            DDPlugin.set_log_level(LogLevel.Debug);
            this.create_nexus_page();
            this.m_harmony.PatchAll();
            logger.LogInfo($"{PluginInfo.GUID} v{PluginInfo.VERSION} loaded.");
        } catch (Exception e) {
            logger.LogError("** Awake FATAL - " + e);
        }
    }

	/*
	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static bool Prefix() {
			
			return true;
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static void Postfix() {
			
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static bool Prefix() {
			try {

				return false;
			} catch (Exception e) {
				logger.LogError("** XXXXX.Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static void Postfix() {
			try {
				
			} catch (Exception e) {
				logger.LogError("** XXXXX.Postfix ERROR - " + e);
			}
		}
	}
	*/
}
