using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class SelfFilter : BitStaticFilter
	{
		public SelfFilter(BitStaticFilter prevFilter = null)
			: base(prevFilter)
		{
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (requester.placeDelegate().y == pos.y && requester.placeDelegate().x == pos.x)
			{
				return base.CheckFilter(pos, mon, affectedParameter, skill, requester, ignoreImmune);
			}
			return false;
		}
	}
}
