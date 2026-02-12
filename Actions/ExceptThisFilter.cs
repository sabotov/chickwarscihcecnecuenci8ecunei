using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ExceptThisFilter : BitStaticFilter
	{
		private bool _ignoreImmune;

		public ExceptThisFilter(BitStaticFilter lower, bool ignoreImmune = true)
			: base(lower)
		{
			_ignoreImmune = ignoreImmune;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune))
			{
				return requester.placeDelegate() != pos;
			}
			return false;
		}
	}
}
