namespace BigFood.GraphQL
{
    public record RegisterUser
    (
        int? Id,
        string UserName,
        string Email,
        string Password
    );
}
