using System.Collections.Generic;

public static class FakeStoryAI
{
    public static StorySegment GenerateSegment(int segCount, StorySessionState session)
    {
        switch (segCount)
        {
            case 0:
                return new StorySegment
                {
                    segmentText = "每到周三中午，三号食堂的“忆味窗”前总会排起长队。学生们等的不是山珍海味，而是一碗看似普通的——芙蓉蛋花汤。据说，这碗汤里藏着李阿姨从不外传的秘密配方。喝过的人都说，能尝到“记忆里最温暖的味道”。大一新生林晓第一次站在队伍里，前面还有十几个人，汤的香气已经飘了过来。",
                    options = new List<string>
                {
                "好奇询问：“同学，这汤真的有这么神奇吗？”",
                "默默排队，先自己尝尝再说",
                "走到窗口：“阿姨，您的汤里加了什么？”"
                }
                };
            case 3:
                return new StorySegment
                {
                    segmentText = "AI 生成的最终段落……",
                    options = new List<string>()
                };
            default:
                return new StorySegment
                {
                    segmentText = "AI 生成的下一段……",
                    options = new List<string>
            {
                "下一段选项 A",
                "下一段选项 B",
                "下一段选项 C"
            }
                };
        }

    }
}
