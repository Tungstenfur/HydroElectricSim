using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace HydroElectricSim.Views;

public partial class MainView
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
        if(Math.Abs(Demand-Production)<0.5) infoList.Add(getInfoLabel("Demand meet!", infoType.Info));
        if (StatorTemp < 60) infoList.Add(getInfoLabel("Low Stator Temperature", infoType.Warn));
        if (StatorTemp > 90) infoList.Add(getInfoLabel("High Stator Temperature", infoType.Error));
        if (TurbineFilling) infoList.Add(getInfoLabel("Turbine Filling", infoType.Info));
        if (oilTempValue<20) infoList.Add(getInfoLabel("Low Oil Temperature", infoType.Warn));
        if (oilTempValue>60) infoList.Add(getInfoLabel("High Oil Temperature,", infoType.Error));
        if (Turbine.GetSpeedStdDev() > 5 && rpm != 0) infoList.Add(getInfoLabel("Turbine vibration high", infoType.Error));
        if(rpm>100&&elecPump.IsChecked==true) infoList.Add(getInfoLabel("Aux pump transfer", infoType.Error));
        if(filterAClog) infoList.Add(getInfoLabel("Filter A clogged", infoType.Warn));
        if(filterBClog) infoList.Add(getInfoLabel("Filter B clogged", infoType.Warn));
        if(elecPump.IsChecked==false&&rpm<100) infoList.Add(getInfoLabel("No oil pump running!", infoType.Error));
        if(filterOff.IsChecked==true) infoList.Add(getInfoLabel("Filter bypassed", infoType.Error));
        if(StatorTemp>80) infoList.Add(getInfoLabel("High Stator Temperature", infoType.Error));
        if(!Lubrication) infoList.Add(getInfoLabel("No lubrication!", infoType.Error));
        if(lubTemperature<40) infoList.Add(getInfoLabel("Low lubrication temperature", infoType.Warn));
        if(lubTemperature>45) infoList.Add(getInfoLabel("High lubrication temperature", infoType.Warn));

        Infos.Children.Clear();
        foreach (Label label in infoList)
        {
            Infos.Children.Add(label);
        }
    }


    private void CoolantTrip_OnClick(object? sender, RoutedEventArgs e)
    {
        coolantValve.IsChecked = false;
        loop1.IsChecked = false;
        loop2.IsChecked = false;
    }
}
