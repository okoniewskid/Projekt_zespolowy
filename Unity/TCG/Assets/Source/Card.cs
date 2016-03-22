using UnityEngine;
using System.Collections;


    public class Carda : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;
    
        public Sprite[] faces;
        public Sprite cardBack;
    
        //wartosci bazowe
        public int card_id;
        public string card_name;
        public int mana_cost;
        public string card_type;
        public string card_subtype;
        public int card_base_power;
        public int card_base_defense;


        //wartosci modyfikowalne
        public int controller_id;
        public int card_power;
        public int card_defense;
        public int damage_dealt_by_card;
        public int damage_dealt_to_card;


        //boole
        public bool is_tapped = false;
        public bool can_attack = false;
        public bool has_sickness = true;
        public bool can_block = true;
        public bool is_dead = false;
        public bool attacked_this_turn = false;
        public bool is_targeted = false;












    }


