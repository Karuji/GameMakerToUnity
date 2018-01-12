using System;
using System.Collections.Generic;

namespace GM2Unity
{
    [Serializable]
    public class ImportAsset
    {
        public string targetName;
        public string targetPath;
        public string targetCompletePath;
        public string targetCompleteFilePath;
        public string sourceXML;
        public string sourceAsset;

        public List<string> SourceElements;

        public ImportAsset()
        {

        }

        public ImportAsset( string TargetPath, string TargetName)
        {
            targetPath = TargetPath;
            targetName = TargetName;
        }

        public ImportAsset( ImportAsset importAsset)
        {
            targetName = importAsset.targetName;
            targetPath = importAsset.targetPath;
            targetCompletePath = importAsset.targetCompletePath;
            targetCompleteFilePath = importAsset.targetCompleteFilePath;
            sourceXML = importAsset.sourceXML;
            sourceAsset = importAsset.sourceAsset;
        }

        public void AddSpriteList(string[] spriteList)
        {
            SourceElements = new List<string>(spriteList);
        }

        public void AddSpriteList(List<string> spriteList)
        {
            SourceElements = spriteList;
        }
    }
}
