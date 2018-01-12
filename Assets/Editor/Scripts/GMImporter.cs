using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine;
using UnityEditor;
using GM2Unity.Utility;

namespace GM2Unity
{
    public class GMImporter : MonoBehaviour
    {
        public enum ImportType
        {
            Null,
            Background,
            Object,
            Room,
            Sprite,
            Sound
        }

        protected static string basePath;
        protected static List<ImportAsset> targetPaths = new List<ImportAsset>();
        protected static XmlNodeList sounds, sprites, backgrounds, objects, rooms;
        protected static double startTime;
        protected List<ImportAsset> importAssets;

        #region monofunctions

        void Start()
        {
            Import();
            importAssets = targetPaths;
        }

        [MenuItem("Assets/Import Game Maker Project %m")]
        public static void Import()
        {
            if (!ImportSettings.InstanceExists)
            {
                GenerateImportSettings();
                EditorUtility.DisplayDialog("Settings Required", "Set Import Settings and Run Importer Again.", "OK");
                return;
            }
            string gameMakerProjectFile = EditorUtility.OpenFilePanel("Find Game Maker .project.gmx", "", "project.gmx");
            basePath = GetBasePath(gameMakerProjectFile);

            XmlElement root = GetRootXmlElement(gameMakerProjectFile);

            sounds = root.SelectNodes("sounds");
            sprites = root.SelectNodes("sprites");
            backgrounds = root.SelectNodes("backgrounds");
            objects = root.SelectNodes("objects");
            rooms = root.SelectNodes("rooms");

            EditorCoroutineRunner.StartCoroutineWithUI(CopyData(), "Copying GM files");

        }

        [MenuItem("Assets/Generate importer settings %#m")]
        public static void GenerateImportSettings()
        {
            Selection.activeObject = ImportSettings.Instance;
        }

        #endregion

        #region coroutines

        protected static IEnumerator CopyData()
        {
            startTime = EditorApplication.timeSinceStartup;

            EditorCoroutineRunner.UpdateUITitle("Import 1/5: Sounds");
            EditorCoroutineRunner.UpdateUILabel("Getting Background data");
            TraverseNodes(sounds, "sound");
            yield return null;
            yield return EditorCoroutineRunner.StartCoroutine(CopyAssets(targetPaths, ImportType.Sound));
            targetPaths = new List<ImportAsset>();

            EditorCoroutineRunner.UpdateUITitle("Import 2/5: Backgrounds");
            EditorCoroutineRunner.UpdateUILabel("Getting Background data");
            TraverseNodes(backgrounds, "background");
            yield return null;
            yield return EditorCoroutineRunner.StartCoroutine(CopyAssets(targetPaths, ImportType.Background));
            targetPaths = new List<ImportAsset>();


            EditorCoroutineRunner.UpdateUITitle("Import 3/5: Sprites");
            EditorCoroutineRunner.UpdateUILabel("Getting Sprite data");
            TraverseNodes(sprites, "sprite");
            yield return null;
            yield return EditorCoroutineRunner.StartCoroutine(CopyAssets(targetPaths, ImportType.Sprite));
            targetPaths = new List<ImportAsset>();

            EditorCoroutineRunner.UpdateUITitle("Import 4/5: Objects");
            EditorCoroutineRunner.UpdateUILabel("Getting Object data");
            TraverseNodes(objects, "object");
            yield return null;
            yield return EditorCoroutineRunner.StartCoroutine(CopyAssets(targetPaths, ImportType.Object));
            targetPaths = new List<ImportAsset>();

            EditorCoroutineRunner.UpdateUITitle("Import 5/5: Rooms");
            EditorCoroutineRunner.UpdateUILabel("Getting Room data");
            TraverseNodes(rooms, "room");
            yield return null;
            yield return EditorCoroutineRunner.StartCoroutine(CopyAssets(targetPaths, ImportType.Room));

            TimeSpan timeSpan = TimeSpan.FromSeconds(EditorApplication.timeSinceStartup - startTime);
            string timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            print("Project Imported! Total Time: <color=#22ffccff></color> " + "<color=#22ee22ff>" + timeText + "</color>");
        }

        protected static IEnumerator CopyAssets(List<ImportAsset> assetData, ImportType importType)
        {
            switch (importType)
            {
                case ImportType.Background:
                    for (int i = 0; i < assetData.Count; i++)
                    {
                        EditorCoroutineRunner.UpdateUI("Importing Background: " + assetData[i].targetName, i/(float)assetData.Count);
                        SpriteImporter.ImportSprite(assetData[i]);
                        yield return null;
                    }
                    break;

                case ImportType.Object:
                    for (int i = 0; i < assetData.Count; i++)
                    {
                        EditorCoroutineRunner.UpdateUI("Importing Object: " + assetData[i].targetName, i/(float)assetData.Count);
                        ObjectImporter.ImportObject(assetData[i]);
                        yield return null;
                    }
                    break;

                case ImportType.Room:
                    for (int i = 0; i < assetData.Count; i++)
                    {
                        EditorCoroutineRunner.UpdateUI("Importing Room: " + assetData[i].targetName, i/(float)assetData.Count);
                        RoomImporter.ImportRoom(assetData[i]);
                        yield return null;
                    }
                    break;

                case ImportType.Sound:
                    for (int i = 0; i < assetData.Count; i++)
                    {
                        EditorCoroutineRunner.UpdateUI("Importing Sound: " + assetData[i].targetName, i/(float)assetData.Count);
                        CheckAndCopy(assetData[i]);
                        yield return null;
                    }
                    break;

                case ImportType.Sprite:
                    for (int i = 0; i < assetData.Count; i++)
                    {
                        EditorCoroutineRunner.UpdateUI("Importing Sprite: " + assetData[i].targetName, i/(float)assetData.Count);
                        SpriteImporter.ImportSprite(assetData[i]);
                        yield return null;
                    }
                    break;
            }
        }

        #endregion

        #region XmlHelpers

        protected static XmlElement GetRootXmlElement(string fileName)
        {
            XmlDocument sourceXML = new XmlDocument();
            sourceXML.Load(fileName);
            XmlElement rootElement = sourceXML.DocumentElement;

            return rootElement;
        }

        #endregion

        #region traversal

        protected static void TraverseNodes(XmlNodeList nodeList, string target, int depth = 0)
        {
            foreach (XmlNode node in nodeList)
            {
                if (node.Name != target)
                {
                    TraverseNodes(node.ChildNodes, target, depth + 1);
                }
                else
                {
                    SetAssetDataFromXML(node, target);
                }
            }
        }

        #endregion

        #region pathing

        protected static string GetBasePath(string relativePath)
        {
            string result = "";

            List<string> parts = new List<string>(relativePath.Split('/'));

            parts.RemoveAt(parts.Count - 1);

            foreach (string part in parts)
            {
                result += part + Path.DirectorySeparatorChar;
            }

            return result;
        }

        protected static string GetRelativeNodePath(XmlNode node)
        {
            if (node.ParentNode == null)
            {
                return "";
            }
            else
            {
                if (node.Attributes["name"] != null)
                {
                    return GetRelativeNodePath(node.ParentNode) + Path.DirectorySeparatorChar + node.Attributes["name"].Value;
                }
                else
                {
                    return GetRelativeNodePath(node.ParentNode);
                }
            }
        }

        protected static string GetCompleteTargetPath( ImportAsset assetData)
        {
            string result = "";

            result = Application.dataPath + assetData.targetPath;
            result = result.Replace('/', Path.DirectorySeparatorChar);

            return result;
        }

        protected static string GetCompleteTargetFilePath( ImportAsset assetData, string extensionType)
        {
            string result = "";

            result += assetData.targetCompletePath + Path.DirectorySeparatorChar + assetData.targetName + extensionType;

            return result;
        }

        #endregion

        #region copying

        protected static void SetAssetDataFromXML(XmlNode node, string targetType)
        {
            ImportAsset importAsset = new ImportAsset(GetRelativeNodePath(node), GetSanitizedName(node.InnerText));
            
            importAsset.sourceXML = basePath + node.InnerText + GetGmxSuffix(node.InnerText);
            XmlElement xmlRoot = GetRootXmlElement(importAsset.sourceXML);
            
            importAsset.targetCompletePath = GetCompleteTargetPath(importAsset);

            ImportType importType = GetImportType(targetType);
            importAsset = AssetDataSource(importAsset, xmlRoot, importType);

            if (importAsset != null)
            {
                targetPaths.Add(importAsset);
            }
        }

        protected static ImportAsset AssetDataSource(ImportAsset importAsset, XmlNode node, ImportType importType)
        {
            switch( importType)
            {
                case ImportType.Background:
                    importAsset.sourceAsset = basePath + "background" + Path.DirectorySeparatorChar + node.SelectSingleNode("//data").InnerText;
                    importAsset.targetCompletePath = GetCompleteTargetPath(importAsset);
                    importAsset.targetCompleteFilePath = GetCompleteTargetFilePath(importAsset, "");
                    break;

                case ImportType.Object:
                    importAsset.targetCompletePath = GetCompleteTargetPath(importAsset);
                    importAsset.targetCompleteFilePath = GetCompleteTargetFilePath(importAsset, "");
                    break;

                case ImportType.Room:
                    importAsset.targetCompletePath = GetCompleteTargetPath(importAsset);
                    importAsset.targetCompleteFilePath = GetCompleteTargetFilePath(importAsset, "");
                    break;

                case ImportType.Sound:
                    importAsset.sourceAsset = basePath + node.SelectSingleNode("origname").InnerText;
                    importAsset.targetCompletePath = GetCompleteTargetPath(importAsset);
                    importAsset.targetCompleteFilePath = GetCompleteTargetFilePath(importAsset, node.SelectSingleNode("extension").InnerText);
                    break;

                case ImportType.Sprite:
                    if (node != null && node.SelectSingleNode("//frame") != null)
                    {
                        importAsset.sourceAsset = basePath + "sprites" + Path.DirectorySeparatorChar + node.SelectSingleNode("//frame").InnerText;
                        importAsset.targetCompletePath = GetCompleteTargetPath(importAsset);
                        importAsset.targetCompleteFilePath = GetCompleteTargetFilePath(importAsset, "");


                        XmlNodeList nodeList = node.SelectNodes("//frame");
                        if (nodeList.Count > 1)
                        {
                            importAsset = new ImportAsset(importAsset);
                            List<string> sourceSprites = new List<string>();

                            foreach (XmlNode nodeling in nodeList)
                            {
                                if (nodeling.InnerText != null)
                                {
                                    sourceSprites.Add(basePath + "sprites" + Path.DirectorySeparatorChar + nodeling.InnerText);
                                }
                            }
                            importAsset.AddSpriteList(sourceSprites);
                        }
                    }
                    else
                    {
                        importAsset = null;
                    }
                    break;

            }

            return importAsset;
        }

        protected static void CheckAndCopy(ImportAsset assetData)
        {
            if (!Directory.Exists(assetData.targetCompletePath))
            {
                Directory.CreateDirectory(assetData.targetCompletePath);
            }

            if (File.Exists(assetData.sourceAsset))
            {
                File.Copy(assetData.sourceAsset, assetData.targetCompleteFilePath, true);

                if (ImportSettings.Instance.ShowLogging)
                {
                    print("Imported audio <color=#22ffccff>" + assetData.targetCompleteFilePath + "</color> " + "<color=#22ee22ff>" + assetData.targetName + "</color>");
                }
            }
            else
            {
                Debug.LogWarning("Cannot find " + assetData.targetName + " where Game Maker said the file should be");
            }
        }

        #endregion

        #region string sanitization

        protected static string GetSanitizedName(string name)
        {
            return name.Split('\\')[1];
        }

        protected static string GetGmxSuffix(string name)
        {
            string result = name.Split('\\')[0];

            if( result[result.Length-1] == 's')
            {
                result = result.Remove(result.Length-1);
            }

            result = "." + result + ".gmx";

            return result;
        }

        protected static ImportType GetImportType(string importString)
        {
            if(importString == "sound")
            {
                return ImportType.Sound;
            }
            if(importString == "sprite")
            {
                return ImportType.Sprite;
            }
            if(importString == "background")
            {
                return ImportType.Background;
            }
            if(importString == "object")
            {
                return ImportType.Object;
            }
            if(importString == "room")
            {
                return ImportType.Room;
            }
            return ImportType.Null;
        }

        #endregion
    }
}
