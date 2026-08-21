using System;
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
    double Flowrate=0.2;
    double StatorTemp=30;
    bool TurbineFilling = false;
    bool Miv=false;
    bool ElecOilPump=false;
    bool MainOilPump=false;
    
    public MainView()
    {
        InitializeComponent();
        _timer = new();
        _timer.Interval = TimeSpan.FromMilliseconds(200);
        _timer.Tick+= Timer_Tick;
        _timer.Start();
    }
    private void Timer_Tick(object? sender, EventArgs e)
    {
        GeneratorTick();
        infoUpdate();
    }

    private void GeneratorTick()
    {
        if(genPreheat.IsChecked == true&& StatorTemp<120)
        {
            StatorTemp += 0.2;
            genTemp.Content = $"Stator Temperature: {StatorTemp:F1}C";
            genTempBar.Value = StatorTemp;
        }
        else if(genCoolant.IsChecked == true && StatorTemp>15)
        {
            StatorTemp -= 0.2;
            genTemp.Content = $"Stator Temperature: {StatorTemp:F1}C";
            genTempBar.Value = StatorTemp;
        }
    }
    private void Flowrate_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        Flowrate = Math.Round(e.NewValue, 2);
        flowrateLabel.Content = $"Flowrate: {Flowrate} m³/s";
    }
    private void GenPreheat_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        genCoolant.IsChecked = false;
    }
    private void GenCoolant_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
}