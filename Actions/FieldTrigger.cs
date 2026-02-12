using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class FieldTrigger : BitStaticTrigger
	{
		private enum CompareType
		{
			More = 0,
			Less = 1,
			Equal = 2
		}

		private string _fieldOriginal;

		private List<string> _fieldFilters;

		private CompareType _compareType;

		private int _count;

		public FieldTrigger(string definition, TriggerType trigger)
			: base(trigger)
		{
			InitDefinition(definition);
		}

		public FieldTrigger(string definition, BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
			InitDefinition(definition);
		}

		private void InitDefinition(string definition)
		{
			definition = definition.Replace("&gt;", ">").Replace("&lt;", "<");
			_fieldFilters = new List<string>(definition.Replace("[", "").Replace("]", "").Split(';'));
			if (_fieldFilters[0] == "friend" || _fieldFilters[0] == "enemy")
			{
				_fieldOriginal = _fieldFilters[0];
				_fieldFilters.RemoveAt(0);
			}
			else
			{
				_fieldOriginal = "all";
			}
			if (_fieldFilters[_fieldFilters.Count - 1][0] == '=' || _fieldFilters[_fieldFilters.Count - 1][0] == '<' || _fieldFilters[_fieldFilters.Count - 1][0] == '>')
			{
				_count = int.Parse(_fieldFilters[_fieldFilters.Count - 1].Substring(1));
				if (_fieldFilters[_fieldFilters.Count - 1][0] == '=')
				{
					_compareType = CompareType.Equal;
				}
				else if (_fieldFilters[_fieldFilters.Count - 1][0] == '<')
				{
					_compareType = CompareType.Less;
				}
				else if (_fieldFilters[_fieldFilters.Count - 1][0] == '>')
				{
					_compareType = CompareType.More;
				}
				_fieldFilters.RemoveAt(_fieldFilters.Count - 1);
			}
			else
			{
				_compareType = CompareType.More;
				_count = 0;
			}
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (!base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param))
			{
				return false;
			}
			List<FieldMonster> list = new List<FieldMonster>();
			switch (_fieldOriginal)
			{
			case "all":
				list.AddRange(requester.parameters.GetMonsters(requester.side).Values);
				list.AddRange(requester.parameters.GetMonsters(requester.enemySide).Values);
				break;
			case "friend":
				list.AddRange(requester.parameters.GetMonsters(requester.side).Values);
				break;
			case "enemy":
				list.AddRange(requester.parameters.GetMonsters(requester.enemySide).Values);
				break;
			}
			foreach (string fieldFilter in _fieldFilters)
			{
				if (int.TryParse(fieldFilter, out var parseInt))
				{
					list = list.FindAll((FieldMonster x) => x.data.monster_id == parseInt);
				}
				else if (MonsterDataUtils.GetClassByString(fieldFilter) != Class.NoClass)
				{
					Class cls = MonsterDataUtils.GetClassByString(fieldFilter);
					list = list.FindAll((FieldMonster x) => x.data.monsterClass == cls);
				}
				else if (MonsterDataUtils.GetGenderByString(fieldFilter) != Gender.NoGender)
				{
					Gender gndr = MonsterDataUtils.GetGenderByString(fieldFilter);
					list = list.FindAll((FieldMonster x) => x.data.gender == gndr);
				}
				else if (MonsterDataUtils.GetRaceByString(fieldFilter) != Race.No)
				{
					Race race = MonsterDataUtils.GetRaceByString(fieldFilter);
					list = list.FindAll((FieldMonster x) => x.data.race == race);
				}
			}
			bool result = true;
			switch (_compareType)
			{
			case CompareType.Equal:
				result = list.Count == _count;
				break;
			case CompareType.Less:
				result = list.Count < _count;
				break;
			case CompareType.More:
				result = list.Count > _count;
				break;
			}
			return result;
		}
	}
}
