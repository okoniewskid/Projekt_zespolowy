using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class card : MonoBehaviour {

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
	public List<ManaColor> Cost;
	public int Level = 0;
	public string GrowID = "";

	public int DiscardCost = 0; 

	public  int CostInCurrency = 0; 

	public Dictionary<string, int> CustomInts = new Dictionary<string, int>() ; 
	public Dictionary<string, string> CustomStrings = new Dictionary<string, string>() ;

	public GameObject highlight = null;

	public int Type = 0;
	public int Subtype = 0;

	public ManaColor CardColor;

	public int CreatureOffense = 0;
	public int CreatureDefense = 0;
	public int Defense
	{
		get {
			if (MainMenu.TCGMaker.core.OptionOneCombatStatForCreatures) return CreatureOffense;
			else return CreatureDefense;

		}
		protected set {
			if (MainMenu.TCGMaker.core.OptionOneCombatStatForCreatures) CreatureOffense = value;
			else CreatureDefense = value;
			
		}
	}

	public abilities abilities = null;
	public List<Buff> buffs;

	public int CreatureStartingOffense = 0;
	public int CreatureStartingDefense = 0;

	public int StartingDefense
	{
		get {
			if (MainMenu.TCGMaker.core.OptionOneCombatStatForCreatures) return CreatureStartingOffense;
			else return CreatureStartingDefense;
			
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
	public string Name="";


	public int id_ingame;

	public bool Hero = false;

	
	public int Amount;
	public bool FilteredOut = false;
	public bool InCollection = false;


	public bool IsZoomed=false;
	bool IsRotatedForZoom=false;
	bool IsMovedForZoom=false;

	public bool isDragged = false;

	public bool checked_for_highlight = false;

	public static bool WaitABit=false;
	
	float old_y;
	int old_sortingorder;

	
	float mouseHoverSeconds = 0;
	float mouseHoverZoomTime = 1.5f; 

	static List<int> NoTargetEffects = new List<int> {2, 12, 15};

	public Slot slot
	{
		get
		{	if (transform.parent != null)
			return transform.parent.GetComponent<Slot>();

			return null;
		}
	}
	 
	
	void Start () {


		

		Hit = playerDeck.pD.Hit;
		HitBySpell = playerDeck.pD.HitBySpell;
		Healed = playerDeck.pD.Healed;



	}

	void AddHighlight()
	{
		
		highlight = (GameObject) Instantiate(CardTemplate.Instance.transform.Find("Highlight").gameObject);
		

		playerDeck.pD.AssignParentWithLocalPos (highlight, gameObject);
		
	}

	bool IsPlayable()
	{

		if (!Player.PlayersTurn) return false;

		if (Player.cards_in_hand.Contains (this)) {
						if (Type == 0 && Player.LandsPlayedThisTurn == 0) 
								return true;

						if (Type == 1 && CanPayCosts (true)  ) 
							if (Level < 1) return true;	
									else if (HasUpgradableCreature(Player.player_creatures)) return true; 

						

						if (Type > 1 && CanPayCosts (true) && ValidSpell (false)) 
								return true;
				} 
		else if (Player.player_creatures.Contains (this))
				if (CanAttack ())
						return true;

		return false;

	}


	void Update()
	{
		if (!checked_for_highlight && highlight) 
		{
			if (!IsPlayable()) Destroy (highlight);
		}
		else if (!checked_for_highlight && !Player.SpellInProcess && !Player.EffectInProcess && IsPlayable() )
			AddHighlight();

		checked_for_highlight = true;

		if (IsACreature())   
		
			 if (Defense<=0 && !Dead ) 	
			{
				Kill();  

				if (Hero)
					if (ControlledByPlayer) Player.HeroIsDead = true;
						else Enemy.HeroIsDead = true;
			}

	}

	public void RemoveEOTBuffsAndDebuffs ()
	{
		List<Buff> buffs_to_remove = new List<Buff>();

		foreach (Buff foundbuff in buffs)
						if (foundbuff.EOT)
							buffs_to_remove.Add (foundbuff);

		foreach (Buff foundEOTbuff in buffs_to_remove)
			RemoveBuff(foundEOTbuff);
				
	}

	
	public void RemoveBuff (Buff buff_to_remove)
	{
		bool positive = buff_to_remove.positive;
		int param = buff_to_remove.param;

		
		switch (buff_to_remove.type) { 
		case Buff.SET_ATTACK_TO: 	
			CreatureOffense = CreatureStartingOffense;
			break;
		case Buff.RAISE_ATTACK_BY:
			if (!positive) param = -param;
			CreatureOffense -= param;
			break;
		case Buff.MULTIPLY_ATTACK_BY:
			if (!positive) param = 1/param;
			CreatureOffense /= param;
			break;
		
		case Buff.SET_DEFENSE_TO: 	
			CreatureDefense = CreatureStartingDefense;
			break;
		case Buff.RAISE_DEFENSE_BY:
			if (!positive) param = -param;
			CreatureDefense -= param;
			break;
		case Buff.MULTIPLY_DEFENSE_BY:
			if (!positive) param = 1/param;
			CreatureDefense /= param;
			break;
		
		case Buff.SET_CRIT_CHANCE_TO: 	
			CritChance = 0;
			break;
		case Buff.ASSIGN_ABILITY:
			if (!positive) ReturnAbilities();
				else AssignSpecialAbility(param, false);
			break;
			}
		buffs.Remove (buff_to_remove);
	}

	public void ReturnAbilities()
	{
		DbCard dbcard = MainMenu.TCGMaker.cards.Where(x => x.id == Index).SingleOrDefault();
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
		
		if (transform.Find("Description3DText")!=null)
			transform.Find("Description3DText").GetComponent<TextMesh>().text = playerDeck.TextWrap(dbcard.text, 30); 
	}

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

		if (transform.Find("Description3DText")!=null)
			transform.Find("Description3DText").GetComponent<TextMesh>().text = ""; 
	}

	public void AddBuff(bool positive, int param, int BuffType, bool EOT=false, card effectcard=null)
	{
			Debug.Log ("creature is being buffed for:" + param +", buff is positive: "+positive);
			Debug.Log ("BuffType" + BuffType);
			Buff newbuff = new Buff();
			
			switch (BuffType) {
			
				case Buff.SET_ATTACK_TO: 	
						CreatureOffense = param;
						break;
				case Buff.RAISE_ATTACK_BY:
						if (!positive) param = -param;
						CreatureOffense += param;
						break;
				case Buff.MULTIPLY_ATTACK_BY:
						if (!positive) param = 1/param;
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
						if (!positive) param = 1/param;
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
						
			if (effectcard.Type == 3 )newbuff.enchantmentcard = effectcard; 
			buffs.Add(newbuff);
	}

	public void AssignSpecialAbility(int ability_code, bool setTrue = true)
	{

		switch (ability_code)	{
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

		if ( CreatureStartingOffense == CreatureOffense ) transform.Find("Offense3DText").GetComponent<Renderer>().material.SetColor ("_Color", Color.white); 
		else if ( CreatureStartingOffense < CreatureOffense ) transform.Find("Offense3DText").GetComponent<Renderer>().material.SetColor ("_Color", Color.blue); 
		else if  ( CreatureStartingOffense > CreatureOffense ) transform.Find("Offense3DText").GetComponent<Renderer>().material.SetColor ("_Color", Color.red); 

		if (!MainMenu.TCGMaker.core.OptionOneCombatStatForCreatures) {
						transform.Find ("Defense3DText").GetComponent<TextMesh> ().text = CreatureDefense.ToString ();
						if (StartingDefense == Defense)
								transform.Find ("Defense3DText").GetComponent<Renderer>().material.SetColor ("_Color", Color.white);
						else if (StartingDefense < Defense)
								transform.Find ("Defense3DText").GetComponent<Renderer>().material.SetColor ("_Color", Color.blue);
						else if (StartingDefense > Defense)
								transform.Find ("Defense3DText").GetComponent<Renderer>().material.SetColor ("_Color", Color.red); 
				}
	}



	public void Kill()
	{
		Debug.Log ("killing card:"+Name);

		if (ControlledByPlayer) {
						Player.TriggerCardAbilities(abilities.ON_FRIENDLY_DIES);	
						Player.AlliesDestroyedThisTurn++;
						Player.RemoveCreature(this);
				}
		else  {
			Enemy.TriggerCardAbilities(abilities.ON_FRIENDLY_DIES);	
			Enemy.AlliesDestroyedThisTurn++;
			Enemy.RemoveCreature(this);
		}


		if (MainMenu.TCGMaker.core.OptionGraveyard)
						MoveToGraveyard ();
				else 
						Destroy (gameObject);
				
	}

	void MoveToGraveyard()
	{
		
		if (IsTurned) UnTurn();
		Dead = true;


		Debug.Log ("moving to graveyard:" + Name );

		Zone graveyard;
		
		if (ControlledByPlayer) 	{ ControlledByPlayer = false; graveyard = Player.GraveyardZone; }
		else graveyard = Enemy.GraveyardZone;			
		
		graveyard.AddCard (this);

		if (Type == 1) { 
			for (int i = 0; i < buffs.Count; i++)	
				RemoveBuff(buffs[i]);

			CreatureOffense = CreatureStartingOffense; 

			transform.Find("Offense3DText").GetComponent<TextMesh>().text = CreatureOffense.ToString();

			if (!MainMenu.TCGMaker.core.OptionOneCombatStatForCreatures)
			{
				CreatureDefense = CreatureStartingDefense; 
				transform.Find("Defense3DText").GetComponent<TextMesh>().text = CreatureDefense.ToString();
			}

			UpdateCreatureAtkDefLabels(); 
		}


		Destroy (highlight);
	}

	public void RemoveFromGraveyard(bool AI=false)
	{
	
		Dead = false;
		
		Zone graveyard;

		if (AI) 	graveyard = Enemy.GraveyardZone;
		else {
			ControlledByPlayer = true;
			graveyard = Player.GraveyardZone;
		}
		
			
		graveyard.RemoveCard (this);
	}

	void OnMouseOver () 
	{

		if (MainMenu.TCGMaker.core.UseGrid && Player.NeedTarget == 1) 
		{
			
			if (Enemy.cards_in_game.Contains(this))
				if (Player.AttackerCreature.slot.IsAdjacent(slot)) slot.Highlight();
					else if (Player.AttackerCreature.Ranged)
						{
							Debug.Log("target valid if in line");
							List<Slot> foundpath = Player.AttackerCreature.slot.IsInALine(slot);
							
							if (foundpath != null)
								foreach (Slot foundslot in  Player.AttackerCreature.slot.IsInALine(slot)) 
									foundslot.Highlight();

						}


		}

		mouseHoverSeconds += Time.deltaTime;
		
		if (ShowedByEnemy)
			return;
		
		if (IsZoomed == false && mouseHoverSeconds >= mouseHoverZoomTime) 
		{
			ZoomCard ();
		}
		

		 if (Input.GetMouseButtonDown (1)) {
			Debug.Log("right click");

			if (Player.cards_in_game.Contains(this)) { 
			
				Debug.Log("displaying menu"); 
				abilities.DisplayMenu = true;


			} 
		}
		
		else if (Input.GetMouseButtonDown (2)) {

		}
	}

	void OnMouseExit() 
	{

		if (MainMenu.TCGMaker.core.UseGrid && slot != null && !slot.zone.PlayerIsChoosingASlot)
			Player.CreaturesZone.RemoveHighlightedSlots();

		if (ShowedByEnemy)
			return;
		if (IsZoomed == true) { 
				
			Debug.Log("OnMouseExit starting to unzoom if we're allowed");
			UnZoom();
		}
	}


	
	
	void ZoomCard()
	{
		Debug.Log ("zooming card:" + Name);
		if (Application.loadedLevelName == MainMenu.SceneNameEditDeck) {
						
						Player.CanUnzoom = false;
						StartCoroutine(WaitBeforeUnZoom());
						BoxCollider2D collider = GetComponent<BoxCollider2D> () as BoxCollider2D;
						collider.size = EditDeckScripts.FirstCardColliderSize;
						collider.offset = new Vector2 (0, 0);
						foreach (Transform child in transform)
								child.gameObject.layer = 8;  
						gameObject.layer = 8; 
				} else { 		

		
			BoxCollider2D thiscollider = GetComponent<BoxCollider2D> () as BoxCollider2D;
			if (transform.parent !=null) Zoom = ZoomHeight / (thiscollider.size.y * transform.localScale.y * transform.parent.localScale.y);
				else Zoom = ZoomHeight / (thiscollider.size.y * transform.localScale.y); 
			Debug.Log ("zoomheight:"+ZoomHeight +"collider height:"+(thiscollider.size.y * transform.localScale.y));

			if (Player.cards_in_game.Contains (this)) {
								if (Type == 4) RevealSecretCard(); 

								foreach (card foundcard in Player.cards_in_game)
										foundcard.GetComponent<Collider2D>().enabled = false;
									}
				
			else if (Player.cards_in_hand.Contains(this)) foreach (card foundcard in Player.cards_in_hand) foundcard.GetComponent<Collider2D>().enabled = false;
			else if (Enemy.cards_in_game.Contains(this)) foreach (card foundcard in Enemy.cards_in_game) foundcard.GetComponent<Collider2D>().enabled = false;
			

			
			if (transform.position.y <= -2.7f) {IsMovedForZoom = true; old_y = transform.position.y; transform.position=new Vector3 (transform.position.x, -2.3f, transform.position.z);}
			if (transform.position.y >= 4f) {IsMovedForZoom = true; old_y = transform.position.y; transform.position=new Vector3 (transform.position.x, 3f, transform.position.z);}
		}

			
		if (IsTurned == true && !MainMenu.TCGMaker.core.UseGrid) { 
						
			if (transform.parent)  transform.parent.Rotate(0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees); 
				else transform.Rotate(0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees);
				
			IsRotatedForZoom = true; 
		}


		Vector3 theScale = transform.localScale;
		if (Application.loadedLevelName == MainMenu.SceneNameEditDeck) {
			EditDeckScripts.Zoomed = true;
			theScale.x *= ZoomEditDeckMode; 
			theScale.y *= ZoomEditDeckMode; 
		} else {
			theScale.x *= Zoom; 
			theScale.y *= Zoom; 

			if (Player.cards_in_game.Contains(this)) foreach (card foundcard in Player.cards_in_game) foundcard.GetComponent<Collider2D>().enabled = true;
			else if (Player.cards_in_hand.Contains(this)) foreach (card foundcard in Player.cards_in_hand) foundcard.GetComponent<Collider2D>().enabled = true;
			else if (Enemy.cards_in_game.Contains(this)) foreach (card foundcard in Enemy.cards_in_game) foundcard.GetComponent<Collider2D>().enabled = true;
		}
		transform.localScale = theScale;


		old_sortingorder = GetComponent<SpriteRenderer> ().sortingOrder ;
	

		if (MainMenu.TCGMaker.core.OptionCardFrameIsSeparateImage) 
		{	 

			GetComponent<SpriteRenderer> ().sortingOrder = 101; 
			foreach (Transform child in transform) child.GetComponent<Renderer>().sortingOrder = 101; 
			transform.Find ("CardArt").GetComponent<Renderer>().sortingOrder = 100; 
			if (highlight) highlight.GetComponent<Renderer>().sortingOrder = 100; 

		}
		else 
		{
			GetComponent<SpriteRenderer> ().sortingOrder = 100; 
			foreach (Transform child in transform) child.GetComponent<Renderer>().sortingOrder = 101; 
		}
		
		IsZoomed = true;
		
	}

	public IEnumerator WaitBeforeUnZoom()
	{
		yield return new WaitForSeconds (0.1f);
		Player.CanUnzoom = true;
	}

	public void UnZoom()
	{
		
		if (Player.CanUnzoom) {
		
			Debug.Log("unzooming card:" + Name);

			if (Player.cards_in_game.Contains (this) && Type == 4) HideSecretCard(); 

			gameObject.layer = 0; 
			foreach (Transform child in transform)	child.gameObject.layer = 0;  
			if (IsRotatedForZoom == true) {
				if (transform.parent && !MainMenu.TCGMaker.core.UseGrid)  transform.parent.Rotate(0, 0, MainMenu.TCGMaker.core.OptionTurnDegrees); 
				else transform.Rotate(0, 0, MainMenu.TCGMaker.core.OptionTurnDegrees);
			}
			IsRotatedForZoom = false;
			
			if (IsMovedForZoom == true) {
				transform.position = new Vector3 (transform.position.x, old_y, transform.position.z);
			}
			IsMovedForZoom = false;
			
			Vector3 theScale = transform.localScale;
			if (Application.loadedLevelName == MainMenu.SceneNameEditDeck) {
				BoxCollider2D collider = GetComponent<BoxCollider2D> () as BoxCollider2D;
				collider.size = EditDeckScripts.OtherCardsColliderSize; 
				collider.offset = EditDeckScripts.OtherCardsColliderCenter;
				EditDeckScripts.Zoomed = false;
				theScale.x /= ZoomEditDeckMode; 
				theScale.y /= ZoomEditDeckMode; 
			} else {
				Debug.Log("scaling back after zoom");
				theScale.x /= Zoom; 
				theScale.y /= Zoom; 
				
			}

			if (MainMenu.TCGMaker.core.OptionCardFrameIsSeparateImage) 
				{	 
					
					foreach (Transform child in transform) child.GetComponent<Renderer>().sortingOrder = old_sortingorder + 1; 
					GetComponent<SpriteRenderer> ().sortingOrder = old_sortingorder + 1;
					transform.Find ("CardArt").GetComponent<Renderer>().sortingOrder = old_sortingorder; 
				if (highlight) highlight.GetComponent<Renderer>().sortingOrder = old_sortingorder; 
				}
			else 
				{
					GetComponent<SpriteRenderer> ().sortingOrder = old_sortingorder;
					foreach (Transform child in transform) child.GetComponent<Renderer>().sortingOrder = old_sortingorder + 1; 
				}

			transform.localScale = theScale;


			IsZoomed = false;
			ShowedByEnemy = false;
			mouseHoverSeconds = 0; 
			
		} 
		
	}

	public bool IsACreature() 
	{
		if (Type == 1) return true;
		else return false;
	}


	public bool IsACreatureOrHeroInGame()
	{
		if (Enemy.enemy_creatures.Contains(this) || Player.player_creatures.Contains (this)) return true;
		else return false;
	}

	public bool IsACreatureInGame()
	{
		if (!Hero && (Enemy.enemy_creatures.Contains(this) || Player.player_creatures.Contains (this))) return true;
		    else return false;
	}

	void BadTarget()
	{
		Debug.Log ("bad target");
		Player.Warning = "To nie jest poprawny cel.";
	}

	public void OnMouseDown() {
		if (CustomStrings.ContainsKey("flavor text"))
		Debug.Log ("flavor text: "+CustomStrings["flavor text"]);

		if (Application.loadedLevelName == MainMenu.SceneNameEditDeck)  
						EditDeckScripts.MoveCard (this);
	
		else if (Application.loadedLevelName == MainMenu.SceneNameMainMenu) 
		{
			if (CostInCurrency <= Currency.PlayerCurrency) 
			{

			Currency.DoBuyCard(Index);
			Currency.GetCurrency(); 
			Debug.Log("bought a card!");
			MainMenu.message = "Bought a card!";
			MainMenu.CollectionNeedsUpdate = true;
			}
			else { Debug.Log("can't afford"); MainMenu.message = "You don't have enough silver"; }  
		}
		else 
		{
		
		
			Debug.Log("card mousedown, needtarget: "+Player.NeedTarget );
			switch (Player.NeedTarget) { 
			case 1: 	
			
					if (Enemy.enemy_creatures.Contains(this)) 
					    {
							if (MainMenu.TCGMaker.core.UseGrid) 
							{
								Slot attacker_slot = Player.AttackerCreature.slot;
								
							if (attacker_slot.IsAdjacent(slot) || (Player.AttackerCreature.Ranged && attacker_slot.IsInALine(slot).Count > 0) ) AssignTarget();
										else Player.Warning = "To nie jest poprawny cel ataku."; 
							}
					   
						else AssignTarget(); 
					}	
					else 	
					{
						Debug.Log ("bad target");
						Player.Warning = "To nie jest poprawny cel ataku.";
					}


			break;

		
			case 2: 
				if (Enemy.enemy_creatures.Contains(this) ) AssignTarget(); 

				else BadTarget();
			break;

				
			case 3:
				if (Player.temp_cards.Contains(this)) {
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
				if (Player.temp_cards.Contains(this)) { 
					AssignTarget (Player.cards_in_graveyard.Find(x => x.Index == Index).gameObject);

					Destroy(GameObject.Find("ChooseCardText"));
				
					foreach (card foundcard in Player.temp_cards) Destroy(foundcard.gameObject);	 
				
				}
				else BadTarget();
				break;
	

			case 4: 
				if ( IsACreatureInGame() ) { 

					if (Player.targets.Count>0)	{
						if (Player.targets[0] != gameObject) AssignTargets(2);
						else BadTarget();
					}

					else AssignTargets(2);
				}
				else BadTarget();
			break;
			
			case 5: 
				if (IsACreatureInGame() && !Hero) 		AssignTarget();

				else BadTarget();
			break;
			
			case 6: 
				if (IsACreatureInGame() && this.AttackedThisTurn > 0) AssignTarget();
						
					else BadTarget();

			break;
			case 40: 
				if (Player.player_creatures.Contains(this) && IsACreatureOrHeroInGame()) AssignTarget();
				
				else BadTarget();
				
				break;
			case 41: 
				if (Enemy.enemy_creatures.Contains(this) && IsACreatureOrHeroInGame()) AssignTarget();
				
				else BadTarget();
				
				break;
			case 99:  
				if (IsACreature() && Level == Player.CurrentTargetParam && GrowID == Player.CurrentTargetParamString && Player.player_creatures.Contains (this)) AssignTarget();
				else Player.Warning = "You need a level "+Player.CurrentTargetParam+" "+ Player.CurrentTargetParamString +" target for this upgrade"; 
				
				
				break;
			case 7:  
				if (Hero && Player.player_creatures.Contains (this)) AssignTarget();
					else Player.Warning = "You need a hero target for this spell";
					

			break;
			
			case 8:
			case 9:
					
				if (IsACreature() && Player.player_creatures.Contains (this)) AssignTarget();
					else BadTarget();
			break;		
			
			case 21:
				
				if (Player.cards_in_hand.Contains (this)) AssignTarget();
				else BadTarget();
			break;		
			case 30:
				
				if (IsACreatureInGame() && CreatureOffense <= Player.CurrentTargetParam) AssignTarget();
				else BadTarget();
			break;
			case 31:
				 
				if (IsACreatureInGame() && Cost.Count <= Player.CurrentTargetParam) AssignTarget();
				else BadTarget();
				break;	
			default:
			{
				if (Player.NeedTarget >0 ) return;	
				
				if (Player.PlayersTurn == false) {	Debug.Log ("not your turn!");	return; } 
				
				if ((ControlledByPlayer == true)&&(Player.GameEnded==false)) 
				{
					if (IsZoomed == true) { UnZoom(); }

					if (IsACreatureOrHeroInGame() && MainMenu.TCGMaker.core.UseGrid && !abilities.DisplayMenu)
					{
						if (IsTurned)Player.Warning = "Ten stwor wykonal juz akcje w tej turze";
							else if (MovedThisTurn>1) Player.Warning = "This creature can't move any more this turn";
								else if (MovedThisTurn>0 && !extramovement) Player.Warning = "This creature has already moved this turn";
									else if (FirstTurnSickness()) Player.Warning = "Stwor nie moze zostac uzyty w turze w ktorej wszedl do gry";
										else  
											{	
												
												Player.CreaturesZone.StartCoroutine("ChooseSlotAndPlace", this);
												
											}

					}
					else PlayCard ();
				}

			}
			break;
			}



		} 
		
	}



	void AssignTarget(GameObject targetgameobject = null)
	{
		if (targetgameobject == null)
						targetgameobject = this.gameObject;
		Player.targets.Add(targetgameobject);
		Debug.Log ("First target of player: " + Player.targets [0]);
		Player.NeedTarget = 0;

	}

	void AssignTargets(int targets_needed)	
	{

			Player.targets.Add(gameObject);
		
		if (Player.targets.Count == targets_needed) Player.NeedTarget = 0;
		
	}

	public void Turn()
	{
		if (!MainMenu.TCGMaker.core.UseGrid) {
			if (transform.parent)
				transform.parent.Rotate (0, 0, MainMenu.TCGMaker.core.OptionTurnDegrees);
			else
				transform.Rotate (0, 0, MainMenu.TCGMaker.core.OptionTurnDegrees); 
		}

		IsTurned = true;
		checked_for_highlight = false;
	}

	public void UnTurn()
	{
		if (!MainMenu.TCGMaker.core.UseGrid) {
			if (transform.parent)
				transform.parent.Rotate (0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees);
			else
				transform.Rotate (0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees); 
		}
		IsTurned = false;
	}

	void SendCard()
	{
		Logic.ScenePhotonView.RPC("SendPlayedCard", PhotonTargets.Others, id_ingame); 
	}


	void SendEffect(int effect_number)
	{
		Logic.ScenePhotonView.RPC("SendEffect", PhotonTargets.Others, effect_number, id_ingame); 
	}

	public void SendHandCard()
	{
		Debug.Log ("sending played hand card:" + Name);
		Logic.ScenePhotonView.RPC("SendPlayedHandCard", PhotonTargets.Others, Index, id_ingame); 
	}

	public void SendUpgradeCreature()
	{
		Debug.Log ("sending creature upgrade:" + Name);
		Logic.ScenePhotonView.RPC("SendUpgradeCreature", PhotonTargets.Others, Index); 
	}

	public void PlayEnemyCardMultiplayer()
	{

		if (Type == 0) 	TurnLandForMana(true); 

		else if (IsACreature ()) CreatureAttack(true);

	}

	public void PlayEnemyHandCardMultiplayer()
	{
		
				if (Type == 0)
						FromHandLand (false);
				else if (Type == 1)
						StartCoroutine(FromHandCreature (true));
				else if (Type == 2)
						FromHandSpell (true);
				else if (Type == 3 || Type == 4)
						StartCoroutine(FromHandEnchantment (true));
		
	}

	public void TurnLandForMana(bool AI=false)
	{
		Debug.Log("tapping land for mana:"+CardColor.name);
		Turn();
		ManaColor mana_to_gain;

		if (MainMenu.TCGMaker.core.UseManaColors)
			mana_to_gain = CardColor;
		else
			mana_to_gain = MainMenu.TCGMaker.core.colors [0]; 

		if (AI)	Enemy.mana.Add(mana_to_gain);
			else 
		{
			Player.mana.Add(mana_to_gain);
			if (MainMenu.IsMulti) SendCard(); 
		}


	}

	bool CanPayCosts(bool potential_mana=false)
	{
		if (CanPayDiscardCost ())
		{
			if (potential_mana && CanPayManaCost (potential_mana)) return true; 
			if (CanPayManaCost()) return true; 

		}
						
		return false;
	}

	bool CanPayDiscardCost()
	{
		if (DiscardCost <= (Player.cards_in_hand.Count + 1))
						return true;
		return false;
	}

	bool CanPayManaCost(bool potential_mana=false)
	{

		if (MainMenu.TCGMaker.core.UseManaColors)
				{
				List<ManaColor> temp = new List<ManaColor>();
				
				if (potential_mana) {
									foreach (card foundland in Player.lands_in_game) 
										if (!foundland.IsTurned)	temp.Add(foundland.CardColor);
									}
				foreach (ManaColor foundmana in Player.mana) 
					temp.Add(foundmana);


				int need_colorless = 0;
				foreach (ManaColor foundcolor in Cost)
				{
					if (foundcolor.name != "colorless")
					{
						ManaColor can_pay = temp.Where(x => x.name == foundcolor.name).FirstOrDefault();
						if (can_pay != null)	temp.Remove(can_pay);
						else return false;
					}
					else need_colorless++;
				}

				if (need_colorless <= temp.Count)return true;

				}
		else if (potential_mana)
			{
				int unturned_lands = 0;
				foreach (card foundland in Player.lands_in_game) 
					if (!foundland.IsTurned) unturned_lands++;

				if (Cost.Count <= (Player.mana.Count + unturned_lands)) return true;
			}

		else if (Cost.Count <= Player.mana.Count) return true;

		return false;
		
	}



	public void PlayCard()
	{	

		Debug.Log ("trying to play: "+Name );


	if (Type == 0) {
		if (Player.cards_in_hand.Contains(this)) { 
			
			
			if (Player.LandsPlayedThisTurn > 0) 
					Player.Warning = "Zagrales juz karte zasobu w tej turze!";
			
				else if (!Player.LandsZone.CanPlace())
					Player.Warning = "Za malo miejsca na zasoby!";
			else {
					FromHandLand ();
					if (MainMenu.IsMulti)  SendHandCard(); 
				} 	
		}
		
			else if (Player.lands_in_game.Contains(this) && !IsTurned) 
				TurnLandForMana();
		
	}
	
		else if (Player.cards_in_hand.Contains(this)) { 
		Debug.Log("Cost"+Cost);
		if (CanPayManaCost() && CanPayDiscardCost()) { 
				 
				if (Type == 3  && !Player.CreaturesZone.CanPlace())
					Player.Warning = "You don't have enough slots to place an enchantment!";
				else if (Type == 4  && !Player.CreaturesZone.CanPlace())
					Player.Warning = "You don't have enough slots to place a secret!";
				else StartCoroutine(PayAdditionalCostAndPlay());
		}
			else Player.Warning = "Za malo zlota w skarbcu";
	}
		
		else if (IsACreatureOrHeroInGame())
			{ 
				TryToAttack();
			}
}

	public bool FirstTurnSickness()
	{
		if (MainMenu.TCGMaker.core.OptionFirstTurnSickness)
			if (FirstTurnInGame && !no_first_turn_sickness) return true;
		return false;
	}

	public void TryToAttack()
	{
		if (!IsTurned && !FirstTurnSickness() && !cant_attack) CreatureAttack();
				else if (cant_attack) Player.Warning = "Ten stwor nie moze atakowac";
					else if (FirstTurnSickness()) Player.Warning = "Stwor nie moze atakowac w turze w ktorej wszedl do gry";
						else if (IsTurned) Player.Warning = "Uzyty stwor nie moze atakowac";
	}

	bool CanAttack()
	{
			
		if (!IsTurned && !FirstTurnSickness() && !cant_attack) return true;
		return false;
	
	}

	public void Discard(bool AI = false) 
	{

		if (AI) Enemy.RemoveHandCard(this);
				else Player.RemoveHandCard(this);

		if (MainMenu.TCGMaker.core.OptionGraveyard)	
		{
			
			MoveToGraveyard();
		}
		else 	Destroy (gameObject);
	}

	IEnumerator PayAdditionalCostAndPlay()
	{		
		if (DiscardCost > 0 && ValidSpell()) 
		{
			Player.ActionCancelled = false;
			Player.targets.Clear();
			Debug.Log("this card has an additional discard cost");
						for (int i = 0; i < DiscardCost; i++) 
								{
									Player.NeedTarget = 21; 
									
									while (Player.NeedTarget > 0) 		yield return new WaitForSeconds (0.1f);
									if (Player.ActionCancelled) { Debug.Log("action cancelled"); return false; }
								}
			foreach (GameObject target in Player.targets) 
					target.GetComponent<card>().Discard();

		}

		

		if (IsACreature()) 	StartCoroutine(FromHandCreature()); 
			
		else if (Type == 2) FromHandSpell();  
		
		else if (Type == 3 || Type == 4)  StartCoroutine(FromHandEnchantment());  
	

	}

	public void FaceDown()
	{
		foreach (Transform child in transform) child.GetComponent<Renderer>().enabled = false;
		GetComponent<SpriteRenderer> ().sprite = playerDeck.pD.cardback;
		faceDown = true;
	}


	public void FaceUp()
	{
		foreach (Transform child in transform)	child.GetComponent<Renderer>().enabled = true;

		Transform templateTransform = CardTemplate.Instance.transform;

		if (templateTransform.Find(CardColor.name+"Frame"))
			GetComponent<SpriteRenderer> ().sprite = templateTransform.Find(CardColor.name+"Frame").GetComponent<SpriteRenderer> ().sprite;

		faceDown = false;
	}

	public void Hide()
	{
		GetComponent<Renderer>().enabled = false;

		foreach (Transform child in transform)
			child.GetComponent<Renderer>().enabled = false;
		
	}

	public void Show()
	{
		GetComponent<Renderer>().enabled = true;
		
		foreach (Transform child in transform)
			child.GetComponent<Renderer>().enabled = true;
		
	}

	public void HideSecretCard()
	{
		transform.Find ("CardArt").GetComponent<SpriteRenderer>().sprite = playerDeck.pD.secretart;

		foreach (Transform child in transform)
			if (child.name!="CardArt") child.GetComponent<Renderer>().enabled = false;
		 
	}


	public void RevealSecretCard()
	{
		transform.Find ("CardArt").GetComponent<SpriteRenderer>().sprite = Art ;
		foreach (Transform child in transform)
						child.GetComponent<Renderer>().enabled = true;

	}

	public bool HasUpgradableCreature(List<card> creatureslist)
	{
		foreach (card foundcard in creatureslist) {
			Debug.Log("foundcard, growid:"+GrowID+"level:"+foundcard.Level);
						if (foundcard.GrowID == GrowID && foundcard.Level == Level-1)
								return true;
				}
		return false;
	}

	public void Grow(card upgrade, bool AI=false)
	{
		
		Index = upgrade.Index;

		
		GetComponent<Collider2D>().enabled = false;

		foreach (Transform child in transform) Destroy(child.gameObject); 
		playerDeck.pD.LoadCardStats(this);

		if (IsTurned)	
			if (!MainMenu.TCGMaker.core.UseGrid) {
				if (transform.parent)
					transform.parent.Rotate (0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees);
				else
					transform.Rotate (0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees); 
			}

		playerDeck.pD.AddArtAndText (this);

		if (AI)
						Enemy.RemoveHandCard (upgrade);
				else
						Player.RemoveHandCard (upgrade);


		Destroy (upgrade.gameObject);
		GetComponent<Collider2D>().enabled = true;

		if (IsTurned)	
			if (!MainMenu.TCGMaker.core.UseGrid) {
				if (transform.parent)
					transform.parent.Rotate (0, 0, MainMenu.TCGMaker.core.OptionTurnDegrees);
				else
					transform.Rotate (0, 0, MainMenu.TCGMaker.core.OptionTurnDegrees); 
			}
	}

	IEnumerator WaitForTargetAndGrow()
	{
		Player.ActionCancelled = false;
		Player.targets.Clear ();
		Player.NeedTarget = 99;
		Player.CurrentTargetParam = Level-1;
		Player.CurrentTargetParamString = GrowID;
		while (Player.NeedTarget > 0)	yield return new WaitForSeconds (0.2f);

		if (!Player.ActionCancelled)
		{
			PayManaCost();

			card oldcard = Player.targets[0].GetComponent<card>();
			if (MainMenu.IsMulti) {
				Player.SendTargets();
				SendUpgradeCreature();
			}
			oldcard.Grow(this);
		}
	}

	public IEnumerator FromHandCreature(bool AI=false) {
		
		Debug.Log("playing a creature");

		if (AI == false) {	

													
						if (GrowID!="" && Level > 0 )
						{
							if (HasUpgradableCreature(Player.player_creatures)) 
																	
								StartCoroutine(WaitForTargetAndGrow());
														
							else Player.Warning = "You don't have a creature to upgrade with this card";
						}
						else 
						{	
							
							Zone creaturezone = Player.CreaturesZone;
							
							Player.WaitForCheck = true;
							
							creaturezone.StartCoroutine("CheckIfCanPlaceInZone",this);				
							
							while (Player.WaitForCheck) yield return new WaitForSeconds(0.15f);			
							if (!Player.ActionCancelled) 	
										{	
											if (MainMenu.IsMulti)  SendHandCard(); 
											
											PayManaCost();
											Player.RemoveHandCard(this); 
											Player.TriggerCardAbilities(abilities.ON_ENTER_CARDSUBTYPE, Subtype);						
											Player.AddCreature(this);
																	
										}
																	
						} 
						
				} else 
		
			{ 				

				
				PayManaCost(AI);
			
				Enemy.RemoveHandCard(this); 
				
				if (GrowID!="" && Level > 0 ) 
					{
						Enemy.ChooseTargetForUpgrade(this);
						card oldcard = Enemy.targets[0].GetComponent<card>();
						
						oldcard.Grow(this, AI);
					}
				else 
			

				Enemy.AddCreature(this);
			
		}

	
	}



	public void FromHandLand(bool ForPlayer=true) 	{ 

		Debug.Log("FromHandLand, ForPlayer:"+ForPlayer);
			if (ForPlayer)
	{
			Player.LandsPlayedThisTurn +=1;

			Player.RemoveHandCard(this);

			Player.AddLand(this);

		

	
	}
	else 
	{
			Debug.Log("playsland");
			Debug.Log("Index:"+Index);
		

			Enemy.RemoveHandCard(this); 
			Enemy.AddLand(this);
	
	}
	
		abilities.TriggerAbility (abilities.ON_ENTER, !ForPlayer);

	}


	IEnumerator WaitForMultiplayerTargetAndDoEffect(int effect_number)
	{
		Debug.Log ("waiting for enemy to send target ienum...");
				

		while (Enemy.NeedTarget==100)	 yield return 0.3f;  
		
		
		EffectManager.AddToStack(true, this, effect_number);
	}


	IEnumerator WaitForTargetAndDoEffect(int Target, int TargetParam, int effect_number)
	{
		Debug.Log ("WaitForTargetAndDoEffect start");
		Player.targets.Clear (); 

		Player.ActionCancelled = false;
		Player.AttackerCreature = this; 
		Player.CurrentTargetParam = TargetParam;
		Player.NeedTarget = Target; 

		if (Target == 3) Player.OpenIntListToChooseCard(playerDeck.pD.Deck); 
		else if (Target == 50) 	Player.OpenListToChooseCard (Player.cards_in_graveyard, 2);	
		else if (Target == 51) Player.OpenListToChooseCard (Player.cards_in_graveyard, 1);	


		while (Player.NeedTarget>0)	 yield return 0.5f;  
				


		if (!Player.ActionCancelled) {
						if (MainMenu.IsMulti) SendTargetsAndEffect(effect_number);
											
						
						EffectManager.AddToStack(false, this, effect_number);
									}
		else  
		{
			Debug.Log("choosing target... spell got cancelled");
			if (Type == 2 ) {} 

			else  if (Effects[effect_number].trigger == 1) 
			{}

			else if (effect_number == (Effects.Count-1)) 
			{
				if ( Type == 4 ) SecretAfterEffects(); 
				else Player.SpellInProcess = false; 
			}
		}


		}


	public void SendTargetsAndEffect(int effect_number)
	{

			Debug.Log ("sending targets and effect, card: "+ Name);
			Player.SendTargets ();

		if (IsACreature())	
			{
				if (Effects[effect_number].trigger == 1 )	
					 SendEffect(effect_number); 
			}

			else if (Type!=3 && Type!=4) SendHandCard (); 
			

	}

	bool ChooseAutomaticTargetsAndDoEffect(int effect_number, bool AI=false) 
	{

		bool TargetIsAutomatic = true;
		Effect effect = Effects[effect_number];
		Debug.Log("trying to choose automatic target, effect target:"+effect.target+"AI:"+AI);

		if (NoTargetEffects.Contains(effect.type))
			 {
			Debug.Log("no target effect");
				if (MainMenu.IsMulti){
					if (IsACreature() && effect.trigger != 0)
						SendEffect (effect_number);	
				
					else if (Type == 2)	SendHandCard (); 
				}
			}
			

		else switch (effect.target) { 
				
					
				case 12: 	
						if (AI)
								Enemy.targets = Enemy.Creatures ();
						else
								Player.targets = Player.Creatures ();
				
						break;
				case 13:	
						if (AI) Enemy.targets = EffectManager.CreaturesInGame();
								
						else 	Player.targets = EffectManager.CreaturesInGame();
								break;
				case 14:	
						if (AI) Enemy.targets = Player.Creatures ();
					
					else 	Player.targets = Enemy.Creatures ();
					break;
				case 16:	
					if (AI) 
							{
								Enemy.targets.Clear();
								foreach (card foundcard in Player.player_creatures) Enemy.targets.Add(foundcard.gameObject);
							}		
					else 	foreach (card foundcard in Enemy.enemy_creatures) Player.targets.Add(foundcard.gameObject);
					break;
				case 200:	
						if (AI)
							{	Enemy.targets.Clear();
								Enemy.targets.Add(Player.RandomCreature());
							}
							
						else 
								Player.targets.Add(Enemy.RandomCreature());
							
						
						break;
				case 201:	
						int number_from = effect.targetparam0; 
						int number_to = effect.targetparam1; 
						int number_of_creatures = Random.Range (number_from, number_to + 1); 
			
						if (AI)
								Enemy.targets = EffectManager.RandomCreatures (number_of_creatures, Player.player_creatures);
						else
								Player.targets = EffectManager.RandomCreatures (number_of_creatures, Enemy.enemy_creatures);
				
						break;
				case 202: 
						if (AI) 				
							{	Enemy.targets.Clear();
								Enemy.targets.Add(Player.RandomCreatureWithCostEqualOrLowerThan(effect.targetparam0));
							}
								
						else	Player.targets.Add(Enemy.RandomCreatureWithCostEqualOrLowerThan(effect.targetparam0));
								
						break;
				case 203: 
					if (AI) 				
					{	Enemy.targets.Clear();
						Enemy.targets.Add(Player.RandomAlly());
					}
					
					else	Player.targets.Add(Enemy.RandomAlly());
					
					break;
				case 230: 
					if (AI){
								Enemy.targets.Clear();		
								Enemy.targets.Add(Enemy.RandomAlly());
							}
							
						else	Player.targets.Add(Player.RandomAlly());
							
						break;
				case 261: 
					if (AI)
						Enemy.targets = EffectManager.RandomGameObjects(effect.targetparam0, EffectManager.CreaturesInGame());
					else
						Player.targets = EffectManager.RandomGameObjects(effect.targetparam0, EffectManager.CreaturesInGame());
					break;
				case 300: 
					if (AI){
						Enemy.targets.Clear();	
						Enemy.targets.Add(EffectManager.RandomCard(Enemy.cards_in_hand, 1)); 
							}
					else
						Player.targets.Add(EffectManager.RandomCard(Player.cards_in_hand, 1));
					break;
				case 301: 
			if (AI){
						Enemy.targets.Clear();	
						Enemy.targets.Add(EffectManager.RandomCardFromIntList(Enemy.Deck));
					}
					else
						Player.targets.Add(EffectManager.RandomCardFromIntList(playerDeck.pD.Deck));
					break;
				case 302: 
				if (AI){
						Enemy.targets.Clear();	
						Enemy.targets.Add(EffectManager.RandomCard(Enemy.cards_in_graveyard, 1)); 
						}
					else
						Player.targets.Add(EffectManager.RandomCard(Player.cards_in_graveyard, 1));
					break;
				case 303: 
					if (AI){
						Enemy.targets.Clear();
						Enemy.targets.Add(EffectManager.RandomCard(Enemy.cards_in_graveyard, 2)); 
					}
					else
						Player.targets.Add(EffectManager.RandomCard(Player.cards_in_graveyard, 2));
					break;
				case 304: 
						if (AI){
						Enemy.targets.Clear();	
						Enemy.targets.Add(EffectManager.RandomCardFromIntList(playerDeck.pD.Deck));
						}
					else
						Player.targets.Add(EffectManager.RandomCardFromIntList(Enemy.Deck));
					break;
				case 10: 
		
						if (AI)
								{
									Enemy.targets.Clear();
									Enemy.targets.Add (GameObject.FindWithTag ("Enemy"));
								}
						else
								
									Player.targets.Add (GameObject.FindWithTag ("Player"));
								
				
						break;
				case 11: 
						if (AI)
						{
							Enemy.targets.Clear();
							Enemy.targets.Add (GameObject.FindWithTag ("Player"));
						}
						else
						
							Player.targets.Add (GameObject.FindWithTag ("Enemy"));
						
						
						break;
				case 15: 

						if (AI)
								{
									Enemy.targets.Clear();
									Enemy.targets.Add (gameObject);
								}
						else				
									Player.targets.Add (gameObject);
								
								break;
				case 60: 
					
					if (AI)
					{
						Enemy.targets.Clear();
						Enemy.targets.Add (playerDeck.pD.MakeCard(Enemy.Deck[0]).gameObject);
						Enemy.Deck.RemoveAt(0);
					}
					else
					{
						Player.targets.Clear();
						Player.targets.Add (playerDeck.pD.MakeCard(playerDeck.pD.Deck[0]).gameObject);
						playerDeck.pD.Deck.RemoveAt(0);
					}
					break;
	
				default:
						TargetIsAutomatic = false;
						break;
				}
		
	if (TargetIsAutomatic)
			{
				
				if (!AI && MainMenu.IsMulti) SendTargetsAndEffect(effect_number); 
				
				EffectManager.AddToStack(AI, this, effect_number);
				return true;
			}
	return false;
	
	}


	public void ApplyEffect(int effect_number, bool AI=false)
	{
		Player.targets.Clear();
		if (!MainMenu.IsMulti) Enemy.targets.Clear();
		Debug.Log ("effect number: " + effect_number);	

		int Target = 0;
		int TargetParam = 0;

		Target = Effects[effect_number].target;
		TargetParam = Effects[effect_number].targetparam0;

		int effect = Effects[effect_number].type;
		Debug.Log ("EnemyNeedTarget:" + Enemy.NeedTarget);

		if (MainMenu.IsMulti && Enemy.NeedTarget == 100) StartCoroutine(WaitForMultiplayerTargetAndDoEffect (effect_number)); 

		else if (ChooseAutomaticTargetsAndDoEffect(effect_number, AI) == false) { 
					
				if (AI) {
							Debug.Log("Effect Target:" + Target);
								
							 if (!MainMenu.IsMulti) {	
										Enemy.NeedTarget = Target;
										Enemy.CurrentTargetParam = TargetParam;
										Enemy.ChooseTarget (effect);
													}		
							 EffectManager.AddToStack(AI, this, effect_number);
						} 
	
			else StartCoroutine (WaitForTargetAndDoEffect (Target, TargetParam, effect_number));	

			}

	}
	

	public void SecretAfterEffects(bool AI=false) 
	{
		

		RevealSecretCard ();

		if (AI)		Enemy.RemoveEnchantment(this);
		else 	Player.RemoveEnchantment(this);

		Debug.Log ("after effects");
		
		if (MainMenu.TCGMaker.core.OptionGraveyard) MoveToGraveyard();
		else Destroy (gameObject);
		
		Player.SpellInProcess = false;
		
		
	}
	
	public void SpellAfterEffects(bool AI=false)
	{
		Debug.Log ("starting spell aftereffects");
		
			if (AI) {
						if (IsZoomed)
								UnZoom ();
					

						Enemy.TriggerCardAbilities(abilities.ON_SPELL);
						Player.TriggerCardAbilities(abilities.ON_OPPONENT_SPELL);
					} 
			else {

			PayManaCost();
			Player.RemoveHandCard(this);

			Player.TriggerCardAbilities(abilities.ON_SPELL);
			Enemy.TriggerCardAbilities(abilities.ON_OPPONENT_SPELL);

				}
	
			
			
		if (MainMenu.TCGMaker.core.OptionGraveyard) MoveToGraveyard();
			else Destroy (gameObject);

			Player.SpellInProcess = false;


	}


	bool ValidSpell(bool warning = true)	
	{


		int target;
		foreach (Effect foundeffect in Effects)
			 {
				target = foundeffect.target;
				

				if (target == 7 && Effects.Count == 1) { 

								if (!Player.HasAHero ()) {
										if (warning) Player.Warning = "This card needs a hero for its target";
										return false;
								}
					}
			else if (target == 203 || target == 16) 
			{
				if (!Enemy.HasACreature() && !Enemy.HasAHero()){
					if (warning) Player.Warning = "Nie ma celu dla tej karty";
					return false;
				}
			}
			else if (target == 202) { 
							
				if (!Enemy.RandomCreatureWithCostEqualOrLowerThan(foundeffect.targetparam0)) {
					if (warning) Player.Warning = "Nie ma celu dla tej karty";
					return false;
				}
			}
				else if (target == 200 || target == 201 || target == 14 ){ 
				
								if (!Enemy.HasACreature ()) {
										if (warning) Player.Warning = "Nie ma celu dla tej karty";
										return false;
									}
					}
				else if (target == 230) { 
								if (!Player.HasACreature () && !Player.HasAHero()) {
									if (warning) Player.Warning = "Nie ma celu dla tej karty";
									return false;
								}
				}
				else if (target == 261) 
				{
					if (EffectManager.CreaturesInGame().Count < foundeffect.targetparam0) {
					if (warning) Player.Warning = "Nie ma celu dla tej karty";
						return false;
					}
				}
				else if (target == 13) { 
								if (!Player.HasACreature () && !Enemy.HasACreature()) {
								if (warning) Player.Warning = "Nie ma celu dla tej karty";
									return false;
								}
				}
				else if (target == 302) {
							if (EffectManager.RandomCard(Player.cards_in_graveyard, 1) == null) {
								if (warning) Player.Warning = "Nie ma celu dla tej karty";
								return false;
							}
				}
				else if (target == 303) 
				if (EffectManager.RandomCard(Player.cards_in_graveyard, 2) == null) {
					if (warning) Player.Warning = "Nie ma celu dla tej karty";
					return false;
				} 
						
				}
		return true;
	}

	public void PayManaCost (bool AI=false)
	{
		Debug.Log("paying mana cost for card: "+Name+" AI:"+AI);

		int need_colorless = 0;

		List<ManaColor> list_to_use;

		if (AI) list_to_use = Enemy.mana;
			else list_to_use = Player.mana;

		foreach (ManaColor foundcolor in Cost)
		if (foundcolor.name!="colorless"){
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

	public IEnumerator FromHandEnchantment(bool AI=false)	
	{

		if (Type == 4) 	HideSecretCard (); 


		if (AI) {
			PayManaCost(AI);
			Enemy.RemoveHandCard(this);
			Enemy.AddEnchantment(this);

				} 
		else { 

			Zone enchzone = Player.CreaturesZone;
			
			Player.WaitForCheck = true;
			
			enchzone.StartCoroutine("CheckIfCanPlaceInZone",this);				
			
			while (Player.WaitForCheck) yield return new WaitForSeconds(0.15f);			
			if (!Player.ActionCancelled) 	
			{	
				if (MainMenu.IsMulti)  SendHandCard(); 
				PayManaCost();
				Player.RemoveHandCard(this);
				Player.AddEnchantment(this);

			}
				}

		abilities.TriggerAbility (abilities.ON_ENTER, AI);

	}

	public void FromHandSpell(bool AI=false)	{
		Player.ActionCancelled = false;
	
		if (AI == true) {	

			PayManaCost(AI);
			Enemy.cards_in_hand.Remove (this);
			Enemy.CardsInHand -= 1;
			

			StartCoroutine(ShowCardAndWait(true));
				}
		else {			


				if (ValidSpell()) 
				{
					
					if (Effects.Count == 0) SpellAfterEffects(); 
						else {
								for(int i = 0; i< Effects.Count; i++) ApplyEffect(i, AI);
									
								Player.CanDoStack = true;
								Debug.Log("Player.CanDoStack:"+Player.CanDoStack);
							}
				}
				 
			else { 
					Debug.Log ("can't cast this spell");
					Player.ActionCancelled = true; 
				}
			}
				
		
	}

	IEnumerator ShowCardAndWait(bool AI)
	{	
		Debug.Log("starting ShowCardAndWait");

		Player.SpellInProcess = true;

		transform.position = new Vector3 (0f, 0f, 0f); 
	
		if (faceDown)	FaceUp ();
		if (!GetComponent<Renderer>()) playerDeck.pD.AddArtAndText (this); 
		Debug.Log ("art scale:" + transform.Find ("CardArt").localScale.y);
		ZoomCard ();
		ShowedByEnemy = true;
	
		yield return new WaitForSeconds (1.3f);

		if (Effects.Count == 0) SpellAfterEffects (AI);	
						
			else {
					for (int i = 0; i< Effects.Count; i++) ApplyEffect (i, AI);
				
					Player.CanDoStack = true;
				}

	}


	IEnumerator AttackTarget()
	{

		while (Player.NeedTarget>0) yield return 0.5f; 
		if (MainMenu.IsMulti && !Player.ActionCancelled) { 
			Player.SendTargets();
			SendCard();

		} 

		if(!Player.ActionCancelled) {

			Player.targets[0].SendMessage("IsAttacked", this);
			abilities.TriggerAbility (abilities.ON_ATTACK); 

			if (AttackedThisTurn>0 || !free_attack) Turn();
			AttackedThisTurn++;

		}
	}



	public void CreatureAttack(bool AI=false)
	{
				Debug.Log ("gonna attack");
				Player.ActionCancelled = false;
				GameObject ourtarget;
				if (AI == false) {

						Player.targets.Clear();
						Player.NeedTarget = 1;
						Player.AttackerCreature = this;
						StartCoroutine (AttackTarget ());
						
					
				} else {		
					
						if (MainMenu.IsMulti || MainMenu.TCGMaker.core.UseGrid) { 

								Debug.Log ("enemy is attacking with " + Name + ", target:" + Enemy.targets[0].name);
								ourtarget = Enemy.targets[0];
											}	 
							else
								{												
								Debug.Log ("AI is attacking");
								
								ourtarget = Enemy.ChooseTargetForAttacking (); 
								}
				
						
						ourtarget.SendMessage ("IsAttacked", this);
						abilities.TriggerAbility (abilities.ON_ATTACK, true); 

						if (AttackedThisTurn>0 || !free_attack) Turn();
						AttackedThisTurn++;
						Enemy.targets.Clear ();
				}


		}

	 public bool IsCriticalStrike()
	{
		if (CritChance > 0)
			{	Debug.Log ("CritChance>0" + CritChance);
				float rnd =  (Random.Range(1,100));
			Debug.Log ("rnd:" + rnd);
				if (rnd < CritChance) {Debug.Log ("critical strike!"); return true; }
		}
		return false;
	}

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

	public void IsAttacked (card Attacker) 
	{
		if (Player.player_creatures.Contains(this)) Player.TriggerCardAbilities(abilities.ON_FRIENDLY_ISATTACKED);
			else Enemy.TriggerCardAbilities(abilities.ON_FRIENDLY_ISATTACKED);

	
		Debug.Log ("a creature "+Name+" is attacked, processing damage..");
		if (Attacker.Ranged) {	
								PlayFX(playerDeck.pD.firefx);
								GetComponent<AudioSource>().PlayOneShot(HitBySpell);
							}
			else 	GetComponent<AudioSource>().PlayOneShot (Hit);
				
		GetComponent<Renderer>().material.color = Color.red;
		Invoke ("RestoreColor", 0.3f); 


		int DamageToCreature; 

		if (noDamage(Attacker, this)) DamageToCreature = 0;
			else 
		{
			DamageToCreature = Attacker.CreatureOffense;

			if (Attacker.IsCriticalStrike ()) {
							Player.Warning = "Critical strike!";
							DamageToCreature = (int)(Attacker.CreatureOffense * Attacker.CritDamageMultiplier);
											}

			if (Attacker.DoubleDamage) DamageToCreature = DamageToCreature * 2;

			if (TakesHalfDamage) 	DamageToCreature = (int)(DamageToCreature / 2);

			if (Attacker.Ranged && less_dmg_from_ranged) DamageToCreature--;

			bool DoNoDamage = false;

			if (MainMenu.TCGMaker.core.OptionKillOrDoNothing) 
					{
						if (Defense >= DamageToCreature) DoNoDamage = true; 
					}

			if (DoNoDamage)	DamageToCreature = 0;
				else Defense -= DamageToCreature;

			Debug.Log("dealt "+DamageToCreature+" damage, attacker offense:"+Attacker.CreatureOffense);
		}

		if (MainMenu.TCGMaker.core.OptionRetaliate && !noDamage(this, Attacker)) {
						
			if (!MainMenu.TCGMaker.core.UseGrid || Ranged || slot.IsAdjacent(Attacker.slot) )
							{
								int DamageToAttacker = CreatureOffense;	

								if (IsCriticalStrike ())
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

		if (Defense <=0 ) {	

			if (Player.player_creatures.Contains(Attacker) || Player.cards_in_graveyard.Contains(Attacker))		Attacker.abilities.TriggerAbility (abilities.ON_KILL);

			else Attacker.abilities.TriggerAbility (abilities.ON_KILL, true);
				}

		if (Attacker.Defense <=0 ) {
			if (Player.player_creatures.Contains(this) || Player.cards_in_graveyard.Contains(this))		abilities.TriggerAbility (abilities.ON_KILL);
			else abilities.TriggerAbility (abilities.ON_KILL, true);
				}
	
	}



	
	public void IsHealed (int param)
	{
		Debug.Log ("healing");

		Debug.Log ("CreatureStartingDefense: "+StartingDefense);
		Debug.Log ("param: "+param);
		if (StartingDefense < Defense + param ) Defense = StartingDefense; 
			else Defense += param;
		 
		PlayFX(playerDeck.pD.healfx); 
		GetComponent<AudioSource>().PlayOneShot(Healed);
		
		Player.CreatureStatsNeedUpdating = true;
	}

	public void IsHitBySpell (Vector3 param) 
	{
		Debug.Log ("creature is hit by spell");
		int amount = (int)param.x;
		int damagetype = (int)param.y;
		int cardid = (int)param.z;

		if (damagetype == 0)	
		{
			PlayFX(playerDeck.pD.firefx); 
			GetComponent<AudioSource>().PlayOneShot(HitBySpell);
		}
		if (damagetype == 1)	
		{
			GetComponent<AudioSource>().PlayOneShot (Hit);
			GetComponent<Renderer>().material.color = Color.red;
			Invoke ("RestoreColor", 0.3f); 
		}
		if (!takes_no_spell_dmg) StartCoroutine (IsDealtSpellDamage (amount, cardid));
	}



	IEnumerator IsDealtSpellDamage (int amount, int cardid)
	{
		Debug.Log ("creature is dealt damage:"+amount);
		yield return new WaitForSeconds(0.8f);

		Defense -= amount;
		Player.CreatureStatsNeedUpdating = true;

		if (Defense <=0 && cardid != -1)
		{
			card effectcard = Logic.FindCardByID(cardid);

			if (effectcard.Type == 1) 
				{ 
					Debug.Log ("triggering OnKill on creature:" +effectcard.Name); 
					if (Player.player_creatures.Contains(effectcard) || Player.cards_in_graveyard.Contains(effectcard))		effectcard.abilities.TriggerAbility (abilities.ON_KILL);
						else effectcard.abilities.TriggerAbility (abilities.ON_KILL, true);
				}
		}


	}


	public void RestoreColor()
	{
		GetComponent<Renderer>().material.color = Color.white; 
	}
}
