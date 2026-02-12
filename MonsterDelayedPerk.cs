using System;
using BattlefieldScripts.Actions;

namespace BattlefieldScripts
{
	[Serializable]
	public class MonsterDelayedPerk
	{
		public class MonsterDelayedPerkSignature
		{
			public TriggerType reduceTrigger;

			public int count;

			public ActionBitSignature perk;
		}

		public TriggerType reduceTrigger;

		public int count;

		public PerkBit perk;
	}
}
