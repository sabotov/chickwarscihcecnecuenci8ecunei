using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class NearEmptyFilter : BitStaticFilter
	{
		private int _range;

		public override bool monsterFilter => false;

		public NearEmptyFilter(BitStaticFilter prevFilter = null, int range = 1)
			: base(prevFilter)
		{
			_range = range;
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			if (_lowerFilter != null)
			{
				foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in base.GetRightMonsters(affectedParameter, skill, requester))
				{
					if (Mathf.Abs(rightMonster.Key.x - requester.placeDelegate().x) <= (float)_range && Mathf.Abs(rightMonster.Key.y - requester.placeDelegate().y) <= (float)_range)
					{
						yield return rightMonster;
					}
				}
				yield break;
			}
			foreach (Vector2 item in requester.parameters.AllEmptyPlaces())
			{
				if (Mathf.Abs(item.x - requester.placeDelegate().x) <= (float)_range && Mathf.Abs(item.y - requester.placeDelegate().y) <= (float)_range)
				{
					yield return new KeyValuePair<Vector2, FieldMonster>(item, null);
				}
			}
		}
	}
}
