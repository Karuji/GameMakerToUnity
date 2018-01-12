using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;

namespace GM2Unity
{
    [InitializeOnLoad]
    public class SpriteImporter : MonoBehaviour
    {

        protected static string path;

        public static void ImportSprite(ImportAsset importAsset = null)
        {

            string shortPath = importAsset.targetPath + Path.DirectorySeparatorChar;
            string[] files;
            Texture2D textured2D;

            if (importAsset.SourceElements != null && importAsset.SourceElements.Count > 0)
            {
                files = importAsset.SourceElements.ToArray();
            }
            else
            {
                files = new string[1];
                files[0] = importAsset.sourceAsset;
            }

            string otherPath = "Assets" + shortPath;
            string assetPath = otherPath + importAsset.targetName + Path.GetExtension(importAsset.sourceAsset);
        
            path = Application.dataPath;
            path = path.Replace('/', Path.DirectorySeparatorChar);
            path += Path.DirectorySeparatorChar + shortPath;

            if (!Directory.Exists(importAsset.targetCompletePath))
            {
                Directory.CreateDirectory(importAsset.targetCompletePath);
            }

            textured2D = GetSpriteTexture(files);

            if (textured2D != null)
            {
                string nameSprite = Path.GetFileNameWithoutExtension(assetPath) + Path.GetExtension(assetPath);
                File.WriteAllBytes(Application.dataPath + shortPath + nameSprite, textured2D.EncodeToPNG());
                Sprite sprite = GetSprite(textured2D);

                AssetDatabase.Refresh();
                if (sprite == null)
                {
                    if (textured2D == null)
                    {
                        textured2D = GetSpriteTexture(files);
                    }

                    sprite = GetSprite(textured2D);
                }
                AssetDatabase.AddObjectToAsset(sprite, otherPath + nameSprite);
                AssetDatabase.SaveAssets();
                UpdateTextureSettings(otherPath, nameSprite, files, importAsset.sourceXML);
                
                if (ImportSettings.Instance.ShowLogging)
                {
                    print("Imported sprite: <color=#22ffccff>" + assetPath + "</color> " + "<color=#22ee22ff>" + nameSprite + "</color>");
                }
            }
            else
            {
                Debug.LogWarning("<color=#ff9999ff>" + importAsset.targetName + "</color><color=#ffdd22ff> cannot be imported, probably because giant texture or something</color>");
            }
        }

        protected static Texture2D GetTexture2D(byte[] rawImageFile)
        {
            Texture2D texture2D = new Texture2D(2, 2);
            texture2D.LoadImage(rawImageFile);

            return texture2D;
        }

        protected static Texture2D GetTexture2D(string imageFile)
        {
            if (!File.Exists(imageFile))
            {
                return null;
            }

            byte[] rawImage = File.ReadAllBytes(imageFile);

            return GetTexture2D(rawImage);

        }

        protected static Texture2D[] GetTextures2D(string[] files)
        {
            Texture2D[] textures = new Texture2D[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                textures[i] = GetTexture2D(files[i]);
            }

            return textures;
        }

        protected static Texture2D GetConsolidatedTexture(Texture2D[] textures)
        {
            if ((textures[0].width * textures.Length > 4096) || (textures[0].height > 4096))
            {
                return null;
            }
            
            Texture2D texture2D = new Texture2D(textures[0].width * textures.Length, textures[0].height);

            for (int i = 0; i < textures.Length; i++)
            {
                Color[] cols = textures[i].GetPixels();
                texture2D.SetPixels(textures[0].width * i, 0, textures[0].width, textures[0].height, cols, 0);
                texture2D.Apply();
            }

            return texture2D;
        }

        protected static Texture2D GetSpriteTexture(string[] files)
        {
            Texture2D texture2D = new  Texture2D(2, 2);

            if (files.Length > 1)
            {
                texture2D = GetConsolidatedTexture(GetTextures2D(files));
            }
            else
            {
                texture2D = GetTexture2D(files[0]);
            }

            return texture2D;
        }

        protected static Sprite GetSprite(Texture2D tex2d)
        {
            Rect rect = new Rect(0f, 0f, tex2d.width, tex2d.height);

            Sprite sprite = Sprite.Create(tex2d, rect, Vector2.zero);

            return sprite;
        }

        protected static Vector4 GetSpriteSizeData(string xmlPath)
        {
            Vector4 result = new Vector4();

            XmlDocument sourceXml = new XmlDocument();
            sourceXml.Load( xmlPath);
            XmlElement rootElement = sourceXml.DocumentElement;

            result.x = int.Parse(rootElement.SelectSingleNode("width").InnerText);
            result.y = int.Parse(rootElement.SelectSingleNode("height").InnerText);

            if(rootElement.SelectSingleNode("xorig") != null)
            {
                result.z = int.Parse(rootElement.SelectSingleNode("xorig").InnerText);
                result.w = int.Parse(rootElement.SelectSingleNode("yorigin").InnerText);

                // Game Maker measures the origin point in pixels,
                // and Unity measures it as a % of the sprite.
                result.z /= result.x;

                // Game Maker's start is top left, Unity's is bottom left.
                result.w = 1 - (result.w / result.y);
            }
            else
            {
                result.z = 0;
                result.w = 1;
            }

            return result;
        }

        protected static SpriteAlignment GetSpriteAlignment(Vector4 sizeData)
        {
            //Center = 0, TopLeft = 1, TopCenter = 2, TopRight = 3, LeftCenter = 4, RightCenter = 5, BottomLeft = 6, BottomCenter = 7, BottomRight = 8, Custom = 9.
            float w = sizeData.z;
            float h = sizeData.w;

            if (w == 0.5f && h == 0.5f)
            {
                return SpriteAlignment.Center;
            }
            else
            if (w == 0f && h == 1f)
            {
                return SpriteAlignment.TopLeft;
            }
            else
            if (w == 0.5f && h == 1f)
            {
                return SpriteAlignment.TopCenter;
            }
            else
            if (w == 1f && h == 1f)
            {
                return SpriteAlignment.TopRight;
            }
            else
            if (w == 0f && h == 0.5f)
            {
                return SpriteAlignment.LeftCenter;
            }
            else
            if (w == 1f && h == 0.5f)
            {
                return SpriteAlignment.RightCenter;
            }
            else
            if (w == 0f && h == 0f)
            {
                return SpriteAlignment.BottomLeft;
            }
            else
            if (w == 0.5f && h == 0f)
            {
                return SpriteAlignment.BottomCenter;
            }
            else
            if (w == 1f && h == 0f)
            {
                return SpriteAlignment.BottomRight;
            }

            return SpriteAlignment.Custom;
        }

        protected static SpriteMetaData[] GetSpriteMetaData(string nameSprite, int numberSubSprites, Vector4 sizeData)
        {
            List<SpriteMetaData> spriteMetaList = new List<SpriteMetaData>();
            string name = Path.GetFileNameWithoutExtension(nameSprite);

            for (int i = 0; i < numberSubSprites; i++)
            {
                SpriteMetaData spriteMetaData = new SpriteMetaData();
                spriteMetaData.name = name + "_" + i.ToString();
                spriteMetaData.rect = new Rect(sizeData.x * i, 0f, sizeData.x, sizeData.y);
                SpriteAlignment alignment = GetSpriteAlignment(sizeData);
                if (alignment == SpriteAlignment.Custom)
                {
                    spriteMetaData.alignment = (int)SpriteAlignment.Custom;
                    spriteMetaData.pivot = new Vector2(sizeData.z, sizeData.w);
                }
                else
                {
                    spriteMetaData.alignment = (int)alignment;
                }
                
                spriteMetaList.Add(spriteMetaData);
            }

            return spriteMetaList.ToArray();
        }

        protected static SpriteMetaData GetSpriteMetaData(Vector4 sizeData)
        {
            SpriteMetaData spriteMetaData = new SpriteMetaData();
            spriteMetaData.rect = new Rect(0f, 0f, sizeData.x, sizeData.y);
            spriteMetaData.alignment = (int)SpriteAlignment.Custom;
            spriteMetaData.pivot = new Vector2(sizeData.z, sizeData.w);

            return spriteMetaData;
        }

        protected static void UpdateTextureSettings(string unityPath, string assetName, string[] files, string spriteXml)
        {
            string assetPath = unityPath + assetName;
            TextureImporter textureImporter = TextureImporter.GetAtPath(assetPath) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.npotScale = TextureImporterNPOTScale.None;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.mipmapEnabled = false;
            textureImporter.spritePixelsPerUnit = ImportSettings.Instance.PixelsPerUnit;

            if (files.Length > 1)
            {
                textureImporter.spriteImportMode = SpriteImportMode.Multiple;
                textureImporter.spritesheet = GetSpriteMetaData(assetName, files.Length, GetSpriteSizeData(spriteXml));
                
            }
            else
            {
                textureImporter.spriteImportMode = SpriteImportMode.Single;	

                TextureImporterSettings importerSettings = new TextureImporterSettings();
                textureImporter.ReadTextureSettings(importerSettings);

                Vector4 sizeData = GetSpriteSizeData(spriteXml);
                SpriteAlignment alignment = GetSpriteAlignment(sizeData);

                if (alignment == SpriteAlignment.Custom)
                {
                    importerSettings.spriteAlignment = (int)SpriteAlignment.Custom;
                    importerSettings.spritePivot = new Vector2(sizeData.z, sizeData.w);
                }
                else
                {
                    importerSettings.spriteAlignment = (int)alignment;
                }

                textureImporter.SetTextureSettings(importerSettings);
            }        

            EditorUtility.SetDirty(textureImporter);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}