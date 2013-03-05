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
	
	Panel[] GetAllPanels()
	{
		return panels.GetComponentsInChildren<Panel>();
	}
	
	void CreatePanel(int x, int y)
	{
		CreatePanel(x, y, null);
	}
	
	void CreatePanel(int x, int y, int? specialPanelType)
	{
		var panel = Instantiate(panelPrefab) as GameObject;
		panel.transform.parent = panels.transform;
		var panelType = specialPanelType ?? Randomizer.Next(0, 5);
		panel.GetComponent<Panel>().Initialize(x, y, panelType);
	}
	
	void GrayoutPanels(int exclutionType)
	{
		foreach(var x in GetAllPanels())
		{
			x.Grayout(x.Type != exclutionType);
		}
	}
	
	void GrayoutPanels(IEnumerable<int> exclutionTypes)
	{
		foreach(var x in GetAllPanels())
		{
			x.Grayout(!exclutionTypes.Any(y => x.Type == y));
		}
	}
	
	void HighlightPanels()
	{
		foreach(var x in GetAllPanels())
		{
			x.Grayout(false);
		}
	}
	
	void OnFingerHitPanel(Panel panel)
	{
		var count = selectedPanels.IndexOf(panel) + 1;
		if(count > 0)
		{
			// 選択してたパネルの上を通ったら、そこまでのルートをリセット.
			var unselectedPanels = selectedPanels.Skip(count);
			foreach(var x in unselectedPanels)
			{
				x.Select(false);
			}
			selectedPanels = selectedPanels.Take(count).ToList();
		}
		else if(selectedPanels.Count == 0 && panel.Type == Panel.Bomb)
		{
			var type = Randomizer.Next(0, 5);
			selectedPanels.Add(panel);
			selectedPanels.AddRange(GetAllPanels().Where(x => x.Type == type));
			GrayoutPanels(new int[]{panel.Type, type});
			return;
		}
		else if(selectedPanels.Count == 0)
		{
			// 最初に選択したのパネル.
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
		
		DrawLine();
	}
	
	void DrawLine()
	{
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
		if(selectedPanels.Count >= 1 && selectedPanels[0].Type == Panel.Bomb)
		{
			// ボム消したらランダムに1種類のきのこ全部消える.
			RemoveAndFillPanels();
			return;
		}
		if(selectedPanels.Count < 3)
		{
			// ボム以外は選択しているパネルが3個以下なら消えない.
			UnSelect();
			return;
		}
		else
		{
			// 3個以上選してると消える.
			RemoveAndFillPanels();
			
			// 指定ターン終わったらゲームオーバー.
			RemainingTurn--;
			if(RemainingTurn <= 0)
			{
				var gameOver = Instantiate(gameoverPrefab);
			}
		}
	}
	
	void UnSelect()
	{
		foreach(var x in selectedPanels)
		{
			x.Select(false);
		}
		ClearSelectedPanels();
	}
	
	void ClearSelectedPanels()
	{
		selectedPanels.Clear();
		lineRenderer.SetVertexCount(0);
		// 全部のパネルの色を明るくする.
		HighlightPanels();
	}
	
	void RemoveAndFillPanels()
	{
		int? specialPanel = null;
		int specialPanelType = selectedPanels[0].Type != Panel.Peach ? Panel.Peach : Panel.Bomb;
		var comboCount = specialPanelType == Panel.Peach ? 5 : 3;
		if(selectedPanels.Count >= comboCount)
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
		
		var deletedCountsByLine = new List<int>(FieldSize){0,0,0,0,0,0};
		for( var i = 0; i < FieldSize; ++i)
		{
			var deletedPanels = selectedPanels.Where(x => x.Position.x == i);
			deletedCountsByLine[i] = deletedPanels.Count();
			var linePanels = GetAllPanels()
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
				var specialType = specialPanel.HasValue && specialPanel == 0 ? (int?)specialPanelType : null;
				CreatePanel(i, y, specialType);
				if(specialPanel.HasValue) specialPanel--;
			}
		}
		ClearSelectedPanels();
	}
}
