using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GM2Unity.Utility;

namespace GM2Unity
{
	public class ImportSettings : SingletonScriptableObject<ImportSettings>
	{
		public int PixelsPerUnit = 10;
		public bool ShowLogging = true;

		
		void OnValidate()
		{
			if (PixelsPerUnit < 1)
			{
				PixelsPerUnit = 1;
				Debug.LogWarning("Pixels per unit cannot be less than 1");
			}
		}
	}
}