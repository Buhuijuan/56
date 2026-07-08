public static class CharacterCreationSession
{
    public static string tempCampusName;
    public static string tempNickName;
    public static int selectedImageId;

    public static void Clear()
    {
        tempCampusName = string.Empty;
        tempNickName = string.Empty;
        selectedImageId = 0;
    }
}
