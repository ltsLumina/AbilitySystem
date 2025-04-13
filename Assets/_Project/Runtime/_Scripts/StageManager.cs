#region
using UnityEngine;
#endregion

public static class StageManager
{
	static float stageOffset;

	public static Vector2 StageOffset => new (stageOffset, 0);

	public static void ScrollLevel(float units = 35f) => stageOffset += units;

	public static Vector2 WithStageOffset(this Vector2 position) => new (position.x + stageOffset, position.y);

	public static void Reset() => stageOffset = 0;
}
