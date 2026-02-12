using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class WarlordTrigger : BitStaticTrigger
	{
		public WarlordTrigger(TriggerType trigger)
			: base(trigger)
		{
		}

		public WarlordTrigger(BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param))
			{
				if (!(requester.parameters.GetWarlord(ArmySide.Left).coords == position))
				{
					return requester.parameters.GetWarlord(ArmySide.Right).coords == position;
				}
				return true;
			}
			return false;
		}
	}
}
