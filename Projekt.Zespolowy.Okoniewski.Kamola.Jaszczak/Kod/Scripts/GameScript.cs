using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameScript : MonoBehaviour
{
    //Skrypt oblsugujacy przebieg rozgrywki czyli poczatek gry, jej koniec oraz niektore efekty 

    void Awake()
    {

        int i;
        bool foundname;
        foreach (Zone foundzone in Object.FindObjectsOfType(typeof(Zone)) as Zone[])
        {

            foundname = false;
            foreach (DBZone dbz in MainMenu.DTA.settings.zones)
                if (dbz.Name == foundzone.dbzone.Name)
                {
                    foundzone.dbzone = dbz;
                    foundname = true;
                    foundzone.zone_id = MainMenu.DTA.settings.zones.IndexOf(dbz);
                    break;
                }

            if (!foundname) foreach (DBZone dbz_e in MainMenu.DTA.settings.enemy_zones)
                    if (dbz_e.Name == foundzone.dbzone.Name)
                    {
                        foundzone.dbzone = dbz_e;
                        foundname = true;
                        foundzone.BelongsToPlayer = false;
                        foundzone.zone_id = MainMenu.DTA.settings.enemy_zones.IndexOf(dbz_e);
                        break;
                    }

            if (foundname)
            {
                foundzone.Awake();
                playerDeck.pD.zones.Add(foundzone);
                i = 0;

                foreach (Transform child in foundzone.transform)
                    if (child.GetComponent<Slot>())
                    {
                        child.GetComponent<Slot>().number_in_zone = i;
                        i++;

                        if (foundzone.dbzone.Name == "Grid")
                        {

                            playerDeck.pD.Grid[child.GetComponent<Slot>().row, child.GetComponent<Slot>().column] = child.GetComponent<Slot>();
                        }

                    }


                switch (foundzone.dbzone.Name)
                {
                    case "Creatures":
                        Player.CreaturesZone = foundzone;
                        break;
                    case "Enemy creatures":
                        Enemy.CreaturesZone = foundzone;
                        break;
                    case "Hand":
                        Player.HandZone = foundzone;
                        break;
                    case "Enemy hand":
                        Enemy.HandZone = foundzone;
                        break;
                    case "Lands":
                        Player.LandsZone = foundzone;
                        break;
                    case "Enemy lands":
                        Enemy.LandsZone = foundzone;
                        break;
                    case "Graveyard":
                        Player.GraveyardZone = foundzone;
                        break;
                    case "Enemy graveyard":
                        Enemy.GraveyardZone = foundzone;
                        break;
                }

                if (MainMenu.DTA.settings.enemy_zones.Contains(foundzone.dbzone)) foundzone.zone_id = MainMenu.DTA.settings.enemy_zones.IndexOf(foundzone.dbzone);
                else if (MainMenu.DTA.settings.zones.Contains(foundzone.dbzone)) foundzone.zone_id = MainMenu.DTA.settings.zones.IndexOf(foundzone.dbzone);

            }
            
        }


        playerDeck.pD.TheSecondPlayerCanPlay = false;
        playerDeck.pD.TheFirstPlayerCanPlay = false;


        Enemy.StartGame();
        Player.StartGame();

 

            Player.CreaturesZone = GameObject.Find("Zone - Grid").GetComponent<Zone>();
            Enemy.CreaturesZone = GameObject.Find("Zone - Grid").GetComponent<Zone>();



            Player.CreaturesZone.slot_to_use = GameObject.FindWithTag("Player").GetComponent<Player>().hero_slot.transform;
            Player.AddCreature(playerDeck.pD.MakeCard(0));
            Enemy.CreaturesZone.slot_to_use = GameObject.FindWithTag("Enemy").GetComponent<Enemy>().hero_slot.transform;
            Enemy.AddCreature(playerDeck.pD.MakeCard(1, true));
        


        playerDeck.pD.SaveDeckBeforePlaying();

        Enemy.Deck = playerDeck.pD.LoadDeck(playerDeck.pD.offlineDeck.text);
        Enemy.NumberOfCardsInDeck = Enemy.Deck.Count;

        playerDeck.pD.ShuffleDeck(playerDeck.pD.Deck);
        playerDeck.pD.ShuffleDeck(Enemy.Deck);




        StartCoroutine(StartGame());
    }


    void Start()
    {


    }


    void Update()
    {

    }

    //rozpoczecie rozgrywki
    IEnumerator StartGame()

    {
        // dobieranie kart przez gracza i jego przeciwnika

        playerDeck.pD.first_or_second = 1;


        foreach (Zone foundzone in Object.FindObjectsOfType(typeof(Zone)) as Zone[])
            if (foundzone.DrawAtTheStartOfGame > 0) foundzone.StartCoroutine("DrawCards", foundzone.DrawAtTheStartOfGame);

        Player.NewTurn();

       
        Player.PlayersTurn = true;
        yield return new WaitForSeconds(0.1f);
    }

    //usuniecie wszelkich wzmocnien i oslabien dzialajacych do konca tury
    public static void RemoveAllEOTBuffsAndDebuffs()
    {
        foreach (card foundcard in Player.cards_in_game)
            if (foundcard.Type == 1)
                foundcard.RemoveEOTBuffsAndDebuffs();


        foreach (card foundcard in Enemy.cards_in_game)
            if (foundcard.Type == 1)
                foundcard.RemoveEOTBuffsAndDebuffs();

    }
}
