using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using busfy_api.src.Domain.Entities.Request;
using System.Text;
using Microsoft.AspNetCore.TestHost;
using Moq;
using webApiTemplate.src.App.IService;
using webApiTemplate.src.Domain.Entities.Shared;
using System.Net;

namespace busfy_api.Tests.Integration
{
    [TestFixture]
    public class PostControllerTests
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Mock<IJwtService> _mockJwtService;

        [SetUp]
        public void Setup()
        {
            _mockJwtService = new Mock<IJwtService>();
            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddScoped(_ => _mockJwtService.Object);
                    });
                });
            _client = _factory.CreateClient();
        }

        [Test]
        public async Task CreatePost_ReturnsOk_WhenPostIsValid()
        {
            var request = new CreatePostBody
            {
                Description = "Test Post",
                Text = "This is a test post content."
            };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            _mockJwtService.Setup(s => s.GetTokenPayload(It.IsAny<string>())).Returns(new TokenInfo { UserId = Guid.NewGuid() });

            var response = await _client.PostAsync("/api/post", content);
            Assert.That(response.IsSuccessStatusCode);
        }

        [Test]
        public async Task CreatePost_ReturnsUnauthorized_WhenTokenIsInvalid()
        {
            var request = new CreatePostBody
            {
                Description = "Valid Description",
                Text = "Valid Text"
            };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            _mockJwtService.Setup(s => s.GetTokenPayload(It.IsAny<string>())).Returns((TokenInfo)null);

            var response = await _client.PostAsync("/api/post", content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}