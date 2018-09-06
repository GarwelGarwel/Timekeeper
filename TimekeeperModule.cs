using System;

namespace Timekeeper
{
    public class TimekeeperModule : VesselModule
    {
        enum CountModes { None, Orbits, Sols };
        CountModes mode;

        int count;
        double phase;
        double time;
        //double solLength;
        bool paused = false;

        public override Activation GetActivation() => Activation.FlightScene | Activation.LoadedVessels;

        protected override void OnLoad(ConfigNode node)
        {
            if (!TimekeeperSettings.ModEnabled) return;
            if (node.HasValue("mode")) mode = (CountModes)Enum.Parse(typeof(CountModes), node.GetValue("mode"), true); else mode = CountModes.None;
            if ((mode == CountModes.None) || ((mode == CountModes.Orbits) && !TimekeeperSettings.CountOrbits) || ((mode == CountModes.Sols) && !TimekeeperSettings.CountSols))
            {
                mode = CountModes.None;
                return;
            }
            time = Core.GetDouble(node, "time");
            count = Core.GetInt(node, "count");
            if (mode == CountModes.Orbits) phase = Core.GetDouble(node, "phase");
        }

        protected override void OnSave(ConfigNode node)
        {
            if ((mode == CountModes.None) || !TimekeeperSettings.ModEnabled) return;
            node.AddValue("mode", mode.ToString());
            node.AddValue("count", count);
            node.AddValue("time", time);
            if (mode == CountModes.Orbits) node.AddValue("phase", phase);
        }

        protected override void OnStart()
        {
            if (!TimekeeperSettings.ModEnabled) return;
            int count2 = 0;
            if (mode == CountModes.Orbits)
            {
                count2 = (int)Math.Floor((Planetarium.GetUniversalTime() - time) / Vessel.orbit.period);
                Core.Log("UT is " + Planetarium.GetUniversalTime() + "; lap time is " + time + "; orbital period is " + Vessel.orbit.period + "; " + count2 + " orbits made.");
                count += count2;
                time += count2 * Vessel.orbit.period;
                phase += (Planetarium.GetUniversalTime() - time) / Vessel.orbit.period * 360;
                if (phase > 360)
                {
                    count++;
                    phase -= 360;
                }
            }
            if (mode == CountModes.Sols)
            {
                //CalculateSolLength();
                count2 = (int)Math.Floor((Planetarium.GetUniversalTime() - time) / solLength);
                time += count2 * Vessel.mainBody.solarDayLength;
                count += count2;
            }
            if ((mode != CountModes.Orbits) && (Vessel.situation == Vessel.Situations.ORBITING)) OrbitsStart();
            if ((mode != CountModes.Sols) && ((Vessel.situation == Vessel.Situations.LANDED) || (Vessel.situation == Vessel.Situations.SPLASHED))) SolsStart();
            GameEvents.onVesselSituationChange.Add(OnVesselSituationChange);
            DisplayData();
        }

        void OnDisable() => GameEvents.onVesselSituationChange.Remove(OnVesselSituationChange);

        ScreenMessage m = new ScreenMessage("", 1, ScreenMessageStyle.UPPER_LEFT);

        void FixedUpdate()
        {
            if (paused || !TimekeeperSettings.ModEnabled) return;
            double now = Planetarium.GetUniversalTime(), interval = now - time;
            switch (mode)
            {
                case CountModes.Orbits:
                    double inc = interval / Vessel.orbit.period * 360;
                    phase += inc;
                    time = now;
                    if (phase >= 360)
                    {
                        count++;
                        phase -= 360;
                        Core.Log(Vessel.name + " has passed " + count + " orbits. Last interval is " + interval.ToString("F3") + " s, phase " + phase + " + " + inc);
                        DisplayData();
                    }
                    m.message = phase.ToString("F1");
                    ScreenMessages.PostScreenMessage(m);
                    break;
                case CountModes.Sols:
                    if (interval >= Vessel.mainBody.solarDayLength)
                    {
                        count++;
                        Core.Log(Vessel.vesselName + " has completed its " + count + "th sol. Time since last sol is " + interval.ToString("F0") + " s; solar day is " + Vessel.mainBody.solarDayLength.ToString("F0") + " s.");
                        time += Vessel.mainBody.solarDayLength;
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
            time = Planetarium.GetUniversalTime();
            phase = 0;
            paused = false;
            Core.Log(Vessel.vesselName + " has begun orbiting.");
            DisplayData();
        }

        //void CalculateSolLength() => solLength = Vessel.mainBody.orbit.period / (Vessel.mainBody.orbit.period / Vessel.mainBody.rotationPeriod + (Vessel.mainBody.inverseRotation ? -1 : 1));

        void SolsStart()
        {
            if (!TimekeeperSettings.CountSols) return;
            mode = CountModes.Sols;
            count = 0;
            time = Planetarium.GetUniversalTime();
            //CalculateSolLength();
            paused = false;
            Core.Log(Vessel.vesselName + " has begun counting sols at UT " + time);
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
