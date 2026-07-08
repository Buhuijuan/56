using System.Collections.Generic;

public static class TaskTargetIds
{
    public const long Reception = 2001;
    public const long DormBamboo3 = 2002;
    public const long CampusHospital = 2003;
    public const long JinLake = 2004;
    public const long BotanicalGarden = 2005;
    public const long Library = 2006;
    public const long ComplexBuilding = 2007;
    public const long TeachingBuilding1 = 2008;
    public const long ExperimentBuilding1 = 2009;
    public const long ArtBuilding = 2010;
    public const long ClockTower = 2011;
    public const long BikeStation = 4001;

    public const long WelcomeVolunteer = 1001;
    public const long ReceptionVolunteer = 1002;
    public const long DormManager = 1003;
    public const long Nurse = 1004;
    public const long BotanicalGardener = 1005;
    public const long Librarian = 1006;
    public const long ComplexGuard = 1007;

    private static readonly Dictionary<long, string> TargetAnchorMap = new Dictionary<long, string>
    {
        [Reception] = "loc_reception",
        [DormBamboo3] = "loc_dorm_bamboo3",
        [CampusHospital] = "loc_campus_hospital",
        [JinLake] = "loc_jin_lake",
        [BotanicalGarden] = "loc_botanical_garden",
        [Library] = "loc_library",
        [ComplexBuilding] = "loc_complex_building",
        [TeachingBuilding1] = "loc_teaching_building_1",
        [ExperimentBuilding1] = "loc_experiment_building_1",
        [ArtBuilding] = "loc_art_square",
        [ClockTower] = "loc_clock_tower",
        [BikeStation] = "loc_bike_station",

        [WelcomeVolunteer] = "loc_welcome_volunteer",
        [ReceptionVolunteer] = "loc_reception_volunteer",
        [DormManager] = "loc_dorm_manager",
        [Nurse] = "loc_nurse",
        [BotanicalGardener] = "loc_botanical_gardener",
        [Librarian] = "loc_librarian",
        [ComplexGuard] = "loc_complex_guard"
    };

    private static readonly HashSet<long> LandmarkIds = new HashSet<long>
    {
        JinLake,
        BotanicalGarden,
        Library,
        ComplexBuilding,
        TeachingBuilding1,
        ExperimentBuilding1,
        ArtBuilding,
        ClockTower
    };

    public static bool TryGetAnchorLocationId(long targetId, out string locationId)
    {
        return TargetAnchorMap.TryGetValue(targetId, out locationId);
    }

    public static bool IsLandmark(long targetId)
    {
        return LandmarkIds.Contains(targetId);
    }

    public static IEnumerable<long> GetLandmarkIds()
    {
        return LandmarkIds;
    }
}
