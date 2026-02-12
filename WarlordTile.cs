using System.Collections.Generic;
using NGUI.Scripts.UI;
using NewAssets.Scripts.UtilScripts;

namespace BattlefieldScripts
{
	public class WarlordTile : MonoBehaviourExt
	{
		public List<UISprite> column1;

		public List<UISprite> column2;

		public List<UISprite> column3;

		public void SetDepth(int depth)
		{
			for (int i = 0; i < column1.Count; i++)
			{
				column1[i].depth = depth - 5 + i;
			}
			for (int j = 0; j < column2.Count; j++)
			{
				column2[j].depth = depth + j;
			}
			for (int k = 0; k < column3.Count; k++)
			{
				column3[k].depth = depth + 5 + k;
			}
		}
	}
}
