using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class NoGenderFilter : BitStaticFilter
	{
		private Gender _gender;

		private bool _ignoreImmune;

		public NoGenderFilter(Gender gender, BitStaticFilter prevFilter = null, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_gender = gender;
			_ignoreImmune = ignoreImmune;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune))
			{
				return mon.data.staticInnerData.gender != _gender;
			}
			return false;
		}
	}
}
