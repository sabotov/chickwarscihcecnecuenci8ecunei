using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class TriggerFilter : BitStaticFilter
	{
		private TriggerType _trigger;

		private bool _ignoreImmune;

		public TriggerFilter(TriggerType trigger, BitStaticFilter prevFilter = null, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_trigger = trigger;
			_ignoreImmune = ignoreImmune;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune))
			{
				return mon.CheckHasTrigger(_trigger);
			}
			return false;
		}
	}
}
