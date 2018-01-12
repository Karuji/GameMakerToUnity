using System.Linq;
using UnityEngine;
using UnityEditor;

namespace GM2Unity.Utility
{
	/// <summary>
	/// Abstract class for making reload-proof singletons out of ScriptableObjects
	/// Based on http://baraujo.net/unity3d-making-singletons-from-scriptableobjects-automatically/
	/// </summary>
	/// <typeparam name="T">Singleton type</typeparam>

	public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
	{
		private static T instance;

		public static T Instance
		{
			get
			{
				if (!instance)
				{
					// Use this instead of FindObjectOfType so it can find SOs created via menus.
					instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
				}
				if (!instance)
				{
					instance = Create();
				}
				
				return instance;
			}
			protected set
			{
				instance = value;
			}
		}

		public static bool InstanceExists { get { return instance != null; } }

		private static T Create()
		{
			instance = ScriptableObject.CreateInstance<T>();
			AssetDatabase.CreateAsset(instance, "Assets/" + typeof(T).ToString() + ".asset");

			return instance;
		}
	}
}
