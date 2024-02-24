using GraphQL.Introspection;

namespace api.services
{
    public class UnitCircle: IUnitCircle

    {
        public UnitCircle() { }

        public List<Tuple<double, double>> CreateCircle(double radius, double longitude, double latitude)
        {
            List<Tuple<double, double>> points = new List<Tuple<double, double>>();
            double range = 0.001;
            double angle = getRationAngle(range, radius);

            for (double i = 0; i < 360; i += angle)
            {
                Console.WriteLine(i);
                points.Add(cordinants(i, radius, longitude, latitude));
            }
            
            return points;
        }

        private double getRationAngle(double range, double radius)
        {
            Console.WriteLine($"{range} * 360) / 2 * Math.pi * {radius}");
            return radius / range;
            //return (range * 360) / 2 * Math.PI * radius;
        }

        private Tuple<double, double> cordinants(double angle, double radius, double longitude, double latitude)
        {
            double rad = Math.PI * angle / 180.0;
            //Console.WriteLine($"{longitude} + ({radius} * {Math.Sin(angle)}) angle: {angle}, {radius * Math.Sin(rad)}");
            //Console.WriteLine($"{latitude} + ({radius} * {Math.Cos(angle)}) angle: {angle}, {radius * Math.Cos(rad)}");

            double longitudeReturn = Math.Round(longitude + (radius * Math.Sin(rad)), 12);
            double latitudeReturn = Math.Round(latitude + (radius * Math.Cos(rad)), 12);

            //Console.WriteLine($"[{latitudeReturn}:{longitudeReturn}]");

            return new Tuple<double, double>(longitudeReturn, latitudeReturn);
        }
    }
}
