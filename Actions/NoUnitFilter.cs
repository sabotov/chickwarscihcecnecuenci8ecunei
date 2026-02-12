using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class NoUnitFilter : BitStaticFilter
	{
		public override bool monsterFilter => false;

		public NoUnitFilter(BitStaticFilter lower)
			: base(lower)
		{
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			if (_lowerFilter != null)
			{
				foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in base.GetRightMonsters(affectedParameter, skill, requester))
				{
					if (!requester.parameters.GetMonsters(requester.side).ContainsKey(rightMonster.Key) && !requester.parameters.GetMonsters(requester.enemySide).ContainsKey(rightMonster.Key))
					{
						yield return rightMonster;
					}
				}
				yield break;
			}
			foreach (Vector2 item in requester.parameters.AllEmptyPlaces())
			{
				if (!requester.parameters.GetMonsters(requester.side).ContainsKey(item) && !requester.parameters.GetMonsters(requester.enemySide).ContainsKey(item))
				{
					yield return new KeyValuePair<Vector2, FieldMonster>(item, null);
				}
			}
		}
	}
}
