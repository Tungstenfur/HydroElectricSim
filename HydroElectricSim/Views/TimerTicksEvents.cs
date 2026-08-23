using System;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace HydroElectricSim.Views;

public partial class MainView
{
    private void HydraulicTick()
    {
        oilTempValue+= production / 180;
        if (OilCoolPump.IsChecked == true && isCoolingActive())
            oilTempValue -= isFilterClogged()? flowrate / 20: flowrate/50;
        OilTemp.Content = $"Oil Temperature: {oilTempValue:F0}C";
        OilTempBar.Value = oilTempValue;
        if(oilTempValue<10) {TripTurbine("Low oil temperature");
            oilTempValue = 15;
            OilCoolPump.IsChecked = false;
        }
        if(oilTempValue>70) {TripTurbine("High oil temperature");
                oilTempValue = 65;
                OilCoolPump.IsChecked = true;
        }
        
    }
    private void GeneratorTick()
    {
        if(GenPreheat.IsChecked == true&& statorTemp<120)
        {
            statorTemp += 0.4*Random.Shared.NextDouble();
            GenTemp.Content = $"Stator Temperature: {statorTemp:F1}C";
            GenTempBar.Value = statorTemp;
        }
        else if(GenCoolant.IsChecked == true && statorTemp>15&&isCoolingActive())
        {
            statorTemp -= 0.4*Random.Shared.NextDouble();
            GenTemp.Content = $"Stator Temperature: {statorTemp:F1}C";
            GenTempBar.Value = statorTemp;
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
            if (miv && !Brake.IsChecked == true)
            {
                rpm = Turbine.GetTurbineSpeed(rpm, wicketPosition);
                RpmLabel.Content = $"Turbine Speed: {rpm:F1} rpm";
                RpmBar.Value = rpm;
            }
            else if (Brake.IsChecked == true)
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
            if(Brake.IsChecked==true) TripTurbine("Brake engaged");
            if(GenTempBar.Value<50) TripTurbine("Generator Short circuit");
            if(GenTempBar.Value>90) TripTurbine("Generator Overheat");
            production=Turbine.GetTurbineOutput(wicketPosition,trashRackFill);
            prodLabel.Content = $"Power Output: {production:F2} MW";
        }
    }
    private int lubFailTicks = 0;
    private void LubricationTick()
    {
        if (LubPump.IsChecked == true)
        {
            if (Preheater.IsChecked == true)
            {
                lubTemperature += 0.1;
            }

            if (lubTemperature > 38)
            {
                lubrication = true;
                lubTemperature += 0.05;
                PumpStatus.Content="Pump running";
            }
            else
            {
                lubrication = false;
                PumpStatus.Content = "Pump not running!";
            }

            if (lubTemperature > 48)
            {
                Fan.IsChecked = true;
                LubPump.IsChecked = false;
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Error!", "Lubrication pump tripped! Overheat!", ButtonEnum.Ok);
                box.ShowAsPopupAsync(this);
                
            }
        }
        else
        {
            lubrication = false;
            PumpStatus.Content = "Pump not running!";
        }
        if(Fan.IsChecked==true)
        {
            lubTemperature -= 0.1;
            if (lubTemperature < 20)
            {
                Fan.IsChecked = false;
            }
        }

        if (!lubrication&&rpm>20)
        {
            lubFailTicks++;
            if (lubFailTicks > 100)
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
        LubTempBar.Value = lubTemperature;
    }
    private void TrashTick()
    {
        if (trashRackFill<100&&miv)
        {
            trashRackFill+=0.4;
            TrashRackPressureBar.Value = trashRackFill;
        }
    }


    
}