using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace SaveTheKids
{
    public static class RimWorldExtensions
    {
        public static bool IsPawnChild(this Pawn pawn)
        {
            return !pawn.ageTracker.Adult || // IsAdult covers babies through 13
                 pawn.ageTracker.AgeBiologicalYearsFloat < 18.0; // Teenagers are children.
        }

        public static ChildDeathCounterComponent GetChildDeathComponent(this World world)
        {
            var deathComponent = world.GetComponent<ChildDeathCounterComponent>();

            if (deathComponent == null)
            {
                deathComponent = new ChildDeathCounterComponent(Find.World);
                world.components.Add(deathComponent);
            }

            return deathComponent;
        }
    }

    public class ChildDeathCounterComponent : WorldComponent
    {
        public ChildDeathCounterComponent(World world) : base(world) { }

        int _deadChildren = 0;
        public int DeadChildren { get { return _deadChildren; } set { _deadChildren = value; } }

        int _savedChildren = 0;
        public int SavedChildren { get { return _savedChildren; } set { _savedChildren = value; } }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref _deadChildren, "DeadChildren");
            Scribe_Values.Look(ref _savedChildren, "SavedChildren");
        }
    }

    public class Alert_DeadChildren : Alert
    {
        public Alert_DeadChildren() : base()
        {
            this.defaultPriority = AlertPriority.Medium;
            this.defaultLabel = "";
            this.defaultExplanation = "You have failed these children.";
        }

        private int getDeadChildrenCount()
        {
            var deathComponent = Find.World.GetChildDeathComponent();

            return deathComponent.DeadChildren;
        }

        public override string GetLabel()
        {
            if (getDeadChildrenCount() == 0) { return ""; }
            return string.Format("Kids killed: {0}", getDeadChildrenCount());
        }


        public override AlertReport GetReport()
        {
            if (getDeadChildrenCount() == 0) { return AlertReport.Inactive; }

            return AlertReport.Active;
        }
    }

    public class Alert_SavedChildren : Alert
    {
        public Alert_SavedChildren() : base()
        {
            this.defaultPriority = AlertPriority.Medium;
            this.defaultLabel = "";
            this.defaultExplanation = "All of them have been saved.";
        }

        private int getSavedChildrenCount()
        {
            var deathComponent = Find.World.GetChildDeathComponent();

            return deathComponent.SavedChildren;
        }

        public override string GetLabel()
        {
            if (getSavedChildrenCount() == 0) { return ""; }
            return string.Format("Kids saved: {0}", getSavedChildrenCount());
        }


        public override AlertReport GetReport()
        {
            if (getSavedChildrenCount() == 0) { return AlertReport.Inactive; }

            return AlertReport.Active;
        }
    }

    [Verse.StaticConstructorOnStartup]
    public static class Patch
    {
        static Patch()
        {
            var pawnKill = typeof(Pawn).GetMethod("Kill");

            var postPawnKill = typeof(SaveTheKids.Patch).GetMethod("PostPawnKill",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            new HarmonyLib.Harmony("ElReyRod.SaveTheKids").Patch(
              pawnKill,
              postfix: new HarmonyLib.HarmonyMethod(postPawnKill));

            var deregisterPawn = typeof(MapPawns).GetMethod("DeRegisterPawn");

            var postDeRegisterPawn = typeof(SaveTheKids.Patch).GetMethod("PostDeRegisterPawn",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            new HarmonyLib.Harmony("ElReyRod.SaveTheKids").Patch(
              deregisterPawn,
              postfix: new HarmonyLib.HarmonyMethod(postDeRegisterPawn));
        }

        public static void PostDeRegisterPawn(MapPawns __instance, Pawn p)
        {
            if (p.IsPawnChild() && !p.Dead)
            {
                Log.Message($"Pawn saved: {p.Name}");
                var deathComponent = Find.World.GetChildDeathComponent();

                deathComponent.SavedChildren++;
            }
        }

        public static void PostPawnKill(Pawn __instance)
        {
            var pawn = __instance;

            if (pawn.IsPawnChild())
            {
                var deathComponent = Find.World.GetChildDeathComponent();

                deathComponent.DeadChildren++;
            }
        }
    }


}