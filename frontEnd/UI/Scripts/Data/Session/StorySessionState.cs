using System;
using System.Collections.Generic;

[Serializable]
public class StorySessionState
{
    public string eventId;
    public string theme;
    public List<StorySegment> segments = new();
    public int currentRound;
    public bool isFinished;
}
