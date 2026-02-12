using System;
using System.Collections.Generic;
using NewAssets.Scripts.UtilScripts;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class GroundAppearAnimation : BitActionAnimation
	{
		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item in monstersAction)
			{
				FieldMonsterVisual monster = item.Value as FieldMonsterVisual;
				if (monster != null)
				{
					monster.SetParamsVisible(visible: false);
					Debug.LogWarning("GroundAppearAnimation isn't supported!");
					Common.VoidDelegate procedure = delegate
					{
						monster.SetParamsVisible(visible: true);
					};
					Initializer.WaitForProcedure(0.4f, procedure);
				}
				item.Key();
			}
			Initializer.WaitForProcedure(0.4f, delegate
			{
				onEnded();
			});
		}
	}
}
