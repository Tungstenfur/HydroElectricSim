using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace HydroElectricSim.Views;

public partial class MainView : UserControl
{
    DispatcherTimer _timer;
    DispatcherTimer _demandTimer;
    double Flowrate=0.2;
    double StatorTemp=30;
    double WicketPosition=0;
    double Demand=0;
    double Production=0;
    double rpm=0;
    double lubTemperature=20;
    double oilTempValue=15;
    bool TurbineFilling = false;
    bool Miv=false;
    bool ElecOilPump=false;
    bool MainOilPump=false;
    bool syncState = false;
    bool filterAClog=false;
    bool filterBClog=false;
    bool Lubrication=false;
    public MainView()
    {
        InitializeComponent();
        _timer = new();
        _timer.Interval = TimeSpan.FromMilliseconds(200);
        _timer.Tick+= Timer_Tick;
        _timer.Start();
        _demandTimer=new();
        _demandTimer.Interval=TimeSpan.FromSeconds(45);
        _demandTimer.Tick+= DemandTimerOnTick;
        _demandTimer.Start();
        DemandTimerOnTick(null, null);
    }

    private void DemandTimerOnTick(object? sender, EventArgs e)
    {
        Demand=Random.Shared.NextDouble() * (15 - 1) + 1;
        demandLabel.Content=$"Demand: {Demand:F2} MW";
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        GeneratorTick();
        TurbineTick();
        HydraulicTick();
        LubricationTick();
        infoUpdate();
        if(Random.Shared.NextDouble() < 0.001) filterAClog=true;
        if(Random.Shared.NextDouble() < 0.001) filterBClog=true;
    }

    private bool isFilterClogged()
    {
        if(filterA.IsChecked==true&&filterAClog) return true;
        if(filterB.IsChecked==true&&filterBClog) return true;
        if (filterOff.IsChecked == true) return true;
        return false;
    }
    private void Flowrate_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        Flowrate = Math.Round(e.NewValue, 2);
        flowrateLabel.Content = $"Flowrate: {Flowrate} m³/s";
    }
    private void GenPreheat_Checked(object? sender, RoutedEventArgs e)
    {
        genCoolant.IsChecked = false;
    }
    private void GenCoolant_Checked(object? sender, RoutedEventArgs e)
    {
        genPreheat.IsChecked = false;
    }

    private async void TurbineFill_Click(object? sender, RoutedEventArgs e)
    {
        bool isRunning = await isOilPumpRunning();
        if (!isRunning)return;
        TurbineFilling=true;
        TurbineFill.IsEnabled=false;
        await Task.Delay(5000);
        MIVopen.IsEnabled=true;
        TurbineFilling=false;
    }
    private async void MIVopen_Click(object? sender, RoutedEventArgs e)
    {
        bool isRunning = await isOilPumpRunning();
        if (!isRunning)return;
        MIVopen.Content = "MIV Opened";
        MIVopen.IsEnabled=false;
        Miv=true;
    }

    


    private async Task<bool> isOilPumpRunning()
    {
        if(elecPump.IsChecked==true || MainOilPump)
        {
            return true;
        }
        else
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Error!", "Unable to open valve! Is the oil pump running?", ButtonEnum.Ok);
            await box.ShowAsPopupAsync(this);
            return false;
        }
    }
    private bool isCoolingActive()
    {
        return coolantValve.IsChecked==true&&loop1.IsChecked==true&&loop2.IsChecked==true;
    }
    private void checkSyncStatus()
    {
        sync.IsEnabled=false;
        if (rpm < 245)
        {
            syncStatus.Content="Cant sync: too slow!";
            return;
        }
        if(rpm>255)
        {
            syncStatus.Content="Cant sync: too fast!";
            return;
        }
        if(Turbine.GetSpeedStdDev()>0.5)
        {
            syncStatus.Content="Cant sync: unstable speed!"+$" (StdDev: {Turbine.GetSpeedStdDev():F2}>0.5)";
            return;
        }
        syncStatus.Content="Ready to sync!";
        sync.IsEnabled=true;
        
        
    }

    private void wicketOpenFine_Click(object? sender, RoutedEventArgs e)
    {
        if(WicketPosition+0.1>=100)
            WicketPosition=100;
        else
            WicketPosition+=0.1;
        UpdateWicketPosition();
    }

    private void wicketCloseFine_Click(object? sender, RoutedEventArgs e)
    {
        if (WicketPosition - 0.1 <= 0)
            WicketPosition = 0;
        else
            WicketPosition -= 0.1;
        UpdateWicketPosition();
    }
    private void UpdateWicketPosition()
    {
        wicketPositionLabel.Content = $"Wicket gates: {WicketPosition:F1}%";
        wicketGates.Value = WicketPosition;
    }

    private void WicketGates_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        WicketPosition = e.NewValue;
        UpdateWicketPosition();
    }

    private void Sync_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        syncState = sync.IsChecked==true;
        if (syncState)
        {
            rpm = 250;
            RpmLabel.Content = $"Turbine Speed: {rpm:F1} rpm";
        }
    }
    private void TripTurbine(string reason)
    {
        syncState=false;
        sync.IsChecked=false;
        brake.IsChecked=true;
        WicketPosition=0;
        wicketGates.Value=0;
        wicketPositionLabel.Content = $"Wicket gates: {WicketPosition:F1}%";
        rpm=0;
        syncStatus.Content="Cant sync: too slow!";
        var box = MessageBoxManager
            .GetMessageBoxStandard("Trip!", $"Turbine tripped: {reason}", ButtonEnum.Ok);
        box.ShowAsPopupAsync(this);
    }

    private void TurbineTrip_OnClick(object? sender, RoutedEventArgs e)
    {
        TripTurbine("Manual trip initiated.");
    }
}