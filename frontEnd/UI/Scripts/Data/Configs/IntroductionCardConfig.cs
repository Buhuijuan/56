using System;
using System.Collections.Generic;

[Serializable]
public class IntroductionCardConfig
{
    public string locationId;
    public string name;
    public string introduction;
}

[Serializable]
public class IntroductionCardConfigListWrapper
{
    public List<IntroductionCardConfig> cards;
}
