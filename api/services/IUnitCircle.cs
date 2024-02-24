namespace api.services
{
    public interface IUnitCircle
    {
        List<Tuple<double, double>> CreateCircle(double radius, double longitude, double latitude);
    }
}
