using BepInEx.Configuration;

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
    public static ConfigEntry<float> m_time_speed;
    public static ConfigEntry<float> m_time_speed_delta;
    public static ConfigEntry<bool> m_is_time_stopped;

    // Hotkeys
    public static ConfigEntry<string> m_hotkey_modifier;
    public static ConfigEntry<string> m_hotkey_time_stop_toggle;
    public static ConfigEntry<string> m_hotkey_time_speed_up;
    public static ConfigEntry<string> m_hotkey_time_speed_down;
    public static ConfigEntry<string> m_hotkey_time_speed_reverse;

    public void load(DDPlugin plugin) {
        this.m_plugin = plugin;

        // General
        m_enabled = this.m_plugin.Config.Bind<bool>("General", "Enabled", true, "Set to false to disable this mod.");
        m_log_level = this.m_plugin.Config.Bind<string>("General", "Log Level", "info", "[Advanced] Logging level, one of: 'none' (no logging), 'error' (only errors), 'warn' (errors and warnings), 'info' (normal logging), 'debug' (extra log messages for debugging issues).  Not case sensitive [string, default info].  Debug level not recommended unless you're noticing issues with the mod.  Changes to this setting require an application restart.");
        m_time_speed = this.m_plugin.Config.Bind<float>("General", "Initial Time Scale", 60.0f, "Initial time scale [in game seconds for every real-time second] (float, default 60 [game default, i.e. one game minute for every realtime second]).  Use configured hotkeys to increase/reduce this number, reduce below zero to reverse time.");
        m_time_speed_delta = this.m_plugin.Config.Bind<float>("General", "Time Scale Delta", 10f, "Change in time scale with each up/down hotkey tick (float, default 0.25).");
        m_is_time_stopped = this.m_plugin.Config.Bind<bool>("General", "Is Time Stopped", false, "This value will be used to determine if clock starts immediately when loading.  It is toggled using the Time Start/Stop Hotkey.");

        // Hotkeys
        m_hotkey_modifier = this.m_plugin.Config.Bind<string>("Hotkeys", "Hotkey - Modifier", "/Keyboard/leftCtrl,/Keyboard/rightCtrl", "Comma-separated list of Unity key 'control paths' used as the special modifier key (i.e. ctrl,alt,command) one of which is required to be down for hotkeys to work.  Set to '' (blank string) to not require a special key (not recommended).  Check the <game-root>/BepInEx/LogOutput.log for a full list of control path values available on your system.  Changing any hotkey requires a game reload.");
        m_hotkey_time_stop_toggle = this.m_plugin.Config.Bind<string>("Hotkeys", "Time Start/Stop Toggle Hotkey", "/Keyboard/0,/Keyboard/numpad0", "Comma-separated list of Unity key 'control paths', any of which will (when combined with the Modifier key) toggle the passage of time.  Check the <game-root>/BepInEx/LogOutput.log for a full list of control path values available on your system.  Changing any hotkey requires a game reload.");
        m_hotkey_time_speed_up = this.m_plugin.Config.Bind<string>("Hotkeys", "Time Scale Increment Hotkey", "/Keyboard/equals,/Keyboard/numpadPlus", "Comma-separated list of Unity key 'control paths', any of which will (when combined with the Modifier key) increase the time speed.  Check the <game-root>/BepInEx/LogOutput.log for a full list of control path values available on your system.  Changing any hotkey requires a game reload.");
        m_hotkey_time_speed_down = this.m_plugin.Config.Bind<string>("Hotkeys", "Time Scale Decrement Hotkey", "/Keyboard/minus,/Keyboard/numpadMinus", "Comma-separated list of Unity key 'control paths', any of which will (when combined with the Modifier key) decrease the time speed.  Check the <game-root>/BepInEx/LogOutput.log for a full list of control path values available on your system.  Changing any hotkey requires a game reload.");
        m_hotkey_time_speed_reverse = this.m_plugin.Config.Bind<string>("Hotkeys", "Time Scale Reverse Hotkey", "/Keyboard/9,/Keyboard/numpad9", "Comma-separated list of Unity key 'control paths', any of which will (when combined with the Modifier key) reverse the time spee.  Check the <game-root>/BepInEx/LogOutput.log for a full list of control path values available on your system.  Changing any hotkey requires a game reload.");
    }
}