using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class NeighbourTrigger : BitStaticTrigger
	{
		private int _distance;

		public NeighbourTrigger(int distance, TriggerType trigger)
			: base(trigger)
		{
			_distance = distance;
		}

		public NeighbourTrigger(int distance, BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
			_distance = distance;
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			bool flag = Mathf.Abs(position.x - requester.placeDelegate().x) <= (float)_distance && Mathf.Abs(position.y - requester.placeDelegate().y) <= (float)_distance;
			return base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && flag;
		}
	}
}
