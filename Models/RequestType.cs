namespace api1.Models
{
    public class RequestType
    {
        public Coordinates coordinates { get; set; }
        public string date { get; set; }
        public string time { get; set; }

        public RequestType(Coordinates coordinates, string date, string time) {
            this.coordinates = coordinates;
            this.time = time;
            this.date = date;
        }
    }
}
