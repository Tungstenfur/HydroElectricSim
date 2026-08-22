using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace HydroElectricSim.Views;

public partial class MainView
{
    private void HydraulicTick()
    {
        oilTempValue+= Production / 180;
        if (oilCoolPump.IsChecked == true && isCoolingActive())
            oilTempValue -= isFilterClogged()? Flowrate / 20: Flowrate/50;
        oilTemp.Content = $"Oil Temperature: {oilTempValue:F0}C";
        oilTempBar.Value = oilTempValue;
        if(oilTempValue<10) {TripTurbine("Low oil temperature");
            oilTempValue = 15;
            oilCoolPump.IsChecked = false;
        }
        if(oilTempValue>70) {TripTurbine("High oil temperature");
                oilTempValue = 65;
                oilCoolPump.IsChecked = true;
        }
        
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
    private void TurbineTick()
    {
        if (!syncState)
        {
            if (rpm > 300)
            {
                TripTurbine("Overspeed detected");
            }
            if(Turbine.GetSpeedStdDev()>7&&rpm!=0)
            {
                TripTurbine("Turbine vibration high");
            }
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
    int lubFailTicks = 0;
    private void LubricationTick()
    {
        if (lubPump.IsChecked == true)
        {
            if (preheater.IsChecked == true)
            {
                lubTemperature += 0.1;
            }

            if (lubTemperature > 38)
            {
                Lubrication = true;
                lubTemperature += 0.05;
            }
            else Lubrication = false;

            if (lubTemperature > 48)
            {
                fan.IsChecked = true;
                lubPump.IsChecked = false;
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Error!", "Lubrication pump tripped! Overheat!", ButtonEnum.Ok);
                box.ShowAsPopupAsync(this);
                
            }
        }
        else
        {
            Lubrication = false;
        }
        if(fan.IsChecked==true)
        {
            lubTemperature -= 0.1;
            if (lubTemperature < 20)
            {
                fan.IsChecked = false;
            }
        }

        if (!Lubrication&&rpm>20)
        {
            lubFailTicks++;
            if (lubFailTicks > 20)
            {
                TripTurbine("Lubrication failure");
                lubFailTicks = 0;
            }
        }
        else
        {
            lubFailTicks = 0;
        }
        LubTemperature.Content = $"Temperature: {lubTemperature:F1}C";
        lubTempBar.Value = lubTemperature;
    }

    private async void ChangeFilterA_OnClick(object? sender, RoutedEventArgs e)
    {
        if (filterA.IsChecked == true) filterOff.IsChecked = true;
        filterA.IsEnabled = false;
        await Task.Delay(30000);
        filterAClog = false;
        filterA.IsEnabled = true;
    }

    private async void ChangeFilterB_OnClick(object? sender, RoutedEventArgs e)
    {
        if (filterB.IsChecked == true) filterOff.IsChecked = true;
        filterB.IsEnabled = false;
        await Task.Delay(30000);
        filterBClog = false;
        filterB.IsEnabled = true;
    }
}