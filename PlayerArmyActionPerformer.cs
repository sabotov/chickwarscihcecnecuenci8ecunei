using System;
using System.Collections.Generic;
using System.Linq;
using ActionBehaviours;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using BattlefieldScripts.SideControllers;
using MyVisual;
using NGUI.Scripts.Internal;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using NewAssets.Scripts.DataClasses.UserData;
using NewAssets.Scripts.Data_Helpers;
using NewAssets.Scripts.UI_Scripts.UI_Controller_Scripts.BattleControllers;
using NewAssets.Scripts.UtilScripts;
using ServiceLocator;
using Tutorial;
using UI_Scripts.WindowManager;
using UnityEngine;
using UserData;
using UtilScripts;
using UtilScripts.ControlScripts;

namespace BattlefieldScripts
{
	public class PlayerArmyActionPerformer : ArmyActionPerformer
	{
		private class HighlightElement : FieldElement
		{
			public void Init(Vector2 thisCoords, ArmyControllerCore curController)
			{
				coords = thisCoords;
				_curController = curController;
			}
		}

		private readonly CachedService<IDelayedActionsHandler> __delayedActionsHandler = new CachedService<IDelayedActionsHandler>();

		private static readonly Dictionary<string, GameObject> SkillEffects = new Dictionary<string, GameObject>();

		private FieldCreator _creator;

		private BattlefieldWindow _window;

		private PlayerAssistant _playerAssistant;

		private UserHandController _guiController;

		private UserSkillController _skillController;

		private ChangeStateButton _autoplayButton;

		private Card _selectedCard;

		private Action<MonsterData, Vector2> _onChosen;

		private Action<TriggerType> _onSkill;

		private AIArmyActionPerformer _autoplayPerformer;

		private bool _isChoosing;

		private bool _forceAnimatedAutofight;

		private List<MonsterData> _curHand;

		private List<TriggerType> _curSkills;

		private List<Tile> _availableTiles = new List<Tile>();

		private readonly List<int> _delayedProceduresIds = new List<int>();

		public bool sendPlaceQuest = true;

		public string levelEffectDescr = "";

		private bool scaleUpEffectShown;

		private int assistantTimerProcId = -1;

		private Action _cardSelectedPerform;

		private IBattleActionPerformer _animatedBattleActionPerformer = new AnimatedBattleActionPerformer();

		private bool _delayedSkillShown;

		private int[] _arrayDelayFromConfig = new int[4];

		private static readonly HashSet<string> autoplayBlockers = new HashSet<string>();

		private BubbleSelectElementContainer _selectBubbles;

		private IDelayedActionsHandler _delayedActionsHandler => __delayedActionsHandler.Value;

		private bool HasPlayerAssistant => _playerAssistant != null;

		public override void Init(Func<List<MonsterData>> enemyHand, Func<List<TriggerType>> enemySkills, ArmyControllerCore thisController, FieldParameters parameters)
		{
			base.Init(enemyHand, enemySkills, thisController, parameters);
			MonsterActionListener.UnregisterBehaviour(MonsterActionBehaviour<UserBattleBehaviour>.instance);
			if (PlayerAssistant.IsAvailable(FieldScriptWrapper.currentBattleType))
			{
				_playerAssistant = new PlayerAssistant(AIAssistantsHelper.GetAvailableAssistant(FieldScriptWrapper.currentBattleType), enemyHand, enemySkills, thisController, parameters);
			}
			else
			{
				_playerAssistant = null;
			}
			scaleUpEffectShown = false;
			_autoplayPerformer = new AIArmyActionPerformer
			{
				canConcede = false,
				isAutoPlay = true
			};
			_autoplayPerformer.Init(enemyHand, enemySkills, thisController, parameters);
			_autoplayPerformer.BlockAnimation(blocked: true);
			_autoplayPerformer.SetAiDecicionMaker(AICharacterHelper.AutoPlayCharacter());
			MonsterActionBehaviour<BubbleSpeechBehaviour>.instance.InitBlocked();
			if (!TutorialModule.Inited || !TutorialModule.Instance.IsStartGameTutorial)
			{
				MonsterActionListener.RegisterBehaviour(MonsterActionBehaviour<BubbleSpeechBehaviour>.instance);
			}
		}

		public void UpdateFieldBoosts(MonsterData data)
		{
			_creator.UpdateFieldBoosts(data);
		}

		public void AttachCreator(FieldCreator creator)
		{
			_creator = creator;
			if (HasPlayerAssistant)
			{
				_playerAssistant.AttachCreator(_creator);
			}
		}

		public override void InformArmyCreated()
		{
			SetBubbleWaiting();
		}

		public override void InformEnemyTurnStarted(bool fromGenerateHand = false)
		{
			_window.RestoreFastSpeed();
			_window.SetDark(dark: true);
			int upkeepCount = _parameters.GetUpkeepCount(_side, visual: true);
			_window.SetTurnLabel(Mathf.Max(upkeepCount, 1));
			if (!fromGenerateHand && upkeepCount > 0)
			{
				_window.ShowEnemyTurnFlyText();
			}
			UpdateLevelEffect();
			CancelDelayedProcedures();
			_window.SetTimer(-1);
			base.InformEnemyTurnStarted();
			_window.SetActiveSide(_side == ArmySide.Right);
		}

		public override void InformTurnEnded()
		{
			_window.RestoreFastSpeed();
			_window.HideRequirements();
			CancelDelayedProcedures();
			MonsterActionBehaviour<UserBattleBehaviour>.instance.ResetSelection();
			RemoveCardSelection();
			_window.SetTimer(-1);
			foreach (Tile fieldTile in _creator.GetFieldTiles())
			{
				fieldTile.SetTileHighlight(Tile.TileHighlightning.NotHighlighted);
			}
			_window.SetActiveSide(_side == ArmySide.Right);
			if (_thisController.GetDrawType() == ArmyControllerCore.DrawType.NewSurvival)
			{
				_guiController.SilentlyReinitCards(_thisController.GetHand());
				_guiController.SetDark(dark: true);
			}
		}

		public override void InformFightCompleted()
		{
			base.InformFightCompleted();
			HideBubblesSelect();
			MonsterActionListener.UnregisterBehaviour(MonsterActionBehaviour<BubbleSpeechBehaviour>.instance);
			_window.SetAutocompleteState(isAutocompleting: false);
			_window.SetFastSpeed(BattlefieldWindow.SpeedUpState.No, GetAutofight());
			_window.BlockExit();
		}

		public override void InformTurnStarted(List<MonsterData> hand, List<TriggerType> skillTriggers)
		{
			skillTriggers.Sort(delegate(TriggerType x, TriggerType y)
			{
				int num3 = (int)x;
				return num3.CompareTo((int)y);
			});
			_window.RestoreFastSpeed();
			_window.HideRequirements();
			foreach (Tile fieldTile in _creator.GetFieldTiles())
			{
				fieldTile.SetTileHighlight(Tile.TileHighlightning.NotHighlighted);
			}
			if (HasPlayerAssistant)
			{
				_playerAssistant.Disable();
			}
			_delayedActionsHandler.CancelDelayedProcedure(assistantTimerProcId);
			_window.SetDark(GetAutofight());
			_window.SetTurnLabel(Mathf.Max(_parameters.GetUpkeepCount(_side, visual: true), 1));
			UpdateLevelEffect();
			_guiController.SilentlyReinitCards(hand);
			int turn = _parameters.GetTurn();
			List<SkillHandData> list = new List<SkillHandData>();
			FieldMonster warlord = _parameters.GetWarlord(_side);
			MonsterData data = warlord.data;
			foreach (TriggerType skillTrigger in skillTriggers)
			{
				if (skillTrigger != TriggerType.WarlordSkillSpecial)
				{
					for (int num = 0; num < data.skills.Count && num < data.skillValues.Count; num++)
					{
						if (data.skills[num].delay_wl_skill != -1)
						{
							_arrayDelayFromConfig[num] = data.skills[num].delay_wl_skill;
						}
						else
						{
							_arrayDelayFromConfig[num] = _parameters.skillDrawDelay;
						}
						if (data.skills[num].trigger == skillTrigger)
						{
							int turnUntilSkill = GetTurnUntilSkill(skillTrigger, turn);
							string value = GetValue(data.skillValues[num], (turnUntilSkill > 0) ? turnUntilSkill : 0);
							SkillHandData item = new SkillHandData
							{
								data = data.skills[num],
								value = value,
								delay = 0f
							};
							list.Add(item);
							break;
						}
					}
					continue;
				}
				foreach (ActionBitSignature skill in warlord.Skills)
				{
					if (skill.trigger == TriggerType.WarlordSkillSpecial)
					{
						SkillStaticData skillByName = SkillDataHelper.GetSkillByName(skill.skillId);
						string strValue = skill.strValue;
						if (skillByName != null)
						{
							SkillHandData item2 = new SkillHandData
							{
								data = skillByName,
								value = strValue
							};
							list.Add(item2);
							break;
						}
					}
				}
			}
			_delayedSkillShown = false;
			bool flag = list.Count != 0;
			if (!flag)
			{
				TriggerType triggerType = MonsterDataUtils.AvailableTrigger(turn, _parameters.GetWarlord(_side).data.skills, _parameters.skillDrawDelay);
				for (int num2 = 0; num2 < data.skills.Count && num2 < data.skillValues.Count; num2++)
				{
					if (data.skills[num2].trigger == triggerType)
					{
						int turnUntilSkill2 = GetTurnUntilSkill(triggerType, turn);
						string value2 = GetValue(data.skillValues[num2], (turnUntilSkill2 > 0) ? turnUntilSkill2 : 0);
						SkillHandData skillHandData = new SkillHandData
						{
							data = data.skills[num2],
							value = value2,
							delay = GetDelay(triggerType, turn),
							turnsUntilAvail = turnUntilSkill2 + _parameters.skillDrawShift
						};
						list.Add(skillHandData);
						Debug.Log(string.Concat("InformTurnStarted add skill ", skillHandData.data.trigger, ", val = ", skillHandData.value, " to player"));
						break;
					}
				}
				_delayedSkillShown = true;
			}
			list.Reverse();
			bool flag2 = false;
			foreach (SkillCard elem in _skillController.GetCards())
			{
				if (list.All((SkillHandData x) => x.data.strId != elem.skillData.strId || x.data.value != elem.skillValue))
				{
					flag2 = true;
				}
			}
			if (list.Count != _skillController.GetCards().Count || !flag || flag2)
			{
				_skillController.SilentlyReinitCards(list);
			}
			_autoplayPerformer.InformTurnStarted(hand, skillTriggers);
			base.InformTurnStarted(hand, skillTriggers);
			_window.SetActiveSide(_side != ArmySide.Right);
			_delayedActionsHandler.WaitForProcedure(0.1f, TryFirstTurnEffect);
		}

		private void TryFirstTurnEffect()
		{
			if (!scaleUpEffectShown)
			{
				_delayedActionsHandler.WaitForCondition(() => !FadeManager.FadeActive, delegate
				{
					scaleUpEffectShown = true;
					_guiController.ScaleElements();
				});
			}
		}

		private int GetTurnUntilSkill(TriggerType trigger, int turn)
		{
			return MonsterDataUtils.GetTurnForSkill(trigger, _parameters.GetWarlord(_side).data.skills, _parameters.skillDrawDelay) - turn;
		}

		private float GetDelay(TriggerType trigger, int turn)
		{
			int turnUntilSkill = GetTurnUntilSkill(trigger, turn);
			int turnForSkill = MonsterDataUtils.GetTurnForSkill(trigger, _parameters.GetWarlord(_side).data.skills, _parameters.skillDrawDelay);
			if (turnUntilSkill >= 0)
			{
				return (float)turnUntilSkill / (float)turnForSkill;
			}
			int num = 1;
			int num2 = Math.Abs(turnUntilSkill) / num + 1;
			turnUntilSkill %= num;
			return (float)(num + turnUntilSkill) % (float)num / (float)(turnForSkill + num2 * num);
		}

		private void UpdateLevelEffect()
		{
			List<SkillStaticData> list = new List<SkillStaticData>();
			List<string> list2 = new List<string>();
			MonsterData data = _parameters.GetWarlord(_side).data;
			MonsterData data2 = _parameters.GetWarlord(base.EnemySide).data;
			List<SkillStaticData> skills = data.skills;
			List<SkillStaticData> skills2 = data2.skills;
			for (int i = 0; i < skills.Count; i++)
			{
				SkillStaticData skillStaticData = skills[i];
				if (skillStaticData.trigger != TriggerType.WarlordSkill1 && skillStaticData.trigger != TriggerType.WarlordSkill2 && skillStaticData.trigger != TriggerType.WarlordSkillSpecial && skillStaticData.trigger != TriggerType.WarlordSkill3 && skillStaticData.trigger != TriggerType.WarlordSkill4 && skillStaticData.skill != SkillType.Attack && skillStaticData.levelSkill)
				{
					list.Add(skillStaticData);
					list2.Add(data.skillValues[i]);
				}
			}
			for (int j = 0; j < skills2.Count; j++)
			{
				SkillStaticData skillStaticData2 = skills2[j];
				if (skillStaticData2.trigger != TriggerType.WarlordSkill1 && skillStaticData2.trigger != TriggerType.WarlordSkill2 && skillStaticData2.trigger != TriggerType.WarlordSkillSpecial && skillStaticData2.trigger != TriggerType.WarlordSkill3 && skillStaticData2.trigger != TriggerType.WarlordSkill4 && skillStaticData2.skill != SkillType.Attack && skillStaticData2.levelSkill)
				{
					list.Add(skillStaticData2);
					list2.Add(data2.skillValues[j]);
				}
			}
			_window.SetLevelEffects(list, list2, levelEffectDescr);
		}

		public void SkipTurn()
		{
			CancelDelayedProcedures();
			_window.SetTimer(-1);
			_isChoosing = false;
			RemoveBattlefieldHighlights();
			_guiController.InformCardUnselected();
			MonsterActionBehaviour<UserBattleBehaviour>.instance.InformTimeEnded();
			MonsterActionListener.UnregisterBehaviour(MonsterActionBehaviour<UserBattleBehaviour>.instance);
			_availableTiles.Clear();
			_onChosen(null, Vector2.zero);
		}

		private void RemoveBattlefieldHighlights()
		{
			foreach (Tile fieldTile in _creator.GetFieldTiles())
			{
				fieldTile.SetTileHighlight(Tile.TileHighlightning.NotHighlighted);
			}
		}

		private void FakePlaceCard()
		{
		}

		public override void PerformPlacingChoose(List<MonsterData> hand, List<TriggerType> skillTriggers, Action<MonsterData, Vector2> onChosen, Action<TriggerType> onSkill, bool isAfterSkill = false, int currentTurn = 0)
		{
			_onChosen = onChosen;
			_onSkill = onSkill;
			if (!isAfterSkill)
			{
				CancelDelayedProcedures();
				_window.SetTimer(-1);
				if (!TestUtilFunctions.IsEndlessTurn() && (!TutorialModule.Inited || TutorialModule.Instance.IsTutorialCompleted()) && WindowScriptCore<BattlefieldWindow>.instance.isTimerActive)
				{
					int battleTimeLimit = GetBattleTimeLimit();
					if (battleTimeLimit != -1)
					{
						int item = _delayedActionsHandler.WaitForProcedure(battleTimeLimit, delegate
						{
							if (_isChoosing)
							{
								SkipTurn();
							}
						}, "DelayedAutoPlace");
						_delayedProceduresIds.Add(item);
						int item2 = _delayedActionsHandler.WaitForProcedure(battleTimeLimit - 10, delegate
						{
							if (_isChoosing)
							{
								_window.SetTimer(Mathf.Min(10, battleTimeLimit));
							}
						}, "DelayedTimer");
						_delayedProceduresIds.Add(item2);
					}
				}
			}
			_curHand = hand;
			_curSkills = skillTriggers;
			_isChoosing = true;
			_selectedCard = null;
			MonsterActionBehaviour<UserBattleBehaviour>.instance.Init(_guiController.GetCards, () => (!_delayedSkillShown) ? _skillController.GetCards().FindAll((SkillCard x) => _curSkills.Contains(x.trigger)) : new List<SkillCard>(), OnCardSelected, OnSkillSelected, new List<Tile>(), OnTileSelected, OnCardStartDragged, OnSkillDragged, OnStopDragging, OnStopSkillDragging, MonstersHighlightDelegate);
			if (!GetAutofight())
			{
				MonsterActionListener.RegisterBehaviour(MonsterActionBehaviour<UserBattleBehaviour>.instance);
			}
			_animatedBattleActionPerformer.Init(new List<Tile>(), OnTileSelected, OnStopSkillDragging, OnStopDragging, MonstersHighlightDelegate, OnSkillSelected, OnCardSelected, OnCardStartDragged, OnSkillDragged);
			if (GetAutofight())
			{
				PerformPlace(isAfterSkill);
			}
			else
			{
				if (!HasPlayerAssistant || !TutorialHandler.Instance.CanUseAssistant)
				{
					return;
				}
				assistantTimerProcId = _delayedActionsHandler.WaitForProcedure(_playerAssistant.Delay, delegate
				{
					if (_parameters.CanPlace(_side))
					{
						AIDecisionsCalculator.DesicionProcessingData data = new AIDecisionsCalculator.DesicionProcessingData(skillTriggers, hand, _simulateRandomCopier);
						_playerAssistant.PerformPlacingChoose(data);
					}
				});
				_delayedProceduresIds.Add(assistantTimerProcId);
			}
		}

		private void PerformPlace(bool isAfterSkill)
		{
			_autoplayPerformer.PerformPlacingChoose(_curHand, _curSkills, AutoplayCardSelected, AutoplaySkillSelected, isAfterSkill);
		}

		private int GetBattleTimeLimit()
		{
			if (Constants.NoobBattleTimerTime > 0 && !Constants.NoobBattleTimerTimeUnlock.IsLocked())
			{
				return Constants.NoobBattleTimerTime;
			}
			if (Constants.BattleTimerTime > 0)
			{
				return Constants.BattleTimerTime;
			}
			return -1;
		}

		public void ConnectToGui(BattlefieldWindow window, UserHandController userHandController, UserSkillController userSkillController, ChangeStateButton autoplayButton, Action onCardSelected)
		{
			_window = window;
			_window.SetTimer(-1);
			_guiController = userHandController;
			if (HasPlayerAssistant)
			{
				_playerAssistant.AttachGui(_window, _guiController);
			}
			_skillController = userSkillController;
			_autoplayButton = autoplayButton;
			_window.SetAutocompleteState(isAutocompleting: false, immediate: true);
			_window.SetTurnLabel(Mathf.Max(_parameters.GetUpkeepCount(_side, visual: true), 1));
			_autoplayButton.SetChosen(GetAutofight());
			ButtonEventListener.AddFunctionToButton(_autoplayButton.gameObject, OnAutoplayClicked);
			_cardSelectedPerform = onCardSelected;
		}

		private void SetAutofight(bool active)
		{
			AutofightUtils.SetAutofight(FieldScriptWrapper.currentBattleType, active);
		}

		private bool GetAutofight()
		{
			if (autoplayBlockers.Count > 0)
			{
				return false;
			}
			return AutofightUtils.GetAutofight(FieldScriptWrapper.currentBattleType);
		}

		public override void InformHandCreated(List<MonsterData> cards, List<TriggerType> skills)
		{
			_guiController.SilentlyReinitCards(cards);
			List<SkillHandData> list = new List<SkillHandData>();
			MonsterData data = _parameters.GetWarlord(_side).data;
			int turn = _parameters.GetTurn();
			foreach (TriggerType skill in skills)
			{
				for (int i = 0; i < data.skills.Count && i < data.skillValues.Count; i++)
				{
					if (data.skills[i].trigger == skill)
					{
						int turnUntilSkill = GetTurnUntilSkill(skill, turn);
						string value = GetValue(data.skillValues[i], (turnUntilSkill > 0) ? turnUntilSkill : 0);
						SkillHandData item = new SkillHandData
						{
							data = data.skills[i],
							value = value
						};
						list.Add(item);
						break;
					}
				}
			}
			list.Reverse();
			_skillController.SilentlyReinitCards(list);
		}

		public override void PerformCarsAddedToHand(List<MonsterData> cards, List<TriggerType> skills, Action onCompleted)
		{
			bool armyCompleted = false;
			bool skillCompleted = false;
			Common.VoidDelegate onCompleted2 = delegate
			{
				if (skillCompleted)
				{
					onCompleted();
				}
				else
				{
					armyCompleted = true;
				}
			};
			Common.VoidDelegate onCompleted3 = delegate
			{
				if (armyCompleted)
				{
					onCompleted();
				}
				else
				{
					skillCompleted = true;
				}
			};
			int turn = _parameters.GetTurn();
			_guiController.InformCardsAdded(cards, onCompleted2);
			List<SkillHandData> list = new List<SkillHandData>();
			FieldMonster warlord = _parameters.GetWarlord(_side);
			MonsterData data = warlord.data;
			bool flag = skills.Count == 1 && skills[0] == TriggerType.WarlordSkillSpecial;
			if (!flag)
			{
				foreach (TriggerType skill in skills)
				{
					for (int num = 0; num < data.skills.Count && num < data.skillValues.Count; num++)
					{
						if (data.skills[num].trigger == skill)
						{
							int turnUntilSkill = GetTurnUntilSkill(skill, turn);
							string value = GetValue(data.skillValues[num], (turnUntilSkill > 0) ? turnUntilSkill : 0);
							SkillHandData item = new SkillHandData
							{
								data = data.skills[num],
								value = value
							};
							list.Add(item);
							break;
						}
					}
				}
			}
			else
			{
				foreach (ActionBitSignature skill2 in warlord.Skills)
				{
					if (skill2.trigger == TriggerType.WarlordSkillSpecial)
					{
						SkillStaticData skillByName = SkillDataHelper.GetSkillByName(skill2.skillId);
						string strValue = skill2.strValue;
						if (skillByName != null)
						{
							SkillHandData item2 = new SkillHandData
							{
								data = skillByName,
								value = strValue
							};
							list.Add(item2);
							break;
						}
					}
				}
			}
			if (_delayedSkillShown && list.Count > 0)
			{
				_skillController.RemoveAllCards();
				_delayedSkillShown = false;
			}
			_skillController.InformCardsAdded(list, onCompleted3, flag);
			_autoplayPerformer.PerformCarsAddedToHand(cards, skills, delegate
			{
			});
		}

		public override void InformCarsRemovedFromHand(List<MonsterData> cards)
		{
			base.InformCarsRemovedFromHand(cards);
			_guiController.InformCardsRemoved(cards);
			_autoplayPerformer.InformCarsRemovedFromHand(cards);
		}

		public override void InformSkillRemovedFromHand(TriggerType cards)
		{
			InformSkillsRemovedFromHand(new List<TriggerType> { cards });
		}

		public override void InformSkillsRemovedFromHand(List<TriggerType> cards)
		{
			base.InformSkillsRemovedFromHand(cards);
			_autoplayPerformer.InformSkillsRemovedFromHand(cards);
		}

		public override void InformStop()
		{
			base.InformStop();
			MonsterActionListener.UnregisterBehaviour(MonsterActionBehaviour<UserBattleBehaviour>.instance);
			_isChoosing = false;
			_autoplayPerformer.InformStop();
			CancelDelayedProcedures();
			_window.SetTimer(-1);
		}

		private void CancelDelayedProcedures()
		{
			if (HasPlayerAssistant)
			{
				_playerAssistant.Disable();
			}
			foreach (int delayedProceduresId in _delayedProceduresIds)
			{
				_delayedActionsHandler.CancelDelayedProcedure(delayedProceduresId);
			}
			_delayedProceduresIds.Clear();
		}

		public void BlockAutoplay(string key)
		{
			if (GetAutofight())
			{
				ApplyAutoplay(autofight: false, save: false);
			}
			autoplayBlockers.Add(key);
		}

		public void ReleaseAutoplay(string key)
		{
			autoplayBlockers.Remove(key);
			if (autoplayBlockers.Count == 0)
			{
				MonsterActionListener.UnregisterBehaviour(MonsterActionBehaviour<UserBattleBehaviour>.instance);
				ApplyAutoplay(GetAutofight(), save: false);
			}
		}

		public void ClearAutoplayBlockers()
		{
			autoplayBlockers.Clear();
		}

		public void StartAutoplay()
		{
			OnAutoplayClicked();
		}

		private void OnAutoplayClicked()
		{
			bool autofight = GetAutofight();
			if (UserArmyModule.inited && Constants.autofight_unlocks.IsLocked())
			{
				if (autofight)
				{
					ApplyAutoplay(autofight: false);
				}
			}
			else
			{
				ApplyAutoplay(!autofight);
			}
		}

		private void ApplyAutoplay(bool autofight, bool save = true)
		{
			if (save)
			{
				SetAutofight(autofight);
			}
			_autoplayButton.SetChosen(autofight);
			_window.SetDark(autofight);
			if (_isChoosing)
			{
				if (autofight)
				{
					_autoplayPerformer.PerformPlacingChoose(_curHand, _curSkills, AutoplayCardSelected, AutoplaySkillSelected);
				}
				else
				{
					MonsterActionListener.RegisterBehaviour(MonsterActionBehaviour<UserBattleBehaviour>.instance);
				}
			}
			if (!autofight)
			{
				_window.RestoreFastSpeed();
			}
			_window.SetAutocompleteState(autofight);
		}

		public void MakeAutoStep()
		{
			_forceAnimatedAutofight = true;
			if (_isChoosing)
			{
				_autoplayPerformer.PerformPlacingChoose(_curHand, _curSkills, AutoplayCardSelected, AutoplaySkillSelected);
			}
		}

		public void FakeActivateAutoFight()
		{
		}

		public void FakeAutoFight()
		{
			_autoplayButton.SetChosen(chosen: true);
			_window.SetDark(dark: true);
			if (_isChoosing)
			{
				_autoplayPerformer.PerformPlacingChoose(_curHand, _curSkills, AutoplayCardSelected, AutoplaySkillSelected);
			}
			_window.SetAutocompleteState(isAutocompleting: true);
		}

		private void AutoplayCardSelected(MonsterData monster, Vector2 place)
		{
			Card card = null;
			Tile tile = null;
			if (monster == null)
			{
				Debug.LogError("AutoplayCardSelected. monster == null");
			}
			else
			{
				card = _guiController.GetCards().Find((Card x) => x.data == monster);
				if (card == null)
				{
					Debug.LogError("AutoplayCardSelected. Cannot place monster " + monster.monster_id + " because it isnt in hand!");
				}
			}
			tile = _creator.GetFieldTiles().Find((Tile x) => x.Coords == place);
			if (tile == null)
			{
				Debug.LogError(string.Concat("Cannot place monster to ", place, " because somebody here!"));
			}
			if (card == null || tile == null)
			{
				SkipTurn();
			}
			else if (_isChoosing)
			{
				if (_forceAnimatedAutofight || !Constants.animated_autofight_unlocks.IsLocked())
				{
					MonsterActionListener.UnregisterBehaviour(MonsterActionBehaviour<UserBattleBehaviour>.instance);
					_forceAnimatedAutofight = false;
					_animatedBattleActionPerformer.AnimatePlace(card, tile);
				}
				else
				{
					OnCardSelected(card);
					OnTileSelected(tile);
				}
			}
		}

		private void AutoplaySkillSelected(TriggerType trigger)
		{
			if (_isChoosing)
			{
				_forceAnimatedAutofight = false;
				OnSkillSelected(_skillController.GetCards().Find((SkillCard x) => x.skillData.trigger == trigger));
			}
		}

		private void OnSkillSelected(SkillCard skill)
		{
			_window.HideRequirements();
			if (HasPlayerAssistant)
			{
				_playerAssistant.Disable();
			}
			_delayedActionsHandler.CancelDelayedProcedure(assistantTimerProcId);
			_isChoosing = false;
			foreach (Tile fieldTile in _creator.GetFieldTiles())
			{
				fieldTile.SetTileHighlight(Tile.TileHighlightning.NotHighlighted);
			}
			MonstersHighlightDelegate(highlghted: false, null, Vector2.zero);
			_guiController.InformCardUnselected();
			_skillController.TempHideElem(skill.trigger, delegate
			{
				_skillController.InformCardsRemoved(skill.trigger);
				if (HasPlayerAssistant && TutorialHandler.Instance.CanUseAssistant && !GetAutofight())
				{
					assistantTimerProcId = _delayedActionsHandler.WaitForProcedure(_playerAssistant.Delay, delegate
					{
						if (_parameters.CanPlace(_side))
						{
							AIDecisionsCalculator.DesicionProcessingData data = new AIDecisionsCalculator.DesicionProcessingData(_curSkills, _curHand, _simulateRandomCopier);
							_playerAssistant.PerformPlacingChoose(data);
						}
					});
					_delayedProceduresIds.Add(assistantTimerProcId);
				}
			});
			MonsterActionListener.UnregisterBehaviour(MonsterActionBehaviour<UserBattleBehaviour>.instance);
			_onSkill(skill.trigger);
			_cardSelectedPerform.SafeInvoke();
			BattleInfoBehaviour.totalBlock = false;
		}

		private GameObject OnCardStartDragged(Card card)
		{
			_guiController.TempHideElem(card.data, delegate
			{
			});
			FieldMonsterVisual fieldMonsterVisual = PrefabCreator.CreateFieldMonster(FieldScriptWrapper.instance.transform, _side, "TEST_SHIT");
			fieldMonsterVisual.gameObject.SetActive(value: false);
			fieldMonsterVisual.showClassLabel = Constants.show_monster_class_unlock.IsUnlocked();
			fieldMonsterVisual.Init(card.data, null);
			fieldMonsterVisual.UpdateSkills();
			fieldMonsterVisual.ApplyEulerAngles(new Vector3(0f, 0f, 0f));
			fieldMonsterVisual.transform.Find("Attack").gameObject.SetActive(value: false);
			fieldMonsterVisual.transform.Find("HP").gameObject.SetActive(value: false);
			fieldMonsterVisual.ApplyColorTint(FieldScriptWrapper.instance.fieldCreator.GetMonsterTintColor());
			fieldMonsterVisual.ConvertZtoDepth(-22f);
			fieldMonsterVisual.gameObject.SetActive(value: true);
			return fieldMonsterVisual.gameObject;
		}

		private GameObject OnSkillDragged(SkillCard skillCard)
		{
			_skillController.TempHideElem(skillCard.trigger, delegate
			{
			});
			_guiController.InformCardUnselected();
			_selectedCard = null;
			foreach (Tile fieldTile in _creator.GetFieldTiles())
			{
				fieldTile.SetTileHighlight(Tile.TileHighlightning.NotHighlighted);
			}
			if (!SkillEffects.ContainsKey(skillCard.skillData.dragEffectName))
			{
				SkillEffects.Add(skillCard.skillData.dragEffectName, Resources.Load<GameObject>("Prefabs/SkillEffects/" + skillCard.skillData.dragEffectName));
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(SkillEffects[skillCard.skillData.dragEffectName]);
			gameObject.GetComponent<SkillDragElement>().Init(skillCard.skillData, skillCard.skillValue);
			_cardSelectedPerform.SafeInvoke();
			return gameObject;
		}

		private void MonstersHighlightDelegate(bool highlghted, MonsterData monster, Vector2 position)
		{
			bool flag = monster != null && monster.monsterClass == Class.Building;
			bool flag2 = false;
			int num = ((_side == ArmySide.Left) ? 1 : (-1));
			foreach (KeyValuePair<Vector2, FieldMonster> monster2 in _parameters.GetMonsters(_side))
			{
				if (monster2.Key.y == position.y && monster2.Key.x * (float)num > position.x * (float)num)
				{
					flag2 = monster != null && monster.monsterClass == Class.Melee;
					break;
				}
			}
			if ((!UserRankModule.inited || Constants.MaxHitHighlightRank >= UserRankModule.instance.rank) && highlghted && position.y != -1f && !flag && !flag2)
			{
				Vector2 vector = new Vector2(-1000f, -1000f);
				FieldMonster fieldMonster = null;
				foreach (KeyValuePair<Vector2, FieldMonster> monster3 in _parameters.GetMonsters(base.EnemySide))
				{
					if (monster3.Key.y == position.y && Mathf.Abs(monster3.Key.x - position.x) < Mathf.Abs(vector.x - position.x))
					{
						vector = monster3.Key;
						fieldMonster = monster3.Value;
					}
				}
				if (fieldMonster == null)
				{
					fieldMonster = _parameters.GetWarlord(base.EnemySide);
				}
				_window.ShowSwords(show: true, fieldMonster.VisualMonster.transform.position);
			}
			else
			{
				_window.ShowSwords(show: false, Vector3.zero);
			}
			foreach (KeyValuePair<Vector2, FieldMonster> monster4 in _parameters.GetMonsters(_side))
			{
				monster4.Value.VisualMonster.ResetParametersGlance();
			}
			foreach (KeyValuePair<Vector2, FieldMonster> monster5 in _parameters.GetMonsters(base.EnemySide))
			{
				monster5.Value.VisualMonster.ResetParametersGlance();
			}
			_parameters.GetWarlord(base.EnemySide).VisualMonster.ResetParametersGlance();
			_parameters.GetWarlord(_side).VisualMonster.ResetParametersGlance();
			if (!highlghted || monster == null)
			{
				return;
			}
			List<SkillStaticData> list = monster.skills.FindAll((SkillStaticData x) => x.trigger == TriggerType.Appear && x.triggerTarget == "self" && !x.filter.Contains("random") && x.skill != SkillType.AddSkill && x.skill != SkillType.AddSkillTemporary && x.skill != SkillType.Transform && x.skill != SkillType.Summon && x.skill != SkillType.SummonRune && x.skill != SkillType.DivineShield);
			if (list.Count == 0)
			{
				return;
			}
			foreach (SkillStaticData item in list)
			{
				ActionBit actionBit = SkillFabric.CreateSkill(item, "1000", () => monster.monsterClass == Class.Ranged, () => monster.attack, () => monster.health, new FieldRandom(), withoutAnimation: true);
				HighlightElement highlightElement = new HighlightElement();
				highlightElement.Init(position, _thisController);
				actionBit.Init(highlightElement, _thisController, _parameters, () => position, new FieldRandom(), withoutDelay: true);
				bool isGood = item.skill == SkillType.Heal || item.skill == SkillType.Regen || item.skill == SkillType.BuffAttack || item.skill == SkillType.BuffHealth;
				bool isHp = item.skill == SkillType.Vampiric || item.skill == SkillType.Pierce || item.skill == SkillType.Bleeding || item.skill == SkillType.SplashAttack || item.skill == SkillType.ChainAttack || item.skill == SkillType.Heal || item.skill == SkillType.Regen || item.skill == SkillType.BuffHealth || item.skill == SkillType.InstantKill || item.skill == SkillType.Damage || item.skill == SkillType.SetHealth || item.skill == SkillType.SetHealthTemp;
				foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in actionBit.GetRightMonsters())
				{
					if (rightMonster.Value != null && rightMonster.Value.VisualMonster != null)
					{
						rightMonster.Value.VisualMonster.SetParanetersGlance(isHp, isGood);
					}
				}
			}
		}

		private void OnStopDragging(Card card)
		{
			_guiController.TempShowElem(card.data, delegate
			{
			});
		}

		private void OnStopSkillDragging(SkillCard skillCard)
		{
			_skillController.TempShowElem(skillCard.trigger, delegate
			{
			});
			BattleInfoBehaviour.totalBlock = false;
		}

		private void OnCardSelected(Card card)
		{
			if (_selectedCard == null || _selectedCard != card)
			{
				_selectedCard = card;
				_guiController.InformCardSelected(card.data);
				TutorialHandler.Instance.RecieveTrigger("CARD_PICKED");
				TutorialHandler.Instance.RecieveTrigger("CARD_ID_" + card.data.monster_id + "_PICKED");
				_availableTiles.Clear();
				Dictionary<Vector2, FieldMonster> monsters = _parameters.GetMonsters(_side);
				bool flag = Constants.show_tile_hints_unlock.IsUnlocked();
				foreach (Tile fieldTile in _creator.GetFieldTiles())
				{
					if (_parameters.GetClassedTiles(card.data.monsterClass, _side).Contains(fieldTile.Coords))
					{
						Tile.TileHighlightning highlightning = Tile.TileHighlightning.GreenBlinking;
						Dictionary<Vector2, FieldRune> runes = _parameters.GetRunes(_side);
						Color? color = null;
						int num = ((_side == ArmySide.Left) ? 1 : (-1));
						Vector2 key = new Vector2(fieldTile.Coords.x + (float)num, fieldTile.Coords.y);
						FieldMonster value = null;
						bool flag2 = monsters.TryGetValue(key, out value);
						int num2 = ((_side == ArmySide.Left) ? 2 : (-2));
						Vector2 key2 = new Vector2(fieldTile.Coords.x + (float)num2, fieldTile.Coords.y);
						FieldMonster value2 = null;
						bool flag3 = monsters.TryGetValue(key2, out value2);
						bool flag4 = false;
						if (_side == ArmySide.Left)
						{
							flag4 = fieldTile.row == _creator.GetFieldWidth() / 2 - 1;
						}
						if (_side == ArmySide.Right)
						{
							flag4 = fieldTile.row == _creator.GetFieldWidth() / 2;
						}
						if ((!runes.ContainsKey(fieldTile.Coords) || runes[fieldTile.Coords].data.destroyAfterUse) && (!UserRankModule.inited || UserRankModule.instance.rank <= Constants.FieldEmblemsMaxRank))
						{
							switch (card.data.monsterClass)
							{
							case Class.Melee:
								highlightning = Tile.TileHighlightning.SwordBlinking;
								if (flag)
								{
									if (flag2)
									{
										highlightning = Tile.TileHighlightning.NotHighlighted;
									}
									else if (flag4)
									{
										color = Tile.GREEN_COLOR;
									}
								}
								break;
							case Class.Ranged:
								highlightning = Tile.TileHighlightning.BowBlinking;
								if (flag)
								{
									if (flag2 && (value.data.monsterClass == Class.Melee || value.data.monsterClass == Class.Building))
									{
										color = Tile.GREEN_COLOR;
									}
									if (flag3 && (value2.data.monsterClass == Class.Melee || value2.data.monsterClass == Class.Building))
									{
										color = Tile.GREEN_COLOR;
									}
								}
								break;
							case Class.Building:
								highlightning = Tile.TileHighlightning.TowerBlinking;
								break;
							}
						}
						fieldTile.SetTileHighlight(highlightning, color);
						_availableTiles.Add(fieldTile);
					}
					else
					{
						fieldTile.SetTileHighlight(Tile.TileHighlightning.NotHighlighted);
					}
				}
				MonsterActionBehaviour<UserBattleBehaviour>.instance.Init(_guiController.GetCards, () => (!_delayedSkillShown) ? _skillController.GetCards().FindAll((SkillCard x) => _curSkills.Contains(x.trigger)) : new List<SkillCard>(), OnCardSelected, OnSkillSelected, _availableTiles, OnTileSelected, OnCardStartDragged, OnSkillDragged, OnStopDragging, OnStopSkillDragging, MonstersHighlightDelegate);
				_animatedBattleActionPerformer.Init(_availableTiles, OnTileSelected, OnStopSkillDragging, OnStopDragging, MonstersHighlightDelegate, OnSkillSelected, OnCardSelected, OnCardStartDragged, OnSkillDragged);
				BattleInfoBehaviour.totalBlock = true;
			}
			if (_selectedCard != null && _cardSelectedPerform != null)
			{
				_cardSelectedPerform();
			}
		}

		public override bool IsPlaceAvailableToPlace(FieldElement element)
		{
			if (!_isChoosing)
			{
				return false;
			}
			if (_availableTiles.Count == 0)
			{
				return false;
			}
			return _availableTiles.Find((Tile tile) => tile.Coords == element.coords) != null;
		}

		private void OnTileSelected(Tile tile)
		{
			if (!(_selectedCard != null) && !(tile == null))
			{
				return;
			}
			_window.HideRequirements();
			CancelDelayedProcedures();
			_window.SetTimer(-1);
			_isChoosing = false;
			foreach (Tile fieldTile in _creator.GetFieldTiles())
			{
				fieldTile.SetTileHighlight(Tile.TileHighlightning.NotHighlighted);
			}
			_guiController.InformCardUnselected();
			MonsterActionListener.UnregisterBehaviour(MonsterActionBehaviour<UserBattleBehaviour>.instance);
			MonsterData data = ((tile == null) ? null : _selectedCard.data);
			Vector2 arg = ((tile == null) ? Vector2.zero : tile.Coords);
			_guiController.TempHideElem(data, delegate
			{
				_guiController.InformCardsRemoved(new List<MonsterData> { data });
			});
			_window.SetTimer(-1);
			_onChosen(data, arg);
			_availableTiles.Clear();
			BattleInfoBehaviour.totalBlock = false;
		}

		private void SetBubbleWaiting()
		{
			MonsterActionBehaviour<BubbleSpeechBehaviour>.instance.InitWaiting(_thisController.GetWarlord(), ShowBubblesSelect);
		}

		private void ShowBubblesSelect()
		{
			_selectBubbles = BubbleSelectElementContainer.CreateBubble();
			BubbleSelectElement bubble = _selectBubbles.bubble1;
			BubbleSelectElement bubble2 = _selectBubbles.bubble2;
			BubbleSelectElement bubble3 = _selectBubbles.bubble3;
			BubbleSelectElement bubble4 = _selectBubbles.bubble4;
			_selectBubbles.transform.parent = _thisController.GetWarlord().visualElement.transform.parent;
			_selectBubbles.transform.position = _thisController.GetWarlord().VisualMonster.transform.position;
			string dialogText = Localization.Localize("#dialog_fast_phrase_" + FastDialogData.Event.ThankYou);
			bubble.Init(dialogText);
			dialogText = Localization.Localize("#dialog_fast_phrase_" + FastDialogData.Event.WellPlayed);
			bubble2.Init(dialogText);
			dialogText = Localization.Localize("#dialog_fast_phrase_" + FastDialogData.Event.Oops);
			bubble3.Init(dialogText);
			dialogText = Localization.Localize("#dialog_fast_phrase_" + FastDialogData.Event.Wow);
			bubble4.Init(dialogText);
			Dictionary<BubbleSelectElement, Common.VoidDelegate> bubbles = new Dictionary<BubbleSelectElement, Common.VoidDelegate>
			{
				{
					bubble,
					delegate
					{
						SelectBubble(FastDialogData.Event.ThankYou);
					}
				},
				{
					bubble2,
					delegate
					{
						SelectBubble(FastDialogData.Event.WellPlayed);
					}
				},
				{
					bubble3,
					delegate
					{
						SelectBubble(FastDialogData.Event.Oops);
					}
				},
				{
					bubble4,
					delegate
					{
						SelectBubble(FastDialogData.Event.Wow);
					}
				}
			};
			MonsterActionBehaviour<BubbleSpeechBehaviour>.instance.InitBubble(bubbles, HideBubblesSelect);
			FieldScriptWrapper.instance.fieldController.HideFastDialog(_side);
			SoundManager.Instance.PlaySound("int_bubble_appear");
		}

		public void HideBubblesSelect()
		{
			if ((bool)_selectBubbles)
			{
				UnityEngine.Object.Destroy(_selectBubbles.gameObject);
			}
			SetBubbleWaiting();
		}

		private void SelectBubble(FastDialogData.Event trigger)
		{
			FieldScriptWrapper.instance.fieldController.RequestFastDialog(trigger, _side);
			HideBubblesSelect();
		}

		public void ShuffleHand()
		{
			foreach (Tile fieldTile in _creator.GetFieldTiles())
			{
				fieldTile.SetTileHighlight(Tile.TileHighlightning.NotHighlighted);
			}
			MonsterActionBehaviour<UserBattleBehaviour>.instance.ResetSelection();
			RemoveCardSelection();
		}

		public void RemoveCardSelection()
		{
			_selectedCard = null;
		}
	}
}
