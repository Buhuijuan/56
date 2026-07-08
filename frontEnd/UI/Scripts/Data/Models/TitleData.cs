using System;

[Serializable]
public class TitleData
{
    public int titleID;
    public string typeString;
    [NonSerialized]
    public TitleType type;
    public string titleName;
}
