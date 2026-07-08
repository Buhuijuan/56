using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterCreationUIManager : MonoBehaviour
{
    public CharacterNamePanelUI namePanel;
    public CharacterImagePanelUI imagePanel;

    public void OnClickBack()
    {
        if (imagePanel.gameObject.activeSelf)
        {
            imagePanel.gameObject.SetActive(false);
            namePanel.gameObject.SetActive(true);
            return;
        }

        if (namePanel.gameObject.activeSelf)
        {
            GameSceneManager.Instance.SwitchScene("03_Menu");
        }
    }
}
