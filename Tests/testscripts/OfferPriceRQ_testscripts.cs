using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

[TestFixture]
public class OfferPriceTests
{
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _client = new HttpClient();
        _client.BaseAddress = new System.Uri("https://api.example.com/");
    }

    [Test]
    public async Task TC_001_ValidSinglePassengerOneWayFlightOfferPriceRequest()
    {
        var payload = new
        {
            offerId = "OFF123456789",
            passengers = new[]
            {
                new { type = "ADULT", count = 1 }
            }
        };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic result = JsonConvert.DeserializeObject(responseBody);
        Assert.IsNotNull(result.totalPrice);
        Assert.IsNotNull(result.totalPrice.currency);
        Assert.IsNotNull(result.totalPrice.amount);
    }

    [Test]
    public async Task TC_002_ValidMultiPassengerRoundTripFlightOfferPriceRequest()
    {
        var payload = new
        {
            offerId = "OFF987654321",
            passengers = new[]
            {
                new { type = "ADULT", count = 2 },
                new { type = "CHILD", count = 1 }
            }
        };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic result = JsonConvert.DeserializeObject(responseBody);
        Assert.IsNotNull(result.totalPrice);
    }

    [Test]
    public async Task TC_003_MissingRequiredOfferIdInRequest()
    {
        var payload = new
        {
            passengers = new[]
            {
                new { type = "ADULT", count = 1 }
            }
        };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("offer ID", responseBody.ToLower());
    }

    [Test]
    public async Task TC_004_InvalidOfferIdFormat()
    {
        var payload = new
        {
            offerId = "",
            passengers = new[]
            {
                new { type = "ADULT", count = 1 }
            }
        };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("validation", responseBody.ToLower());
    }

    [Test]
    public async Task TC_005_NonExistentOfferId()
    {
        var payload = new
        {
            offerId = "00000000-0000-0000-0000-000000000000",
            passengers = new[]
            {
                new { type = "ADULT", count = 1 }
            }
        };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("not found", responseBody.ToLower());
    }

    [Test]
    public async Task TC_006_ValidRequestWithSpecialServiceCodes()
    {
        var payload = new
        {
            offerId = "OFF123456789",
            passengers = new[]
            {
                new { type = "ADULT", count = 1 }
            },
            services = new[]
            {
                new { code = "BAG", quantity = 1 },
                new { code = "MEAL", preference = "VGML" }
            }
        };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic result = JsonConvert.DeserializeObject(responseBody);
        Assert.IsNotNull(result.totalPrice);
    }

    [Test]
    public async Task TC_007_ExpiredOfferId()
    {
        var payload = new
        {
            offerId = "EXP123456789",
            passengers = new[]
            {
                new { type = "ADULT", count = 1 }
            }
        };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.Gone, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("expired", responseBody.ToLower());
    }

    [Test]
    public async Task TC_008_ValidRequestWithInfantPassenger()
    {
        var payload = new
        {
            offerId = "OFF123456789",
            passengers = new[]
            {
                new { type = "ADULT", count = 1 },
                new { type = "INFANT", count = 1 }
            }
        };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic result = JsonConvert.DeserializeObject(responseBody);
        Assert.IsNotNull(result.totalPrice);
    }

    [Test]
    public async Task TC_009_MalformedJsonPayload()
    {
        var malformedJson = "{\"offerId\":\"OFF123456789\",\"passengers\":[{\"type\":\"ADULT\",\"count\":1}";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("parse", responseBody.ToLower());
    }

    [Test]
    public async Task TC_010_ValidRequestWithLoyaltyProgramNumber()
    {
        var payload = new
        {
            offerId = "OFF123456789",
            passengers = new[]
            {
                new { type = "ADULT", count = 1, loyaltyNumber = "FF123456789" }
            }
        };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic result = JsonConvert.DeserializeObject(responseBody);
        Assert.IsNotNull(result.totalPrice);
    }

    [TearDown]
    public void Cleanup()
    {
        _client?.Dispose();
    }
}