namespace BigFood.GraphQL
{
    public class CalculateDistance
    {
            public double GetDistance(double longitude, double latitude, double otherLongitude, double otherLatitude)
            {
                if ((latitude == otherLatitude) && (longitude == otherLongitude)) {
                    return 0;
                }
                else 
                {
                    var d1 = latitude * (Math.PI / 180.0);
                    var num1 = longitude * (Math.PI / 180.0);
                    var d2 = otherLatitude * (Math.PI / 180.0);
                    var num2 = otherLongitude * (Math.PI / 180.0) - num1;
                    var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
                    
                    return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
                }
            }

            public double distance(double lat1, double lon1, double lat2, double lon2) 
            {
                if ((lat1 == lat2) && (lon1 == lon2)) {
                    return 0;
                }
                else {
                    double theta = lon1 - lon2;
                    double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
                    dist = Math.Acos(dist);
                    dist = rad2deg(dist);
                    dist = dist * 1.609344;
                    return (dist);
                }
            }
            public double rad2deg(double rad) 
            {
                return (rad / Math.PI * 180.0);
            }
            public double deg2rad(double deg) 
            {
                return (deg * Math.PI / 180.0);
            }
    }
}