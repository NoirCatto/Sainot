using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using RWCustom;
using BepInEx;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using On.JollyCoop.JollyMenu;
using On.Menu;
using Debug = UnityEngine.Debug;
#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Sainot;

[BepInPlugin("NoirCatto.Sainot", "Sain't", "1.0.0")]
public partial class Sainot : BaseUnityPlugin
{
    private static SainotOptions Options;
    private static bool Rambo => Options.Rambo.Value;
    private static int BeltsCapacity => Options.BeltCapacity.Value;
    private static bool StartWithBombs => Options.StartWithBombs.Value;

    public RainWorld RwInstance;

    public Sainot()
    {
        try
        {
            Options = new SainotOptions(this, Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
    
    private void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
    }

    private bool IsInit;
    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (IsInit) return;
            
            On.Player.ctor += PlayerOnctor;
            On.Player.GrabUpdate += PlayerOnGrabUpdate;
            IL.Player.GrabUpdate += PlayerILGrabUpdate;
            On.Player.Grabability += PlayerOnGrabability;
            On.Player.GraphicsModuleUpdated += PlayerOnGraphicsModuleUpdated;
            On.PlayerGraphics.InitiateSprites += PlayerGraphicsOnInitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphicsOnDrawSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphicsOnAddToContainer;
            On.PlayerGraphics.Update += PlayerGraphicsOnUpdate;
            On.Player.NewRoom += PlayerOnNewRoom;
            On.Player.Update += PlayerOnUpdate;
            IL.Player.ThrowObject += PlayerOnThrowObject;
            On.SlugcatStats.ctor += SlugcatStatsOnctor;

            IL.SeedCob.HitByWeapon += SeedCobOnHitByWeapon;

            On.RainWorldGame.ShutDownProcess += RainWorldGameOnShutDownProcess;
            On.GameSession.ctor += GameSessionOnctor;

            RwInstance = self;

            MachineConnector.SetRegisteredOI("NoirCatto.Sainot", Options);
            IsInit = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }

    private void RainWorldGameOnShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
    {
        orig(self);
        ClearMemory();
    }
    private void GameSessionOnctor(On.GameSession.orig_ctor orig, GameSession self, RainWorldGame game)
    {
        orig(self, game);
        ClearMemory();
    }

    #region Helpers
    private void ClearMemory()
    {
        BombBelts.Clear();
        HeadRags.Clear();
    }

    private bool IsBeltEnabled(Player self)
    {
        return ModManager.MSC && self.slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Saint && Rambo;
    }
    #endregion
}
