using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class LineTrigger : BitStaticTrigger
	{
		private int _lineNum;

		public LineTrigger(TriggerType trigger, int num = 0)
			: base(trigger)
		{
			_lineNum = num;
		}

		public LineTrigger(BitStaticTrigger lowerTrigger = null, int num = 0)
			: base(lowerTrigger)
		{
			_lineNum = num;
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param))
			{
				return monster.coords.y == (float)_lineNum;
			}
			return false;
		}
	}
}
