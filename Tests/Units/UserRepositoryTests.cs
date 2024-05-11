using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Caching.Distributed;
using busfy_api.src.Infrastructure.Data;
using busfy_api.src.Domain.Models;
using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Infrastructure.Repository;

namespace busfy_api.Tests.Infrastructure.Repository
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private IUserRepository _userRepository;
        private Mock<AppDbContext> _mockContext;
        private Mock<IDistributedCache> _mockDistributedCache;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<AppDbContext>();
            _mockDistributedCache = new Mock<IDistributedCache>();
            _userRepository = new UserRepository(_mockContext.Object, _mockDistributedCache.Object);
        }

        [Test]
        public async Task AddAsync_UserAlreadyExists_ReturnsNull()
        {
            var email = "test@example.com";
            var signUpBody = new SignUpBody { Email = email };
            _mockContext.Setup(c => c.Users.FindAsync(email)).ReturnsAsync(new UserModel());

            var result = await _userRepository.AddAsync(signUpBody, "User");
            Assert.That(result == null);
        }

        [Test]
        public async Task UpdateProfileAsync_UserNotFound_ReturnsNull()
        {
            var updateProfileBody = new UpdateProfileBody { Email = "nonexistent@example.com" };
            Guid userId = Guid.NewGuid();
            _mockContext.Setup(c => c.Users.FindAsync(userId)).ReturnsAsync((UserModel)null);

            var result = await _userRepository.UpdateProfileAsync(updateProfileBody, userId);
            Assert.That(result == null);
        }

        [Test]
        public async Task VerifyRecoveryCode_InvalidCode_ReturnsFalse()
        {
            var email = "test@example.com";
            var recoveryCode = "wrongCode";
            _mockContext.Setup(c => c.Users.FindAsync(email)).ReturnsAsync(new UserModel
            {
                RestoreCode = "correctCode",
                RestoreCodeValidBefore = DateTime.UtcNow.AddMinutes(5),
                WasPasswordResetRequest = true
            });


            var result = await _userRepository.VerifyRecoveryCode(email, recoveryCode);
            Assert.That(!result);
        }
    }
}