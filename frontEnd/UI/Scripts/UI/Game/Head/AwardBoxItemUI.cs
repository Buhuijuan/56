using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AwardBoxItemUI : MonoBehaviour
{

    public Sprite lockedImage, unclaimedImage, claimedImage;
    public Button button;
    public Image icon;
    private AwardStateType currentState;

    public void UpdateSprite()
    {
        switch (currentState)
        {
            case AwardStateType.Locked:
                icon.sprite = lockedImage;
                button.interactable = false;
                break;
            case AwardStateType.UnClaimed:
                icon.sprite = unclaimedImage;
                button.interactable = true;
                break;
            case AwardStateType.Claimed:
                icon.sprite = claimedImage;
                button.interactable = false;
                break;
        }
    }
    public AwardStateType GetCurrentState()
    {
        return currentState;
    }
    public void SetCurrentState(AwardStateType newState)
    {
        currentState = newState;
    }
}
