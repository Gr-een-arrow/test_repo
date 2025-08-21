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
    }

    [Test]
    public async Task TC_001_ValidSingleOffer_ReturnsTotalPrice()
    {
        var requestBody = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[]
                    {
                        new { OfferItemRefID = "item1", PaxRefID = "pax1" },
                        new { OfferItemRefID = "item2", PaxRefID = "pax2" },
                        new { OfferItemRefID = "item3", PaxRefID = "pax3" }
                    }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("PricedOffer"));
        Assert.IsTrue(responseContent.Contains("total price"));
    }

    [Test]
    public async Task TC_002_MultipleValidOffers_ReturnsIndividualPrices()
    {
        var requestBody = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[] { new { OfferItemRefID = "item1", PaxRefID = "pax1" } }
                },
                new
                {
                    OfferRefID = "b2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7002",
                    OwnerCode = "BA",
                    SelectedOfferItems = new[] { new { OfferItemRefID = "item2", PaxRefID = "pax2" } }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("PricedOffer"));
    }

    [Test]
    public async Task TC_003_MissingOfferRefID_ReturnsError()
    {
        var requestBody = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OwnerCode = "VS",
                    SelectedOfferItems = new[] { new { OfferItemRefID = "item1", PaxRefID = "pax1" } }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("OfferRefID is required"));
    }

    [Test]
    public async Task TC_004_InvalidOfferRefIDFormat_ReturnsError()
    {
        var requestBody = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "invalid-format",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[] { new { OfferItemRefID = "item1", PaxRefID = "pax1" } }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("Invalid OfferRefID format"));
    }

    [Test]
    public async Task TC_005_MissingOwnerCode_ReturnsError()
    {
        var requestBody = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    SelectedOfferItems = new[] { new { OfferItemRefID = "item1", PaxRefID = "pax1" } }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("OwnerCode is required"));
    }

    [Test]
    public async Task TC_006_EmptySelectedOfferList_ReturnsError()
    {
        var requestBody = new
        {
            SelectedOfferList = new object[] { }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("At least one SelectedOffer is required"));
    }

    [Test]
    public async Task TC_007_InvalidOfferItemRefID_ReturnsError()
    {
        var requestBody = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[] { new { OfferItemRefID = "invalid-id", PaxRefID = "pax1" } }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("Offer item not found"));
    }

    [Test]
    public async Task TC_008_MissingPaxRefID_ReturnsError()
    {
        var requestBody = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[] { new { OfferItemRefID = "item1" } }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("PaxRefID is required for each SelectedOfferItem"));
    }

    [Test]
    public async Task TC_009_NonExistentOfferRefID_ReturnsError()
    {
        var requestBody = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "00000000-0000-0000-0000-000000000000|invalid",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[] { new { OfferItemRefID = "item1", PaxRefID = "pax1" } }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("Offer not found"));
    }

    [Test]
    public async Task TC_010_ValidOfferWithZeroItems_ReturnsZeroPrice()
    {
        var requestBody = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    OwnerCode = "VS",
                    SelectedOfferItems = new object[] { }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("0.00"));
    }

    [Test]
    public async Task TC_011_OfferRefIDWithSpecialCharacters_ReturnsError()
    {
        var requestBody = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|<script>",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[] { new { OfferItemRefID = "item1", PaxRefID = "pax1" } }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("Invalid characters in OfferRefID"));
    }

    [Test]
    public async Task TC_012_ValidOfferWithDuplicatePaxRefIDs_ReturnsCorrectTotal()
    {
        var requestBody = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[]
                    {
                        new { OfferItemRefID = "item1", PaxRefID = "pax1" },
                        new { OfferItemRefID = "item2", PaxRefID = "pax1" }
                    }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/OfferPriceRQ", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("PricedOffer"));
    }

    [TearDown]
    public void Teardown()
    {
        _client?.Dispose();
    }
}