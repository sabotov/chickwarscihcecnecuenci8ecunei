using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class NearParamFilter : BitStaticFilter
	{
		private int _distance = 1;

		private bool _ignoreImmune;

		public NearParamFilter(BitStaticFilter prevFilter = null, int distance = 1, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_distance = distance;
			_ignoreImmune = ignoreImmune;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (affectedParameter != null && Mathf.Abs(pos.x - affectedParameter.coords.x) <= (float)_distance && Mathf.Abs(pos.y - affectedParameter.coords.y) <= (float)_distance)
			{
				return base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune);
			}
			return false;
		}
	}
}
