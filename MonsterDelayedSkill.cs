using System;
using BattlefieldScripts.Actions;

namespace BattlefieldScripts
{
	[Serializable]
	public class MonsterDelayedSkill
	{
		public class MonsterDelayedSkillSignature
		{
			public TriggerType reduceTrigger;

			public int count;

			public ActionBitSignature skill;
		}

		public TriggerType reduceTrigger;

		public int count;

		public string skillVal;

		public ActionBit skill;
	}
}
