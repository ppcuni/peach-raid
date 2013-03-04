using UnityEngine;
using System.Collections;

public abstract class ModalPanel : MonoBehaviour
{
	void Update()
	{
#if UNITY_IPHONE || UNITY_ANDROID
		if(Input.touchCount > 0)
		{
			OnTap();
		}
#else
		if(Input.GetMouseButton(0))
		{
			OnTap();
		}
#endif
	}
	
	protected abstract void OnTap();
}

public class Title : ModalPanel
{
	protected override void OnTap()
	{
		Destroy(this.gameObject);
	}
	
	void OnGUI()
	{
		GUI.color = Color.black;
		GUI.Label(new Rect(120,150,300,200), "Tap to Start\n\n Tap and Draw\nSame Kinokos");
	}
}
