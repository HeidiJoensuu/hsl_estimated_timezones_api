using api1.GraphQL;

namespace api1.Models
{
    public class ResponseType
    {
        public Plan Plan { get; set; }
    }

    public class Plan
    {
        public List<Itinerary> Itineraries { get; set; }
    }

    public class Itinerary
    {
        public long Duration { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
    }
}
