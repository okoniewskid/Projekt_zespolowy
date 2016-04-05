using UnityEngine;
using System.Collections;

//Skrypt wyswietlajacy odpowiednie powiadomienie o wygranej badz przegranej

public class VictoryDefeat : MonoBehaviour
{
    Sprite victoryordefeat;


    void Start()
    {
        this.tag = "VictoryDefeat";
    }

    void EndOfGame()
    {
        if (Player.Lost)
        {

            victoryordefeat = playerDeck.pD.defeat;
            GetComponent<SpriteRenderer>().sprite = victoryordefeat;
        }
        else if (Enemy.Lost)
        {

            victoryordefeat = playerDeck.pD.victory;
            GetComponent<SpriteRenderer>().sprite = victoryordefeat;
        }
        GetComponent<Renderer>().sortingOrder = 100;

    }
}