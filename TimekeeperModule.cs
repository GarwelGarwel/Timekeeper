using System;

namespace Timekeeper
{
    public class TimekeeperModule : VesselModule
    {
        enum CountModes { None, Orbits, Sols };
        CountModes mode;

        int count;
        double startPhase, lastPhase;
        double lapTime;
        bool paused = false;

        protected override void OnLoad(ConfigNode node)
        {
            if (!TimekeeperSettings.ModEnabled) return;
            if (node.HasValue("mode")) mode = (CountModes)Enum.Parse(typeof(CountModes), node.GetValue("mode"), true); else mode = CountModes.None;
            if ((mode == CountModes.None) || ((mode == CountModes.Orbits) && !TimekeeperSettings.CountOrbits) || ((mode == CountModes.Sols) && !TimekeeperSettings.CountSols))
            {
                mode = CountModes.None;
                return;
            }
            lapTime = Core.GetDouble(node, "lapTime");
            count = Core.GetInt(node, "count");
            if (mode == CountModes.Orbits) startPhase = Core.GetDouble(node, "startPhase");
        }

        protected override void OnSave(ConfigNode node)
        {
            if ((mode == CountModes.None) || !TimekeeperSettings.ModEnabled) return;
            node.AddValue("mode", mode.ToString());
            node.AddValue("count", count);
            node.AddValue("lapTime", lapTime);
            if (mode == CountModes.Orbits) node.AddValue("startPhase", startPhase);
        }

        protected override void OnStart()
        {
            if (!TimekeeperSettings.ModEnabled) return;
            int count2 = 0;
            if (mode == CountModes.Orbits)
            {
                count2 = (int)Math.Floor((Planetarium.GetUniversalTime() - lapTime) / Vessel.orbit.period);
                lapTime += count2 * Vessel.orbit.period;
                count += count2;
            }
            if (mode == CountModes.Sols)
            {
                count2 = (int)Math.Floor((Planetarium.GetUniversalTime() - lapTime) / Vessel.mainBody.rotationPeriod);
                lapTime += count2 * Vessel.mainBody.rotationPeriod;
                count += count2;
            }
            if ((mode != CountModes.Orbits) && (Vessel.situation == Vessel.Situations.ORBITING)) OrbitsStart();
            if ((mode != CountModes.Sols) && ((Vessel.situation == Vessel.Situations.LANDED) || (Vessel.situation == Vessel.Situations.SPLASHED))) SolsStart();
            GameEvents.onVesselSituationChange.Add(OnVesselSituationChange);
            DisplayData();
        }

        void OnDisable() => GameEvents.onVesselSituationChange.Remove(OnVesselSituationChange);

        /// <summary>
        /// This formula is based on "absolute phase angle" by HetaruSun. It is a sum of Longitude of Ascending Node, Argument of Periapsis and True Anomaly. It is not affected by changes of orbit parameters (Pe, Ap, AN etc.)
        /// </summary>
        double OrbitPhase => (Vessel.orbit.LAN + (Vessel.orbit.argumentOfPeriapsis + Vessel.orbit.trueAnomaly * 180 / Math.PI) * (Vessel.orbit.inclination < 90 ? 1 : -1) + 180) % 360 - 180;

        ScreenMessage m = new ScreenMessage("", 1, ScreenMessageStyle.UPPER_LEFT);

        void FixedUpdate()
        {
            if (paused || !Vessel.loaded || !TimekeeperSettings.ModEnabled) return;
            double time = Planetarium.GetUniversalTime(), elapsed = time - lapTime;
            switch (mode)
            {
                case CountModes.Orbits:
                    if ((OrbitPhase * lastPhase > 0) && ((startPhase - OrbitPhase) * (startPhase - lastPhase) < 0))
                    {
                        count++;
                        lapTime = Planetarium.GetUniversalTime();
                        Core.Log(Vessel.vesselName + " has passed " + count + " orbits. Current phase: " + OrbitPhase + "; start phase: " + startPhase + "; last phase: " + lastPhase);
                        DisplayData();
                    }
                    lastPhase = OrbitPhase;
                    m.message = lastPhase.ToString("F1") + " | " + startPhase.ToString("F1");
                    ScreenMessages.PostScreenMessage(m);
                    break;
                case CountModes.Sols:
                    if (elapsed >= Vessel.mainBody.rotationPeriod)
                    {
                        count++;
                        Core.Log(Vessel.vesselName + " has completed its " + count + "th sol. Time since last sol is " + elapsed.ToString("F0") + " s; rotation period is " + Vessel.mainBody.rotationPeriod.ToString("F0") + " s.");
                        lapTime += Vessel.mainBody.rotationPeriod;
                        DisplayData();
                    }
                    break;
            }
        }

        void OrbitsStart()
        {
            if (!TimekeeperSettings.CountOrbits) return;
            mode = CountModes.Orbits;
            count = 0;
            lapTime = Planetarium.GetUniversalTime();
            lastPhase = startPhase = OrbitPhase;
            paused = false;
            Core.Log(Vessel.vesselName + " has begun orbiting at position " + startPhase);
            DisplayData();
        }

        void SolsStart()
        {
            if (!TimekeeperSettings.CountSols) return;
            mode = CountModes.Sols;
            count = 0;
            lapTime = Planetarium.GetUniversalTime();
            paused = false;
            Core.Log(Vessel.vesselName + " has begun counting sols at UT " + lapTime);
            DisplayData();
        }

        void StopTimekeeping()
        {
            mode = CountModes.None;
            count = 0;
        }

        void DisplayData()
        {
            if (!Vessel.isActiveVessel) return;
            if (mode == CountModes.Orbits) Core.ShowNotification("Orbit " + (count + (TimekeeperSettings.ZeroCounters ? 0 : 1)));
            else if (mode == CountModes.Sols) Core.ShowNotification("Sol " + (count + (TimekeeperSettings.ZeroCounters ? 0 : 1)));
        }

        void OnVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> e)
        {
            Core.Log("OnVesselSituationChange(<'" + e.host.vesselName + "', " + e.from + " -> " + e.to + ">)");
            if (e.host != Vessel) return;
            switch (e.to)
            {
                case Vessel.Situations.ORBITING:
                    OrbitsStart();
                    break;
                case Vessel.Situations.LANDED:
                case Vessel.Situations.SPLASHED:
                    if (paused) paused = false;
                    else SolsStart();
                    break;
                case Vessel.Situations.FLYING:
                case Vessel.Situations.SUB_ORBITAL:
                    if (mode == CountModes.Sols) paused = true;
                    else StopTimekeeping();
                    break;
                default:
                    StopTimekeeping();
                    break;
            }
        }
    }
}
