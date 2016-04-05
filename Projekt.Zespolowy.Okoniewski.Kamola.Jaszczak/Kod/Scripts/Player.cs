using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


//Skrypt obslugujacy rozgrywkê, przebieg tury, zawierajacy metody sterujace poczynaniami gracza

public class Player : MonoBehaviour
{

    public static float waiting_for_match_time;
    public GUISkin customskin = null;
    public static int Turn;

    public static int Life;
    public static List<ManaColor> mana = new List<ManaColor>();
    public static int Lands = 0;

    public static int LandsPlayedThisTurn = 0;
    public static bool PlayersTurn = false;

    public static Zone HandZone;
    public static Zone LandsZone;
    public static Zone CreaturesZone;
    public static Zone GraveyardZone;

    public static Zone EnchantmentsZone;
    public static Zone UpgradableCreaturesZone;

    public static bool WaitForCheck = false;

    public Texture2D TargetCursorTexture;
    public static int NeedTarget = 0;
    public static int CurrentTargetParam = 0;
    public static string CurrentTargetParamString = "";

    public static bool CreatureStatsNeedUpdating = false;

    public static bool Lost;

    public static float TurnPopUpTimer = 1.5f;
    public static float TurnPopUpTimerMax = 1.5f;

    public static List<GameObject> targets = new List<GameObject>();


    public static card AttackerCreature;


    public static bool CanUnzoom = true;

    private float WarningTimeToDisplay;

    public static bool TriggerInProcess = false;

    public static bool SpellInProcess = false;
    public static bool EffectInProcess = false;
    public static bool ActionCancelled = false;
    public static bool CanDoStack = false;

    public static bool DisplayingCreatureMenu = false;

    public static string Warning = "";
    public static string WarningText = "";

    public AudioClip Healed;
    public AudioClip HitBySpell;
    public AudioClip Hit;
    public AudioClip TakesCard;

    public Transform healfx;
    public Transform firefx;
    public Transform manafx;

    public static Object[] BattleMusic;
    private int currentSongIndex = 0;


    public static List<card> player_creatures = new List<card>();
    public static List<card> lands_in_game = new List<card>();
    public static List<card> cards_in_game = new List<card>();
    public static List<card> cards_in_hand = new List<card>();
    public static List<card> cards_in_graveyard = new List<card>();
    public static List<card> temp_cards = new List<card>();
    public static List<card> enchantments = new List<card>();


    public static bool GameEnded = false;
    public static bool TutorialOn = false;

    public float averagetime;

    public Slot hero_slot;
    public static bool HeroIsDead = false;

    public static int AlliesDestroyedThisTurn = 0;

    public void Awake()
    {
        this.tag = "Player";
    }

    public static void StartGame()
    {



        AlliesDestroyedThisTurn = 0;
        CanDoStack = false;

        temp_cards.Clear();
        lands_in_game.Clear();
        lands_in_game = Player.LandsZone.cards;
        cards_in_game.Clear();
        cards_in_hand.Clear();
        cards_in_hand = Player.HandZone.cards;
        cards_in_graveyard.Clear();
        cards_in_graveyard = Player.GraveyardZone.cards;
        player_creatures.Clear();
        targets.Clear();

        Life = MainMenu.DTA.settings.OptionStartingLife;
        Turn = 0;
        mana.Clear();

        Lands = 0;
        LandsPlayedThisTurn = 0;
        PlayersTurn = false;
        NeedTarget = 0;
        CreatureStatsNeedUpdating = false;
        SpellInProcess = false;
        EffectInProcess = false;
        Warning = "";
        WarningText = "";
        GameEnded = false;

        DisplayingCreatureMenu = false;

        Lost = false;
        HeroIsDead = false;
        GameObject ourCamera = GameObject.FindGameObjectWithTag("MainCamera");
        ourCamera.AddComponent<AudioSource>();
        ourCamera.GetComponent<AudioSource>().volume = 0.12f;
        BattleMusic = Resources.LoadAll("audio/music/battle");


    }

    public static void AddLand(card card_to_add)
    {
        cards_in_game.Add(card_to_add);
        Zone lands = Player.LandsZone;

        lands.AddCard(card_to_add);
        card_to_add.GetComponent<AudioSource>().PlayOneShot(card_to_add.sfxEntry);

    }

    public static void AddHandCard(card card_to_add)
    {
        Zone handcards = Player.HandZone;

        handcards.AddCard(card_to_add);

        GameObject.FindWithTag("Player").SendMessage("TakesCardSFX");
    }

    public static void RemoveHandCard(card card_to_remove)
    {
        Zone handcards = Player.HandZone;

        handcards.RemoveCard(card_to_remove);
    }

    public static void AddCreature(card card_to_add)
    {

        cards_in_game.Add(card_to_add);
        player_creatures.Add(card_to_add);

        Zone creaturezone = Player.CreaturesZone;

        card_to_add.ControlledByPlayer = true;

        creaturezone.AddCard(card_to_add);


        card_to_add.FirstTurnInGame = true;


        card_to_add.GetComponent<AudioSource>().PlayOneShot(card_to_add.sfxEntry);

        Player.CreatureStatsNeedUpdating = true;

    }

    public static void RemoveCreature(card card_to_remove)
    {
        cards_in_game.Remove(card_to_remove);
        player_creatures.Remove(card_to_remove);

        Zone creaturezone = Player.CreaturesZone;

        creaturezone.RemoveCard(card_to_remove);
    }

    public static void AddEnchantment(card card_to_add)
    {
        cards_in_game.Add(card_to_add);
        enchantments.Add(card_to_add);

        Zone creaturezone = Player.CreaturesZone;

        creaturezone.AddCard(card_to_add);
        card_to_add.GetComponent<AudioSource>().PlayOneShot(card_to_add.sfxEntry);

    }

    public static void RemoveEnchantment(card card_to_remove)
    {
        cards_in_game.Remove(card_to_remove);
        enchantments.Remove(card_to_remove);

        Zone creaturezone = Player.CreaturesZone;

        creaturezone.RemoveCard(card_to_remove);

    }
    public static void TriggerCardAbilities(int ability_trigger, int subtype = -1)
    {
        TriggerInProcess = true;
        foreach (card foundcard in cards_in_game)
        {
            if (foundcard.GetComponent<abilities>() != null) foundcard.abilities.TriggerAbility(ability_trigger, false, subtype);
        }
        TriggerInProcess = false;
    }

    void Start()
    {



        StartCoroutine("MusicPlaylist");

    }

    void OnMouseDown()
    {
        if (Player.NeedTarget == 8)
        {


            AssignTarget();
        }
    }

    void AssignTarget()
    {
        targets.Add(gameObject);
        NeedTarget = 0;

    }

    public static List<GameObject> Creatures()
    {
        List<GameObject> templist = new List<GameObject>();


        foreach (card foundcard in player_creatures)
            if (!foundcard.Hero) templist.Add(foundcard.gameObject);
        return templist;

    }

    public static GameObject RandomAlly()
    {
        return player_creatures[Random.Range(0, player_creatures.Count)].gameObject;
    }

    public static GameObject RandomCreature()
    {
        List<card> foundcreatures = new List<card>();

        foreach (card foundcard in player_creatures)
            if (!foundcard.Hero) foundcreatures.Add(foundcard);

        if (foundcreatures.Count > 0)
            return EffectManager.RandomCard(foundcreatures);
        else
            return null;
    }

    public static GameObject RandomCreatureWithCostEqualOrLowerThan(int param)
    {
        List<card> foundcreatures = new List<card>();

        foreach (card foundcard in player_creatures)
            if (foundcard.Cost.Count <= param && !foundcard.Hero) foundcreatures.Add(foundcard);

        if (foundcreatures.Count > 0)
            return EffectManager.RandomCard(foundcreatures);
        else
            return null;
    }


    public static void AddMana(int amount, int type = -1)
    {
        for (int i = 0; i < amount; i++)
            mana.Add(MainMenu.DTA.settings.colors[0]);
    }


    public static void NewTurn()
    {
        AlliesDestroyedThisTurn = 0;
        TurnPopUpTimer = 0;
        Player.targets.Clear();

        GameScript.RemoveAllEOTBuffsAndDebuffs();
        Player.Turn++;

        TriggerCardAbilities(abilities.ON_START_OF_YOUR_TURN);

        mana.Clear();

        Player.LandsPlayedThisTurn = 0;

        foreach (card foundcard in cards_in_game)
        {

            foundcard.FirstTurnInGame = false;


            foundcard.MovedThisTurn = 0;


            if (foundcard.IsTurned)
            {

                foundcard.transform.Rotate(0, 0, -MainMenu.DTA.settings.OptionTurnDegrees);
                foundcard.IsTurned = false;
                foundcard.AttackedThisTurn = 0;
            };
            foundcard.checked_for_highlight = false;
        }

        foreach (card foundcard in cards_in_hand)
            foundcard.checked_for_highlight = false;

        Player.HandZone.DrawCard();

    }



    void Update()
    {

        TurnPopUpTimer += Time.deltaTime;


        if (Input.GetMouseButtonDown(1))
        {
            if (Player.NeedTarget > 0)
            {
                ActionCancelled = true;

                Player.NeedTarget = 0;
            }
            else if (Player.DisplayingCreatureMenu)
            {
                Player.DisplayingCreatureMenu = false;

                foreach (card foundcreature in Player.player_creatures)
                    foundcreature.abilities.DisplayMenu = false;
            }
        }

        if (CreatureStatsNeedUpdating)
        {

            foreach (card foundcard in Player.player_creatures)
            {
                foundcard.UpdateCreatureAtkDefLabels();
            }
            foreach (card foundcard in Enemy.enemy_creatures)
            {
                foundcard.UpdateCreatureAtkDefLabels();
            }
            CreatureStatsNeedUpdating = false;

        }

        if ((Life <= 0) && !GameEnded)
        {


            GameLost();

        }
        else if ((Enemy.Life <= 0) && !GameEnded)
        {
            GameWon();

        }

    }

    public void GameWon()
    {


        Enemy.Lost = true;
        GameObject[] tempObjects = FindObjectsOfType(typeof(GameObject)) as GameObject[];

        for (int i = 0; i < tempObjects.Length; i++)
        {
            if (tempObjects[i].GetComponent<Renderer>() != null)
                tempObjects[i].GetComponent<Renderer>().material.color = Color.blue;
        }

        GameEnded = true;
        GameObject.FindWithTag("VictoryDefeat").SendMessage("EndOfGame");
        Player.PlayersTurn = false;
    }

    public void GameLost()
    {

        GameObject[] tempObjects = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        for (int i = 0; i < tempObjects.Length; i++)

        {
            if (tempObjects[i].GetComponent<Renderer>() != null)
                tempObjects[i].GetComponent<Renderer>().material.color = Color.grey;
        }

        Lost = true;
        GameObject.FindWithTag("VictoryDefeat").SendMessage("EndOfGame");
        GameEnded = true;
        Player.PlayersTurn = false;
    }

    public void IsAttacked(card Attacker)
    {
        int DamageToPlayer = Attacker.CreatureOffense;

        if (Attacker.IsCriticalStrike())
        {
            Player.Warning = "Critical strike!";
            DamageToPlayer = (int)(Attacker.CreatureOffense * Attacker.CritDamageMultiplier);
        }

        if (Attacker.DoubleDamage) DamageToPlayer = DamageToPlayer * 2;

        Life -= DamageToPlayer;

        GetComponent<AudioSource>().PlayOneShot(Hit);
        GetComponent<Renderer>().material.color = Color.red;
        Invoke("RestoreColor", 0.3f);
    }

    public void IsHealed(int param)
    {
        Life += param;
        Instantiate(healfx, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1), transform.rotation);
        GetComponent<AudioSource>().PlayOneShot(Healed);

    }

    public void TakesCardSFX()
    {
        GetComponent<AudioSource>().PlayOneShot(TakesCard);

    }

    public void GainsMana(int amount)
    {
        for (int i = 0; i < amount; i++)
            mana.Add(MainMenu.DTA.settings.colors[0]);

        Instantiate(manafx, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1), transform.rotation);
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

        else if (damagetype == 1)
        {
            GetComponent<AudioSource>().PlayOneShot(Hit);
            GetComponent<Renderer>().material.color = Color.red;
            Invoke("RestoreColor", 0.3f);
        }

        Life -= amount;


    }

    public void RestoreColor()
    {
        GetComponent<Renderer>().material.color = Color.white;
    }


    public static bool HasACreature()
    {
        foreach (card foundcreature in player_creatures)
            if (!foundcreature.Hero) return true;

        return false;
    }


    public static bool HasAHero()
    {
        foreach (card foundcard in player_creatures)
            if (foundcard.Hero) return true;
        return false;
    }


    IEnumerator MusicPlaylist()
    {

        bool PlayMusic = MainMenu.DTA.settings.OptionGameMusic;

        if (BattleMusic.Length > 0)
            while (PlayMusic)
            {
                GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
                AudioSource cameraMusic = camera.GetComponent<AudioSource>();

                yield return new WaitForSeconds(1.0f);
                if (!cameraMusic.isPlaying)
                {
                    if (currentSongIndex != (BattleMusic.Length - 1))
                    {
                        currentSongIndex++;
                        cameraMusic.clip = (AudioClip)BattleMusic[currentSongIndex];
                        cameraMusic.Play();
                    }
                    else
                    {
                        currentSongIndex = 0;
                        cameraMusic.clip = (AudioClip)BattleMusic[currentSongIndex];
                        cameraMusic.Play();
                    }
                }

            }
    }


    public static void OpenListToChooseCard(List<card> cardlist, int cardtype = -1)
    {
        bool at_least_one_valid_card = false;
        temp_cards.Clear();
        card tempcard = new card();
        int i = 0;

        GameObject choosecardtext = (GameObject)Instantiate(playerDeck.pD.choosecardprefab);
        choosecardtext.name = "ChooseCardText";

        choosecardtext.GetComponent<Renderer>().sortingOrder = 90;

        if (MainMenu.DTA.settings.UseGrid)
            foreach (Transform foundslot in Player.CreaturesZone.transform)
                foundslot.GetComponent<Collider>().enabled = false;

        foreach (card foundcard in cardlist)
        {
            if (cardtype != -1 && foundcard.Type == cardtype)
            {
                at_least_one_valid_card = true;
                tempcard = playerDeck.pD.MakeCard(foundcard.Index);
                playerDeck.pD.AddArtAndText(tempcard);

                tempcard.GetComponent<Renderer>().sortingOrder = 91;
                foreach (Transform child in tempcard.transform) child.GetComponent<Renderer>().sortingOrder = 91;
                tempcard.transform.Find("CardArt").GetComponent<Renderer>().sortingOrder = 90;

                tempcard.transform.position = new Vector3(1f + 0.5f * i, 1f, 0.1f);
                temp_cards.Add(tempcard);
                i++;
            }
        }
        if (!at_least_one_valid_card)
        {


            Player.NeedTarget = 0;
            Player.ActionCancelled = true;
            Destroy(GameObject.FindGameObjectWithTag("ChooseCardText"));
        }
    }

    public static void OpenIntListToChooseCard(List<int> cardlist, int cardtype = -1)
    {
        temp_cards.Clear();
        card tempcard = new card();
        int i = 0;

        GameObject choosecardtext = (GameObject)Instantiate(playerDeck.pD.choosecardprefab);
        choosecardtext.name = "ChooseCardText";
        choosecardtext.transform.position = new Vector3(-2f, 2.8f, 0.1f);
        DbCard dbcard;

        if (MainMenu.DTA.settings.UseGrid)
            foreach (Transform foundslot in Player.CreaturesZone.transform)
                foundslot.GetComponent<Collider>().enabled = false;

        foreach (int foundcardID in cardlist)
        {
            dbcard = MainMenu.DTA.cards.Where(x => x.id == foundcardID).SingleOrDefault();



            if (cardtype == -1 || dbcard.type == cardtype)

            {
                tempcard = playerDeck.pD.MakeCard(foundcardID);
                playerDeck.pD.AddArtAndText(tempcard);

                if (i > 7) tempcard.transform.position = new Vector3(-2f + 1f * (i - 8), -0.5f, 0.1f);
                else tempcard.transform.position = new Vector3(-2f + 1f * i, 1f, 0.1f);

                tempcard.GetComponent<Renderer>().sortingOrder = 91;
                foreach (Transform child in tempcard.transform) child.GetComponent<Renderer>().sortingOrder = 91;
                tempcard.transform.Find("CardArt").GetComponent<Renderer>().sortingOrder = 90;

                temp_cards.Add(tempcard);
            }
            i++;
        }
    }

    void PlayerTurnPopup(int windowID)
    {
        if (MainMenu.DTA.settings.OptionPlayerTurnPopup)
        {
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.Label(new Rect(50, 10, 152, 25), "Your Turn");

            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.label.fontStyle = FontStyle.Normal;
        }
    }

    IEnumerator EndTurn()
    {
        TriggerCardAbilities(abilities.ON_END_OF_YOUR_TURN);
        while (SpellInProcess) yield return new WaitForSeconds(0.2f);
        PlayersTurn = false;
        Enemy.NewTurn();
    }

    void OnGUI()
    {

        if (GUI.Button(new Rect(Screen.width * (1f / 130f), Screen.height * (7.41f / 8f), Screen.width * (1f / 15f), Screen.height * (1f / 14f)), "End Turn"))
        {
            if (PlayersTurn == true) StartCoroutine("EndTurn");

        }

        if ((TurnPopUpTimer < TurnPopUpTimerMax) && !Player.TutorialOn)
        {
            GUI.Window(0, new Rect((Screen.width * 0.5f) - 160, (Screen.height * 0.5f) - 50
             , 250, 50), PlayerTurnPopup, "");
        }

        GUI.skin = customskin;



        if (NeedTarget > 0)
        {

            if (NeedTarget == 4)
            {
                if (targets.Count == 0) GUI.Label(new Rect(400, 5, 200, 30), "Choose first target creature");
                else GUI.Label(new Rect(400, 5, 200, 30), "Choose second target creature");

            }
            else if (NeedTarget == 7)
                GUI.Label(new Rect(400, 5, 200, 30), "Choose a hero");
            else if (NeedTarget == 21)
                GUI.Label(new Rect(400, 5, 200, 30), "Choose a card to discard");
            else GUI.Label(new Rect(400, 5, 200, 30), "Choose a target");

            Cursor.SetCursor(TargetCursorTexture, new Vector2(5, 0), CursorMode.Auto);
        }
        else Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);


        string activeplayer = "";

        if (Player.PlayersTurn == true) activeplayer = "Player";
        else activeplayer = "Enemy";

        GUI.Label(new Rect(4, 275, 200, 25), "Turn " + Player.Turn + ": " + activeplayer);



        GUI.Label(new Rect(4, 440, 60, 20), "Player");

        GUI.Label(new Rect(60, 440, 60, 20), "Life: " + Player.Life.ToString());

        GUI.Label(new Rect(4, 460, 140, 20), "Cards in deck: " + playerDeck.pD.Deck.Count.ToString());
        GUI.Label(new Rect(4, 480, 140, 20), "Cards in hand: " + Player.cards_in_hand.Count.ToString());


        GUI.Label(new Rect(4, 500, 200, 20), "Mana: " + Player.mana.Count);

        GUI.Label(new Rect(800, 400, 180, 30), "Cards in graveyard: " + cards_in_graveyard.Count.ToString());

        GUI.Label(new Rect(740, 2, 300, 100), "Tap lands to gain mana\nUse mana to cast creatures and spells\nTap creatures to attack\nRMB on a creature to use its abilities\nHave fun!");



        if (Warning != "")
        {

            WarningTimeToDisplay = 5;
            WarningText = Warning;
            Warning = "";
        }


        if (WarningTimeToDisplay > 0)
        {
            GUI.contentColor = Color.red;

            GUI.Label(new Rect(400, 35, 300, 60), WarningText);
            WarningTimeToDisplay = WarningTimeToDisplay - Time.deltaTime;
        }


    }


}