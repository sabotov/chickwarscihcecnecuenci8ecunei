using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class NotThisWarlordTrigger : BitStaticTrigger
	{
		public NotThisWarlordTrigger(TriggerType trigger)
			: base(trigger)
		{
		}

		public NotThisWarlordTrigger(BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param))
			{
				if (affectedMonster != null)
				{
					if (requester.parameters.GetWarlord(ArmySide.Left).coords != affectedMonster.coords)
					{
						return requester.parameters.GetWarlord(ArmySide.Right).coords != affectedMonster.coords;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}
}
