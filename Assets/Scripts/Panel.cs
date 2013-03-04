using UnityEngine;
using System.Collections;

public class Panel : MonoBehaviour
{
	public int Type { get; private set; }
	public Vector2 Position { get; private set; }
	private readonly int Margin = 22;
	
	[SerializeField]
	private Texture[] TypeTextures = null;
	private readonly int[] TypeScores = new int[]
	{
		10,10,10,10,10,200
	};
	
	public void Initialize(int x, int y, int type)
	{
		Type = type;
		this.renderer.material.mainTexture = TypeTextures[Type];
		Position = new Vector2(x, y);
		var pos = new Vector3(x * Margin, y * Margin, 0);
		this.transform.localPosition = pos;
		var fromPos = this.transform.localPosition;
		fromPos.y += Margin*3;
		iTween.MoveFrom(this.gameObject, fromPos, 0.8f);
	}
	
	public void RePosition(int y)
	{
		var pos = this.transform.localPosition;
		pos.y = y * Margin;
		iTween.MoveTo(this.gameObject, pos, 0.8f);
		Position = new Vector2(Position.x, y);
	}
	
	static readonly Vector3 Selected = new Vector3(16, 16, 16);
	static readonly Vector3 Normal = new Vector3(20, 20, 20);
	
	public void Select(bool isSelect)
	{
		if(isSelect)
		{
			this.transform.localScale = Selected;
		}
		else
		{
			this.transform.localScale = Normal;
		}
	}
	
	public void Grayout(bool isGrayout)
	{
		if(isGrayout)
			this.renderer.material.color = Color.gray;
		else
			this.renderer.material.color = Color.white;
	}
	
	public bool IsNeighbourWithSameType(Panel other)
	{
		var distance = Vector2.Distance(this.Position, other.Position);
		return distance < 1.5f && this.Type == other.Type;
	}
	
	public int Score { get { return TypeScores[Type]; } }
}
