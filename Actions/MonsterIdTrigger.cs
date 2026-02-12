using System.Linq;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class MonsterIdTrigger : BitStaticTrigger
	{
		private int[] _id;

		private bool _no_reverse = true;

		public MonsterIdTrigger(int[] id, TriggerType trigger)
			: base(trigger)
		{
			_id = id;
		}

		public MonsterIdTrigger(BitStaticTrigger lowerTrigger = null, int[] id = null, bool no_reverse = true)
			: base(lowerTrigger)
		{
			_id = id;
			_no_reverse = no_reverse;
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (_no_reverse)
			{
				if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && monster is FieldMonster)
				{
					return _id.Contains(((FieldMonster)monster).data.monster_id);
				}
				return false;
			}
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && monster is FieldMonster)
			{
				return !_id.Contains(((FieldMonster)monster).data.monster_id);
			}
			return false;
		}
	}
}
