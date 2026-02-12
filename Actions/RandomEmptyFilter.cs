using System.Collections.Generic;
using System.Linq;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class RandomEmptyFilter : BitStaticFilter
	{
		private readonly int _count;

		public override bool monsterFilter => false;

		public RandomEmptyFilter(BitStaticFilter prevFilter = null, int count = 1)
			: base(prevFilter)
		{
			_count = count;
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			int num = ((_lowerFilter != null) ? base.GetRightMonsters(affectedParameter, skill, requester).Count() : requester.parameters.AllEmptyPlaces().Count());
			int[] positions = new int[Mathf.Min(_count, num)];
			for (int i = 0; i < positions.Length; i++)
			{
				int num2 = requester.random.GetRange(0, num);
				bool flag = false;
				for (int j = 0; j < i; j++)
				{
					flag = flag || positions[j] == num2;
				}
				while (flag)
				{
					num2++;
					if (num2 > num - 1)
					{
						num2 = 0;
					}
					flag = false;
					for (int k = 0; k < i; k++)
					{
						flag = flag || positions[k] == num2;
					}
				}
				positions[i] = num2;
			}
			int pos = 0;
			if (_lowerFilter != null)
			{
				foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in base.GetRightMonsters(affectedParameter, skill, requester))
				{
					if (positions.Contains(pos))
					{
						yield return rightMonster;
					}
					pos++;
				}
				yield break;
			}
			foreach (Vector2 item in requester.parameters.AllEmptyPlaces())
			{
				if (positions.Contains(pos))
				{
					yield return new KeyValuePair<Vector2, FieldMonster>(item, null);
				}
				pos++;
			}
		}
	}
}
