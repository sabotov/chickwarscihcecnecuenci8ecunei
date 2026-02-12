using System.Collections.Generic;
using System.Linq;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class SkillTrigger : BitStaticTrigger
	{
		private readonly List<ActionBitSignature> _skills;

		public SkillTrigger(List<ActionBitSignature> skills, TriggerType trigger)
			: base(trigger)
		{
			_skills = skills;
		}

		public SkillTrigger(List<ActionBitSignature> skills, BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
			_skills = skills;
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			FieldMonster fieldMonster = monster as FieldMonster;
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && monster is FieldMonster)
			{
				return _skills.Any((ActionBitSignature x) => fieldMonster.CheckHasActionSignature(x));
			}
			return false;
		}
	}
}
