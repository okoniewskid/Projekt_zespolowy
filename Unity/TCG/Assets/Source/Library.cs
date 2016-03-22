using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Source;
using System;
using System.Threading;




public class Library : MonoBehaviour {

    public static Library lib;

    public List<Carda> deck;

    public Sprite cardback;


    public List<int> LoadDeck(string deckstring)
    {
        List<int> tempdeck = new List<int>();
        string[] temparray = deckstring.Split(","[0]);
        for (int i = 0; i < temparray.Length; i++)
        {
            if (temparray[i].Length > 0) tempdeck.Add(System.Int32.Parse(temparray[i]));
        }
        return tempdeck;
    }

    public void ShuffleDeck(List<int> DeckToShuffle)
    {
        for (int i = 0; i < DeckToShuffle.Count; i++)
        {
            int temp = DeckToShuffle[i];
            int randomIndex = System.Random.Range(i, DeckToShuffle.Count);
            DeckToShuffle[i] = DeckToShuffle[randomIndex];
            DeckToShuffle[randomIndex] = temp;
        }
    }







    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
