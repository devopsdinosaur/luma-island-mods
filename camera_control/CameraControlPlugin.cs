using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine;

public static class PluginInfo {

    public const string TITLE = "Camera Control";
    public const string NAME = "camera_control";
    public const string SHORT_DESCRIPTION = "Add more control to the camera.  Remove the camera fog/haze and zoom in and out as far as you want using configurable hotkeys and allow the camera to drop completely to the ground for much better viewing!  First-person mode coming soon.";

    public const string VERSION = "0.0.7";

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
    private static bool m_added_actions = false;
    private const float CAMERA_DEFAULT_DISTANCE = 16f;
    private const float CAMERA_DEFAULT_LOOK_AT_Y = 0f;

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
                if (m_added_actions) {
                    return;
                }
                m_added_actions = true;
                UnityUtils.dump_control_paths();
                UnityUtils.add_hotkey_action(___m_input, "camera_zoom_in", Settings.m_hotkey_modifier.Value, Settings.m_hotkey_camera_zoom_in.Value, performed: context => {
                    Settings.m_camera_distance.Value -= Settings.m_camera_zoom_delta.Value;
                    _debug_log($"Camera ZOOM IN button pressed");
                });
                UnityUtils.add_hotkey_action(___m_input, "camera_zoom_out", Settings.m_hotkey_modifier.Value, Settings.m_hotkey_camera_zoom_out.Value, performed: context => {
                    Settings.m_camera_distance.Value += Settings.m_camera_zoom_delta.Value;
                    _debug_log($"Camera ZOOM OUT button pressed");
                });
                UnityUtils.add_hotkey_action(___m_input, "camera_zoom_reset", Settings.m_hotkey_modifier.Value, Settings.m_hotkey_camera_zoom_reset.Value, performed: context => {
                    Settings.m_camera_distance.Value = CAMERA_DEFAULT_DISTANCE;
                    _debug_log($"Camera ZOOM RESET button pressed");
                });
                UnityUtils.add_hotkey_action(___m_input, "camera_look_up", Settings.m_hotkey_modifier.Value, Settings.m_hotkey_camera_look_y_up.Value, performed: context => {
                    Settings.m_camera_lookat_y_offset.Value += Settings.m_camera_lookat_delta.Value;
                    _debug_log($"Camera LOOK UP button pressed");
                });
                UnityUtils.add_hotkey_action(___m_input, "camera_look_down", Settings.m_hotkey_modifier.Value, Settings.m_hotkey_camera_look_y_down.Value, performed: context => {
                    Settings.m_camera_lookat_y_offset.Value -= Settings.m_camera_lookat_delta.Value;
                    _debug_log($"Camera LOOK DOWN button pressed");
                });
                UnityUtils.add_hotkey_action(___m_input, "camera_look_reset", Settings.m_hotkey_modifier.Value, Settings.m_hotkey_camera_look_reset.Value, performed: context => {
                    Settings.m_camera_lookat_y_offset.Value = CAMERA_DEFAULT_LOOK_AT_Y;
                    _debug_log($"Camera LOOK RESET button pressed");
                });
            } catch (Exception e) {
                logger.LogError("** HarmonyPatch_LocalPlayerController_Initialize.Prefix ERROR - " + e);
            }
        }
    }

    [HarmonyPatch(typeof(TopDownTrackingCamera), "Update")]
    class HarmonyPatch_TopDownTrackingCamera_Update {
        public static float m_fog_start_distance = float.MinValue;
        public static float m_fog_end_distance = float.MinValue;

        private static bool Prefix(TopDownTrackingCamera __instance) {
            try {
                if (!Settings.m_enabled.Value) {
                    return true;
                }
                __instance.m_distance = Settings.m_camera_distance.Value;
                __instance.m_minPitch = Settings.m_min_camera_pitch.Value;
                if (m_fog_start_distance == float.MinValue) {
                    m_fog_start_distance = RenderSettings.fogStartDistance;
                    m_fog_end_distance = RenderSettings.fogEndDistance;
                }
                if (Settings.m_disable_fog.Value) {
                    RenderSettings.fogStartDistance = RenderSettings.fogEndDistance = float.MaxValue;
                } else {
                    RenderSettings.fogStartDistance = m_fog_start_distance;
                    RenderSettings.fogEndDistance = m_fog_end_distance;
                }
                __instance.heightCorrection = !Settings.m_disable_automatic_pitch_correction.Value;
                return true;
            } catch (Exception e) {
                logger.LogError("** HarmonyPatch_TopDownTrackingCamera_Update.Prefix ERROR - " + e);
            }
            return true;
        }

        private static void Postfix(TopDownTrackingCamera __instance, float3 ___m_animatedOffset, float3 ___m_focusSmooth, Timer ___m_shakeTimer, AnimationCurve ___m_shakeOffsetX, AnimationCurve ___m_shakeOffsetZ) {
            try {
                if (!Settings.m_enabled.Value) {
                    return;
                }
                __instance.heightCorrection = Settings.m_disable_automatic_pitch_correction.Value;
                float3 offset = float3.zero;
                if (___m_shakeTimer != null) {
                    float time = ___m_shakeTimer.duration - ___m_shakeTimer.GetRemainingTime();
                    offset = __instance.transform.rotation * new Vector3(___m_shakeOffsetX.Evaluate(time), ___m_shakeOffsetZ.Evaluate(time), 0f);
                }
                __instance.transform.LookAt(___m_animatedOffset + ___m_focusSmooth + offset + (float3) (Vector3.up * Settings.m_camera_lookat_y_offset.Value), Vector3.up);
            } catch (Exception e) {
                logger.LogError("** HarmonyPatch_TopDownTrackingCamera_Update.Postfix ERROR - " + e);
            }
        }
    }
}
