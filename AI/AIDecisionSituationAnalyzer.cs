using System;
using System.Collections.Generic;
using System.Linq;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using NewAssets.Scripts.Data_Helpers;
using NewAssets.Scripts.UtilScripts;
using UnityEngine;

namespace BattlefieldScripts.AI
{
	public class AIDecisionSituationAnalyzer
	{
		public static readonly float LOOSE_PROFIT = -1000000f;

		private AICharacter _aiCharacter;

		private ArmySide _side;

		private int _fieldWidth;

		private int _fieldHeight;

		private int _drawDelay;

		private int _drawShift;

		public AIDecisionSituationAnalyzer()
		{
		}

		public AIDecisionSituationAnalyzer(AICharacter aiCharacter, ArmySide side, int fieldWidth, int fieldHeight, int drawDelay, int drawShift)
		{
			_aiCharacter = aiCharacter;
			_side = side;
			_fieldWidth = fieldWidth;
			_fieldHeight = fieldHeight;
			_drawDelay = drawDelay;
			_drawShift = drawShift;
		}

		public AIDecisionSituationAnalyzer OpponentSituationAnalyzer()
		{
			return new AIDecisionSituationAnalyzer
			{
				_side = _side.OtherSide(),
				_drawDelay = _drawDelay,
				_drawShift = _drawShift,
				_aiCharacter = _aiCharacter,
				_fieldWidth = _fieldWidth,
				_fieldHeight = _fieldHeight
			};
		}

		public Dictionary<MonsterData, IEnumerable<Vector2>> GetMonstersAndPlaces(Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> armies, ArmySide side, List<MonsterData> monsters, ArmyActionPerformer.PossiblePlacesDelegate getPossiblePlaces)
		{
			Dictionary<MonsterData, IEnumerable<Vector2>> dictionary = new Dictionary<MonsterData, IEnumerable<Vector2>>();
			List<MonsterData> list = new List<MonsterData>();
			foreach (MonsterData mon in monsters)
			{
				if (list.Find((MonsterData m) => m.monster_id == mon.monster_id) == null)
				{
					list.Add(mon);
				}
			}
			if (side == ArmySide.Right && TestUtilFunctions.UseMon())
			{
				MonsterData key = list.Find((MonsterData x) => x.GetName() == TestUtilFunctions.GetMonsterName());
				Vector2 monsterPlace = TestUtilFunctions.GetMonsterPlace();
				dictionary.Add(key, new List<Vector2> { monsterPlace });
				return dictionary;
			}
			if (side == ArmySide.Left && TestUtilFunctions.UseEnemy())
			{
				MonsterData monsterData = list.Find((MonsterData x) => x.GetName() == TestUtilFunctions.GetEnemyMonsterName());
				if (monsterData != null)
				{
					Vector2 enemyMonsterPlace = TestUtilFunctions.GetEnemyMonsterPlace();
					dictionary.Add(monsterData, new List<Vector2> { enemyMonsterPlace });
					return dictionary;
				}
			}
			Dictionary<int, List<FieldMonster>> dictionary2 = new Dictionary<int, List<FieldMonster>>();
			Dictionary<int, List<FieldRune>> dictionary3 = new Dictionary<int, List<FieldRune>>();
			ArmySide armySide = side.OtherSide();
			Dictionary<string, int> dictionary4 = new Dictionary<string, int>();
			for (int num = 0; num < _fieldHeight; num++)
			{
				List<FieldMonster> objectsOnLine = GetObjectsOnLine(num, armies[side].army, side);
				List<FieldMonster> objectsOnLine2 = GetObjectsOnLine(num, armies[armySide].army, armySide);
				dictionary2.Add(num, objectsOnLine);
				List<FieldRune> objectsOnLine3 = GetObjectsOnLine(num, armies[side].runes, side);
				List<FieldRune> objectsOnLine4 = GetObjectsOnLine(num, armies[armySide].runes, armySide);
				dictionary3.Add(num, objectsOnLine3);
				string text = "line_";
				foreach (FieldMonster item in objectsOnLine)
				{
					text = text + "m(" + item.data.monster_id + "_" + (string)item.data.attack + "_" + (string)item.data.health + ")_";
				}
				foreach (FieldRune item2 in objectsOnLine3)
				{
					text = text + "r(" + item2.data.name + ")_";
				}
				text += "e_";
				foreach (FieldMonster item3 in objectsOnLine2)
				{
					text = text + "m(" + item3.data.monster_id + "_" + (string)item3.data.attack + "_" + (string)item3.data.health + ")_";
				}
				foreach (FieldRune item4 in objectsOnLine4)
				{
					text = text + "r(" + item4.data.name + ")_";
				}
				if (!dictionary4.ContainsKey(text))
				{
					dictionary4.Add(text, num);
				}
			}
			foreach (MonsterData item5 in list)
			{
				IEnumerable<Vector2> enumerable = getPossiblePlaces(side, armies[side].army, item5);
				new List<Vector2>(enumerable);
				List<Vector2> list2 = new List<Vector2>();
				foreach (Vector2 item6 in enumerable)
				{
					if (dictionary4.ContainsValue((int)item6.y))
					{
						list2.Add(item6);
					}
				}
				bool flag = item5.monsterClass == Class.Melee;
				bool flag2 = item5.monsterClass == Class.Ranged;
				if (flag || flag2)
				{
					foreach (KeyValuePair<string, int> line in dictionary4)
					{
						IEnumerable<Vector2> enumerable2 = enumerable.Where((Vector2 place) => (int)place.y == line.Value);
						new List<Vector2>(enumerable2);
						if (enumerable2.Count() < 2)
						{
							continue;
						}
						_ = dictionary3[line.Value];
						int num2 = -1;
						int num3 = -1;
						if (side == ArmySide.Left)
						{
							num2 = 1;
							num3 = (flag ? 2 : 0);
						}
						else
						{
							num2 = _fieldWidth - 2;
							num3 = (flag ? (_fieldWidth / 2) : (_fieldWidth - 1));
						}
						Vector2 vector = new Vector2(num2, line.Value);
						Vector2 vector2 = new Vector2(num3, line.Value);
						bool flag3 = false;
						bool flag4 = false;
						foreach (SkillStaticData skill in item5.skills)
						{
							if (skill.skill == SkillType.Summon && skill.trigger == TriggerType.Appear)
							{
								flag3 = true;
							}
							if (skill.triggerBit is NeighbourTrigger || skill.filterBit is NearEmptyFilter || skill.filterBit is NeighbourFilter || skill.valueBit is NeighbourFilter)
							{
								flag4 = true;
							}
						}
						if (!flag3 && !flag4)
						{
							if (armies[side].runes.ContainsKey(vector) && !armies[side].runes.ContainsKey(vector2))
							{
								list2.Remove(vector2);
							}
							else
							{
								list2.Remove(vector);
							}
						}
					}
				}
				if (!dictionary.ContainsKey(item5))
				{
					dictionary.Add(item5, list2);
				}
			}
			return dictionary;
		}

		private Dictionary<Vector2, T> GetObjectsOnLineDict<T>(int lineNum, Dictionary<Vector2, T> allObjects, ArmySide side)
		{
			Dictionary<Vector2, T> dictionary = new Dictionary<Vector2, T>();
			new List<int>();
			int num = -1;
			int num2 = -1;
			switch (side)
			{
			case ArmySide.Left:
				num = 0;
				num2 = _fieldWidth / 2 - 1;
				break;
			case ArmySide.Right:
				num = _fieldWidth / 2;
				num2 = _fieldWidth - 1;
				break;
			}
			for (int i = num; i <= num2; i++)
			{
				T value = default(T);
				Vector2 key = new Vector2(i, lineNum);
				if (allObjects.TryGetValue(key, out value))
				{
					dictionary.Add(key, value);
				}
			}
			return dictionary;
		}

		private List<T> GetObjectsOnLine<T>(int lineNum, Dictionary<Vector2, T> allObjects, ArmySide side)
		{
			return new List<T>(GetObjectsOnLineDict(lineNum, allObjects, side).Values);
		}

		private string GetLineBehindCoeffKey(int lineNum, Dictionary<Vector2, MonsterData> monsters, ArmySide side)
		{
			List<float> list = new List<float>();
			List<string> list2 = new List<string>();
			new List<int>();
			int num = -1;
			int num2 = -1;
			switch (side)
			{
			case ArmySide.Left:
			{
				num = 0;
				num2 = _fieldWidth / 2 - 1;
				for (int num3 = num2; num3 >= num; num3--)
				{
					list.Add(num3);
				}
				break;
			}
			case ArmySide.Right:
			{
				num = _fieldWidth / 2;
				num2 = _fieldWidth - 1;
				for (int i = num; i <= num2; i++)
				{
					list.Add(i);
				}
				break;
			}
			}
			foreach (float item2 in list)
			{
				MonsterData value = null;
				Vector2 key = new Vector2(item2, lineNum);
				string item = "";
				if (monsters.TryGetValue(key, out value))
				{
					if (value.monsterClass == Class.Ranged)
					{
						item = "RANGED";
					}
					else if (value.monsterClass == Class.Melee)
					{
						item = "MELEE";
					}
					else if (value.monsterClass == Class.Building)
					{
						item = "BUILDING";
					}
				}
				else
				{
					item = "NO";
				}
				list2.Add(item);
			}
			return MyUtil.StringListToString(list2, "-");
		}

		private float BehindProfit(Vector2 place, Dictionary<Vector2, MonsterData> monsters, ArmySide side)
		{
			int lineNum = (int)place.y;
			float profitByLineKey = BehindLineCoeffsHelper.GetProfitByLineKey(GetLineBehindCoeffKey(lineNum, monsters, side));
			return 0f + profitByLineKey * _aiCharacter.behindCoef;
		}

		public AIDecision CalcProfitBySituation(AIDecision preDecision, Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> armiesBefore, Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> armiesAfterSimulate, Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> armiesAfterPlace, CopiedSimulateRandom simRandom, int depth, bool showLog)
		{
			float num = 0f;
			ArmySide armySide = _side.OtherSide();
			Dictionary<Vector2, FieldMonster> army = armiesAfterSimulate[_side].army;
			Dictionary<Vector2, FieldMonster> army2 = armiesAfterSimulate[armySide].army;
			FieldMonster warlord = armiesAfterSimulate[_side].warlord;
			FieldMonster warlord2 = armiesAfterSimulate[armySide].warlord;
			FieldMonster warlord3 = armiesBefore[_side].warlord;
			_ = armiesBefore[armySide].warlord;
			Dictionary<Vector2, FieldRune> runes = armiesAfterSimulate[_side].runes;
			Dictionary<Vector2, FieldRune> runes2 = armiesAfterSimulate[armySide].runes;
			bool isDefeat = false;
			bool isDefeat2 = false;
			bool showLog2 = false;
			bool showProfitLog = false;
			if (TestUtilFunctions.AILogMode() && _aiCharacter.staticDataId == TestUtilFunctions.GetAiCharId())
			{
				if (preDecision.decisionType == AIDecision.Type.Monster && preDecision.monster.NameLog() == TestUtilFunctions.GetMonsterName() && preDecision.place == TestUtilFunctions.GetMonsterPlace())
				{
					showLog2 = true;
					showProfitLog = true;
				}
				else if (preDecision.decisionType == AIDecision.Type.Skill)
				{
					showLog2 = true;
				}
			}
			float num2 = ArmyStateProfit(army, army2, runes, warlord, _aiCharacter.myAtkCoef, _aiCharacter.myHpCoef, _aiCharacter.myWarlordCoef, _side, out isDefeat, showLog2, "MY ARMY: ");
			float num3 = ArmyStateProfit(army2, army, runes2, warlord2, _aiCharacter.enemyAtkCoef, _aiCharacter.enemyHpCoef, _aiCharacter.enemyWarlordCoef, armySide, out isDefeat2, showLog2, "ENEMY ARMY: ", showProfitLog);
			if (isDefeat)
			{
				preDecision.isDefeat = true;
			}
			else if (isDefeat2)
			{
				preDecision.isWin = true;
			}
			Dictionary<Vector2, MonsterData> dictionary = new Dictionary<Vector2, MonsterData>();
			foreach (KeyValuePair<Vector2, FieldMonster> item in armiesAfterPlace[_side].army)
			{
				dictionary.Add(item.Key, item.Value.data);
			}
			float num4 = 0f;
			if (preDecision.decisionType == AIDecision.Type.Monster || preDecision.decisionType == AIDecision.Type.SkillAndMonster)
			{
				num4 = BehindProfit(preDecision.place, dictionary, _side);
			}
			float num5 = KillEnemyUnitsProfit(army2, army, armiesAfterPlace, armiesAfterSimulate[armySide].runes, _side, _aiCharacter.targetCreaturesCoef);
			int num6 = (int)warlord3.GetHealth() - (int)warlord.GetHealth();
			bool useLightMetric = TestUtilFunctions.UseLightMetric();
			float num7 = SaveWarlordProfit(armiesAfterSimulate, armiesBefore, _side, simRandom, _aiCharacter.saveWarlordCoef, depth <= 1, useLightMetric, out isDefeat);
			float num8 = SaveWarlordProfit(armiesAfterSimulate, armiesBefore, armySide, simRandom, _aiCharacter.killWarlordCoef, calcNextTurn: true, useLightMetric, out isDefeat2);
			if (isDefeat)
			{
				preDecision.isDefeat = true;
			}
			else if (isDefeat2)
			{
				preDecision.isWin = true;
			}
			float num9 = LinePushProfit(army, army2, runes, runes2, _aiCharacter, _side);
			num += num2 - num3 + num4 + num5 + num7 - num8 + num9;
			string text = "";
			text = preDecision.ToStringShort() + ", profit " + num + ", iqProfit: %iqProfit%, . myArmy " + num2 + ", enemyArmy " + num3 + ", behind " + num4 + ", killCreatersCoeff " + num5 + ", saveWarlord " + num7 + ", warlordDamage " + num6 + ", killEnemyWarlord " + num8 + ", linePush " + num9;
			preDecision.profit += num;
			text = text.Replace("%iqProfit%", num.ToString());
			preDecision.infoString = text;
			return preDecision;
		}

		public float GetProfitForCurrentSituation(Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> armies, Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> armiesBefore, ArmySide side, CopiedSimulateRandom simRandom, TriggerType warlordSkill = TriggerType.NoTrigger, bool trace = false)
		{
			bool isDefeat = false;
			bool isDefeat2 = false;
			ArmySide armySide = side.OtherSide();
			SimulateEnviroment.SimulateArmy simulateArmy = armies[side];
			SimulateEnviroment.SimulateArmy simulateArmy2 = armies[armySide];
			Dictionary<Vector2, FieldMonster> army = simulateArmy.army;
			Dictionary<Vector2, FieldMonster> army2 = simulateArmy2.army;
			FieldMonster warlord = simulateArmy.warlord;
			FieldMonster warlord2 = simulateArmy2.warlord;
			Dictionary<Vector2, FieldRune> runes = simulateArmy.runes;
			Dictionary<Vector2, FieldRune> runes2 = simulateArmy2.runes;
			float num = ArmyStateProfit(army, army2, runes, warlord, _aiCharacter.myAtkCoef, _aiCharacter.myHpCoef, _aiCharacter.myWarlordCoef, side, out isDefeat);
			float num2 = ArmyStateProfit(army2, army, runes2, warlord2, _aiCharacter.enemyAtkCoef, _aiCharacter.enemyHpCoef, _aiCharacter.enemyWarlordCoef, armySide, out isDefeat2);
			float targetCreaturesCoeff = 1f;
			if (warlordSkill != TriggerType.NoTrigger)
			{
				targetCreaturesCoeff = WarlordSkillsPreset.instance.GetKillCreatureCoeff(simulateArmy.warlord.data, warlordSkill);
			}
			float num3 = KillEnemyUnitsProfit(simulateArmy2.army, simulateArmy.army, armiesBefore, simulateArmy2.runes, side, targetCreaturesCoeff);
			float num4 = SaveWarlordProfit(armies, armies, side, simRandom, _aiCharacter.saveWarlordCoef, calcNextTurn: true, useLightMetric: false, out isDefeat);
			float num5 = SaveWarlordProfit(armies, armies, armySide, simRandom, _aiCharacter.killWarlordCoef, calcNextTurn: true, useLightMetric: false, out isDefeat2);
			float num6 = LinePushProfit(army, army2, runes, runes2, _aiCharacter, side);
			return 0f + (num - num2 + num4 - num5 + num6 + num3);
		}

		private float ArmyStateProfit(Dictionary<Vector2, FieldMonster> myMonsters, Dictionary<Vector2, FieldMonster> enemyMonsters, Dictionary<Vector2, FieldRune> runes, FieldMonster warlord, float atkCoef, float hpCoef, float warlordCoef, ArmySide side, out bool isDefeat, bool showLog = false, string logPref = "", bool showProfitLog = false)
		{
			float num = 0f;
			isDefeat = false;
			string text = logPref;
			foreach (KeyValuePair<Vector2, FieldMonster> myMonster in myMonsters)
			{
				FieldMonster value = myMonster.Value;
				FieldRune value2 = null;
				runes.TryGetValue(myMonster.Key, out value2);
				bool flag = false;
				flag = value.IsRanged() || GetMonstersInFrontOf(value, myMonsters, side).Count == 0;
				float num2 = _aiCharacter.MonsterProfit(value, value2, atkCoef, hpCoef, myMonsters.Count, enemyMonsters.Count, flag);
				if (showProfitLog)
				{
					Debug.Log(string.Concat(value, ". profit: ", num2));
				}
				num += num2;
				if (showLog)
				{
					text = text + value.data.NameLog() + "(hp " + (string)value.Health + ", atk " + value.Attack + "), (" + num2 + ") " + value.coords;
				}
			}
			if (warlord != null)
			{
				float num3 = (((int)warlord.GetHealth() >= 0) ? warlordCoef : (warlordCoef * 10000f));
				float num4 = (float)(int)warlord.GetHealth() * num3;
				num += num4;
				if ((int)warlord.GetHealth() <= 0)
				{
					num += LOOSE_PROFIT;
					isDefeat = true;
				}
				if (showLog)
				{
					text = text + ". Warlord: hp " + (string)warlord.GetHealth() + "(" + num4 + ")";
				}
			}
			else
			{
				num += LOOSE_PROFIT;
				isDefeat = true;
			}
			if (showLog)
			{
				Debug.LogWarning(text);
			}
			return num;
		}

		private float KillEnemyUnitsProfit(Dictionary<Vector2, FieldMonster> enemyMonsters, Dictionary<Vector2, FieldMonster> myMonsters, Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> _armiesAfterPlace, Dictionary<Vector2, FieldRune> enemyRunes, ArmySide side, float targetCreaturesCoeff, bool showLog = false)
		{
			float num = 0f;
			float num2 = 0f;
			ArmySide key = side.OtherSide();
			foreach (KeyValuePair<Vector2, FieldMonster> item in _armiesAfterPlace[key].army)
			{
				FieldMonster value = item.Value;
				FieldMonster value2 = null;
				bool flag = false;
				if (!enemyMonsters.TryGetValue(item.Key, out value2) || !value2.IsEqual(value))
				{
					FieldRune value3 = null;
					enemyRunes.TryGetValue(item.Key, out value3);
					num += targetCreaturesCoeff * _aiCharacter.MonsterProfit(value, value3, _aiCharacter.enemyAtkCoef, _aiCharacter.enemyHpCoef, enemyMonsters.Count, myMonsters.Count);
				}
			}
			if (showLog)
			{
				Debug.Log("KillEnemyUnitsProfit forKill " + num + ", forDamage " + num2);
			}
			return num + num2;
		}

		private float SaveWarlordProfit(Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> simArmies, Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> armies, ArmySide side, CopiedSimulateRandom simRandom, float warordCoef, bool calcNextTurn, bool useLightMetric, out bool isDefeat)
		{
			float num = 0f;
			isDefeat = false;
			ArmySide armySide = side.OtherSide();
			Dictionary<Vector2, FieldMonster> army = simArmies[side].army;
			Dictionary<Vector2, FieldMonster> army2 = simArmies[armySide].army;
			FieldMonster warlord = simArmies[side].warlord;
			_ = simArmies[armySide].warlord;
			FieldMonster warlord2 = armies[side].warlord;
			int num2 = warlord.MaxHealth;
			int num3 = warlord.GetHealth();
			int num4 = (int)warlord2.GetHealth() - (int)warlord.GetHealth();
			if (calcNextTurn)
			{
				int num5 = 0;
				if (useLightMetric)
				{
					for (int i = 0; i < _fieldHeight; i++)
					{
						List<FieldMonster> list = new List<FieldMonster>(GetObjectsOnLineDict(i, army2, armySide).Values);
						if (armySide == ArmySide.Left)
						{
							list.Sort((FieldMonster m1, FieldMonster m2) => -m1.coords.x.CompareTo(m2.coords.x));
						}
						else
						{
							list.Sort((FieldMonster m1, FieldMonster m2) => m1.coords.x.CompareTo(m2.coords.x));
						}
						List<FieldMonster> objectsOnLine = GetObjectsOnLine(i, army, side);
						if (side == ArmySide.Left)
						{
							objectsOnLine.Sort((FieldMonster m1, FieldMonster m2) => -m1.coords.x.CompareTo(m2.coords.x));
						}
						else
						{
							objectsOnLine.Sort((FieldMonster m1, FieldMonster m2) => m1.coords.x.CompareTo(m2.coords.x));
						}
						for (int num6 = 0; num6 < list.Count; num6++)
						{
							FieldMonster fieldMonster = list[num6];
							bool flag = false;
							if (!fieldMonster.IsRanged() && GetMonstersInFrontOf(fieldMonster, army2, armySide).Count != 0)
							{
								continue;
							}
							if (objectsOnLine.Count > 0)
							{
								int attack = fieldMonster.Attack;
								Vector2 coords = objectsOnLine[0].coords;
								objectsOnLine[0].ChangeParam(null, ParamType.Health, -attack);
								bool flag2 = false;
								if ((int)objectsOnLine[0].Health <= 0)
								{
									flag2 = true;
									objectsOnLine.RemoveAt(0);
								}
								foreach (ActionBitSignature skill in fieldMonster.Skills)
								{
									switch (skill.signature)
									{
									case SkillType.Damage:
										if (skill.trigger == TriggerType.Attack)
										{
											num5 += skill.value;
										}
										break;
									case SkillType.Vampiric:
										if (!flag2)
										{
											objectsOnLine[0].ChangeParam(null, ParamType.Health, -skill.value);
											if ((int)objectsOnLine[0].Health <= 0)
											{
												flag2 = true;
												objectsOnLine.RemoveAt(0);
											}
										}
										break;
									case SkillType.Pierce:
									{
										for (int num7 = objectsOnLine.Count - 1; num7 > -1; num7--)
										{
											FieldMonster fieldMonster2 = objectsOnLine[num7];
											int value2 = skill.value;
											fieldMonster2.ChangeParam(null, ParamType.Health, -value2);
											if ((int)fieldMonster2.Health <= 0)
											{
												objectsOnLine.RemoveAt(num7);
											}
										}
										break;
									}
									case SkillType.SplashAttack:
									{
										List<Vector2> list2 = new List<Vector2>();
										foreach (KeyValuePair<Vector2, FieldMonster> item in army)
										{
											if (Math.Abs(item.Key.x - coords.x) == 1f || Math.Abs(item.Key.y - coords.y) == 1f)
											{
												int value = skill.value;
												item.Value.ChangeParam(null, ParamType.Health, -value);
												if ((int)item.Value.Health <= 0)
												{
													list2.Add(item.Key);
												}
											}
										}
										foreach (Vector2 item2 in list2)
										{
											army.Remove(item2);
										}
										break;
									}
									case SkillType.Silence:
										if (!flag2)
										{
											objectsOnLine[0].Silence();
										}
										break;
									case SkillType.ExtraAttack:
										if (objectsOnLine.Count > 0)
										{
											objectsOnLine[0].ChangeParam(null, ParamType.Health, -attack);
											if ((int)objectsOnLine[0].Health <= 0)
											{
												objectsOnLine.RemoveAt(0);
											}
										}
										else
										{
											num5 += fieldMonster.Attack;
										}
										break;
									}
								}
							}
							else
							{
								num5 += fieldMonster.Attack;
							}
						}
					}
				}
				else
				{
					SimulateEnviroment simulateEnviroment = SimulateEnviroment.Create();
					Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> afterFoughtArmies = null;
					simulateEnviroment.Init(simArmies, _fieldWidth, _fieldHeight, simRandom, _drawDelay, _drawShift);
					SimulateEnviroment.OnStepSimulated onSimulated = delegate(Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> afterFought)
					{
						afterFoughtArmies = afterFought;
					};
					simulateEnviroment.SimulateFight(side, onSimulated);
					num5 = num3 - (int)afterFoughtArmies[side].warlord.GetHealth();
				}
				num4 += num5;
				if (num3 - num4 <= 0)
				{
					num += LOOSE_PROFIT;
					isDefeat = true;
				}
			}
			float num8 = (float)(-num4) * Mathf.Clamp((float)num2 / (float)num3, 1f, 5f);
			return num + num8 * warordCoef;
		}

		private float LinePushProfit(Dictionary<Vector2, FieldMonster> myMonsters, Dictionary<Vector2, FieldMonster> enemyMonsters, Dictionary<Vector2, FieldRune> myRunes, Dictionary<Vector2, FieldRune> enemyRunes, AICharacter ai_char, ArmySide side)
		{
			ArmySide side2 = side.OtherSide();
			float a = 0f;
			float num = 0f;
			float num2 = ai_char.myHpCoef * 2f;
			float num3 = ai_char.myHpCoef / 2f;
			float num4 = ai_char.myAtkCoef * 2f;
			float num5 = ai_char.myAtkCoef / 2f;
			for (int i = 0; i < _fieldHeight; i++)
			{
				List<FieldMonster> objectsOnLine = GetObjectsOnLine(i, myMonsters, side);
				List<FieldMonster> objectsOnLine2 = GetObjectsOnLine(i, enemyMonsters, side2);
				float num6 = 0f;
				float num7 = 0f;
				foreach (FieldMonster item in objectsOnLine)
				{
					bool flag = false;
					if (item.IsRanged() || GetMonstersInFrontOf(item, myMonsters, side).Count == 0)
					{
						bool num8 = GetMonstersInFrontOf(item, myMonsters, side).Count > 0;
						FieldRune value = null;
						Vector2 coords = item.coords;
						myRunes.TryGetValue(coords, out value);
						float atkCoef = (num8 ? num4 : num5);
						float hpCoef = (num8 ? num3 : num2);
						float num9 = ai_char.MonsterProfit(item, value, atkCoef, hpCoef, myMonsters.Count, enemyMonsters.Count);
						num6 += num9;
					}
				}
				foreach (FieldMonster item2 in objectsOnLine2)
				{
					FieldRune value2 = null;
					Vector2 coords2 = item2.coords;
					myRunes.TryGetValue(coords2, out value2);
					float num10 = ai_char.MonsterProfit(item2, value2, ai_char.enemyAtkCoef, ai_char.enemyHpCoef, enemyMonsters.Count, myMonsters.Count);
					num7 += num10;
				}
				float num11 = num6 * (float)objectsOnLine.Count - num7;
				a = Mathf.Max(a, num11);
				num += num11;
			}
			return num / (float)_fieldHeight * ai_char.lineTargetCoef;
		}

		private List<FieldMonster> GetMonstersInFrontOf(FieldMonster mon, Dictionary<Vector2, FieldMonster> monsters, ArmySide side)
		{
			List<FieldMonster> list = new List<FieldMonster>();
			new List<int>();
			int num = (int)mon.coords.y;
			int num2 = -1;
			int num3 = -1;
			switch (side)
			{
			case ArmySide.Left:
				num2 = (int)mon.coords.x + 1;
				num3 = _fieldWidth / 2 - 1;
				break;
			case ArmySide.Right:
				num2 = _fieldWidth / 2;
				num3 = (int)mon.coords.x - 1;
				break;
			}
			for (int i = num2; i <= num3; i++)
			{
				FieldMonster value = null;
				Vector2 key = new Vector2(i, num);
				if (monsters.TryGetValue(key, out value))
				{
					list.Add(value);
				}
			}
			return list;
		}
	}
}
