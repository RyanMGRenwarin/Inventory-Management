using FluentAssertions;
using InventoryManagement.Application.DTOs.Auth;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Web.Areas.Auth.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace InventoryManagement.Web.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly AuthController _sut;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _loggerMock = new Mock<ILogger<AuthController>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            httpContext.Session = new TestSession();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _sut = new AuthController(
                _authServiceMock.Object,
                _loggerMock.Object,
                _httpContextAccessorMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                },

                TempData = new TempDataDictionary(
                            httpContext,
                            Mock.Of<ITempDataProvider>())
            };
        }

        // ============================================================
        // Login (GET)
        // ============================================================

        [Fact]
        public void Login_Get_Should_Return_ViewResult()
        {
            // Act
            var result = _sut.Login();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Login_Get_Should_Redirect_When_Authenticated()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "admin") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = _sut.Login();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
        }

        // ============================================================
        // Login (POST)
        // ============================================================

        [Fact]
        public async Task Login_Post_Should_Redirect_When_Success()
        {
            // Arrange
            var request = new LoginRequestDto { Username = "admin", Password = "Admin@123" };

            _authServiceMock
                .Setup(x => x.LoginAsync(request, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LoginResponseDto
                {
                    AccessToken = "access-token",
                    RefreshToken = "refresh-token",
                    FullName = "Admin",
                    Role = "Admin",
                    ExpiresIn = 900
                });

            // Act
            var result = await _sut.Login(request);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Login_Post_Should_Return_View_When_Invalid_Credentials()
        {
            // Arrange
            var request = new LoginRequestDto { Username = "admin", Password = "wrong" };

            _authServiceMock
                .Setup(x => x.LoginAsync(request, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((LoginResponseDto?)null);

            // Act
            var result = await _sut.Login(request);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Login_Post_Should_Return_View_When_ModelState_Invalid()
        {
            // Arrange
            var request = new LoginRequestDto();
            _sut.ModelState.AddModelError("Username", "Required");

            // Act
            var result = await _sut.Login(request);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        // ============================================================
        // Register (GET)
        // ============================================================

        [Fact]
        public void Register_Get_Should_Return_ViewResult()
        {
            // Act
            var result = _sut.Register();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        // ============================================================
        // Register (POST)
        // ============================================================

        [Fact]
        public async Task Register_Post_Should_Redirect_When_Success()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "newuser",
                Email = "new@test.com",
                FullName = "New User",
                Password = "Password@123",
                ConfirmPassword = "Password@123"
            };

            _authServiceMock
                .Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.Register(request);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Login");
        }

        [Fact]
        public async Task Register_Post_Should_Return_View_When_Username_Exists()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "existing",
                Email = "new@test.com",
                FullName = "New User",
                Password = "Password@123",
                ConfirmPassword = "Password@123"
            };

            _authServiceMock
                .Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.Register(request);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        // ============================================================
        // ForgotPassword (GET)
        // ============================================================

        [Fact]
        public void ForgotPassword_Get_Should_Return_ViewResult()
        {
            // Act
            var result = _sut.ForgotPassword();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        // ============================================================
        // ForgotPassword (POST)
        // ============================================================

        [Fact]
        public async Task ForgotPassword_Post_Should_Redirect_To_Login()
        {
            // Arrange
            var request = new ForgotPasswordRequestDto { Email = "test@test.com" };
            _sut.ModelState.Clear();

            // Act
            var result = await _sut.ForgotPassword(request);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Login");
        }

        // ============================================================
        // Logout (GET)
        // ============================================================

        [Fact]
        public void Logout_Get_Should_Return_ViewResult()
        {
            // Act
            var result = _sut.Logout();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        // ============================================================
        // Logout (POST)
        // ============================================================

        [Fact]
        public async Task Logout_Post_Should_Redirect_To_Login()
        {
            // Act
            var result = await _sut.Logout(default);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Login");
        }

        // ============================================================
        // RefreshToken (POST - AJAX)
        // ============================================================

        [Fact]
        public async Task RefreshToken_Should_Return_Json_When_Success()
        {
            // Arrange
            _sut.ControllerContext.HttpContext.Request.Headers["Cookie"] = "RefreshToken=valid-token";

            _authServiceMock
                .Setup(x => x.RefreshTokenAsync("valid-token", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RefreshTokenResponseDto
                {
                    AccessToken = "new-access",
                    RefreshToken = "new-refresh",
                    ExpiresIn = 900
                });

            // Act
            var result = await _sut.RefreshToken();

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().NotBeNull();
        }
    }

    /// <summary>
    /// Test session implementation for unit tests.
    /// </summary>
    internal class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);

    }
}
