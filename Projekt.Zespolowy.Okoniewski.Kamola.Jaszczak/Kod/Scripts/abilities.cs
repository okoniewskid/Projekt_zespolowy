using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class abilities : MonoBehaviour
{

    /*
     * Skrypt obsługujący umiejętności dodatkowe kart,  
     * wyświetlający menu służące do uruchomienia umiejętności na karcie 
    */

    public int Trigger0;

    card thiscard;

    public bool DisplayMenu = false;

    // parametry aktywacji umiejętności (zależne od samej karty)
    public const int ON_ENTER = 0;
    public const int ON_ACTIVATE = 1;
    public const int ON_ATTACK = 2;
    public const int ON_KILL = 3;

    // parametry aktywacji umiejętności (zależne od interakcji zewnętrznych)
    public const int ON_SPELL = 20; //gdy gracz zagra kartę
    public const int ON_OPPONENT_SPELL = 21; //gdy przeciwnik zagra kartę
    public const int ON_FRIENDLY_DIES = 22; //gdy zginie sojusznicza jednostka
    public const int ON_FRIENDLY_ISATTACKED = 23; //gdy przeciwnik zaatakuje jednostkę gracza

    public const int ON_START_OF_YOUR_TURN = 30; //na początku tury gracza
    public const int ON_END_OF_YOUR_TURN = 31; //na końcu tury gracza

    public const int ON_ENTER_CARDSUBTYPE = 50; //gdy do gry wejdzie karta o danym podtypie

    List<Texture2D> cost_textures = new List<Texture2D>();

    int[] activated_abilities;

    //uruchamianie umiejętności
    public void TriggerAbility(int ability_trigger, bool AI = false, int subtype = -1)
    {
        int i = 0;
        bool foundability = false;

        foreach (Effect effect in thiscard.Effects)
        {
            if (effect.trigger == ability_trigger && (ability_trigger != ON_ENTER_CARDSUBTYPE || subtype == effect.triggerparam0))
            {
                foundability = true;

                Player.SpellInProcess = true;

                thiscard.ApplyEffect(i, AI);
            }
            i++;
        }
        if (foundability) Player.CanDoStack = true;

    }

    public void Awake()
    {
        thiscard = GetComponent<card>();
        UpdateActivatedAbilities();
    }

    //sporzadza liste umiejetnosci karty zeby wyswietlic je w menu umiejetnosci
    void UpdateActivatedAbilities()
    {
        int arraylength = 0;
        foreach (Effect effect in thiscard.Effects)
        {

            if (effect.trigger == ON_ACTIVATE) arraylength++;

        }

        int[] temparray = new int[arraylength];
        int i = 0;
        int j = 0;

        foreach (Effect effect in thiscard.Effects)
        {


            if (effect.trigger == ON_ACTIVATE) { temparray[j] = i; j++; }
            i++;

        }

        activated_abilities = temparray;
    }

    //zamykanie menu umiejętności
    void CloseMenu()
    {

        DisplayMenu = false;
        Player.DisplayingCreatureMenu = false;
        cost_textures.Clear();
    }

    void CombineIcons(int i, Sprite sprite_to_add)
    {

        Texture2D texture_to_add = MainMenu.SpriteToTexture(sprite_to_add);

        if (cost_textures.Count == i)
            cost_textures.Add(texture_to_add);

        else {
            Texture2D newtex = new Texture2D((int)(cost_textures[i].width + texture_to_add.width + 1), cost_textures[i].height + 1);
            Color[] pixels = cost_textures[i].GetPixels();

            newtex.SetPixels(0, 0, cost_textures[i].width, cost_textures[i].height, pixels);

            Color[] pixels2 = texture_to_add.GetPixels();

            newtex.SetPixels(cost_textures[i].width, 0, texture_to_add.width, texture_to_add.height, pixels2);
            newtex.Apply();
            cost_textures[i] = newtex;
        }
    }

    //wyswietlanie menu umiejętnosci karty
    public void OnGUI()
    {

        if (DisplayMenu)
        {
            Player.DisplayingCreatureMenu = true;
            Vector3 p = Camera.main.WorldToScreenPoint(transform.position);

            int Cost = 0;

            string ability_text;

            for (int i = 0; i < activated_abilities.Length; i++)
            {
                Effect effect = thiscard.Effects[activated_abilities[i]];
                Cost = effect.cost.Count;

                if (effect.name == null) ability_text = "Use ability";
                else if (effect.name == "") ability_text = "Use ability";
                else ability_text = effect.name.ToString();

                GUIContent content = new GUIContent();

                if (Cost != 0) ability_text += ", cost: " + Cost;


                content.text = ability_text;

                if (GUI.Button(new Rect(p.x + 20, Screen.height - p.y + i * 30, 270, 30), content))
                {

                    if (thiscard.IsTurned)
                    {
                        if (thiscard.IsACreature()) Player.Warning = "This creature has already commited this turn";
                        else Player.Warning = "This card has already commited this turn";
                    }
                    else if (thiscard.FirstTurnSickness()) Player.Warning = "A creature cannot use its abilities on its first turn";
                    else if (Cost <= Player.mana.Count)
                    {
                        if (Cost > 0) Player.mana.RemoveAt(0);
                        thiscard.ApplyEffect(activated_abilities[i], false);
                        Player.CanDoStack = true;

                    }
                    else Player.Warning = "You don't have enough mana";
                    CloseMenu();
                }
            }

            if (activated_abilities.Length > 0 || thiscard.GrowID != "")
            {
                if (GUI.Button(new Rect(p.x + 20, Screen.height - p.y + (activated_abilities.Length) * 30, 270, 30), "Cancel")) DisplayMenu = false;
            }
            else CloseMenu();

        }
    }


    //znajduje umiejetnosci uruchamiane gdy karta wchodzi do gry
    public void OnEnter(bool AI = false)
    {
        int i = 0;


        bool foundability = false;
        foreach (Effect effect in thiscard.Effects)
        {
            int target = effect.target;


            if (effect.trigger == ON_ENTER)
            {
                foundability = true;
                Player.SpellInProcess = true;


                thiscard.ApplyEffect(i, AI);
            }
            i++;
        }

        if (foundability) Player.CanDoStack = true;

    }

    void Start()
    {

    }


}
