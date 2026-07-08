using System.Collections.Generic;

public static class GameStateStore
{
    public static BackendPlayerMeData Me;
    public static BackendGameHomeData Home;
    public static BackendSignInPageData SignIn;
    public static BackendClockInPageData ClockIn;
    public static BackendQuizCurrentPageData Quiz;
    public static BackendTaskPageData Tasks;

    public static void Clear()
    {
        Me = null;
        Home = null;
        SignIn = null;
        ClockIn = null;
        Quiz = null;
        Tasks = null;
        BackendTaskStore.Clear();
    }

    public static bool HasRoles => Me != null && Me.roles != null && Me.roles.Count > 0;
    public static bool HasCurrentRole => Home != null && Home.role != null;
}
