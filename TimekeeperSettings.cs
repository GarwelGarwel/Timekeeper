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

        public static bool ModEnabled
        {
            get => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().modEnabled;
            set => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().modEnabled = value;
        }

        [GameParameters.CustomParameterUI("Count Orbits", toolTip = "Enable or disable sols counter")]
        public bool countOrbits = true;

        public static bool CountOrbits
        {
            get => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().countOrbits;
            set => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().countOrbits = value;
        }

        [GameParameters.CustomParameterUI("Count Sols", toolTip = "Enable or disable sols counter")]
        public bool countSols = true;

        public static bool CountSols
        {
            get => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().countSols;
            set => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().countSols = value;
        }

        [GameParameters.CustomParameterUI("Zero-based Counters", toolTip = "Start counting from orbit/sol 0 instead of 1")]
        public bool zeroCounters = false;

        public static bool ZeroCounters
        {
            get => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().zeroCounters;
            set => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().zeroCounters = value;
        }

        [GameParameters.CustomParameterUI("Debug Mode", toolTip = "Log everything to help Garwel see what the mod's doing wrong")]
        public bool debugMode = true;

        public static bool DebugMode
        {
            get => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().debugMode;
            set => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>().debugMode = value;
        }
    }
}
