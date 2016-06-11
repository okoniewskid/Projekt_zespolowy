using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class abilities : MonoBehaviour {
	
	public int Trigger0; 

	card thiscard;

	public bool DisplayMenu = false;

	
	public  const int ON_ENTER = 0;
	public  const int ON_ACTIVATE = 1;
	public  const int ON_ATTACK = 2;
	public  const int ON_KILL = 3;

	
	public  const int ON_SPELL = 20; 
	public  const int ON_OPPONENT_SPELL = 21; 
	public  const int ON_FRIENDLY_DIES = 22; 
	public  const int ON_FRIENDLY_ISATTACKED = 23; 

	public  const int ON_START_OF_YOUR_TURN = 30; 
	public  const int ON_END_OF_YOUR_TURN = 31; 
	public  const int ON_ENTER_CARDSUBTYPE = 50; 

	List<Texture2D> cost_textures = new List<Texture2D>();

	int[] activated_abilities; 

	public void TriggerAbility(int ability_trigger, bool AI=false, int subtype = -1) 
	{
		int i = 0;
		bool foundability = false;
		
		foreach (Effect effect in thiscard.Effects) {

			
			if (effect.trigger == ability_trigger && (ability_trigger!=ON_ENTER_CARDSUBTYPE || subtype == effect.triggerparam0)) 
			{
				foundability = true;
				Debug.Log("found triggered effect, card:" + thiscard.Name + ", effect: " + effect.type);
				Player.SpellInProcess = true;
				if (thiscard.Secret) { 
					
					thiscard.RevealSecretCard();
				}
				thiscard.ApplyEffect (i, AI);
			}
			i++;
		}
		if (foundability) Player.CanDoStack = true;
		
	}

	public void Awake()
	{

		thiscard = GetComponent<card> ();
		UpdateActivatedAbilities();
	}

	void UpdateActivatedAbilities()
	{
	
		int arraylength=0;
		foreach (Effect effect in thiscard.Effects) {
		
				if (effect.trigger == ON_ACTIVATE) arraylength++;

		}


		int[] temparray = new int[arraylength];
		int i = 0;
		int j = 0;

		foreach (Effect effect in thiscard.Effects)	{

		
					if (effect.trigger == ON_ACTIVATE)  { temparray[j] = i; j++; }
						i++;
				
			}
			
		activated_abilities = temparray;			
	}


	void CloseMenu()
	{
		Debug.Log ("closing menu");
		DisplayMenu = false;
		Player.DisplayingCreatureMenu = false;
		cost_textures.Clear();
	}

	void CombineIcons(int i, Sprite sprite_to_add)
	{
		Debug.Log("combine icons, i:"+i);
		Texture2D texture_to_add = MainMenu.SpriteToTexture(sprite_to_add);

		if (cost_textures.Count == i) 
			cost_textures.Add(texture_to_add);


		else {
			Texture2D newtex = new Texture2D((int)(cost_textures[i].width+texture_to_add.width+1), cost_textures[i].height+1);
			Color[] pixels = cost_textures[i].GetPixels();
			
			newtex.SetPixels(0, 0, cost_textures[i].width, cost_textures[i].height, pixels); 


			Color[] pixels2 = texture_to_add.GetPixels();

			newtex.SetPixels(cost_textures[i].width, 0, texture_to_add.width, texture_to_add.height, pixels2);
			newtex.Apply (); 

			cost_textures[i] = newtex;
		}
	}

	public void OnGUI()
	{

		if (DisplayMenu)
		{
			Player.DisplayingCreatureMenu = true;
			Vector3 p = Camera.main.WorldToScreenPoint(transform.position);

			int Cost=0;
			int GridOffset = 0;

			if (thiscard.IsACreatureOrHeroInGame() && MainMenu.TCGMaker.core.UseGrid && !thiscard.IsTurned) 
				{ 
				GridOffset++;
				
				if (GUI.Button(new Rect(p.x+20, Screen.height-p.y , 270, 30), "Attack"))
					{
						thiscard.TryToAttack();
						
						CloseMenu();
					}
				}
			string ability_text;

			for (int i=0; i<activated_abilities.Length; i++)
			{
				Effect effect = thiscard.Effects[activated_abilities[i]];
				Cost = effect.cost.Count;

				if (effect.name == null) ability_text = "Uzyj umiejetnosc";
					else if (effect.name == "") ability_text =  "Uzyj umiejetnosc";
						else  ability_text =  effect.name.ToString();

				GUIContent content = new GUIContent(); 

				if (MainMenu.TCGMaker.core.UseManaColors)
				{
					
					if (cost_textures.Count == 0)	
						foreach (ManaColor foundcolor in effect.cost)
								if (foundcolor.icon) 
									{
										CombineIcons(i, foundcolor.icon);
									}
											

					if (cost_textures.Count > 0) content.image = cost_textures[i];
				}
				else if (Cost != 0)  ability_text +=  ", cost: " + Cost;  


				content.text = ability_text;

				if (GUI.Button(new Rect(p.x+20, Screen.height-p.y + (i+GridOffset)*30 , 270, 30), content)) 
				{
				
					if (thiscard.IsTurned) 
					{
						if (thiscard.IsACreature())Player.Warning = "Ten stwor zostal uzyty w tej turze";
						else Player.Warning = "Ten stwor zostal uzyty w tej turze";
					}
					else if (thiscard.FirstTurnSickness()) Player.Warning = "Stwor nie moze uzywac umiejetnosci w turze w ktorej wszedk do gry";
					else if (Cost <= Player.mana.Count) 
					{	
						if (Cost>0)Player.mana.RemoveAt(0);
						thiscard.ApplyEffect (activated_abilities[i], false);
						Player.CanDoStack = true;
					
					}
					else Player.Warning = "Nie masz wystarczajaco zlota w skarbcu";
					CloseMenu();
				}
			}
		
			if (activated_abilities.Length + GridOffset >0 || thiscard.GrowID!="") 
			{
				if (GUI.Button(new Rect(p.x+20,Screen.height-p.y+(activated_abilities.Length+GridOffset)*30,270,30), "Anuluj")) DisplayMenu = false;
			}	
				else CloseMenu();

		}
	}

	public void OnEnter(bool AI=false)
	{
		int i = 0;

		if (AI)	Debug.Log ("starting enemy onenter, creature" + thiscard.Name);
		bool foundability = false;
		foreach (Effect effect in thiscard.Effects) {
			int target = effect.target;
			Debug.Log ("found effect");

				if (effect.trigger == ON_ENTER) 
				{
					foundability = true;
					Player.SpellInProcess = true;
					Debug.Log("found onenter");

					if (AI && MainMenu.IsMulti){
						if 	( effect.type == 13 || effect.type == 5 )	{ Debug.Log ("waiting for enemy to send target"); Enemy.NeedTarget = 100; } 
						else if ( target == 2 || target == 5 || target == 8 || target == 9 || target == 201)	{ Debug.Log ("waiting for enemy to send target"); Enemy.NeedTarget = 100; } 				
					}
					thiscard.ApplyEffect (i, AI);
				}
				i++;
		}
		if (foundability) Player.CanDoStack = true;
	}
	
	void Start () {
	
	}
}
