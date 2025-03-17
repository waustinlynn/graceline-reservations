using GracelineCMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GracelineCMS.Domain.Permissions
{
    public interface IUserGroupService
    {
        Task CreateUserGroup(string userId, string organizationId, string userGroupName);
    }

    public enum DefaultUserGroupType
    {
        Admin
    }

}
