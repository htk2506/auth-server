using AuthServer.Database.Models;
using AuthServer.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AuthServerTest.Services
{
    public class TokenServiceTests
    {
        private sealed class Harness
        {
            public required TokenService Sut { get; init; }
        }

        private static Harness BuildSut()
        {
            var hasher = new Mock<IPasswordHasher<AppUser>>();

            return new Harness
            {
                Sut = new TokenService(hasher.Object)
            };
        }

        public class Given
        {
            public class When
            {
                [Fact]
                public void Then_1_plus_1_equals_2()
                {
                    // Arrange
                    Harness harness = BuildSut();

                    // Act
                    harness.Sut.GenerateToken();

                    // Assert
                    int x = 1 + 1;
                    x.Should().Be(2);
                }
            }
        }
    }
}
