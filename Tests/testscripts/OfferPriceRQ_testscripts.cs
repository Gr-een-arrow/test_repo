using System.Net.Http.Json;
using Xunit;
using System.Text.Json;
using System.Text;
using System.Net;

public class OfferPriceRQTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public OfferPriceRQTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task OfferPriceRQ_001_ValidSingleAdult_ReturnsCorrectTotalPrice()
    {
        var payload = new
        {
            OfferItemRefID = new[] { "OFFER1|ITEM1" },
            PaxRefID = new[] { "ADULT_1" }
        };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(content.GetProperty("totalPrice").GetDecimal() > 0);
    }

    [Fact]
    public async Task OfferPriceRQ_002_ValidMultiPassenger_ReturnsCorrectTotalPrice()
    {
        var payload = new
        {
            OfferItemRefID = new[] { "OFFER1|ITEM1", "OFFER1|ITEM2", "OFFER1|ITEM3" },
            PaxRefID = new[] { "ADULT_1", "YOUNG_1", "CHILD_1" }
        };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        var totalPrice = content.GetProperty("totalPrice").GetDecimal();
        var individualPrices = content.GetProperty("individualPrices").EnumerateArray().Sum(p => p.GetDecimal());
        Assert.Equal(individualPrices, totalPrice);
    }

    [Fact]
    public async Task OfferPriceRQ_003_MissingOfferItemRefID_ReturnsBadRequest()
    {
        var payload = new { PaxRefID = new[] { "ADULT_1" } };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("OfferItemRefID is required", content);
    }

    [Fact]
    public async Task OfferPriceRQ_004_InvalidOfferItemRefIDFormat_ReturnsUnprocessableEntity()
    {
        var payload = new
        {
            OfferItemRefID = new[] { "invalid-format" },
            PaxRefID = new[] { "ADULT_1" }
        };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", payload);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid OfferItemRefID format", content);
    }

    [Fact]
    public async Task OfferPriceRQ_005_MissingPaxRefID_ReturnsBadRequest()
    {
        var payload = new { OfferItemRefID = new[] { "OFFER1|ITEM1" } };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("PaxRefID is required", content);
    }

    [Fact]
    public async Task OfferPriceRQ_006_NonExistentOfferItemRefID_ReturnsNotFound()
    {
        var payload = new
        {
            OfferItemRefID = new[] { "00000000-0000-0000-0000-000000000000|invalid" },
            PaxRefID = new[] { "ADULT_1" }
        };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", payload);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Offer item not found", content);
    }

    [Fact]
    public async Task OfferPriceRQ_007_ValidSpecialCharacters_ReturnsPrice()
    {
        var payload = new
        {
            OfferItemRefID = new[] { "OFFER-1|ITEM_1" },
            PaxRefID = new[] { "ADULT_1" }
        };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(content.GetProperty("totalPrice").GetDecimal() > 0);
    }

    [Fact]
    public async Task OfferPriceRQ_008_EmptyOfferItemRefID_ReturnsBadRequest()
    {
        var payload = new
        {
            OfferItemRefID = new[] { "" },
            PaxRefID = new[] { "ADULT_1" }
        };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("OfferItemRefID cannot be empty", content);
    }

    [Fact]
    public async Task OfferPriceRQ_009_MaxPassengers_ReturnsCorrectTotal()
    {
        var offerItems = Enumerable.Range(1, 9).Select(i => $"OFFER1|ITEM{i}").ToArray();
        var paxIds = Enumerable.Range(1, 9).Select(i => $"ADULT_{i}").ToArray();
        var payload = new
        {
            OfferItemRefID = offerItems,
            PaxRefID = paxIds
        };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        var totalPrice = content.GetProperty("totalPrice").GetDecimal();
        var individualPrices = content.GetProperty("individualPrices").EnumerateArray().Sum(p => p.GetDecimal());
        Assert.Equal(individualPrices, totalPrice);
    }

    [Fact]
    public async Task OfferPriceRQ_010_DuplicateOfferItemRefID_ReturnsUnprocessableEntity()
    {
        var payload = new
        {
            OfferItemRefID = new[] { "OFFER1|ITEM1", "OFFER1|ITEM1" },
            PaxRefID = new[] { "ADULT_1", "ADULT_2" }
        };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", payload);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Duplicate OfferItemRefID not allowed", content);
    }
}