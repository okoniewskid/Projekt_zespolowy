using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ManaColor
{

	public string name;
	public Sprite icon;
	public Texture icon_texture;

	public bool Default = false;

	public ManaColor(string m_name="")
	{
		name = m_name;

	}
}

[System.Serializable]
public class CoreOptions   {

	public  int OptionStartingLife = 20;
	
	public   List<DBZone> zones = new List<DBZone>(); 
	public   List<DBZone> enemy_zones = new List<DBZone>(); 

	public  bool OptionFirstTurnSickness = true;
	public  bool OptionGraveyard = true;

	public  int MaxHandSize = 7;

	public  bool OptionManaDoesntReset = false;
	public  bool OptionManaAutoIncrementsEachTurn = false;
	public  int OptionManaMaxIncrement = 10; 

	public bool UseCardColors = false; 
	public bool UseManaColors = false; 

	public List<ManaColor> colors = new List<ManaColor>();


	public  bool OptionCantAttackPlayerThatHasHeroes = false;	
	public  bool OptionGameLostIfHeroDead = false;


	public bool OptionRetaliate = true;
	public bool OptionOneCombatStatForCreatures = false;  
	public bool OptionKillOrDoNothing = false; 
	
	public  bool OptionGameMusic = false;
	public  bool OptionPlayerTurnPopup = true;
	public  float OptionTurnDegrees = -90; 
	

	public  bool OptionCardFrameIsSeparateImage = true;

	public bool UseGrid = false;

	public void AddDefaultColors()
	{
		ManaColor colorless = new ManaColor("colorless");
		colorless.Default = true;
		colors.Add(colorless);
	}

	public void AddDefaultZones()
	{

		DBZone newzone = new DBZone ("Hand");
		newzone.DrawAtTheStartOfGame = 6;
		zones.Add (newzone);
		newzone = new DBZone ("Lands");
		zones.Add (newzone);
		newzone = new DBZone ("Creatures");
		zones.Add (newzone);
		newzone = new DBZone ("Graveyard");
		newzone.UseSlots = newzone.StackAllInOneSlot = true;
		zones.Add (newzone);

		foreach (DBZone zone in zones) {
						zone.Default = true;
						DBZone enemyzone = new DBZone(zone);
						enemyzone.Name = "Enemy " + enemyzone.Name.ToLower();
						enemy_zones.Add (enemyzone);
				}
		newzone = new DBZone ("Grid");
		newzone.UseSlots = true;
		newzone.PlayerCanChooseSlot = true;
		newzone.Shared = true;
		zones.Add (newzone);
	}

	void Start () {

	}

	void Update () {
	
	}
}
