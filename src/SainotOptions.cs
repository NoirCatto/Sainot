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
        Rambo = this.config.Bind<bool>(nameof(Rambo), true);
        StartWithBombs = this.config.Bind<bool>(nameof(StartWithBombs), true);
        BeltCapacity = this.config.Bind<int>(nameof(BeltCapacity), 3, new ConfigAcceptableRange<int>(0, 100));
        HeadRagColor = this.config.Bind<Color>(nameof(HeadRagColor), new Color(0.8f, 0.05f, 0.04f));
        HeadRagUseColor = this.config.Bind<bool>(nameof(HeadRagUseColor), false);
        HeadRagColorRainbow = this.config.Bind<bool>(nameof(HeadRagColorRainbow), false);
        HeadRagLength = this.config.Bind<int>(nameof(HeadRagLength), 5, new ConfigAcceptableRange<int>(3, 15));
    }

    public readonly Configurable<bool> Rambo;
    public readonly Configurable<bool> StartWithBombs;
    public readonly Configurable<int> BeltCapacity;
    public readonly Configurable<Color> HeadRagColor;
    public readonly Configurable<bool> HeadRagUseColor;
    public readonly Configurable<bool> HeadRagColorRainbow;
    public readonly Configurable<int> HeadRagLength;
    private UIelement[] UIArrOptions;
    private UIelement[] UIArrRambo;
    private UIelement[] UIArrHeadband;
    private UIelement[] UIArrColors;
    private OpCheckBox RamboCheckBox;
    private OpCheckBox HeadRagUseColorCheckBox;
    private OpCheckBox HeadRagColorRainbowCheckBox;
    private OpColorPicker HeadRagColorColorPicker;
    private OpLabel BandanasWarning;
    public override void Initialize()
    {
        var opTab = new OpTab(this, "Options");
        this.Tabs = new[]
        {
            opTab
        };

        RamboCheckBox = new OpCheckBox(Rambo, 10f, 520f);
        UIArrOptions = new UIelement[]
        {
            new OpLabel(10f, 550f, "Rambo", true),
            RamboCheckBox,
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

        HeadRagUseColorCheckBox = new OpCheckBox(HeadRagUseColor, 10f, 280f);
        UIArrHeadband = new UIelement[]
        {
            new OpLabel(10f, 340f, "Headband length (default = 5)"),
            new OpSlider(HeadRagLength, new Vector2(10f, 310f), 100),

            new OpLabel(40f, 280f, "Custom headband color"),
            HeadRagUseColorCheckBox,
        };
        opTab.AddItems(UIArrHeadband);

        HeadRagColorRainbowCheckBox = new OpCheckBox(HeadRagColorRainbow, 10f, 250f);
        HeadRagColorColorPicker = new OpColorPicker(HeadRagColor, new Vector2(10f, 90f));
        UIArrColors = new UIelement[]
        {
            HeadRagColorRainbowCheckBox,
            new OpLabel(40f, 250f, "Rainbow headband"),
            HeadRagColorColorPicker,
        };
        opTab.AddItems(UIArrColors);

        BandanasWarning = new OpLabel(150f, 250f, "Bandanas mod overrules Saint's headband settings."){ color = Color.red};
        opTab.AddItems(BandanasWarning);
    }

    public override void Update()
    {
        if (!RamboCheckBox.GetValueBool())
        {
            foreach (var element in UIArrRambo)
                element.Hide();
            foreach (var element in UIArrHeadband)
                element.Hide();
            foreach (var element in UIArrColors)
                element.Hide();
            BandanasWarning.Hide();
        }
        else
        {
            foreach (var element in UIArrRambo)
                element.Show();

            if (!Sainot.Bandanas)
            {
                foreach (var element in UIArrHeadband)
                    element.Show();
                BandanasWarning.Hide();

                if (HeadRagUseColorCheckBox.GetValueBool())
                {
                    foreach (var element in UIArrColors)
                        element.Show();
                }
                else
                {
                    foreach (var element in UIArrColors)
                        element.Hide();
                }
            }
            else
            {
                foreach (var element in UIArrHeadband)
                    element.Hide();
                foreach (var element in UIArrColors)
                    element.Hide();
                BandanasWarning.Show();
            }
        }
    }
    
    
}