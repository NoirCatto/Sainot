using System;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using BepInEx;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Sainot;

[BepInPlugin("NoirCatto.Sainot", "Sain't", "1.0.0")]
public partial class Sainot : BaseUnityPlugin
{
    public static SainotOptions ModOptions;

    public static bool Rambo => ModOptions.Rambo.Value;
    public static bool StartWithBombs => ModOptions.StartWithBombs.Value;
    public static int BeltCapacity => ModOptions.BeltCapacity.Value;

    public static bool Bandanas; //NoirCatto.Bandanas

    public Sainot()
    {
        try
        {
            ModOptions = new SainotOptions(this, Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
    
    private void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
        On.RainWorld.PostModsInit += RainWorldOnPostModsInit;
    }

    private bool _isInit;
    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        if (_isInit) return;

        try
        {
            _isInit = true;
            On.SlugcatStats.ctor += SlugcatStatsOnctor;
            On.Player.ctor += PlayerOnctor;
            On.Player.NewRoom += PlayerOnNewRoom;
            On.Player.GrabUpdate += PlayerOnGrabUpdate;
            IL.Player.GrabUpdate += PlayerILGrabUpdate;
            On.Player.Grabability += PlayerOnGrabability;

            On.Player.GraphicsModuleUpdated += PlayerOnGraphicsModuleUpdated;
            On.PlayerGraphics.InitiateSprites += PlayerGraphicsOnInitiateSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphicsOnAddToContainer;
            On.PlayerGraphics.DrawSprites += PlayerGraphicsOnDrawSprites;
            On.PlayerGraphics.Update += PlayerGraphicsOnUpdate;

            On.Room.AddObject += RoomOnAddObject;

            IL.Player.ThrowObject += PlayerOnThrowObject;
            IL.SeedCob.HitByWeapon += SeedCobOnHitByWeapon;

            MachineConnector.SetRegisteredOI("NoirCatto.Sainot", ModOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private bool _isPostInit;
    private void RainWorldOnPostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);
        if (_isPostInit) return;
        
        try
        {
            _isPostInit = true;

            if (ModManager.ActiveMods.Any(x => x.id == "NoirCatto.Bandanas"))
            {
                Bandanas = true;
                Logger.LogInfo("Sain't: Bandanas mod detected, ignoring local...");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

}
