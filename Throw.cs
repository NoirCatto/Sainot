using System;
using System.Linq;
using System.Runtime.InteropServices;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using UnityEngine;

namespace Sainot;

public partial class Sainot
{
    private void PlayerOnThrowObject(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;

            c.GotoNext(i => i.MatchCallOrCallvirt<Player>("TossObject"));
            c.GotoNext(i => i.MatchLdsfld<ModManager>("MSC"));
            c.GotoNext(MoveType.After,i => i.MatchBrfalse(out label));
            c.GotoPrev(i => i.MatchLdsfld<ModManager>("MSC"));
            c.GotoPrev(i => i.MatchLdsfld<ModManager>("MSC"));
            c.GotoNext(MoveType.After, i => i.MatchBrfalse(out _));
            c.Emit(OpCodes.Br, label);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }

    private void SeedCobOnHitByWeapon(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;

            c.GotoNext(i => i.MatchLdsfld<MoreSlugcatsEnums.RoomRainDangerType>("Blizzard"));
            c.GotoNext(MoveType.After, i => i.MatchBrfalse(out label));
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate((Weapon weapon) => weapon is not Spear);
            c.Emit(OpCodes.Brfalse, label);

            c.GotoNext(i => i.MatchLdfld<Weapon>("thrownBy"));
            c.GotoNext(MoveType.After, i => i.MatchBrfalse(out label));
            c.Emit(OpCodes.Br, label);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
        
    }
    
    private Player.ObjectGrabability PlayerOnGrabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (obj is ScavengerBomb bomb && BombBelts.Any(x => x.Bombs.ContainsKey(bomb)))
        {
            return Player.ObjectGrabability.CantGrab;
        }
        
        return orig(self, obj);
    }
    
}