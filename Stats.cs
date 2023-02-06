using MoreSlugcats;

namespace Sainot;

public partial class Sainot
{
    private void SlugcatStatsOnctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
    {
        orig(self, slugcat, malnourished);
        if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
        {
            self.throwingSkill = 1;
            if (Rambo)
            {
                self.throwingSkill = 2;
                self.runspeedFac = 1.3f;
                self.bodyWeightFac = 1.20f;
                self.generalVisibilityBonus = 0.1f;
                self.visualStealthInSneakMode = 0.3f;
                self.loudnessFac = 1.35f;
                self.poleClimbSpeedFac = 1.35f;
                self.corridorClimbSpeedFac = 1.4f;
            }
        }
    }

}