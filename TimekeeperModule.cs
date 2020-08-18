using System;

namespace Timekeeper
{
    public class TimekeeperModule : VesselModule
    {
        enum CountMode { None = 0, Orbits, Sols };

        CountMode mode;
        int count;
        double time;
        double phase;
        bool paused = false;
        ScreenMessage m = new ScreenMessage("", 1, ScreenMessageStyle.UPPER_LEFT);

        public override Activation GetActivation() => Activation.FlightScene | Activation.LoadedVessels;

        protected override void OnLoad(ConfigNode node)
        {
            if (!TimekeeperSettings.Instance.ModEnabled)
                return;
            Core.Log($"OnLoad({node.CountValues} values)");
            mode = node.HasValue("mode") ? (CountMode)Enum.Parse(typeof(CountMode), node.GetValue("mode"), true) : CountMode.None;
            if ((mode == CountMode.None) || ((mode == CountMode.Orbits) && !TimekeeperSettings.Instance.CountOrbits)
                || ((mode == CountMode.Sols) && !TimekeeperSettings.Instance.CountSols))
            {
                mode = CountMode.None;
                return;
            }
            time = node.GetDouble("time");
            count = node.GetInt("count");
            if (mode == CountMode.Orbits)
                phase = node.GetDouble("phase");
            else paused = node.GetBool("paused");
        }

        protected override void OnSave(ConfigNode node)
        {
            if (mode == CountMode.None || !TimekeeperSettings.Instance.ModEnabled)
                return;
            Core.Log("OnSave");
            node.AddValue("mode", mode.ToString());
            node.AddValue("count", count);
            node.AddValue("time", time);
            if (mode == CountMode.Orbits)
                node.AddValue("phase", phase);
            if (paused)
                node.AddValue("paused", true);
        }

        protected override void OnStart()
        {
            if (!TimekeeperSettings.Instance.ModEnabled)
                return;
            Core.Log("TimekeeperModule.OnStart");

            int count2 = 0;
            if (mode == CountMode.Orbits)
            {
                count2 = (int)Math.Floor((Planetarium.GetUniversalTime() - time) / Vessel.orbit.period);
                Core.Log($"UT is {Planetarium.GetUniversalTime()}; last update time is {time}; orbital period is {Vessel.orbit.period}; {count2} new orbits made.");
                count += count2;
                phase += (Planetarium.GetUniversalTime() - time - count2 * Vessel.orbit.period) / Vessel.orbit.period * 360;
                if (phase >= 360)
                {
                    count++;
                    phase -= 360;
                }
                time = Planetarium.GetUniversalTime();
            }

            if (mode == CountMode.Sols)
            {
                count2 = (int)Math.Floor((Planetarium.GetUniversalTime() - time) / Vessel.mainBody.solarDayLength);
                Core.Log($"UT is {Planetarium.GetUniversalTime()}; sol start time is {time}; solar day is {Vessel.mainBody.solarDayLength:F1}; {count2} sols passed.");
                time += count2 * Vessel.mainBody.solarDayLength;
                count += count2;
            }

            if (mode != CountMode.Orbits && Vessel.situation == Vessel.Situations.ORBITING)
                OrbitsStart();

            if (mode != CountMode.Sols && (Vessel.situation == Vessel.Situations.LANDED || Vessel.situation == Vessel.Situations.SPLASHED))
                SolsStart();

            GameEvents.onVesselSituationChange.Add(OnVesselSituationChange);
            GameEvents.onVesselSOIChanged.Add(OnVesselSOIChanged);
            DisplayData();
        }

        void OnDisable()
        {
            GameEvents.onVesselSituationChange.Remove(OnVesselSituationChange);
            GameEvents.onVesselSOIChanged.Remove(OnVesselSOIChanged);
        }

        void FixedUpdate()
        {
            if (paused || !TimekeeperSettings.Instance.ModEnabled)
                return;
            double now = Planetarium.GetUniversalTime();
            double interval = now - time;
            switch (mode)
            {
                case CountMode.Orbits:
                    double inc = interval / Vessel.orbit.period * 360;
                    phase += inc;
                    time = now;
                    if (phase >= 360)
                    {
                        count++;
                        phase -= 360;
                        Core.Log($"{Vessel.name} has passed {count} orbits. Last interval is {interval:F3} s, phase {phase} + {inc}.");
                        DisplayData();
                    }
                    if (TimekeeperSettings.Instance.DebugMode)
                    {
                        m.message = phase.ToString("F1");
                        ScreenMessages.PostScreenMessage(m);
                    }
                    break;

                case CountMode.Sols:
                    if (interval >= Vessel.mainBody.solarDayLength)
                    {
                        count++;
                        Core.Log($"{Vessel.vesselName} has completed its {count}th sol. Time since last sol is {interval:F0} s; solar day is {Vessel.mainBody.solarDayLength:F0} s.");
                        time += Vessel.mainBody.solarDayLength;
                        DisplayData();
                    }
                    break;
            }
        }

        void OrbitsStart()
        {
            if (!TimekeeperSettings.Instance.CountOrbits)
                return;
            mode = CountMode.Orbits;
            count = 0;
            time = Planetarium.GetUniversalTime();
            phase = 0;
            paused = false;
            Core.Log(Vessel.vesselName + " has begun orbiting @ " + time + " with period of " + Vessel.orbit.period.ToString("F1"));
            DisplayData();
        }

        void SolsStart()
        {
            if (!TimekeeperSettings.Instance.CountSols)
                return;
            mode = CountMode.Sols;
            count = 0;
            time = Planetarium.GetUniversalTime();
            paused = false;
            Core.Log(Vessel.vesselName + " has begun counting sols at UT " + time);
            DisplayData();
        }

        void StopTimekeeping()
        {
            mode = CountMode.None;
            count = 0;
        }

        void DisplayData()
        {
            if (!Vessel.isActiveVessel)
            {
                Core.Log("DisplayData for " + Vessel.vesselName + " while active vessel is " + FlightGlobals.ActiveVessel.vesselName);
                return;
            }
            if (mode == CountMode.Orbits)
                Core.ShowNotification("Orbit " + (count + (TimekeeperSettings.Instance.ZeroCounters ? 0 : 1)));
            else if (mode == CountMode.Sols)
                Core.ShowNotification("Sol " + (count + (TimekeeperSettings.Instance.ZeroCounters ? 0 : 1)));
        }

        void OnVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> e)
        {
            Core.Log("OnVesselSituationChange(<'" + e.host.vesselName + "', " + e.from + " -> " + e.to + ">)");
            if (e.host != Vessel)
                return;

            switch (e.to)
            {
                case Vessel.Situations.ORBITING:
                    if (mode != CountMode.Orbits)
                        OrbitsStart();
                    break;

                case Vessel.Situations.LANDED:
                case Vessel.Situations.SPLASHED:
                    if (paused)
                        paused = false;
                    else SolsStart();
                    break;

                case Vessel.Situations.FLYING:
                case Vessel.Situations.SUB_ORBITAL:
                    if (mode == CountMode.Sols)
                        paused = true;
                    break;

                default:
                    StopTimekeeping();
                    break;
            }
        }

        void OnVesselSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> e) => StopTimekeeping();
    }
}
