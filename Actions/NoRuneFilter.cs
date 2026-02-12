using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class NoRuneFilter : BitStaticFilter
	{
		public override bool monsterFilter => false;

		public NoRuneFilter(BitStaticFilter prevFilter = null)
			: base(prevFilter)
		{
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			if (_lowerFilter != null)
			{
				foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in base.GetRightMonsters(affectedParameter, skill, requester))
				{
					if (!requester.parameters.GetRunes(requester.side).ContainsKey(rightMonster.Key) && !requester.parameters.GetRunes(requester.enemySide).ContainsKey(rightMonster.Key))
					{
						yield return new KeyValuePair<Vector2, FieldMonster>(rightMonster.Key, null);
					}
				}
				yield break;
			}
			foreach (Vector2 item in requester.parameters.AllEmptyPlaces())
			{
				if (!requester.parameters.GetRunes(requester.side).ContainsKey(item) && !requester.parameters.GetRunes(requester.enemySide).ContainsKey(item))
				{
					yield return new KeyValuePair<Vector2, FieldMonster>(item, null);
				}
			}
		}
	}
}
