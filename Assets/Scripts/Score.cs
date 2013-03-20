using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour
{
	[SerializeField]
	private Game game = null;
	
	void OnGUI()
	{
		GUI.Label(new Rect(10,0,200,100), "Score:" + game.Score.ToString("00000"));
		GUI.Label(new Rect(200,0,200,100), "Remaining Turn:" + game.RemainingTurn.ToString("00"));
	}
}
