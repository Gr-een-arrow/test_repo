using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

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
    public async Task TC_001_ValidPassengerListWithAllSupportedPassengerTypes_ShouldReturn200()
    {
        var payload = new
        {
            PaxList = new[]
            {
                new { PaxID = "ADT1", PTC = "ADT" },
                new { PaxID = "CHD1", PTC = "CHD" },
                new { PaxID = "GBE1", PTC = "GBE" },
                new { PaxID = "INF1", PTC = "INF" },
                new { PaxID = "SNR1", PTC = "SNR" }
            }
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Test]
    public async Task TC_002_MaximumOneInfantPerAdultValidation_ShouldReturn200()
    {
        var payload = new
        {
            PaxList = new[]
            {
                new { PaxID = "ADT1", PTC = "ADT" },
                new { PaxID = "INF1", PTC = "INF" }
            }
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Test]
    public async Task TC_003_MultipleInfantsWithDifferentAdults_ShouldReturn200()
    {
        var payload = new
        {
            PaxList = new[]
            {
                new { PaxID = "ADT1", PTC = "ADT" },
                new { PaxID = "INF1", PTC = "INF" },
                new { PaxID = "ADT2", PTC = "ADT" },
                new { PaxID = "INF2", PTC = "INF" }
            }
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Test]
    public async Task TC_004_ChildAndYouthRequireAdult_ShouldReturn400()
    {
        var payload = new
        {
            PaxList = new[]
            {
                new { PaxID = "CHD1", PTC = "CHD" },
                new { PaxID = "GBE1", PTC = "GBE" }
            }
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    public async Task TC_005_InvalidPTCCodeRejection_ShouldReturn400()
    {
        var payload = new
        {
            PaxList = new[]
            {
                new { PaxID = "PAX1", PTC = "XYZ" }
            }
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    public async Task TC_006_DuplicatePaxIDValidation_ShouldReturn400()
    {
        var payload = new
        {
            PaxList = new[]
            {
                new { PaxID = "PAX1", PTC = "ADT" },
                new { PaxID = "PAX1", PTC = "CHD" }
            }
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    public async Task TC_007_NoAdultInPassengerList_ShouldReturn400()
    {
        var payload = new
        {
            PaxList = new[]
            {
                new { PaxID = "CHD1", PTC = "CHD" },
                new { PaxID = "INF1", PTC = "INF" }
            }
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    public async Task TC_008_MultipleInfantsAssignedToSingleAdult_ShouldReturn400()
    {
        var payload = new
        {
            PaxList = new[]
            {
                new { PaxID = "ADT1", PTC = "ADT" },
                new { PaxID = "INF1", PTC = "INF" },
                new { PaxID = "INF2", PTC = "INF" }
            }
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    public async Task TC_009_ValidSeniorPassenger_ShouldReturn200()
    {
        var payload = new
        {
            PaxList = new[]
            {
                new { PaxID = "ADT1", PTC = "ADT" },
                new { PaxID = "SNR1", PTC = "SNR" }
            }
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Test]
    public async Task TC_010_MinimumRequiredAdultPassenger_ShouldReturn200()
    {
        var payload = new
        {
            PaxList = new[]
            {
                new { PaxID = "ADT1", PTC = "ADT" }
            }
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("AirShoppingRQ", content);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TearDown]
    public void Teardown()
    {
        _client?.Dispose();
    }
}