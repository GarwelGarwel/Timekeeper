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

        public static TimekeeperSettings Instance => HighLogic.CurrentGame.Parameters.CustomParams<TimekeeperSettings>();

        [GameParameters.CustomParameterUI("Mod Enabled", toolTip = "Turn Timekeeper on/off")]
        public bool ModEnabled = true;

        [GameParameters.CustomParameterUI("Count Orbits", toolTip = "Enable or disable orbits counter")]
        public bool CountOrbits = true;

        [GameParameters.CustomParameterUI("Count Sols", toolTip = "Enable or disable sols counter")]
        public bool CountSols = true;

        [GameParameters.CustomParameterUI("Zero-based Counters", toolTip = "Start counting from orbit/sol 0 instead of 1")]
        public bool ZeroCounters = false;

        [GameParameters.CustomFloatParameterUI("Screen Message Duration", toolTip = "# of seconds before the screen message showing number of orbits/sols disappers", displayFormat = "N0", minValue = 0, maxValue = 60)]
        public float MessageDuration = 5;

        [GameParameters.CustomParameterUI("Debug Mode", toolTip = "Log everything to help Garwel see what the mod's doing wrong + display current phase")]
        public bool DebugMode = false;
    }
}
