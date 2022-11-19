//Create and Maintain by Hsinpa

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Hsinpa.Utility
{
    public class UtilityFunc  {


		private static System.Random random = new System.Random();

		public static void SetRandomSeed(int seed)
		{
			random = new System.Random(seed);
		}

		public static float NormalizeByRange(float target, float min, float max)
		{
			return (target - min) / (max - min);
		}

		//0f-1f
		public static float Random()
		{
			return (float)random.NextDouble();
		}



		/// <summary>
		///  Load single sprite from multiple mode
		/// </summary>
		/// <param name="spriteArray"></param>
		/// <param name="spriteName"></param>
		/// <returns></returns>
		public static Sprite LoadSpriteFromMulti(Sprite[] spriteArray, string spriteName) {
			foreach (Sprite s in spriteArray) {
				
				if (s.name == spriteName) return s;
			}
			return null;
		}

        /// <summary>
        /// Clear every child gameobject
        /// </summary>
        /// <param name="parent"></param>
        public static void ClearChildObject(Transform parent, string execptionObj = "") {
            foreach (Transform t in parent) {

				if (execptionObj != null && t.name == execptionObj)
					continue;

				GameObject.Destroy(t.gameObject);
            }
        }

		public static List<T> CreateObjectWithCallback<T>(Transform parent, T prefab, int length, System.Action<int, T> callback) where T : MonoBehaviour {
			List<T> generalList = new List<T>();

			for (int i = 0; i < length; i++) {
				GameObject item = CreateObjectToParent(parent, prefab.gameObject);
				var itemType = item.GetComponent<T>();

				callback(i, itemType);
				generalList.Add(itemType);
			}

			return generalList;
		}

		public static void DeleteObject(GameObject p_object) {
			if (Application.isPlaying)	GameObject.Destroy(p_object);
			if (Application.isEditor)	GameObject.DestroyImmediate(p_object);				
        }


        /// <summary>
        ///  Insert gameobject to parent
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static GameObject CreateObjectToParent(Transform parent, GameObject prefab) {
            GameObject item = GameObject.Instantiate(prefab);
            item.transform.SetParent(parent);
            item.transform.localScale = Vector3.one;
			item.transform.position = new Vector3(item.transform.position.x, item.transform.position.y, 1);
			item.transform.localPosition = new Vector3( 0, 0, 1);
            return item;
        }

		public static GameObject FindObject(GameObject parent, string name) {
		     Transform[] trs= parent.GetComponentsInChildren<Transform>(true);
		     foreach(Transform t in trs){
		         if(t.name == name){
		              return t.gameObject;
		         }
		     }
		     return null;
		 }


		/// <summary>
		/// Rolls the dice, only return 1 or 0.
		/// </summary>
		/// <returns>The dice.</returns>
		public static int RollDice() {
			return Mathf.RoundToInt(UnityEngine.Random.Range(0,1));
		}
		
		/// <summary>
		/// Possibilities the match.
		/// </summary>
		/// <returns><c>true</c>, if match was possibilityed, <c>false</c> otherwise.</returns>
		public static bool PercentageGame(float rate) {
			float testValue = UnityEngine.Random.Range(0f ,1f);
			return ( rate >= testValue ) ? true : false;
		}

		public static T PercentageTurntable<T>(T[] p_group, float[] percent_array) {
			float percent = UnityEngine.Random.Range(0f, 100f);
			float max = 100;

			for (int i = 0 ; i < percent_array.Length; i++) {
				float newMax = max - percent_array[i];
				if (max >= percent && newMax <= percent ) return p_group[i];

				max = newMax;
			}
			return default (T);
		}

		public static List<T> Shuffle<T>(List<T> p_list) {
			int n = p_list.Count; 
			System.Random rng = new System.Random();
		    while (n > 1) {
		        n--;  
		        int k = rng.Next(n + 1);  
				T value = p_list[k];  
				p_list[k] = p_list[n];  
				p_list[n] = value;  
		    }
		    return p_list;
		}

		public static T PercentageTurntable<T>(T[] p_group, int[] percent_array) {
			float[] convertFloat = System.Array.ConvertAll(percent_array, s => (float)s);
			return PercentageTurntable<T>(p_group, convertFloat);
		}

		public static void FramePageHandler(CanvasGroup canvas, Transform buttonGroup ) {
			foreach(Transform child in buttonGroup) {
				
				CanvasGroup closeCanvas= child.GetComponent<CanvasGroup>();
				if (closeCanvas == null) continue;
				
				closeCanvas.alpha =0;
				closeCanvas.interactable =false;
				closeCanvas.blocksRaycasts =false;
			}
			canvas.alpha =1;
			canvas.interactable =true;
			canvas.blocksRaycasts =true;
		}

		public static T[] RandomizeArray<T>(T[] array) {
			for (int i = array.Length - 1; i > 0; i--) {
				int r = UnityEngine.Random.Range(0,i);
				T tmp = array[i];
				array[i] = array[r];
				array[r] = tmp;
	        }
			return array;
	    }

		public static string Base64Encode(string plainText) {
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return System.Convert.ToBase64String(plainTextBytes);
		}
		
		public static string Base64Decode(string base64EncodedData) {
			var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		}


		// Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
		string ColorToHex(Color32 color)
		{
			string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
			return hex;
		}
		
		public static Color HexToColor(string hex)
		{
			byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
			return new Color32(r,g,b, 255);
		}

		public static T ParseEnum<T>(string value)	{
		    return (T) System.Enum.Parse(typeof(T), value, true);
		}

		public static string GenerateRandomGUID()
		{
			return System.Guid.NewGuid().ToString();
		}

		public static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}

		public static async Task DoDelayWork(float p_delay, System.Action p_action)
		{
			await DoDelayWork(p_delay, p_action, CancellationToken.None);
		}

		public static async Task DoDelayWork(float p_delay, System.Action p_action, CancellationToken token)
		{
			try
			{
				await Task.Delay(System.TimeSpan.FromSeconds(p_delay), token);

				if (p_action != null)
					p_action();
			}
			catch (System.Exception exception)
			{

			}
		}

		public static IEnumerator DoEndFrameCoroutineWork(System.Action p_action)
		{
			yield return new WaitForEndOfFrame();

			if (p_action != null)
				p_action();
		}

        public static IEnumerator DoDelayCoroutineWork(float p_delay, System.Action p_action)
        {
            yield return new WaitForSeconds(p_delay);

            if (p_action != null)
                p_action();
        }


        public static async Task DoNexFrame(System.Action p_action)
		{
			await Task.Yield();

			if (p_action != null)
				p_action();
		}

		public static string GetGameObjectPath(Transform transform)
		{
			string path = transform.name;
			while (transform.parent != null)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}
			return path;
		}

		public static Dictionary<string, T> SetDictionary<T>(Dictionary<string, T> dict, string key, T addValue) {

			if (dict.ContainsKey(key))
			{
				dict[key] = addValue;
			}
			else {
				dict.Add(key, addValue);
			}

			return dict;
		}

		public static Dictionary<K, List<T>> SetListDictionary<K, T>(Dictionary<K, List<T>> dict, K id, T dataStruct)
		{
			if (dict.ContainsKey(id))
			{
				dict[id].Add(dataStruct);
			}
			else
			{
				dict.Add(id, new List<T>() { dataStruct });
			}
			return dict;
		}

		public static void SetSimpleBtnEvent(Button btn, System.Action eventCallback)
		{
			if (btn == null || eventCallback == null) return;
			btn.onClick.RemoveAllListeners();
			btn.onClick.AddListener(() =>
			{
				eventCallback();
			});
		}

		public static String MixString(string s1, string s2)
		{
			//return s1 + ":" + s2;
			double x = (s1.GetHashCode() * 0.5f);
			double y = (s2.GetHashCode() * 0.5f);

			return ((x + y) * 2).ToString().Trim();
		}


		public static string CreateStreamIDByInt(string constant, int intA, int intB)
		{
			string sA = intA.ToString(), sB = intB.ToString();

			return constant + "_" + ((intA > intB) ? (sA + "_" + sB) : (sB + "_" + sA));
		}

        //In Second
        public static DateTime UnixTimeStampToLocalDateTime(string unixTimeStamp)
		{
			string timeString = "";
			if (double.TryParse(unixTimeStamp, out double utcTime))
			{
				return UnixTimeStampToLocalDateTime(utcTime);
			}

			return DateTime.Now;
		}
		public static DateTime UnixTimeStampToLocalDateTime(double unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
			return dtDateTime;
		}

		public static int FindIndex<T>(T[] dataArray, Func<T, bool> logicOperation) {
			if (dataArray == null) return -1;

			int lens = dataArray.Length;
			for (int i = 0; i < lens; i++) {
				if (logicOperation(dataArray[i])) {
					return i;
				}
			}

			return -1;
		}

		public static void ListMutOpt<T>(List<T> list, System.Func<T, T> operation)
		{
			if (list == null) return;

			int lens = list.Count;

			for (int i = 0; i < lens; i++)
			{
				list[i] = operation(list[i]);
			}
		}

		public static void ListOpt<T>(IReadOnlyCollection<T> list, System.Action<T> operation)
		{
			if (list == null) return;

			foreach (var l in list)
			{
				 operation(l);
			}
		}

		public static float Distance2D(float x, float y)
        {
			return Mathf.Sqrt((x * x) + (y * y));
        }
	}
}