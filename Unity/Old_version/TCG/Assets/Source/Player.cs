using Assets.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Player : MonoBehaviour
    {
        public static int life;
        public static int mana;
        public static int turn_count;

        public static bool active_turn = false;
        public static bool has_won = false;
        public static bool has_lost = false;

        public static Hand hand;
        public static Graveyard grave;
        public static Battlefield bf;
        public static Exile exile;
        public static Library deck;

        public static List<Carda> player_hand = new List<Carda>();
        public static List<Carda> player_battlefield = new List<Carda>();
        public static List<Carda> player_graveyard = new List<Carda>();
        public static List<Carda> player_exile = new List<Carda>();
        public static List<Carda> player_deck = new List<Carda>();


        public static List<GameObject> targets = new List<GameObject>();

        public void Awake()
        {
            this.tag = "Player";
        }

        //nowa gra
        public static void StartGame() 
        {
            Debug.Log("starting game - player");

            life = 20;
            mana = 0;
            turn_count = 0;
            player_battlefield.Clear();
            player_hand.Clear();
            player_graveyard.Clear();
            player_exile.Clear();
           
        
            //TODO: initiating a deck

            active_turn = false;
            has_won = false;
            has_lost = false;

        }


        public static void AddToBattlefield(Carda played_card)
        {
            player_battlefield.Add(played_card);
            Battlefield batf = Player.bf;

            batf.AddCard(played_card);
        }

        public static void RemoveFromBattlefield(Carda removed_card)
        {
            player_battlefield.Remove(removed_card);
            Battlefield batf = Player.bf;
            batf.RemoveCard(removed_card);
        }

        public static void AddToHand(Carda add_card)
        {
            player_hand.Add(add_card);
            Hand handy = Player.hand;
            handy.AddCard(add_card);

        }

        public static void RemoveFromHand(Carda removed_card)
        {
            player_hand.Remove(removed_card);
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
    
        public static void NewTurn()
        {

            Player.targets.Clear();
            Player.turn_count++;
            IncreaseMana();

            foreach (Carda foundcard in player_battlefield)
             {

                 foundcard.has_sickness = false;
                 foundcard.can_attack = true;
           
                 if (foundcard.is_tapped)
                { 
                    Debug.Log("untapping");
                    // foundcard.transform.Rotate(0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees);
                    foundcard.is_tapped = false;
                 };
            }

            //TODO: draw
        }

        public void GameWon()
          {
             Enemy.has_lost = true;

            //jakaś reakcja interfejsu na wygraną


             Player.active_turn = false;
         }

        public void GameLost()
         {
              Enemy.has_won = true;
              //jakaś reakcja interfejsu na przegraną
             Player.active_turn = false;
          }


        public void EndTurn()
         {
        //TODO
         }
    

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


    }

