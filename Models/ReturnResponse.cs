namespace api1.Models
{
    public class ReturnResponse
    {
        public Guid Id { get; set; }
        public Coordinates Coordinates { get; set; }
        public List<Itinerary> Itineraries { get; set; }

        public ReturnResponse(Guid id, Coordinates coordinates, List<Itinerary> itineraries) { 
            this.Id = id;
            this.Coordinates = coordinates;
            this.Itineraries = itineraries;
        }
    }
}
