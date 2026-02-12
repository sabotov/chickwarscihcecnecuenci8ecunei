using System;
using System.Collections.Generic;
using System.Linq;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using UnityEngine;

namespace BattlefieldScripts
{
	public abstract class ArmyActionPerformer
	{
		public delegate IEnumerable<Vector2> PossiblePlacesDelegate(ArmySide side, Dictionary<Vector2, FieldMonster> monsters, MonsterData monster);

		protected Func<List<MonsterData>> _enemyHand;

		protected Func<List<TriggerType>> _enemySkills;

		protected Func<CopiedSimulateRandom> _simulateRandomCopier;

		protected ArmySide _side;

		protected ArmyControllerCore _thisController;

		protected FieldParameters _parameters;

		protected ArmySide EnemySide
		{
			get
			{
				if (_side != ArmySide.Left)
				{
					return ArmySide.Left;
				}
				return ArmySide.Right;
			}
		}

		public virtual void Init(Func<List<MonsterData>> enemyHand, Func<List<TriggerType>> enemySkills, ArmyControllerCore thisController, FieldParameters parameters)
		{
			_thisController = thisController;
			_side = thisController.Side;
			_enemyHand = enemyHand;
			_enemySkills = enemySkills;
			_parameters = parameters;
		}

		public void AttachRandomCopyGetter(Func<CopiedSimulateRandom> getter)
		{
			_simulateRandomCopier = getter;
		}

		public Func<CopiedSimulateRandom> GetSRandomCopyGetter()
		{
			return _simulateRandomCopier;
		}

		public bool IsPlacingAvailable(List<MonsterData> hand, List<TriggerType> skillTriggers, bool playerCanUseShuffle)
		{
			bool num = hand.Find((MonsterData x) => x.monsterClass == Class.Ranged) != null && _parameters.GetClassedTiles(Class.Ranged, _side).Any();
			bool flag = hand.Find((MonsterData x) => x.monsterClass == Class.Melee) != null && _parameters.GetClassedTiles(Class.Melee, _side).Any();
			bool flag2 = hand.Find((MonsterData x) => x.monsterClass == Class.Building) != null && _parameters.GetClassedTiles(Class.Building, _side).Any();
			return num || flag || flag2 || skillTriggers.Count > 0 || playerCanUseShuffle;
		}

		public virtual void PerformPlacingChoose(List<MonsterData> hand, List<TriggerType> skillTriggers, Action<MonsterData, Vector2> onChosen, Action<TriggerType> onSkill, bool isAfterSkill = false, int currentTurn = 0)
		{
			throw new NotImplementedException("PerformPlacingChoose should be overriden");
		}

		public virtual void PerformCarsAddedToHand(List<MonsterData> cards, List<TriggerType> skills, Action onCompleted)
		{
			onCompleted();
		}

		public virtual void InformTurnEnded()
		{
		}

		public virtual void InformArmyCreated()
		{
		}

		public virtual void InformEnemySpeech(FastDialogData.Event trigger)
		{
		}

		public virtual void InformHandCreated(List<MonsterData> cards, List<TriggerType> skills)
		{
		}

		public virtual void InformEnemyTurnStarted(bool fromGenerateHand = false)
		{
		}

		public virtual void InformEnemyTurnEnded(bool fromGenerateHand = false)
		{
		}

		public virtual void InformFightCompleted()
		{
		}

		public virtual void InformMonsterDead(FieldMonster data, bool isWarlord = false)
		{
		}

		public virtual void InformMonsterHit(FieldMonster data, bool isWarlord = false)
		{
		}

		public virtual void InformTurnStarted(List<MonsterData> hand, List<TriggerType> skillTriggers)
		{
		}

		public virtual void InformCarsRemovedFromHand(List<MonsterData> cards)
		{
		}

		public virtual void InformSkillRemovedFromHand(TriggerType cards)
		{
		}

		public virtual void InformSkillsRemovedFromHand(List<TriggerType> cards)
		{
		}

		public virtual void InformStop()
		{
		}

		protected List<MonsterData> GetEnemyGetStartDeck()
		{
			return _parameters.GetStartDeck(EnemySide);
		}

		protected List<MonsterData> GetStartDeck()
		{
			return _parameters.GetStartDeck(_side);
		}

		protected List<MonsterData> GetEnemyDeck()
		{
			return _parameters.GetDeck(EnemySide);
		}

		protected List<MonsterData> GetDeck()
		{
			return _parameters.GetDeck(_side);
		}

		protected List<MonsterData> GetEnemyHand()
		{
			return _parameters.GetHand(EnemySide);
		}

		protected List<MonsterData> GetHand()
		{
			return _parameters.GetHand(_side);
		}

		protected string GetValue(string valStr, int addValue = 0)
		{
			string result = valStr;
			if (valStr.Length > 10 && valStr.Substring(0, 10) == "turnNumber")
			{
				bool flag = false;
				int num = 0;
				int num2 = 0;
				bool flag2 = true;
				bool flag3 = true;
				string text = valStr.Replace(" ", "").Replace("turnNumber", "");
				for (int i = 0; i < text.Length; i++)
				{
					char c = text[i];
					switch (c)
					{
					case '*':
						flag = true;
						break;
					case '/':
						flag = false;
						break;
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						if (flag2)
						{
							num *= 10;
							num += int.Parse(c.ToString() ?? "");
						}
						else
						{
							num2 *= 10;
							num2 += int.Parse(c.ToString() ?? "");
						}
						break;
					case '+':
						flag2 = false;
						break;
					case '-':
						flag3 = false;
						flag2 = false;
						break;
					}
				}
				if (!flag3)
				{
					num2 *= -1;
				}
				int num3 = _parameters.GetTurn(visual: true) + addValue;
				result = string.Concat((flag ? (num3 * num) : ((num3 - num3 % num) / num)) + num2);
			}
			return result;
		}

		public virtual bool IsPlaceAvailableToPlace(FieldElement arg)
		{
			return true;
		}
	}
}
