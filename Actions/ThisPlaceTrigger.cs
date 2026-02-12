using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ThisPlaceTrigger : BitStaticTrigger
	{
		public ThisPlaceTrigger(TriggerType trigger)
			: base(trigger)
		{
		}

		public ThisPlaceTrigger(BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && requester.placeDelegate().x == position.x)
			{
				return requester.placeDelegate().y == position.y;
			}
			return false;
		}
	}
}
