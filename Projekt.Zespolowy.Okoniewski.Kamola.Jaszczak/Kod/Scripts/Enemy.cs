﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Skrypt obslugujacy przeciwnika, w duzej mierze analogiczny do skryptu Player


public class Enemy : MonoBehaviour
{

    public AudioClip Hit;
    public AudioClip HitBySpell;
    public AudioClip Healed;
    public AudioClip TakesCard;
    public static int Life;

    public static Zone HandZone;
    public static Zone LandsZone;
    public static Zone CreaturesZone;
    public static Zone GraveyardZone;
    public static Zone EnchantmentsZone;
    public static Zone UpgradableCreaturesZone;

    public static List<ManaColor> mana = new List<ManaColor>();

    public static bool EnemyTurn = false;
    public static bool Lost;
    public static int NeedTarget = 0;
    public static int CurrentTargetParam = 0;
    public static int Lands = 0;

    public static float TurnPopUpTimer = 0;
    public static float TurnPopUpTimerMax = 1.5f;

    public static List<int> Deck;
    public static int NumberOfCardsInDeck;

    public static int PotentialMana = 0;
    public static int CardsInHand = 0;


    public static string EnemyName;


    public Transform healfx;
    public Transform firefx;
    public Transform manafx;


    public static List<GameObject> targets = new List<GameObject>();

    public static List<card> lands_in_game = new List<card>();
    public static List<card> cards_in_game = new List<card>();
    public static List<card> cards_in_graveyard = new List<card>();
    public static List<card> cards_in_hand = new List<card>();
    public static List<card> enemy_creatures = new List<card>();
    public static List<card> enchantments = new List<card>();

    public Slot hero_slot;
    public static bool HeroIsDead = false;

    public static int AlliesDestroyedThisTurn;

    public void Awake()
    {
        this.tag = "Enemy";
    }

    public static void StartGame()
    {
        AlliesDestroyedThisTurn = 0;
        HeroIsDead = false;
        
        Life = 20;
        mana.Clear();

        Lands = 0;
        Lost = false;
        NumberOfCardsInDeck = 0;
        PotentialMana = 0;
        CardsInHand = 0;
        EnemyName = "Enemy";

        lands_in_game.Clear();
        lands_in_game = Enemy.LandsZone.cards;
        cards_in_game.Clear();
        cards_in_graveyard.Clear();
        cards_in_graveyard = Enemy.GraveyardZone.cards;
        cards_in_hand.Clear();
        cards_in_hand = Enemy.HandZone.cards;
        enemy_creatures.Clear();
        targets.Clear();

    }


    public static void AddLand(card card_to_add)
    {
        cards_in_game.Add(card_to_add);
        Zone lands = Enemy.LandsZone;

        lands.AddCard(card_to_add);
        card_to_add.GetComponent<AudioSource>().PlayOneShot(card_to_add.sfxEntry);
    }

    public static void AddCreature(card card_to_add)
    {
        cards_in_game.Add(card_to_add);
        enemy_creatures.Add(card_to_add);

        Zone creaturezone = Enemy.CreaturesZone;

        creaturezone.AddCard(card_to_add);

        card_to_add.FirstTurnInGame = true;

        card_to_add.GetComponent<AudioSource>().PlayOneShot(card_to_add.sfxEntry);

        Player.CreatureStatsNeedUpdating = true;
    }

    public static void AddEnchantment(card card_to_add)
    {
        cards_in_game.Add(card_to_add);
        enchantments.Add(card_to_add);

        Zone creaturezone = Enemy.CreaturesZone;

        creaturezone.AddCard(card_to_add);
        card_to_add.GetComponent<AudioSource>().PlayOneShot(card_to_add.sfxEntry);
    }

    public static void RemoveEnchantment(card card_to_remove)
    {
        cards_in_game.Remove(card_to_remove);
        enchantments.Remove(card_to_remove);

        Zone creaturezone = Enemy.CreaturesZone;

        creaturezone.RemoveCard(card_to_remove);

    }

    public static void RemoveCreature(card card_to_remove)
    {
        cards_in_game.Remove(card_to_remove);
        enemy_creatures.Remove(card_to_remove);

        Zone creaturezone = Enemy.CreaturesZone;

        creaturezone.RemoveCard(card_to_remove);
    }

    public static void RemoveHandCard(card card_to_remove)
    {
        Zone handzone = Enemy.HandZone;
        handzone.RemoveCard(card_to_remove);
        CardsInHand -= 1;

    }

    public static void AddHandCard(card card_to_add)
    {
        Zone handzone = Enemy.HandZone;
        handzone.AddCard(card_to_add);

        CardsInHand += 1;

        GameObject.FindWithTag("Enemy").SendMessage("TakesCardSFX");

    }



    void Start()
    {

    }


    void Update()
    {
        TurnPopUpTimer += Time.deltaTime;
    }

    void OpponentTurnPopup(int windowID)
    {
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(50, 10, 152, 25), "Opponent's Turn");

        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        GUI.skin.label.fontStyle = FontStyle.Normal;
        GUI.Label(new Rect(16, 36, 220, 50), "The enemy is now taking its turn.");
    }

    public static void TriggerCardAbilities(int ability_trigger, int subtype = -1)
    {
        Player.TriggerInProcess = true;
        foreach (card foundcard in cards_in_game)
        {
            if (foundcard.GetComponent<abilities>() != null) foundcard.abilities.TriggerAbility(ability_trigger, true, subtype);
        }
        Player.TriggerInProcess = false;
    }

    public static void ChooseTargetForUpgrade(card upgrade)
    {
        targets.Clear();
        foreach (card foundcard in enemy_creatures)
            if (foundcard.Level == (upgrade.Level - 1) && foundcard.GrowID == upgrade.GrowID)
            {
                targets.Add(foundcard.gameObject);
                break;
            }

    }

    public static void ChooseTarget(int Effect)
    {
       
        targets.Clear();
        bool NegativeEffect = true;
        if (Effect == 11 || Effect == 0) NegativeEffect = false;

        List<card> creatures_to_search = new List<card>();

        if (NegativeEffect)
        {
            foreach (card foundcard in Player.player_creatures)
                if (!foundcard.Hero) creatures_to_search.Add(foundcard);
        }
        else {
            foreach (card foundcard in Enemy.enemy_creatures)
                if (!foundcard.Hero) creatures_to_search.Add(foundcard);
        }


        if (NeedTarget == 2) targets.Add(GameObject.FindWithTag("Player"));

        else if (NeedTarget == 9 || NeedTarget == 40 || (NeedTarget == 5 && !NegativeEffect)) targets.Add(EffectManager.HighestAttackCreature(enemy_creatures));

        else if (NeedTarget == 30)
        {

            foreach (card foundcreature in creatures_to_search)
                if (foundcreature.CreatureOffense <= CurrentTargetParam) { targets.Add(foundcreature.gameObject); break; }
        }
        else if (NeedTarget == 31)
        {

            foreach (card foundcreature in creatures_to_search)
                if (foundcreature.Cost.Count <= CurrentTargetParam) { targets.Add(foundcreature.gameObject); break; }
        }

        else if (NeedTarget == 6)
        {

            List<card> TurnedPlayercreatures = EffectManager.TurnedCreatures(creatures_to_search);

            targets.Add(EffectManager.HighestAttackCreature(TurnedPlayercreatures).gameObject);
        }

        else if (NeedTarget == 3)
        {
            int cardid = 0;

            if (Effect == 4) cardid = EffectManager.RandomCardIdFromIntList(Deck, 0);
            else if (Effect == 13) cardid = EffectManager.RandomCardIdFromIntList(Deck, 1);
            else cardid = Deck[Random.Range(0, Deck.Count)];

            targets.Add(playerDeck.pD.MakeCard(cardid, true).gameObject);
            Deck.Remove(cardid);
        }

        else if (NeedTarget == 21)
            Enemy.targets.Add(EffectManager.RandomCard(cards_in_hand));

        else if (NeedTarget == 50 || NeedTarget == 51)

            foreach (card graveyardcard in Enemy.cards_in_graveyard)
            {
                if (graveyardcard.Type == 1 && NeedTarget == 51) { Enemy.targets.Add(graveyardcard.gameObject); break; }
                if (graveyardcard.Type == 2 && NeedTarget == 50) { Enemy.targets.Add(graveyardcard.gameObject); break; }
            }

        else if (NeedTarget == 41) targets.Add(EffectManager.HighestAttackCreature(Player.player_creatures));



        else if (Effect == 6 && NeedTarget == 4)
        {
           
            if (Player.player_creatures.Count > 1)
            {

                GameObject ourtarget = Player.player_creatures[Random.Range(0, Player.player_creatures.Count)].gameObject;
                GameObject secondtarget = Player.player_creatures[Random.Range(0, Player.player_creatures.Count)].gameObject;

                while (secondtarget == ourtarget)
                {
                    secondtarget = Player.player_creatures[Random.Range(0, Player.player_creatures.Count)].gameObject;
                }
                targets.Add(ourtarget);
                targets.Add(secondtarget);
            }
            else if (Player.player_creatures.Count == 1 && Enemy.enemy_creatures.Count > 0)
            {
                GameObject ourtarget = Player.player_creatures[0].gameObject;
                GameObject secondtarget = Enemy.enemy_creatures[Random.Range(0, Enemy.enemy_creatures.Count)].gameObject;
                targets.Add(ourtarget);
                targets.Add(secondtarget);
            }

        }
        else if (NegativeEffect) targets.Add(EffectManager.HighestAttackCreature(creatures_to_search));

    }


    public static List<GameObject> Creatures()

    {
        
        List<GameObject> templist = new List<GameObject>();


        foreach (card foundcard in enemy_creatures)
            if (!foundcard.Hero) templist.Add(foundcard.gameObject);
        return templist;

    }

    public static GameObject RandomAlly()
    {
        return enemy_creatures[Random.Range(0, enemy_creatures.Count)].gameObject;
    }

    public static GameObject RandomCreature()
    {
        List<card> foundcreatures = new List<card>();

        foreach (card foundcard in enemy_creatures)
            if (!foundcard.Hero) foundcreatures.Add(foundcard);

        if (foundcreatures.Count > 0)
            return EffectManager.RandomCard(foundcreatures);
        else
            return null;
    }

    public static GameObject RandomCreatureWithCostEqualOrLowerThan(int param)
    {
        List<card> foundcreatures = new List<card>();

        foreach (card foundcard in enemy_creatures)
            if (foundcard.Cost.Count <= param && !foundcard.Hero) foundcreatures.Add(foundcard);

        if (foundcreatures.Count > 0)
            return EffectManager.RandomCard(foundcreatures);
        else
            return null;
    }


    public static GameObject ChooseTargetForAttacking()
    {
        return GameObject.FindWithTag("Player") as GameObject;
    }
    void AssignTarget()
    {

        Player.targets.Add(gameObject);
        Player.NeedTarget = 0;
    }

    void OnMouseDown()
    {
        if (Player.NeedTarget == 1)
        {


            AssignTarget();

        }
        else if (Player.NeedTarget == 2)
        {

            AssignTarget();
        }
    }


    public static bool HasACreature()
    {
        foreach (card foundcreature in enemy_creatures)
            if (!foundcreature.Hero) return true;

        return false;
    }


    public static bool HasAHero()
    {
        foreach (card foundcard in enemy_creatures)
        {
            if (foundcard.Hero) return true;
        }
        return false;
    }

    public static void AddMana(int amount, int type = -1)
    {

    }

    public static void NewTurn()
    {
        AlliesDestroyedThisTurn = 0;
        TurnPopUpTimer = 0;
        Enemy.targets.Clear();
        GameScript.RemoveAllEOTBuffsAndDebuffs();
        Player.Turn++;
        TriggerCardAbilities(abilities.ON_START_OF_YOUR_TURN);

        mana.Clear();

        Enemy.EnemyTurn = true;


        foreach (card foundcard in cards_in_game)
        {

            if (foundcard.FirstTurnInGame) { foundcard.FirstTurnInGame = false; }

            if (foundcard.IsTurned == true)
            {

                foundcard.transform.Rotate(0, 0, 90);
                foundcard.IsTurned = false;
                foundcard.AttackedThisTurn = 0;
            }

        }



    }



    void OnGUI()
    {

        GUI.skin = GameObject.FindWithTag("Player").GetComponent<Player>().customskin;
        GUI.Label(new Rect(4, 130, 260, 20), Enemy.EnemyName);
        GUI.Label(new Rect(4, 150, 60, 20), "Life: " + Life.ToString());
        GUI.Label(new Rect(4, 170, 160, 20), "Cards in hand: " + CardsInHand.ToString());
        GUI.Label(new Rect(4, 190, 160, 20), "Cards in deck: " + NumberOfCardsInDeck.ToString());


        GUI.Label(new Rect(4, 210, 200, 20), "Mana: " + mana);

        GUI.Label(new Rect(800, 210, 180, 30), "Cards in graveyard: " + cards_in_graveyard.Count.ToString());


    }

    public void GainsMana(int amount)
    {

        for (int i = 0; i < amount; i++)
            mana.Add(MainMenu.DTA.settings.colors[0]);

        Instantiate(manafx, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1), transform.rotation);
    }

    public void IsAttacked(card Attacker)
    {
      
        Life -= Attacker.CreatureOffense;

        GetComponent<AudioSource>().PlayOneShot(Hit);
        GetComponent<Renderer>().material.color = Color.red;
        Invoke("RestoreColor", 0.3f);
    }

    public void RestoreColor()
    {
        GetComponent<Renderer>().material.color = Color.white;
    }

    public void TakesCardSFX()
    {
        GetComponent<AudioSource>().PlayOneShot(TakesCard);
    }

    public void IsHitBySpell(Vector3 param)
    {
        int amount = (int)param.x;
        int damagetype = (int)param.y;

        if (damagetype == 0)
        {
            Instantiate(firefx, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1), transform.rotation);
            GetComponent<AudioSource>().PlayOneShot(HitBySpell);
            GetComponent<Renderer>().material.color = Color.red;
            Invoke("RestoreColor", 0.3f);
        }
        if (damagetype == 1)
        {
            GetComponent<AudioSource>().PlayOneShot(Hit);
            GetComponent<Renderer>().material.color = Color.red;
            Invoke("RestoreColor", 0.3f);
        }
        Life -= amount;
    }
    public void IsHealed(int param)
    {
        Life += param;
        Instantiate(healfx, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1), transform.rotation);
        GetComponent<AudioSource>().PlayOneShot(Healed);

    }
}
