using System;
using System.Collections.Generic;
using UnityEngine;

public static class FakeAI
{
    static List<string> defaultReplies = new List<string>()
    {
        "这个问题很有意思，我来想想……",
        "让我整理一下思路，再告诉你。",
        "嗯，我大概明白你的意思了。",
        "如果换个角度思考，也许会有新的发现。",
        "我觉得你问得很好，我们可以从几个方面来看……"
    };

    public static string GetAnswer(string question)
    {
        question = question.Trim();

        if (question.Contains("你好") || question.Contains("hello"))
            return "你好呀，很高兴见到你！有什么想问我的吗？";

        if (question.Contains("食堂") || question.Contains("吃什么"))
            return "我推荐你试试三号食堂的芙蓉蛋花汤，据说里面有秘密配方。";

        if (question.Contains("天气"))
            return "今天的天气挺不错的，适合散步，也适合创作故事。";

        if (question.Contains("故事"))
            return "如果你想写故事，我可以帮你构思开头、角色和冲突点。";

        if (question.Contains("学校") || question.Contains("校园"))
            return "校园里最近挺热闹的，听说活动墙上又多了不少新故事。";

        if (question.Contains("帮助") || question.Contains("怎么用"))
            return "你可以在输入框里向我提问，我会尽力回答你的问题。";
        if (question.Contains("找路") || question.Contains("图书馆"))
            return "从你现在所在的教学楼A座正门出发，沿着门前那条宽阔的主干道向东直行。这条道路两旁种满了高大的梧桐树，春天时嫩芽初绽，秋天则铺满金黄落叶，非常好认。大约步行200米后，你会经过第二食堂——那是一座红白相间的两层建筑，饭点时总能闻到饭菜香气。继续向前走约50米，会遇到第一个岔路口，此时请右转，进入一条相对安静的林荫道。沿着这条路一直走，你会路过一个小型广场，广场中央有一座圆形喷泉，晴天时常常能看到有人在附近长椅上看书休息。从喷泉处继续向前，大约再走3分钟，视野会逐渐开阔。这时抬头向前方望去，你会看到一座带有玻璃穹顶的红色建筑，阳光照射下穹顶会闪闪发光，那就是图书馆。建筑主体共有五层，外墙由红砖和玻璃幕墙构成，显得既庄重又现代。走到建筑前方，你会发现图书馆的主入口位于正中央，门前有几级宽阔的石阶，两侧各有一盏复古风格的路灯。入口上方悬挂着金色的馆名标识，非常醒目。推门而入，便是宽敞明亮的图书馆大厅了。";
        return defaultReplies[UnityEngine.Random.Range(0, defaultReplies.Count)];
    }
}
