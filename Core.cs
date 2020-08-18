using UnityEngine;

namespace Timekeeper
{
    /// <summary>
    /// Log levels:
    /// <list type="bullet">
    /// <item><definition>None: do not log</definition></item>
    /// <item><definition>Error: log only errors</definition></item>
    /// <item><definition>Important: log only errors and important information</definition></item>
    /// <item><definition>Debug: log all information</definition></item>
    /// </list>
    /// </summary>
    public enum LogLevel { None = 0, Error, Important, Debug };

    static class Core
    {
        /// <summary>
        /// Current <see cref="LogLevel"/>: either Debug or Important
        /// </summary>
        public static LogLevel Level => TimekeeperSettings.Instance.DebugMode ? LogLevel.Debug : LogLevel.Important;

        public static void ShowNotification(string msg) => ScreenMessages.PostScreenMessage(msg, TimekeeperSettings.Instance.MessageDuration);

        public static string GetString(this ConfigNode n, string key, string defaultValue = null) => n.HasValue(key) ? n.GetValue(key) : defaultValue;

        public static double GetDouble(this ConfigNode n, string key, double defaultValue = 0)
            => double.TryParse(n.GetValue(key), out double res) ? res : defaultValue;

        public static int GetInt(this ConfigNode n, string key, int defaultValue = 0)
            => int.TryParse(n.GetValue(key), out int res) ? res : defaultValue;

        public static bool GetBool(this ConfigNode n, string key, bool defaultValue = false)
            => bool.TryParse(n.GetValue(key), out bool res) ? res : defaultValue;

        /// <summary>
        /// Write into output_log.txt
        /// </summary>
        /// <param name="message">Text to log</param>
        /// <param name="messageLevel"><see cref="LogLevel"/> of the entry</param>
        public static void Log(string message, LogLevel messageLevel = LogLevel.Debug)
        {
            if (messageLevel <= Level)
                Debug.Log("[Timekeeper] " + message);
        }
    }
}
