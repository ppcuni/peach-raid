using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
	const int FieldSize = 6;
	
	[SerializeField]
	private GameObject panels = null;
	[SerializeField]
	private Finger finger = null;
	[SerializeField]
	private GameObject panelPrefab = null;
	[SerializeField]
	private LineRenderer lineRenderer = null;
	
	private List<Panel> selectedPanels = new List<Panel>();
	
	private System.Random Randomizer = new System.Random();
	
	void Awake()
	{
		Application.targetFrameRate = 60;
	}
	
	void Start()
	{
		for(var y = 0; y < FieldSize; ++y)
			for(var x = 0; x < FieldSize; ++x)
		{
			CreatePanel(x, y);
		}
		
		finger.OnHitPanel += OnFingerHitPanel;
		finger.OnRelease += OnFingerRelease;
	}
	
	void OnDestroy()
	{
		finger.OnHitPanel -= OnFingerHitPanel;
		finger.OnRelease += OnFingerRelease;
	}
	
	void CreatePanel(int x, int y)
	{
		var panel = Instantiate(panelPrefab) as GameObject;
		panel.transform.parent = panels.transform;
		var panelType = Randomizer.Next(0, 5);
		panel.GetComponent<Panel>().Initialize(x, y, panelType);
	}
	
	void OnFingerHitPanel(Panel panel)
	{
		var count = selectedPanels.IndexOf(panel) + 1;
		if(count > 0)
		{
			var unselectedPanels = selectedPanels.Skip(count);
			foreach(var x in unselectedPanels)
			{
				x.Select(false);
			}
			selectedPanels = selectedPanels.Take(count).ToList();
		}
		else if(selectedPanels.Count == 0)
		{
			panel.Select(true);
			selectedPanels.Add(panel);
		}
		else if(selectedPanels.Last().IsNeighbourWithSameType(panel))
		{
			panel.Select(true);
			selectedPanels.Add(panel);
		}
		
		var length = selectedPanels.Count;
		if(length >= 2)
		{
			lineRenderer.SetVertexCount(length);
			lineRenderer.SetWidth(2,2);
			for(var i = 0; i < length; ++i)
			{
				var pos = selectedPanels[i].transform.localPosition;
				pos.z = -10;
				lineRenderer.SetPosition(i, pos);
			}
		}
	}
	
	void OnFingerRelease()
	{
		if(selectedPanels.Count < 3)
		{
			foreach(var x in selectedPanels)
			{
				x.Select(false);
			}
		}
		else
		{
			// パネル消す.
			foreach(var x in selectedPanels)
			{
				Destroy(x.gameObject);
			}
			
			var allPanels = panels.GetComponentsInChildren<Panel>();
			var deletedCountsByLine = new List<int>(FieldSize){0,0,0,0,0,0};
			for( var i = 0; i < FieldSize; ++i)
			{
				var deletedPanels = selectedPanels.Where(x => x.Position.x == i);
				deletedCountsByLine[i] = deletedPanels.Count();
				var linePanels = allPanels
					.Where(x => x.Position.x == i && !deletedPanels.Any(y => x == y))
					.Select(x => new {Panel = x, UnderCount = deletedPanels.Count(y => y.Position.y < x.Position.y)});
				
				// 消えてないパネルの位置再設定.
				foreach(var x in linePanels)
				{
					x.Panel.RePosition((int)(x.Panel.Position.y - x.UnderCount));
				}
				
				// 消えた分補充.
				for(var y = (FieldSize - deletedCountsByLine[i]); y < FieldSize; ++y)
				{
					CreatePanel(i, y);
				}
			}
		}
		
		selectedPanels.Clear();
		lineRenderer.SetVertexCount(0);
	}
}
