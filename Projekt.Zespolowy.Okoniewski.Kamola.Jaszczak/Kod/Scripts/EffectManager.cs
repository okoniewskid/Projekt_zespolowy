using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



//Skrypt obslugujacy czary i umiejetnosci kart po wybraniu celu
//Umiejetnosci oraz czary korzystaja ze stosu w celu rozpatrywania kolejnosci ich zajscia


public class EffectToDo
{

    public bool AI;
    public card effectcard;
    public int effect_number;
    public List<GameObject> targets = new List<GameObject>();
}


public class EffectManager : MonoBehaviour
{
    static List<GameObject> ourtargets = new List<GameObject>();
    static GameObject ourtarget;
    static GameObject secondtarget;
    static public EffectManager instance;

    public static List<EffectToDo> Stack = new List<EffectToDo>();


    void Awake()
    {
        instance = this;
    }
    void Start()
    {

    }


    void Update()
    {
        if (Stack.Count > 0 && !Player.EffectInProcess && Player.CanDoStack)
        {

            Player.EffectInProcess = true;

            EffectToDo lastadded = new EffectToDo();

            lastadded.AI = Stack[Stack.Count - 1].AI;
            lastadded.effectcard = Stack[Stack.Count - 1].effectcard;
            lastadded.effect_number = Stack[Stack.Count - 1].effect_number;
            lastadded.targets = Stack[Stack.Count - 1].targets;

            Stack.Remove(Stack[Stack.Count - 1]);

            DoEffect(lastadded.AI, lastadded.effectcard, lastadded.effect_number, lastadded.targets);


        }
    }

    //dodaj na stos
    public static void AddToStack(bool AI, card effectcard, int effect_number)
    {
        EffectToDo effect_to_add = new EffectToDo();

        effect_to_add.AI = AI;
        effect_to_add.effectcard = effectcard;
        effect_to_add.effect_number = effect_number;
        if (AI) foreach (GameObject target in Enemy.targets) effect_to_add.targets.Add(target);
        else foreach (GameObject target in Player.targets) effect_to_add.targets.Add(target);

        Stack.Add(effect_to_add);

    }


    //wykonaj efekt
    public static void DoEffect(bool AI, card effectcard, int effect_number, List<GameObject> effecttargets)
    {
        if (Stack.Count == 0)

        {
            Player.CanDoStack = false;


            GameObject.FindWithTag("Player").GetComponent<AudioSource>().PlayOneShot(effectcard.sfxAbility0);
            if (effectcard.Type == 2) effectcard.StartCoroutine("SpellAfterEffects", AI); //czar
            else Player.SpellInProcess = false; //inna umiejetnosc

        }




        int Param0 = 0;
        int param0type = effectcard.Effects[effect_number].param0type;

        int effect = effectcard.Effects[effect_number].type;

        if (effectcard.Type == 1 && effectcard.Effects[effect_number].trigger == 1) //umiejetnosc aktywowana jednostki
            effectcard.Turn();



        if (param0type == 1)    //ilosc sojusznikow
            if (effectcard.ControlledByPlayer) Param0 = Player.player_creatures.Count();
            else Param0 = Enemy.enemy_creatures.Count();

        else if (param0type == 2) //ilosc zniszczonych sojusznikow w biezacej turze
            if (effectcard.ControlledByPlayer) Param0 = Player.AlliesDestroyedThisTurn;
            else Param0 = Enemy.AlliesDestroyedThisTurn;

        else Param0 = effectcard.Effects[effect_number].param0;


        bool EOT = false;
        int BuffDebuffType = 0;


        ourtargets = effecttargets;

        switch (effect)
        { // sprawdz id efektu na gorze stosu i wywolaj odpowiednia metode
            case 0:     //leczenie 

                Heal(Param0);
                break;
            case 1:     // obrazenia

                if (Player.player_creatures.Contains(effectcard) || Enemy.enemy_creatures.Contains(effectcard)) Damage(Param0, effectcard.id_ingame);
                else Damage(Param0, effectcard.id_ingame);

                break;
            case 2:     //dobieranie kart

                DrawCard(AI, Param0);
                break;
            case 4: //umiesc karte w strefie kart złota



                PutTargetCardInLandZone(AI);


                break;
            case 5: //umiesc karte w swojej rece (z pola bitwy, cmentarza itp.)



                PutTargetCardInHand(AI);


                break;
            case 6: //walka pomiedzy jednostkami

                Brawl();
                break;

            case 8: //obrocenie karty do pozycji pierwotnej

                UntapTarget();
                break;
            case 9: //zniszcz jednostke

                DestroyCreature();
                break;
            case 10: //efekt oslabienia (debuff)

                EOT = effectcard.Effects[effect_number].eot;


                BuffDebuffType = effectcard.Effects[effect_number].bufftype;
                DoBuff(false, Param0, BuffDebuffType, EOT, effectcard);
                break;

            case 11: //wzmocnienie (buff)

                EOT = effectcard.Effects[effect_number].eot;


                BuffDebuffType = effectcard.Effects[effect_number].bufftype;
                DoBuff(true, Param0, BuffDebuffType, EOT, effectcard);
                break;
            case 12: //umiesc karte na polu bitwy


                PlaceCreature(AI, Param0, effectcard);
                break;
            case 13: //umiesc jednostke na polu bitwy pod swoja kontrola (karta mogla wczesniej byc pod kontrola przeciwnika)


                PlaceCreature(AI);
                break;
            case 15: //dodaj manę


                GainMana(AI, Param0);
                break;
            default:

                break;
        }

        Player.EffectInProcess = false;






    }


    //umiesc jednostke w grze
    public static void PlaceCreature(bool AI, int Index = -1, card effectcard = null)
    {

        card newcreature;
        if (Index != -1)
        {
            newcreature = playerDeck.pD.MakeCard(Index);

            playerDeck.pD.PlaceCreatureInGame(newcreature, AI);
        }
        else

        {


            foreach (GameObject target in ourtargets)
            {
                card target_card = target.GetComponent<card>();


                if (Player.cards_in_hand.Contains(target_card)) Player.RemoveHandCard(target_card);
                else if (Enemy.cards_in_hand.Contains(target_card)) Enemy.RemoveHandCard(target_card);

                playerDeck.pD.PlaceCreatureInGame(target.GetComponent<card>(), AI);


                if (Player.cards_in_graveyard.Contains(target_card))
                {
                    target_card.PlayFX(playerDeck.pD.healfx);
                    target_card.RemoveFromGraveyard();
                }
                else if (Enemy.cards_in_graveyard.Contains(target_card))
                {
                    target_card.PlayFX(playerDeck.pD.healfx);
                    target_card.RemoveFromGraveyard(true);
                }

            }
        }
    }

    //dodaj mane
    public static void GainMana(bool AI, int amount)
    {
        if (AI)
            GameObject.FindWithTag("Enemy").SendMessage("GainsMana", amount);
        else
            GameObject.FindWithTag("Player").SendMessage("GainsMana", amount);

    }

    //umiesc karte w strefie kart złota
    public static void PutTargetCardInLandZone(bool AI = false)
    {
        card targetcard;
        foreach (GameObject target in ourtargets)
        {
            targetcard = target.GetComponent<card>();
            if (Player.cards_in_graveyard.Contains(targetcard)) Player.cards_in_graveyard.Remove(targetcard);
            else if (Enemy.cards_in_graveyard.Contains(targetcard)) Enemy.cards_in_graveyard.Remove(targetcard);
            if (!AI)
            {

                Player.AddLand(targetcard);
                targetcard.GetComponent<Renderer>().sortingOrder = 0;
                targetcard.ControlledByPlayer = true;
                GameObject.FindWithTag("Player").SendMessage("TakesCardSFX");
            }
            else
            {
                GameObject.FindWithTag("Enemy").SendMessage("TakesCardSFX");

                Enemy.AddLand(targetcard);
            }
        }
    }

    //umiesc karte w rece
    public static void PutTargetCardInHand(bool AI = false)
    {

        card targetcard;
        foreach (GameObject target in ourtargets)
        {
            targetcard = target.GetComponent<card>();

            if (Player.cards_in_graveyard.Contains(targetcard)) Player.cards_in_graveyard.Remove(targetcard);
            else if (Enemy.cards_in_graveyard.Contains(targetcard)) Enemy.cards_in_graveyard.Remove(targetcard);
            else if (Player.player_creatures.Contains(targetcard)) Player.RemoveCreature(targetcard);
            else if (Enemy.enemy_creatures.Contains(targetcard)) Enemy.RemoveCreature(targetcard);

            if (!AI)
            {

                Player.AddHandCard(targetcard);
                targetcard.ControlledByPlayer = true;

            }
            else
            {
                Enemy.AddHandCard(targetcard);
            }
        }

    }


    //dobierz jedna lub wiecej kart
    public static void DrawCard(bool AI, int param)
    {

        instance.StartCoroutine(DrawCards(param, AI));
    }

    static IEnumerator DrawCards(int param, bool AI = false)
    {
        Zone zone_to_place_cards;
        if (AI) zone_to_place_cards = Enemy.HandZone;
        else zone_to_place_cards = Player.HandZone;


        for (int i = 0; i < param; i++)
        {

            zone_to_place_cards.DrawCard();


        }
        yield return new WaitForSeconds(1f);


    }
    //ulecz
    public static void Heal(int param)
    {

        foreach (GameObject target in ourtargets)
        {

            target.SendMessage("IsHealed", param);
        }
    }

    //zadaj obrazenia
    public static void Damage(int param, int cardid = -1)
    {

        foreach (GameObject target in ourtargets)
        {

            target.SendMessage("IsHitBySpell", new Vector3(param, 0, cardid));
        }


    }


    //wzmocnienie
    public static void DoBuff(bool positive, int param, int BuffType, bool EOT = false, card effectcard = null)
    {

        card card_to_buff;
        foreach (GameObject target in ourtargets)
        {
            card_to_buff = target.GetComponent<card>();

            card_to_buff.AddBuff(positive, param, BuffType, EOT, effectcard);

        }

    }


    //walka pomiedzy jednostkami
    public static void Brawl()
    {
        ourtargets[0].SendMessage("IsHitBySpell", new Vector3(ourtargets[1].GetComponent<card>().CreatureOffense, 1, -1));
        ourtargets[1].SendMessage("IsHitBySpell", new Vector3(ourtargets[0].GetComponent<card>().CreatureOffense, 1, -1));

    }
    //obroc karte do pierwotnej orientacji
    public static void UntapTarget()
    {
        foreach (GameObject target in ourtargets)
            if (target.GetComponent<card>().IsTurned) target.GetComponent<card>().UnTurn();


    }
    //zniszcz jednostke
    public static void DestroyCreature()
    {
        foreach (GameObject target in ourtargets)
            target.GetComponent<card>().Kill();





    }


    //ilosc jednostek w grze
    public static List<GameObject> CreaturesInGame(bool alsoHeroes = false)
    {
        List<GameObject> output = new List<GameObject>();

        foreach (card foundcreature in Player.player_creatures)
            if (!foundcreature.Hero || alsoHeroes) output.Add(foundcreature.gameObject);

        foreach (card foundcreature in Enemy.enemy_creatures)
            if (!foundcreature.Hero || alsoHeroes) output.Add(foundcreature.gameObject);
        return output;

    }

    //losowe jednostki
    public static List<GameObject> RandomCreatures(int need_creatures, List<card> cardslist)
    {
        List<GameObject> output = new List<GameObject>();

        if (cardslist.Count <= need_creatures)
        {
            foreach (card foundcard in cardslist) output.Add(foundcard.gameObject);

        }
        else if (need_creatures > 0)
        {
            output.Add(RandomCard(cardslist));
            for (int i = 1; i < need_creatures; i++)
            {

                GameObject tempcreature = RandomCard(cardslist);
                if (!output.Contains(tempcreature))
                {
                    output.Add(tempcreature);
                }
                else i--;
            }
        }
        return output;
    }

    //losowy obiekt w grze
    public static List<GameObject> RandomGameObjects(int need_objects, List<GameObject> gameobjlist)
    {
        List<GameObject> output = new List<GameObject>();

        if (gameobjlist.Count <= need_objects)
        {
            foreach (GameObject foundobj in gameobjlist) output.Add(foundobj);

        }
        else if (need_objects > 0)
        {
            output.Add(RandomGameObject(gameobjlist));
            for (int i = 1; i < need_objects; i++)
            {

                GameObject tempobj = RandomGameObject(gameobjlist);
                if (!output.Contains(tempobj))
                {
                    output.Add(tempobj);
                }
                else i--;
            }
        }
        return output;
    }

    //jednostki oborcone
    public static List<card> TurnedCreatures(List<card> creatureslist)
    {

        List<card> output = new List<card>();
        foreach (card foundcreature in creatureslist)
        {
            if (foundcreature.IsTurned == true)
            {
                output.Add(foundcreature);
            }
        }
        return output;
    }

    //losowa karta z listy po id
    public static int RandomCardIdFromIntList(List<int> cardlist, int type = -1)
    {
        List<int> foundcards = new List<int>();
        if (type != -1)
            foreach (int foundcard in cardlist)
            {
                DbCard dbcard = MainMenu.DTA.cards.Where(x => x.id == foundcard).SingleOrDefault();

                if (dbcard != null && dbcard.type == type)
                    foundcards.Add(foundcard);
            }
        else
            foundcards = cardlist;

        return foundcards[Random.Range(0, foundcards.Count)];

    }
    //losowa karta z listy
    public static GameObject RandomCardFromIntList(List<int> cardlist, int type = -1)
    {
        List<int> foundcards = new List<int>();
        if (type != -1)
            foreach (int foundcard in cardlist)
            {
                DbCard dbcard = MainMenu.DTA.cards.Where(x => x.id == foundcard).SingleOrDefault();

                if (dbcard != null && dbcard.type == type)
                    foundcards.Add(foundcard);
            }
        else
            foundcards = cardlist;

        int randomcard = foundcards[Random.Range(0, foundcards.Count)];

        return playerDeck.pD.MakeCard(randomcard).gameObject;
    }
    //losowy obiekt w grze
    public static GameObject RandomGameObject(List<GameObject> objlist)
    {
        return objlist[Random.Range(0, objlist.Count)];
    }
    //losowa karta
    public static GameObject RandomCard(List<card> cardlist, int type = -1)
    {
        List<card> foundcards = new List<card>();
        if (type != -1)
        {
            foreach (card foundcard in cardlist)
            {
                if (foundcard.Type == type)
                    foundcards.Add(foundcard);
            }
        }
        else
            foundcards = cardlist;

        if (foundcards.Count > 0)
            return foundcards[Random.Range(0, foundcards.Count)].gameObject;
        else
            return null;
    }
    //jednostka o najwyzszym ataku
    public static GameObject HighestAttackCreature(List<card> creatureslist, bool alsoHeroes = false)
    {
        int highestAttackValue = -1;
        foreach (card foundcreature in creatureslist)
        {
            if (foundcreature.CreatureOffense > highestAttackValue && (alsoHeroes || !foundcreature.Hero))
                highestAttackValue = foundcreature.CreatureOffense;
        }
        List<card> biggestCreatures = new List<card>();
        foreach (card foundcreature in creatureslist)
        {
            if (foundcreature.CreatureOffense == highestAttackValue) biggestCreatures.Add(foundcreature);
        }
        return biggestCreatures[0].gameObject;
    }

}
