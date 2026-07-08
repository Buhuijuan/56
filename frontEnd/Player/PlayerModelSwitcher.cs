using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelSwitcher : MonoBehaviour
{
    public static PlayerModelSwitcher Instance;
    public GameObject walkModel;
    public GameObject bikeModel;
    public float walkSpeed = 3.5f;
    public float bikeSpeed = 10f;

    private PlayerAgentMove move;
    private bool currentBikeMode;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        move = PlayerAgentMove.Instance;
        SetBikeMode(false);
    }
    public void SetBikeMode(bool isBike)
    {
        bool enteredBikeMode = !currentBikeMode && isBike;
        currentBikeMode = isBike;

        walkModel.SetActive(!isBike);
        bikeModel.SetActive(isBike);

        if (move == null) move = PlayerAgentMove.Instance;
        if (move == null) return;
        move.isBikeMode = isBike;

        if (isBike)
        {
            move.moveSpeed = bikeSpeed;
            move.isAccelerating = false;
        }
        else
        {
            move.moveSpeed = walkSpeed;
        }

        if (enteredBikeMode)
            TitleEventReporter.ReportBikeRide();
    }

}
