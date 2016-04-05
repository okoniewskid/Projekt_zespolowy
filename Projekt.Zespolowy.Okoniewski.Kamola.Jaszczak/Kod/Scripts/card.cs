using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Skrypt zawierający definicję klasy Card i metod obslugujących interakcje, przenoszenie kart miedzy strefami oraz modyfikujących stany karty
//rodzaje kart: jednostka, czar(karta posiadająca jednorazowy efekt nastepujacy zaraz po zagraniu jej, po tym trafia na cmentarz), zaklecie(karta posiadajaca efekt przez caly czas gdy pozostaje na polu bitwy, nie jest jednostką!), złoto(karta generująca manę)


public class card : MonoBehaviour
{

    public static AudioClip Hit;
    public static AudioClip HitBySpell;
    public static AudioClip Healed;

    public AudioClip sfxMove0;
    public AudioClip sfxMove1;

    public AudioClip sfxEntry;
    public AudioClip sfxAbility0;

    public List<Effect> Effects = new List<Effect>();

    public bool Secret = false;
    public bool faceDown = false;

    public Sprite Art;
    public int Index = 0;
    public List<ManaColor> Cost = new List<ManaColor>();
    public int Level = 0;
    public string GrowID = "";

    public int DiscardCost = 0;

    public int CostInCurrency = 0;

    public Dictionary<string, int> CustomInts = new Dictionary<string, int>();
    public Dictionary<string, string> CustomStrings = new Dictionary<string, string>();

    public GameObject highlight = null;
    public ManaColor CardColor;
    public int Type = 0;
    public int Subtype = 0;

    public int CreatureOffense = 0;
    public int CreatureDefense = 0;
    public int Defense
    {
        get
        {
            return CreatureDefense;

        }
        protected set
        {
            CreatureDefense = value;

        }
    }

    public abilities abilities = null;
    public List<Buff> buffs;

    public int CreatureStartingOffense = 0;
    public int CreatureStartingDefense = 0;

    public int StartingDefense
    {
        get
        {
             return CreatureStartingDefense;

        }

    }

    public int CritChance = 0;
    public float CritDamageMultiplier = 2.5f;

    public bool FirstTurnInGame = true;

    public bool IsTurned = false;
    public bool ControlledByPlayer = false;

    public bool DoubleDamage = false;
    public bool TakesHalfDamage = false;

    public Transform healfx;
    public Transform firefx;

    public bool Dead = false;
    public bool ShowedByEnemy = false;



    public bool takes_no_combat_dmg = false;
    public bool deals_no_combat_dmg = false;
    public bool no_first_turn_sickness = false;
    public bool cant_attack = false;
    public bool free_attack = false;
    public bool takes_no_spell_dmg = false;

    public bool extramovement = false;
    public bool less_dmg_from_ranged = false;
    public bool no_dmg_from_ranged = false;

    public int AttackedThisTurn = 0;

    public bool Ranged = false;
    public int MovedThisTurn = 0;

    public static float ZoomHeight;

    public float Zoom;

    public static float ZoomEditDeckMode = 1.7f;
    public string Name = "";


    public int id_ingame;

    public bool Hero = false;

    public int Amount;
    public bool FilteredOut = false;
    public bool InCollection = false;


    public bool IsZoomed = false;
    bool IsRotatedForZoom = false;
    bool IsMovedForZoom = false;

    public bool isDragged = false;

    public bool checked_for_highlight = false;

    public static bool WaitABit = false;
    float old_y;
    int old_sortingorder;

    float mouseHoverSeconds = 0;
    float mouseHoverZoomTime = 0.5f;

    static List<int> NoTargetEffects = new List<int> { 2, 12, 15 };

    public Slot slot
    {
        get
        {
            if (transform.parent != null)
                return transform.parent.GetComponent<Slot>();

            return null;
        }
    }

    void Start()
    {

        Hit = playerDeck.pD.Hit;
        HitBySpell = playerDeck.pD.HitBySpell;
        Healed = playerDeck.pD.Healed;
    }

    void AddHighlight()
    {
        highlight = (GameObject)Instantiate(CardTemplate.Instance.transform.Find("Highlight").gameObject);

        playerDeck.pD.AssignParentWithLocalPos(highlight, gameObject);
    }

    //czy karte mozna zagrac
    bool IsPlayable()
    {

        if (!Player.PlayersTurn) return false;

        if (Player.cards_in_hand.Contains(this))
        {
            if (Type == 0 && Player.LandsPlayedThisTurn == 0) //czy gracz zagrał w tej turze karte złota, jesli nie to moze ją zagrac
                return true;

            if (Type == 1 && CanPayCosts(true))
                if (Level < 1) return true; //czy gracz moze zaplacic koszt many
                else if (HasUpgradableCreature(Player.player_creatures)) return true;



            if (Type > 1 && CanPayCosts(true) && ValidSpell(false)) //czy karta ma legalne cele 
                return true;
        }
        else if (Player.player_creatures.Contains(this))
            if (CanAttack())
                return true;

        return false;

    }


    void Update()
    {
        if (!checked_for_highlight && highlight)
        {
            if (!IsPlayable()) Destroy(highlight);
        }
        else if (!checked_for_highlight && !Player.SpellInProcess && !Player.EffectInProcess && IsPlayable())
            AddHighlight();

        checked_for_highlight = true;

        if (IsACreature())

            if (Defense <= 0 && !Dead)
            {
                Kill();

                if (Hero)
                    if (ControlledByPlayer) Player.HeroIsDead = true;
                    else Enemy.HeroIsDead = true;
            }

    }


    //usun wzmocnienia karty ktore mialy trwac do konca tury
    public void RemoveEOTBuffsAndDebuffs()
    {
        List<Buff> buffs_to_remove = new List<Buff>();

        foreach (Buff foundbuff in buffs)
            if (foundbuff.EOT)
                buffs_to_remove.Add(foundbuff);

        foreach (Buff foundEOTbuff in buffs_to_remove)
            RemoveBuff(foundEOTbuff);

    }

    //usun wzmocnienie
    public void RemoveBuff(Buff buff_to_remove)
    {
        bool positive = buff_to_remove.positive;
        int param = buff_to_remove.param;

        //wzmocnienia ofensywne
        switch (buff_to_remove.type)
        {
            case Buff.SET_ATTACK_TO:
                CreatureOffense = CreatureStartingOffense;
                break;
            case Buff.RAISE_ATTACK_BY:
                if (!positive) param = -param;
                CreatureOffense -= param;
                break;
            case Buff.MULTIPLY_ATTACK_BY:
                if (!positive) param = 1 / param;
                CreatureOffense /= param;
                break;
            //wzmocnienia defensywne
            case Buff.SET_DEFENSE_TO:
                CreatureDefense = CreatureStartingDefense;
                break;
            case Buff.RAISE_DEFENSE_BY:
                if (!positive) param = -param;
                CreatureDefense -= param;
                break;
            case Buff.MULTIPLY_DEFENSE_BY:
                if (!positive) param = 1 / param;
                CreatureDefense /= param;
                break;
            //inne wzmocnienia
            case Buff.SET_CRIT_CHANCE_TO:
                CritChance = 0;
                break;
            case Buff.ASSIGN_ABILITY:
                if (!positive) ReturnAbilities();
                else AssignSpecialAbility(param, false);
                break;
        }
        buffs.Remove(buff_to_remove);
    }


    public void ReturnAbilities()
    {
        DbCard dbcard = MainMenu.DTA.cards.Where(x => x.id == Index).SingleOrDefault();
        Effects = dbcard.effects;

        takes_no_combat_dmg = dbcard.takes_no_combat_dmg;
        deals_no_combat_dmg = dbcard.deals_no_combat_dmg;
        no_first_turn_sickness = dbcard.no_first_turn_sickness;
        cant_attack = dbcard.cant_attack;
        free_attack = dbcard.free_attack;
        takes_no_spell_dmg = dbcard.takes_no_spell_dmg;
        extramovement = dbcard.extramovement;
        less_dmg_from_ranged = dbcard.less_dmg_from_ranged;
        no_dmg_from_ranged = dbcard.no_dmg_from_ranged;

        if (transform.Find("Description3DText") != null)
            transform.Find("Description3DText").GetComponent<TextMesh>().text = playerDeck.TextWrap(dbcard.text, 30);
    }

    //usuwa efekty i umiejetnosci 
    public void RemoveAllAbilities()
    {
        Effects.Clear();

        takes_no_combat_dmg = false;
        deals_no_combat_dmg = false;
        no_first_turn_sickness = false;
        cant_attack = false;
        free_attack = false;
        takes_no_spell_dmg = false;
        extramovement = false;
        less_dmg_from_ranged = false;
        no_dmg_from_ranged = false;

        if (transform.Find("Description3DText") != null)
            transform.Find("Description3DText").GetComponent<TextMesh>().text = ""; //clear the card text
    }

    //dodaje wzmocnienie
    public void AddBuff(bool positive, int param, int BuffType, bool EOT = false, card effectcard = null)
    {
        Buff newbuff = new Buff();

        switch (BuffType)
        {
            
            case Buff.SET_ATTACK_TO:
                CreatureOffense = param;
                break;
            case Buff.RAISE_ATTACK_BY:
                if (!positive) param = -param;
                CreatureOffense += param;
                break;
            case Buff.MULTIPLY_ATTACK_BY:
                if (!positive) param = 1 / param;
                CreatureOffense *= param;
                break;
           
            case Buff.SET_DEFENSE_TO:
                CreatureDefense = param;
                break;
            case Buff.RAISE_DEFENSE_BY:
                if (!positive) param = -param;
                CreatureDefense += param;
                break;
            case Buff.MULTIPLY_DEFENSE_BY:
                if (!positive) param = 1 / param;
                CreatureDefense *= param;
                break;
            
            case Buff.SET_CRIT_CHANCE_TO:
                CritChance = param;
                break;
            case Buff.ASSIGN_ABILITY:
                if (!positive) RemoveAllAbilities();
                else AssignSpecialAbility(param);
                break;
        }
        UpdateCreatureAtkDefLabels();


        newbuff.type = BuffType;
        newbuff.positive = positive;
        newbuff.param = param;
        newbuff.EOT = EOT;

        if (effectcard.Type == 3) newbuff.enchantmentcard = effectcard; 
        buffs.Add(newbuff);
    }

    //przypisuje umiejetnosci
    public void AssignSpecialAbility(int ability_code, bool setTrue = true)
    {

        switch (ability_code)
        {
            case Buff.DEALS_NO_COMBAT_DMG:
                deals_no_combat_dmg = setTrue;
                break;
            case Buff.TAKES_NO_COMBAT_DMG:
                takes_no_combat_dmg = setTrue;
                break;
            case Buff.CANT_ATTACK:
                cant_attack = setTrue;
                break;
            case Buff.EXTRA_MOVEMENT:
                extramovement = setTrue;
                break;
            case Buff.FIRST_ATTACK_DOESNT_TURN:
                free_attack = setTrue;
                break;
            case Buff.TAKES_NO_DMG_FROM_SPELLS:
                takes_no_spell_dmg = setTrue;
                break;
            case Buff.NO_FIRST_TURN_SICKNESS:
                no_first_turn_sickness = setTrue;
                break;
        }
    }

    public void UpdateCreatureAtkDefLabels()
    {

        transform.Find("Offense3DText").GetComponent<TextMesh>().text = CreatureOffense.ToString();

        if (CreatureStartingOffense == CreatureOffense) transform.Find("Offense3DText").GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        else if (CreatureStartingOffense < CreatureOffense) transform.Find("Offense3DText").GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        else if (CreatureStartingOffense > CreatureOffense) transform.Find("Offense3DText").GetComponent<Renderer>().material.SetColor("_Color", Color.red);

        
            transform.Find("Defense3DText").GetComponent<TextMesh>().text = CreatureDefense.ToString();
            if (StartingDefense == Defense)
                transform.Find("Defense3DText").GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            else if (StartingDefense < Defense)
                transform.Find("Defense3DText").GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
            else if (StartingDefense > Defense)
                transform.Find("Defense3DText").GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        
    }


    //niszczy karte w grze 
    public void Kill()
    {


        if (ControlledByPlayer)
        {
            Player.TriggerCardAbilities(abilities.ON_FRIENDLY_DIES);    
            Player.AlliesDestroyedThisTurn++;
            Player.RemoveCreature(this);
        }
        else {
            Enemy.TriggerCardAbilities(abilities.ON_FRIENDLY_DIES);
            Enemy.AlliesDestroyedThisTurn++;
            Enemy.RemoveCreature(this);
        }


        
            MoveToGraveyard();
        
    }

    //przenies karte na cmentarz
    void MoveToGraveyard()
    {
        if (IsTurned) UnTurn();
        Dead = true;

        Zone graveyard;

        if (ControlledByPlayer) { ControlledByPlayer = false; graveyard = Player.GraveyardZone; }
        else graveyard = Enemy.GraveyardZone;

        graveyard.AddCard(this);

        if (Type == 1)
        { 
            for (int i = 0; i < buffs.Count; i++)   //usuwa efekty specjalnie na karcie 
                RemoveBuff(buffs[i]);

            CreatureOffense = CreatureStartingOffense;

            transform.Find("Offense3DText").GetComponent<TextMesh>().text = CreatureOffense.ToString();

            
                CreatureDefense = CreatureStartingDefense;
                transform.Find("Defense3DText").GetComponent<TextMesh>().text = CreatureDefense.ToString();
            

            UpdateCreatureAtkDefLabels();
        }

        Destroy(highlight);
    }

    //usun karte z cmentarza
    public void RemoveFromGraveyard(bool AI = false)
    {

        Dead = false;

        Zone graveyard;

        if (AI) graveyard = Enemy.GraveyardZone;
        else {
            ControlledByPlayer = true;
            graveyard = Player.GraveyardZone;
        }


        graveyard.RemoveCard(this);
    }

    //metoda obsluguje najazd kursorem na karte
    void OnMouseOver()
    {

        mouseHoverSeconds += Time.deltaTime;
        if (ShowedByEnemy)
            return;
        //karta jest przyblizana
        if (IsZoomed == false && mouseHoverSeconds >= mouseHoverZoomTime)
        {
            ZoomCard();
        }

        //klikniecie prawym klawiszem otwiera menu umiejetnosci
        if (Input.GetMouseButtonDown(1))
        {

            if (Player.cards_in_game.Contains(this))
            {


                abilities.DisplayMenu = true;


            }
        }

        else if (Input.GetMouseButtonDown(2))
        {

        }
    }
    //anuluje przyblizenie gdy kursor nie znajduje sie nad kartą
    void OnMouseExit()
    {

        if (MainMenu.DTA.settings.UseGrid && slot != null && !slot.zone.PlayerIsChoosingASlot)
            Player.CreaturesZone.RemoveHighlightedSlots();

        if (ShowedByEnemy)
            return;
        if (IsZoomed == true)
        {
            UnZoom();
        }
    }

    //metoda odpowiadajaca za przyblizenie karty
    void ZoomCard()
    {
        if (Application.loadedLevelName == MainMenu.SceneNameEditDeck)
        {

            Player.CanUnzoom = false;
            StartCoroutine(WaitBeforeUnZoom());
            BoxCollider2D collider = GetComponent<BoxCollider2D>() as BoxCollider2D;
            collider.size = EditDeckScripts.FirstCardColliderSize;
            collider.offset = new Vector2(0, 0);
            foreach (Transform child in transform)
                child.gameObject.layer = 8;
            gameObject.layer = 8;
        }
        else {


            BoxCollider2D thiscollider = GetComponent<BoxCollider2D>() as BoxCollider2D;
            if (transform.parent != null) Zoom = ZoomHeight / (thiscollider.size.y * transform.localScale.y * transform.parent.localScale.y);
            else Zoom = ZoomHeight / (thiscollider.size.y * transform.localScale.y);


            if (Player.cards_in_game.Contains(this))
            {


                foreach (card foundcard in Player.cards_in_game)
                    foundcard.GetComponent<Collider2D>().enabled = false;
            }

            else if (Player.cards_in_hand.Contains(this)) foreach (card foundcard in Player.cards_in_hand) foundcard.GetComponent<Collider2D>().enabled = false;
            else if (Enemy.cards_in_game.Contains(this)) foreach (card foundcard in Enemy.cards_in_game) foundcard.GetComponent<Collider2D>().enabled = false;

            if (transform.position.y <= -2.7f) { IsMovedForZoom = true; old_y = transform.position.y; transform.position = new Vector3(transform.position.x, -2.3f, transform.position.z); }
            if (transform.position.y >= 4f) { IsMovedForZoom = true; old_y = transform.position.y; transform.position = new Vector3(transform.position.x, 3f, transform.position.z); }
        }

        //jesli karta jest obrocona (po ataku) to w przyblizeniu bedzie z powrotem w pozycji poczatkowej
        if (IsTurned == true && !MainMenu.DTA.settings.UseGrid)
        {

            if (transform.parent) transform.parent.Rotate(0, 0, -MainMenu.DTA.settings.OptionTurnDegrees);
            else transform.Rotate(0, 0, -MainMenu.DTA.settings.OptionTurnDegrees);

            IsRotatedForZoom = true;
        }


        Vector3 theScale = transform.localScale;
        if (Application.loadedLevelName == MainMenu.SceneNameEditDeck)
        {
            EditDeckScripts.Zoomed = true;
            theScale.x *= ZoomEditDeckMode;
            theScale.y *= ZoomEditDeckMode;
        }
        else {
            theScale.x *= Zoom;
            theScale.y *= Zoom;

            if (Player.cards_in_game.Contains(this)) foreach (card foundcard in Player.cards_in_game) foundcard.GetComponent<Collider2D>().enabled = true;
            else if (Player.cards_in_hand.Contains(this)) foreach (card foundcard in Player.cards_in_hand) foundcard.GetComponent<Collider2D>().enabled = true;
            else if (Enemy.cards_in_game.Contains(this)) foreach (card foundcard in Enemy.cards_in_game) foundcard.GetComponent<Collider2D>().enabled = true;
        }
        transform.localScale = theScale;


        old_sortingorder = GetComponent<SpriteRenderer>().sortingOrder;


        if (MainMenu.DTA.settings.OptionCardFrameIsSeparateImage)
        {

            GetComponent<SpriteRenderer>().sortingOrder = 101;
            foreach (Transform child in transform) child.GetComponent<Renderer>().sortingOrder = 101;
            transform.Find("CardArt").GetComponent<Renderer>().sortingOrder = 100;
            if (highlight) highlight.GetComponent<Renderer>().sortingOrder = 100;

        }
        else
        {
            GetComponent<SpriteRenderer>().sortingOrder = 100;
            foreach (Transform child in transform) child.GetComponent<Renderer>().sortingOrder = 101;
        }

        IsZoomed = true;

    }

    public IEnumerator WaitBeforeUnZoom()
    {
        yield return new WaitForSeconds(0.1f);
        Player.CanUnzoom = true;
    }

    //powrot karty z przyblizenia
    public void UnZoom()
    {
        if (Player.CanUnzoom)
        {

            gameObject.layer = 0;
            foreach (Transform child in transform) child.gameObject.layer = 0;
            if (IsRotatedForZoom == true)
            {
                if (transform.parent && !MainMenu.DTA.settings.UseGrid) transform.parent.Rotate(0, 0, MainMenu.DTA.settings.OptionTurnDegrees);
                else transform.Rotate(0, 0, MainMenu.DTA.settings.OptionTurnDegrees);
            }
            IsRotatedForZoom = false;

            if (IsMovedForZoom == true)
            {
                transform.position = new Vector3(transform.position.x, old_y, transform.position.z);
            }
            IsMovedForZoom = false;

            Vector3 theScale = transform.localScale;
            if (Application.loadedLevelName == MainMenu.SceneNameEditDeck)
            {
                BoxCollider2D collider = GetComponent<BoxCollider2D>() as BoxCollider2D;
                collider.size = EditDeckScripts.OtherCardsColliderSize;
                collider.offset = EditDeckScripts.OtherCardsColliderCenter;
                EditDeckScripts.Zoomed = false;
                theScale.x /= ZoomEditDeckMode;
                theScale.y /= ZoomEditDeckMode;
            }
            else {
                theScale.x /= Zoom;
                theScale.y /= Zoom;

            }

            if (MainMenu.DTA.settings.OptionCardFrameIsSeparateImage)
            {

                foreach (Transform child in transform) child.GetComponent<Renderer>().sortingOrder = old_sortingorder + 1;
                GetComponent<SpriteRenderer>().sortingOrder = old_sortingorder + 1;
                transform.Find("CardArt").GetComponent<Renderer>().sortingOrder = old_sortingorder;
                if (highlight) highlight.GetComponent<Renderer>().sortingOrder = old_sortingorder;
            }
            else
            {
                GetComponent<SpriteRenderer>().sortingOrder = old_sortingorder;
                foreach (Transform child in transform) child.GetComponent<Renderer>().sortingOrder = old_sortingorder + 1;
            }

            transform.localScale = theScale;


            IsZoomed = false;
            ShowedByEnemy = false;
            mouseHoverSeconds = 0;

        }
    }

    //sprawdza czy karta jest jednostką
    public bool IsACreature()
    {
        if (Type == 1) return true;
        else return false;
    }

    //czy jednostka jest w grze
    public bool IsACreatureInGame()
    {
        if (!Hero && (Enemy.enemy_creatures.Contains(this) || Player.player_creatures.Contains(this))) return true;
        else return false;
    }

    //nieprawidlowy cel
    void BadTarget()
    {
        Player.Warning = "This is not a valid target for this spell";
    }

    //obsluga klikniecia na karte lewym przyciskiem myszy
    public void OnMouseDown()
    {

        //wybor celu jednostki
        switch (Player.NeedTarget)
        {
            case 1:     //wybor jednostki jako celu ataku

                if (Enemy.enemy_creatures.Contains(this)) // jednostka przeciwnika
                {
                    if (MainMenu.DTA.settings.UseGrid)
                    {
                        Slot attacker_slot = Player.AttackerCreature.slot;

                        if (attacker_slot.IsAdjacent(slot) || (Player.AttackerCreature.Ranged && attacker_slot.IsInALine(slot).Count > 0)) AssignTarget();
                        else Player.Warning = "This is not a valid target for attacking";
                    }

                    else AssignTarget();
                }
                else    //jednostka sojusznicza
                {
                    Player.Warning = "This is not a valid target for attacking";
                }

                break;


            case 2: //jednostka przeciwnika lub przeciwnik
                if (Enemy.enemy_creatures.Contains(this)) AssignTarget();

                else BadTarget();
                break;

            //jesli celem jest karta znajdujaca sie w talii lub na cmentarzu
            case 3:
                if (Player.temp_cards.Contains(this))
                { // talia
                    AssignTarget(gameObject);

                    Player.temp_cards.Remove(this);

                    playerDeck.pD.Deck.Remove(Index);

                    Destroy(GameObject.Find("ChooseCardText"));

                    foreach (card foundcard in Player.temp_cards) Destroy(foundcard.gameObject);

                }
                else BadTarget();
                break;
            case 50:
            case 51:
                if (Player.temp_cards.Contains(this))
                { // cmentarz
                    AssignTarget(Player.cards_in_graveyard.Find(x => x.Index == Index).gameObject);


                    foreach (card foundcard in Player.temp_cards) Destroy(foundcard.gameObject);

                }
                else BadTarget();
                break;


            case 4: //dwie jednostki na polu bitwy
                if (IsACreatureInGame())
                {

                    if (Player.targets.Count > 0)
                    {
                        if (Player.targets[0] != gameObject) AssignTargets(2);
                        else BadTarget();
                    }

                    else AssignTargets(2);
                }
                else BadTarget();
                break;

            case 5: //dowolna jednostka
                if (IsACreatureInGame() && !Hero) AssignTarget();

                else BadTarget();
                break;

            case 6: //downolna jednostka ktora atakowala w tej turze
                if (IsACreatureInGame() && this.AttackedThisTurn > 0) AssignTarget();

                else BadTarget();

                break;
            case 40: //sojusznicza jednostka
                if (Player.player_creatures.Contains(this)) AssignTarget();

                else BadTarget();

                break;
            case 41: //jednostka przeciwnika
                if (Enemy.enemy_creatures.Contains(this)) AssignTarget();

                else BadTarget();

                break;
            case 99:  //sojusznicza jednostka o okreslonym poziomie
                if (IsACreature() && Level == Player.CurrentTargetParam && GrowID == Player.CurrentTargetParamString && Player.player_creatures.Contains(this)) AssignTarget();
                else Player.Warning = "You need a level " + Player.CurrentTargetParam + " " + Player.CurrentTargetParamString + " target for this upgrade"; //you need a level 1 Orc target for this upgrade


                break;
            case 7:  //sojuszniczy Bohater
                if (Hero && Player.player_creatures.Contains(this)) AssignTarget();
                else Player.Warning = "You need a hero target for this spell";


                break;

            case 8:
            case 9:
                //sojusznicza jednostka
                if (IsACreature() && Player.player_creatures.Contains(this)) AssignTarget();
                else BadTarget();
                break;

            case 21:
                //celem jest karta w rece gracza jako dodatkowy kosztu
                if (Player.cards_in_hand.Contains(this)) AssignTarget();
                else BadTarget();
                break;
            case 30:
                //jednostka o ataku mniejszym badz rownym X
                if (IsACreatureInGame() && CreatureOffense <= Player.CurrentTargetParam) AssignTarget();
                else BadTarget();
                break;
            case 31:
                //jednostka o koszcie mniejszym lub rownym X
                if (IsACreatureInGame() && Cost.Count <= Player.CurrentTargetParam) AssignTarget();
                else BadTarget();
                break;
            default:
                {
                    if (Player.NeedTarget > 0) return;

                    if (Player.PlayersTurn == false) { return; }

                    if ((ControlledByPlayer == true) && (Player.GameEnded == false))
                    {
                        if (IsZoomed == true) { UnZoom(); }

                        if (MainMenu.DTA.settings.UseGrid && !abilities.DisplayMenu)
                        {
                            if (IsTurned) Player.Warning = "This creature cannot move because it has commited this turn";
                            else if (MovedThisTurn > 1) Player.Warning = "This creature can't move any more this turn";
                            else if (MovedThisTurn > 0 && !extramovement) Player.Warning = "This creature has already moved this turn";
                            else if (FirstTurnSickness()) Player.Warning = "A creature cannot move on its first turn";
                            else
                            {

                                Player.CreaturesZone.StartCoroutine("ChooseSlotAndPlace", this);

                            }

                        }
                        else PlayCard();
                    }

                }
                break;
        }
    }


    //przypisanie celu
    void AssignTarget(GameObject targetgameobject = null)
    {
        if (targetgameobject == null)
            targetgameobject = this.gameObject;
        Player.targets.Add(targetgameobject);
        Player.NeedTarget = 0;

    }
    //przypisanie wiecj niz jednego celu
    void AssignTargets(int targets_needed)
    {

        Player.targets.Add(gameObject);

        if (Player.targets.Count == targets_needed) Player.NeedTarget = 0;

    }
    //obrot karty o 90 stopni
    public void Turn()
    {
        if (!MainMenu.DTA.settings.UseGrid)
        {
            if (transform.parent)
                transform.parent.Rotate(0, 0, MainMenu.DTA.settings.OptionTurnDegrees);
            else
                transform.Rotate(0, 0, MainMenu.DTA.settings.OptionTurnDegrees);
        }

        IsTurned = true;
        checked_for_highlight = false;
    }

    //obrocenie karty z powrotem
    public void UnTurn()
    {
        if (!MainMenu.DTA.settings.UseGrid)
        {
            if (transform.parent)
                transform.parent.Rotate(0, 0, -MainMenu.DTA.settings.OptionTurnDegrees);
            else
                transform.Rotate(0, 0, -MainMenu.DTA.settings.OptionTurnDegrees);
        }
        IsTurned = false;
    }

    //obrocenie karty złota w celu uzyskania Many
    public void TurnLandForMana(bool AI = false)
    {
        Turn();
        ManaColor mana_to_gain;

        mana_to_gain = MainMenu.DTA.settings.colors[0];

        if (AI) Enemy.mana.Add(mana_to_gain);
        else
        {
            Player.mana.Add(mana_to_gain);
        }


    }
    //sprawdz czy gracz jest w stanie zaplacic koszt Many
    bool CanPayCosts(bool potential_mana = false)
    {
        if (CanPayDiscardCost())
        {
            if (potential_mana && CanPayManaCost(potential_mana)) return true; //jesli gracz posiada odpowiednia ilosc kart złota w grze i moze uzyskac wystarczajaco many uzywajac ich
            if (CanPayManaCost()) return true; //gracz posiada odpowiednia ilosc Many w puli

        }

        return false;
    }

    //sprawdz czy gracz ma wystarczajaca ilosc kart w rece aby zaplacic dodatkowy koszt pozbycia sie karty z reki
    bool CanPayDiscardCost()
    {
        if (DiscardCost <= (Player.cards_in_hand.Count + 1))
            return true;
        return false;
    }


    bool CanPayManaCost(bool potential_mana = false)
    {

        if (potential_mana)
        {
            int unturned_lands = 0;
            foreach (card foundland in Player.lands_in_game)
                if (!foundland.IsTurned) unturned_lands++;

            if (Cost.Count <= (Player.mana.Count + unturned_lands)) return true;
        }

        else if (Cost.Count <= Player.mana.Count) return true;

        return false;

    }


    //zagranie karty
    public void PlayCard()
    {
        if (Type == 0)
        { //jesli karta jest kartą złota
            if (Player.cards_in_hand.Contains(this))
            { // i znajduje się w ręce gracza to nastepuje proba przeniesienia jej na pole bitwy


                if (Player.LandsPlayedThisTurn > 0)
                    Player.Warning = "You've already played a land this turn!";

                else if (!Player.LandsZone.CanPlace())
                    Player.Warning = "You don't have enough slots to place a land!";
                else {
                    FromHandLand();

                }
            }

            else if (Player.lands_in_game.Contains(this) && !IsTurned) //jesli karta znajduje się na polu bitwy i nie jest obrocona to uzyj jej
                TurnLandForMana();

        }

        else if (Player.cards_in_hand.Contains(this))
        { //jesli karta jest  w rece gracza i nie jest kartą złota

            if (CanPayManaCost() && CanPayDiscardCost())
            { // jesli gracz jest w stanie zaplacic koszt zagrania karty

                if (Type == 3 && !Player.CreaturesZone.CanPlace())
                    Player.Warning = "You don't have enough slots to place an enchantment!";
                else if (Type == 4 && !Player.CreaturesZone.CanPlace())
                    Player.Warning = "You don't have enough slots to place a secret!";
                else StartCoroutine(PayAdditionalCostAndPlay());
            }
            else Player.Warning = "You don't have enough mana";
        }
        //jesli karta jest jednostką w grze
        else if (IsACreatureInGame())
        {
            TryToAttack();
        }
    }
    //karta weszla do gry w biezacej turze
    public bool FirstTurnSickness()
    {
        if (FirstTurnInGame && !no_first_turn_sickness) return true;
        else return false;

    }
    //sproboj ataku
    public void TryToAttack()
    {
        if (!IsTurned && !FirstTurnSickness() && !cant_attack) CreatureAttack(); //jesli karta jest nie obrocona i nie weszla do gry w biezacej turze to zaatakuj
        else if (cant_attack) Player.Warning = "This creature cannot attack";
        else if (FirstTurnSickness()) Player.Warning = "A creature can't attack the first turn it is in game";
        else if (IsTurned) Player.Warning = "A tapped creature can't attack";
    }

    //atak jest mozliwy
    bool CanAttack()
    {

        if (!IsTurned && !FirstTurnSickness() && !cant_attack) return true;
        return false;

    }

    //odrzuc karte z reki na cmentarz
    public void Discard(bool AI = false)
    {

        if (AI) Enemy.RemoveHandCard(this);
        else Player.RemoveHandCard(this);

        
            MoveToGraveyard();
       
    }

    //zaplac dodatkowe koszty i zagraj akrte
    IEnumerator PayAdditionalCostAndPlay()
    {
        if (DiscardCost > 0 && ValidSpell())
        {
            Player.ActionCancelled = false;
            Player.targets.Clear();

            for (int i = 0; i < DiscardCost; i++)
            {
                Player.NeedTarget = 21; // potrzebna karta na rece aby ja odrzucic

                while (Player.NeedTarget > 0) yield return new WaitForSeconds(0.1f);
                if (Player.ActionCancelled) { yield return false; }
            }
            foreach (GameObject target in Player.targets) //odrzuc karte z reki
                target.GetComponent<card>().Discard();

        }

        // po zaplaceniu dodatkowego kosztu zagraj karte

        if (IsACreature()) StartCoroutine(FromHandCreature());

        else if (Type == 2) FromHandSpell();

        else if (Type == 3 || Type == 4) StartCoroutine(FromHandEnchantment());


    }

    //karta jest odwrocona rewersem do gory
    public void FaceDown()
    {
        foreach (Transform child in transform) child.GetComponent<Renderer>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = playerDeck.pD.cardback;
        faceDown = true;
    }

    //karta jest odworocna awersem do gory
    public void FaceUp()
    {
        foreach (Transform child in transform) child.GetComponent<Renderer>().enabled = true;

        Transform templateTransform = CardTemplate.Instance.transform;


        GetComponent<SpriteRenderer>().sprite = templateTransform.GetComponent<SpriteRenderer>().sprite;

        faceDown = false;
    }
    //ukrycie karty
    public void Hide()
    {
        GetComponent<Renderer>().enabled = false;

        foreach (Transform child in transform)
            child.GetComponent<Renderer>().enabled = false;

    }
    //odkrycie karty
    public void Show()
    {
        GetComponent<Renderer>().enabled = true;

        foreach (Transform child in transform)
            child.GetComponent<Renderer>().enabled = true;

    }

    public bool HasUpgradableCreature(List<card> creatureslist)
    {
        foreach (card foundcard in creatureslist)
        {
            if (foundcard.GrowID == GrowID && foundcard.Level == Level - 1)
                return true;
        }
        return false;
    }

    public void Grow(card upgrade, bool AI = false)
    {

        Index = upgrade.Index;

        GetComponent<Collider2D>().enabled = false;

        foreach (Transform child in transform) Destroy(child.gameObject);
        playerDeck.pD.LoadCardStats(this);

        if (IsTurned)
            if (!MainMenu.DTA.settings.UseGrid)
            {
                if (transform.parent)
                    transform.parent.Rotate(0, 0, -MainMenu.DTA.settings.OptionTurnDegrees);
                else
                    transform.Rotate(0, 0, -MainMenu.DTA.settings.OptionTurnDegrees);
            }

        playerDeck.pD.AddArtAndText(this);

        if (AI)
            Enemy.RemoveHandCard(upgrade);
        else
            Player.RemoveHandCard(upgrade);


        Destroy(upgrade.gameObject);
        GetComponent<Collider2D>().enabled = true;

        if (IsTurned)
            if (!MainMenu.DTA.settings.UseGrid)
            {
                if (transform.parent)
                    transform.parent.Rotate(0, 0, MainMenu.DTA.settings.OptionTurnDegrees);
                else
                    transform.Rotate(0, 0, MainMenu.DTA.settings.OptionTurnDegrees);
            }
    }

    IEnumerator WaitForTargetAndGrow()
    {
        Player.ActionCancelled = false;
        Player.targets.Clear();
        Player.NeedTarget = 99;
        Player.CurrentTargetParam = Level - 1;
        Player.CurrentTargetParamString = GrowID;
        while (Player.NeedTarget > 0) yield return new WaitForSeconds(0.2f);

        if (!Player.ActionCancelled)
        {
            PayManaCost();

            card oldcard = Player.targets[0].GetComponent<card>();

            oldcard.Grow(this);
        }
    }

    //Zagranie karty jednostki z reki
    public IEnumerator FromHandCreature(bool AI = false)
    {

        if (AI == false)
        {   //zagranie gracza


            if (GrowID != "" && Level > 0) // wzmocnienie jednostki
            {
                if (HasUpgradableCreature(Player.player_creatures))

                    StartCoroutine(WaitForTargetAndGrow());

                else Player.Warning = "You don't have a creature to upgrade with this card";
            }
            else // zwykla jednostka
            {

                Zone creaturezone = Player.CreaturesZone;

                Player.WaitForCheck = true;

                creaturezone.StartCoroutine("CheckIfCanPlaceInZone", this);

                while (Player.WaitForCheck) yield return new WaitForSeconds(0.15f);
                if (!Player.ActionCancelled)
                {


                    PayManaCost();
                    Player.RemoveHandCard(this); //usuniecie z reki
                    Player.TriggerCardAbilities(abilities.ON_ENTER_CARDSUBTYPE, Subtype);   //uruchomienie umiejetnosci wraz z wejsciem na pole bitwy jesli karta takie posiada					
                    Player.AddCreature(this); //dodanie na pole bitwy

                }

            }

        }
        else

        {               //zagranie przeciwnika


            PayManaCost(AI);

            Enemy.RemoveHandCard(this); //usuniecie karty z reki

            if (GrowID != "" && Level > 0)
            {
                Enemy.ChooseTargetForUpgrade(this);
                card oldcard = Enemy.targets[0].GetComponent<card>();

                oldcard.Grow(this, AI);
            }
            else

                Enemy.AddCreature(this); //dodanie na pole bitwy

        }


    }


    //zagranie karty złota na pole bitwy
    public void FromHandLand(bool ForPlayer = true)
    {


        if (ForPlayer) //gracz
        {
            Player.LandsPlayedThisTurn += 1;

            Player.RemoveHandCard(this);

            Player.AddLand(this);




        }
        else //przeciwnik zagrywa karte złota
        {

            Enemy.RemoveHandCard(this);
            Enemy.AddLand(this);

        }

        abilities.TriggerAbility(abilities.ON_ENTER, !ForPlayer);

    }


    //oczekuj na wybranie celu i wykonaj efekt
    IEnumerator WaitForTargetAndDoEffect(int Target, int TargetParam, int effect_number)
    {

        Player.targets.Clear(); //wyczysc poprzednie cele

        Player.ActionCancelled = false;
        Player.AttackerCreature = this;
        Player.CurrentTargetParam = TargetParam;
        Player.NeedTarget = Target;

        if (Target == 3) Player.OpenIntListToChooseCard(playerDeck.pD.Deck); //celem jest karta z talii
        else if (Target == 50) Player.OpenListToChooseCard(Player.cards_in_graveyard, 2); //celem jest karta z cmentarza
        else if (Target == 51) Player.OpenListToChooseCard(Player.cards_in_graveyard, 1);  //celem jest jednostka z cmentarza


        while (Player.NeedTarget > 0) yield return 0.5f;



        if (!Player.ActionCancelled)
        {

            EffectManager.AddToStack(false, this, effect_number);
        }
        else  //efekt jest anulowany
        {
            if (Type == 2) { }

            else if (Effects[effect_number].trigger == 1)
            { }

            else if (effect_number == (Effects.Count - 1))
            {
                Player.SpellInProcess = false;
            }
        }


    }



    //autmoatyczny wybor celu
    bool ChooseAutomaticTargetsAndDoEffect(int effect_number, bool AI = false)
    {

        bool TargetIsAutomatic = true;
        Effect effect = Effects[effect_number];


        if (NoTargetEffects.Contains(effect.type))
        {
        }
        else switch (effect.target)
            {


                case 12:    // wszystkie sojusznicze jednostki
                    if (AI)
                        Enemy.targets = Enemy.Creatures();
                    else
                        Player.targets = Player.Creatures();

                    break;
                case 13:    //wszystkie jednostki
                    if (AI) Enemy.targets = EffectManager.CreaturesInGame();

                    else Player.targets = EffectManager.CreaturesInGame();
                    break;
                case 14:    //wszystkie wrogie jednostki
                    if (AI) Enemy.targets = Player.Creatures();

                    else Player.targets = Enemy.Creatures();
                    break;
                case 16:    //wszystkie wrogie jednostkie lub bohaterowie
                    if (AI)
                    {
                        Enemy.targets.Clear();
                        foreach (card foundcard in Player.player_creatures) Enemy.targets.Add(foundcard.gameObject);
                    }
                    else foreach (card foundcard in Enemy.enemy_creatures) Player.targets.Add(foundcard.gameObject);
                    break;
                case 200:   //losowa wroga jednostka
                    if (AI)
                    {
                        Enemy.targets.Clear();
                        Enemy.targets.Add(Player.RandomCreature());
                    }

                    else
                        Player.targets.Add(Enemy.RandomCreature());


                    break;
                case 201:   //od X do Y wrogich jednostek
                    int number_from = effect.targetparam0;
                    int number_to = effect.targetparam1;
                    int number_of_creatures = Random.Range(number_from, number_to + 1); 

                    if (AI)
                        Enemy.targets = EffectManager.RandomCreatures(number_of_creatures, Player.player_creatures);
                    else
                        Player.targets = EffectManager.RandomCreatures(number_of_creatures, Enemy.enemy_creatures);

                    break;
                case 202: //losowa wroga jednostka o koszcie X lub mniej
                    if (AI)
                    {
                        Enemy.targets.Clear();
                        Enemy.targets.Add(Player.RandomCreatureWithCostEqualOrLowerThan(effect.targetparam0));
                    }

                    else Player.targets.Add(Enemy.RandomCreatureWithCostEqualOrLowerThan(effect.targetparam0));

                    break;
                case 203: //losowa wroga jednostka lub bohater
                    if (AI)
                    {
                        Enemy.targets.Clear();
                        Enemy.targets.Add(Player.RandomAlly());
                    }

                    else Player.targets.Add(Enemy.RandomAlly());

                    break;
                case 230: //losowa jednostka sojusznicza
                    if (AI)
                    {
                        Enemy.targets.Clear();
                        Enemy.targets.Add(Enemy.RandomAlly());
                    }

                    else Player.targets.Add(Player.RandomAlly());

                    break;
                case 261: //X losowych jednostek w grze (nie bohaterow)
                    if (AI)
                        Enemy.targets = EffectManager.RandomGameObjects(effect.targetparam0, EffectManager.CreaturesInGame());
                    else
                        Player.targets = EffectManager.RandomGameObjects(effect.targetparam0, EffectManager.CreaturesInGame());
                    break;
                case 300: //losowa jednostka w rece 
                    if (AI)
                    {
                        Enemy.targets.Clear();
                        Enemy.targets.Add(EffectManager.RandomCard(Enemy.cards_in_hand, 1)); 
                    }
                    else
                        Player.targets.Add(EffectManager.RandomCard(Player.cards_in_hand, 1));
                    break;
                case 301: //losowa jednostka w talii
                    if (AI)
                    {
                        Enemy.targets.Clear();
                        Enemy.targets.Add(EffectManager.RandomCardFromIntList(Enemy.Deck));
                    }
                    else
                        Player.targets.Add(EffectManager.RandomCardFromIntList(playerDeck.pD.Deck));
                    break;
                case 302: //losowa jednostka z cmentarza
                    if (AI)
                    {
                        Enemy.targets.Clear();
                        Enemy.targets.Add(EffectManager.RandomCard(Enemy.cards_in_graveyard, 1));
                    }
                    else
                        Player.targets.Add(EffectManager.RandomCard(Player.cards_in_graveyard, 1));
                    break;
                case 303: //losowa karta czaru z cmentarza
                    if (AI)
                    {
                        Enemy.targets.Clear();
                        Enemy.targets.Add(EffectManager.RandomCard(Enemy.cards_in_graveyard, 2)); 
                    }
                    else
                        Player.targets.Add(EffectManager.RandomCard(Player.cards_in_graveyard, 2));
                    break;
                case 304: //losowa karta czaru w talii przeciwnika
                    if (AI)
                    {
                        Enemy.targets.Clear();
                        Enemy.targets.Add(EffectManager.RandomCardFromIntList(playerDeck.pD.Deck));
                    }
                    else
                        Player.targets.Add(EffectManager.RandomCardFromIntList(Enemy.Deck));
                    break;
                case 10: //  gracz aktywny

                    if (AI)
                    {
                        Enemy.targets.Clear();
                        Enemy.targets.Add(GameObject.FindWithTag("Enemy"));
                    }
                    else

                        Player.targets.Add(GameObject.FindWithTag("Player"));


                    break;
                case 11: //przeciwnik
                    if (AI)
                    {
                        Enemy.targets.Clear();
                        Enemy.targets.Add(GameObject.FindWithTag("Player"));
                    }
                    else

                        Player.targets.Add(GameObject.FindWithTag("Enemy"));


                    break;
                case 15: //jednostka celuje w samą siebie

                    if (AI)
                    {
                        Enemy.targets.Clear();
                        Enemy.targets.Add(gameObject);
                    }
                    else
                        Player.targets.Add(gameObject);

                    break;
                case 60: //pierwsza karta z góry talii

                    if (AI)
                    {
                        Enemy.targets.Clear();
                        Enemy.targets.Add(playerDeck.pD.MakeCard(Enemy.Deck[0]).gameObject);
                        Enemy.Deck.RemoveAt(0);
                    }
                    else
                    {
                        Player.targets.Clear();
                        Player.targets.Add(playerDeck.pD.MakeCard(playerDeck.pD.Deck[0]).gameObject);
                        playerDeck.pD.Deck.RemoveAt(0);
                    }
                    break;

                default:
                    TargetIsAutomatic = false;
                    break;
            }

        if (TargetIsAutomatic)
        {
            EffectManager.AddToStack(AI, this, effect_number);
            return true;
        }
        return false;

    }


    public void ApplyEffect(int effect_number, bool AI = false)
    {
        Player.targets.Clear();
        Enemy.targets.Clear();

        int Target = 0;
        int TargetParam = 0;

        Target = Effects[effect_number].target;
        TargetParam = Effects[effect_number].targetparam0;

        int effect = Effects[effect_number].type;


        if (ChooseAutomaticTargetsAndDoEffect(effect_number, AI) == false)
        { //jesli potrzebny cel

            if (AI)
            {

                Enemy.NeedTarget = Target;
                Enemy.CurrentTargetParam = TargetParam;
                Enemy.ChooseTarget(effect);

                EffectManager.AddToStack(AI, this, effect_number);
            }

            else StartCoroutine(WaitForTargetAndDoEffect(Target, TargetParam, effect_number));  //gracz wybiera cel

        }

    }


    //efekty dzialajace w odpowiedzi na czar 
    public void SpellAfterEffects(bool AI = false)
    {

        if (AI)
        {
            if (IsZoomed)
                UnZoom();


            Enemy.TriggerCardAbilities(abilities.ON_SPELL);
            Player.TriggerCardAbilities(abilities.ON_OPPONENT_SPELL);
        }
        else {

            PayManaCost();
            Player.RemoveHandCard(this);

            Player.TriggerCardAbilities(abilities.ON_SPELL);
            Enemy.TriggerCardAbilities(abilities.ON_OPPONENT_SPELL);

        }



        MoveToGraveyard();
       

        Player.SpellInProcess = false;


    }

    //sprawdz czy zagranie karty jest legalne
    bool ValidSpell(bool warning = true)
    {


        int target;
        foreach (Effect foundeffect in Effects)
        {
            target = foundeffect.target;


            if (target == 7 && Effects.Count == 1)
            { //celem jest bohater

                if (!Player.HasAHero())
                {
                    if (warning) Player.Warning = "This card needs a hero for its target";
                    return false;
                }
            }
            else if (target == 203 || target == 16) //losowa wroga jednostka lub bohater
            {
                if (!Enemy.HasACreature() && !Enemy.HasAHero())
                {
                    if (warning) Player.Warning = "There are no enemies to target with this spell";
                    return false;
                }
            }
            else if (target == 202)
            { //losowa wroga jednostka o koszcie <= X

                if (!Enemy.RandomCreatureWithCostEqualOrLowerThan(foundeffect.targetparam0))
                {
                    if (warning) Player.Warning = "There are no creatures to target with this spell";
                    return false;
                }
            }
            else if (target == 200 || target == 201 || target == 14)
            { //losowa wroga jednostka (nie Bohater)

                if (!Enemy.HasACreature())
                {
                    if (warning) Player.Warning = "There are no enemy creatures to target with this spell";
                    return false;
                }
            }
            else if (target == 230)
            { //losowa jednostka sojusznicza
                if (!Player.HasACreature() && !Player.HasAHero())
                {
                    if (warning) Player.Warning = "There are no allies to target with this spell";
                    return false;
                }
            }
            else if (target == 261) //X losowych jednostek
            {
                if (EffectManager.CreaturesInGame().Count < foundeffect.targetparam0)
                {
                    if (warning) Player.Warning = "There are not enough creatures to target with this spell";
                    return false;
                }
            }
            else if (target == 13)
            { //wszystkie jednostki
                if (!Player.HasACreature() && !Enemy.HasACreature())
                {
                    if (warning) Player.Warning = "There are no creatures to target with this spell";
                    return false;
                }
            }
            else if (target == 302)
            {//losowa jednostka z cmentarza
                if (EffectManager.RandomCard(Player.cards_in_graveyard, 1) == null)
                {
                    if (warning) Player.Warning = "You have no creatures in the graveyard to target";
                    return false;
                }
            }
            else if (target == 303) //losowa karta czaru z cmentarza
                if (EffectManager.RandomCard(Player.cards_in_graveyard, 2) == null)
                {
                    if (warning) Player.Warning = "You have no spells in the graveyard to target";
                    return false;
                }

        }
        return true;
    }

    //zaplac koszt many
    public void PayManaCost(bool AI = false)
    {


        int need_colorless = 0;

        List<ManaColor> list_to_use;

        if (AI) list_to_use = Enemy.mana;
        else list_to_use = Player.mana;

        foreach (ManaColor foundcolor in Cost)
            if (foundcolor.name != "colorless")
            {
                list_to_use.Remove(list_to_use.Where(x => x.name == foundcolor.name).First());

            }
            else need_colorless++;

        while (need_colorless > 0)
        {
            list_to_use.RemoveAt(0);
            need_colorless--;
        }
        foreach (card foundcard in Player.cards_in_hand)
            foundcard.checked_for_highlight = false;
    }

    //zagraj karte zaklecia
    public IEnumerator FromHandEnchantment(bool AI = false)
    {

        if (AI)
        {
            PayManaCost(AI);
            Enemy.RemoveHandCard(this);
            Enemy.AddEnchantment(this);

        }
        else { //gracz

            Zone enchzone = Player.CreaturesZone;

            Player.WaitForCheck = true;

            enchzone.StartCoroutine("CheckIfCanPlaceInZone", this);

            while (Player.WaitForCheck) yield return new WaitForSeconds(0.15f);
            if (!Player.ActionCancelled)
            {
                PayManaCost();
                Player.RemoveHandCard(this);
                Player.AddEnchantment(this);

            }
        }

        abilities.TriggerAbility(abilities.ON_ENTER, AI);

    }
    //Zagraj karte czaru z reki 
    public void FromHandSpell(bool AI = false)
    {
        Player.ActionCancelled = false;

        if (AI == true)
        {   //przeciwnik

            PayManaCost(AI);
            Enemy.cards_in_hand.Remove(this);
            Enemy.CardsInHand -= 1;

            StartCoroutine(ShowCardAndWait(true));
        }
        else {          // gracz


            if (ValidSpell())
            {

                if (Effects.Count == 0) SpellAfterEffects();
                else {
                    for (int i = 0; i < Effects.Count; i++) ApplyEffect(i, AI);

                    Player.CanDoStack = true;

                }
            }

            else {

                Player.ActionCancelled = true;
            }
        }


    }

    //pokaz karte
    IEnumerator ShowCardAndWait(bool AI)
    {


        Player.SpellInProcess = true;

        transform.position = new Vector3(0f, 0f, 0f); //pokaz karte graczowi

        if (faceDown) FaceUp();
        if (!GetComponent<Renderer>()) playerDeck.pD.AddArtAndText(this);

        ZoomCard();
        ShowedByEnemy = true;

        yield return new WaitForSeconds(1.3f);

        if (Effects.Count == 0) SpellAfterEffects(AI);

        else {
            for (int i = 0; i < Effects.Count; i++) ApplyEffect(i, AI);

            Player.CanDoStack = true;
        }

    }

    //atakuj cel
    IEnumerator AttackTarget()
    {

        while (Player.NeedTarget > 0) yield return 0.5f;
        if (!Player.ActionCancelled)
        {


            abilities.TriggerAbility(abilities.ON_ATTACK);

            if (AttackedThisTurn > 0 || !free_attack) Turn();
            AttackedThisTurn++;

        }
    }


    //atak celu przez jednostke
    public void CreatureAttack(bool AI = false)
    {

        Player.ActionCancelled = false;
        GameObject ourtarget;
        if (AI == false)
        {

            Player.targets.Clear();
            Player.NeedTarget = 1;
            Player.AttackerCreature = this;
            StartCoroutine(AttackTarget());


        }
        else {      //przeciwnik

            if (MainMenu.DTA.settings.UseGrid)
            {


                ourtarget = Enemy.targets[0];
            }
            else
            {


                ourtarget = Enemy.ChooseTargetForAttacking();
            }



            abilities.TriggerAbility(abilities.ON_ATTACK, true);

            if (AttackedThisTurn > 0 || !free_attack) Turn();
            AttackedThisTurn++;
            Enemy.targets.Clear();
        }


    }

    //cios krytyczny, losowa szansa na podwojenie obrazen
    public bool IsCriticalStrike()
    {
        if (CritChance > 0)
        {
            float rnd = (Random.Range(1, 100));

            if (rnd < CritChance) { return true; }
        }
        return false;
    }

    //brak obrazen
    bool noDamage(card attacker, card target)
    {
        if (attacker.deals_no_combat_dmg || target.takes_no_combat_dmg || (attacker.Ranged && target.no_dmg_from_ranged)) return true;

        return false;
    }


    public void PlayFX(Transform particle)
    {
        Transform newobj = (Transform)Instantiate(particle, transform.position, transform.rotation);
        newobj.GetComponent<Renderer>().sortingOrder = 99;
    }

    //jednostka lub gracz jest atakowana/atakowany
    public void IsAttacked(card Attacker)
    {
        if (Player.player_creatures.Contains(this)) Player.TriggerCardAbilities(abilities.ON_FRIENDLY_ISATTACKED);
        else Enemy.TriggerCardAbilities(abilities.ON_FRIENDLY_ISATTACKED);



        if (Attacker.Ranged)
        {
            PlayFX(playerDeck.pD.firefx);
            GetComponent<AudioSource>().PlayOneShot(HitBySpell);
        }
        else GetComponent<AudioSource>().PlayOneShot(Hit);

        GetComponent<Renderer>().material.color = Color.red;
        Invoke("RestoreColor", 0.3f);


        int DamageToCreature;

        if (noDamage(Attacker, this)) DamageToCreature = 0;
        else
        {
            DamageToCreature = Attacker.CreatureOffense;

            if (Attacker.IsCriticalStrike())
            {
                Player.Warning = "Critical strike!";
                DamageToCreature = (int)(Attacker.CreatureOffense * Attacker.CritDamageMultiplier);
            }

            if (Attacker.DoubleDamage) DamageToCreature = DamageToCreature * 2;

            if (TakesHalfDamage) DamageToCreature = (int)(DamageToCreature / 2);

            if (Attacker.Ranged && less_dmg_from_ranged) DamageToCreature--;

            bool DoNoDamage = false;

           
            if (Defense >= DamageToCreature) DoNoDamage = true;
            

            if (DoNoDamage) DamageToCreature = 0;
            else Defense -= DamageToCreature;


        }

        if (MainMenu.DTA.settings.OptionRetaliate && !noDamage(this, Attacker))
        {

            if (!MainMenu.DTA.settings.UseGrid || Ranged || slot.IsAdjacent(Attacker.slot))
            {
                int DamageToAttacker = CreatureOffense;

                if (IsCriticalStrike())
                    DamageToAttacker = (int)(CreatureOffense * CritDamageMultiplier);

                if (DoubleDamage)
                    DamageToAttacker = DamageToAttacker * 2;

                if (Attacker.TakesHalfDamage)
                    DamageToAttacker = (int)(DamageToAttacker / 2);

                if (Ranged && Attacker.less_dmg_from_ranged) DamageToAttacker--;

                Attacker.Defense -= DamageToAttacker;
            }

        }


        Player.CreatureStatsNeedUpdating = true;

        if (Defense <= 0)
        {   //cel ataku zginal

            if (Player.player_creatures.Contains(Attacker) || Player.cards_in_graveyard.Contains(Attacker)) Attacker.abilities.TriggerAbility(abilities.ON_KILL);

            else Attacker.abilities.TriggerAbility(abilities.ON_KILL, true);
        }

        if (Attacker.Defense <= 0)
        {
            if (Player.player_creatures.Contains(this) || Player.cards_in_graveyard.Contains(this)) abilities.TriggerAbility(abilities.ON_KILL);
            else abilities.TriggerAbility(abilities.ON_KILL, true);
        }

    }



    //gracz badz jednostka jest uleczona
    public void IsHealed(int param)
    {



        if (StartingDefense < Defense + param) Defense = StartingDefense; //wartosc po uleczeniu nie moze przekroczyc wartosci pierwotnej
        else Defense += param;

        PlayFX(playerDeck.pD.healfx);
        GetComponent<AudioSource>().PlayOneShot(Healed);
        Player.CreatureStatsNeedUpdating = true;
    }

    //jednostka lub gracz zostaje obrana za cel czaru
    public void IsHitBySpell(Vector3 param)
    {

        int amount = (int)param.x;
        int damagetype = (int)param.y;
        int cardid = (int)param.z;
        //rodzaje obrazen
        if (damagetype == 0)    //ogien
        {
            PlayFX(playerDeck.pD.firefx);
            GetComponent<AudioSource>().PlayOneShot(HitBySpell);
        }
        if (damagetype == 1)    //fizyczne
        {
            GetComponent<AudioSource>().PlayOneShot(Hit);
            GetComponent<Renderer>().material.color = Color.red;
            Invoke("RestoreColor", 0.3f);
        }
        if (!takes_no_spell_dmg) StartCoroutine(IsDealtSpellDamage(amount, cardid));
    }

    //znajdz karte po ID
    public static card FindCardByID(int id)
    {

        foreach (card enemycard in Enemy.cards_in_game)
        {
            if (enemycard.id_ingame == id)
            {

                return enemycard;
            }

        }
        foreach (card playercard in Player.cards_in_game)
        {
            if (playercard.id_ingame == id)
            {

                return playercard;
            }

        }
        return null;
    }

    //otrzymuje obrazenia od czaru
    IEnumerator IsDealtSpellDamage(int amount, int cardid)
    {

        yield return new WaitForSeconds(0.8f);

        Defense -= amount;
        Player.CreatureStatsNeedUpdating = true;

        if (Defense <= 0 && cardid != -1)
        {
            card effectcard = FindCardByID(cardid);

            if (effectcard.Type == 1)
            {

                if (Player.player_creatures.Contains(effectcard) || Player.cards_in_graveyard.Contains(effectcard)) effectcard.abilities.TriggerAbility(abilities.ON_KILL);
                else effectcard.abilities.TriggerAbility(abilities.ON_KILL, true);
            }
        }


    }

    //metoda zdejmujaca efekt nalozony na avatar gracza/przeciwnika po zadaniu obrazen
    public void RestoreColor()
    {
        GetComponent<Renderer>().material.color = Color.white;
    }
}
