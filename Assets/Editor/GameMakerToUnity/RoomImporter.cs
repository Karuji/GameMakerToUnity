using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace GM2Unity
{
	public class RoomImporter : MonoBehaviour
	{
		public static void ImportRoom(ImportAsset importAsset = null)
		{

			XmlDocument sourceXml = new XmlDocument();
			sourceXml.Load(importAsset.sourceXML);
			XmlElement rootElement = sourceXml.DocumentElement;

			XmlNodeList nodeList = rootElement.SelectNodes("//instance");

			int ppu = ImportSettings.Instance.PixelsPerUnit;

			Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

			int height = (int.Parse(rootElement.SelectSingleNode("//height").InnerText)) / ppu;

			string assetPath = "Assets" + importAsset.targetPath + "/" + importAsset.targetName + ".unity";
			assetPath = assetPath.Replace('\\','/');

			foreach (XmlNode node in nodeList)
			{
				Vector3 pos = new Vector3();
				pos.x = (int.Parse(node.Attributes["x"].Value)) / ppu;
				pos.y = height - (int.Parse(node.Attributes["y"].Value) / ppu);

				string assetName = node.Attributes["objName"].Value;

				string[] guids = AssetDatabase.FindAssets(assetName);

				if (guids != null && guids.Length > 0)
				{
					string path = AssetDatabase.GUIDToAssetPath(guids[0]);
					Object prefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
					GameObject go = PrefabUtility.InstantiatePrefab(prefab as GameObject) as GameObject;
					pos.z = go.transform.position.z;
					go.transform.position = pos;
				}
				else
				{
					Debug.LogWarning("<color=#ff9999ff>Cannot find an object in the database for </color><color=#ffdd22ff>: " + assetName + "</color>");
				}
			}

			if (!Directory.Exists(importAsset.targetCompletePath))
			{
				Directory.CreateDirectory(importAsset.targetCompletePath);
			}

			bool sceneSaved = EditorSceneManager.SaveScene(scene, assetPath);
			if (!sceneSaved)
			{
				Debug.LogWarning("Saved scene <color=#22ffccff>" + assetPath + "</color> " + "<color=#ee2222ff>Nope</color>");
			}
			else
			{
				if (ImportSettings.Instance.ShowLogging)
				{
					print("Saved scene <color=#22ffccff>" + assetPath + "</color> " + "<color=#22ee22ff>OK</color>");
				}
			}
		}
	}
}
