using UnityEngine;

using System.Collections;
using System.Linq;
using System.Collections.Generic;


[System.Serializable]
public class DbCard
{
	
	public List<CustomInt> CustomInts = new List<CustomInt>();
	public List<CustomString> CustomStrings = new List<CustomString>();
	public Sprite art;
	public string name="New card";
	public string text="";

	public string growid="";
	public int id=-1;

	public bool hero = false;
	public bool ranged = false;

	public bool takes_no_combat_dmg = false;
	public bool deals_no_combat_dmg = false;
	public bool no_first_turn_sickness = false;
	public bool cant_attack = false;
	public bool free_attack = false;
	public bool takes_no_spell_dmg = false;

	public bool extramovement = false;
	public bool less_dmg_from_ranged = false;
	public bool no_dmg_from_ranged = false;

	public int type=2;
	public int subtype=-1;

	public ManaColor color;
	public int level=-1;
	
	public List<ManaColor> cost=new List<ManaColor>();
	public int discardcost=-1;
	
	public int offense=-1;
	public int defense=-1;
	
	public AudioClip sfxmove0;
	public AudioClip sfxmove1;
	
	public AudioClip sfxentry; 
	public AudioClip sfxability0; 

	public List<Effect> effects = new List<Effect>();


		
	void Awake()
	{

	}
	
}