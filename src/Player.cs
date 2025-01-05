using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;

namespace Sainot;

public partial class Sainot
{
    private void SlugcatStatsOnctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
    {
        orig(self, slugcat, malnourished);
        if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
        {
            self.throwingSkill = 1; //Always fix throws, only increase stats in Rambo mode

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

    private void PlayerOnGrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        orig(self, eu);
        if (!self.TryGetData(out var data))
            return;

        data.Belt.Increment = self.input[0].pckp;

        //Don't immediately retrieve bomb if swallowing, etc
        if (self.grasps[0]?.grabbed is not null and not ScavengerBomb)
        {
            data.Belt.Lock();
        }

        //Prefer eating over belt stuff if food in main hand
        if ((self.FoodInStomach < self.MaxFoodInStomach && self.grasps[0]?.grabbed is IPlayerEdible) || self.grasps[0]?.grabbed is KarmaFlower)
        {
            data.Belt.Lock();
        }

        data.Belt.Update();
    }

    private void PlayerILGrabUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;

            //Saint stop eating batflies
            c.GotoNext(i => i.MatchCallOrCallvirt<Player>("BiteEdibleObject"));
            c.GotoPrev(i => i.MatchLdfld<Player>("SlugCatClass"));
            c.GotoPrev(MoveType.After, i => i.MatchBrfalse(out label));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Player self) =>
            {
                if (self.TryGetData(out var data))
                {
                    if (data.Belt.ReadyToAddBomb())
                        return false;

                    if (self.grasps[0]?.grabbed != null)
                        data.Belt.Lock();
                }

                return true;
            });
            c.Emit(OpCodes.Brfalse, label);

            //Stop eating in general
            c.GotoNext(i => i.MatchCallOrCallvirt<Player>("BiteEdibleObject"));
            c.GotoPrev(MoveType.After, i => i.MatchBge(out label));
            c.GotoNext(i => i.MatchCallOrCallvirt<Player>("BiteEdibleObject"));
            c.GotoPrev(MoveType.Before, i => i.MatchLdarg(0));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Player self) =>
            {
                if (self.TryGetData(out var data))
                {
                    if (data.Belt.ReadyToAddBomb())
                        return false;

                    if (self.grasps[0]?.grabbed != null)
                        data.Belt.Lock();
                }

                return true;
            });
            c.Emit(OpCodes.Brfalse, label);

            //Bomb straight to belt
            c.GotoNext(i => i.MatchLdsfld<SoundID>("Slugcat_Switch_Hands_Init"));
            c.GotoPrev(i => i.MatchLdfld<Player>("pickUpCandidate"));
            c.GotoPrev(MoveType.Before, i => i.MatchLdarg(0));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Player self) =>
            {
                if (self.TryGetData(out var data))
                {
                    if (self.wantToPickUp > 0 && self.pickUpCandidate is ScavengerBomb bombCandidate)
                    {
                        if (!data.Belt.HasAFreeHand(out _) && !data.Belt.IsFull())
                        {
                            UnityEngine.Debug.Log("Bomb straight to belt");
                            data.Belt.Lock();
                            data.Belt.BombStraightToBelt(bombCandidate);
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }

    private Player.ObjectGrabability PlayerOnGrabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (obj is ScavengerBomb bomb)
        {
            if (ModManager.CoopAvailable && !Custom.rainWorld.options.friendlySteal)
            {
                if (bomb.abstractPhysicalObject.stuckObjects.Any(x => x is AbstractBombStick))
                    return Player.ObjectGrabability.CantGrab;
            }
            else
            {
                if (self.TryGetData(out var data) && data.Belt.Bombs.ContainsKey(bomb))
                    return Player.ObjectGrabability.CantGrab;
            }
        }

        return orig(self, obj);
    }
}