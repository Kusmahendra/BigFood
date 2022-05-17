namespace BigFood.GraphQL
{
    public record OrderInput
    (
        int FoodId,
        int CourierId,
        int Quantity
    );
        
    
}