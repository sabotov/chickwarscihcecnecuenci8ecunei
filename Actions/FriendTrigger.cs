using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class FriendTrigger : BitStaticTrigger
	{
		public FriendTrigger(TriggerType trigger)
			: base(trigger)
		{
		}

		public FriendTrigger(BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param))
			{
				if (requester.side != monster.Side)
				{
					return requester.parameters.GetMonsters(requester.side).ContainsKey(position);
				}
				return true;
			}
			return false;
		}
	}
}
