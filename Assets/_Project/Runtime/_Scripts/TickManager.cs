﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lumina.Essentials.Attributes;
using UnityEngine;

/// <summary>
///     <para> Manages the game's tick rate. </para>
///     <para>A "micro tick" is a single tick within a tick cycle and occurs every 60th of a second.</para>
///     <para>A "tick" is a single cycle of ticks and occurs every second.</para>
/// </summary>
[DisallowMultipleComponent]
public class TickManager : Singleton<TickManager>
{
    [SerializeField] Tick tick;
    
    [Tooltip("The number of ticks per second.")]
    [SerializeField] int tickRate = 20;

    [Tooltip("The duration of a tick cycle in seconds.")] 
    [SerializeField] float tickCycleDuration = 1f;

    [Header("Other")]
    [SerializeField] List<int> laps = new (10);
    
    public static event Action OnMicroTick;
    public static event Action OnTick; 

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), "Tick: " + tick.Current);
        GUI.HorizontalSlider(new Rect(10, 30, 100, 20), tick.Current, 1, tick.Rate);
        var slider = GUI.HorizontalSlider(new Rect(10, 50, 100, 20), Time.timeScale, 0, 1);
        // snap to 0.1 increments
        Time.timeScale = Mathf.Round(slider * 10) / 10;
    }

    #region Stopwatch
    Stopwatch stopwatch;
    int tickCounter;
    #endregion

    bool cycleCompleted;
    
    void Start()
    {
        Application.targetFrameRate = 60;
        
        tick = new Tick(tickRate);
        
        stopwatch = new ();
        tickCounter = 0;
        cycleCompleted = false;

        OnMicroTick += () =>
        {
            Logger.Log("Tick! \n[no. " + tick.Current + "]");
            tickCounter++;

            switch (tickCounter)
            {
                case 1:
                    stopwatch.Start();
                    break;

                case var _ when tickCounter == tick.Rate:
                    stopwatch.Stop();
                    laps.Add(Mathf.RoundToInt(stopwatch.ElapsedMilliseconds));
                    stopwatch.Reset();
                    tickCounter = 0;

                    if (!cycleCompleted)
                    {
                        OnTick?.Invoke();
                        cycleCompleted = true;
                    }

                    break;
            }
        };
    }

    void Update()
    {
        if (tick.Update(Time.deltaTime * Time.timeScale, tickCycleDuration))
        {
            OnMicroTick?.Invoke();
            cycleCompleted = false;
        }
    }

     void OnDestroy() => OnMicroTick = null;
}

[Serializable]
public class Tick
{
    [SerializeField, ReadOnly] int tick;
    [SerializeField] bool pause;
    
    float accumulatedTime;

    public bool Pause
    {
        get => pause;
        set => pause = value;
    }

    public int Current
    {
        get => tick;
        private set => tick = value;
    }

    public int Rate { get; }

    public Tick(int rate)
    {
        Pause = false;
        Current = 0;
        Rate = rate;
        accumulatedTime = 0f;
    }

    /// <summary>
    ///     <para> Updates the tick based on the current time and timescale. </para>
    /// </summary>
    /// <param name="deltaTime"> The delta time adjusted by the timescale. </param>
    /// <param name="tickCycleDuration"> The duration of a tick cycle in seconds. </param>
    /// <returns> Returns true if the tick has been updated. </returns>
    public bool Update(float deltaTime, float tickCycleDuration)
    {
        if (Pause) return false;

        accumulatedTime += deltaTime;
        float tickDuration = tickCycleDuration / Rate;

        if (accumulatedTime >= tickDuration)
        {
            Current++;
            accumulatedTime -= tickDuration;
            if (Current > Rate) { Current = 1; }
            return true;
        }
        
        return false;
    }
}