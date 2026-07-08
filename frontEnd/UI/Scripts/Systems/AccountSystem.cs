using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class AccountSystem
{
    private static AccountData currentAccount;
    private static RoleData currentRole;

    public static void InitFromBackend(AccountData account, List<RoleData> roles, long currentRoleId)
    {
        currentAccount = account ?? new AccountData();
        currentAccount.roles = roles ?? new List<RoleData>();

        if (currentRoleId != 0)
            currentRole = currentAccount.roles.Find(r => r.roleID == currentRoleId.ToString());
        else if (currentAccount.roles.Count > 0)
            currentRole = currentAccount.roles[0];
        else
            currentRole = null;
    }

    public static void UpdateCurrentRole(RoleData updatedRole)
    {
        if (currentAccount == null || updatedRole == null)
            return;

        int index = currentAccount.roles.FindIndex(r => r.roleID == updatedRole.roleID);
        if (index >= 0)
            currentAccount.roles[index] = updatedRole;

        if (currentRole != null && currentRole.roleID == updatedRole.roleID)
            currentRole = updatedRole;
    }

    public static void SetCurrentRole(long roleId)
    {
        if (currentAccount == null || currentAccount.roles == null)
        {
            currentRole = null;
            return;
        }

        currentRole = currentAccount.roles.Find(r => r.roleID == roleId.ToString());
    }

    public static AccountData GetCurrentAccount() => currentAccount;

    public static RoleData GetCurrentRole() => currentRole;

    public static List<RoleData> GetRoles()
    {
        if (currentAccount == null)
            return new List<RoleData>();

        currentAccount.roles ??= new List<RoleData>();
        return currentAccount.roles;
    }

    public static IEnumerator CreateRole(string campusName, string nickname, int avatarId, Action<bool, string> callback)
    {
        bool ok = false;
        string msg = string.Empty;

        yield return BackendFacade.CreateRole(campusName, nickname, avatarId, result =>
        {
            if (result.Success && result.Data != null && result.Data.success)
            {
                ok = true;
            }
            else
            {
                ok = false;
                msg = result.Message ?? result.Data?.message ?? "Create role failed";
            }
        });

        if (!ok)
        {
            callback?.Invoke(false, msg);
            yield break;
        }

        bool restoreOk = false;
        string restoreErr = string.Empty;

        yield return BackendFacade.RestoreSession((success, error) =>
        {
            restoreOk = success;
            restoreErr = error;
        });

        if (!restoreOk)
        {
            callback?.Invoke(false, restoreErr ?? "Role initialization failed");
            yield break;
        }

        TitleEventReporter.ReportCharacterCreated();
        callback?.Invoke(true, "Create role succeeded");
    }

    public static IEnumerator DeleteRole(RoleData role, Action<bool, string> callback)
    {
        if (currentAccount == null || role == null)
        {
            callback?.Invoke(false, "Current account or role is invalid");
            yield break;
        }

        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            if (!long.TryParse(role.roleID, out long roleId))
            {
                callback?.Invoke(false, "Invalid role id");
                yield break;
            }

            bool deleteOk = false;
            string deleteErr = string.Empty;
            yield return BackendFacade.DeleteRole(roleId, result =>
            {
                deleteOk = result.Success;
                if (!deleteOk)
                    deleteErr = result.Message ?? "Delete role failed";
            });

            if (!deleteOk)
            {
                callback?.Invoke(false, deleteErr);
                yield break;
            }

            bool restoreOk = false;
            string restoreErr = string.Empty;
            yield return BackendFacade.RestoreSession((success, error) =>
            {
                restoreOk = success;
                restoreErr = error;
            });

            if (!restoreOk)
            {
                callback?.Invoke(false, restoreErr ?? "Role refresh failed");
                yield break;
            }

            callback?.Invoke(true, "Role deleted");
            yield break;
        }

        currentAccount.roles.Remove(role);
        if (currentRole == role)
            currentRole = null;
        callback?.Invoke(true, "Role deleted locally");
    }

    public static IEnumerator ModifyPassword(string newPwd, Action<bool, string> callback)
    {
        if (currentAccount == null)
        {
            callback?.Invoke(false, "Current account is invalid");
            yield break;
        }

        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            bool ok = false;
            string err = string.Empty;
            yield return BackendFacade.ChangePassword(newPwd, result =>
            {
                ok = result.Success;
                if (!ok)
                    err = result.Message ?? "Modify password failed";
            });

            if (!ok)
            {
                callback?.Invoke(false, err);
                yield break;
            }
        }

        currentAccount.password = newPwd;
        callback?.Invoke(true, "Password updated");
        yield break;
    }

    public static IEnumerator ChangeMailbox(string newMailbox, Action<bool, string> callback)
    {
        if (currentAccount == null)
        {
            callback?.Invoke(false, "Current account is invalid");
            yield break;
        }

        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            bool ok = false;
            string err = string.Empty;
            yield return BackendFacade.ChangeMailbox(newMailbox, result =>
            {
                ok = result.Success;
                if (!ok)
                    err = result.Message ?? "Change mailbox failed";
            });

            if (!ok)
            {
                callback?.Invoke(false, err);
                yield break;
            }
        }

        currentAccount.mailbox = newMailbox;
        if (SessionStore.HasToken)
        {
            SessionSnapshot snapshot = SessionStore.Current;
            snapshot.mailbox = newMailbox;
            SessionStore.Save(snapshot);
        }

        callback?.Invoke(true, "Mailbox updated");
        yield break;
    }

    public static IEnumerator DeleteAccount(Action<bool, string> callback)
    {
        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            bool ok = false;
            string err = string.Empty;
            yield return BackendFacade.DeleteAccount(result =>
            {
                ok = result.Success;
                if (!ok)
                    err = result.Message ?? "Delete account failed";
            });

            if (!ok)
            {
                callback?.Invoke(false, err);
                yield break;
            }
        }

        currentAccount = null;
        currentRole = null;
        SessionStore.Clear();
        GameStateStore.Clear();

        callback?.Invoke(true, "Account deleted");
        yield break;
    }

    public static bool IsValidEmail(string email)
    {
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }

    public static bool IsValidPwd(string pwd)
    {
        string pattern = @"^(?=.*[0-9])(?=.*[a-zA-Z])[a-zA-Z0-9]{8,20}$";
        return Regex.IsMatch(pwd, pattern);
    }
}
