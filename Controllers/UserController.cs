using JwtAuth.Helpers;
using JwtAuth.Models;
using JwtAuth.Requests;
using JwtAuth.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _DbContext;
        public UserController(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        [HttpGet]
        [Route("Home")]
        public  IActionResult Home()
        {
            return Ok("Hello, welcome to an authorized endpoint!");
        }

            [HttpPost]
        [Route("NewUser")]
        [AllowAnonymous]
        public async Task<IActionResult> NewUser([FromBody] NewUserRequest _Request)
        {
            var _Response = new Response();
            //check if email exists
            var _UserEmail = await _DbContext.UserProfiles.SingleOrDefaultAsync(user => user.Email == _Request.Email);
            if (_UserEmail != null)
            {
                _Response.Status = VarHelper.ResponseStatus.ERROR.ToString();
                _Response.Message = "This email has been registered already.";
                return Ok(_Response);
            }

            //create user record
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(AppHelper.GetUnique8ByteKey());
            string sSalt = Convert.ToBase64String(plainTextBytes);
            string sHashedPassword = HashingHelper.HashUsingPbkdf2(_Request.Password, sSalt);

            UserProfile _UserProfile = new UserProfile
            {
                UserId = AppHelper.GetNewGuid(),
                Email = _Request.Email,
                Firstname = _Request.Firstname,
                Lastname = _Request.Lastname,
                PasswordSalt = sSalt,
                Password = sHashedPassword,
            };

            await _DbContext.UserProfiles.AddAsync(_UserProfile);
      
            await _DbContext.SaveChangesAsync(true);
           
            _Response.Status = VarHelper.ResponseStatus.SUCCESS.ToString();
            _Response.Message = "Your registeration was successful.";
            return Ok(_Response);
        }


        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest _Request)
        {
            Response _Response = new Response();
           

            //validate user credentials
            var _UserProfile = await _DbContext.UserProfiles.SingleOrDefaultAsync(user => user.Email == _Request.Email);
            if (_UserProfile == null)
            {
                _Response.Status = VarHelper.ResponseStatus.ERROR.ToString();
                _Response.Message = "Invalid Email";
                return Ok(_Response);
            }
            var passwordHash = HashingHelper.HashUsingPbkdf2(_Request.Password, _UserProfile.PasswordSalt);
            if (_UserProfile.Password != passwordHash)
            {
                _Response.Status = VarHelper.ResponseStatus.ERROR.ToString();
                _Response.Message = "Invalid PIN or Password";
                return Ok(_Response);
            }


            //create response user profile
            LoginResponse _User = new LoginResponse
            {
                UserId = _UserProfile.UserId,
                Email = _UserProfile.Email
            };

            //generate token
            var token = await Task.Run(() => TokenHelper.GenerateToken(_User));
            var refreshToken = TokenHelper.GenerateRefreshToken();

            _UserProfile.RefreshToken = refreshToken;
            _UserProfile.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            _DbContext.SaveChanges();

            _User.Token = token;
            _User.RefreshToken = refreshToken;
            _User.TokenExpirationDate = DateTime.Now.AddMinutes(30);
            _User.Email = _UserProfile.Email;
            _User.Firstname = _UserProfile.Firstname;
            _User.Lastname = _UserProfile.Lastname;
            _User.Status = VarHelper.ResponseStatus.SUCCESS.ToString();
            _User.Message = "Login successful";
            return Ok(_User);
        }

        [HttpPost]
        [Route("RefreshToken")]
        [AllowAnonymous]
        public IActionResult Refresh(TokenModelRequest tokenApiModel)
        {
            var _Response = new Response();

            if (tokenApiModel is null)
            {
                _Response.Status = VarHelper.ResponseStatus.ERROR.ToString();
                _Response.Message = "Invalid client request";

                return Ok(_Response);
            }

            string accessToken = tokenApiModel.AccessToken;
            string refreshToken = tokenApiModel.RefreshToken;

            var principal = TokenHelper.GetPrincipalFromExpiredToken(accessToken);
            var userId = principal.Identity.Name; //this is mapped to the Name claim by default

            var user = _DbContext.UserProfiles.SingleOrDefault(u => u.UserId == userId);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                _Response.Status = VarHelper.ResponseStatus.ERROR.ToString();
                _Response.Message = "Invalid client request";

                return Ok(_Response);
            }

            var newAccessToken = TokenHelper.GenerateAccessToken(principal.Claims);
            var newRefreshToken = TokenHelper.GenerateRefreshToken();

            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            user.RefreshToken = newRefreshToken;
            _DbContext.SaveChanges();

            return Ok(new AuthenticatedResponse()
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Status = VarHelper.ResponseStatus.SUCCESS.ToString(),
                Message = "Token refreshed",
            });
        }

        [HttpPost]
        [Route("RevokeToken")]
        public async Task<IActionResult> Revoke()
        {
            var _Response = new Response();

            var userId = User.Identity.Name;

            var user = await _DbContext.UserProfiles.SingleOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                _Response.Status = VarHelper.ResponseStatus.ERROR.ToString();
                _Response.Message = "Invalid User request";

                return Ok(_Response);
            }

            user.RefreshToken = null;

            await _DbContext.SaveChangesAsync();

            _Response.Status = VarHelper.ResponseStatus.SUCCESS.ToString();
            _Response.Message = "Token revoked";

            return Ok(_Response);
        }


    }
}
