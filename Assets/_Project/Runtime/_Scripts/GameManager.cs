#region
using UnityEngine;
#endregion

public class GameManager : Singleton<GameManager>
{
	public Boss CurrentBoss { get; private set; }

	public void Initialize(Boss boss) => CurrentBoss = boss;

	void Start()
	{
		CurrentBoss.OnBossStarted += StartTimer;
		CurrentBoss.OnDeath += StopTimer;
	}

	void StartTimer(Color color)
	{
		timer = 0f;
		isTimerRunning = true;
	}

	void StopTimer() => isTimerRunning = false;

	float timer;
	bool isTimerRunning;

	void Update()
	{
		if (isTimerRunning) timer += Time.deltaTime;
	}

	void OnGUI() =>

			// Display the timer in the top right corner
			GUI.Label(new (Screen.width - 200, 75, 200, 20), $"Time: {timer:F2}");
}
