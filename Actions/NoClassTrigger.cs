using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class NoClassTrigger : BitStaticTrigger
	{
		private Class _class;

		public NoClassTrigger(Class @class, TriggerType trigger)
			: base(trigger)
		{
			_class = @class;
		}

		public NoClassTrigger(Class @class, BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
			_class = @class;
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && monster is FieldMonster)
			{
				return (monster as FieldMonster).data.staticInnerData.monsterClass != _class;
			}
			return false;
		}
	}
}
