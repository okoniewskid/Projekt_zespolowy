using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Source;

public class Graveyard : MonoBehaviour {

    public int grave_id;
    public List<Carda> cards_in_grave;
    

    public void AddCard(Carda added_card)
    {
        cards_in_grave.Add(added_card);
    }

    public void RemoveCard(Carda removed_card)
    {
        cards_in_grave.Remove(removed_card);
    }




    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
