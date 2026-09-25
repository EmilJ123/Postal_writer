using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class S_SaveData 
{
   public Vector3 playerPosition;
   public string mapBoundary; //The boundary name for the map
   public List<S_InventorySaveData> inventorySaveData;
}
