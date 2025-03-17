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
