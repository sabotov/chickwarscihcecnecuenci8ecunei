using System;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts.UtilScripts.Loaders;
using DG.Tweening;
using Gameplay;
using MyEffects;
using NGUI.Scripts.UI;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using Spine;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts
{
	public class AnimatedMonster : ShaderAnimation
	{
		private static readonly List<MonsterAnimationType> AllowedAnimations = new List<MonsterAnimationType>
		{
			MonsterAnimationType.Damaged,
			MonsterAnimationType.MeleeHit,
			MonsterAnimationType.RangedShot,
			MonsterAnimationType.Victory
		};

		public SkeletonAnimation skeletonAnimation;

		[SerializeField]
		private UISprite _shadowSprite;

		public BoneManager boneManager;

		private List<Material> _animationMaterials;

		private List<MonsterAnimationType> curAllowedAnimations;

		public int curAnimIndex;

		private string curAnimName = "";

		private string loadingAnimName = "";

		private LoaderCore<SkeletonDataAsset>.OnLoadedDelegate _onSkeletonLoaded;

		private Action _onLoadedAnimation;

		private bool shouldDie;

		private const string DAMAGED_NAME = "damage";

		private const string IDLE_NAME = "idle";

		private const string DEATH_NAME = "death";

		private const string MELEE_HIT_NAME = "hit";

		private const string RANGED_SHOT_NAME = "rangehit";

		private const string VICTORY_NAME = "win";

		private const string FLYING_NAME = "idle_fly";

		private bool _flying;

		private readonly Dictionary<string, Common.VoidDelegate> _eventsDelegates = new Dictionary<string, Common.VoidDelegate>();

		private Action _onCompleted;

		private bool _shouldLaunchIddle;

		private bool _shouldFreezeIddle;

		private bool _isInited;

		private bool _clearParams;

		public bool loadAnimOnStart;

		public string animationName;

		public bool forceFlying;

		public bool animatingAttack;

		private bool _hittedLock;

		public bool IsLoaded
		{
			get
			{
				if (skeletonAnimation != null)
				{
					return skeletonAnimation.IsLoaded;
				}
				return false;
			}
		}

		public bool Ready => _isInited;

		public bool Flying
		{
			get
			{
				if (!forceFlying)
				{
					return _flying;
				}
				return true;
			}
			set
			{
				if (_flying != value)
				{
					_flying = value;
					if (skeletonAnimation.AnimationName == "idle" || skeletonAnimation.AnimationName == "idle_fly")
					{
						_shouldFreezeIddle = skeletonAnimation.paused;
						AnimateIdleLooped();
					}
				}
			}
		}

		public float Alpha => skeletonAnimation.Alpha;

		public void InitAllowedAnimations(Class monsterClass)
		{
			curAnimIndex = 0;
			AnimateByType(MonsterAnimationType.Idle);
			curAllowedAnimations = new List<MonsterAnimationType>();
			foreach (MonsterAnimationType allowedAnimation in AllowedAnimations)
			{
				bool flag = true;
				switch (allowedAnimation)
				{
				case MonsterAnimationType.MeleeHit:
					flag = monsterClass == Class.Melee;
					break;
				case MonsterAnimationType.RangedShot:
					flag = monsterClass == Class.Ranged || monsterClass == Class.Building;
					break;
				}
				if (flag)
				{
					curAllowedAnimations.Add(allowedAnimation);
				}
			}
		}

		public void ShowNextAnimation()
		{
			curAnimIndex++;
			curAnimIndex %= curAllowedAnimations.Count;
			AnimateByType(curAllowedAnimations[curAnimIndex]);
		}

		public void Init()
		{
			Init("Warrior");
		}

		public void Init(MonsterData monster, Common.VoidDelegate onLoaded = null, bool shouldPlaceDefault = false)
		{
			Init(monster.animationName, onLoaded, shouldPlaceDefault);
		}

		public void Init(string animationName, Common.VoidDelegate onLoaded = null, bool shouldPlaceDefault = false)
		{
			_isInited = false;
			_animationMaterials = null;
			_hittedLock = false;
			skeletonAnimation.AnimationName = "idle";
			if (!string.IsNullOrEmpty(loadingAnimName))
			{
				MonsterAnimationAssetManager.Instance.RemoveWaitingDelegate(loadingAnimName, _onSkeletonLoaded);
			}
			if (!shouldPlaceDefault)
			{
				base.gameObject.SetActive(value: false);
			}
			loadingAnimName = animationName;
			_onSkeletonLoaded = delegate(SkeletonDataAsset skeleton, bool isRightBundle)
			{
				try
				{
					curAnimName = animationName;
					OnSkeletonLoaded(skeleton);
				}
				catch (Exception message)
				{
					Debug.LogError(message);
					OnSkeletonLoaded(MonsterAnimationAssetManager.Instance.GetDefaultAsset());
				}
				onLoaded?.Invoke();
				if (!shouldPlaceDefault)
				{
					base.gameObject.SetActive(value: true);
				}
			};
			if (shouldPlaceDefault && !MonsterAnimationAssetManager.Instance.ElementLoaded(animationName))
			{
				AnimateDefault();
			}
			MonsterAnimationAssetManager.Instance.GetElement(animationName, _onSkeletonLoaded);
		}

		private void OnSkeletonLoaded(SkeletonDataAsset skeleton)
		{
			if (skeleton == null)
			{
				Debug.LogError("OnSkeletonLoaded skeleton == null");
				return;
			}
			loadingAnimName = "";
			_onSkeletonLoaded = null;
			if (skeletonAnimation.skeletonDataAsset == null || skeletonAnimation.skeletonDataAsset != skeleton)
			{
				skeletonAnimation.skeletonDataAsset = skeleton;
				skeletonAnimation.Initialize(overwrite: true);
			}
			boneManager.ResetBones();
			boneManager.PrepareBones();
			skeletonAnimation.state.Event += HandleEvent;
			_animationMaterials = new List<Material>(skeletonAnimation.GetComponent<MeshRenderer>().materials);
			_isInited = true;
			if (_onLoadedAnimation != null)
			{
				_onLoadedAnimation();
				_onLoadedAnimation = null;
			}
		}

		private void AnimateDefault()
		{
			SkeletonDataAsset defaultAsset = MonsterAnimationAssetManager.Instance.GetDefaultAsset();
			if (defaultAsset == null)
			{
				Debug.LogError("AnimateDefault skeleton == null");
				return;
			}
			if (skeletonAnimation.skeletonDataAsset == null || skeletonAnimation.skeletonDataAsset != defaultAsset)
			{
				skeletonAnimation.skeletonDataAsset = defaultAsset;
				skeletonAnimation.Initialize(overwrite: true);
			}
			boneManager.ResetBones();
			boneManager.PrepareBones();
			skeletonAnimation.state.Event += HandleEvent;
		}

		public void RecheckFlying()
		{
			if (skeletonAnimation.AnimationName == "idle" || skeletonAnimation.AnimationName == "idle_fly")
			{
				AnimateIdleLooped();
			}
		}

		private void Start()
		{
			if (loadAnimOnStart && !string.IsNullOrEmpty(animationName))
			{
				animationName = animationName.ToLower();
				Init(animationName);
			}
		}

		public override void Update()
		{
			base.Update();
			if (_shouldLaunchIddle)
			{
				AnimateIdleLooped();
				_shouldLaunchIddle = false;
				_shouldFreezeIddle = false;
			}
		}

		public void ClearParams()
		{
			_clearParams = true;
		}

		public float GetAnimationDuration(string animationName)
		{
			return skeletonAnimation.Skeleton.data.FindAnimation(animationName).Duration / skeletonAnimation.timeScale;
		}

		public void AnimateIdleLooped()
		{
			_onCompleted = null;
			_shouldLaunchIddle = false;
			skeletonAnimation.state.ClearEnd();
			skeletonAnimation.state.ClearTracks();
			skeletonAnimation.loop = true;
			skeletonAnimation.timeScale = TimeDebugController.instance.idleAnimMultiplier;
			if (Flying && skeletonAnimation.Skeleton.Data.FindAnimation("idle_fly") != null)
			{
				skeletonAnimation.AnimationName = "idle_fly";
				skeletonAnimation.state.SetAnimation(0, "idle_fly", skeletonAnimation.loop);
			}
			else
			{
				skeletonAnimation.AnimationName = "idle";
				skeletonAnimation.state.SetAnimation(0, "idle", skeletonAnimation.loop);
			}
			if (_shouldFreezeIddle && !_clearParams)
			{
				FreezeMeshes(freeze: true);
			}
			else if (_clearParams)
			{
				_clearParams = false;
			}
		}

		public void AnimateMeleeHit(Dictionary<string, Common.VoidDelegate> onHitEvents, Action onAnimation = null)
		{
			_onCompleted = onAnimation;
			_shouldLaunchIddle = false;
			skeletonAnimation.state.ClearEnd();
			skeletonAnimation.loop = false;
			skeletonAnimation.timeScale = TimeDebugController.instance.meleeHitAnimMultiplier * TimeDebugController.totalTimeMultiplier;
			_eventsDelegates.Clear();
			foreach (KeyValuePair<string, Common.VoidDelegate> onHitEvent in onHitEvents)
			{
				if (_eventsDelegates.ContainsKey(onHitEvent.Key))
				{
					_eventsDelegates.Remove(onHitEvent.Key);
				}
				_eventsDelegates.Add(onHitEvent.Key, onHitEvent.Value);
			}
			skeletonAnimation.state.ClearTracks();
			skeletonAnimation.AnimationName = "hit";
			skeletonAnimation.state.SetAnimation(0, "hit", skeletonAnimation.loop);
			skeletonAnimation.state.End += OnAnimationEnded;
			animatingAttack = true;
		}

		public void AnimateRangedShoot(Dictionary<string, Common.VoidDelegate> onHitEvents)
		{
			_onCompleted = null;
			_shouldLaunchIddle = false;
			skeletonAnimation.state.ClearEnd();
			skeletonAnimation.loop = false;
			skeletonAnimation.timeScale = TimeDebugController.instance.rangedShotAnimMultiplier * TimeDebugController.totalTimeMultiplier;
			_eventsDelegates.Clear();
			foreach (KeyValuePair<string, Common.VoidDelegate> onHitEvent in onHitEvents)
			{
				if (_eventsDelegates.ContainsKey(onHitEvent.Key))
				{
					_eventsDelegates.Remove(onHitEvent.Key);
				}
				_eventsDelegates.Add(onHitEvent.Key, onHitEvent.Value);
			}
			skeletonAnimation.state.ClearTracks();
			skeletonAnimation.AnimationName = "rangehit";
			skeletonAnimation.state.SetAnimation(0, "rangehit", skeletonAnimation.loop);
			skeletonAnimation.state.End += OnAnimationEnded;
			animatingAttack = true;
		}

		public void AnimateDeath(Action onAnimation = null)
		{
			shouldDie = true;
			_eventsDelegates.Clear();
			_onCompleted = onAnimation;
			_shouldLaunchIddle = false;
			_shouldFreezeIddle = false;
			skeletonAnimation.state.ClearEnd();
			skeletonAnimation.loop = false;
			skeletonAnimation.timeScale = TimeDebugController.instance.deathAnimMultiplier * TimeDebugController.totalTimeMultiplier;
			skeletonAnimation.state.ClearTracks();
			skeletonAnimation.AnimationName = "death";
			skeletonAnimation.state.SetAnimation(0, "death", skeletonAnimation.loop);
			skeletonAnimation.state.End += OnAnimationEndedNoRelaunch;
		}

		public void AnimateVictory()
		{
			_eventsDelegates.Clear();
			_onCompleted = null;
			_shouldLaunchIddle = false;
			skeletonAnimation.state.ClearEnd();
			skeletonAnimation.loop = false;
			skeletonAnimation.timeScale = TimeDebugController.instance.victoryAnimMultiplier * TimeDebugController.totalTimeMultiplier;
			skeletonAnimation.state.ClearTracks();
			skeletonAnimation.AnimationName = "win";
			skeletonAnimation.state.SetAnimation(0, "win", skeletonAnimation.loop);
			skeletonAnimation.state.End += OnAnimationEnded;
		}

		public void AnimateHitted()
		{
			_eventsDelegates.Clear();
			_onCompleted = null;
			if (_hittedLock)
			{
				return;
			}
			bool freezeAfter = skeletonAnimation.paused;
			_shouldLaunchIddle = false;
			_hittedLock = true;
			skeletonAnimation.state.ClearEnd();
			skeletonAnimation.loop = false;
			skeletonAnimation.timeScale = TimeDebugController.instance.damageAnimMultiplier * TimeDebugController.totalTimeMultiplier;
			skeletonAnimation.state.ClearTracks();
			skeletonAnimation.AnimationName = "damage";
			skeletonAnimation.state.SetAnimation(0, "damage", skeletonAnimation.loop);
			Spine.AnimationState.StartEndDelegate startEndDelegate = delegate
			{
			};
			startEndDelegate = delegate(Spine.AnimationState state, int trackIndex)
			{
				OnAnimationEnded(state, trackIndex);
				if (freezeAfter)
				{
					FreezeMeshes(freeze: true);
					_shouldFreezeIddle = true;
				}
				skeletonAnimation.state.ClearEnd();
			};
			skeletonAnimation.state.End += startEndDelegate;
		}

		public void PauseAnimation(bool paused = true)
		{
			skeletonAnimation.paused = paused;
		}

		public void AnimateByType(MonsterAnimationType aType)
		{
			switch (aType)
			{
			case MonsterAnimationType.Damaged:
				AnimateHitted();
				break;
			case MonsterAnimationType.Death:
				AnimateDeath();
				break;
			case MonsterAnimationType.Idle:
				AnimateIdleLooped();
				break;
			case MonsterAnimationType.MeleeHit:
				AnimateMeleeHit(new Dictionary<string, Common.VoidDelegate>());
				break;
			case MonsterAnimationType.RangedShot:
				AnimateRangedShoot(new Dictionary<string, Common.VoidDelegate>());
				break;
			case MonsterAnimationType.Victory:
				AnimateVictory();
				break;
			}
		}

		private void HandleEvent(Spine.AnimationState state, int trackIndex, Spine.Event e)
		{
			string key = e.Data.Name;
			if (e.Data.Name == "hit" && _eventsDelegates.ContainsKey("fire"))
			{
				Debug.LogError("ERROR!!! Class or animation wrong!");
				key = "fire";
			}
			if (e.Data.Name == "fire" && _eventsDelegates.ContainsKey("hit"))
			{
				Debug.LogError("ERROR!!! Class or animation wrong!");
				key = "hit";
			}
			if (_eventsDelegates.ContainsKey(key))
			{
				_eventsDelegates[key]();
				_eventsDelegates.Remove(key);
			}
		}

		private void OnAnimationEnded(Spine.AnimationState state, int trackIndex)
		{
			if (_onCompleted != null)
			{
				_onCompleted();
			}
			animatingAttack = false;
			skeletonAnimation.state.ClearEnd();
			_shouldLaunchIddle = true;
			_hittedLock = false;
		}

		private void OnAnimationEndedNoRelaunch(Spine.AnimationState state, int trackIndex)
		{
			if (_onCompleted != null)
			{
				_onCompleted();
			}
			skeletonAnimation.state.ClearEnd();
		}

		public Vector3 GetShootBonePosition()
		{
			try
			{
				boneManager["weapon_point"].Activate(isActivate: true);
				return boneManager["weapon_point"].transform.position;
			}
			catch (NullReferenceException ex)
			{
				StringBuilder stringBuilder = new StringBuilder("I can not find weapon point! \n");
				stringBuilder.Append(ex.StackTrace);
				Debug.LogError(stringBuilder);
				return new Vector3(0f, 0f, 0f);
			}
		}

		public override void SetMaterials(List<Material> mats)
		{
			skeletonAnimation.ApplyForceMaterials(mats);
		}

		public override List<Material> GetMaterials()
		{
			return _animationMaterials;
		}

		protected override void FreezeMeshes(bool freeze)
		{
			PauseAnimation(freeze);
			if (freeze)
			{
				_shouldFreezeIddle = true;
				_shouldLaunchIddle = true;
				return;
			}
			_shouldFreezeIddle = false;
			if (!shouldDie)
			{
				_shouldLaunchIddle = true;
			}
		}

		protected override void ResetMaterials()
		{
			skeletonAnimation.ResetForcedMaterials();
		}

		protected override void SetMeshTimeMult(float timeMult)
		{
			skeletonAnimation.timeMultiplier = timeMult;
		}

		public void ApplyAlpha(float alpha)
		{
			skeletonAnimation.ApplyAlpha(alpha);
			SetShadowAlpha(alpha);
		}

		public void ApplyAlphaTween(float alpha, float time, Ease ease)
		{
			skeletonAnimation.ApplyAlphaTween(alpha, time, ease);
			if (_shadowSprite != null)
			{
				DOTween.To(SetShadowAlpha, _shadowSprite.color.a, alpha, time).SetEase(ease);
			}
		}

		public void ApplyColorTint(Color color)
		{
			skeletonAnimation.ApplyColor(color);
			SetShadowAlpha(color.a);
		}

		public void ApplyColorTint(float value)
		{
			skeletonAnimation.ApplyColor(value);
			SetShadowAlpha(Alpha);
		}

		private void SetShadowAlpha(float value)
		{
			if (!(_shadowSprite == null))
			{
				Color color = _shadowSprite.color;
				color.a = value;
				_shadowSprite.color = color;
			}
		}
	}
}
