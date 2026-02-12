using System;
using System.Collections.Generic;
using Assets.Scripts.UtilScripts.Loaders;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using MyEffects.Animations;
using NGUI.Scripts.Internal;
using NGUI.Scripts.UI;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using NewAssets.Scripts.DataClasses.UserData;
using NewAssets.Scripts.Data_Helpers;
using NewAssets.Scripts.UtilScripts;
using ServiceLocator;
using Tutorial;
using UI_Scripts.WindowManager;
using UnityEngine;
using UserData;
using UtilScripts;

namespace BattlefieldScripts
{
	[PoolSize(10, true, false)]
	[DefaultPrefab("Prefabs/BattlePrefabs/FieldMonster")]
	public class FieldMonsterVisual : FieldVisual
	{
		public const string MEELE_HIT_EVENT = "hit";

		public const string RANGED_FIRED_EVENT = "fire";

		private readonly CachedService<IDelayedActionsHandler> __delayedActionsHandler = new CachedService<IDelayedActionsHandler>();

		private bool _isLoaded;

		private bool _isScaleinited;

		private bool _isShifted;

		private Vector3 _normalLocalScale;

		private Action _onLoaded;

		private Vector3 _origPos;

		private Vector3? _posBeforeAttack;

		private Vector2 _tempCoords;

		public bool addSkill;

		public Vector2 animateCoords;

		public UIAtlas arrowAtlas;

		public string arrowName;

		private Vector3 atkCurrentPos;

		private Vector3 attackContainerPos;

		public bool deal1Damage;

		public float effectPower = 0.8f;

		public bool heal;

		private Vector3 hpContainerPos;

		private Vector3 hpCurrentPos;

		public bool kill;

		public int monsterId;

		public bool showClassLabel;

		public string skillName;

		public string skillValue;

		private Vector3 storedAttackContainerPos;

		private Vector3 storedHpContainerPos;

		[Header("Swap stats animation")]
		public Ease swapStatsInEase = Ease.Linear;

		public Ease swapStatsOutEase = Ease.Linear;

		public float tweenInTime = 0.15f;

		public float tweenOutDelay = 0.2f;

		public float tweenOutTime = 0.15f;

		private bool valuesStored;

		[SerializeField]
		private Transform _additionalCollideElement;

		private Vector3 _healthSize;

		private Vector3 _attackSize;

		protected FieldMonsterWrapper _wrapper;

		protected FieldMonster _curMonster;

		private float _paramsAlpha = 1f;

		private bool statsSwapped;

		private IDelayedActionsHandler _delayedActionsHandler => __delayedActionsHandler.Value;

		protected virtual float AnimationStartScale => 0.6f;

		public AnimatedMonster Image => _wrapper.image;

		public bool IsWarlord
		{
			get
			{
				if (_curMonster != null)
				{
					return _curMonster.data.is_warlord;
				}
				return false;
			}
		}

		public Vector2 Coords => _curMonster.coords;

		public MonsterData Data { get; private set; }

		public UISprite MonsterShadow => _wrapper.shadow;

		public bool blockAnimation { get; private set; }

		private bool CanShowNewVisualSkills
		{
			get
			{
				bool num = FieldScriptWrapper.GetBattleType() == FieldScriptWrapper.StartBattleArmyParams.BattleType.Pit;
				bool flag = FieldScriptWrapper.GetBattleType() == FieldScriptWrapper.StartBattleArmyParams.BattleType.Dungeon;
				bool flag2 = _curMonster.Side == ArmySide.Right;
				bool flag3 = !TutorialModule.Inited || TutorialModule.Instance.IsTutorialCompleted();
				if ((num || flag) && flag3 && flag2 && _wrapper.TryShowNewSkills)
				{
					return _wrapper.SkillsDescriptionController;
				}
				return false;
			}
		}

		public List<StaticAnimationBit> StaticAnimations { get; private set; }

		protected virtual void Update()
		{
			if (deal1Damage)
			{
				deal1Damage = false;
				_curMonster.ChangeParam(null, ParamType.Health, -1);
			}
			if (heal)
			{
				heal = false;
				_curMonster.ChangeParam(null, ParamType.Health, 100);
			}
			if (kill)
			{
				kill = false;
				_curMonster.ChangeParam(null, ParamType.Health, -(int)_curMonster.Health);
			}
			if (addSkill)
			{
				addSkill = false;
				if (!string.IsNullOrEmpty(skillName) && !string.IsNullOrEmpty(skillValue))
				{
					SkillStaticData skillByName = SkillDataHelper.GetSkillByName(skillName);
					_curMonster.AddSkill(skillByName, skillValue);
				}
			}
		}

		private void InitArrow()
		{
			arrowName = Data.staticInnerData.arrowSprite;
			if (arrowName.StartsWith("arrow_"))
			{
				ArrowAssetManager.Instance.LoadAtlasForSprite(arrowName, delegate(UIAtlas atlas, string spriteName)
				{
					arrowAtlas = atlas;
					arrowName = spriteName;
				});
			}
		}

		public virtual void Init(MonsterData newData, FieldMonster curMonster)
		{
			SetMonsterShadow(visible: true);
			_isLoaded = false;
			Data = newData;
			monsterId = newData.monster_id;
			_curMonster = curMonster;
			blockAnimation = false;
			_origPos = base.transform.localPosition;
			if (_wrapper == null)
			{
				_wrapper = GetComponent<FieldMonsterWrapper>();
			}
			SetFlyingStatus();
			InitArrow();
			if (newData.animationName != "")
			{
				_wrapper.image.skeletonAnimation.transform.localEulerAngles = new Vector3(0f, _wrapper.image.skeletonAnimation.transform.localEulerAngles.y, _wrapper.image.skeletonAnimation.transform.localEulerAngles.z);
				_wrapper.image.skeletonAnimation.gameObject.SetActive(value: true);
				_wrapper.image.Init(newData, delegate
				{
					_isLoaded = true;
					SetGold(newData.isElite);
					if (newData.isGoldMineWarlord)
					{
						_wrapper.image.AnimateGold();
					}
					_wrapper.image.ApplyAlpha(0f);
					_delayedActionsHandler.WaitForOneFrame(delegate
					{
						_wrapper.image.ApplyAlpha(1f);
					});
					if (!_wrapper.image.animatingAttack)
					{
						_wrapper.image.AnimateIdleLooped();
					}
					if (_onLoaded != null)
					{
						_onLoaded();
						_onLoaded = null;
					}
				}, shouldPlaceDefault: true);
				_wrapper.image.skeletonAnimation.transform.localScale = new Vector3(AnimationStartScale * newData.animationScale, AnimationStartScale * newData.animationScale, 1f);
			}
			else
			{
				_isLoaded = true;
			}
			StaticAnimations = new List<StaticAnimationBit>();
			if (newData.isGoldMineMonster)
			{
				_wrapper.goldLabel.text = newData.goldCount.ToString();
			}
			if ((bool)_wrapper.goldContainer)
			{
				_wrapper.goldContainer.gameObject.SetActive(newData.isGoldMineMonster);
			}
			_wrapper.hpContainer.gameObject.SetActive(value: true);
			_wrapper.atkContainer.gameObject.SetActive(!newData.isGoldMineMonster && newData.monsterClass != Class.Building);
			_paramsAlpha = _wrapper.hpLabel.alpha;
			_healthSize = _wrapper.hpLabel.transform.localScale;
			_attackSize = _wrapper.atkLabel.transform.localScale;
			if (_wrapper.monsterClassLabel != null)
			{
				_wrapper.monsterClassLabel.text = MonsterDataUtils.GetClassName(Data.monsterClass);
				_wrapper.monsterClassLabel.gameObject.SetActive(showClassLabel);
			}
			UpdateParameters();
			ResetParametersGlance();
			if (CheckIfShouldShowInfoIcon())
			{
				TutorialHandler.Instance.RecieveTrigger("NEW_MONSTER_SHOWN");
				_wrapper.infoIcon.gameObject.SetActive(value: true);
				_wrapper.infoIcon.SetCompleteDelegate(delegate
				{
					_wrapper.infoIcon.gameObject.SetActive(value: false);
					StatisticModule.Instance.SetMonsterAsSeenInBattle(Data.monster_id);
				});
			}
		}

		private bool CheckIfShouldShowInfoIcon()
		{
			if (Constants.show_new_units_pvp_info.IsUnlocked() && !Data.isLevelMonster && FieldScriptWrapper.currentBattleType == FieldScriptWrapper.StartBattleArmyParams.BattleType.PvP && _wrapper.infoIcon != null && !StatisticModule.Instance.CheckIfMonsterWasSeenInBattleById(Data.monster_id))
			{
				return !UserArmyModule.instance.HasInCollectionById(Data);
			}
			return false;
		}

		public void SetFlyingStatus()
		{
			if (_curMonster != null)
			{
				_wrapper.image.Flying = _curMonster.CanEvade;
			}
			else
			{
				_wrapper.image.Flying = false;
			}
		}

		public override void Destroy()
		{
			_isShifted = false;
			_isForwarded = false;
			blockAnimation = false;
			showClassLabel = false;
			SetParamsVisible(visible: true);
			Animate(MonsterAnimationType.Idle);
			_posBeforeAttack = null;
			if (_wrapper != null)
			{
				_wrapper.image.skeletonAnimation.gameObject.SetActive(value: false);
			}
			Transform transform = base.transform.Find("DivineShieldPrefab(Clone)");
			if (transform != null)
			{
				string hierarchyPath = base.transform.GetHierarchyPath();
				Debug.LogError("Has DivineShieldPrefab\n" + hierarchyPath);
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			InstantiateHelper.Push(this);
		}

		public void Transform(MonsterData newData)
		{
			_wrapper.image.Init(newData, delegate
			{
				_wrapper.image.AnimateIdleLooped();
			}, shouldPlaceDefault: true);
			Data = newData;
			_wrapper.atkContainer.gameObject.SetActive(Data.monsterClass != Class.Building);
			UpdateParameters();
			UpdateSkills();
		}

		public void SetHpAttackTogether(bool together, Common.VoidDelegate onComplete = null)
		{
			Vector3 endPos;
			Vector3 endPos2;
			if (together)
			{
				if (!statsSwapped)
				{
					atkCurrentPos = attackContainerPos;
					hpCurrentPos = hpContainerPos;
				}
				endPos = attackContainerPos + (hpContainerPos - attackContainerPos) * (0.5f * effectPower);
				endPos2 = hpContainerPos + (attackContainerPos - hpContainerPos) * (0.5f * effectPower);
			}
			else
			{
				endPos = atkCurrentPos;
				endPos2 = hpCurrentPos;
				statsSwapped = !statsSwapped;
			}
			float tweenDelay = (together ? 0f : tweenOutDelay);
			Ease ease = (together ? swapStatsInEase : swapStatsOutEase);
			float tweenTime = (together ? tweenInTime : tweenOutTime);
			int k = 0;
			_wrapper.hpContainer.transform.TryTweenLocalPosition(endPos2, tweenTime, tweenDelay, ease, shouldTween: true, delegate
			{
				k++;
			});
			_wrapper.atkContainer.transform.TryTweenLocalPosition(endPos, tweenTime, tweenDelay, ease, shouldTween: true, delegate
			{
				k++;
			});
			_delayedActionsHandler.WaitForCondition(() => k == 2, onComplete.SafeInvoke);
		}

		public void DialogState(bool state)
		{
			_wrapper.image.skeletonAnimation.gameObject.SetLayerRecursively(Initializer.UIRootContainer.GetUIRoot((!state) ? LayerName.Battle_Layer : LayerName.GUI).gameObject.layer);
		}

		public void DialogRotate()
		{
			Vector3 localEulerAngles = _wrapper.image.transform.localRotation.eulerAngles + new Vector3(0f, 180f, 0f);
			_wrapper.image.skeletonAnimation.transform.localEulerAngles = localEulerAngles;
		}

		public void SetParamsVisible(bool visible, float time = 0f)
		{
			_wrapper.hpSprite.alpha = (visible ? _paramsAlpha : 0f);
			_wrapper.atkLabel.alpha = (visible ? _paramsAlpha : 0f);
			_wrapper.hpLabel.alpha = (visible ? _paramsAlpha : 0f);
			_wrapper.meleeSprite.alpha = (visible ? _paramsAlpha : 0f);
			_wrapper.bowSprite.alpha = (visible ? _paramsAlpha : 0f);
		}

		public void HideParams()
		{
			_wrapper.hpContainer.gameObject.SetActive(value: false);
			if ((bool)_wrapper.goldContainer)
			{
				_wrapper.goldContainer.gameObject.SetActive(value: false);
			}
			_wrapper.atkContainer.gameObject.SetActive(value: false);
		}

		public void ShowParams()
		{
			_wrapper.hpContainer.gameObject.SetActive(value: true);
			_wrapper.atkContainer.gameObject.SetActive(value: true);
		}

		public void Animate(MonsterAnimationType type, Dictionary<string, Common.VoidDelegate> eventDelegates = null, bool pet = false)
		{
			if (blockAnimation)
			{
				return;
			}
			if (eventDelegates == null)
			{
				eventDelegates = new Dictionary<string, Common.VoidDelegate>();
			}
			SetFlyingStatus();
			switch (type)
			{
			case MonsterAnimationType.MeleeHit:
				_wrapper.image.AnimateMeleeHit(eventDelegates);
				break;
			case MonsterAnimationType.Damaged:
				if (pet)
				{
					_delayedActionsHandler.WaitForProcedure(TimeDebugController.instance.delayAnimPet, delegate
					{
						_wrapper.image.AnimateHitted();
					});
				}
				else
				{
					_wrapper.image.AnimateHitted();
				}
				break;
			case MonsterAnimationType.Death:
				_wrapper.hpContainer.gameObject.SetActive(value: false);
				_wrapper.atkContainer.gameObject.SetActive(value: false);
				foreach (UISprite skill in _wrapper.skills)
				{
					skill.enabled = false;
				}
				_wrapper.image.AnimateDeath();
				SetMonsterShadow(visible: false);
				blockAnimation = true;
				break;
			case MonsterAnimationType.RangedShot:
				_wrapper.image.AnimateRangedShoot(eventDelegates);
				break;
			case MonsterAnimationType.Idle:
				_wrapper.image.AnimateIdleLooped();
				break;
			case MonsterAnimationType.Victory:
				_wrapper.image.AnimateVictory();
				break;
			}
		}

		public void AnimateDeathGoldWarlord()
		{
			AnimatedMonster monsterImage = _wrapper.image;
			monsterImage.GoldAlpha = 1f;
			_wrapper.shadow.alpha = 1f;
			_delayedActionsHandler.WaitForProcedure(_wrapper.animationDelay, delegate
			{
				monsterImage.AnimateMaterialization(MaterializationAnimator.MaterialType.GoldNoShake, _wrapper.shakeTime);
				float amplitude = 0f;
				Vector3 _normalPosition = monsterImage.transform.localPosition;
				_delayedActionsHandler.WaitForProcedure(_wrapper.fadeDelay, delegate
				{
					DOTween.To(() => monsterImage.GoldAlpha, delegate(float x)
					{
						monsterImage.GoldAlpha = x;
						_wrapper.shadow.alpha = x;
					}, 0f, _wrapper.fadeTime);
				});
				DOTween.To(() => amplitude, delegate(float x)
				{
					amplitude = x;
					float num = Mathf.Sin(amplitude * 2f) * _wrapper.shakeXAmpl;
					float num2 = Mathf.Cos(amplitude) * _wrapper.shakeYAmpl;
					monsterImage.transform.localPosition = new Vector3(_normalPosition.x + num, _normalPosition.y + num2, _normalPosition.z);
				}, _wrapper.shakeMult * _wrapper.shakeTime, _wrapper.shakeTime).SetEase(_wrapper.shakeEase).OnComplete(delegate
				{
					monsterImage.transform.localPosition = _normalPosition;
					GameObject gameObject = EffectCreator.CreateEffect("Rays_Effect", _wrapper.EffectsContainer.transform, new Vector3(0f, 0f, 5f), _wrapper.effectLifeTime);
					GameObject obj = EffectCreator.CreateEffect("Sparks_Effect", _wrapper.EffectsContainer.transform, new Vector3(0f, 0f, 5f), _wrapper.effectLifeTime);
					gameObject.transform.localScale *= _wrapper.effectScale;
					obj.transform.localScale *= _wrapper.effectScale;
				});
			});
		}

		private void SetMonsterShadow(bool visible)
		{
			if (_wrapper != null && MonsterShadow != null)
			{
				MonsterShadow.gameObject.SetActive(visible);
			}
		}

		public void ApplyColorTint(Color tint)
		{
			_wrapper.image.ApplyColorTint(tint);
		}

		public void SetBigScale(bool scaled)
		{
			if (!_isScaleinited)
			{
				_isScaleinited = true;
				_normalLocalScale = base.transform.localScale;
			}
			if (base.transform != null)
			{
				base.transform.localScale = _normalLocalScale * (scaled ? 1.2f : 1f);
			}
		}

		public void AnimateVictory()
		{
		}

		public void AnimateDefeat()
		{
		}

		public void AnimateDeath()
		{
			SoundManager.Instance.PlaySound(Data.staticInnerData.deathSound);
			if (UnityEngine.Random.value > 0.5f)
			{
				SoundManager.Instance.PlaySound(Data.staticInnerData.deathVoice);
			}
			EffectAnimation effectAnimation = new EffectAnimation(Data.staticInnerData.deathEffect);
			effectAnimation.Init(this);
			effectAnimation.Animate(new Dictionary<Common.StringDelegate, FieldVisual> { 
			{
				() => "",
				this
			} }, delegate
			{
			});
		}

		public void AnimateDamage()
		{
			if (UnityEngine.Random.value > 0.5f)
			{
				SoundManager.Instance.PlaySound(Data.staticInnerData.damagedVoice);
			}
		}

		public Vector3 GetShootPosition()
		{
			return _wrapper.image.GetShootBonePosition();
		}

		public void InformTrigger(TriggerType trigger)
		{
			for (int num = StaticAnimations.Count - 1; num > -1; num--)
			{
				StaticAnimations[num].InformTrigger(trigger);
			}
			switch (trigger)
			{
			case TriggerType.ClearParams:
				_wrapper.image.ClearParams();
				break;
			case TriggerType.NewTurn:
				SetSkillsTurn();
				break;
			case TriggerType.TurnEnded:
				SetSkillsTurn(1);
				UpdateSkillsDescription();
				break;
			}
			UpdateParameters();
		}

		public void InformCleanse()
		{
			for (int num = StaticAnimations.Count - 1; num > -1; num--)
			{
				StaticAnimations[num].InformCleanse();
			}
			UpdateParameters();
		}

		public void UpdateSkills()
		{
			foreach (UISprite skill in _wrapper.skills)
			{
				skill.enabled = false;
			}
			if (_curMonster == null)
			{
				return;
			}
			List<ActionBitSignature> skills = _curMonster.Skills;
			skills.AddRange(_curMonster.Perks);
			List<string> list = new List<string>();
			foreach (ActionBitSignature item in skills)
			{
				SkillStaticData skillByName = SkillDataHelper.GetSkillByName(item.name);
				if (skillByName != null && skillByName.showInBattle)
				{
					string skill_icon = skillByName.skill_icon;
					if (skill_icon == null || !list.Contains(skill_icon))
					{
						list.Add(skill_icon);
					}
				}
			}
			if (CanShowNewVisualSkills)
			{
				_wrapper.SkillsDescriptionController.gameObject.SetActive(value: true);
				UpdateSkillsDescription();
			}
			else if ((bool)_wrapper.SkillsDescriptionController)
			{
				_wrapper.SkillsDescriptionController.gameObject.SetActive(value: false);
			}
			if (list.Count == 0)
			{
				return;
			}
			for (int i = 0; i < _wrapper.skills.Count; i++)
			{
				UISprite uISprite = _wrapper.skills[i];
				if (i >= list.Count || CanShowNewVisualSkills)
				{
					uISprite.enabled = false;
					continue;
				}
				uISprite.enabled = true;
				SearchImgFromDifAtlases.SetNameAndAtlas(uISprite, list[i]);
				uISprite.ResizeSpriteByHeight();
			}
		}

		private void UpdateSkillsDescription()
		{
			if (_wrapper.SkillsDescriptionController != null)
			{
				_wrapper.SkillsDescriptionController.Init(_curMonster.data.SkillsToShow(), GetCurrentSkillsValues());
				_wrapper.SkillsDescriptionController.SetTurnCount(WindowScriptCore<BattlefieldWindow>.instance.CurrentTurn);
			}
		}

		private List<string> GetCurrentSkillsValues()
		{
			_curMonster.data.GetSkillStates();
			List<string> list = new List<string>();
			List<ActionBitSignature> monsterSignatures = MonsterDataUtils.GetMonsterSignatures(_curMonster);
			List<SkillStaticData> dataSkills = _curMonster.data.SkillsToShow();
			int num = 0;
			int i;
			for (i = 0; i < dataSkills.Count; i++)
			{
				if (dataSkills[i].trigger != TriggerType.WarlordSkill4)
				{
					SkillStaticData skillByName = SkillDataHelper.GetSkillByName(dataSkills[i].strId);
					List<string> list2 = monsterSignatures.FindAll((ActionBitSignature x) => x.name == dataSkills[i].strId).ConvertAll((ActionBitSignature x) => x.strValue);
					string item = ((skillByName.skill == SkillType.ExtraAttack) ? num.ToString() : MonsterDataUtils.GetSkillValue(list2.ToArray()));
					list.Add(item);
				}
			}
			return list;
		}

		public void SetSkillsTurn(int offset = 0)
		{
			if (CanShowNewVisualSkills)
			{
				_wrapper.SkillsDescriptionController.SetTurnCount(WindowScriptCore<BattlefieldWindow>.instance.CurrentTurn, offset);
			}
		}

		public void ResetParametersGlance()
		{
			if (_wrapper.transform != null)
			{
				_wrapper.hpGoodGlance.enabled = false;
				_wrapper.hpBadGlance.enabled = false;
				_wrapper.atkGoodGlance.enabled = false;
				_wrapper.atkBadGlance.enabled = false;
			}
		}

		public void SetParanetersGlance(bool isHp, bool isGood)
		{
			if (isHp)
			{
				if (isGood)
				{
					_wrapper.hpGoodGlance.enabled = true;
				}
				else
				{
					_wrapper.hpBadGlance.enabled = true;
				}
			}
			else if (isGood)
			{
				_wrapper.atkGoodGlance.enabled = true;
			}
			else
			{
				_wrapper.atkBadGlance.enabled = true;
			}
		}

		public void UpdateParameters()
		{
			if (_curMonster != null)
			{
				if ((int)_curMonster.Health < (int)_curMonster.MaxHealth)
				{
					_wrapper.hpLabel.text = Localization.Localize("#param_red").Replace("%val%", ((string)_curMonster.Health) ?? "");
				}
				else if ((int)_curMonster.MaxHealth > (int)Data.health)
				{
					_wrapper.hpLabel.text = Localization.Localize("#param_green").Replace("%val%", ((string)_curMonster.Health) ?? "");
				}
				else
				{
					_wrapper.hpLabel.text = ((string)_curMonster.Health) ?? "";
				}
				if (_curMonster.Attack > (int)Data.attack)
				{
					_wrapper.atkLabel.text = Localization.Localize("#param_green").Replace("%val%", string.Concat(_curMonster.Attack));
				}
				else if (_curMonster.Attack < (int)Data.attack)
				{
					_wrapper.atkLabel.text = Localization.Localize("#param_red").Replace("%val%", string.Concat(_curMonster.Attack));
				}
				else
				{
					_wrapper.atkLabel.text = string.Concat(_curMonster.Attack);
				}
				bool flag = _curMonster.IsRanged();
				if (_wrapper.bowSprite != null)
				{
					_wrapper.bowSprite.enabled = flag;
				}
				if (_wrapper.meleeSprite != null)
				{
					_wrapper.meleeSprite.enabled = !flag;
				}
			}
		}

		public override bool Collided(Vector3 position)
		{
			if (_wrapper == null || _wrapper.collideElement == null)
			{
				return false;
			}
			bool flag = false;
			Func<Transform, bool> func = delegate(Transform collideElement)
			{
				bool num = position.y > collideElement.position.y - collideElement.lossyScale.y / 2f && position.y < collideElement.position.y + collideElement.lossyScale.y / 2f;
				bool flag2 = position.x > collideElement.position.x - collideElement.lossyScale.x / 2f && position.x < collideElement.position.x + collideElement.lossyScale.x / 2f;
				return num && flag2;
			};
			flag = func(_wrapper.collideElement);
			if (_additionalCollideElement != null && _additionalCollideElement.gameObject.activeInHierarchy)
			{
				flag = flag || func(_additionalCollideElement);
			}
			return flag;
		}

		public virtual void ApplyEulerAngles(Vector3 angle)
		{
			if (_wrapper == null)
			{
				_wrapper = GetComponent<FieldMonsterWrapper>();
			}
			if (!valuesStored)
			{
				storedAttackContainerPos = _wrapper.atkContainer.localPosition;
				storedHpContainerPos = _wrapper.hpContainer.localPosition;
				valuesStored = true;
			}
			_wrapper.image.skeletonAnimation.transform.localEulerAngles = new Vector3(_wrapper.image.skeletonAnimation.transform.localEulerAngles.x, angle.y, angle.z);
			_wrapper.hpContainer.localPosition = new Vector3(1f * Mathf.Abs(storedHpContainerPos.x), storedHpContainerPos.y, storedHpContainerPos.z);
			_wrapper.atkContainer.localPosition = new Vector3(-1f * Mathf.Abs(storedAttackContainerPos.x), storedAttackContainerPos.y, storedAttackContainerPos.z);
			hpContainerPos = _wrapper.hpContainer.localPosition;
			attackContainerPos = _wrapper.atkContainer.localPosition;
		}

		public void PreActionAnimation(TriggerType trigger, Action onAnimation)
		{
			if (trigger == TriggerType.Attack && Data.monsterClass == Class.Melee)
			{
				if (!_isShifted)
				{
					int num = ((_curMonster.Side == ArmySide.Left) ? 1 : (-1));
					bool num2 = _curMonster.parameters.GetMonsters(_curMonster.Side).ContainsKey(new Vector2(Coords.x + (float)num, Coords.y));
					bool flag = _curMonster.parameters.GetWarlord(_curMonster.Side).coords == Coords;
					bool flag2 = !_curMonster.canAttack;
					if (num2 || flag || flag2)
					{
						onAnimation();
						return;
					}
					_origPos = base.transform.localPosition;
					animateCoords = Coords;
				}
				else if (Constants.show_battle_logs)
				{
					string text = (_posBeforeAttack.HasValue ? _posBeforeAttack.Value.ToString() : "null");
					Debug.LogError(string.Concat("PreActionAnimation. Not shifted. ", _curMonster.data, ", _origPos ", _origPos, ", _posBeforeAttack ", text));
				}
				Dictionary<Vector2, FieldMonster> monsters = _curMonster.parameters.GetMonsters(_curMonster.EnemySide);
				_tempCoords = new Vector2(1000f, 10000f);
				Vector3 vector = _origPos;
				foreach (Vector2 key in monsters.Keys)
				{
					if (key.y == Coords.y && Math.Abs(_tempCoords.x - Coords.x) > Math.Abs(key.x - Coords.x))
					{
						_tempCoords = key;
						vector = monsters[key].VisualMonster.transform.localPosition;
					}
				}
				if (_tempCoords.x == 1000f)
				{
					_tempCoords = _curMonster.parameters.GetWarlord(_curMonster.EnemySide).coords;
					vector = _curMonster.parameters.GetWarlord(_curMonster.EnemySide).VisualMonster.transform.localPosition;
				}
				int num3 = (int)Mathf.Abs(_tempCoords.x - animateCoords.x);
				animateCoords = _tempCoords;
				animateCoords.x += ((_curMonster.Side != ArmySide.Left) ? 1 : (-1));
				float num4 = (vector - _origPos).x / (float)num3;
				Vector3 endValue = new Vector3(vector.x, base.transform.localPosition.y, base.transform.localPosition.z);
				endValue.x -= num4;
				float duration = (float)(num3 - 1) / (TimeDebugController.instance.meleeAnimInSpeed * TimeDebugController.totalTimeMultiplier);
				_posBeforeAttack = base.transform.localPosition;
				DOTween.To(() => base.transform.localPosition, delegate(Vector3 x)
				{
					base.transform.localPosition = new Vector3(x.x, base.transform.localPosition.y, base.transform.localPosition.z);
				}, endValue, duration).OnComplete(delegate
				{
					onAnimation();
				}).SetEase(TimeDebugController.instance.meleeInEase)
					.Play();
				_isShifted = true;
			}
			else
			{
				onAnimation();
			}
		}

		public bool GetDieState()
		{
			return _curMonster.ShouldDie;
		}

		public void PostActionAnimation(TriggerType trigger, Action onAnimation)
		{
			bool show_battle_logs = Constants.show_battle_logs;
			bool use_anim_hack_in_battle = Constants.use_anim_hack_in_battle;
			bool use_anim_hack_in_battle_ = Constants.use_anim_hack_in_battle_2;
			if (show_battle_logs && trigger == TriggerType.Attack && Data.monsterClass == Class.Melee)
			{
				Debug.Log(string.Concat("PostActionAnimation. ", _curMonster.data, ", trigger ", trigger, ", _isShifted ", _isShifted.ToString(), ", ShouldDie ", _curMonster.ShouldDie.ToString()));
			}
			if (trigger == TriggerType.Attack && Data.monsterClass == Class.Melee && _isShifted)
			{
				_isShifted = false;
				if (_curMonster.ShouldDie)
				{
					_delayedActionsHandler.WaitForProcedure(TimeDebugController.instance.meleeMiddleDelay / TimeDebugController.totalTimeMultiplier, delegate
					{
						onAnimation();
					});
					return;
				}
				float num = (float)(int)Mathf.Abs(Coords.x - animateCoords.x) / (TimeDebugController.instance.meleeAnimOutSpeed * TimeDebugController.totalTimeMultiplier);
				float num2 = 0f;
				if (Mathf.Abs(_origPos.x - base.transform.localPosition.x) > 1f)
				{
					num2 = TimeDebugController.instance.meleeMiddleDelay / TimeDebugController.totalTimeMultiplier;
				}
				Common.VoidDelegate OnAnimComplete = delegate
				{
					_origPos = base.transform.localPosition;
					onAnimation();
				};
				TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => base.transform.localPosition, delegate(Vector3 x)
				{
					base.transform.localPosition = new Vector3(x.x, base.transform.localPosition.y, base.transform.localPosition.z);
				}, _origPos, num);
				t.SetDelay(num2);
				t.SetEase(TimeDebugController.instance.meleeOutEase).OnComplete(delegate
				{
					OnAnimComplete();
				}).Play();
				if (show_battle_logs && trigger == TriggerType.Attack)
				{
					Debug.Log(string.Concat("PostActionAnimation. tweenOut play. ", _curMonster.data, ", timeOut ", num, ", _origPos ", _origPos, ", _posBeforeAttack ", _posBeforeAttack, ", delay ", num2));
				}
				if (!use_anim_hack_in_battle)
				{
					return;
				}
				Vector3 origPos = _origPos;
				_delayedActionsHandler.WaitForProcedure(num + num2, delegate
				{
					if (base.transform.localPosition.x != origPos.x)
					{
						Debug.LogError(string.Concat("Caught tween monster bug! monster: ", _curMonster.data, ". return to origPos = ", origPos));
						base.transform.localPosition = new Vector3(origPos.x, base.transform.localPosition.y, base.transform.localPosition.z);
					}
				});
			}
			else
			{
				onAnimation();
				if (trigger == TriggerType.Attack && Data.monsterClass == Class.Melee && use_anim_hack_in_battle_ && !_curMonster.HasDoubleAttack && _posBeforeAttack.HasValue && base.transform.localPosition.x != _posBeforeAttack.Value.x)
				{
					_isShifted = false;
					Debug.LogError(string.Concat("Caught tween monster bug(2)! monster: ", _curMonster.data, ". return to origPos = ", _posBeforeAttack));
					base.transform.localPosition = new Vector3(_posBeforeAttack.Value.x, base.transform.localPosition.y, base.transform.localPosition.z);
				}
			}
		}

		public void BounceHealth()
		{
			try
			{
				TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => _wrapper.hpLabel.transform.localScale, delegate(Vector3 x)
				{
					_wrapper.hpLabel.transform.localScale = new Vector3(x.x, x.y, _healthSize.z);
				}, _healthSize * 1.5f, 0.1f);
				TweenCallback action = delegate
				{
					DOTween.To(() => _wrapper.hpLabel.transform.localScale, delegate(Vector3 x)
					{
						_wrapper.hpLabel.transform.localScale = new Vector3(x.x, x.y, _healthSize.z);
					}, _healthSize, 0.1f).Play();
				};
				t.OnComplete(action);
				t.Play();
			}
			catch (Exception ex)
			{
				Debug.LogError("BounceHealth Error.\n" + ex);
				_wrapper.hpLabel.transform.localScale = _healthSize;
			}
		}

		public void BounceAttack()
		{
			try
			{
				TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => _wrapper.atkLabel.transform.localScale, delegate(Vector3 x)
				{
					_wrapper.atkLabel.transform.localScale = new Vector3(x.x, x.y, _attackSize.z);
				}, _attackSize * 1.5f, 0.1f);
				TweenCallback action = delegate
				{
					DOTween.To(() => _wrapper.atkLabel.transform.localScale, delegate(Vector3 x)
					{
						_wrapper.atkLabel.transform.localScale = new Vector3(x.x, x.y, _attackSize.z);
					}, _attackSize, 0.1f).Play();
				};
				t.OnComplete(action);
				t.Play();
			}
			catch (Exception ex)
			{
				Debug.LogError("BounceAttack Error.\n" + ex);
				_wrapper.atkLabel.transform.localScale = _attackSize;
			}
		}

		public void AttachStaticAnimation(StaticAnimationBit newStaticAnimation)
		{
			StaticAnimations.Add(newStaticAnimation);
		}

		public void DeattachStaticAnimation(StaticAnimationBit staticAnimation)
		{
			StaticAnimations.Remove(staticAnimation);
		}

		public void DeattachAllStaticAnimations()
		{
			for (int num = StaticAnimations.Count - 1; num > -1; num--)
			{
				StaticAnimations[num].InformDeath();
			}
		}

		public void CleanStaticAnimation()
		{
			for (int num = StaticAnimations.Count - 1; num > -1; num--)
			{
				if (StaticAnimations[num].GetShouldDeleteAnim())
				{
					StaticAnimations[num].InformCleanse();
				}
			}
		}

		public void SetGold(bool gold)
		{
			if (gold)
			{
				_wrapper.image.ApplyMixedGoldFx(Color.yellow);
			}
			else
			{
				_wrapper.image.ResetEffects();
			}
		}
	}
}
