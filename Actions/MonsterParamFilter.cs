using System;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class MonsterParamFilter : BitStaticFilter
	{
		private string param_type;

		private string skill_name;

		private string comparison_type;

		private bool requires_value;

		public bool isPercentValue = true;

		public int comparison_value;

		private bool _ignoreImmune;

		public MonsterParamFilter(string paramType, string paramValue, BitStaticFilter prevFilter = null, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_ignoreImmune = ignoreImmune;
			if (paramType.Contains("skill_value_"))
			{
				param_type = "skill_value";
				skill_name = paramType.Replace("skill_value_", "");
			}
			else
			{
				param_type = paramType;
				skill_name = "";
			}
			int startIndex = 2;
			comparison_type = paramValue.Substring(0, 2);
			if (comparison_type != ">=" && comparison_type != "<=")
			{
				startIndex = 1;
				comparison_type = paramValue.Substring(0, 1);
				if (comparison_type != ">" && comparison_type != "<" && comparison_type != "=")
				{
					throw new Exception("Invalid comparison type " + comparison_type);
				}
			}
			string text = paramValue.Substring(startIndex);
			isPercentValue = text.Contains("%");
			string text2 = paramValue.Substring(startIndex).Replace("%", "");
			if (text2 == "x")
			{
				requires_value = true;
				comparison_value = 0;
			}
			else if (!int.TryParse(text2, out comparison_value))
			{
				throw new Exception("Cant parse comparison value " + paramValue);
			}
		}

		private float GetPercentValue(int current, int max)
		{
			if (max == 0)
			{
				if (current == 0)
				{
					return 0f;
				}
				return float.MaxValue;
			}
			return (float)current / (float)max;
		}

		private bool CheckMonster(FieldMonster mon, int value)
		{
			switch (param_type)
			{
			case "skill_value":
			{
				int skillValue = mon.GetSkillValue(skill_name);
				switch (comparison_type)
				{
				case "<":
					return skillValue < value;
				case ">":
					return skillValue > value;
				case "=":
					return skillValue == value;
				case ">=":
					return skillValue >= value;
				case "<=":
					return skillValue <= value;
				default:
					return false;
				}
			}
			case "atk":
			{
				float num3 = (isPercentValue ? GetPercentValue(mon.Attack, mon.data.attack) : ((float)mon.Attack));
				float num4 = (isPercentValue ? ((float)value / 100f) : ((float)value));
				switch (comparison_type)
				{
				case "<":
					return num3 < num4;
				case ">":
					return num3 > num4;
				case "=":
					return num3 == num4;
				case ">=":
					return num3 >= num4;
				case "<=":
					return num3 <= num4;
				default:
					return false;
				}
			}
			case "hp":
			{
				float num = (isPercentValue ? ((float)(int)mon.Health / (float)(int)mon.MaxHealth) : ((float)(int)mon.Health));
				float num2 = (isPercentValue ? ((float)value / 100f) : ((float)value));
				switch (comparison_type)
				{
				case "<":
					return num < num2;
				case ">":
					return num > num2;
				case "=":
					return num == num2;
				case ">=":
					return num >= num2;
				case "<=":
					return num <= num2;
				default:
					return false;
				}
			}
			default:
				return false;
			}
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune))
			{
				return CheckMonster(mon, requires_value ? requester.Value : comparison_value);
			}
			return false;
		}
	}
}
