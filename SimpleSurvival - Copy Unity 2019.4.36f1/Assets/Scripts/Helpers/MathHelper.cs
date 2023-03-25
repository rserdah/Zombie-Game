using System;
using UnityEngine;

public static class MathHelper
{
    /// <summary>
    /// Loops value so that it is never less than zero and never greater than or equal to max. Use for indices where only 0 through length - 1 are valid indices (Arrays, etc.).
    /// </summary>
    /// <param name="value"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int LoopInt(int value, int max)
    {
        if(value < 0)
            return max - 1;
        else if(value >= max)
            return 0;
        else
            return value;
    }

    /// <summary>
    /// If value is less than or equal to min, will return max, if it is greater than or equal to max, it will return min. Else, it will return value.
    /// Used for scrolling through a range of values and going back to the beginning of the range when the end is reached and vice versa.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int LoopInt(int value, int min, int max)
    {
        if(value <= min)
            return max;
        else if(value >= max)
            return min;
        else return value;
    }

    /// <summary>
    /// !!! Check to verify functionality !!!
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="minInclusive"></param>
    /// <param name="maxInclusive"></param>
    /// <returns></returns>
    public static int LoopInt(int value, int min, int max, bool minInclusive, bool maxInclusive)
    {
        if(minInclusive)
        {
            if(value <= min)
                return max;
        }
        else
        {
            if(value < min + 1)
                return max;
        }

        if(maxInclusive)
        {
            if(value >= max)
                return min;
        }
        else
        {
            if(value > max - 1)
                return min;
        }

        return value;
    }

    /// <summary>
    /// Truncates value so it has decimalPlaces number of decimal places (for example Truncate(1.125, 2) = 1.12)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="decimalPlaces"></param>
    /// <returns></returns>
    public static float Truncate(float value, int decimalPlaces)
    {
        decimal dec = (decimal)value;
        decimal step = (decimal)Mathf.Pow(10, decimalPlaces);
        dec = Math.Truncate(dec * step);
        return (float)dec / (float)step;
    }

    public static bool ApproximatelyEqual(float a, float b, float error)
    {
        return Mathf.Abs(b - a) <= error;
    }

    public static bool Even(int x)
    {
        return x % 2 == 0;
    }

    /// <summary>
    /// Simple function to return a bool based on a given percent chance. The float parameter percent is given as a percent out of 100 so if percent >= 100, then true is guaranteed (i.e. >=100%), 
    /// else the returned bool has a percent% chance of being true.
    /// </summary>
    /// <param name="percent"></param>
    /// <returns></returns>
    public static bool PercentChance(float percent)
    {
        if(percent >= 100f) return true;
        if(percent <= 0f) return false;

        float x = UnityEngine.Random.Range(0f, 100f);

        return x <= percent;
    }
} //MathHelper
