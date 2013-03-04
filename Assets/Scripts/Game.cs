using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
	const int FieldSize = 6;
	const int MaxTurn = 32;
	
	[SerializeField]
	private GameObject panels = null;
	[SerializeField]
	private Finger finger = null;
	[SerializeField]
	private GameObject panelPrefab = null;
	[SerializeField]
	private GameObject gameoverPrefab = null;
	[SerializeField]
	private LineRenderer lineRenderer = null;
	
	private List<Panel> selectedPanels = new List<Panel>();
	
	private System.Random Randomizer = new System.Random();
	
	public int Score { get; set; }
	public int RemainingTurn { get; set; }
	
	void Awake()
	{
		Application.targetFrameRate = 60;
	}
	
	void Start()
	{
		Score = 0;
		RemainingTurn = MaxTurn;
		
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
		CreatePanel(x, y, false);
	}
	
	void CreatePanel(int x, int y, bool isSpecial)
	{
		var panel = Instantiate(panelPrefab) as GameObject;
		panel.transform.parent = panels.transform;
		var panelType = isSpecial ? 5 : Randomizer.Next(0, 5);
		panel.GetComponent<Panel>().Initialize(x, y, panelType);
	}
	
	void GrayoutPanels(int exclutionType)
	{
		foreach(var x in panels.GetComponentsInChildren<Panel>())
		{
			x.Grayout(x.Type != exclutionType);
		}
	}
	
	void HighlightPanels()
	{
		foreach(var x in panels.GetComponentsInChildren<Panel>())
		{
			x.Grayout(false);
		}
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
			// つながらないパネルの色を暗くする.
			GrayoutPanels(panel.Type);
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
		else
		{
			lineRenderer.SetVertexCount(0);
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
			int? specialPanel = null;
			if(selectedPanels.Count >= 5)
			{
				specialPanel = Randomizer.Next(0, selectedPanels.Count - 1);
			}
			
			// パネル消す.
			for(var i = 0; i < selectedPanels.Count; ++i)
			{
				var x = selectedPanels[i];
				Score += x.Score * (10 + i) / 10 ;
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
					var isSpecial = specialPanel.HasValue && specialPanel == 0;
					CreatePanel(i, y, isSpecial);
					if(specialPanel.HasValue) specialPanel--;
				}
			}
			
			RemainingTurn--;
			if(RemainingTurn <= 0)
			{
				var gameOver = Instantiate(gameoverPrefab);
			}
		}
		
		selectedPanels.Clear();
		lineRenderer.SetVertexCount(0);
		// 全部のパネルの色を明るくする.
		HighlightPanels();
	}
}
