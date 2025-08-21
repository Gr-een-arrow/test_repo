[Test]
public async Task TC_001_ValidSingleAdultPassenger_ReturnsCorrectTotalPrice()
{
    var payload = new
    {
        offerId = "OFFER123456",
        passengers = new[]
        {
            new { type = "ADULT", count = 1 }
        }
    };

    var response = await _client.PostAsync($"{BaseUrl}/OfferPriceRQ", 
        new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
    
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    var content = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
    Assert.IsTrue(content.totalPrice > 0);
}

[Test]
public async Task TC_002_ValidMultiplePassengerTypes_ReturnsCorrectTotalPrice()
{
    var payload = new
    {
        offerId = "OFFER123456",
        passengers = new[]
        {
            new { type = "ADULT", count = 2 },
            new { type = "CHILD", count = 1 }
        }
    };

    var response = await _client.PostAsync($"{BaseUrl}/OfferPriceRQ", 
        new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
    
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    var content = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
    Assert.IsTrue(content.totalPrice > 0);
    Assert.IsNotNull(content.passengerBreakdown);
}

[Test]
public async Task TC_003_MissingOfferId_ReturnsError()
{
    var payload = new
    {
        passengers = new[]
        {
            new { type = "ADULT", count = 1 }
        }
    };

    var response = await _client.PostAsync($"{BaseUrl}/OfferPriceRQ", 
        new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
    
    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    var content = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
    Assert.AreEqual("offerId is required", content.errorMessage.ToString());
}

[Test]
public async Task TC_004_InvalidOfferIdFormat_ReturnsError()
{
    var payload = new
    {
        offerId = "",
        passengers = new[]
        {
            new { type = "ADULT", count = 1 }
        }
    };

    var response = await _client.PostAsync($"{BaseUrl}/OfferPriceRQ", 
        new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
    
    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    var content = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
    Assert.AreEqual("Invalid offerId format", content.errorMessage.ToString());
}

[Test]
public async Task TC_005_NonExistentOfferId_ReturnsError()
{
    var payload = new
    {
        offerId = "INVALID123",
        passengers = new[]
        {
            new { type = "ADULT", count = 1 }
        }
    };

    var response = await _client.PostAsync($"{BaseUrl}/OfferPriceRQ", 
        new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
    
    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    var content = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
    Assert.AreEqual("Offer not found", content.errorMessage.ToString());
}

[Test]
public async Task TC_006_ExpiredOffer_ReturnsError()
{
    var payload = new
    {
        offerId = "EXPIRED123",
        passengers = new[]
        {
            new { type = "ADULT", count = 1 }
        }
    };

    var response = await _client.PostAsync($"{BaseUrl}/OfferPriceRQ", 
        new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
    
    Assert.AreEqual(HttpStatusCode.Gone, response.StatusCode);
    var content = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
    Assert.AreEqual("Offer expired", content.errorMessage.ToString());
}

[Test]
public async Task TC_007_MissingPassengerDetails_ReturnsError()
{
    var payload = new
    {
        offerId = "OFFER123456"
    };

    var response = await _client.PostAsync($"{BaseUrl}/OfferPriceRQ", 
        new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
    
    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    var content = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
    Assert.AreEqual("Passenger details required", content.errorMessage.ToString());
}

[Test]
public async Task TC_008_ZeroPassengers_ReturnsError()
{
    var payload = new
    {
        offerId = "OFFER123456",
        passengers = new object[] { }
    };

    var response = await _client.PostAsync($"{BaseUrl}/OfferPriceRQ", 
        new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
    
    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    var content = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
    Assert.AreEqual("At least one passenger required", content.errorMessage.ToString());
}

[Test]
public async Task TC_009_SpecialCharactersInOfferId_ReturnsPrice()
{
    var payload = new
    {
        offerId = "OFFER-123-456",
        passengers = new[]
        {
            new { type = "ADULT", count = 1 }
        }
    };

    var response = await _client.PostAsync($"{BaseUrl}/OfferPriceRQ", 
        new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
    
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    var content = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
    Assert.IsTrue(content.totalPrice > 0);
}

[Test]
public async Task TC_010_MalformedJson_ReturnsError()
{
    var malformedJson = "{invalid json}";

    var response = await _client.PostAsync($"{BaseUrl}/OfferPriceRQ", 
        new StringContent(malformedJson, Encoding.UTF8, "application/json"));
    
    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    var content = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
    Assert.AreEqual("Invalid JSON format", content.errorMessage.ToString());
}

[Test]
public async Task TC_011_MaximumPassengers_ReturnsCorrectTotalPrice()
{
    var payload = new
    {
        offerId = "OFFER123456",
        passengers = new[]
        {
            new { type = "ADULT", count = 9 }
        }
    };

    var response = await _client.PostAsync($"{BaseUrl}/OfferPriceRQ", 
        new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
    
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    var content = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
    Assert.IsTrue(content.totalPrice > 0);
}

[Test]
public async Task TC_012_UnsupportedHttpMethod_ReturnsError()
{
    var response = await _client.GetAsync($"{BaseUrl}/OfferPriceRQ");
    
    Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    Assert.IsTrue(response.Headers.Contains("Allow"));
}