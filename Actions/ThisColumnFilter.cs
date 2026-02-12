using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ThisColumnFilter : BitStaticFilter
	{
		private bool _ignoreImmune;

		public ThisColumnFilter(BitStaticFilter prevFilter = null, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_ignoreImmune = ignoreImmune;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (requester.placeDelegate().x == pos.x)
			{
				return base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune);
			}
			return false;
		}
	}
}
