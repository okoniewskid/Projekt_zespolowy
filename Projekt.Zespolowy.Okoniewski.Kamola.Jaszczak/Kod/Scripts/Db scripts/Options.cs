using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//opcje dotyczace gry

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
public class Options   {
	
	public  int OptionStartingLife = 20;
	
	//opcje mechaniki gry
	public   List<DBZone> zones = new List<DBZone>(); 
	public   List<DBZone> enemy_zones = new List<DBZone>(); 

	public  int  MaxHandSize = 7;
	public  int  OptionManaMaxIncrement = 10; 

	public List<ManaColor> colors = new List<ManaColor>();

	public bool OptionRetaliate = true;
	
	public  bool OptionGameMusic = false;
	public  bool OptionPlayerTurnPopup = true;
	public  float OptionTurnDegrees = -90;
	
	//opcje widoku kart

	public  bool OptionCardFrameIsSeparateImage = true;

	public bool UseGrid = false;

	public void AddDefaultColors()
	{
		ManaColor colorless = new ManaColor("colorless");
		colorless.Default = true;
		colors.Add(colorless);
	}
    //strefy
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
