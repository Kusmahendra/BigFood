namespace BigFood.GraphQL
{
    public record OrderDetailsInput
    (
        int Id,
        int FoodId,
        int Quantity,
        int CourierId
    );
    
}