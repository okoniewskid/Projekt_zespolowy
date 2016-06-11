using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AITurn : MonoBehaviour{
	static public AITurn instance; 

	bool OK_to_do_next_AI_action = false; 

	void Awake(){ 
		instance = this; 
	}

	IEnumerator TryToAttack()
	{
		while (Player.SpellInProcess) 	yield return new WaitForSeconds(0.2f); 
		Enemy.targets.Clear ();
		Debug.Log("starting trytoattack");
	
		foreach (card foundcreature in Enemy.enemy_creatures) {   
			Enemy.targets.Clear();

			Debug.Log("enemy creature: "+foundcreature.Name);

			if (foundcreature.IsTurned == false && foundcreature.FirstTurnSickness() == false){ 


				if (MainMenu.TCGMaker.core.UseGrid)
				{				
					foreach (card playercreature in Player.player_creatures)

						if (!playercreature.Hero || foundcreature.Hero)
						{
							if (foundcreature.CreatureDefense - playercreature.CreatureOffense <= 0) 
									if (!foundcreature.Ranged || playercreature.Ranged) break;
										if (foundcreature.slot.IsAdjacent(playercreature.slot)) break;
						}	
										
					
						
						 else if (foundcreature.slot.IsAdjacent(playercreature.slot)) 
							{
									if (foundcreature.Hero && !playercreature.Hero) break;
									
									yield return new WaitForSeconds(1f); 
									Debug.Log("found adjacent creature");
									Enemy.targets.Add(playercreature.gameObject);
									foundcreature.CreatureAttack(true); 
									break;
							}
						else if (foundcreature.Ranged && foundcreature.slot.IsInALine(playercreature.slot) != null)
							
							{	
								if ((foundcreature.Hero && !playercreature.Hero) && playercreature.Ranged) break;
								yield return new WaitForSeconds(1f); 
								Enemy.targets.Add(playercreature.gameObject);
								foundcreature.CreatureAttack(true); 
								break;
							}

				}
				else { 
					yield return new WaitForSeconds(1f); 
					foundcreature.CreatureAttack(true); 
					break;
					}
				
			}
			
		}
		yield return new WaitForSeconds(0.1f);
		Debug.Log ("did creature AI actions, ok to do next");
		OK_to_do_next_AI_action = true;
	}


	IEnumerator AIPlays()
	{	
		OK_to_do_next_AI_action = false;
		instance.StartCoroutine("PayCostAndPlay");
		while (!OK_to_do_next_AI_action) 	yield return new WaitForSeconds(0.2f); 
		OK_to_do_next_AI_action = false;
		instance.StartCoroutine("TryToAttack");
		while (!OK_to_do_next_AI_action) 	yield return new WaitForSeconds(0.2f); 
		OK_to_do_next_AI_action = false;
		instance.StartCoroutine("TryToAttack");
		while (!OK_to_do_next_AI_action) 	yield return new WaitForSeconds(0.2f); 
		OK_to_do_next_AI_action = false;
		instance.StartCoroutine("TryToAttack");
		while (!OK_to_do_next_AI_action) 	yield return new WaitForSeconds(0.2f); 
		OK_to_do_next_AI_action = false;
		instance.StartCoroutine("PayCostAndPlay");
		while (!OK_to_do_next_AI_action) 	yield return new WaitForSeconds(0.2f); 
		OK_to_do_next_AI_action = false;
		instance.StartCoroutine("TryToAttack");
		while (!OK_to_do_next_AI_action) 	yield return new WaitForSeconds(0.2f); 

		Enemy.TriggerCardAbilities(abilities.ON_END_OF_YOUR_TURN);

		if (!Player.GameEnded) {
			Enemy.EnemyTurn = false;
			Player.PlayersTurn = true; 	
			Player.NewTurn ();
		}
	}

	bool CanPayManaCost(card card_to_check)
	{
		
		if (MainMenu.TCGMaker.core.UseManaColors)
		{
			List<ManaColor> temp = new List<ManaColor>();

			foreach (ManaColor foundmana in Enemy.mana)	
					temp.Add(foundmana);

			foreach (card foundland in Enemy.lands_in_game) 
					if (!foundland.IsTurned)temp.Add(foundland.CardColor);

			int need_colorless = 0;

			foreach (ManaColor foundcolor in card_to_check.Cost)
			{
				if (foundcolor.name != "colorless")
				{
				ManaColor can_pay = temp.Where(x => x.name == foundcolor.name).FirstOrDefault();
				if (can_pay != null)	temp.Remove(can_pay);
				else return false;
				}
				else need_colorless++;
			}
			
			if (need_colorless <= temp.Count) return true;
			
		}
		else {
			int potential_mana = Enemy.mana.Count;
			foreach (card foundland in Enemy.lands_in_game) 
				if (!foundland.IsTurned) potential_mana++;

			if (card_to_check.Cost.Count <= potential_mana) return true;
		}	
		return false;
	}
	IEnumerator PayCostAndPlay()
	{
		
		List<card> templist = new List<card>(Enemy.cards_in_hand);
		
		foreach (card foundcard in templist) {

			while (Player.SpellInProcess) 	yield return new WaitForSeconds(0.2f); 
		
			if (foundcard.Type != 0 && CanPayManaCost(foundcard) && foundcard.DiscardCost <= (Enemy.CardsInHand+1) && CheckCard(foundcard)) 

			{
		
					Debug.Log ("AI plays card: "+foundcard.Name);

					List<ManaColor> LeftToTurn = new List<ManaColor>();
					foreach (ManaColor foundcost in foundcard.Cost)
						LeftToTurn.Add(foundcost);

					foreach (ManaColor unspentmana in Enemy.mana)	
					if (LeftToTurn.Any(x => x.name == unspentmana.name)) LeftToTurn.Remove(LeftToTurn.Where(x => x.name == unspentmana.name).First()); 
					
					yield return new WaitForSeconds(0.25f); 
					
					if (LeftToTurn.Count > 0)foreach (card foundland in Enemy.lands_in_game) { 

					 	if (!foundland.IsTurned) 
							if(!MainMenu.TCGMaker.core.UseManaColors || LeftToTurn.Any(x => x.name == foundland.CardColor.name))
							{
								yield return new WaitForSeconds(0.3f); 
								foundland.TurnLandForMana(true);
								if (MainMenu.TCGMaker.core.UseManaColors)	LeftToTurn.Remove(LeftToTurn.Where(x => x.name == foundland.CardColor.name).First()); 
									else LeftToTurn.RemoveAt(0);
								if (LeftToTurn.Count == 0) { yield return new WaitForSeconds(0.5f); break; } 
							}
						}

					for (int i=0; i<foundcard.DiscardCost; i++) {		
						foreach (card foundcardtodiscard in Enemy.cards_in_hand)
						{
							if (foundcardtodiscard != foundcard) {foundcardtodiscard.Discard(true); break; }
						}
					}

					if (foundcard.Type == 1)  foundcard.StartCoroutine("FromHandCreature", true);  

					else if (foundcard.Type == 2)   foundcard.FromHandSpell(true);	

					else  foundcard.StartCoroutine("FromHandEnchantment", true);	

					break; 
				
			}
		}

		while (Player.SpellInProcess) 	yield return new WaitForSeconds(0.2f); 
		
		OK_to_do_next_AI_action = true;


	}

	static public void DoCoroutine(){
		instance.StartCoroutine("AIPlays"); 
	}


	static public bool CheckCard(card card_to_check)
	{
		
		if (card_to_check.Level > 0) { 
			foreach (card foundcard in Enemy.cards_in_game) 
				if (foundcard.Level == (card_to_check.Level-1) && foundcard.GrowID == card_to_check.GrowID) return true;

			return false;
		}

		if (card_to_check.Type == 0) { 
			if (!Enemy.LandsZone.CanPlace(true)) 
								return false;
				}
		else if (card_to_check.Type !=2 && !Enemy.CreaturesZone.CanPlace(true)) 
			return false;


		if (card_to_check.Effects.Count > 0) 
		{
			foreach (Effect foundeffect in card_to_check.Effects) {
			if (foundeffect.trigger != 1) { 
				
				int Target = 0;
				int TargetParam = 0;

				int effect = foundeffect.type;
				Target = foundeffect.target;
				TargetParam = foundeffect.targetparam0;

				Debug.Log("AI is checking effect:"+foundeffect.type+" target:"+Target+"on card:"+card_to_check.Name);

				bool NegativeEffect = true;
				if (effect == 11 || effect == 0)	NegativeEffect = false;
				
				List<card> creatures_to_search = new List<card>();
			
				if (foundeffect.param0type == 1) 
					{ 
						if (!Enemy.HasACreature()) return false; 
					}
				else if (foundeffect.param0type == 2) 
					{ 
						if (Enemy.AlliesDestroyedThisTurn <= 0 ) return false; 
					}

				if (NegativeEffect) { 
					foreach (card foundcard in Player.player_creatures)
						if (!foundcard.Hero) creatures_to_search.Add(foundcard);
				}
				else { 
					foreach (card foundcard in Enemy.enemy_creatures)
							if (!foundcard.Hero) creatures_to_search.Add(foundcard);
				}


				if (Target == 12) 
				{
					if (!Enemy.HasACreature()) return false;
				}


				if (Target == 7) 
				{
					if (!Enemy.HasAHero()) return false;
				}

				
				if  (effect == 0 && (Target == 12 || Target == 9))
				{
					bool found = false;
					foreach (card foundcreature in Enemy.enemy_creatures)
					{
						if (MainMenu.TCGMaker.core.OptionOneCombatStatForCreatures)
						{
							if (foundcreature.CreatureOffense < foundcreature.CreatureStartingOffense) found = true;
						}

						else if (foundcreature.CreatureDefense < foundcreature.CreatureStartingDefense) found = true;
					}
					 if (!found) return false;
				}

				
					if  (Target == 6)
					{	
						bool found = false;
						foreach (card foundcreature in creatures_to_search)
							if (foundcreature.IsTurned && foundcreature.AttackedThisTurn>0)  found = true;
						
						if (!found) return false;
					}
					else if (Target == 261) 
					{
						if (EffectManager.CreaturesInGame().Count < foundeffect.targetparam0) return false;

					}

					else if (Target == 30) 
					{
					foreach (card foundcreature in creatures_to_search)
					
						if (foundcreature.CreatureOffense > TargetParam) return false;

					}
					else if (Target == 31) 
					{
					foreach (card foundcreature in creatures_to_search)
						
						if (!foundcreature.Hero && foundcreature.Cost.Count > TargetParam) return false;

					}
					else if (Target == 202) 
					{
						if (!Enemy.RandomCreatureWithCostEqualOrLowerThan(foundeffect.targetparam0)) return false;
							
					}
					else if (Target == 200 || Target == 201 || Target == 14)  
					{
						if  (!Player.HasACreature()) return false;

					}
					else if (Target == 203 || Target == 16) 
					{
						if  (!Player.HasACreature() && !Player.HasAHero()) return false;

					}
					else if ( Target == 5 || Target == 4) 
					{
								if (creatures_to_search.Count == 0)	
								return false;
					}
					else if (Target == 6 ) 
					{
						bool found = false;
						foreach (card foundcard in creatures_to_search)
							if (foundcard.IsTurned) found = true;

						if (!found) return false;
					}
				

				if (Target == 50 || Target == 51 || Target == 302 || Target == 303 ) 
				{
					bool found = false;
					foreach (card graveyardcard in Enemy.cards_in_graveyard) 
					{
						if (graveyardcard.Type == 2 && (Target == 50 || Target == 303 )) found = true; 
						if (graveyardcard.Type == 1 && (Target == 51 || Target == 302 )) found = true; 
					}
					if (!found) return false;
				}


				if (effect == 8) 
					{
						bool found = false;
						foreach (card foundcard in Enemy.enemy_creatures)
									if (foundcard.IsTurned) found = true;
						
						if (!found) return false;
					}
				
				else if  (effect == 6)
				{
					if (!Player.HasACreature()) return false;
					
						if (!Enemy.HasACreature() && Player.Creatures().Count <= 1 ) return false;

				}

				
				else if  (effect == 11)
				{
					if (!Enemy.HasACreature())	return false;
				}
			}
		}
		}
		Debug.Log("finished checking, returning true");
		return true;
	}
}