namespace BigFood.GraphQL
{
    public record FoodInput
    (
        int? Id,
        string Name,
        int Stock,
        int Price
    );    
    
}