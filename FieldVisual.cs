using NGUI.Scripts.UI;
using NewAssets.Scripts.UtilScripts;
using UnityEngine;

namespace BattlefieldScripts
{
	public class FieldVisual : MonoBehaviourExt
	{
		private UIPanel _panel;

		protected bool _isForwarded;

		public UIPanel panel
		{
			get
			{
				if (_panel == null)
				{
					_panel = GetComponent<UIPanel>();
				}
				return _panel;
			}
		}

		public virtual void Destroy()
		{
			if (base.gameObject != null)
			{
				Object.Destroy(base.gameObject);
			}
		}

		public void SetForward(bool v)
		{
			if (_isForwarded != v)
			{
				if (base.transform != null)
				{
					base.transform.localPosition += new Vector3(0f, 0f, v ? (-0.5f) : 0.5f);
				}
				_isForwarded = v;
			}
		}

		public void SetPosition(Vector3 position)
		{
			base.transform.position = new Vector3(position.x, position.y, 0f);
		}

		public void ConvertZtoDepth(float depth)
		{
			panel.depthDelta = (int)(0f - depth);
		}

		public virtual bool Collided(Vector3 position)
		{
			return false;
		}
	}
}
