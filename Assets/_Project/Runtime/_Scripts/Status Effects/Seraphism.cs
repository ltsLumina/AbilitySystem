public class Seraphism : Buff
{
	public override void Reset()
	{
		statusName = "Seraphism";
		description = "Reduces cast time by 20%, spell speed by 15%, and increases damage dealt by 10% for self and allies.";
		duration = 12;
		target = Target.Self | Target.Ally;
		timing = Timing.Postfix;
	}

	protected override void OnInvoke()
	{
		foreach (Player player in PlayerManager.Instance.Players)
		{
			player.Stats.Remove("cast speed", 0.2f);
			player.Stats.Remove("spell speed", 0.15f);
			player.Stats.Add("damage", 0.1f);
		}
	}

	protected override void OnDecay()
	{
		foreach (Player player in PlayerManager.Instance.Players)
		{
			player.Stats.Add("cast speed", 0.2f);
			player.Stats.Add("spell speed", 0.15f);
			player.Stats.Remove("damage", 0.1f);
		}
	}
}
