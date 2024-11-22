using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public static class PluginInfo {

    public const string TITLE = "Time Management";
    public const string NAME = "time_management";
    public const string SHORT_DESCRIPTION = "Slow down, speed up, stop, and even reverse time using configurable hotkeys.";

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
public class TimeManagementPlugin : DDPlugin {
    private Harmony m_harmony = new Harmony(PluginInfo.GUID);
    private static TextMeshProUGUI m_time_scale_text;

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

    public static InputAction[] add_hotkey_action(PlayerInput player_input, string name, string keys, Action<InputAction.CallbackContext> started = null, Action<InputAction.CallbackContext> performed = null, Action<InputAction.CallbackContext> canceled = null) {
        List<InputAction> actions = new List<InputAction>();
        int total_binding_count = 0;
        foreach (string modifier_path in Settings.m_hotkey_modifier.Value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)) {
            InputAction action = new InputAction(name);
            int binding_count = 0;
            foreach (string action_path in keys.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)) {
                try {
                    action.AddCompositeBinding("OneModifier").With("Modifier", modifier_path).With("Binding", action_path);
                    binding_count++;
                } catch {
                    _warn_log($"** add_hotkey_action WARNING - unable to bind '{name}' action to hotkey (modifier: '{modifier_path}', key: '{action_path}).");
                    continue;
                }
            }
            if (binding_count == 0) {
                
                continue;
            }
            total_binding_count += binding_count;
            if (started != null) {
                action.started += started;
            }
            if (performed != null) {
                action.performed += performed;
            }
            if (canceled != null) {
                action.canceled += canceled;
            }
            action.Enable();
            player_input.actions.AddItem(action);
            actions.Add(action);
        }
        if (total_binding_count == 0) {
            _warn_log($"** add_hotkey_action WARNING - '{name}' action has no valid keybindings.");
        }
        return actions.ToArray();
    }

    [HarmonyPatch(typeof(LocalPlayerController), "Initialize")]
    class HarmonyPatch_LocalPlayerController_Initialize {
        private static void Postfix(LocalPlayerController __instance, PlayerInput ___m_input) {
            try {
                GameObject template = __instance.transform.parent.Find("GameUI").Find("GameLayer").Find("TimeAndCurrency").Find("Time").Find("Day").gameObject;
                GameObject obj = GameObject.Instantiate(template, template.transform.parent);
                obj.name = "TimeManagement_Time_Scale_Info";
                obj.transform.position += Vector3.right * 10;
                obj.SetActive(true);
                m_time_scale_text = obj.GetComponent<TextMeshProUGUI>();
                m_time_scale_text.fontSize = m_time_scale_text.fontSizeMin = m_time_scale_text.fontSizeMax = m_time_scale_text.fontSize / 2;
                List<string> available_paths = new List<string>();
                foreach (InputDevice device in InputSystem.devices) {
                    foreach (InputControl control in device.allControls) {
                        available_paths.Add(control.path);
                    }
                }
                _info_log($"\n\n-- All Currently Available Input Control Paths on this System --\n\n{String.Join("\n", available_paths)}\n");
                add_hotkey_action(___m_input, "time_stop", Settings.m_hotkey_time_stop_toggle.Value, performed: context => {
                    Settings.m_is_time_stopped.Value = !Settings.m_is_time_stopped.Value;
                    //_debug_log($"Time stop hotkey pressed - is_time_stopped: {Settings.m_is_time_stopped.Value}");
                });
                add_hotkey_action(___m_input, "time_speed_up", Settings.m_hotkey_time_speed_up.Value, performed: context => {
                    //_debug_log($"Time speed up hotkey pressed");
                    Settings.m_time_speed.Value += Settings.m_time_speed_delta.Value;
                });
                add_hotkey_action(___m_input, "time_speed_down", Settings.m_hotkey_time_speed_down.Value, performed: context => {
                    //_debug_log($"Time speed down hotkey pressed");
                    Settings.m_time_speed.Value -= Settings.m_time_speed_delta.Value;
                });
                add_hotkey_action(___m_input, "time_speed_reverse", Settings.m_hotkey_time_speed_reverse.Value, performed: context => {
                    //_debug_log($"Time speed reverse hotkey pressed");
                    Settings.m_time_speed.Value *= -1;
                });
            } catch (Exception e) {
                logger.LogError("** HarmonyPatch_LocalPlayerController_Initialize.Prefix ERROR - " + e);
            }
        }
    }

    [HarmonyPatch(typeof(GameState), "DuringDay_Update")]
    class HarmonyPatch_GameState_DuringDay_Update {
        private static bool Prefix(GameState __instance) {
            try {
                if (!Settings.m_enabled.Value) {
                    return true;
                }
                __instance.m_timeScale = (Settings.m_is_time_stopped.Value ? 0 : Settings.m_time_speed.Value);
                m_time_scale_text.text = $"({Settings.m_time_speed.Value:#0.0} gs/rts{(Settings.m_is_time_stopped.Value ? " |STOPPED|" : "")})";
                return true;
            } catch (Exception e) {
                logger.LogError("** HarmonyPatch_GameState_DuringDay_Update.Prefix ERROR - " + e);
            }
            return true;
        }
    }
}
