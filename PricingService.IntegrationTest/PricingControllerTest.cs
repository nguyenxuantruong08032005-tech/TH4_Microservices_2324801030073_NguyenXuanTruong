using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PricingService.Api.Commands;
using PricingService.Api.Commands.Dto;
using Xunit;
using static Xunit.Assert;

namespace PricingService.IntegrationTest;

[Collection("PricingControllerFixtureCollection")]
public class PricingControllerTest
{
    private readonly PricingControllerFixture fixture;

    public PricingControllerTest(PricingControllerFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task PriceForTravelPolicyIsCorrect()
    {
        var response = await PostAsJson(new CalculatePriceCommand
        {
            ProductCode = "TRI",
            PolicyFrom = DateTimeOffset.Now.AddDays(5),
            PolicyTo = DateTimeOffset.Now.AddDays(10),
            SelectedCovers = new List<string> { "C1", "C2", "C3" },
            Answers = new List<QuestionAnswer>
            {
                new NumericQuestionAnswer { QuestionCode = "NUM_OF_ADULTS", Answer = 1M },
                new NumericQuestionAnswer { QuestionCode = "NUM_OF_CHILDREN", Answer = 1M },
                new TextQuestionAnswer { QuestionCode = "DESTINATION", Answer = "EUR" }
            }
        });

        Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var calculationResult = JsonConvert.DeserializeObject<CalculatePriceResult>(responseBody);
        NotNull(calculationResult);
        Equal(98M, calculationResult.TotalPrice);
    }

    [Fact]
    public async Task CommandIsProperlyValidated()
    {
        var response = await PostAsJson(new CalculatePriceCommand());

        Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private Task<HttpResponseMessage> PostAsJson(CalculatePriceCommand command)
    {
        var json = JsonConvert.SerializeObject(command);
        return fixture.Client.PostAsync(
            "/api/Pricing",
            new StringContent(json, Encoding.UTF8, "application/json"));
    }
}
