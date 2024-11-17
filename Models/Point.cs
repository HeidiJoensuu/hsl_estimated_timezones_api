namespace api1.Models
{
    public class Point
    {
        public Guid Id { get; set; }
        public Coordinates Coordinates { get; set; }


        public Point(Guid id, Coordinates coordinates)
        {
            this.Id = id;
        }
    }
}
