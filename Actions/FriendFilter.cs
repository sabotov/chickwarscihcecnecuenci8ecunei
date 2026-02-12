using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class FriendFilter : BitStaticFilter
	{
		private bool _ignImmune;

		public FriendFilter(BitStaticFilter lower, bool ignoreIm = true)
			: base(lower)
		{
			_ignImmune = ignoreIm;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			BitStaticFilter.IsEnemySideToPlayEffect = false;
			if (base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignImmune))
			{
				if (requester.side != mon.Side && !requester.parameters.GetMonsters(requester.side).ContainsKey(pos))
				{
					return requester.parameters.GetWarlord(requester.side).coords == pos;
				}
				return true;
			}
			return false;
		}
	}
}
