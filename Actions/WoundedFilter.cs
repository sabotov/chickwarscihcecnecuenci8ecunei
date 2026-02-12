using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class WoundedFilter : BitStaticFilter
	{
		private bool _ignoreImmune;

		public WoundedFilter(BitStaticFilter prevFilter = null, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_ignoreImmune = ignoreImmune;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune))
			{
				return (int)mon.Health < (int)mon.MaxHealth;
			}
			return false;
		}
	}
}
