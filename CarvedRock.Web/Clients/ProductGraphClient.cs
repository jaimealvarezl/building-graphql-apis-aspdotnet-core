using System.Threading.Tasks;
using CarvedRock.Web.Models;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using GraphQL.Client.Http;

namespace CarvedRock.Web.Clients
{
    public class ProductGraphClient
    {
        private readonly GraphQLHttpClient _client;

        public ProductGraphClient(GraphQLHttpClient client)
        {
            _client = client;
        }

        public async Task<ProductModel> GetProduct(int id)
        {
            var query = new GraphQLRequest
            {
                Query = @" 
                query productQuery($productId: ID!)
                { product(id: $productId) 
                    { id name price rating photoFileName description stock introducedAt 
                      reviews { title review }
                    }
                }",
                Variables = new { productId = id }
            };
            var response = await _client.SendQueryAsync(query);
            return response.GetDataFieldAs<ProductModel>("product");
        }

        public async Task AddReview(ProductReviewInputModel review)
        {
            var query = new GraphQLRequest
            {
                Query = @" 
                mutation($review: reviewInput!)
                {
                    createReview(review: $review)
                    {
                        id
                    }
                }",
                Variables = new { review }
            };
            var response = await _client.SendQueryAsync(query);
            if (response != null)
            {
                var reviewReturned = response.GetDataFieldAs<ProductReviewModel>("createReview");
            }
        }

        public async Task SubscribeToUpdates()
        {
            var result = await _client.SendSubscribeAsync("subscription { reviewAdded { title productId } }");
            result.OnReceive += Receive;
        }

        private void Receive(GraphQLResponse resp)
        {
            var review = resp.Data["reviewAdded"];
        }
    }
}
