using System;
using System.Collections.Generic;

namespace HydroElectricSim;

internal static class Turbine
{
    private static List<double> speedHistory = new();
    internal static double GetTurbineSpeed(double speed, double wicketPosition)
    {
        if(wicketPosition<0.1)return speed-(speed/200);
        //https://www.desmos.com/calculator/4v6lmt0y3w
        return speed+(30*wicketPosition/(speed+2));
    }
    internal static double GetTurbineOutput(double wicketPosition, double trashRackFill)
    {
        return Math.Max((wicketPosition*0.2-(10*0.15)*GetTrashRackPenalty(trashRackFill)),0);
    }
    internal static void UpdateSpeedHistory(double speed)
    {
        speedHistory.Add(speed);
        if (speedHistory.Count > 10)
        {
            speedHistory.RemoveAt(0);
        }
    }

    internal static double GetSpeedStdDev()
    {
        if (speedHistory.Count == 0) return 0;
        double mean = 0;
        foreach (double speed in speedHistory)
            mean += speed;
        mean/= speedHistory.Count;
        double variance = 0;
        foreach (double speed in speedHistory)
            variance += (speed - mean) * (speed - mean);
        variance/=speedHistory.Count;
        return Math.Sqrt(variance);
    }
    internal static double GetTrashRackPenalty(double trashRackFill)
    {
        return Math.Min((-0.015*trashRackFill)+2,1);
    }
}