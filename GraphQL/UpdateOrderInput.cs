namespace BigFood.GraphQL
{
    public record UpdateOrderInput
    (
        double LocationLat,
        double LocationLong,
        int StepCount
    );
}