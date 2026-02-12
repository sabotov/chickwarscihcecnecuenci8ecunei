using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class NotWarlordTrigger : BitStaticTrigger
	{
		public NotWarlordTrigger(TriggerType trigger)
			: base(trigger)
		{
		}

		public NotWarlordTrigger(BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			bool flag = base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param);
			FieldMonster warlord = requester.parameters.GetWarlord(ArmySide.Left);
			FieldMonster warlord2 = requester.parameters.GetWarlord(ArmySide.Right);
			if (trigger != TriggerType.Death)
			{
				if (flag && warlord.coords != position && warlord2.coords != position && affectedMonster != warlord)
				{
					return affectedMonster != warlord2;
				}
				return false;
			}
			if (flag && warlord.coords != position && warlord2.coords != position && monster != warlord)
			{
				return monster != warlord2;
			}
			return false;
		}
	}
}
