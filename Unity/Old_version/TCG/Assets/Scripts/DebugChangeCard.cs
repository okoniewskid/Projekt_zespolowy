using UnityEngine;
using System.Collections;

public class DebugChangeCard : MonoBehaviour
{
    Card cardModel;
    int cardIndex = 0;

    public GameObject card;
    
    void Awake()
    {
        cardModel = card.GetComponent<Card>();
    }

    void OnGUI()
    {
        if(GUI.Button(new Rect(10, 10, 100, 28), "Draw a card"))
        {
            cardModel.cardIndex = cardIndex;
            cardModel.ToggleFace(true);

            cardIndex++;

            if(cardIndex == 52)
            {
                cardIndex = 0;
                cardModel.ToggleFace(false);
            }
        }
    }
}
