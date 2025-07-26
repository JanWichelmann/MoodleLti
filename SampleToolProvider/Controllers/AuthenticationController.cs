using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoodleLti;
using MoodleLti.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SampleToolProvider.Controllers
{
    /// <summary>
    /// Handles authentication via LTI.
    /// </summary>
    [Authorize]
    [Route("auth/")]
    public class AuthenticationController : Controller
    {
        private readonly MoodleLtiOptions _ltiOptions;

        public AuthenticationController(IOptions<MoodleLtiOptions> ltiOptions)
        {
            _ltiOptions = ltiOptions.Value;
        }

        [AllowAnonymous]
        [HttpGet]
        [HttpPost]
        public IActionResult RenderError()
        {
            return Unauthorized("Login via Moodle or /auth/login");
        }

        /// <summary>
        /// Handles a login request via GET.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("login")]
        public async Task<IActionResult> LoginAsync()
        {
            try
            {
                // Create identity
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, "test"));
                identity.AddClaim(new Claim(ClaimTypes.Email, "test@test"));

                // TODO role

                // Sign in
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                // TODO
                return Ok("login successful");
            }
            catch
            {
                // TODO
                throw;
            }
        }

        /// <summary>
        /// Handles a login request via LTI tool launch.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("moodle/login")]
        public async Task<IActionResult> MoodleLoginAsync()
        {
            try
            {
                // Parse and check request
                var authData = await MoodleAuthenticationTools.ParseAuthenticationRequestAsync(Request, _ltiOptions.OAuthConsumerKey, _ltiOptions.OAuthSharedSecret);
                
                // Create identity
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, authData.LoginName));
                identity.AddClaim(new Claim(ClaimTypes.Email, authData.Email));

                // TODO role

                // Sign in
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                // TODO
                return Json(authData);
            }
            catch
            {
                // TODO
                throw;
            }
        }

        [HttpGet("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            // Sign out
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok("logout successful");
        }
    }
}
