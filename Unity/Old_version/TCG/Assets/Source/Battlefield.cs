using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Source
{
    public class Battlefield : MonoBehaviour
    {
        public int bf_id;
        public List<Carda> cards_on_bf;
        
        void Start()
        {

        }

        void Update()
        {

        }


        public void AddCard(Carda added_card)
        {
            cards_on_bf.Add(added_card);
        }

        public void RemoveCard(Carda removed_card)
        {
            cards_on_bf.Remove(removed_card);
        }





    }

}
