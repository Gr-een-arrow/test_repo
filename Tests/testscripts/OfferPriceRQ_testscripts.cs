using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

public class OfferPriceRQTests
{
    private readonly HttpClient _client = new HttpClient();
    private const string Endpoint = "https://api.example.com/v1/OfferPriceRQ";

    [Fact]
    public async Task OfferPriceRQ_001_ValidSingleOffer_ReturnsTotalPrice()
    {
        var payload = @"{
            "SelectedOfferList": {
                "SelectedOffer": [{
                    "OfferRefID": "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    "OwnerCode": "VS",
                    "SelectedOfferItem": [
                        {"OfferItemRefID": "item1", "PaxRefID": "pax1"},
                        {"OfferItemRefID": "item2", "PaxRefID": "pax2"},
                        {"OfferItemRefID": "item3", "PaxRefID": "pax3"}
                    ]
                }]
            }
        }";

        var response = await _client.PostAsync(Endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("PricedOffer", content);
        Assert.Contains("TotalPrice", content);
    }

    [Fact]
    public async Task OfferPriceRQ_002_ValidMultipleOffers_ReturnsTotalPrices()
    {
        var payload = @"{
            "SelectedOfferList": {
                "SelectedOffer": [
                    {
                        "OfferRefID": "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                        "OwnerCode": "VS",
                        "SelectedOfferItem": [{"OfferItemRefID": "item1", "PaxRefID": "pax1"}]
                    },
                    {
                        "OfferRefID": "b3827160-ff4c-598d-9b74-e83b9d8920b8|bn0g0gwjuU0TW1Xe8OSJ8802",
                        "OwnerCode": "BA",
                        "SelectedOfferItem": [{"OfferItemRefID": "item2", "PaxRefID": "pax2"}]
                    }
                ]
            }
        }";

        var response = await _client.PostAsync(Endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("PricedOffer", content);
    }

    [Fact]
    public async Task OfferPriceRQ_003_MissingOfferRefID_ReturnsError()
    {
        var payload = @"{
            "SelectedOfferList": {
                "SelectedOffer": [{
                    "OwnerCode": "VS",
                    "SelectedOfferItem": [{"OfferItemRefID": "item1", "PaxRefID": "pax1"}]
                }]
            }
        }";

        var response = await _client.PostAsync(Endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("OfferRefID is required", content);
    }

    [Fact]
    public async Task OfferPriceRQ_004_InvalidOfferRefIDFormat_ReturnsError()
    {
        var payload = @"{
            "SelectedOfferList": {
                "SelectedOffer": [{
                    "OfferRefID": "invalid-format-without-pipe",
                    "OwnerCode": "VS",
                    "SelectedOfferItem": [{"OfferItemRefID": "item1", "PaxRefID": "pax1"}]
                }]
            }
        }";

        var response = await _client.PostAsync(Endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("invalid OfferRefID format", content);
    }

    [Fact]
    public async Task OfferPriceRQ_005_MissingOwnerCode_ReturnsError()
    {
        var payload = @"{
            "SelectedOfferList": {
                "SelectedOffer": [{
                    "OfferRefID": "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    "SelectedOfferItem": [{"OfferItemRefID": "item1", "PaxRefID": "pax1"}]
                }]
            }
        }";

        var response = await _client.PostAsync(Endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("OwnerCode is required", content);
    }

    [Fact]
    public async Task OfferPriceRQ_006_EmptySelectedOfferList_ReturnsError()
    {
        var payload = @"{
            "SelectedOfferList": {
                "SelectedOffer": []
            }
        }";

        var response = await _client.PostAsync(Endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("at least one offer must be selected", content);
    }

    [Fact]
    public async Task OfferPriceRQ_007_NonExistentOfferRefID_ReturnsError()
    {
        var payload = @"{
            "SelectedOfferList": {
                "SelectedOffer": [{
                    "OfferRefID": "00000000-0000-0000-0000-000000000000|xxxxxxxxxxxxxxxxxxxx",
                    "OwnerCode": "VS",
                    "SelectedOfferItem": [{"OfferItemRefID": "item1", "PaxRefID": "pax1"}]
                }]
            }
        }";

        var response = await _client.PostAsync(Endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("offer not found", content);
    }

    [Fact]
    public async Task OfferPriceRQ_008_MissingSelectedOfferItem_ReturnsError()
    {
        var payload = @"{
            "SelectedOfferList": {
                "SelectedOffer": [{
                    "OfferRefID": "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    "OwnerCode": "VS"
                }]
            }
        }";

        var response = await _client.PostAsync(Endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("at least one SelectedOfferItem is required", content);
    }

    [Fact]
    public async Task OfferPriceRQ_009_InvalidPaxRefID_ReturnsError()
    {
        var payload = @"{
            "SelectedOfferList": {
                "SelectedOffer": [{
                    "OfferRefID": "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    "OwnerCode": "VS",
                    "SelectedOfferItem": [{"OfferItemRefID": "item1", "PaxRefID": "INVALID_PAX_TYPE"}]
                }]
            }
        }";

        var response = await _client.PostAsync(Endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("invalid passenger reference", content);
    }

    [Fact]
    public async Task OfferPriceRQ_010_ExpiredOffer_ReturnsError()
    {
        var payload = @"{
            "SelectedOfferList": {
                "SelectedOffer": [{
                    "OfferRefID": "expired-offer-ref|expired-offer-id",
                    "OwnerCode": "VS",
                    "SelectedOfferItem": [{"OfferItemRefID": "item1", "PaxRefID": "pax1"}]
                }]
            }
        }";

        var response = await _client.PostAsync(Endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("offer has expired", content);
    }

    [Fact]
    public async Task OfferPriceRQ_011_SpecialCharactersInOfferRefID_ReturnsTotalPrice()
    {
        var payload = @"{
            "SelectedOfferList": {
                "SelectedOffer": [{
                    "OfferRefID": "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001-TEST@123",
                    "OwnerCode": "VS",
                    "SelectedOfferItem": [{"OfferItemRefID": "item1", "PaxRefID": "pax1"}]
                }]
            }
        }";

        var response = await _client.PostAsync(Endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("PricedOffer", content);
    }

    [Fact]
    public async Task OfferPriceRQ_012_LowercaseOwnerCode_ReturnsError()
    {
        var payload = @"{
            "SelectedOfferList": {
                "SelectedOffer": [{
                    "OfferRefID": "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    "OwnerCode": "vs",
                    "SelectedOfferItem": [{"OfferItemRefID": "item1", "PaxRefID": "pax1"}]
                }]
            }
        }";

        var response = await _client.PostAsync(Endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("invalid OwnerCode format", content);
    }
}