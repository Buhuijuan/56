using System;
using System.Collections.Generic;

[Serializable]
public class StoryRecord
{
    public string storyId;
    public string authorId;
    public string theme;
    public DateTime createdAt;
    public List<StorySegment> segments;
    public string fullText;
}
