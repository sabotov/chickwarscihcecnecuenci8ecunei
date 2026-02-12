using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class RaceTrigger : BitStaticTrigger
	{
		private Race _race;

		private bool _noReverse = true;

		public RaceTrigger(Race race, TriggerType trigger, bool noReverse = true)
			: base(trigger)
		{
			_race = race;
			_noReverse = noReverse;
		}

		public RaceTrigger(Race race, BitStaticTrigger lowerTrigger = null, bool noReverse = true)
			: base(lowerTrigger)
		{
			_race = race;
			_noReverse = noReverse;
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (_noReverse)
			{
				if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && monster is FieldMonster)
				{
					return (monster as FieldMonster).data.staticInnerData.race == _race;
				}
				return false;
			}
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && monster is FieldMonster)
			{
				return (monster as FieldMonster).data.staticInnerData.race != _race;
			}
			return false;
		}
	}
}
