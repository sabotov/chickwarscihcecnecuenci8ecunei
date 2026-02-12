using System.Collections.Generic;
using BattlefieldScripts.Actions;

namespace BattlefieldScripts
{
	public class SilenceData
	{
		public List<ActionBitSignature> silencedSkills;

		public SilenceData Clone()
		{
			return new SilenceData
			{
				silencedSkills = silencedSkills.ConvertAll((ActionBitSignature x) => x.Clone())
			};
		}
	}
}
