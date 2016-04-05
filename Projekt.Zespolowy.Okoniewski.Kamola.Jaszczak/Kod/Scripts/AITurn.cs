using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/*
 * Skrypt obsługujący sztuczną intelgiencję (bota) i przebieg jego tury
*/


public class AITurn : MonoBehaviour
{
    static public AITurn instance;

    bool OK_to_do_next_AI_action = false;

    void Awake()
    {
        instance = this;
    }

    //atak bota
    IEnumerator TryToAttack()
    {
        while (Player.SpellInProcess) yield return new WaitForSeconds(0.2f); //bot oczekuje na rozpatrzenie przez grę kart zagrywanych przez jego przeciwnika
        Enemy.targets.Clear();


        foreach (card foundcreature in Enemy.enemy_creatures)
        {   //bot sprawdza czy mozliwy z jego strony jest atak
            Enemy.targets.Clear();



            if (foundcreature.IsTurned == false && foundcreature.FirstTurnSickness() == false)
            {

                yield return new WaitForSeconds(1f);
                foundcreature.CreatureAttack(true);
                break;


            }

        }
        yield return new WaitForSeconds(0.1f);
        OK_to_do_next_AI_action = true;
    }

    //bot zagrywa kartę złota, kartę z ręki i próbuje zaatakować 
    IEnumerator AIPlays()
    {

        OK_to_do_next_AI_action = false;
        instance.StartCoroutine("PayCostAndPlay");
        while (!OK_to_do_next_AI_action) yield return new WaitForSeconds(0.2f);
        OK_to_do_next_AI_action = false;
        instance.StartCoroutine("TryToAttack");
        while (!OK_to_do_next_AI_action) yield return new WaitForSeconds(0.2f);
        OK_to_do_next_AI_action = false;
        instance.StartCoroutine("TryToAttack");
        while (!OK_to_do_next_AI_action) yield return new WaitForSeconds(0.2f);
        OK_to_do_next_AI_action = false;
        instance.StartCoroutine("TryToAttack");
        while (!OK_to_do_next_AI_action) yield return new WaitForSeconds(0.2f);
        OK_to_do_next_AI_action = false;
        instance.StartCoroutine("PayCostAndPlay");
        while (!OK_to_do_next_AI_action) yield return new WaitForSeconds(0.2f);
        OK_to_do_next_AI_action = false;
        instance.StartCoroutine("TryToAttack");
        while (!OK_to_do_next_AI_action) yield return new WaitForSeconds(0.2f);

        Enemy.TriggerCardAbilities(abilities.ON_END_OF_YOUR_TURN);


        if (!Player.GameEnded)
        {
            Enemy.EnemyTurn = false;
            Player.PlayersTurn = true;

            Player.NewTurn();
        }
    }

    //bot sprawdza czy stac go na zagranie karty
    bool CanPayManaCost(card card_to_check)
    {

        int potential_mana = Enemy.mana.Count;
        foreach (card foundland in Enemy.lands_in_game)
            if (!foundland.IsTurned) potential_mana++;

        if (card_to_check.Cost.Count <= potential_mana) return true;


        return false;
    }
    //bot placi koszt i zagrywa karte
    IEnumerator PayCostAndPlay()
    {

        List<card> templist = new List<card>(Enemy.cards_in_hand);

        foreach (card foundcard in templist)
        {

            while (Player.SpellInProcess) yield return new WaitForSeconds(0.2f);

            if (foundcard.Type != 0 && CanPayManaCost(foundcard) && foundcard.DiscardCost <= (Enemy.CardsInHand + 1) && CheckCard(foundcard))

            { //bot placi koszt many uzywając swoich kart złota

                List<ManaColor> LeftToTurn = new List<ManaColor>();
                foreach (ManaColor foundcost in foundcard.Cost)
                    LeftToTurn.Add(foundcost);

                foreach (ManaColor unspentmana in Enemy.mana)
                    if (LeftToTurn.Any(x => x.name == unspentmana.name)) LeftToTurn.Remove(LeftToTurn.Where(x => x.name == unspentmana.name).First());

                yield return new WaitForSeconds(0.25f);

                if (LeftToTurn.Count > 0) foreach (card foundland in Enemy.lands_in_game)
                    {

                        if (!foundland.IsTurned)
                            if (LeftToTurn.Any(x => x.name == foundland.CardColor.name))
                            {
                                yield return new WaitForSeconds(0.3f);
                                foundland.TurnLandForMana(true);

                                LeftToTurn.RemoveAt(0);
                                if (LeftToTurn.Count == 0) { yield return new WaitForSeconds(0.5f); break; }
                            }
                    }

                for (int i = 0; i < foundcard.DiscardCost; i++)
                {
                    foreach (card foundcardtodiscard in Enemy.cards_in_hand)
                    {
                        if (foundcardtodiscard != foundcard) { foundcardtodiscard.Discard(true); break; }
                    }
                }

                if (foundcard.Type == 1) foundcard.StartCoroutine("FromHandCreature", true);

                else if (foundcard.Type == 2) foundcard.FromHandSpell(true);

                else foundcard.StartCoroutine("FromHandEnchantment", true);

                break;
                //jesli to mozliwe, bot zagrywa kartę z ręki
            }
        }

        while (Player.SpellInProcess) yield return new WaitForSeconds(0.2f);
        OK_to_do_next_AI_action = true;
    }




    static public void DoCoroutine()
    {

        instance.StartCoroutine("AIPlays");


    }

    //metoda sprawdzająca rodzaj karty i jej mozliwe cele
    static public bool CheckCard(card card_to_check)
    {

        if (card_to_check.Level > 0)
        {
            foreach (card foundcard in Enemy.cards_in_game)
                if (foundcard.Level == (card_to_check.Level - 1) && foundcard.GrowID == card_to_check.GrowID) return true;

            return false;
        }

        if (card_to_check.Type == 0)
        {
            if (!Enemy.LandsZone.CanPlace(true))
                return false;
        }
        else if (card_to_check.Type != 2 && !Enemy.CreaturesZone.CanPlace(true)) //jesli karta jest jednostką
            return false;


        if (card_to_check.Effects.Count > 0)
        {
            foreach (Effect foundeffect in card_to_check.Effects)
            {
                if (foundeffect.trigger != 1)
                {

                    int Target = 0;
                    int TargetParam = 0;

                    int effect = foundeffect.type;
                    Target = foundeffect.target;
                    TargetParam = foundeffect.targetparam0;


                    bool NegativeEffect = true;
                    if (effect == 11 || effect == 0) NegativeEffect = false; //jesli karta jest wzmocnieniem lub leczy jednostki

                    List<card> creatures_to_search = new List<card>();

                    if (foundeffect.param0type == 1) //ilosc jednostek bota
                    {
                        if (!Enemy.HasACreature()) return false;
                    }
                    else if (foundeffect.param0type == 2) //ilosc jednostek bota ktore zginely w tej turze
                    {
                        if (Enemy.AlliesDestroyedThisTurn <= 0) return false;
                    }

                    if (NegativeEffect)
                    {
                        foreach (card foundcard in Player.player_creatures)
                            if (!foundcard.Hero) creatures_to_search.Add(foundcard);
                    }
                    else {
                        foreach (card foundcard in Enemy.enemy_creatures)
                            if (!foundcard.Hero) creatures_to_search.Add(foundcard);
                    }


                    if (Target == 12) //jesli celem są wszystkie sojusznicze jednostki
                    {
                        if (!Enemy.HasACreature()) return false;
                    }


                    if (Target == 7) //jesli jednostka jest typu Bohater
                    {
                        if (!Enemy.HasAHero()) return false;
                    }

                    // jesli karta leczy i potrzebuje celu (tzn. nie dziala na wszystkie jednostki jednoczesnie) 
                    if (effect == 0 && (Target == 12 || Target == 9))
                    {
                        bool found = false;
                        foreach (card foundcreature in Enemy.enemy_creatures)
                        {
                            if (foundcreature.CreatureDefense < foundcreature.CreatureStartingDefense) found = true;
                        }
                        if (!found) return false;
                    }

                    // jesli celem karty jest karta ktora w biezacej turze atakowala
                    if (Target == 6)
                    {
                        bool found = false;
                        foreach (card foundcreature in creatures_to_search)
                            if (foundcreature.IsTurned && foundcreature.AttackedThisTurn > 0) found = true;

                        if (!found) return false;
                    }
                    else if (Target == 261) //jesli celem jest X losowych jednostek na polu bitwy
                    {
                        if (EffectManager.CreaturesInGame().Count < foundeffect.targetparam0) return false;

                    }

                    else if (Target == 30) //jesli celem jest jednostka o ataku X lub mniej
                    {
                        foreach (card foundcreature in creatures_to_search)

                            if (foundcreature.CreatureOffense > TargetParam) return false;

                    }
                    else if (Target == 31) //celem jest jednostka o koszcie many X lub mniej
                    {
                        foreach (card foundcreature in creatures_to_search)

                            if (!foundcreature.Hero && foundcreature.Cost.Count > TargetParam) return false;

                    }
                    else if (Target == 202) //celem jest jednostka przeciwnika o koszcie X lub mniej
                    {
                        if (!Enemy.RandomCreatureWithCostEqualOrLowerThan(foundeffect.targetparam0)) return false;

                    }
                    else if (Target == 200 || Target == 201 || Target == 14)  //celami sa jednostki przeciwnika
                    {
                        if (!Player.HasACreature()) return false;

                    }
                    else if (Target == 5 || Target == 4) //celem sa wszystkie jednostki
                    {
                        if (creatures_to_search.Count == 0)
                            return false;
                    }
                    else if (Target == 6) //obrocona jednostka (np. po ataku)
                    {
                        bool found = false;
                        foreach (card foundcard in creatures_to_search)
                            if (foundcard.IsTurned) found = true;

                        if (!found) return false;
                    }


                    if (Target == 50 || Target == 51 || Target == 302 || Target == 303)
                    {
                        bool found = false;
                        foreach (card graveyardcard in Enemy.cards_in_graveyard) //jesli celem jest karta na cmentarzu
                        {
                            if (graveyardcard.Type == 2 && (Target == 50 || Target == 303)) found = true; //jesli nie jest to jednostka
                            if (graveyardcard.Type == 1 && (Target == 51 || Target == 302)) found = true;
                        }
                        if (!found) return false;
                    }


                    if (effect == 8) //odwrocenie jednostki
                    {
                        bool found = false;
                        foreach (card foundcard in Enemy.enemy_creatures)
                            if (foundcard.IsTurned) found = true;

                        if (!found) return false;
                    }
                    // jesli efektem karty jest Starcie to upewnia sie czy na polu bitwy sa co najmniej 2 jednostki (Starcie wymaga co najmniej 2 jednostek)
                    else if (effect == 6)
                    {
                        if (!Player.HasACreature()) return false;

                        if (!Enemy.HasACreature() && Player.Creatures().Count <= 1) return false;

                    }

                    // jesli efektem jest wzmocnienie, upewnia sie czy przeciwnik ma jednostke
                    else if (effect == 11)
                    {
                        if (!Enemy.HasACreature()) return false;
                    }
                }
            }
        }
        return true;
    }



}