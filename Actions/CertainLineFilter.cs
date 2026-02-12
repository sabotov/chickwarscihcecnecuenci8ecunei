using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class CertainLineFilter : BitStaticFilter
	{
		private bool _ignoreImmune;

		private int _num;

		public CertainLineFilter(BitStaticFilter prevFilter = null, int num = 0, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_ignoreImmune = ignoreImmune;
			_num = num;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (mon.coords.y == (float)_num)
			{
				return base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune);
			}
			return false;
		}
	}
}
