using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository.IRepository;

namespace Villa_API.Controllers
{
    [Route("api/UserAuth")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepo;
        protected APIResponse _response;
        public UserController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
            this._response = new();
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var loginResponse = await _userRepo.Login(model);
            if (loginResponse.User == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccessfull = false;
                _response.ErrorMessages.Add("UserName or password is incorrect ");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccessfull = true;
            _response.Result = loginResponse;
            return Ok(_response);
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterationRequestDTO model)
        {
            bool isUserNameUnique = _userRepo.IsUniqueUser(model.UserName);
            if (!isUserNameUnique) 
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccessfull = false;
                _response.ErrorMessages.Add("UserName already exists");
                return BadRequest(_response);
            }
            var user = await _userRepo.Register(model);
            if (user == null) 
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccessfull = false;
                _response.ErrorMessages.Add("Error while registering");
                return BadRequest(_response);
            }
            _response.StatusCode= HttpStatusCode.OK;
            _response.IsSuccessfull = true;
            return Ok(user);
        }
    }
}
