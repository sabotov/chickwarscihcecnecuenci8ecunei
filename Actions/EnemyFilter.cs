using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class EnemyFilter : BitStaticFilter
	{
		private bool _ignoreImmune;

		public EnemyFilter(BitStaticFilter prevFilter = null, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_ignoreImmune = ignoreImmune;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			BitStaticFilter.IsEnemySideToPlayEffect = true;
			if (base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune))
			{
				if (requester.side == mon.Side && !requester.parameters.GetMonsters(requester.enemySide).ContainsKey(pos))
				{
					return requester.parameters.GetWarlord(requester.enemySide).coords == pos;
				}
				return true;
			}
			return false;
		}
	}
}
