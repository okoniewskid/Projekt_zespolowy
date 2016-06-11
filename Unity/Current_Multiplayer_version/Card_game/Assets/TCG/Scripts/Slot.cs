using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Slot : MonoBehaviour {
	[HideInInspector]
	public int number_in_zone;


	public int row;

	public int column;

	[HideInInspector]
	public bool highlighted;

	[HideInInspector]
	public Zone zone
	{
		get
		{
			return transform.parent.GetComponent<Zone>();
		}
	}



	public void OnMouseDown(){
		Debug.Log("slot mousedown!");
		if (highlighted) 
		{
			Player.targets.Add(gameObject);
			Debug.Log("chose a slot!");
			zone.PlayerIsChoosingASlot = false;
			Player.NeedTarget = 0;
			zone.RemoveHighlightedSlots();
			
		}
	}

	public void Highlight()
	{
		if (GetComponent<MeshRenderer>()) GetComponent<Renderer>().material = playerDeck.pD.slot_highlighted;
		highlighted = true;
	}



	void OnMouseOver () 
	{	
		
		if (highlighted) 
				 if (GetComponent<MeshRenderer>()) GetComponent<Renderer>().material = playerDeck.pD.slot_mouseover;
				 


	}

	void OnMouseExit () 
	{
		

		if (highlighted) 
			if (GetComponent<MeshRenderer>()) GetComponent<Renderer>().material = playerDeck.pD.slot_highlighted;

	}

	void OnGUI() {

		if (zone.PlayerIsChoosingASlot && !MainMenu.TCGMaker.core.UseGrid) 
		{
			if (highlighted) GUI.DrawTexture(Zone.GUIRectWithObject(gameObject), Zone.hl_all_texture); 

			Vector3 mouse_pos= Camera.main.ScreenToWorldPoint(Input.mousePosition);
		                         
			if (GetComponent<Collider>().bounds.Contains(new Vector3(mouse_pos.x,mouse_pos.y, 0f))) 
			{ 

				 GUI.DrawTexture(Zone.GUIRectWithObject(gameObject), Zone.hl_mouseover_texture);

			}
		}
	}

	public Transform RandomEmptyAdjacentSlot()
	{
		Slot otherslot;
			foreach (Transform child in transform.parent)
			{
				otherslot = child.GetComponent<Slot>();
					if (otherslot!=null && child.childCount == 0) 
					if ((row%2) == 0)
				{
					if (
					!(otherslot.row - row == 1 && otherslot.column - column == 1) &&
					 !(otherslot.row - row == -1 && otherslot.column - column == 1) &&
						Mathf.Abs(row - otherslot.row) < 2  &&
						 Mathf.Abs(column - otherslot.column) < 2
					)
					return otherslot.transform;
			}
			else {
				if (
					!(otherslot.row - row == 1 && otherslot.column - column == -1) &&
					!(otherslot.row - row == -1 && otherslot.column - column == -1) &&
					Mathf.Abs(row - otherslot.row) < 2  &&
					Mathf.Abs(column - otherslot.column) < 2
					)
					return otherslot.transform;
			}
		}
			return null;

	}

	public List<Slot> IsInALine(Slot otherslot)
	{
		List<Slot> path = new List<Slot>();
		path.Add(otherslot);
		int i = 0;
		int maxsteps = 6;

		if (row == otherslot.row) 
		{
			while ( i<maxsteps )
			{
				if (i > column && i < otherslot.column ) path.Add(playerDeck.pD.Grid[row, i]);
				if (i < column && i > otherslot.column ) path.Add(playerDeck.pD.Grid[row, i]);
				i++;
			}
			return path;
		}

		int currentrow = row, currentcolumn = column;

		
		i = 0;
		if (row%2 == 0) i = 1;

		while (currentrow>0)
			{
				currentrow--;
				if  (i%2 == 0)
					if (currentcolumn < maxsteps) currentcolumn++;
					else break;
		
				path.Add(playerDeck.pD.Grid[currentrow, currentcolumn]);

				if (playerDeck.pD.Grid[currentrow, currentcolumn]!=null)
					if (playerDeck.pD.Grid[currentrow, currentcolumn].transform.childCount>0) 
						if (playerDeck.pD.Grid[currentrow, currentcolumn] != otherslot) break;
							else return path; 

				i++;
			}

		
		i = 0;
		if (row%2 == 0) i = 1;
		currentrow = row;
		currentcolumn = column;
		path.Clear();

		while (currentrow>0)
		{
			currentrow--;
			if  (i%2 != 0)
				if (currentcolumn>0) currentcolumn--;
				else break;

			path.Add(playerDeck.pD.Grid[currentrow, currentcolumn]);

			if (playerDeck.pD.Grid[currentrow, currentcolumn]!=null)
				if (playerDeck.pD.Grid[currentrow, currentcolumn].transform.childCount>0) 
					if (playerDeck.pD.Grid[currentrow, currentcolumn] != otherslot) break;
						else return path; 
			
			i++;
		}

		
		i = 0;
		if (row%2 == 0) i = 1;
		currentrow = row;
		currentcolumn = column;
		path.Clear();
		
		while (currentrow<maxsteps)
		{
			currentrow++;
			if  (i%2 == 0)
				if (currentcolumn<maxsteps) currentcolumn++;
				else break;

			path.Add(playerDeck.pD.Grid[currentrow, currentcolumn]);

			if (playerDeck.pD.Grid[currentrow, currentcolumn]!=null)
				if (playerDeck.pD.Grid[currentrow, currentcolumn].transform.childCount>0) 
					if (playerDeck.pD.Grid[currentrow, currentcolumn] != otherslot) break;
						else return path; 
			
			i++;
		}
		
		i = 0;
		if (row%2 == 0) i = 1;
		currentrow = row;
		currentcolumn = column;
		path.Clear();
		
		while (currentrow<maxsteps)
		{
			currentrow++;
			if  (i%2 != 0)
				if (currentcolumn>0) currentcolumn--;
				else break;
					
			path.Add(playerDeck.pD.Grid[currentrow, currentcolumn]);
			Debug.Log("added to path: " + currentrow + ", " + currentcolumn);


			if (playerDeck.pD.Grid[currentrow, currentcolumn]!=null)
				if (playerDeck.pD.Grid[currentrow, currentcolumn].transform.childCount>0) 
					if (playerDeck.pD.Grid[currentrow, currentcolumn] != otherslot) break;
						else return path; 
			
			i++;
		}
		return null;
		
	}

	public bool IsAdjacent(Slot otherslot)
	{

		if (row%2 == 0)
		{
			if (otherslot.row - row == 1 && otherslot.column - column == 1) return false;
			if (otherslot.row - row == -1 && otherslot.column - column == 1) return false;
		}

		else 
		{
			if (otherslot.row - row == 1 && otherslot.column - column == -1) return false;
			if (otherslot.row - row == -1 && otherslot.column - column == -1) return false;
		}

		if (Mathf.Abs(row - otherslot.row) < 2  &&
		    Mathf.Abs(column - otherslot.column) < 2 ) { 
			return true; }
			
		{  
			return false; }

	}

	public bool HeroIsAdjacent(bool AI = false)
	{
		Slot ourhero;

		if (AI) ourhero = GameObject.FindWithTag("Enemy").GetComponent<Enemy>().hero_slot;
			else ourhero = GameObject.FindWithTag("Player").GetComponent<Player>().hero_slot;

		if (IsAdjacent(ourhero)) return true;

		return false;
	}
}




