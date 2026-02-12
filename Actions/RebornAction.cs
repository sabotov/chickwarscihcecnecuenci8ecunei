using System;
using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.Data_Helpers;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class RebornAction : BitAction
	{
		public override bool ShouldCheckIfFilterNotEmpty => false;

		public RebornAction(BitActionAnimation animation)
			: base(animation)
		{
		}

		public override void PerformAction(IEnumerable<KeyValuePair<Vector2, FieldMonster>> monsters, Action<bool, FieldElement> onCompleted)
		{
			List<Action> list = new List<Action>();
			FieldMonster thisMon = _myMonster as FieldMonster;
			if (thisMon == null || !thisMon.ShouldDie)
			{
				onCompleted(arg1: false, null);
				return;
			}
			string skillId;
			int rebornCount = thisMon.GetRebornCount(out skillId);
			if (rebornCount <= 0)
			{
				onCompleted(arg1: false, null);
				return;
			}
			rebornCount--;
			Action item = delegate
			{
				_animation.Animate(new Dictionary<Common.StringDelegate, FieldVisual> { 
				{
					delegate
					{
						_myController.PlaceMonster(thisMon.data, thisMon.coords, delegate
						{
							onCompleted(arg1: false, null);
						}, null, delegate(FieldMonster fMon)
						{
							fMon.Silence(SkillType.Reborn);
							if (rebornCount > 0)
							{
								SkillStaticData skillByName = SkillDataHelper.GetSkillByName(skillId);
								if (skillByName != null)
								{
									fMon.AddSkill(skillByName, rebornCount.ToString());
								}
							}
						}, fromReborn: true);
						return "";
					},
					null
				} }, delegate
				{
				});
			};
			list.Add(item);
			if (list.Count > 0)
			{
				list[0]();
			}
			else
			{
				onCompleted(arg1: false, null);
			}
		}
	}
}
