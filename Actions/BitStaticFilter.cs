using System;
using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class BitStaticFilter
	{
		public const string REQUIRES_VALUE_STR = "x";

		private List<int> linesList = new List<int>();

		private List<int> colList = new List<int>();

		public static bool IsEnemySideToPlayEffect = true;

		protected BitStaticFilter _lowerFilter;

		public virtual bool monsterFilter => true;

		public List<int> LinesList
		{
			get
			{
				return linesList;
			}
			set
			{
				linesList = value;
			}
		}

		public List<int> ColList
		{
			get
			{
				return colList;
			}
			set
			{
				colList = value;
			}
		}

		public BitStaticFilter(BitStaticFilter lowerFilter = null)
		{
			if (lowerFilter != null && lowerFilter.monsterFilter != monsterFilter && !(lowerFilter is ThisCoordFilter) && !(this is ThisCoordFilter) && !(lowerFilter is ExceptDeadFilter) && !(this is ExceptDeadFilter))
			{
				Debug.LogError(string.Concat("Wrong filter stacking! Trying to put ", lowerFilter.GetType(), " in ", GetType()));
				throw new Exception(string.Concat("Wrong filter stacking! Trying to put ", lowerFilter.GetType(), " in ", GetType()));
			}
			_lowerFilter = lowerFilter;
		}

		public virtual IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			new Dictionary<Vector2, FieldMonster>();
			if (_lowerFilter != null)
			{
				foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in _lowerFilter.GetRightMonsters(affectedParameter, skill, requester))
				{
					if (CheckFilter(rightMonster.Key, rightMonster.Value, affectedParameter, skill, requester))
					{
						yield return rightMonster;
					}
				}
				yield break;
			}
			foreach (KeyValuePair<Vector2, FieldMonster> monster in requester.parameters.GetMonsters(requester.enemySide))
			{
				if (CheckFilter(monster.Key, monster.Value, affectedParameter, skill, requester))
				{
					yield return monster;
				}
			}
			foreach (KeyValuePair<Vector2, FieldMonster> monster2 in requester.parameters.GetMonsters(requester.side))
			{
				if (CheckFilter(monster2.Key, monster2.Value, affectedParameter, skill, requester))
				{
					yield return monster2;
				}
			}
		}

		public virtual bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (!monsterFilter)
			{
				return true;
			}
			if (mon != null && mon.data.is_pet)
			{
				return false;
			}
			if (ignoreImmune)
			{
				return (_lowerFilter != null) ? _lowerFilter.CheckFilter(pos, mon, affectedParameter, skill, requester, ignoreImmune) : mon.IsNotImmune(skill, mon, requester);
			}
			return _lowerFilter == null || _lowerFilter.CheckFilter(pos, mon, affectedParameter, skill, requester, ignoreImmune);
		}
	}
}
