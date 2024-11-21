using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private static float m_original_default_speed = -1;
    private static bool m_is_sprinting = false;
    public static bool IsSprinting {
        get {
            return m_is_sprinting;
        }
        set {
            m_is_sprinting = value;
        }
    }
    
    private void Awake() {
        logger = this.Logger;
        try {
            this.m_plugin_info = PluginInfo.to_dict();
            Settings.Instance.load(this);
            DDPlugin.set_log_level(Settings.m_log_level.Value);
            this.create_nexus_page();
            Hotkeys.load();
            this.m_harmony.PatchAll();
            logger.LogInfo($"{PluginInfo.GUID} v{PluginInfo.VERSION} loaded.");
        } catch (Exception e) {
            logger.LogError("** Awake FATAL - " + e);
        }
    }

    [HarmonyPatch(typeof(LocalPlayerController), "Initialize")]
    class HarmonyPatch_LocalPlayerController_Initialize {
        private static void Postfix(LocalPlayerController __instance, PlayerInput ___m_input) {
            try {
                // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Actions.html
                ___m_input.actions.A
            } catch (Exception e) {
                logger.LogError("** HarmonyPatch_LocalPlayerController_Initialize.Prefix ERROR - " + e);
            }
        }
    }

    public class SprintChecker : MonoBehaviour {
        private void OnGUI() {
            try {
                if (!Settings.m_enabled.Value || !Event.current.isKey) {
                    return;
                }
                bool is_key_down = false;
                foreach (KeyCode key in Hotkeys.HotKeys[Hotkeys.HOTKEY_SPRINT]) {
                    if ((is_key_down = (Event.current.keyCode == key))) {
                        break;
                    }
                }
                if (Settings.m_hold_key_to_sprint.Value) {
                    IsSprinting = is_key_down;
                }
            } catch (Exception e) {
                logger.LogError("** SprintChecker.OnGUI ERROR - " + e);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerMovementSpeed), "Awake")]
    class HarmonyPatch_PlayerMovementSpeed_Awake {
        private static void Postfix(PlayerMovementSpeed __instance) {
            try {
                __instance.gameObject.AddComponent<SprintChecker>();
            } catch (Exception e) {
                logger.LogError("** HarmonyPatch_PlayerMovementSpeed_Awake.Prefix ERROR - " + e);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerMovementSpeed), "Update")]
    class HarmonyPatch_PlayerMovementSpeed_Update {
        private static bool m_last_sprint_value = true;
        private static bool Prefix(ref float ___m_defaultMovementSpeed) {
        	try {
                if (!Settings.m_enabled.Value || (Settings.m_speed_multiplier.Value <= 0 && Settings.m_sprint_speed_multiplier.Value <= 0)) {
                    return true;
                }
                if (m_original_default_speed == -1) {
                    m_original_default_speed = ___m_defaultMovementSpeed;
                }
                ___m_defaultMovementSpeed = m_original_default_speed * Settings.m_speed_multiplier.Value * (m_is_sprinting ? Settings.m_sprint_speed_multiplier.Value : 1f);
                if (IsSprinting != m_last_sprint_value) {
                    _debug_log($"[{Time.time}] movement_speed_multiplier: {Settings.m_speed_multiplier.Value}, is_sprinting: {IsSprinting}");
                    m_last_sprint_value = IsSprinting;
                }
                return true;
			} catch (Exception e) {
				logger.LogError("** HarmonyPatch_PlayerMovementSpeed_Update.Prefix ERROR - " + e);
			}
            return true;
		}
	}
}
