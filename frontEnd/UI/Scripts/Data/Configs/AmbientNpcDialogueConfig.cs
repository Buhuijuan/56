using System;
using System.Collections.Generic;

[Serializable]
public class AmbientNpcDialogueConfig
{
    public string npcId;
    public string npcName;
    public string avatarKey;
    public string locationId;
    public float cooldownSeconds = 20f;
    public List<string> contents;
}

[Serializable]
public class AmbientNpcDialogueConfigListWrapper
{
    public List<AmbientNpcDialogueConfig> dialogues;
}
