using System;
using System.Collections.Generic;

[Serializable]
public class TaskNpcDialogueConfig
{
    public string taskCode;
    public long triggerTargetId;
    public string npcId;
    public string npcName;
    public string avatarKey;
    public string eventType;
    public string reportTargetType;
    public long reportTargetId;
    public bool reportTaskEvent;
    public List<string> contents;
}

[Serializable]
public class TaskNpcDialogueConfigListWrapper
{
    public List<TaskNpcDialogueConfig> dialogues;
}
