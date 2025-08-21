[Test]
public async Task TC_001_ValidSingleOffer_ReturnsTotalPrice()
{
    var payload = new
    {
        SelectedOfferList = new[]
        {
            new
            {
                OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                OwnerCode = "VS",
                SelectedOfferItem = new[]
                {
                    new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001|item-1", PaxRefID = "ADULT_1" },
                    new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001|item-2", PaxRefID = "ADULT_2" },
                    new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001|item-3", PaxRefID = "CHILD_1" }
                }
            }
        }
    };

    var json = JsonConvert.SerializeObject(payload);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _client.PostAsync("OfferPriceRQ", content);

    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.IsTrue(responseContent.Contains("PricedOffer"));
    Assert.IsTrue(responseContent.Contains("TotalPrice"));
}

[Test]
public async Task TC_002_ValidMultipleOffers_ReturnsTotalPrices()
{
    var payload = new
    {
        SelectedOfferList = new[]
        {
            new
            {
                OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                OwnerCode = "VS",
                SelectedOfferItem = new[]
                {
                    new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001|item-1", PaxRefID = "ADULT_1" }
                }
            },
            new
            {
                OfferRefID = "b3827160-fe4c-598d-9b74-e83b9d892148|bn0g0jwmuU0TW1Xe0zOSJ8002",
                OwnerCode = "BA",
                SelectedOfferItem = new[]
                {
                    new { OfferItemRefID = "b3827160-fe4c-598d-9b74-e83b9d892148|bn0g0jwmuU0TW1Xe0zOSJ8002|item-1", PaxRefID = "ADULT_1" }
                }
            }
        }
    };

    var json = JsonConvert.SerializeObject(payload);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _client.PostAsync("OfferPriceRQ", content);

    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.IsTrue(responseContent.Contains("PricedOffer"));
    Assert.AreEqual(2, responseContent.Split(new[] { "PricedOffer" }, System.StringSplitOptions.None).Length - 1);
}

[Test]
public async Task TC_003_MissingOfferRefID_ReturnsBadRequest()
{
    var payload = new
    {
        SelectedOfferList = new[]
        {
            new
            {
                OwnerCode = "VS",
                SelectedOfferItem = new[]
                {
                    new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001|item-1", PaxRefID = "ADULT_1" }
                }
            }
        }
    };

    var json = JsonConvert.SerializeObject(payload);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _client.PostAsync("OfferPriceRQ", content);

    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.IsTrue(responseContent.Contains("OfferRefID"));
}

[Test]
public async Task TC_004_InvalidOfferRefIDFormat_ReturnsBadRequest()
{
    var payload = new
    {
        SelectedOfferList = new[]
        {
            new
            {
                OfferRefID = "invalid-format",
                OwnerCode = "VS",
                SelectedOfferItem = new[]
                {
                    new { OfferItemRefID = "invalid-format|item-1", PaxRefID = "ADULT_1" }
                }
            }
        }
    };

    var json = JsonConvert.SerializeObject(payload);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _client.PostAsync("OfferPriceRQ", content);

    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.IsTrue(responseContent.Contains("format"));
}

[Test]
public async Task TC_005_NonExistentOfferRefID_ReturnsNotFound()
{
    var payload = new
    {
        SelectedOfferList = new[]
        {
            new
            {
                OfferRefID = "00000000-0000-0000-0000-000000000000|invalid",
                OwnerCode = "VS",
                SelectedOfferItem = new[]
                {
                    new { OfferItemRefID = "00000000-0000-0000-0000-000000000000|invalid|item-1", PaxRefID = "ADULT_1" }
                }
            }
        }
    };

    var json = JsonConvert.SerializeObject(payload);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _client.PostAsync("OfferPriceRQ", content);

    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.IsTrue(responseContent.Contains("not found"));
}

[Test]
public async Task TC_006_MissingOwnerCode_ReturnsBadRequest()
{
    var payload = new
    {
        SelectedOfferList = new[]
        {
            new
            {
                OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                SelectedOfferItem = new[]
                {
                    new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001|item-1", PaxRefID = "ADULT_1" }
                }
            }
        }
    };

    var json = JsonConvert.SerializeObject(payload);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _client.PostAsync("OfferPriceRQ", content);

    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.IsTrue(responseContent.Contains("OwnerCode"));
}

[Test]
public async Task TC_007_EmptySelectedOfferItem_ReturnsBadRequest()
{
    var payload = new
    {
        SelectedOfferList = new[]
        {
            new
            {
                OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                OwnerCode = "VS",
                SelectedOfferItem = new object[] { }
            }
        }
    };

    var json = JsonConvert.SerializeObject(payload);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _client.PostAsync("OfferPriceRQ", content);

    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.IsTrue(responseContent.Contains("offer items"));
}

[Test]
public async Task TC_008_MismatchedOfferItemRefID_ReturnsBadRequest()
{
    var payload = new
    {
        SelectedOfferList = new[]
        {
            new
            {
                OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                OwnerCode = "VS",
                SelectedOfferItem = new[]
                {
                    new { OfferItemRefID = "different-offer-id|item-1", PaxRefID = "ADULT_1" }
                }
            }
        }
    };

    var json = JsonConvert.SerializeObject(payload);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _client.PostAsync("OfferPriceRQ", content);

    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.IsTrue(responseContent.Contains("mismatch"));
}

[Test]
public async Task TC_009_ValidOfferWithSpecialPaxRefID_ReturnsTotalPrice()
{
    var payload = new
    {
        SelectedOfferList = new[]
        {
            new
            {
                OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                OwnerCode = "VS",
                SelectedOfferItem = new[]
                {
                    new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001|item-1", PaxRefID = "ADULT_1_SPECIAL" }
                }
            }
        }
    };

    var json = JsonConvert.SerializeObject(payload);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _client.PostAsync("OfferPriceRQ", content);

    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.IsTrue(responseContent.Contains("PricedOffer"));
    Assert.IsTrue(responseContent.Contains("TotalPrice"));
}

[Test]
public async Task TC_010_EmptySelectedOfferList_ReturnsBadRequest()
{
    var payload = new
    {
        SelectedOfferList = new object[] { }
    };

    var json = JsonConvert.SerializeObject(payload);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _client.PostAsync("OfferPriceRQ", content);

    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.IsTrue(responseContent.Contains("empty"));
}