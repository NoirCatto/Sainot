using BepInEx.Logging;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace Sainot;

public class SainotOptions : OptionInterface
{
    private readonly ManualLogSource Logger;
    public SainotOptions(Sainot modInstance, ManualLogSource loggerSource)
    {
        Logger = loggerSource;
        Rambo = this.config.Bind<bool>("Rambo", true);
        StartWithBombs = this.config.Bind<bool>("StartWithBombs", true);
        BeltCapacity = this.config.Bind<int>("BeltCapacity", 3, new ConfigAcceptableRange<int>(0, 100));
    }

    public readonly Configurable<bool> Rambo;
    public readonly Configurable<bool> StartWithBombs;
    public readonly Configurable<int> BeltCapacity;
    private UIelement[] UIArrOptions;
    private UIelement[] UIArrRambo;

    public override void Initialize()
    {
        var opTab = new OpTab(this, "Options");
        this.Tabs = new[]
        {
            opTab
        };

        UIArrOptions = new UIelement[]
        {
            new OpLabel(10f, 550f, "Rambo", true),
            new OpCheckBox(Rambo, 10f, 520f),
            new OpLabel(40f, 520f, "Turns Sain't into Rambo") { verticalAlignment = OpLabel.LabelVAlignment.Center }
        };
        opTab.AddItems(UIArrOptions);

        UIArrRambo = new UIelement[]
        {
            new OpLabel(10f, 470f, "Rambo", true){color = new Color(0.65f, 0.1f, 0.1f)},
            new OpCheckBox(StartWithBombs, 10f, 440f),
            new OpLabel(40f, 440f, "Start cycle with BombBelt resupplied"),
            
            new OpLabel(10f, 410f, "BombBelt capacity (default = 3)"),
            new OpUpdown(BeltCapacity, new Vector2(10f, 380f), 100f),
            
        };
        opTab.AddItems(UIArrRambo);
    }

    public override void Update()
    {
        if (!((OpCheckBox)UIArrOptions[1]).GetValueBool())
        {
            foreach (var element in UIArrRambo)
            {
                element.Hide();
            }
        }
        else
        {
            foreach (var element in UIArrRambo)
            {
                element.Show();
            }
        }
    }
    
    
}