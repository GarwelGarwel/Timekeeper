using System;

namespace Timekeeper
{
    public class TimekeeperModule : VesselModule
    {
        enum CountModes { None, Orbits, Sols };
        CountModes mode;

        int count;
        double startLocation, lastLocation;
        double lapTime;
        bool paused = false;

        public override bool ShouldBeActive() => Vessel.vesselType != VesselType.Debris;

        protected override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            if (node.HasValue("mode")) mode = (CountModes)Enum.Parse(typeof(CountModes), node.GetValue("mode"), true); else mode = CountModes.None;
            if (mode == CountModes.None) return;
            lapTime = Core.GetDouble(node, "lapTime");
            if (mode == CountModes.Orbits)
            {
                count = (int)Math.Floor((Planetarium.GetUniversalTime() - lapTime) / Vessel.orbit.period);
                lapTime += count * Vessel.orbit.period;
                count += Core.GetInt(node, "count");
                startLocation = Core.GetDouble(node, "startLocation");
            }
        }

        protected override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            if (mode == CountModes.None) return;
            node.AddValue("mode", mode.ToString());
            node.AddValue("count", count);
            node.AddValue("lapTime", lapTime);
            if (mode == CountModes.Orbits) node.AddValue("startLocation", startLocation);
        }

        protected override void OnStart()
        {
            base.OnStart();
            if ((mode != CountModes.Orbits) && (Vessel.situation == Vessel.Situations.ORBITING)) OrbitsStart();
            if ((mode != CountModes.Sols) && ((Vessel.situation == Vessel.Situations.LANDED) || (Vessel.situation == Vessel.Situations.SPLASHED))) SolsStart();
            GameEvents.onVesselSituationChange.Add(OnVesselSituationChange);
            DisplayData();
        }

        void OnDisable() => GameEvents.onVesselSituationChange.Remove(OnVesselSituationChange);

        double OrbitPosition => Vessel.orbit.trueAnomaly;

        void FixedUpdate()
        {
            if (paused) return;
            switch (mode)
            {
                case CountModes.Orbits:
                    if ((OrbitPosition - lastLocation) * (startLocation - lastLocation) < 0)
                    {
                        count++;
                        lapTime = Planetarium.GetUniversalTime();
                        Core.Log(Vessel.vesselName + " has completed its " + count + "th orbit. Current location: " + OrbitPosition + "; start location: " + startLocation + "; last location: " + lastLocation);
                    }
                    lastLocation = OrbitPosition;
                    break;
                case CountModes.Sols:
                    double time = Planetarium.GetUniversalTime();
                    if (time - lapTime >= Vessel.mainBody.rotationPeriod)
                    {
                        count++;
                        Core.Log(Vessel.vesselName + " has completed its " + count + "th sol. Time since last sol is " + (time - lapTime).ToString("F2") + " s.");
                        lapTime += Vessel.mainBody.rotationPeriod;
                    }
                    break;
            }
        }

        void OrbitsStart()
        {
            mode = CountModes.Orbits;
            count = 0;
            lapTime = Planetarium.GetUniversalTime();
            lastLocation = startLocation = OrbitPosition;
            paused = false;
            Core.Log(Vessel.vesselName + " has begun orbiting at position " + startLocation);
            DisplayData();
        }

        void SolsStart()
        {
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
            switch (mode)
            {
                case CountModes.Orbits:
                    Core.ShowNotification("Orbit " + (count + 1));
                    break;
                case CountModes.Sols:
                    Core.ShowNotification("Sol " + (count + 1));
                    break;
            }
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
