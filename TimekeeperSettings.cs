namespace Timekeeper
{
    class TimekeeperSettings : GameParameters.CustomParameterNode
    {
        public override string Section => "Timekeeper";
        public override string DisplaySection => Section;
        public override string Title => "Timekeeper Settings";
        public override int SectionOrder => 1;
        public override GameParameters.GameMode GameMode => GameParameters.GameMode.ANY;
        public override bool HasPresets => false;

        [GameParameters.CustomParameterUI("Mod Enabled", toolTip = "Turn Timekeeper on/off")]
        public bool modEnabled = true;
        public static bool ModEnabled => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().modEnabled;

        [GameParameters.CustomParameterUI("Count Orbits", toolTip = "Enable or disable orbits counter")]
        public bool countOrbits = true;
        public static bool CountOrbits => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().countOrbits;

        [GameParameters.CustomParameterUI("Count Sols", toolTip = "Enable or disable sols counter")]
        public bool countSols = true;
        public static bool CountSols => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().countSols;

        [GameParameters.CustomParameterUI("Zero-based Counters", toolTip = "Start counting from orbit/sol 0 instead of 1")]
        public bool zeroCounters = false;
        public static bool ZeroCounters => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().zeroCounters;

        [GameParameters.CustomFloatParameterUI("Screen Message Duration", toolTip = "# of seconds before the screen message showing number of orbits/sols disappers", displayFormat = "N0", minValue = 0, maxValue = 60)]
        public float messageDuration = 5;
        public static float MessageDuration => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().messageDuration;

        [GameParameters.CustomParameterUI("Debug Mode", toolTip = "Log everything to help Garwel see what the mod's doing wrong + display current phase")]
        public bool debugMode = false;
        public static bool DebugMode => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().debugMode;
    }
}
