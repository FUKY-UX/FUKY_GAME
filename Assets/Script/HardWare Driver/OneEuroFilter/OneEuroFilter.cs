﻿using System;
using UnityEngine;

public class OneEuroFilter : MonoBehaviour
{
    private float minCutoff;
    private float beta;
    private float dCutoff;
    private float xPrev;
    private float dxPrev;
    private float tPrev;
    /// <summary>
    /// <para>使用该方法调整滤波效果</para>在需要快速响应的应用中，可能需要增加beta和/或dCutoff的值；<para></para>而在需要高度平滑输出的应用中，可能需要增加minCutoff的值
    /// </summary>
    /// <param name="minCutoff">较低的minCutoff值会导致更多的高频噪声通过，而较高的值则会使输出更加平滑</param>
    /// <param name="beta">增加beta会使滤波器在快速移动时更加响应，但也可能引入更多的高频噪声。<para></para>减小beta则会使滤波器更加平滑，但可能导致在快速移动时响应滞后。</param>
    /// <param name="dCutoff">较高的dCutoff值会使滤波器对速度变化更加敏感，而较低的值则会使输出在速度变化时更加平滑</param>
    public void UpdateSetting(float _minCutoff, float _beta, float _dCutoff)
    {
        minCutoff = _minCutoff;
        beta = _beta;
        dCutoff = _dCutoff;
    }

    public OneEuroFilter(float t0, float x0, float dx0 = 0.0f, float minCutoff = 1.0f, float beta = 0.0f, float dCutoff = 1.0f)
    {
        // The parameters.
        this.minCutoff = minCutoff;
        this.beta = beta;
        this.dCutoff = dCutoff;
        // Previous values.
        this.xPrev = x0;
        this.dxPrev = dx0;
        this.tPrev = t0;
    }
    /// <summary>
    /// Dt是Δtime，x是采样值
    /// </summary>
    /// <param name="Dt"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public float Compute(float Dt, float x)
    {
        float tE = Dt;

        // The filtered derivative of the signal.
        float aD = SmoothingFactor(tE, dCutoff);
        float dx = (x - xPrev) / tE;
        float dxHat = ExponentialSmoothing(aD, dx, dxPrev);

        // The filtered signal.
        float cutoff = minCutoff + beta * Math.Abs(dxHat);
        float a = SmoothingFactor(tE, cutoff);
        float xHat = ExponentialSmoothing(a, x, xPrev);

        // Memorize the previous values.
        xPrev = xHat;
        dxPrev = dxHat;

        return xHat;
    }

    private float SmoothingFactor(float tE, float cutoff)
    {
        float r = 2 * (float)Math.PI * cutoff * tE;
        return r / (r + 1);
    }

    private float ExponentialSmoothing(float a, float x, float xPrev)
    {
        return a * x + (1 - a) * xPrev;
    }
}

// Example usage:
//public class Program
//{
//    public static void Main()
//    {
//        float t0 = 0.0;
//        float x0 = 0.0;
//        OneEuroFilter filter = new OneEuroFilter(t0, x0);

//        float t = 1.0;
//        float x = 10.0;

//        float result = filter.Compute(t, x);
//        Console.WriteLine($"Filtered value: {result}");
//    }
//}