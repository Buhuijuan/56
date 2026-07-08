using System;
using System.Collections.Generic;

[Serializable]
public class TaskProgressPromptConfig
{
    public string taskCode;
    public string npcName = "AI小精灵";
    public string avatarKey = "elf_default";
    public string triggerLocationId;
    public List<string> contents;
}

[Serializable]
public class TaskProgressPromptConfigListWrapper
{
    public List<TaskProgressPromptConfig> prompts;
}
