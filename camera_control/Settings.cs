﻿using BepInEx.Configuration;

public class Settings {
    private static Settings m_instance = null;
    public static Settings Instance {
        get {
            if (m_instance == null) {
                m_instance = new Settings();
            }
            return m_instance;
        }
    }
    private DDPlugin m_plugin = null;

    // General
    public static ConfigEntry<bool> m_enabled;
    public static ConfigEntry<string> m_log_level;
    public static ConfigEntry<float> m_camera_distance;
    public static ConfigEntry<float> m_camera_zoom_delta;
    public static ConfigEntry<float> m_min_camera_pitch;

    // Hotkeys
    public static ConfigEntry<string> m_hotkey_modifier;
    public static ConfigEntry<string> m_hotkey_camera_zoom_in;
    public static ConfigEntry<string> m_hotkey_camera_zoom_out;
    public static ConfigEntry<string> m_hotkey_camera_zoom_reset;

    private string hotkey_description(string unique) {
        const bool IS_MODIFIER_AVAILABLE = true;
        return $"Comma-separated list of Unity key 'control paths', any of which will {(unique == null ? "act as the special modifier key (i.e. alt/ctrl/shift) required to be pressed along with other hotkeys" : (IS_MODIFIER_AVAILABLE ? "(when combined with the Modifier key) " : " ") + unique)}.  Check the <game-root>/BepInEx/config/available_control_paths.txt for a full list of control path values available on your system.  Changing any hotkey requires a game reload.";
    }

    public void load(DDPlugin plugin) {
        this.m_plugin = plugin;

        // General
        m_enabled = this.m_plugin.Config.Bind<bool>("General", "Enabled", true, "Set to false to disable this mod.");
        m_log_level = this.m_plugin.Config.Bind<string>("General", "Log Level", "info", "[Advanced] Logging level, one of: 'none' (no logging), 'error' (only errors), 'warn' (errors and warnings), 'info' (normal logging), 'debug' (extra log messages for debugging issues).  Not case sensitive [string, default info].  Debug level not recommended unless you're noticing issues with the mod.  Changes to this setting require an application restart.");
        m_camera_distance = this.m_plugin.Config.Bind<float>("General", "Camera - Distance", 16.0f, "Distance between camera and player (float, default 16 [game default]).  Use configured hotkeys to increase/reduce this number.");
        m_camera_zoom_delta = this.m_plugin.Config.Bind<float>("General", "Camera - Zoom Delta", 0.5f, "The change in camera distance (forward/back) with each hotkey press (float, default 0.5).");
        m_min_camera_pitch = this.m_plugin.Config.Bind<float>("General", "Camera - Minimum Pitch", -45f, "The angle in degrees (from player toward the ground) at which the camera will stop.  Set to a lower number to allow for more range of motion.  Numbers below -45 (the default value) will sometimes allow the camera (at sufficient velocity) to clip below ground objects, but it will correct itself quickly and will not cause game issues.");

        // Hotkeys
        m_hotkey_modifier = this.m_plugin.Config.Bind<string>("Hotkeys", "Hotkey - Modifier", "/Keyboard/leftCtrl,/Keyboard/rightCtrl", hotkey_description(null));
        m_hotkey_camera_zoom_in = this.m_plugin.Config.Bind<string>("Hotkeys", "Hotkey - Camera Zoom In", "/Mouse/scroll/up", hotkey_description("move the camera closer to the player"));
        m_hotkey_camera_zoom_out = this.m_plugin.Config.Bind<string>("Hotkeys", "Hotkey - Camera Zoom Out", "/Mouse/scroll/down", hotkey_description("move the camera farther away from the player"));
        m_hotkey_camera_zoom_reset = this.m_plugin.Config.Bind<string>("Hotkeys", "Hotkey - Camera Zoom Reset", "/Mouse/scroll/middleButton", hotkey_description("reset the camera to the game default position (16)"));
    }
}