using UnityEngine;
using System.Collections;
using System;

public class Finger : MonoBehaviour
{
	[SerializeField]
	private new Camera camera = null;
	
	void Update()
	{
		Vector3 pos = new Vector3(-100,-100,0);
		var isInput = false;
#if UNITY_IPHONE || UNITY_ANDROID
		if(Input.touchCount > 0)
		{
			pos = camera.ScreenToWorldPoint(Input.GetTouch(0).position);
			pos.z = 0;
			isInput = true;
		}
#else
		if(Input.GetMouseButton(0))
		{
			pos = camera.ScreenToWorldPoint(Input.mousePosition);
			pos.z = 0;
			isInput = true;
		}
		if(Input.GetMouseButtonUp(0))
		{
			pos = new Vector3(-100,-100,0);
			isInput = true;
			if(OnRelease != null)
				OnRelease();
		}
#endif
		if(isInput)
		{
			this.transform.localPosition = pos;
		}
	}
	
	public event Action OnRelease;
	public event Action<Panel> OnHitPanel;
	void OnTriggerEnter (Collider other)
	{
		var panel = other.gameObject.GetComponent<Panel>();
		if(OnHitPanel != null && panel != null)
		{
			OnHitPanel(panel);
		}
	}
}
