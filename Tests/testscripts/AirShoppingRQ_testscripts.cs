using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

[TestFixture]
public class AirShoppingTests
{
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
    }

    [Test]
    public async Task TC_001_ValidRoundTripSearch_ReturnsFlightOffers()
    {
        var requestBody = @"{
            "OriginDestCriteria": [
                {
                    "OriginDepCriteria": { "IATA_LocationCode": "LHR", "Date": "2025-05-07" },
                    "DestArrivalCriteria": { "IATA_LocationCode": "JNB" }
                },
                {
                    "OriginDepCriteria": { "IATA_LocationCode": "JNB", "Date": "2025-05-14" },
                    "DestArrivalCriteria": { "IATA_LocationCode": "LHR" }
                }
            ],
            "CabinTypeCode": "5",
            "PrefLevelCode": "Required",
            "PaxList": [
                { "PassengerTypeCode": "ADT" },
                { "PassengerTypeCode": "GBE" },
                { "PassengerTypeCode": "CHD" }
            ]
        }";

        var response = await _client.PostAsync("AirShoppingRQ", new StringContent(requestBody, Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("flightOffers"));
    }

    [Test]
    public async Task TC_002_ValidOneWaySearch_ReturnsFlightOffers()
    {
        var requestBody = @"{
            "OriginDestCriteria": [
                {
                    "OriginDepCriteria": { "IATA_LocationCode": "LAX", "Date": "2025-06-10" },
                    "DestArrivalCriteria": { "IATA_LocationCode": "NYC" }
                }
            ],
            "CabinTypeCode": "5",
            "PrefLevelCode": "Required",
            "PaxList": [{ "PassengerTypeCode": "ADT" }]
        }";

        var response = await _client.PostAsync("AirShoppingRQ", new StringContent(requestBody, Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("flightOffers"));
    }

    [Test]
    public async Task TC_003_MissingOriginIataCode_ReturnsBadRequest()
    {
        var requestBody = @"{
            "OriginDestCriteria": [
                {
                    "OriginDepCriteria": { "Date": "2025-05-07" },
                    "DestArrivalCriteria": { "IATA_LocationCode": "JNB" }
                }
            ],
            "CabinTypeCode": "5",
            "PrefLevelCode": "Required"
        }";

        var response = await _client.PostAsync("AirShoppingRQ", new StringContent(requestBody, Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("origin"));
    }

    [Test]
    public async Task TC_004_MissingDestinationIataCode_ReturnsBadRequest()
    {
        var requestBody = @"{
            "OriginDestCriteria": [
                {
                    "OriginDepCriteria": { "IATA_LocationCode": "LHR", "Date": "2025-05-07" },
                    "DestArrivalCriteria": {}
                }
            ],
            "CabinTypeCode": "5",
            "PrefLevelCode": "Required"
        }";

        var response = await _client.PostAsync("AirShoppingRQ", new StringContent(requestBody, Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("destination"));
    }

    [Test]
    public async Task TC_005_InvalidDateFormat_ReturnsBadRequest()
    {
        var requestBody = @"{
            "OriginDestCriteria": [
                {
                    "OriginDepCriteria": { "IATA_LocationCode": "LHR", "Date": "07-05-2025" },
                    "DestArrivalCriteria": { "IATA_LocationCode": "JNB" }
                }
            ],
            "CabinTypeCode": "5",
            "PrefLevelCode": "Required"
        }";

        var response = await _client.PostAsync("AirShoppingRQ", new StringContent(requestBody, Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("date format"));
    }

    [Test]
    public async Task TC_006_PastDate_ReturnsBadRequest()
    {
        var requestBody = @"{
            "OriginDestCriteria": [
                {
                    "OriginDepCriteria": { "IATA_LocationCode": "LHR", "Date": "2020-01-01" },
                    "DestArrivalCriteria": { "IATA_LocationCode": "JNB" }
                }
            ],
            "CabinTypeCode": "5",
            "PrefLevelCode": "Required"
        }";

        var response = await _client.PostAsync("AirShoppingRQ", new StringContent(requestBody, Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("past"));
    }

    [Test]
    public async Task TC_007_InvalidIataCodeLength_ReturnsBadRequest()
    {
        var requestBody = @"{
            "OriginDestCriteria": [
                {
                    "OriginDepCriteria": { "IATA_LocationCode": "LHRR", "Date": "2025-05-07" },
                    "DestArrivalCriteria": { "IATA_LocationCode": "JNB" }
                }
            ],
            "CabinTypeCode": "5",
            "PrefLevelCode": "Required"
        }";

        var response = await _client.PostAsync("AirShoppingRQ", new StringContent(requestBody, Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("IATA"));
    }

    [Test]
    public async Task TC_008_EmptyPassengerList_ReturnsBadRequest()
    {
        var requestBody = @"{
            "OriginDestCriteria": [
                {
                    "OriginDepCriteria": { "IATA_LocationCode": "LHR", "Date": "2025-05-07" },
                    "DestArrivalCriteria": { "IATA_LocationCode": "JNB" }
                }
            ],
            "CabinTypeCode": "5",
            "PrefLevelCode": "Required"
        }";

        var response = await _client.PostAsync("AirShoppingRQ", new StringContent(requestBody, Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("passenger"));
    }

    [Test]
    public async Task TC_009_UnsupportedCabinTypeCode_ReturnsBadRequest()
    {
        var requestBody = @"{
            "OriginDestCriteria": [
                {
                    "OriginDepCriteria": { "IATA_LocationCode": "LHR", "Date": "2025-05-07" },
                    "DestArrivalCriteria": { "IATA_LocationCode": "JNB" }
                }
            ],
            "CabinTypeCode": "99",
            "PrefLevelCode": "Required"
        }";

        var response = await _client.PostAsync("AirShoppingRQ", new StringContent(requestBody, Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("cabin type"));
    }

    [Test]
    public async Task TC_010_NoFlightsAvailable_ReturnsEmptyList()
    {
        var requestBody = @"{
            "OriginDestCriteria": [
                {
                    "OriginDepCriteria": { "IATA_LocationCode": "XXX", "Date": "2025-05-07" },
                    "DestArrivalCriteria": { "IATA_LocationCode": "YYY" }
                }
            ],
            "CabinTypeCode": "5",
            "PrefLevelCode": "Required",
            "PaxList": [{ "PassengerTypeCode": "ADT" }]
        }";

        var response = await _client.PostAsync("AirShoppingRQ", new StringContent(requestBody, Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("flightOffers": []"));
    }

    [Test]
    public async Task TC_011_MissingCabinTypeCode_ReturnsBadRequest()
    {
        var requestBody = @"{
            "OriginDestCriteria": [
                {
                    "OriginDepCriteria": { "IATA_LocationCode": "LHR", "Date": "2025-05-07" },
                    "DestArrivalCriteria": { "IATA_LocationCode": "JNB" }
                }
            ],
            "PrefLevelCode": "Required"
        }";

        var response = await _client.PostAsync("AirShoppingRQ", new StringContent(requestBody, Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("cabin type"));
    }

    [Test]
    public async Task TC_012_ReturnDateBeforeDeparture_ReturnsBadRequest()
    {
        var requestBody = @"{
            "OriginDestCriteria": [
                {
                    "OriginDepCriteria": { "IATA_LocationCode": "LHR", "Date": "2025-05-14" },
                    "DestArrivalCriteria": { "IATA_LocationCode": "JNB" }
                },
                {
                    "OriginDepCriteria": { "IATA_LocationCode": "JNB", "Date": "2025-05-07" },
                    "DestArrivalCriteria": { "IATA_LocationCode": "LHR" }
                }
            ],
            "CabinTypeCode": "5",
            "PrefLevelCode": "Required"
        }";

        var response = await _client.PostAsync("AirShoppingRQ", new StringContent(requestBody, Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("date sequence"));
    }

    [TearDown]
    public void Teardown()
    {
        _client?.Dispose();
    }
}