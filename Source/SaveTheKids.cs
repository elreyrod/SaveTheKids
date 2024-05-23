using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace SaveTheKids
{
    public class ChildDeathCounterComponent : WorldComponent
    {
        public ChildDeathCounterComponent(World world) : base(world) { }

        int _deadChildren = 0;
        public int DeadChildren { get { return _deadChildren; } set { _deadChildren = value; } }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref _deadChildren, "DeadChildren");
        }
    }

    public class Alert_DeadChildren : Alert
    {

        protected List<Thing> affectedThings = new List<Thing>();
        protected int lastTick = 0;

        public Alert_DeadChildren() : base()
        {
            this.defaultPriority = AlertPriority.Medium;
            this.defaultLabel = "";
            this.defaultExplanation = "You have failed these children.";
        }

        private int getDeadChildrenCount()
        {
            var deathComponent = Find.World.GetComponent<ChildDeathCounterComponent>();

            if (deathComponent == null)
            {
                deathComponent = new ChildDeathCounterComponent(Find.World);
                Find.World.components.Add(deathComponent);
            }

            return deathComponent.DeadChildren;
        }

        public override string GetLabel()
        {
            if (getDeadChildrenCount() == 0) { return ""; }
            return string.Format("Kids killed: {0}", getDeadChildrenCount());
        }


        public override AlertReport GetReport()
        {
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
        }

        public static void PostPawnKill(Pawn __instance)
        {
            var pawn = __instance;

            if (!pawn.ageTracker.Adult)
            {
                var deathComponent = Find.World.GetComponent<ChildDeathCounterComponent>();

                if (deathComponent == null)
                {
                    deathComponent = new ChildDeathCounterComponent(Find.World);
                    Find.World.components.Add(deathComponent);
                }

                deathComponent.DeadChildren++;

                Log.Warning("Kid dead:" + pawn.ageTracker.AgeBiologicalYears.ToString());
                Log.Warning("Dead Component Kid Count" + deathComponent.DeadChildren.ToString());
            }


        }
    }


}