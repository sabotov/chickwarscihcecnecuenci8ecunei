using NewAssets.Scripts.DataClasses;

namespace BattlefieldScripts
{
	internal class PetMonsterVisual : FieldMonsterVisual
	{
		public override void Init(MonsterData data, FieldMonster curMonster)
		{
			base.Init(data, curMonster);
			if (data.isImmortal)
			{
				_wrapper.hpContainer.gameObject.SetActive(value: false);
			}
			_wrapper.atkContainer.gameObject.SetActive(value: false);
		}
	}
}
