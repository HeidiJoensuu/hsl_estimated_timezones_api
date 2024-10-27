using api1.GraphQL;
using api1.Models;
using GraphQL;

namespace api1.Service
{
    public interface IGraphQLService
    {
        Task<GraphQLResponse<ResponseType>?> Get(Coordinates oldCoords, Coordinates newCoords);
    }
}
