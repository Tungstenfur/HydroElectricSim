using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace HydroElectricSim.Views;

public partial class MainView : UserControl
{
    private readonly Dictionary<infoType, Color> colors = new()
    {
        { infoType.Info, Colors.Blue },
        { infoType.Warn, Colors.Orange },
        { infoType.Error, Colors.Red }
    };

    private Label getInfoLabel(string info, infoType type)
    {
        Label label = new();
        label.FontSize = 18;
        label.Content = info;
        label.Foreground = new SolidColorBrush(colors[type]);
        return label;
    }
    private void infoUpdate()
    {
        List<Label> infoList = new();
        if (StatorTemp < 60) infoList.Add(getInfoLabel("Low Stator Temperature", infoType.Warn));
        if (StatorTemp > 90) infoList.Add(getInfoLabel("High Stator Temperature", infoType.Error));
        if (TurbineFilling) infoList.Add(getInfoLabel("Turbine Filling", infoType.Info));
        Infos.Children.Clear();
        foreach (Label label in infoList)
        {
            Infos.Children.Add(label);
        }
    }

    
}
