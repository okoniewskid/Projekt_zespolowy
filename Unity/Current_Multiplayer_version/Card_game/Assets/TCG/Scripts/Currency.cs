using UnityEngine;
using System.Collections;



public class Currency : MonoBehaviour
{
	static public Currency instance;
	private static string secretKey = "WorkSucks"; 
	public static string addSilverURL = "http://losange-vision.com/addsilver.php?";

	public static string buyCardURL = "http://losange-vision.com/buycard.php?";


	public static string mycurrencyURL = "http://localhost/tcg/mycurrency.php";

	public static string messagecurrency = "";
	public static int PlayerCurrency;

	public static string Md5Sum(string strToEncrypt)
	{
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(strToEncrypt);
		
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);
		
		string hashString = "";
		
		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}
		
		return hashString.PadLeft(32, '0');
	}



	void Awake(){
		DontDestroyOnLoad(transform.gameObject);
		instance = this;
	}


	void Start()
	{

	}

	void Update()
	{

	}

	static public void GetCurrency()
	{
		WWWForm form = new WWWForm(); 
		
		form.AddField("userid", MainMenu.userid);
				
		WWW w = new WWW(mycurrencyURL, form);

		instance.StartCoroutine("GetMyCurrency",w);
	}

	static public void DoAssignCurrency(int score)
	{
	
		instance.StartCoroutine(AssignCurrency(score));
	}


	static IEnumerator AssignCurrency(int score)
	{
		Debug.Log("gonna assign " + score +" silver to player: " + MainMenu.userid);

		string hash = Md5Sum(MainMenu.userid + score + secretKey);
		
		string post_url = addSilverURL + "userid=" + WWW.EscapeURL(MainMenu.userid.ToString()) + "&silver=" + score + "&hash=" + hash;
		
		WWW hs_post = new WWW(post_url);
		yield return hs_post;
		
		if (hs_post.error != null)
		{
			Debug.Log("There was an error assigning silver: " + hs_post.error);
		}
		else Debug.Log("assigned successfully");
	}

	static public void DoBuyCard (int Index)
	{
		
		instance.StartCoroutine(BuyCard(Index));
	}
	
	
	static IEnumerator BuyCard(int Index)
	{
		Debug.Log("gonna buy card id " + Index +"for player: " + MainMenu.userid);
	
		string hash = Md5Sum(MainMenu.userid + Index + secretKey);
		
		string post_url = buyCardURL + "userid=" + WWW.EscapeURL(MainMenu.userid.ToString()) + "&index=" + Index + "&hash=" + hash;
		
		WWW hs_post = new WWW(post_url);
		yield return hs_post;
		
		if (hs_post.error != null)
		{
			Debug.Log("There was an error buying card: " + hs_post.error);
		}
		else Debug.Log("bought successfully");
	}
	


	IEnumerator GetMyCurrency(WWW w)
	{
		yield return w;
		if (w.error ==null)
		{
			Debug.Log(w.text);
			PlayerCurrency=System.Int32.Parse(w.text);
			messagecurrency = "My silver: "+w.text+"\n";
		}
		else messagecurrency = "ERROR:" +w.error + "\n";
		Debug.Log(messagecurrency);
	}


	
}