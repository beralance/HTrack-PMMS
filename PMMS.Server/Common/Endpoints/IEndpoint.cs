namespace PMMS.Server.Common.Endpoints
{
    public interface IEndpoint
    {
        void MapEndpoint(IEndpointRouteBuilder app);
    }
}