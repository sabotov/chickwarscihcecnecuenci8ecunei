using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ThisLineEmpty : BitStaticFilter
	{
		public override bool monsterFilter => false;

		public ThisLineEmpty(BitStaticFilter prevFilter = null)
			: base(prevFilter)
		{
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			if (_lowerFilter != null)
			{
				foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in base.GetRightMonsters(affectedParameter, skill, requester))
				{
					if (rightMonster.Key.y == requester.placeDelegate().y)
					{
						yield return rightMonster;
					}
				}
				yield break;
			}
			foreach (Vector2 item in requester.parameters.AllEmptyPlaces())
			{
				if (item.y == requester.placeDelegate().y)
				{
					yield return new KeyValuePair<Vector2, FieldMonster>(item, null);
				}
			}
		}
	}
}
