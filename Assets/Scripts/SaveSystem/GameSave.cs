using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSave
{
    //string key = GUID Gameobject ID
    public Dictionary<string, GameObjectSave> gameObjectData;

    public GameSave()
    {
        gameObjectData = new Dictionary<string, GameObjectSave>();
    }
}
