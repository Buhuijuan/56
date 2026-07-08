using System;
using System.Collections.Generic;

[Serializable]
public class QuizQuestion
{
    public string questionId;
    public string questionText;
    public List<string> options;
    public int correctIndex;
    public string explanation;
}
