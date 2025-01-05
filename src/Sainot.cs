using System;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using MoreSlugcats;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Sainot;

[BepInPlugin("NoirCatto.Sainot", "Sain't", "1.0.0")]
public partial class Sainot : BaseUnityPlugin
{
    public static SainotOptions Options;

    public Sainot()
    {
        try
        {
            Options = new SainotOptions(this, Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
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

            MachineConnector.SetRegisteredOI("NoirCatto.Sainot", Options);
            IsInit = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
