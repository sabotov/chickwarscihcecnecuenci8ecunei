using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ExceptDeadFilter : BitStaticFilter
	{
		private bool _ignoreImmune;

		public ExceptDeadFilter(BitStaticFilter lower, bool ignoreImmune = true)
			: base(lower)
		{
			_ignoreImmune = ignoreImmune;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune))
			{
				if (mon != null)
				{
					return !mon.ShouldDie;
				}
				return true;
			}
			return false;
		}
	}
}
