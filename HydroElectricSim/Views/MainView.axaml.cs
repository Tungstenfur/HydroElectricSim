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
    double oilTempValue=15;
    bool TurbineFilling = false;
    bool Miv=false;
    bool ElecOilPump=false;
    bool MainOilPump=false;
    bool syncState = false;
    
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
        infoUpdate();
    }

    private void HydraulicTick()
    {
        oilTempValue+= Production / 180;
        if (oilCoolPump.IsChecked == true && isCoolingActive())
            oilTempValue -= Flowrate / 40;
        oilTemp.Content = $"Oil Temperature: {oilTempValue:F0}C";
        oilTempBar.Value = oilTempValue;
    }
    private void GeneratorTick()
    {
        if(genPreheat.IsChecked == true&& StatorTemp<120)
        {
            StatorTemp += 0.4*Random.Shared.NextDouble();
            genTemp.Content = $"Stator Temperature: {StatorTemp:F1}C";
            genTempBar.Value = StatorTemp;
        }
        else if(genCoolant.IsChecked == true && StatorTemp>15&&isCoolingActive())
        {
            StatorTemp -= 0.4*Random.Shared.NextDouble();
            genTemp.Content = $"Stator Temperature: {StatorTemp:F1}C";
            genTempBar.Value = StatorTemp;
        }
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

    private void TurbineTick()
    {
        if (!syncState)
        {
            if (Miv && !brake.IsChecked == true)
            {
                rpm = Turbine.GetTurbineSpeed(rpm, WicketPosition);
                RpmLabel.Content = $"Turbine Speed: {rpm:F1} rpm";
                RpmBar.Value = rpm;
            }
            else if (brake.IsChecked == true)
            {
                rpm -= 0.5;
                if (rpm < 0) rpm = 0;
                RpmLabel.Content = $"Turbine Speed: {rpm:F1} rpm";
                RpmBar.Value = rpm;
            }

            Turbine.UpdateSpeedHistory(rpm);
            checkSyncStatus();
        }
        else
        {
            Production=Turbine.GetTurbineOutput(WicketPosition);
            prodLabel.Content = $"Power Output: {Production:F2} MW";
        }
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
}