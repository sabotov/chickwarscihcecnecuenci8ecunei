using System;
using System.Collections.Generic;
using System.Linq;
using NewAssets.Scripts.DataClasses;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class SummonRuneAction : BitAction
	{
		private string skillValue;

		private RuneData _rune;

		private int count = -1;

		private int value = -1;

		public SummonRuneAction(BitActionAnimation animation, RuneData runeToPlace, string skillValue = "-1")
			: base(animation)
		{
			_rune = runeToPlace;
			this.skillValue = skillValue;
		}

		private void SetRuneValues()
		{
			if (value != -1)
			{
				for (int i = 0; i < _rune.skillValues.Count; i++)
				{
					_rune.skillValues[i] = value;
				}
			}
		}

		private RuneData CreateNewRunePlus(RuneData data)
		{
			for (int i = 0; i < data.skills.Count; i++)
			{
				data.skillValues[i] *= 2;
			}
			return data;
		}

		public static void ParseSkillValue(string skillValue, out int count, out int value)
		{
			string text = skillValue;
			text = text.Replace(" ", "");
			int result = -1;
			if (text.Contains("c") && text.Contains("v"))
			{
				string s = text.Substring(1, text.IndexOf("v") - 1);
				string s2 = text.Substring(text.IndexOf("v") + 1);
				if (int.TryParse(s, out result))
				{
					count = result;
				}
				else
				{
					count = -1;
					Debug.LogError("Couldnt parse rune count! Override disabled.");
				}
				if (int.TryParse(s2, out result))
				{
					value = result;
					return;
				}
				value = -1;
				Debug.LogError("Couldnt parse rune value! Override disabled.");
			}
			else if (int.TryParse(text, out result))
			{
				count = -1;
				value = result;
			}
			else
			{
				Debug.LogError("Invalid skill value format! Count and value overrides disabled!");
				count = -1;
				value = -1;
			}
		}

		public override void PerformAction(IEnumerable<KeyValuePair<Vector2, FieldMonster>> monsters, Action<bool, FieldElement> onCompleted)
		{
			if (!monsters.Any())
			{
				onCompleted(arg1: false, null);
				return;
			}
			FieldMonster fieldMonster = _myMonster as FieldMonster;
			if ((fieldMonster == null || fieldMonster.ShouldDie) && fieldMonster.GetRebornCount(out var _) > 0)
			{
				onCompleted(arg1: false, null);
				return;
			}
			Dictionary<Common.StringDelegate, FieldVisual> monstersAction = new Dictionary<Common.StringDelegate, FieldVisual> { 
			{
				delegate
				{
					ParseSkillValue(skillValue, out count, out value);
					SetRuneValues();
					int num = 0;
					foreach (KeyValuePair<Vector2, FieldMonster> monster in monsters)
					{
						if (_myController.GetFieldRunes().ContainsKey(monster.Key))
						{
							if (_myController.GetFieldRunes().TryGetValue(monster.Key, out var fieldRune))
							{
								RuneData runeData = CreateNewRunePlus(fieldRune.data);
								_myController.DestroyRune(fieldRune);
								_myController.PlaceRune(runeData, monster.Key);
								num++;
								if (num == count)
								{
									break;
								}
							}
						}
						else
						{
							_myController.PlaceRune(_rune, monster.Key);
							num++;
							if (num == count)
							{
								break;
							}
						}
					}
					return "";
				},
				null
			} };
			_animation.Animate(monstersAction, delegate
			{
				onCompleted(arg1: true, null);
			});
		}
	}
}
