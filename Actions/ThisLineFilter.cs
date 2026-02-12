using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ThisLineFilter : BitStaticFilter
	{
		private bool _ignoreImmune;

		public ThisLineFilter(BitStaticFilter prevFilter = null, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_ignoreImmune = ignoreImmune;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (requester.placeDelegate().y == pos.y)
			{
				return base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune);
			}
			return false;
		}
	}
}
