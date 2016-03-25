using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Source;

public class Enemy : MonoBehaviour {

    public static int life;
    public static int mana;
    public static int turn_count;
    public static string EnemyName;

    public static bool active_turn = false;
        public static bool has_won = false;
        public static bool has_lost = false;

        public static Battlefield bf;
        public static Graveyard grave;
        public static Hand hand;
        public static Exile exile;
        public static Library deck;

        public static List<Carda> enemy_deck = new List<Carda>();
        public static List<Carda> enemy_hand = new List<Carda>();
        public static List<Carda> enemy_battlefield = new List<Carda>();
        public static List<Carda> enemy_graveyard = new List<Carda>();
        public static List<Carda> enemy_exile = new List<Carda>();

    public static List<GameObject> targets = new List<GameObject>();

    public static int number_of_cards_in_hand;
    public static int number_of_cards_in_library;



    public void Awake()
    {
        this.tag = "Enemy";
    }


    public static void StartGame()
    {
        Debug.Log("starting game - enemy");

        life = 20;
        mana = 0;
        turn_count = 0;
        enemy_battlefield.Clear();
        enemy_hand.Clear();
        enemy_graveyard.Clear();
        enemy_exile.Clear();

        //TODO: initiating a deck

        active_turn = false;
        has_won = false;
        has_lost = false;

    }

    public static void AddToBattlefield(Carda played_card)
    {
        enemy_battlefield.Add(played_card);
        Battlefield batf = Player.bf;

        batf.AddCard(played_card);
    }

    public static void RemoveFromBattlefield(Carda removed_card)
    {
        enemy_battlefield.Remove(removed_card);
        Battlefield batf = Player.bf;
        batf.RemoveCard(removed_card);
    }

    public static void AddToHand(Carda add_card)
    {
        enemy_hand.Add(add_card);
        Hand handy = Player.hand;
        handy.AddCard(add_card);

    }

    public static void RemoveFromHand(Carda removed_card)
    {
        enemy_hand.Remove(removed_card);
        Hand handy = Player.hand;
        handy.RemoveCard(removed_card);

    }

    public static void IncreaseMana()
    {
        if (mana < 10)
        {
            mana++;
        }
    }


    // Use this for initialization
    void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }

