using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts
{
	public class WarlordMonsterVisual : FieldMonsterVisual
	{
		public bool deal10Damage;

		public bool heal10;

		protected override float AnimationStartScale => 0.75f;

		public override void Init(MonsterData data, FieldMonster curMonster)
		{
			base.Init(data, curMonster);
			if (data.isImmortal)
			{
				_wrapper.hpContainer.gameObject.SetActive(value: false);
			}
			_wrapper.atkContainer.gameObject.SetActive(value: false);
		}

		public override void Destroy()
		{
			Object.Destroy(base.gameObject);
		}

		protected override void Update()
		{
			base.Update();
			if (deal10Damage)
			{
				deal10Damage = false;
				_curMonster.ChangeParam(null, ParamType.Health, -10);
			}
			if (heal10)
			{
				heal10 = false;
				_curMonster.ChangeParam(null, ParamType.Health, 10);
			}
		}

		public override void ApplyEulerAngles(Vector3 angle)
		{
			base.ApplyEulerAngles(angle);
			float x = (float)((angle.y == 0f) ? 1 : (-1)) * Mathf.Abs(_wrapper.hpContainer.localPosition.x);
			_wrapper.hpContainer.localPosition = new Vector3(x, _wrapper.hpContainer.localPosition.y, _wrapper.hpContainer.localPosition.z);
		}
	}
}
