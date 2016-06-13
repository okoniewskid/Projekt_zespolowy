using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class TCGData : ScriptableObject {

	public int test = 2;

	public CoreOptions core;
	 


	public Stats stats;

	public List<DbCard> cards;


	void Start () {
	
	}
	

	void Update () {
	
	}
}
