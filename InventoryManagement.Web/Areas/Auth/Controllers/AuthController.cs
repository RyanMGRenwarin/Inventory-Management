using InventoryManagement.Application.DTOs.Auth;
using InventoryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Web.Areas.Auth.Controllers
{
    /// <summary>
    /// Authentication controller for login, registration, and logout.
    /// </summary>
    [Area("Auth")]
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the AuthController class.
        /// </summary>
        /// <param name="authService">Authentication service.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="httpContextAccessor">HTTP context accessor.</param>
        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Displays the login page.
        /// </summary>
        /// <returns>Login view.</returns>
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var loginModel = new LoginRequestDto();
            // Check for remember me cookie
            if (Request.Cookies.TryGetValue("RememberMe", out var rememberedUsername))
            {
                loginModel.Username = rememberedUsername;
                loginModel.RememberMe = true;
            }

            return View(loginModel);
        }

        /// <summary>
        /// Handles user login.
        /// </summary>
        /// <param name="request">Login request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to home on success, or returns to login on error.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(request);
                }

                var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                var response = await _authService.LoginAsync(request, ipAddress, cancellationToken);

                if (response == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
                    return View(request);
                }

                // Handle "Remember Me" cookie
                if (request.RememberMe)
                {
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddDays(30),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax
                    };
                    Response.Cookies.Append("RememberMe", request.Username, cookieOptions);
                }
                else
                {
                    Response.Cookies.Delete("RememberMe");
                }

                //store access token in http-only cookie (not in session)
                Response.Cookies.Append("AccessToken", response.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(15) // Sesuai JWT expiry
                });

                // Session is still being used for user data (except for access token)
                HttpContext.Session.SetString("UserFullName", response.FullName);
                HttpContext.Session.SetString("UserRole", response.Role);

                // Store refresh token in HTTP-only cookie
                var refreshTokenOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
                Response.Cookies.Append("RefreshToken", response.RefreshToken, refreshTokenOptions);

                _logger.LogInformation("User logged in successfully: {Username} from IP: {IpAddress}", request.Username, ipAddress);

                TempData["SuccessMessage"] = $"Welcome back, {response.FullName}!";

                return RedirectToAction("Index", "Home", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(request);
            }
        }

        /// <summary>
        /// Displays the registration page.
        /// </summary>
        /// <returns>Register view.</returns>
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            return View(new RegisterRequestDto());
        }

        /// <summary>
        /// Handles user registration.
        /// </summary>
        /// <param name="request">Registration request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to login on success, or returns to register on error.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(request);
                }

                var result = await _authService.RegisterAsync(request, cancellationToken);

                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "Username or email already exists.");
                    return View(request);
                }

                _logger.LogInformation("User registered successfully: {Username}", request.Username);

                TempData["SuccessMessage"] = "Registration successful! Please login.";

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Username}", request.Username);
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                return View(request);
            }
        }

        /// <summary>
        /// Displays the forgot password page.
        /// </summary>
        /// <returns>Forgot password view.</returns>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            return View(new ForgotPasswordRequestDto());
        }

        /// <summary>
        /// Handles forgot password request.
        /// </summary>
        /// <param name="request">Forgot password request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Forgot password view with success message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(request);
                }

                // This is a placeholder - actual password reset would send an email
                // For now, just show a success message
                _logger.LogInformation("Password reset requested for email: {Email}", request.Email);

                TempData["SuccessMessage"] = "If an account exists with this email, you will receive a password reset link.";

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for email: {Email}", request.Email);
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(request);
            }
        }

        /// <summary>
        /// Displays the logout confirmation page.
        /// </summary>
        /// <returns>Logout view.</returns>
        [HttpGet]
        public IActionResult Logout()
        {
            return View();
        }

        /// <summary>
        /// Handles user logout.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to login page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
        {
            try
            {
                var refreshToken = Request.Cookies["RefreshToken"];

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    await _authService.LogoutAsync(refreshToken, ipAddress, cancellationToken);
                }

                // Clear session
                HttpContext.Session.Clear();

                // Clear cookies
                Response.Cookies.Delete("AccessToken");
                Response.Cookies.Delete("RefreshToken");
                Response.Cookies.Delete("RememberMe");

                _logger.LogInformation("User logged out successfully");

                TempData["SuccessMessage"] = "You have been logged out successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }

            return RedirectToAction("Login", "Auth", new { area = "Auth" });
        }

        /// <summary>
        /// Handles token refresh via AJAX.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>JSON with new access token.</returns>
        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken = default)
        {
            try
            {
                var refreshToken = Request.Cookies["RefreshToken"];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Json(new { success = false, message = "Refresh token not found." });
                }

                var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                var response = await _authService.RefreshTokenAsync(refreshToken, ipAddress, cancellationToken);

                if (response == null)
                {
                    return Json(new { success = false, message = "Invalid or expired refresh token." });
                }

                // Update access token in session
                HttpContext.Session.SetString("AccessToken", response.AccessToken);

                // Update refresh token cookie (rotation)
                var refreshTokenOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
                Response.Cookies.Append("RefreshToken", response.RefreshToken, refreshTokenOptions);

                return Json(new
                {
                    success = true,
                    accessToken = response.AccessToken,
                    expiresIn = response.ExpiresIn
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return Json(new { success = false, message = "An error occurred while refreshing token." });
            }
        }

        /// <summary>
        /// Temporary endpoint to test BCrypt hash.
        /// </summary>
        /// <returns>The BCrypt hash for "Admin@123".</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult TestHash()
        {
            var password = "Admin@123";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            return Content($"Password: {password}\nHash: {hash}");
        }


        /// <summary>
        /// Temporary endpoint to test BCrypt hash.
        /// </summary>
        /// <returns>The BCrypt hash for "User@123".</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult TestHash2()
        {
            var password = "User@123";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            return Content($"Password: {password}\nHash: {hash}");
        }
    }
}
