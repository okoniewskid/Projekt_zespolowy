using UnityEngine;
using System.Collections;

public class VictoryDefeat : MonoBehaviour {
	Sprite victoryordefeat;
	public int VictoryCurrency = 50;
	public int DefeatCurrency = 20;

	void Start () {
		this.tag = "VictoryDefeat";
	}
	void EndOfGame () {
		if (Player.Lost) {
			Currency.DoAssignCurrency(Currency.PlayerCurrency+DefeatCurrency); 
			victoryordefeat = playerDeck.pD.defeat;
			GetComponent<SpriteRenderer> ().sprite = victoryordefeat;
		}
		else if (Enemy.Lost)
		{
			Currency.DoAssignCurrency(Currency.PlayerCurrency+VictoryCurrency); 
			victoryordefeat = playerDeck.pD.victory;
			GetComponent<SpriteRenderer> ().sprite = victoryordefeat;
		}
		GetComponent<Renderer>().sortingOrder = 100;
	}
	void OnGUI()
	{
		Rect windowRect = new Rect(400,300,300,90);
		if (Player.Life <= 0)  windowRect = GUI.Window(0, windowRect, DoMyWindow, "Otrzymałeś " + DefeatCurrency +  " srebra!"); 
		if (Enemy.Life <= 0)  windowRect = GUI.Window(0, windowRect, DoMyWindow, "Otrzymałeś " + VictoryCurrency +  " srebra!"); 	
		
	}
	void DoMyWindow(int windowID) {
		if (GUILayout.Button("Wróć do menu."))  Application.LoadLevel(MainMenu.SceneNameMainMenu);
	}
}