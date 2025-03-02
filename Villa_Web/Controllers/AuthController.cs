using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using Villa_Utility;
using Villa_Web.Models;
using Villa_Web.Models.Dto;
using Villa_Web.Services.IServices;

namespace Villa_Web.Controllers
{
	public class AuthController : Controller
	{
		private IAuthService _authService;
		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}
		public IActionResult Login()
		{
			LoginRequestDTO obj = new();
			return View(obj);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginRequestDTO obj)
		{
			APIResponse response = await _authService.LoginAsync<APIResponse>(obj);
			if (response != null && response.IsSuccessfull)
			{
				LoginResponseDTO model = JsonConvert.DeserializeObject<LoginResponseDTO>(Convert.ToString(response.Result));

				var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
				identity.AddClaim(new Claim(ClaimTypes.Name, model.User.UserName));
				identity.AddClaim(new Claim(ClaimTypes.Role, model.User.Role));
				var principal = new ClaimsPrincipal(identity);
				await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
				HttpContext.Session.SetString(SD.SessionToken, model.Token);
				return RedirectToAction("Index", "Home");
			}
			else
			{
				ModelState.AddModelError("CustomError", response.ErrorMessages.FirstOrDefault());
				return View(obj);
			}

		}
		public IActionResult Register()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterationRequestDTO obj)
		{
			APIResponse response = await _authService.RegisterAsync<APIResponse>(obj);
			if (response != null && response.IsSuccessfull)
			{
				return RedirectToAction("Login");
			}
			return View();
		}
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync();
			HttpContext.Session.SetString(SD.SessionToken, "");
			return RedirectToAction("Index", "Home");
		}
		public IActionResult AccessDenied()
		{
			return View();
		}
	}
}
