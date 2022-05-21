namespace BigFood.GraphQL
{
    public record UpdateOrderInput
    (
        string LocationLat,
        string LocationLong,
        int StepCount
    );
}