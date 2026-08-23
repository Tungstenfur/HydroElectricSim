using System;
using System.Threading.Tasks;
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
    double flowrate=0.2;
    double statorTemp=30;
    double wicketPosition=0;
    double demand=0;
    double production=0;
    double rpm=0;
    double lubTemperature=20;
    double oilTempValue=15;
    bool turbineFilling = false;
    bool miv=false;
    bool elecOilPump=false;
    bool mainOilPump=false;
    bool syncState = false;
    bool filterAClog=false;
    bool filterBClog=false;
    bool lubrication=false;
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
        demand=Random.Shared.NextDouble() * (15 - 1) + 1;
        demandLabel.Content=$"Demand: {demand:F2} MW";
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
        if(FilterA.IsChecked==true&&filterAClog) return true;
        if(FilterA.IsChecked==true&&filterBClog) return true;
        if (FilterA.IsChecked == true) return true;
        return false;
    }
    private void Flowrate_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        flowrate = Math.Round(e.NewValue, 2);
        FlowrateLabel.Content = $"Flowrate: {flowrate} m³/s";
    }
    private void GenPreheat_Checked(object? sender, RoutedEventArgs e)
    {
        GenCoolant.IsChecked = false;
    }
    private void GenCoolant_Checked(object? sender, RoutedEventArgs e)
    {
        GenPreheat.IsChecked = false;
    }

    private async void TurbineFill_Click(object? sender, RoutedEventArgs e)
    {
        bool isRunning = await isOilPumpRunning();
        if (!isRunning)return;
        turbineFilling=true;
        TurbineFill.IsEnabled=false;
        await Task.Delay(5000);
        MivOpen.IsEnabled=true;
        turbineFilling=false;
    }
    private async void MIVopen_Click(object? sender, RoutedEventArgs e)
    {
        bool isRunning = await isOilPumpRunning();
        if (!isRunning)return;
        MivOpen.Content = "MIV Opened";
        MivOpen.IsEnabled=false;
        miv=true;
    }

    


    private async Task<bool> isOilPumpRunning()
    {
        if(ElecPump.IsChecked==true || mainOilPump)
        {
            return true;
        }

        var box = MessageBoxManager
            .GetMessageBoxStandard("Error!", "Unable to open valve! Is the oil pump running?");
        await box.ShowAsPopupAsync(this);
        return false;
    }
    private bool isCoolingActive()
    {
        return CoolantValve.IsChecked==true&&Loop1.IsChecked==true&&Loop2.IsChecked==true;
    }
    private void checkSyncStatus()
    {
        Sync.IsEnabled=false;
        if (rpm < 245)
        {
            SyncStatus.Content="Cant sync: too slow!";
            return;
        }
        if(rpm>255)
        {
            SyncStatus.Content="Cant sync: too fast!";
            return;
        }
        if(Turbine.GetSpeedStdDev()>0.5)
        {
            SyncStatus.Content="Cant sync: unstable speed!"+$" (StdDev: {Turbine.GetSpeedStdDev():F2}>0.5)";
            return;
        }
        SyncStatus.Content="Ready to sync!";
        Sync.IsEnabled=true;
        
        
    }

    private void wicketOpenFine_Click(object? sender, RoutedEventArgs e)
    {
        if(wicketPosition+0.1>=100)
            wicketPosition=100;
        else
            wicketPosition+=0.1;
        UpdateWicketPosition();
    }

    private void wicketCloseFine_Click(object? sender, RoutedEventArgs e)
    {
        if (wicketPosition - 0.1 <= 0)
            wicketPosition = 0;
        else
            wicketPosition -= 0.1;
        UpdateWicketPosition();
    }
    private void UpdateWicketPosition()
    {
        WicketPositionLabel.Content = $"Wicket gates: {wicketPosition:F1}%";
        WicketGates.Value = wicketPosition;
    }

    private void WicketGates_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        wicketPosition = e.NewValue;
        UpdateWicketPosition();
    }

    private void Sync_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        syncState = Sync.IsChecked==true;
        if (syncState)
        {
            rpm = 250;
            RpmLabel.Content = $"Turbine Speed: {rpm:F1} rpm";
        }
    }
    private void TripTurbine(string reason)
    {
        syncState=false;
        Sync.IsChecked=false;
        Brake.IsChecked=true;
        wicketPosition=0;
        WicketGates.Value=0;
        WicketPositionLabel.Content = $"Wicket gates: {wicketPosition:F1}%";
        rpm=0;
        SyncStatus.Content="Cant sync: too slow!";
        var box = MessageBoxManager
            .GetMessageBoxStandard("Trip!", $"Turbine tripped: {reason}", ButtonEnum.Ok);
        box.ShowAsPopupAsync(this);
    }

    private void TurbineTrip_OnClick(object? sender, RoutedEventArgs e)
    {
        TripTurbine("Manual trip initiated.");
    }
}