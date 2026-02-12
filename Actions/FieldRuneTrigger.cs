using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class FieldRuneTrigger : BitStaticTrigger
	{
		private enum CompareType
		{
			More = 0,
			Less = 1,
			Equal = 2
		}

		private string _fieldOriginal;

		private CompareType _compareType;

		private int _count;

		public FieldRuneTrigger(string definition, TriggerType trigger)
			: base(trigger)
		{
			InitDefinition(definition);
		}

		public FieldRuneTrigger(string definition, BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
			InitDefinition(definition);
		}

		private void InitDefinition(string definition)
		{
			definition = definition.Replace("&gt;", ">").Replace("&lt;", "<");
			List<string> list = new List<string>(definition.Replace("[", "").Replace("]", "").Split(';'));
			if (list[0] == "friend" || list[0] == "enemy")
			{
				_fieldOriginal = list[0];
				list.RemoveAt(0);
			}
			else
			{
				_fieldOriginal = "all";
			}
			if (list.Count > 0 && (list[list.Count - 1][0] == '=' || list[list.Count - 1][0] == '<' || list[list.Count - 1][0] == '>'))
			{
				_count = int.Parse(list[list.Count - 1].Substring(1));
				if (list[list.Count - 1][0] == '=')
				{
					_compareType = CompareType.Equal;
				}
				else if (list[list.Count - 1][0] == '<')
				{
					_compareType = CompareType.Less;
				}
				else if (list[list.Count - 1][0] == '>')
				{
					_compareType = CompareType.More;
				}
				list.RemoveAt(list.Count - 1);
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
			int num = 0;
			switch (_fieldOriginal)
			{
			case "all":
				num += requester.parameters.GetRunes(requester.side).Values.Count;
				num += requester.parameters.GetRunes(requester.enemySide).Values.Count;
				break;
			case "friend":
				num += requester.parameters.GetRunes(requester.side).Values.Count;
				break;
			case "enemy":
				num += requester.parameters.GetRunes(requester.enemySide).Values.Count;
				break;
			}
			bool result = true;
			switch (_compareType)
			{
			case CompareType.Equal:
				result = num == _count;
				break;
			case CompareType.Less:
				result = num < _count;
				break;
			case CompareType.More:
				result = num > _count;
				break;
			}
			return result;
		}
	}
}
