namespace api1.Models
{
    public class Point
    {
        public Guid id { get; set; }
        public Coordinates coordinates { get; set; }

        public Point(Guid id, Coordinates coordinates)
        {
            this.id = id;
            this.coordinates = coordinates;
        }
    }
}
