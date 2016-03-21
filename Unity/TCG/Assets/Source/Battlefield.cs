using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Source
{
    public class Battlefield : MonoBehaviour
    {
        public int bf_id;
        public List<Card> cards_on_bf;
        public Player bf_owner;


        void Start()
        {

        }

        void Update()
        {

        }

        public void AddCard(Card added_card)
        {
            cards_on_bf.Add(added_card);
        }


    }

}
