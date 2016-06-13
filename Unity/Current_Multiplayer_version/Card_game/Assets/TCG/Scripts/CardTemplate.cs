using UnityEngine;
using System.Collections;
public class CardTemplate : MonoBehaviourSingleton<CardTemplate>{

	void Start () {
		if (GetComponent<BoxCollider2D>())
		{
			MainMenu.ColliderWidth = GetComponent<BoxCollider2D>().size.x;
			MainMenu.ColliderHeight = GetComponent<BoxCollider2D>().size.y;
		}
		else if (GetComponent<BoxCollider>()){
			MainMenu.ColliderWidth = GetComponent<BoxCollider>().size.x;
			MainMenu.ColliderHeight = GetComponent<BoxCollider>().size.y;
		}

		gameObject.SetActive(false);
		
		card.ZoomHeight = MainMenu.ColliderHeight * 2.75f;
	}
	
	void Update () {
	
	}
}
