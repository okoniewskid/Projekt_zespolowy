using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class Data : ScriptableObject {

	public int test = 2;

	public Options settings;
	 

	public Stats stats;

	public List<DbCard> cards;


	void Start () {
	
	}

	void Update () {
	
	}
}
