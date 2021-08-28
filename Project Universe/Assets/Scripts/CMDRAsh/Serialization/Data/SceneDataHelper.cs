using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SceneDataHelper
{
    //player scene
    [SerializeField] public string PlayerSceneString;
    [SerializeField] public string PlayerSceneSubLocation;//what subscene or section of the game is the player
    [SerializeField] public string DateTime;
    [SerializeField] public string CharacterName;

    public SceneDataHelper(string sceneString, string sublocation, string dateTime, string characterName)
    {
        PlayerSceneString = sceneString;
        PlayerSceneSubLocation = sublocation;
        DateTime = dateTime;
        CharacterName = characterName;
    }
}
