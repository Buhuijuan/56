using System;
using System.Collections.Generic;

[Serializable]
public class QuizSessionState
{
    public int currentIndex;                 // 0~9
    public List<QuizQuestion> questions;     // 本次抽取的10题
    public List<int> userAnswers;            // 玩家选择
    public bool isFinished;
}
