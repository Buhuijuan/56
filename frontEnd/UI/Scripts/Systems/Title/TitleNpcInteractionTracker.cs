using UnityEngine;

public class TitleNpcInteractionTracker : MonoBehaviour
{
    public string npcId;
    public long taskNpcTargetId;

    public void RecordInteraction()
    {
        string resolvedNpcId = string.IsNullOrWhiteSpace(npcId) ? gameObject.name : npcId;
        TitleProgressSystem.RecordNpcConversation(resolvedNpcId);

        long targetId = ResolveTaskNpcTargetId(resolvedNpcId);
        if (targetId > 0)
            TaskEventRuntime.ReportNpcDialogue(targetId);
    }

    private void OnMouseDown()
    {
        RecordInteraction();
    }

    private long ResolveTaskNpcTargetId(string resolvedNpcId)
    {
        if (taskNpcTargetId > 0)
            return taskNpcTargetId;

        if (string.IsNullOrWhiteSpace(resolvedNpcId))
            return 0;

        string normalized = resolvedNpcId.Trim().ToLowerInvariant();
        return normalized switch
        {
            "1001" => TaskTargetIds.WelcomeVolunteer,
            "1002" => TaskTargetIds.ReceptionVolunteer,
            "1003" => TaskTargetIds.DormManager,
            "1004" => TaskTargetIds.Nurse,
            "welcomevolunteer" => TaskTargetIds.WelcomeVolunteer,
            "welcome_volunteer" => TaskTargetIds.WelcomeVolunteer,
            "npc_welcome_volunteer" => TaskTargetIds.WelcomeVolunteer,
            "receptionvolunteer" => TaskTargetIds.ReceptionVolunteer,
            "reception_volunteer" => TaskTargetIds.ReceptionVolunteer,
            "npc_reception_volunteer" => TaskTargetIds.ReceptionVolunteer,
            "dormmanager" => TaskTargetIds.DormManager,
            "dorm_manager" => TaskTargetIds.DormManager,
            "npc_dorm_manager" => TaskTargetIds.DormManager,
            "宿管阿姨" => TaskTargetIds.DormManager,
            "宿管" => TaskTargetIds.DormManager,
            "nurse" => TaskTargetIds.Nurse,
            "npc_nurse" => TaskTargetIds.Nurse,
            "护士" => TaskTargetIds.Nurse,
            "医生" => TaskTargetIds.Nurse,
            "1007" => TaskTargetIds.ComplexGuard,
            "complexguard" => TaskTargetIds.ComplexGuard,
            "complex_guard" => TaskTargetIds.ComplexGuard,
            "npc_complex_guard" => TaskTargetIds.ComplexGuard,
            "综合楼保安" => TaskTargetIds.ComplexGuard,
            "保安" => TaskTargetIds.ComplexGuard,
            _ => 0
        };
    }
}
