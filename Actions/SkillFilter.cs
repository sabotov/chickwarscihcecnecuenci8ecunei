using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class SkillFilter : BitStaticFilter
	{
		private ActionBitSignature _skill;

		private bool _checkResult;

		private bool _ignoreImmune;

		public SkillFilter(ActionBitSignature skill, bool result, BitStaticFilter prevFilter = null, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_skill = skill;
			_checkResult = result;
			_ignoreImmune = ignoreImmune;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune))
			{
				return mon.CheckHasActionSignature(_skill) == _checkResult;
			}
			return false;
		}
	}
}
