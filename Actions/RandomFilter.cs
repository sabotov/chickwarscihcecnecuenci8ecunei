using System.Collections.Generic;
using System.Linq;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class RandomFilter : BitStaticFilter
	{
		private int elem_count = 1;

		public RandomFilter(BitStaticFilter lower, int count)
			: base(lower)
		{
			elem_count = count;
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			int num = base.GetRightMonsters(affectedParameter, skill, requester).Count();
			if (num == 0)
			{
				yield break;
			}
			List<int> randNums = new List<int>();
			for (int i = 0; i < elem_count && num - i >= 0; i++)
			{
				int j;
				for (j = requester.random.GetRange(0, num - i); randNums.Contains(j); j++)
				{
				}
				randNums.Add(j);
			}
			int counter = 0;
			foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in base.GetRightMonsters(affectedParameter, skill, requester))
			{
				if (randNums.Contains(counter))
				{
					yield return rightMonster;
				}
				counter++;
			}
		}
	}
}
