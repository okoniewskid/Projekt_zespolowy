using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Source;

public class Exile : MonoBehaviour {

    public int exile_id;
    public List<Carda> cards_in_exile;
    private Player exile_owner;





    public void AddCard(Carda added_card)
    {
        cards_in_exile.Add(added_card);
    }

    public void RemoveCard(Carda added_card)
    {
        cards_in_exile.Remove(added_card);
    }


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
