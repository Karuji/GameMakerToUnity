using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace GM2Unity
{
	[InitializeOnLoad]
	public class ObjectImporter : MonoBehaviour
	{
		public static void ImportObject(ImportAsset importAsset = null)
		{
			XmlDocument sourceXml = new XmlDocument();
			sourceXml.Load(importAsset.sourceXML);
			XmlElement rootElement = sourceXml.DocumentElement;

			string spriteName = rootElement.SelectSingleNode("spriteName").InnerText;
			string localPath = "Assets" + importAsset.targetPath;
			string localName = localPath + "/" + importAsset.targetName + ".prefab";
			localName = localName.Replace('\\', '/');
			int depth = 0;

			if (rootElement.SelectSingleNode("//depth").InnerText != null)
			{
				depth = int.Parse(rootElement.SelectSingleNode("//depth").InnerText);
				depth /= ImportSettings.Instance.PixelsPerUnit;
			}

			if (!Directory.Exists(importAsset.targetCompletePath))
			{
				Directory.CreateDirectory(importAsset.targetCompletePath);
			}

			GameObject go = new GameObject();
			Vector3 vector3 = new Vector3(0f, 0f, depth);
			go.name = importAsset.targetName;
			go.transform.position = vector3;

			if (spriteName != "<undefined>")
			{
				Sprite sprite = FindSprite(spriteName);
				if (sprite != null)
				{
					SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
					sr.sprite = sprite;
				}
				else
				{
					Debug.LogWarning("<color=#ff9999ff>Cannot find sprite for: " + spriteName + " in object </color><color=#ffdd22ff>" + importAsset.targetName+ "</color>");
				}
			}

			Object prefab = PrefabUtility.CreateEmptyPrefab(localName);
			PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.Default);

			DestroyImmediate(go);

			if (ImportSettings.Instance.ShowLogging)
			{
				print("Imported object: <color=#22ffccff>" + localName + "</color> " + "<color=#22ee22ff>" + importAsset.targetName + "</color>");
			}
		}

		public static Sprite FindSprite(string spriteName)
		{

			string[] guids = AssetDatabase.FindAssets(spriteName + " t:sprite", new string[]{"Assets/sprites"});

			if (guids != null && guids.Length > 0)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[0]);

				Sprite sprite = (Sprite)AssetDatabase.LoadAssetAtPath(path, typeof(Sprite));

				if (sprite == null)
				{
					print("<color=#ffaaaaff>Loaded a null sprite for</color> " + spriteName);
				}

				return sprite;
			}

			return null;
		}
	}
}