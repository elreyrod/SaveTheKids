using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using UnityEngine;

namespace SaveTheKids
{
    public class SaveTheKidsSettings : ModSettings
    {
        public bool showDeadKidCounter = true;
        public bool showSavedKidCounter = false;
        public bool countAllUnder18AsKids = true;


        public override void ExposeData()
        {
            Scribe_Values.Look(ref showDeadKidCounter, "showDeadKidCounter");
            Scribe_Values.Look(ref showSavedKidCounter, "showSavedKidCounter");
            Scribe_Values.Look(ref countAllUnder18AsKids, "countAllUnder18AsKids");
            base.ExposeData();
        }
    }

    public class SaveTheKidsMod : Mod
    {
        SaveTheKidsSettings settings;

        public SaveTheKidsMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<SaveTheKidsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Show Dead Kid Counter", ref settings.showDeadKidCounter);
            listingStandard.CheckboxLabeled("Show Saved Kid Counter", ref settings.showSavedKidCounter);
            listingStandard.CheckboxLabeled("Count all pawns under 18 as kids", ref settings.countAllUnder18AsKids);
            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Save The Kids";
        }
    }
}