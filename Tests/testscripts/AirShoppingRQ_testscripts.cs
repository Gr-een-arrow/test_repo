using NUnit.Framework;
using System;
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
        _client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
    }

    [Test]
    public async Task TC_001_ValidRoundTripSearch_ShouldReturnFlightOffers()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "LHR", Date = "2025-05-07" },
                    DestArrivalCriteria = new { IATA_LocationCode = "JNB" }
                },
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "JNB", Date = "2025-05-14" },
                    DestArrivalCriteria = new { IATA_LocationCode = "LHR" }
                }
            },
            CabinTypeCode = "5",
            PrefLevelCode = "Required",
            PaxList = new[]
            {
                new { PTC = "ADT" },
                new { PTC = "GBE" },
                new { PTC = "CHD" }
            }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic result = JsonConvert.DeserializeObject(responseBody);
        Assert.IsNotNull(result.FlightOffers);
        Assert.Greater(result.FlightOffers.Count, 0);
        Assert.IsNotNull(result.FlightOffers[0].Price);
        Assert.IsNotNull(result.FlightOffers[0].Airline);
        Assert.IsNotNull(result.FlightOffers[0].FlightTimes);
    }

    [Test]
    public async Task TC_002_ValidOneWaySearch_ShouldReturnFlightOffers()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "LAX", Date = "2025-06-01" },
                    DestArrivalCriteria = new { IATA_LocationCode = "NYC" }
                }
            }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic result = JsonConvert.DeserializeObject(responseBody);
        Assert.IsNotNull(result.FlightOffers);
        Assert.Greater(result.FlightOffers.Count, 0);
    }

    [Test]
    public async Task TC_003_EconomyCabinSearch_ShouldReturnFilteredOffers()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "LHR", Date = "2025-05-07" },
                    DestArrivalCriteria = new { IATA_LocationCode = "JNB" }
                }
            },
            CabinTypeCode = "1",
            PrefLevelCode = "Required"
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic result = JsonConvert.DeserializeObject(responseBody);
        Assert.IsNotNull(result.FlightOffers);
        Assert.Greater(result.FlightOffers.Count, 0);
    }

    [Test]
    public async Task TC_004_MissingOriginIATA_ShouldReturn400()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { Date = "2025-05-07" },
                    DestArrivalCriteria = new { IATA_LocationCode = "JNB" }
                }
            }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("origin", responseBody.ToLower());
    }

    [Test]
    public async Task TC_005_InvalidIATACodeFormat_ShouldReturn400()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "LOND", Date = "2025-05-07" },
                    DestArrivalCriteria = new { IATA_LocationCode = "JNB" }
                }
            }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("iata", responseBody.ToLower());
    }

    [Test]
    public async Task TC_006_PastTravelDate_ShouldReturn400()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "LHR", Date = "2023-01-01" },
                    DestArrivalCriteria = new { IATA_LocationCode = "JNB" }
                }
            }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("past", responseBody.ToLower());
    }

    [Test]
    public async Task TC_007_MissingDestinationIATA_ShouldReturn400()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "LHR", Date = "2025-05-07" }
                }
            }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("destination", responseBody.ToLower());
    }

    [Test]
    public async Task TC_008_EmptyPaxList_ShouldReturn400()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "LHR", Date = "2025-05-07" },
                    DestArrivalCriteria = new { IATA_LocationCode = "JNB" }
                }
            },
            PaxList = new object[] { }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("paxlist", responseBody.ToLower());
    }

    [Test]
    public async Task TC_009_InvalidCabinTypeCode_ShouldReturn400()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "LHR", Date = "2025-05-07" },
                    DestArrivalCriteria = new { IATA_LocationCode = "JNB" }
                }
            },
            CabinTypeCode = "FIRST",
            PrefLevelCode = "Required"
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("cabintypecode", responseBody.ToLower());
    }

    [Test]
    public async Task TC_010_UnsupportedIATACodes_ShouldReturn404()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "XXX", Date = "2025-05-07" },
                    DestArrivalCriteria = new { IATA_LocationCode = "YYY" }
                }
            }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("route", responseBody.ToLower());
    }

    [Test]
    public async Task TC_011_ReturnDateBeforeDeparture_ShouldReturn400()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "LHR", Date = "2025-05-14" },
                    DestArrivalCriteria = new { IATA_LocationCode = "JNB" }
                },
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "JNB", Date = "2025-05-07" },
                    DestArrivalCriteria = new { IATA_LocationCode = "LHR" }
                }
            }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("sequence", responseBody.ToLower());
    }

    [Test]
    public async Task TC_012_MissingPrefLevelCode_ShouldReturn400()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "LHR", Date = "2025-05-07" },
                    DestArrivalCriteria = new { IATA_LocationCode = "JNB" }
                }
            ],
            CabinTypeCode = "5"
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("preflevelcode", responseBody.ToLower());
    }

    [Test]
    public async Task TC_013_MultiplePassengers_ShouldReturnFlightOffers()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "LHR", Date = "2025-05-07" },
                    DestArrivalCriteria = new { IATA_LocationCode = "JNB" }
                }
            },
            PaxList = new[]
            {
                new { PTC = "ADT" },
                new { PTC = "ADT" },
                new { PTC = "ADT" },
                new { PTC = "CHD" },
                new { PTC = "CHD" },
                new { PTC = "INF" }
            }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        dynamic result = JsonConvert.DeserializeObject(responseBody);
        Assert.IsNotNull(result.FlightOffers);
        Assert.Greater(result.FlightOffers.Count, 0);
    }

    [Test]
    public async Task TC_014_MalformedDateFormat_ShouldReturn400()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "LHR", Date = "07-05-2025" },
                    DestArrivalCriteria = new { IATA_LocationCode = "JNB" }
                }
            }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("format", responseBody.ToLower());
    }

    [Test]
    public async Task TC_015_SameOriginAndDestination_ShouldReturn400()
    {
        var request = new
        {
            OriginDestCriteria = new[]
            {
                new
                {
                    OriginDepCriteria = new { IATA_LocationCode = "LHR", Date = "2025-05-07" },
                    DestArrivalCriteria = new { IATA_LocationCode = "LHR" }
                }
            }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("route", responseBody.ToLower());
    }

    [TearDown]
    public void Teardown()
    {
        _client?.Dispose();
    }
}