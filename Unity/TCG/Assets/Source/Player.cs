using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Source
{
    class Player : MonoBehaviour
    {
        public static int starting_life = 20;
        public static int starting_mana = 0;

        public static bool active_turn = false;
        public static bool has_won = false;
        public static bool has_lost = false;

        public static int current_mana = 0;

        public static Battlefield player_bf;
        public static Graveyard player_grave;
        public static Hand player_hand;
        public static Exile player_exile;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


    }
}
