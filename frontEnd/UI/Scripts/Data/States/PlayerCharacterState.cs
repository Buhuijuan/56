using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterState
{
    public HashSet<int> unlockedCharacters = new();
    public int currentCharacterID;
}
