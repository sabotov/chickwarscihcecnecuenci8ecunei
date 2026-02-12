using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class GenderTrigger : BitStaticTrigger
	{
		private Gender _gender;

		private bool _noReverse = true;

		public GenderTrigger(Gender gender, TriggerType trigger, bool noReverse = true)
			: base(trigger)
		{
			_gender = gender;
			_noReverse = noReverse;
		}

		public GenderTrigger(Gender gender, BitStaticTrigger lowerTrigger = null, bool noReverse = true)
			: base(lowerTrigger)
		{
			_gender = gender;
			_noReverse = noReverse;
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (_noReverse)
			{
				if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && monster is FieldMonster)
				{
					return (monster as FieldMonster).data.staticInnerData.gender == _gender;
				}
				return false;
			}
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && monster is FieldMonster)
			{
				return (monster as FieldMonster).data.staticInnerData.gender != _gender;
			}
			return false;
		}
	}
}
