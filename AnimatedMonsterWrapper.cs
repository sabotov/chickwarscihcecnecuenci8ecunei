using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts
{
	public class AnimatedMonsterWrapper : MonsterImageBase
	{
		public AnimatedMonster animation;

		public override void Init(MonsterData data)
		{
			animation.Init(data, null, shouldPlaceDefault: true);
		}

		public override void applyAlpha(float alpha)
		{
			animation.ApplyAlpha(alpha);
		}

		public override void applyColorTint(Color color)
		{
			animation.ApplyColorTint(color);
		}
	}
}
