using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

[TestFixture]
public class AirShoppingTests
{
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _client = new HttpClient();
        _client.BaseAddress = new System.Uri("https://api.example.com/");
    }

    [Test]
    public async Task TC_001_ValidSearchWithAllRequiredParameters_ReturnsFlightOffers()
    {
        var payload = new
        {
            origin = "LAX",
            destination = "NYC",
            departureDate = "2024-12-01",
            returnDate = "2024-12-08"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic responseObj = JsonConvert.DeserializeObject(responseBody);
        Assert.Greater(responseObj.offers.Count, 0);
    }

    [Test]
    public async Task TC_002_OneWayTripSearch_ReturnsOutboundFlightsOnly()
    {
        var payload = new
        {
            origin = "LHR",
            destination = "CDG",
            departureDate = "2024-11-15"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic responseObj = JsonConvert.DeserializeObject(responseBody);
        Assert.Greater(responseObj.offers.Count, 0);
    }

    [Test]
    public async Task TC_003_SearchWithCityCodes_ReturnsFlightsFromMultipleAirports()
    {
        var payload = new
        {
            origin = "NYC",
            destination = "LON",
            departureDate = "2024-12-01"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic responseObj = JsonConvert.DeserializeObject(responseBody);
        Assert.Greater(responseObj.offers.Count, 0);
    }

    [Test]
    public async Task TC_004_MissingOrigin_ReturnsError()
    {
        var payload = new
        {
            destination = "JFK",
            departureDate = "2024-12-01"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("origin is required", responseBody);
    }

    [Test]
    public async Task TC_005_MissingDestination_ReturnsError()
    {
        var payload = new
        {
            origin = "LAX",
            departureDate = "2024-12-01"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("destination is required", responseBody);
    }

    [Test]
    public async Task TC_006_MissingDepartureDate_ReturnsError()
    {
        var payload = new
        {
            origin = "LAX",
            destination = "NYC"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("departureDate is required", responseBody);
    }

    [Test]
    public async Task TC_007_InvalidDateFormat_ReturnsError()
    {
        var payload = new
        {
            origin = "LAX",
            destination = "NYC",
            departureDate = "01-12-2024"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("invalid date format, use YYYY-MM-DD", responseBody);
    }

    [Test]
    public async Task TC_008_PastDepartureDate_ReturnsEmptyFlights()
    {
        var payload = new
        {
            origin = "LAX",
            destination = "NYC",
            departureDate = "2020-01-01"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic responseObj = JsonConvert.DeserializeObject(responseBody);
        Assert.AreEqual(responseObj.offers.Count, 0);
    }

    [Test]
    public async Task TC_009_InvalidIataCodeFormat_ReturnsError()
    {
        var payload = new
        {
            origin = "LOSANGELES",
            destination = "NEWYORK",
            departureDate = "2024-12-01"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("invalid IATA code format", responseBody);
    }

    [Test]
    public async Task TC_010_ReturnDateBeforeDeparture_ReturnsError()
    {
        var payload = new
        {
            origin = "LAX",
            destination = "NYC",
            departureDate = "2024-12-08",
            returnDate = "2024-12-01"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("returnDate must be after departureDate", responseBody);
    }

    [Test]
    public async Task TC_011_SpecialCharactersInCityNames_ReturnsError()
    {
        var payload = new
        {
            origin = "L@X",
            destination = "NYC",
            departureDate = "2024-12-01"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("invalid characters in origin", responseBody);
    }

    [Test]
    public async Task TC_012_ResponseIncludesAllRequiredFields()
    {
        var payload = new
        {
            origin = "SFO",
            destination = "BOS",
            departureDate = "2024-12-15"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic responseObj = JsonConvert.DeserializeObject(responseBody);
        Assert.Greater(responseObj.offers.Count, 0);
        foreach (var offer in responseObj.offers)
        {
            Assert.IsNotNull(offer.price);
            Assert.IsNotNull(offer.airline);
            Assert.IsNotNull(offer.departureTime);
            Assert.IsNotNull(offer.arrivalTime);
        }
    }

    [Test]
    public async Task TC_013_NumericIataCodes_ReturnsError()
    {
        var payload = new
        {
            origin = "123",
            destination = "456",
            departureDate = "2024-12-01"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("invalid IATA code format", responseBody);
    }

    [Test]
    public async Task TC_014_CaseInsensitiveIataCodes_ReturnsFlightOffers()
    {
        var payload = new
        {
            origin = "lax",
            destination = "nyc",
            departureDate = "2024-12-01"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic responseObj = JsonConvert.DeserializeObject(responseBody);
        Assert.Greater(responseObj.offers.Count, 0);
    }

    [Test]
    public async Task TC_015_SameOriginAndDestination_ReturnsError()
    {
        var payload = new
        {
            origin = "LAX",
            destination = "LAX",
            departureDate = "2024-12-01"
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("origin and destination cannot be the same", responseBody);
    }

    [TearDown]
    public void Teardown()
    {
        _client?.Dispose();
    }
}