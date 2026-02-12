using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ThisLineTrigger : BitStaticTrigger
	{
		public ThisLineTrigger(TriggerType trigger)
			: base(trigger)
		{
		}

		public ThisLineTrigger(BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param))
			{
				return requester.placeDelegate().y == position.y;
			}
			return false;
		}
	}
}
