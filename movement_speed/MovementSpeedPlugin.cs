using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public static class PluginInfo {

    public const string TITLE = "Movement Speed";
    public const string NAME = "movement_speed";
    public const string SHORT_DESCRIPTION = "Adds a configurable Sprint key (default: left shift) and configurable multipliers to increase movement speed when walking and sprinting!";

    public const string VERSION = "0.0.2";

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

    [HarmonyPatch(typeof(LocalPlayerController), "Initialize")]
    class HarmonyPatch_LocalPlayerController_Initialize {
        private static void Postfix(LocalPlayerController __instance, PlayerInput ___m_input) {
            try {
                List<string> available_paths = new List<string>();
                foreach (InputDevice device in InputSystem.devices) {
                    foreach (InputControl control in device.allControls) {
                        available_paths.Add(control.path);
                    }
                }
                _debug_log($"\n\n-- All Currently Available Input Control Paths on this System --\n\n{String.Join("\n", available_paths)}\n");
                InputAction action = new InputAction("sprint");
                foreach (string path in Settings.m_hotkey_sprint.Value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)) {
                    action.AddBinding(path);
                }
                action.started += context => {
                    //_debug_log($"[{context.time}] Sprint STARTED.");
                    if (Settings.m_hold_key_to_sprint.Value) {
                        m_is_sprinting = true;
                    }
                };
                action.canceled += context => {
                    //_debug_log($"[{context.time}] Sprint CANCELED.");
                    if (Settings.m_hold_key_to_sprint.Value) {
                        m_is_sprinting = false;
                    } else {
                        m_is_sprinting = !m_is_sprinting;
                    }
                };
                action.Enable();
                ___m_input.actions.AddItem(action);
            } catch (Exception e) {
                logger.LogError("** HarmonyPatch_LocalPlayerController_Initialize.Prefix ERROR - " + e);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerMovementSpeed), "Update")]
    class HarmonyPatch_PlayerMovementSpeed_Update {
        private static bool Prefix(ref float ___m_defaultMovementSpeed) {
        	try {
                if (!Settings.m_enabled.Value || (Settings.m_speed_multiplier.Value <= 0 && Settings.m_sprint_speed_multiplier.Value <= 0)) {
                    return true;
                }
                if (m_original_default_speed == -1) {
                    m_original_default_speed = ___m_defaultMovementSpeed;
                }
                ___m_defaultMovementSpeed = m_original_default_speed * Settings.m_speed_multiplier.Value * (m_is_sprinting ? Settings.m_sprint_speed_multiplier.Value : 1f);
                return true;
			} catch (Exception e) {
				logger.LogError("** HarmonyPatch_PlayerMovementSpeed_Update.Prefix ERROR - " + e);
			}
            return true;
		}
	}
}
