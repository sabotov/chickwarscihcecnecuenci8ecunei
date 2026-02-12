using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class BitFilter
	{
		public ArmySide side;

		public FieldParameters parameters;

		public FieldRandom random;

		public Func<Vector2> placeDelegate;

		public Common.BoolDelegate isRanged;

		protected BitStaticFilter _data;

		protected int _filterValue = -1;

		public ArmySide enemySide
		{
			get
			{
				if (side != ArmySide.Left)
				{
					return ArmySide.Left;
				}
				return ArmySide.Right;
			}
		}

		public BitStaticFilter Data => _data;

		public List<int> LinesForEffect
		{
			get
			{
				return _data.LinesList;
			}
			set
			{
				_data.LinesList = value;
			}
		}

		public List<int> ColumnsForEffect
		{
			get
			{
				return _data.ColList;
			}
			set
			{
				Debug.LogWarning(value);
				_data.ColList = value;
			}
		}

		public int Value => _filterValue;

		public BitFilter(BitStaticFilter data = null, int value = -1)
		{
			_data = data;
			_filterValue = value;
		}

		public virtual void Init(ArmySide thisSide, FieldParameters thisParameters, Func<Vector2> positionDelegate, FieldRandom thisRandom, Common.BoolDelegate isThisRanged)
		{
			placeDelegate = positionDelegate;
			side = thisSide;
			parameters = thisParameters;
			random = thisRandom;
			isRanged = isThisRanged;
		}

		public virtual IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill)
		{
			foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in _data.GetRightMonsters(affectedParameter, skill, this))
			{
				yield return rightMonster;
			}
		}
	}
}
