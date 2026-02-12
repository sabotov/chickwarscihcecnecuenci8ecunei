using NewAssets.Scripts.UtilScripts;
using UnityEngine;

namespace BattlefieldScripts
{
	public class BubbleSelectElementContainer : MonoBehaviourExt
	{
		private static GameObject _pref;

		public BubbleSelectElement bubble1;

		public BubbleSelectElement bubble2;

		public BubbleSelectElement bubble3;

		public BubbleSelectElement bubble4;

		private static GameObject pref
		{
			get
			{
				if (_pref == null)
				{
					_pref = Resources.Load<GameObject>("Prefabs/BattlePrefabs/SpeechSelectBubbleContainer");
				}
				return _pref;
			}
		}

		public static BubbleSelectElementContainer CreateBubble()
		{
			return Object.Instantiate(pref).GetComponent<BubbleSelectElementContainer>();
		}
	}
}
