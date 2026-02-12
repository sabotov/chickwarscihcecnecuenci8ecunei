using System.Collections.Generic;
using System.Linq;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ArmySideFilter : BitStaticFilter
	{
		public enum RowType
		{
			All = 0,
			Forward = 1,
			Center = 2,
			Backward = 3
		}

		private readonly bool _friendly;

		private readonly Class _monClass;

		private readonly RowType _rowType;

		public override bool monsterFilter => false;

		public ArmySideFilter(BitStaticFilter prevFilter = null, bool friendly = true, Class monClass = Class.NoClass)
			: base(prevFilter)
		{
			_friendly = friendly;
			_monClass = monClass;
			_rowType = RowType.All;
		}

		public ArmySideFilter(BitStaticFilter prevFilter, bool friendly, RowType rowType)
			: base(prevFilter)
		{
			_friendly = friendly;
			_monClass = Class.NoClass;
			_rowType = rowType;
		}

		private bool FitRowType(int coord, int width)
		{
			if (_rowType == RowType.All)
			{
				return true;
			}
			if (_rowType == RowType.Backward)
			{
				if (coord != 0)
				{
					return coord == width - 1;
				}
				return true;
			}
			if (_rowType == RowType.Center)
			{
				if (coord != 1)
				{
					return coord == width - 2;
				}
				return true;
			}
			if (_rowType == RowType.Forward)
			{
				if (coord != 2)
				{
					return coord == width - 3;
				}
				return true;
			}
			return false;
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			if (_lowerFilter != null)
			{
				IEnumerable<Vector2> oppositePos = requester.parameters.GetClassedTiles(Class.Building, _friendly ? requester.side : requester.enemySide);
				if (_monClass != Class.NoClass)
				{
					oppositePos = requester.parameters.GetClassedTiles(_monClass, _friendly ? requester.side : requester.enemySide);
				}
				foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in base.GetRightMonsters(affectedParameter, skill, requester))
				{
					if (oppositePos.Contains(rightMonster.Key) && FitRowType((int)rightMonster.Key.x, requester.parameters.width))
					{
						yield return rightMonster;
					}
				}
				yield break;
			}
			IEnumerable<Vector2> classedTiles = requester.parameters.GetClassedTiles(Class.Building, _friendly ? requester.side : requester.enemySide);
			if (_monClass != Class.NoClass)
			{
				classedTiles = requester.parameters.GetClassedTiles(_monClass, _friendly ? requester.side : requester.enemySide);
			}
			foreach (Vector2 item in classedTiles)
			{
				if (FitRowType((int)item.x, requester.parameters.width))
				{
					yield return new KeyValuePair<Vector2, FieldMonster>(item, null);
				}
			}
		}

		public static RowType GetRowType(string str)
		{
			switch (str)
			{
			case "backward":
				return RowType.Backward;
			case "center":
				return RowType.Center;
			case "forward":
				return RowType.Forward;
			default:
				return RowType.All;
			}
		}
	}
}
