using api1.Models;
using api1.Service;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

namespace api1.GraphQL
{
    public class GraphQLService: IGraphQLService
    {
        private readonly IConfiguration _config;
        private string ConnectionString;

        
        public GraphQLService(IConfiguration configuration)
        {
            _config = configuration;
        }

        public async Task<GraphQLResponse<ResponseType>?> Get(Coordinates oldCoords, Coordinates newCoords)
        {
            string fromLat = oldCoords.lat.ToString().Replace(",", ".");
            string fromLon = oldCoords.lng.ToString().Replace(",", ".");
            string toLat = newCoords.lat.ToString().Replace(",", ".");
            string toLon = newCoords.lng.ToString().Replace(",", ".");

            //string fromLat = "60.2387503";
            //string fromLon = "24.8045110";
            //string toLat = "60.64701964";
            //string toLon = "25.368289";

            Console.WriteLine(fromLat +", " + fromLon + " | " + toLat + ", " + toLon);

            var graphQLClient = new GraphQLHttpClient(
                "https://api.digitransit.fi/routing/v1/routers/hsl/index/graphql?digitransit-subscription-key=" + _config["digitransit-subscription-key"],
                new NewtonsoftJsonSerializer());

            var testiRequest = new GraphQLRequest
            {
                Query = $$"""
                query myQuery {
                plan(
                    from: { lat: {{fromLat}}, lon: {{fromLon}} },
                    to: { lat: {{toLat}}, lon: {{toLon}} },
                    numItineraries: 4
                    allowedTicketTypes: "HSL:ABCD"
                ) {
                    itineraries {
                        duration
                        startTime
                        endTime
                        }
                    }
                }
                """,
                OperationName = "myQuery",
            };
            int timeout = 10000;
            var task = graphQLClient.SendQueryAsync<ResponseType>(testiRequest);

            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                
                try
                {
                    Console.WriteLine("Running");
                    return await task;
                }
                catch (GraphQLHttpRequestException e)
                {
                    Console.WriteLine(e.Message + "\n" + e.Content);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
