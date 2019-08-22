using System;
using Xunit;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using Consumer;
using System.Collections.Generic;
using System.Net;

namespace tests
{
    public class ConsumerPactTests : IClassFixture<ConsumerPactClassFixture>
    {
        private IMockProviderService _mockProviderService;
        private string _mockProviderServiceBaseUri;
        public ConsumerPactTests(ConsumerPactClassFixture fixture)
        {
            _mockProviderService = fixture.MockProviderService;
            _mockProviderService.ClearInteractions(); //NOTE: Clears any previously registered interactions before the test is run
            _mockProviderServiceBaseUri = fixture.MockProviderServiceBaseUri;
        }

        [Fact]
        public void ItHandlesInvalidDateParam()
        {
            // Arange
            var invalidRequestMessage = "validDateTime is not a date or time";
            _mockProviderService.Given("There is data")
                                .UponReceiving("A invalid GET request for Date Validation with invalid date parameter")
                                .With(new ProviderServiceRequest 
                                {
                                    Method = HttpVerb.Get,
                                    Path = "/api/provider",
                                    Query = "validDateTime=lolz"
                                })
                                .WillRespondWith(new ProviderServiceResponse {
                                    Status = 400,
                                    Headers = new Dictionary<string, object>
                                    {
                                        { "Content-Type", "application/json; charset=utf-8" }
                                    },
                                    Body = new 
                                    {
                                        message = invalidRequestMessage
                                    }
                                });
                                
            // Act
            var result = ConsumerApiClient.ValidateDateTimeUsingProviderApi("lolz", _mockProviderServiceBaseUri).GetAwaiter().GetResult();
            var resultBodyText = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Assert
            Assert.Contains(invalidRequestMessage, resultBodyText);
        }

        [Fact]
        public void ItHandlesAnEmptyDateParameter()
        {
            var invalidRequestMessage = "validDateTime is required";
            _mockProviderService.Given("There is data")
                .UponReceiving("An invalid GET request for Date with no date parameter")
                .With(new ProviderServiceRequest{
                    Method = HttpVerb.Get,
                    Path= "/api/provider",
                    Query = "validDateTime="
                })
                .WillRespondWith(new ProviderServiceResponse {
                    Status = 400,
                    Headers = new Dictionary<string, object>
                    {
                        { "Content-Type", "application/json; charset=utf-8" }
                    },
                    Body = new 
                    {
                        message = invalidRequestMessage
                    }
                });

            // Act
            var result = ConsumerApiClient.ValidateDateTimeUsingProviderApi("", _mockProviderServiceBaseUri).GetAwaiter().GetResult();
            var resultBodyText = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Assert
            Assert.Contains(invalidRequestMessage, resultBodyText);
        }

        [Fact]
        public void ItHandlesHavingNoDataInTheDataFolder()
        {
            _mockProviderService.Given("There is no data")
                .UponReceiving("A valid GET request without data")
                .With(new ProviderServiceRequest{
                    Method = HttpVerb.Get,
                    Path= "/api/provider",
                    Query = "validDateTime=05/01/2018"
                })
                .WillRespondWith(new ProviderServiceResponse {
                    Status = 404
                });

            // Act
            var result = ConsumerApiClient.ValidateDateTimeUsingProviderApi("05/01/2018", _mockProviderServiceBaseUri).GetAwaiter().GetResult();
            var resultBodyText = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Assert
            Assert.Equal(false, result.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.True(string.IsNullOrEmpty(resultBodyText));
        }

        [Fact]
        public void ItParsesADateCorrectly()
        {
            // Arange
            var validDateTime = "01-05-2018 00:00:00";
            var test = "NO";
            _mockProviderService.Given("There is data")
                                .UponReceiving("A valid GET request returns a date")
                                .With(new ProviderServiceRequest 
                                {
                                    Method = HttpVerb.Get,
                                    Path = "/api/provider",
                                    Query = "validDateTime=05/01/2018"
                                })
                                .WillRespondWith(new ProviderServiceResponse {
                                    Status = 200,
                                    Headers = new Dictionary<string, object>
                                    {
                                        { "Content-Type", "application/json; charset=utf-8" }
                                    },
                                    Body = new 
                                    {
                                        test = test,
                                        validDateTime = validDateTime
                                    }
                                });
                                
            // Act
            var result = ConsumerApiClient.ValidateDateTimeUsingProviderApi("05/01/2018", _mockProviderServiceBaseUri).GetAwaiter().GetResult();
            var resultBodyText = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Assert
            Assert.Contains(validDateTime, resultBodyText);
        }
    }
}
