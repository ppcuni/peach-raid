using UnityEngine;
using System.Collections;

public class GameOver : ModalPanel
{
	private Game game = null;
	
	void Start()
	{
		game = GameObject.Find("Game").GetComponent<Game>();
	}
	
	protected override void OnTap()
	{
		Application.LoadLevel("Main");
	}
	
	void OnGUI()
	{
		GUI.color = Color.black;
		GUI.Label(new Rect(120,150,300,200), "Game Over\n Your Score is\n\n      " + game.Score.ToString("00000") + "\n\n\n Tap to Continue");
	}
}
