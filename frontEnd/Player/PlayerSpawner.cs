using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    public Transform spawnPoint;
    private GameObject currentPlayer;

    void Start()
    {
        SpawnPlayer();
        CharacterSystem.OnCharacterChanged += SpawnPlayer;
    }

    void OnDestroy()
    {
        CharacterSystem.OnCharacterChanged -= SpawnPlayer;
    }

    void SpawnPlayer()
    {
        Vector3 pos;
        Quaternion rot;

        if (currentPlayer != null)
        {
            pos = currentPlayer.transform.position;
            rot = currentPlayer.transform.rotation;

            Destroy(currentPlayer);
        }
        else
        {
            pos = spawnPoint.position;
            rot = spawnPoint.rotation;
        }

        int id = CharacterSystem.characterState.currentCharacterID;
        CharacterData data = CharacterSystem.GetCharacterData(id);

        currentPlayer = Instantiate(data.characterPrefab, pos, rot);
        var campusScene = SceneManager.GetSceneByName("05_Campus");
        SceneManager.MoveGameObjectToScene(currentPlayer, campusScene);
        // 绑定控制器
        PlayerAgentMove.Instance = currentPlayer.GetComponent<PlayerAgentMove>();
        PlayerModelSwitcher.Instance = currentPlayer.GetComponent<PlayerModelSwitcher>();

        // 摄像机绑定
        var cam = FindObjectOfType<CameraOrbitByJoystick>();
        cam.target = currentPlayer.transform;
        cam.pivot = currentPlayer.transform.Find("CameraPivot");

        // 宠物绑定
        var pet = FindObjectOfType<PetFollow>();
        pet.followTarget = currentPlayer.transform.Find("PetFollowPoint");
    }

}
