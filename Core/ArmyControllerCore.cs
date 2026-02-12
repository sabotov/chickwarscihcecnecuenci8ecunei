using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DataClasses;
using Assets.Scripts.UtilScripts.Loaders;
using BattlefieldScripts.Actions;
using LevelSetup;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using NewAssets.Scripts.UtilScripts;
using ServiceLocator;
using UI_Scripts.WindowManager;
using UnityEngine;

namespace BattlefieldScripts.Core
{
	public class ArmyControllerCore
	{
		public enum DrawType
		{
			Normal = 0,
			Survival = 1,
			NewSurvival = 2,
			Pit = 3,
			GoldMine = 4
		}

		public const int HAND_SIZE = 4;

		protected const int SKILL_SIZE = 1;

		private readonly CachedService<IPitModule> ___pitModule = new CachedService<IPitModule>();

		private readonly CachedService<IDelayedActionsHandler> __delayedActionsHandler = new CachedService<IDelayedActionsHandler>();

		protected FieldControllerCore _controller;

		protected ArmyActionPerformer _actionPerformer;

		protected FieldParameters _parameters;

		protected IteratorCore _iterator;

		protected FieldRandom _random;

		protected List<MonsterData> _hand;

		protected List<MonsterData> _deck;

		protected List<int> _handDraw;

		protected List<MonsterData> _orderedDeck;

		protected List<MonsterData> _reserveMonsters = new List<MonsterData>();

		protected List<TriggerType> _skills;

		protected List<TriggerType> _availableSkills;

		protected FieldMonster _warlord;

		protected FieldMonster _pet;

		protected Dictionary<Vector2, FieldMonster> _fieldMonsters;

		protected Dictionary<Vector2, FieldRune> _runes;

		protected Action _onUpkeepFinished;

		protected Action _onPlacementFinished;

		protected Action _onActionFinished;

		protected Action _onEndingFinished;

		private bool _canPlace;

		private DrawType _drawType;

		private FieldScriptWrapper.StartBattleArmyParams.BattleType _battleType;

		private int _playerUseShuffleCount;

		private List<Rarity> _raritySequence;

		protected int _upkeepCount;

		protected int _upkeepVisualModifier;

		protected int _upkeepCountLeft;

		private bool isSameHand;

		private bool skillAnimataIsActive;

		private IPitModule _pitModule => ___pitModule.Value;

		private IDelayedActionsHandler _delayedActionsHandler => __delayedActionsHandler.Value;

		public ArmySide Side { get; protected set; }

		public ArmySide EnemySide
		{
			get
			{
				if (Side != ArmySide.Left)
				{
					return ArmySide.Left;
				}
				return ArmySide.Right;
			}
		}

		public bool PlayerCanUseShuffle
		{
			get
			{
				if (_battleType == FieldScriptWrapper.StartBattleArmyParams.BattleType.Pit)
				{
					return _playerUseShuffleCount < _pitModule.CardsHandler.MaxShuffleHandCount;
				}
				return false;
			}
		}

		public int UseShuffleCount => _playerUseShuffleCount;

		public ArmyControllerCore(ArmySide thisSide)
		{
			Side = thisSide;
		}

		public void Init(FieldControllerCore controller, ArmyActionPerformer actionPerformer, FieldParameters fieldParameters, FieldRandom random, IteratorCore iterator, DrawType drawType, FieldScriptWrapper.StartBattleArmyParams.BattleType battleType)
		{
			_actionPerformer = actionPerformer;
			_controller = controller;
			_parameters = fieldParameters;
			_random = random;
			_iterator = iterator;
			_drawType = drawType;
			_battleType = battleType;
			_fieldMonsters = new Dictionary<Vector2, FieldMonster>();
			_runes = new Dictionary<Vector2, FieldRune>();
			_reserveMonsters = new List<MonsterData>();
			_raritySequence = new List<Rarity>();
			_actionPerformer.AttachRandomCopyGetter(_random.GetSimulateCopy);
			_playerUseShuffleCount = 0;
		}

		public void AttachRaritySequence(List<Rarity> rarSeq)
		{
			_raritySequence = rarSeq;
		}

		protected List<TriggerType> GetThisStepSkills()
		{
			if (_skills.Count == 0)
			{
				return _skills;
			}
			TriggerType triggerType = TriggerType.BlockPerformed;
			foreach (TriggerType skill in _skills)
			{
				if (skill < triggerType)
				{
					triggerType = skill;
				}
			}
			return new List<TriggerType> { triggerType };
		}

		public virtual void CreateArmy()
		{
			_hand = new List<MonsterData>();
			_skills = new List<TriggerType>();
			_availableSkills = new List<TriggerType> { TriggerType.WarlordSkill1 };
		}

		public void AddSkillTimerPoint()
		{
			_upkeepCount++;
			_upkeepVisualModifier--;
		}

		public virtual bool CanPlaceMonster(Vector2 place)
		{
			return !_fieldMonsters.ContainsKey(place);
		}

		public void GenerateHand()
		{
			if (_drawType == DrawType.Normal || _drawType == DrawType.NewSurvival || _drawType == DrawType.Pit)
			{
				if (_handDraw != null && _handDraw.Count > 0)
				{
					int num = Mathf.Min(_handDraw[0], 4 - _hand.Count);
					DrawCards(num, delegate
					{
					}, silent: true);
					isSameHand = true;
				}
				else
				{
					int num2 = 4 - _hand.Count;
					DrawCards(num2, delegate
					{
					}, silent: true);
				}
				_actionPerformer.InformHandCreated(_hand, _skills);
			}
			if (_drawType == DrawType.Survival)
			{
				List<MonsterData> list = _deck.FindAll((MonsterData x) => true);
				_deck = _deck.FindAll((MonsterData x) => x.rarity != Rarity.COMMON);
				DrawCards(4 - _hand.Count, delegate
				{
				}, silent: true);
				foreach (MonsterData item in _hand)
				{
					list.Remove(item);
				}
				_deck = list;
				DrawCards(4 - _hand.Count, delegate
				{
				}, silent: true);
				_actionPerformer.InformHandCreated(_hand, _skills);
			}
			_actionPerformer.InformEnemyTurnStarted(fromGenerateHand: true);
		}

		public void ShuffleHand(bool playerUseShuffle)
		{
			if (!playerUseShuffle || PlayerCanUseShuffle)
			{
				bool num = _battleType == FieldScriptWrapper.StartBattleArmyParams.BattleType.Pit;
				int count = GetHand().Count;
				if (num)
				{
					_pitModule.CardsHandler.UpdateCardBattleStates();
				}
				else
				{
					AddMonstersToDeck(GetHand());
				}
				RemoveCardsFromHand(new List<MonsterData>(GetHand()));
				DrawCards(count, delegate
				{
				}, silent: false, withoutSkillCard: true);
				if (playerUseShuffle)
				{
					_playerUseShuffleCount++;
				}
				OnShuffleUsed();
			}
		}

		public Dictionary<Vector2, FieldMonster> GetFieldMonsters()
		{
			return _fieldMonsters;
		}

		public Dictionary<Vector2, FieldRune> GetFieldRunes()
		{
			return _runes;
		}

		public FieldMonster GetWarlord()
		{
			return _warlord;
		}

		public FieldMonster GetPet()
		{
			return _pet;
		}

		public List<MonsterData> GetDeck()
		{
			return _deck;
		}

		public void ForceEnablePlacing()
		{
			_canPlace = true;
		}

		public List<MonsterData> GetReserveMonster()
		{
			return _reserveMonsters;
		}

		public List<MonsterData> GetHand()
		{
			return _hand;
		}

		public List<MonsterData> GetStartDeck()
		{
			List<MonsterData> list = new List<MonsterData>();
			list.AddRange(_deck);
			list.AddRange(_hand);
			list.AddRange(_reserveMonsters);
			return list;
		}

		public List<TriggerType> GetSkills()
		{
			return _skills;
		}

		public DrawType GetDrawType()
		{
			return _drawType;
		}

		public FieldScriptWrapper.StartBattleArmyParams.BattleType GetBattleType()
		{
			return _battleType;
		}

		public List<Rarity> GetRaritySequence()
		{
			return _raritySequence;
		}

		public int GetUpkeepCount(bool visual = false)
		{
			return _upkeepCount + (visual ? _upkeepVisualModifier : 0);
		}

		public List<TriggerType> GetAvailableSkills()
		{
			return _availableSkills;
		}

		public Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> GetArmiesStates()
		{
			return _controller.GetArmiesStates();
		}

		public bool CanUseSkill(TriggerType trigger, int turn)
		{
			int turnUntilSkill = GetTurnUntilSkill(trigger, turn);
			if (trigger == TriggerType.WarlordSkill4)
			{
				if (turnUntilSkill <= 0 && turnUntilSkill % 1 == 0)
				{
					return true;
				}
			}
			else if (turnUntilSkill == 0)
			{
				return true;
			}
			return false;
		}

		private int GetTurnUntilSkill(TriggerType trigger, int turn)
		{
			return MonsterDataUtils.GetTurnForSkill(trigger, _warlord.data.skills, _parameters.skillDrawDelay) - turn;
		}

		public bool CanPlace()
		{
			return _canPlace;
		}

		public void PerformUpkeepPhase(Action onFinished)
		{
			_upkeepCount++;
			if (Side == ArmySide.Left)
			{
				_upkeepCountLeft++;
			}
			int turn = _upkeepCount;
			if (!CheckUpkeepEquality())
			{
				turn = _upkeepCount - 1;
			}
			TriggerType triggerType = MonsterDataUtils.AvailableTrigger(turn, _warlord.data.skills, _parameters.skillDrawDelay);
			if (CanUseSkill(triggerType, turn))
			{
				TriggerType triggerType2 = triggerType;
				if (!_availableSkills.Contains(triggerType2))
				{
					_availableSkills.Add(triggerType2);
					if (!(this is SimulateArmyController))
					{
						Debug.Log(string.Concat("PerformUpkeepPhase add skill ", triggerType2, " to ", Side));
					}
				}
			}
			_onUpkeepFinished = onFinished;
			Action onCompleted = delegate
			{
				_actionPerformer.InformTurnStarted(_hand, _skills);
				if (_handDraw != null && _handDraw.Count > 0)
				{
					int num = Mathf.Min(_handDraw[0], 4 - _hand.Count);
					DrawCards((!isSameHand) ? num : 0, delegate
					{
						if (_deck.Count == 0)
						{
							_deck.AddRange(_reserveMonsters);
							_reserveMonsters.Clear();
						}
						_onUpkeepFinished();
					});
					isSameHand = false;
					_handDraw.Remove(_handDraw[0]);
				}
				else
				{
					int num2 = 4 - _hand.Count;
					DrawCards(num2, delegate
					{
						if (_deck.Count == 0)
						{
							_deck.AddRange(_reserveMonsters);
							_reserveMonsters.Clear();
						}
						_onUpkeepFinished();
					});
				}
			};
			PerformTrigger(TriggerType.NewTurn, SkillType.NoSkill, Vector2.zero, null, null, onCompleted, checkDeath: true);
		}

		public bool CheckUpkeepEquality()
		{
			return _upkeepCountLeft == _upkeepCount;
		}

		public void PerformPlacementPhase(Action onFinished)
		{
			_canPlace = true;
			_onPlacementFinished = onFinished;
			List<TriggerType> thisStepSkills = GetThisStepSkills();
			if (HaveNoAvailableActions(thisStepSkills))
			{
				OnPlacementChoosed(null, Vector2.zero);
			}
			else
			{
				_actionPerformer.PerformPlacingChoose(_hand, thisStepSkills, OnPlacementChoosed, OnSkillUsed, isAfterSkill: false, _upkeepCount);
			}
		}

		public void PerformActionPhase(Action onFinished)
		{
			_onActionFinished = onFinished;
			FightMonsters();
		}

		public void PerformEndingPhase(Action onFinished)
		{
			_onEndingFinished = delegate
			{
				if (_drawType == DrawType.NewSurvival)
				{
					_deck.AddRange(_reserveMonsters);
					_reserveMonsters.Clear();
					_deck.AddRange(_hand);
					_hand.Clear();
					DrawCards(4 - _hand.Count, onFinished);
				}
				else
				{
					onFinished();
				}
				_actionPerformer.InformTurnEnded();
				InformEndingPhase();
			};
			PerformTrigger(TriggerType.TurnEnded, SkillType.NoSkill, Vector2.zero, null, null, delegate
			{
				_onEndingFinished();
			}, checkDeath: true);
		}

		protected virtual void InformEndingPhase()
		{
		}

		public void DrawCards(int num, Action onCompleted, bool silent = false, bool withoutSkillCard = false)
		{
			List<MonsterData> list = new List<MonsterData>();
			for (int i = 0; i < num && _deck.Count > 0; i++)
			{
				MonsterData monsterData;
				if (_drawType == DrawType.NewSurvival)
				{
					int num2 = GetUpkeepCount(visual: true) + 1;
					if (num2 == 0)
					{
						num2 = 1;
					}
					while (num2 > _raritySequence.Count)
					{
						num2 -= _raritySequence.Count;
					}
					Rarity curRar = _raritySequence[num2 - 1];
					List<MonsterData> list2 = _deck.FindAll((MonsterData x) => x.rarity == curRar && x.canCollect);
					if (list2.Count == 0 && curRar >= Rarity.COMMON)
					{
						list2 = _deck.FindAll((MonsterData x) => x.rarity == curRar - 1);
					}
					if (list2.Count == 0 && curRar >= Rarity.UNCOMMON)
					{
						list2 = _deck.FindAll((MonsterData x) => x.rarity == curRar - 2);
					}
					if (list2.Count == 0 && curRar >= Rarity.RARE)
					{
						list2 = _deck.FindAll((MonsterData x) => x.rarity == curRar - 3);
					}
					if (list2.Count == 0 && curRar >= Rarity.EPIC)
					{
						list2 = _deck.FindAll((MonsterData x) => x.rarity == curRar - 4);
					}
					if (list2.Count == 0 && curRar >= Rarity.LEGENDARY)
					{
						list2 = _deck.FindAll((MonsterData x) => x.rarity == curRar - 5);
					}
					if (list2.Count == 0)
					{
						list2 = _deck;
					}
					monsterData = list2[_random.GetRange(0, list2.Count)];
					_deck.Remove(monsterData);
				}
				else if (_drawType == DrawType.Pit && (_actionPerformer is PlayerArmyActionPerformer || (_actionPerformer is TestArmyActionPerformer && Side == ArmySide.Left)))
				{
					monsterData = _pitModule.CardsHandler.GetNextCard();
					if (monsterData == null)
					{
						continue;
					}
				}
				else if (_orderedDeck == null || _orderedDeck.Count == 0)
				{
					monsterData = _deck[_random.GetRange(0, _deck.Count)];
					_deck.Remove(monsterData);
				}
				else
				{
					monsterData = _orderedDeck[0];
					_orderedDeck.Remove(monsterData);
				}
				list.Add(monsterData);
			}
			if (!(this is SimulateArmyController))
			{
				MonsterAnimationAssetManager.Instance.LoadElements(list.ConvertAll((MonsterData a) => a.animationName), delegate
				{
				});
			}
			List<TriggerType> list3 = new List<TriggerType>();
			while (_availableSkills.Count > 0 && !withoutSkillCard)
			{
				TriggerType trigger = _availableSkills[0];
				if (_warlord.data.skills.Find((SkillStaticData x) => x.trigger == trigger) != null && !list3.Contains(trigger))
				{
					list3.Add(trigger);
				}
				_availableSkills.Remove(trigger);
			}
			AddCardsToHand(list, list3, onCompleted, silent);
		}

		public void AddCardsToHand(MonsterData newCard, Action onCompleted)
		{
			AddCardsToHand(new List<MonsterData> { newCard }, new List<TriggerType>(), onCompleted);
		}

		public void AddCardsToHand(TriggerType newCard, Action onCompleted)
		{
			AddCardsToHand(new List<MonsterData>(), new List<TriggerType> { newCard }, onCompleted);
		}

		public void AddCardsToHand(List<MonsterData> newCards, List<TriggerType> newSkills, Action onCompleted, bool silent = false)
		{
			_hand.AddRange(newCards);
			_skills.AddRange(newSkills);
			if (!silent)
			{
				_actionPerformer.PerformCarsAddedToHand(newCards, newSkills, onCompleted);
			}
			else
			{
				onCompleted();
			}
		}

		public void AttachOrderedDeck(List<MonsterData> oDeck)
		{
			_orderedDeck = new List<MonsterData>(oDeck);
		}

		public void RemoveCardsFromHand(MonsterData card)
		{
			List<MonsterData> list = new List<MonsterData> { card };
			RemoveCardsFromHand(list);
			if (_drawType == DrawType.Pit && (_actionPerformer is PlayerArmyActionPerformer || (_actionPerformer is TestArmyActionPerformer && Side == ArmySide.Left)))
			{
				_pitModule.CardsHandler.AddToLocked(list);
			}
		}

		public void RemoveCardsFromHand(List<MonsterData> cards)
		{
			foreach (MonsterData card in cards)
			{
				_hand.Remove(card);
			}
			_actionPerformer.InformCarsRemovedFromHand(cards);
		}

		public void AddMonstersToDeck(List<MonsterData> monsters)
		{
			_deck.AddRange(monsters);
		}

		public void MoveFieldMonstersToHand()
		{
			foreach (KeyValuePair<Vector2, FieldMonster> fieldMonster in _fieldMonsters)
			{
				_deck.Add(fieldMonster.Value.data);
				fieldMonster.Value.visualElement.Destroy();
			}
			_fieldMonsters.Clear();
		}

		public virtual void InformEnemyMonsterDead(Vector2 coords, FieldMonster monster)
		{
		}

		public virtual void InformMonsterDead(Vector2 coords, FieldMonster monster)
		{
		}

		public virtual void InformEnemyMonsterHit(Vector2 coords, FieldMonster monster)
		{
		}

		public virtual void InformMonsterHit(Vector2 coords, FieldMonster monster)
		{
		}

		public virtual void PerformMonsterDead(Vector2 coords)
		{
			if (coords == _warlord.coords)
			{
				_controller.InformMonsterDead(Side, coords, _warlord);
				_controller.InformDefeat(Side);
			}
			else
			{
				_controller.InformMonsterDead(Side, coords, _fieldMonsters[coords]);
				_fieldMonsters.Remove(coords);
			}
		}

		public virtual void PerformMonsterHit(Vector2 coords)
		{
			if (coords == _warlord.coords)
			{
				_controller.InformMonsterHit(Side, coords, _warlord);
			}
			else
			{
				_controller.InformMonsterHit(Side, coords, _fieldMonsters[coords]);
			}
		}

		public void BroadcastAction(TriggerType trigger, SkillType origin, Vector2 actionPlace, FieldElement actionParameter, FieldElement affectedParameter, Action onPerformed, object param = null)
		{
			_controller.BroadcastAction(Side, trigger, origin, actionPlace, actionParameter, affectedParameter, onPerformed, param);
		}

		public void BroadcastDeathRecheck(ArmySide armySide, Action onCompl)
		{
			_controller.BroadcastDeathRecheck(armySide, onCompl);
		}

		public void RecheckDeath(Action onCompleted)
		{
			Action action = delegate
			{
				_iterator.IterateOnActions(PlaceIteration(), delegate(Vector2 place, Action step)
				{
					if (_fieldMonsters.ContainsKey(place) && _fieldMonsters[place].ShouldDie)
					{
						_fieldMonsters[place].CheckDeath(step);
					}
					else
					{
						step();
					}
				}, onCompleted);
			};
			if (_warlord != null && _warlord.ShouldDie)
			{
				_warlord.CheckDeath(action);
			}
			else
			{
				action();
			}
		}

		public virtual void AnimateVictory()
		{
			if (_pet != null)
			{
				_pet.AnimateVictory();
			}
			_warlord.AnimateVictory();
			foreach (KeyValuePair<Vector2, FieldMonster> fieldMonster in _fieldMonsters)
			{
				fieldMonster.Value.AnimateVictory();
			}
		}

		public virtual void AnimateDefeat()
		{
			_warlord.AnimateDefeat();
			foreach (KeyValuePair<Vector2, FieldMonster> fieldMonster in _fieldMonsters)
			{
				fieldMonster.Value.AnimateDefeat();
			}
		}

		public void DestroyRunes()
		{
			foreach (KeyValuePair<Vector2, FieldRune> rune in _runes)
			{
				rune.Value.visualElement.Destroy();
			}
			_runes.Clear();
		}

		public virtual void PlaceRune(RuneData runeData, Vector2 place)
		{
			if (_parameters.GetArmyTiles(EnemySide).Contains(place))
			{
				_controller.RequestRuneAdding(Side, runeData, place);
				return;
			}
			FieldRune fieldRune = new FieldRune
			{
				coords = place
			};
			fieldRune.Init(this, runeData, null, _parameters, _random, _iterator);
			_runes.Add(place, fieldRune);
		}

		public void PlaceMonster(MonsterData monster, Vector2 place, Action onPlaced, FieldMonster copyTarget = null, Action<FieldMonster> onPlacedAction = null, bool fromReborn = false)
		{
			if (_parameters.GetArmyTiles(EnemySide).Contains(place))
			{
				_controller.RequestMonsterSummon(Side, monster, place, onPlaced, copyTarget, onPlacedAction, fromReborn);
				return;
			}
			SilentlyPlaceMonster(monster, place);
			FieldMonster mon = _fieldMonsters[place];
			if (copyTarget != null)
			{
				mon.CopyStatus(copyTarget);
			}
			onPlacedAction?.Invoke(mon);
			Action onEffect = delegate
			{
				BroadcastAction(TriggerType.Appear, SkillType.NoSkill, place, mon, mon, onPlaced);
			};
			AnimateAppearEffect(mon, onEffect);
		}

		public void DestroyRune(FieldRune fieldRune)
		{
			if (_parameters.GetArmyTiles(EnemySide).Contains(fieldRune.coords))
			{
				_controller.RequestRuneDeleting(Side, fieldRune);
				return;
			}
			_runes.Remove(fieldRune.coords);
			if (fieldRune.visualElement != null)
			{
				fieldRune.visualElement.Destroy();
			}
		}

		private void UpdateMonsterCounterOnNewTurn(FieldElement monster)
		{
			foreach (ActionBit action in monster.Actions)
			{
				if (action._startCounter != -1)
				{
					action._counter = action._startCounter;
					if (action._startCounter == 1)
					{
						action._counter++;
					}
				}
			}
		}

		public void PerformTrigger(TriggerType trigger, SkillType originSkill, Vector2 actionPlace, FieldElement actionParameter, FieldElement affectedParameter, Action onCompleted, bool checkDeath = false, object param = null)
		{
			Action<Action> deathRecheck = delegate(Action onCompl)
			{
				if (checkDeath)
				{
					_controller.BroadcastDeathRecheck(Side, onCompl);
				}
				else
				{
					onCompl();
				}
			};
			Action<Action> tryStep = delegate(Action onCompl)
			{
				if (checkDeath)
				{
					deathRecheck(onCompl);
				}
				else
				{
					onCompl();
				}
			};
			Action<FieldElement, Action> performAction = delegate(FieldElement elem, Action onCompl)
			{
				if (trigger == TriggerType.NewTurn)
				{
					UpdateMonsterCounterOnNewTurn(elem);
				}
				if (elem.ShouldPerformAction(trigger, originSkill, actionPlace, actionParameter, affectedParameter, param))
				{
					elem.PerformAction(trigger, originSkill, actionPlace, actionParameter, affectedParameter, delegate
					{
						tryStep(onCompl);
					}, deathRecheck, param);
				}
				else
				{
					elem.OnTriggerCompleted(onCompl, trigger, somethingPerformed: false);
				}
			};
			performAction(_warlord, delegate
			{
				_iterator.IterateOnActions(PlaceIteration(), delegate(Vector2 place, Action onCompl)
				{
					if (_fieldMonsters.ContainsKey(place))
					{
						performAction(_fieldMonsters[place], onCompl);
					}
					else
					{
						onCompl();
					}
				}, delegate
				{
					if (_pet == null)
					{
						_iterator.IterateOnActions(PlaceIteration(), delegate(Vector2 place, Action onCompl)
						{
							if (_runes.ContainsKey(place))
							{
								performAction(_runes[place], onCompl);
							}
							else
							{
								onCompl();
							}
						}, onCompleted);
					}
					else
					{
						performAction(_pet, delegate
						{
							_iterator.IterateOnActions(PlaceIteration(), delegate(Vector2 place, Action onCompl)
							{
								if (_runes.ContainsKey(place))
								{
									performAction(_runes[place], onCompl);
								}
								else
								{
									onCompl();
								}
							}, onCompleted);
						});
					}
				});
			});
		}

		private IEnumerable<Vector2> PlaceIteration()
		{
			int delta = ((Side != ArmySide.Left) ? 1 : (-1));
			int startX = ((Side == ArmySide.Left) ? (_parameters.width / 2 - 1) : (_parameters.width / 2));
			int endX = ((Side != ArmySide.Left) ? (_parameters.width - 1) : 0);
			for (int i = 0; i < _parameters.height; i++)
			{
				for (int j = startX; j * delta <= endX * delta; j += delta)
				{
					yield return new Vector2(j, i);
				}
			}
		}

		protected virtual void OnPlasementFinished()
		{
			_onPlacementFinished();
		}

		private bool HaveNoAvailableActions(List<TriggerType> thisStepSkills)
		{
			if (_hand.Count != 0 || thisStepSkills.Count != 0)
			{
				return !_actionPerformer.IsPlacingAvailable(_hand, _skills, PlayerCanUseShuffle);
			}
			return true;
		}

		protected virtual void OnSkillSelected()
		{
			_canPlace = true;
			List<TriggerType> thisStepSkills = GetThisStepSkills();
			if (HaveNoAvailableActions(thisStepSkills))
			{
				OnPlacementChoosed(null, Vector2.zero);
			}
			else
			{
				_actionPerformer.PerformPlacingChoose(_hand, thisStepSkills, OnPlacementChoosed, OnSkillUsed, isAfterSkill: true);
			}
		}

		public virtual void OnSkillReplaced(TriggerType trigger)
		{
			_skills.Remove(trigger);
			_actionPerformer.InformSkillRemovedFromHand(trigger);
		}

		protected virtual void OnSkillUsed(TriggerType trigger)
		{
			if (_skills.Any((TriggerType x) => x < trigger))
			{
				Debug.LogError(string.Concat("Cannot perform trigger ", trigger, " because there is lower trigger!"));
				return;
			}
			if (!_skills.Contains(trigger))
			{
				Debug.LogError(string.Concat("Cannot perform trigger ", trigger, " because it isnt in hand!"));
				return;
			}
			if (!_canPlace)
			{
				Debug.LogError("Cannot place something, it's not my turn!");
				return;
			}
			ShowUseSkillWarlordAnimation();
			_delayedActionsHandler.WaitForProcedure(FadeManager.FADE_IN_TIME_FOR_WARLORD, delegate
			{
				_canPlace = false;
				Action onCompleted = OnSkillSelected;
				_skills.Remove(trigger);
				_actionPerformer.InformSkillRemovedFromHand(trigger);
				Action onCompleted2 = delegate
				{
					_delayedActionsHandler.WaitForCondition(() => !skillAnimataIsActive, delegate
					{
						BroadcastDeathRecheck(Side, delegate
						{
						});
						onCompleted();
						if (trigger != TriggerType.WarlordSkill4)
						{
							_warlord.Silence(trigger);
						}
					});
				};
				PerformTrigger(trigger, SkillType.NoSkill, Vector2.zero, null, null, onCompleted2, checkDeath: true);
			});
		}

		private void ShowUseSkillWarlordAnimation()
		{
			if (!skillAnimataIsActive)
			{
				if (_pet != null)
				{
					_pet.AnimateUseSkill();
				}
				_warlord.AnimateUseSkill();
			}
		}

		protected virtual void OnPlacementChoosed(MonsterData monster, Vector2 place)
		{
			if (!_hand.Contains(monster) && monster != null)
			{
				Debug.LogError("Cannot place monster " + monster.monster_id + " because it isnt in hand!");
				return;
			}
			if (_fieldMonsters.ContainsKey(place) && monster != null)
			{
				Debug.LogError(string.Concat("Cannot place monster to ", place, " because somebody here!"));
				return;
			}
			if (!_canPlace)
			{
				Debug.LogError("Cannot place something, it's not my turn!");
				return;
			}
			_canPlace = false;
			Action action = OnPlasementFinished;
			if (monster != null)
			{
				BattleStatisticsModule.HandleUseCard(place.x < 3f, monster.monster_id);
				RemoveCardsFromHand(monster);
				PlaceMonster(monster, place, action);
				_reserveMonsters.Add(monster);
			}
			else
			{
				action();
			}
		}

		protected virtual void SilentlyPlaceMonster(MonsterData monster, Vector2 place)
		{
			FieldMonster fieldMonster = new FieldMonster
			{
				coords = place
			};
			fieldMonster.Init(this, monster, null, _parameters, _random, _iterator);
			try
			{
				_fieldMonsters.Add(place, fieldMonster);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Concat("SilentlyPlaceMonster ", monster.image, " ", place, " ", ex));
			}
		}

		protected virtual void AnimateAppearEffect(FieldMonster mon, Action onEffect)
		{
			onEffect();
		}

		protected virtual void OnShuffleUsed()
		{
			List<TriggerType> thisStepSkills = GetThisStepSkills();
			if (HaveNoAvailableActions(thisStepSkills))
			{
				OnPlacementChoosed(null, Vector2.zero);
			}
		}

		protected void FightMonsters()
		{
			PerformTrigger(TriggerType.Attack, SkillType.NoSkill, Vector2.zero, null, null, delegate
			{
				_onActionFinished();
			}, checkDeath: true);
		}

		protected virtual void StopAllTriggers()
		{
			_iterator.Break();
			foreach (KeyValuePair<Vector2, FieldMonster> fieldMonster in _fieldMonsters)
			{
				fieldMonster.Value.StopAllActions();
			}
			_actionPerformer.InformStop();
		}

		protected int OrderSort(Vector2 v1, Vector2 v2)
		{
			return (int)Mathf.Sign(v1.y * 1000f - v2.y * 1000f + (v1.x - v2.x) * (float)((Side != ArmySide.Left) ? 1 : (-1)));
		}

		public virtual void InformEnemySpeech(FastDialogData.Event trigger)
		{
		}

		public void InformEnemyTurnStarted()
		{
			_actionPerformer.InformEnemyTurnStarted();
		}

		public void InformEnemyTurnEnded()
		{
			_actionPerformer.InformEnemyTurnEnded();
		}

		public void InformFightCompleted()
		{
			_actionPerformer.InformFightCompleted();
		}

		public virtual void InformVictory()
		{
			StopAllTriggers();
		}

		public void InformDefeat()
		{
			StopAllTriggers();
		}

		public void InformError()
		{
			StopAllTriggers();
		}

		public void InformLeave()
		{
			StopAllTriggers();
		}
	}
}
