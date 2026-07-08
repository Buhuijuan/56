using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTaskState
{
    public HashSet<string> completedTasks = new();
    public HashSet<string> completedGoals = new();
    public HashSet<string> claimedTasks = new();
}
