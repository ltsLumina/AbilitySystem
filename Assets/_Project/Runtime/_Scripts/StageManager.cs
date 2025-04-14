#region
using UnityEngine;
#endregion

public static class StageManager
{
	public const int STAGE_WIDTH = 35;
	
	static float stageOffset;

	public static Vector2 StageOffset => new (stageOffset, 0);

	public static int Stage => Mathf.FloorToInt(stageOffset / STAGE_WIDTH);

	public static void ScrollLevel(float units = STAGE_WIDTH) => stageOffset += units;

	public static Vector2 WithStageOffset(this Vector2 position) => new (position.x + stageOffset, position.y);

	public static void Reset() => stageOffset = 0;
}
