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
    public static ConfigEntry<float> m_speed_multiplier;
    public static ConfigEntry<float> m_sprint_speed_multiplier;
    public static ConfigEntry<bool> m_hold_key_to_sprint;

    // Hotkeys
    public static ConfigEntry<string> m_hotkey_sprint;

    public void load(DDPlugin plugin) {
        this.m_plugin = plugin;

        // General
        m_enabled = this.m_plugin.Config.Bind<bool>("General", "Enabled", true, "Set to false to disable this mod.");
        m_log_level = this.m_plugin.Config.Bind<string>("General", "Log Level", "info", "[Advanced] Logging level, one of: 'none' (no logging), 'error' (only errors), 'warn' (errors and warnings), 'info' (normal logging), 'debug' (extra log messages for debugging issues).  Not case sensitive [string, default info].  Debug level not recommended unless you're noticing issues with the mod.  Changes to this setting require an application restart.");
        m_speed_multiplier = this.m_plugin.Config.Bind<float>("General", "Movement Speed Multiplier", 1, "Multiplier applied to normal player movement (float, default 1 [no change]).");
        m_sprint_speed_multiplier = this.m_plugin.Config.Bind<float>("General", "Sprint Speed Multiplier", 1.5f, "Multiplier applied (in addition to Movement Speed Multiplier) to movement speed (float, default 1.5).");
        m_hold_key_to_sprint = this.m_plugin.Config.Bind<bool>("General", "Hold Sprint Key", true, "Set to false to cause sprint key to toggle sprint status.");

        // Hotkeys
        m_hotkey_sprint = this.m_plugin.Config.Bind<string>("Hotkeys", "Hotkey - Sprint", "LeftShift,RightShift", "Comma-separated list of Unity Keycodes, any of which will cause player to sprint (either toggle or held, depending on value of 'Hold Sprint Key' setting).");
    }
}