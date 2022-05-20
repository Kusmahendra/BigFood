namespace BigFood.GraphQL
{
    public record AssignRoleInput
    (
        int UserId,
        int RoleId,
        string Message
    );
}