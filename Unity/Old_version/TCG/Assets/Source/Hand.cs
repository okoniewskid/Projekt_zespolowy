using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Source;

public class Hand : MonoBehaviour {


    public int hand_id;
    public List<Carda> cards_in_hand;
    

    public void AddCard(Carda added_card)
    {
        cards_in_hand.Add(added_card);
    }

    public void RemoveCard(Carda removed_card)
    {
        cards_in_hand.Remove(removed_card);
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
