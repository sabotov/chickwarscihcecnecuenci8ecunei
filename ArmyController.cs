using System;
using System.Collections.Generic;
using ActionBehaviours;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.UserData;
using NewAssets.Scripts.UtilScripts;
using ServiceLocator;
using Tutorial;
using UnityEngine;
using UserData;
using UtilScripts;

namespace BattlefieldScripts
{
	public class ArmyController : ArmyControllerCore
	{
		private readonly CachedService<IDelayedActionsHandler> __delayedActionsHandler = new CachedService<IDelayedActionsHandler>();

		private ArmyData _startData;

		private FieldCreator _creator;

		private FieldController _controllerClassed;

		private BattleInfoBehaviour _infoBehaviour;

		private IDelayedActionsHandler _delayedActionsHandler => __delayedActionsHandler.Value;

		public void Init(FieldCreator creator, FieldController controller, ArmyActionPerformer actionPerformer, FieldParameters fieldParameters, ArmyData data, FieldRandom random, IteratorCore iterator, DrawType drawType, FieldScriptWrapper.StartBattleArmyParams.BattleType battleType)
		{
			_startData = data;
			_creator = creator;
			_controllerClassed = controller;
			Init(controller, actionPerformer, fieldParameters, random, iterator, drawType, battleType);
			_infoBehaviour = new BattleInfoBehaviour();
			Func<FieldMonster> warlordDelegate = () => (!(_actionPerformer is PlayerArmyActionPerformer)) ? GetWarlord() : null;
			Func<FieldMonster> petDelegate = base.GetPet;
			_infoBehaviour.Init(base.GetFieldMonsters, warlordDelegate, petDelegate, base.GetFieldRunes, _actionPerformer.IsPlaceAvailableToPlace);
			MonsterActionListener.RegisterBehaviour(_infoBehaviour);
		}

		public override void CreateArmy()
		{
			base.CreateArmy();
			foreach (KeyValuePair<Place, MonsterData> fMon in _startData.fieldMonsters)
			{
				SilentlyPlaceMonster(fMon.Value, fMon.Key);
				FieldMonster mon = _fieldMonsters[fMon.Key];
				_delayedActionsHandler.WaitForOneFrame(delegate
				{
					if (mon.ShouldPerformAction(TriggerType.Appear, SkillType.NoSkill, fMon.Key, mon, mon))
					{
						mon.PerformAction(TriggerType.Appear, SkillType.NoSkill, fMon.Key, mon, mon, delegate
						{
						}, mon.CheckDeath);
					}
				});
			}
			foreach (KeyValuePair<Place, RuneData> rune in _startData.runes)
			{
				PlaceRune(rune.Value, rune.Key);
			}
			_deck = new List<MonsterData>();
			foreach (KeyValuePair<int, MonsterData> item in _startData.deck)
			{
				_deck.Add(item.Value);
			}
			_handDraw = _startData.handDraw;
			FieldMonsterVisual visual = _creator.PlaceWarlord(base.Side);
			_warlord = new FieldMonster
			{
				coords = new Vector2((base.Side == ArmySide.Left) ? (-1) : _creator.GetFieldWidth(), 1f)
			};
			_warlord.Init(this, _startData.warlord, visual, _parameters, _random, _iterator);
			if (_startData.petData != null)
			{
				FieldMonsterVisual visual2 = _creator.PlacePet(base.Side);
				_pet = new FieldMonster
				{
					coords = new Vector2((base.Side == ArmySide.Left) ? (-2) : _creator.GetFieldWidth(), 2f)
				};
				_pet.Init(this, _startData.petData.petMonsterData, visual2, _parameters, _random, _iterator);
			}
			_hand = new List<MonsterData>();
			_availableSkills = new List<TriggerType>();
			_actionPerformer.InformArmyCreated();
		}

		protected override void OnPlasementFinished()
		{
			_delayedActionsHandler.WaitForProcedure(TimeDebugController.instance.afterPlacementDelay / TimeDebugController.totalTimeMultiplier, delegate
			{
				_onPlacementFinished();
			});
		}

		public override void PlaceRune(RuneData runeData, Vector2 place)
		{
			if (_parameters.GetArmyTiles(base.EnemySide).Contains(place))
			{
				_controller.RequestRuneAdding(base.Side, runeData, place);
				return;
			}
			FieldRuneVisual visual = _creator.PlaceRune(base.Side, place, runeData.prefab, string.Concat(place, "_", runeData.prefab));
			FieldRune fieldRune = new FieldRune
			{
				coords = place
			};
			_runes.Add(place, fieldRune);
			fieldRune.Init(this, runeData, visual, _parameters, _random, _iterator);
		}

		protected override void SilentlyPlaceMonster(MonsterData monster, Vector2 place)
		{
			FieldMonsterVisual visual = _creator.PlaceUnit(base.Side, place, monster.image);
			FieldMonster fieldMonster = new FieldMonster
			{
				coords = place
			};
			fieldMonster.Init(this, monster, visual, _parameters, _random, _iterator);
			_fieldMonsters.Add(place, fieldMonster);
		}

		protected override void AnimateAppearEffect(FieldMonster mon, Action onEffect)
		{
			Action onEffectFinished = delegate
			{
				if (TutorialHandler.Instance.RecieveTrigger("MONSTER_PLACEMENT_EFFECT_PLAYED"))
				{
					TutorialHandler.Instance.OnSomethingPerformed += onEffect;
				}
				else
				{
					onEffect();
				}
			};
			SoundManager.Instance.PlaySound("fight_appear_effect");
			SoundManager.Instance.PlaySound(mon.data.staticInnerData.appearVoice);
			if (mon.data.staticInnerData.appearEffect != "")
			{
				Action onEnded = delegate
				{
					if (FieldScriptWrapper.currentBattleType != FieldScriptWrapper.StartBattleArmyParams.BattleType.GoldMine)
					{
						_delayedActionsHandler.WaitForProcedure(0.2f, delegate
						{
							onEffectFinished();
						});
					}
					else
					{
						onEffectFinished();
					}
				};
				EffectAnimation effectAnimation = new EffectAnimation("fx_appear");
				effectAnimation.Init(mon.visualElement);
				effectAnimation.Animate(new Dictionary<Common.StringDelegate, FieldVisual> { 
				{
					() => "",
					mon.visualElement
				} }, onEnded);
			}
			else if (mon.data.staticInnerData.appearAnimation != "")
			{
				BitActionAnimation bitActionAnimation = null;
				switch (mon.data.staticInnerData.appearAnimation)
				{
				case "alpha":
					bitActionAnimation = new AlphaAppearAnimation();
					break;
				case "top":
					bitActionAnimation = new TopAppearAnimation();
					break;
				case "ground":
					bitActionAnimation = new GroundAppearAnimation();
					break;
				}
				if (bitActionAnimation == null)
				{
					onEffectFinished();
					return;
				}
				bitActionAnimation.Init(mon.visualElement);
				bitActionAnimation.Animate(new Dictionary<Common.StringDelegate, FieldVisual> { 
				{
					() => "",
					mon.visualElement
				} }, onEffectFinished);
			}
			else
			{
				onEffectFinished();
			}
		}

		protected override void StopAllTriggers()
		{
			MonsterActionListener.UnregisterBehaviour(_infoBehaviour);
			_infoBehaviour.Unregister();
			base.StopAllTriggers();
		}

		protected override void OnSkillUsed(TriggerType trigger)
		{
			TutorialHandler.Instance.RecieveTrigger("SKILL_USED");
			TutorialHandler.Instance.RecieveTrigger(string.Concat("SKILL_", trigger, "_", base.Side, "USED"));
			base.OnSkillUsed(trigger);
			if (_actionPerformer is PlayerArmyActionPerformer && ((PlayerArmyActionPerformer)_actionPerformer).sendPlaceQuest)
			{
				RegisterQuest(QuestType.UseSkill, null);
			}
		}

		protected override void OnPlacementChoosed(MonsterData monster, Vector2 place)
		{
			if (monster != null)
			{
				TutorialHandler.Instance.RecieveTrigger("CARD_PLAYED");
				TutorialHandler.Instance.RecieveTrigger("CARD_PLACE_" + place.x + "_" + place.y + "_PLAYED");
				TutorialHandler.Instance.RecieveTrigger(string.Concat("CARD_ID_", monster.monster_id, "_", base.Side, "_PLAYED"));
				TutorialHandler.Instance.RecieveTrigger("CARD_PLACE_" + place.x + "_" + place.y + "_PLAYED");
			}
			base.OnPlacementChoosed(monster, place);
			if (_actionPerformer is PlayerArmyActionPerformer && ((PlayerArmyActionPerformer)_actionPerformer).sendPlaceQuest)
			{
				RegisterQuest(QuestType.PlaceUnits, monster);
			}
		}

		public override void InformEnemyMonsterDead(Vector2 coords, FieldMonster monster)
		{
			if (_actionPerformer is PlayerArmyActionPerformer && ((PlayerArmyActionPerformer)_actionPerformer).sendPlaceQuest && !monster.VisualMonster.IsWarlord)
			{
				RegisterQuest(QuestType.KillMonster, monster.data);
			}
			base.InformEnemyMonsterDead(coords, monster);
		}

		public override void InformEnemyMonsterHit(Vector2 coords, FieldMonster monster)
		{
			base.InformEnemyMonsterHit(coords, monster);
		}

		public override void InformMonsterDead(Vector2 coords, FieldMonster monster)
		{
			_actionPerformer.InformMonsterDead(monster, coords == _warlord.coords);
			base.InformEnemyMonsterDead(coords, monster);
		}

		private void PlayPetSoundOnWarlordHit()
		{
			if (Constants.PlayPetSoundOnWarlordHit && UnityEngine.Random.value > 1f - (float)Constants.ChancePlayPetSoundOnWarlordHit / 100f)
			{
				SoundManager.Instance.PlaySound(_pet.data.staticInnerData.appearVoice);
			}
		}

		public override void InformMonsterHit(Vector2 coords, FieldMonster monster)
		{
			if (monster == _warlord && _pet != null)
			{
				PlayPetSoundOnWarlordHit();
				_pet.AnimateDamaged();
			}
			_actionPerformer.InformMonsterHit(monster, coords == _warlord.coords);
			base.InformEnemyMonsterHit(coords, monster);
		}

		public override void PerformMonsterDead(Vector2 coords)
		{
			TutorialHandler.Instance.RecieveTrigger("MONSTER_KILLED");
			if (_fieldMonsters.ContainsKey(coords))
			{
				TutorialHandler.Instance.RecieveTrigger(string.Concat("MONSTER_ID_", _fieldMonsters[coords].data.monster_id, "_", base.Side, "_KILLED"));
			}
			TutorialHandler.Instance.RecieveTrigger("MONSTER_PLACE_" + coords.x + "_" + coords.y + "_KILLED");
			if (coords == _warlord.coords)
			{
				_controllerClassed.RequestFastDialog(FastDialogData.Event.BattleVictory, base.EnemySide);
			}
			base.PerformMonsterDead(coords);
		}

		public override void InformEnemySpeech(FastDialogData.Event trigger)
		{
			_actionPerformer.InformEnemySpeech(trigger);
		}

		public override void AnimateVictory()
		{
			SoundManager.Instance.PlaySound("vo_fight_army_joy");
			if (_pet != null)
			{
				SoundManager.Instance.PlaySound(_pet.data.staticInnerData.appearVoice);
				_pet.AnimateVictory();
			}
			base.AnimateVictory();
		}

		public override void AnimateDefeat()
		{
			_warlord.AnimateDefeat();
			if (_pet != null)
			{
				SoundManager.Instance.PlaySound(_pet.data.staticInnerData.deathVoice);
				_pet.AnimateDefeat();
			}
			foreach (KeyValuePair<Vector2, FieldMonster> fieldMonster in _fieldMonsters)
			{
				fieldMonster.Value.AnimateDefeat();
			}
		}

		public ArmyController(ArmySide thisSide)
			: base(thisSide)
		{
		}

		protected override void InformEndingPhase()
		{
			base.InformEndingPhase();
		}

		private void RegisterQuest(QuestType trigger, MonsterData monster)
		{
			BattleQuestHelper.RegisterQuest(trigger, monster);
		}
	}
}
