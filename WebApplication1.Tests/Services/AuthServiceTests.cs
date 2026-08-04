using Moq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.AuthenticationRepository;
using WebApplication1.Services.AuthenticationServices;
using WebApplication1.Utils;
using WebApplication1.Utils.DTOs.UserDTO;
using WebApplication1.Utils.Exceptions;

namespace WebApplication1.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IAuthenticationRepository> _repositoryMock;
    private Mock<IConfiguration> _configurationMock;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private AuthService _service;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IAuthenticationRepository>();
        _configurationMock = new Mock<IConfiguration>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

        _service = new AuthService(
            _repositoryMock.Object,
            _configurationMock.Object,
            _httpContextAccessorMock.Object);
    }

    private static User BuildUserWithPassword(string password, bool isActive)
    {
        var user = new User
        {
            UserId = 42,
            Username = "vozac1",
            TenantId = 1,
            IsActive = isActive,
            Roles = new Roles { Name = "Vozac" }
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        return user;
    }

    // ---------- RefreshTokenHasher (čista funkcija) ----------

    [Test]
    public void RefreshTokenHasher_IsDeterministic_AndDoesNotReturnRawToken()
    {
        const string raw = "some-256-bit-random-token";

        var hash1 = RefreshTokenHasher.Hash(raw);
        var hash2 = RefreshTokenHasher.Hash(raw);

        Assert.Multiple(() =>
        {
            Assert.That(hash1, Is.EqualTo(hash2), "Heš mora biti determinističan zbog lookup-a po hešu.");
            Assert.That(hash1, Is.Not.EqualTo(raw), "Sirov token se nikad ne sme čuvati kao takav.");
            Assert.That(RefreshTokenHasher.Hash("drugi-token"), Is.Not.EqualTo(hash1));
        });
    }

    // ---------- IsActive gate na loginu ----------

    [Test]
    public void LoginAsync_WhenUserIsInactive_ThrowsUserInactive()
    {
        var user = BuildUserWithPassword("tajna123", isActive: false);

        _repositoryMock.Setup(r => r.GetTenantBySlugAsync("firma"))
            .ReturnsAsync(new Tenant { TenantId = 1, Slug = "firma", IsActive = true });
        _repositoryMock.Setup(r => r.GetByUsernameAndTenantAsync("vozac1", 1))
            .ReturnsAsync(user);

        var request = new LoginUserDto { TenantSlug = "firma", Username = "vozac1", Password = "tajna123" };

        var ex = Assert.ThrowsAsync<ValidationException>(() => _service.LoginAsync(request));
        Assert.That(ex!.SubKey, Is.EqualTo("UserInactive"));
    }

    // ---------- Refresh koristi HEŠ pri lookup-u ----------

    [Test]
    public async Task RefreshTokensAsync_LooksUpByHashedToken_NotRawToken_AndReturnsNullWhenInvalid()
    {
        const string rawRefreshToken = "raw-refresh-token-from-cookie";
        var expectedHash = RefreshTokenHasher.Hash(rawRefreshToken);

        // Nijedan korisnik ne odgovara -> null (kontroler to mapira na 401).
        _repositoryMock.Setup(r => r.GetUserByRefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var request = new RefreshTokenRequestDto { RefreshToken = rawRefreshToken };

        var result = await _service.RefreshTokensAsync(request);

        Assert.That(result, Is.Null, "Nevažeći refresh token mora vratiti null (-> 401), ne baciti 404.");

        // Repo mora biti pozvan sa HEŠOM, ne sa sirovim tokenom.
        _repositoryMock.Verify(r => r.GetUserByRefreshTokenAsync(expectedHash), Times.Once);
        _repositoryMock.Verify(r => r.GetUserByRefreshTokenAsync(rawRefreshToken), Times.Never);
    }

    // ---------- Grace period: rotacija zadrži prethodni token ----------

    [Test]
    public async Task RefreshTokensAsync_OnRotation_MovesCurrentTokenToPreviousWithGrace()
    {
        // Pravi IConfiguration jer uspešan refresh generiše novi JWT (CreateToken).
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:Token"] = "unit-test-signing-key-mora-biti-najmanje-64-bajta-duga-da-radi-HS512-xxxx",
                ["AppSettings:Issuer"] = "TestIssuer",
                ["AppSettings:Audience"] = "TestAudience",
            })
            .Build();

        var service = new AuthService(_repositoryMock.Object, configuration, _httpContextAccessorMock.Object);

        var currentHash = RefreshTokenHasher.Hash("stari-token");
        var user = new User
        {
            UserId = 7,
            Username = "vozac1",
            TenantId = 1,
            IsActive = true,
            Roles = new Roles { Name = "Vozac" },
            RefreshToken = currentHash,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1),
        };

        User? saved = null;
        _repositoryMock.Setup(r => r.GetUserByRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync(user);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Callback<User>(u => saved = u).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await service.RefreshTokensAsync(new RefreshTokenRequestDto { RefreshToken = "stari-token" });

        Assert.That(result, Is.Not.Null);
        Assert.That(saved, Is.Not.Null);
        Assert.Multiple(() =>
        {
            // Stari (do sada tekući) token je prešao u "previous" sa postavljenim grace deadline-om.
            Assert.That(saved!.PreviousRefreshToken, Is.EqualTo(currentHash));
            Assert.That(saved.PreviousRefreshTokenExpiryTime, Is.Not.Null);
            Assert.That(saved.PreviousRefreshTokenExpiryTime, Is.GreaterThan(DateTime.UtcNow));
            // Tekući token je rotiran u nešto novo (ne isti heš).
            Assert.That(saved.RefreshToken, Is.Not.EqualTo(currentHash));
            Assert.That(saved.RefreshToken, Is.Not.Null);
        });
    }
}
