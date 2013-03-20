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
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		if(Input.touchCount > 0)
		{
			var touch = Input.GetTouch(0);
			if(touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
			{
				pos = camera.ScreenToWorldPoint(touch.position);
				pos.z = 0;
				isInput = true;
			}
		}
		if(Input.touchCount == 0)
		{
			if(OnRelease != null)
				OnRelease();
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
