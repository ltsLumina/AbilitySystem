#region
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Lumina.Essentials.Attributes;
using UnityEngine;
#endregion

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

	public static event Action OnTick;
	public static event Action OnCycle;

	static bool showTickLogs;

	void OnGUI()
	{
		GUI.Label(new (10, 10, 100, 20), "Tick: " + tick.Current);
		GUI.HorizontalSlider(new (10, 30, 100, 20), tick.Current, 1, tick.Rate);
		float slider = GUI.HorizontalSlider(new (10, 50, 100, 20), Time.timeScale, 0, 2);
		showTickLogs = GUI.Toggle(new (10, 70, 100, 20), showTickLogs, "Show Tick Logs");

		// snap to 0.1 increments
		Time.timeScale = Mathf.Round(slider * 10) / 10;
	}

	#region Stopwatch
	Stopwatch stopwatch;
	int tickCounter;
	#endregion

	bool cycleCompleted;

	/// <summary>
	///     <para> The number of ticks per second. </para>
	/// </summary>
	public int TickRate => tickRate;

	/// <summary>
	///     <para> The duration of a tick cycle in seconds. </para>
	/// </summary>
	public float TickCycleDuration => tickCycleDuration;

	/// <summary>
	///     <para> The duration of a single tick in seconds. </para>
	/// </summary>
	public float TickDuration => 1f / tickRate;

	void Start()
	{
		Application.targetFrameRate = 60;

		tick = new (tickRate);

		stopwatch = new ();
		tickCounter = 0;
		cycleCompleted = false;

		OnTick += () =>
		{
			if (showTickLogs) Logger.Log("Tick! \n[no. " + tick.Current + "]");
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
						OnCycle?.Invoke();
						cycleCompleted = true;
					}

					break;
			}
		};
	}

	void Update()
	{
		if (tick.Cycle(Time.deltaTime * Time.timeScale, tickCycleDuration))
		{
			OnTick?.Invoke();
			cycleCompleted = false;
		}
	}

	void OnDestroy()
	{
		OnTick = null;
		OnCycle = null;
	}

	public static Task WaitForNextCycle()
	{
		var tcs = new TaskCompletionSource<bool>();

		OnCycle += Cycle; // Add the listener to the event and wait for a cycle to complete.
		return tcs.Task;

		void Cycle() // Once the cycle has completed, remove the listener and set the task as completed.
		{
			OnCycle -= Cycle;
			tcs.SetResult(true);
		}
	}
}

[Serializable]
public class Tick
{
	[SerializeField] [ReadOnly] int tick;
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
	public bool Cycle(float deltaTime, float tickCycleDuration)
	{
		if (Pause) return false;

		accumulatedTime += deltaTime;
		float tickDuration = tickCycleDuration / Rate;

		if (accumulatedTime >= tickDuration)
		{
			Current++;
			accumulatedTime -= tickDuration;
			if (Current > Rate) Current = 1;
			return true;
		}

		return false;
	}
}
