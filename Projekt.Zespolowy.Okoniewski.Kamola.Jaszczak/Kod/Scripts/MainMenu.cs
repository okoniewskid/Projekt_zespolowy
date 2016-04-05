
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;


//Skrypt glownego menu

public class MainMenu : MonoBehaviour
{
    public static Data currentsettings;
    public static Data DTA
    {
        get
        {
            if (currentsettings == null)
                currentsettings = (Data)Resources.Load("TCGData", typeof(Data));

            return currentsettings;
        }
    }

    public static bool FirstLoadMenu = true;

    public static GUIStyle mystyle;

    public static bool CollectionNeedsUpdate = false;

    public static readonly string SceneNameMainMenu = "MainMenuScene";

    public static readonly string SceneNameGame = "GameScene";

    public static readonly string SceneNameEditDeck = "EditDeckScene";

    public static string deckstring, collectionstring;

    public static string message;

    public static float ColliderWidth, ColliderHeight;


    public void Awake()
    {

        CardTemplate instance = CardTemplate.Instance;

    }


    public void Start()
    {
        playerDeck.pD.LoadPlayerDeckOffline();

        FirstLoadMenu = false;
    }

    public void Update()
    {

    }


    public static Texture2D SpriteToTexture(Sprite sprite)

    {
        Texture2D croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);

        Color[] pixels = sprite.texture.GetPixels(0,
                                                   0,
                                                   (int)sprite.rect.width,
                                                   (int)sprite.rect.height);

        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();
        return croppedTexture;

    }



    //wyswietlanie menu i jego zawartosci
    public void OnGUI()
    {

        GUI.skin.box.fontStyle = FontStyle.Bold;
        GUI.Box(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 300), "Tarotia");
        GUILayout.BeginArea(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 250));

        GUILayout.Space(50);

        GUILayout.BeginHorizontal();

        if (message != "") GUILayout.Box(message);
        GUILayout.EndHorizontal();




        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Single Game", GUILayout.Width(200)))
        {
            Application.LoadLevel(SceneNameGame);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Change deck", GUILayout.Width(200)))
        {

            Application.LoadLevel(SceneNameEditDeck);


        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();



        GUILayout.EndArea();
    }


}
