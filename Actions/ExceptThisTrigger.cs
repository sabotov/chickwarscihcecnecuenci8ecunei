using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ExceptThisTrigger : BitStaticTrigger
	{
		public ExceptThisTrigger(TriggerType trigger)
			: base(trigger)
		{
		}

		public ExceptThisTrigger(BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param))
			{
				return position != requester.placeDelegate();
			}
			return false;
		}
	}
}
