using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class SkillNameFilter : BitStaticFilter
	{
		private string _skill;

		private bool _noRevers = true;

		private bool _ignoreImmune;

		public SkillNameFilter(string skill, BitStaticFilter prevFilter = null, bool noRevers = true, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_skill = skill;
			_noRevers = noRevers;
			_ignoreImmune = ignoreImmune;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (_noRevers)
			{
				if (base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune))
				{
					return mon.CheckHasActionName(_skill);
				}
				return false;
			}
			if (base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune))
			{
				return !mon.CheckHasActionName(_skill);
			}
			return false;
		}
	}
}
