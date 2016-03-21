using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Source
{
    public class Enemy : MonoBehaviour {

        public static int starting_life = 20;
        public static int starting_mana = 0;

        public static bool active_turn = false;
        public static bool has_won = false;
        public static bool has_lost = false;

        public static Battlefield enemy_bf;
        public static Graveyard enemy_grave;
        public static Hand enemy_hand;
        public static Exile enemy_exile;

        public static List<Card> enemy_library;

        public static int number_of_cards_in_hand = 0;
        public static int number_of_cards_in_library = 0;
        public static int current_mana = 0;



        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }
}
