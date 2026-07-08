using System;
using System.Collections.Generic;

[Serializable]
public class AccountData
{
    public string accountID;
    public string mailbox;
    public string password;
    public List<RoleData> roles = new();
}
