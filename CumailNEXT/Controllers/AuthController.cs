using Cumail_REST_API.Components.Auth;
using CumailNEXT.Components.Auth;
using CumailNEXT.Implementation.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace CumailNEXT.Controllers;

internal interface ITestSchema
{
    public int GetValue();
    public string GetName();

    public static ITestSchema CreateTestObject(int customValue = 0, string customName = "test")
    {
        return new TestSchema
        {
            Name = customName,
            Value = customValue
        };
    }
}

internal class TestSchema : ITestSchema
{
    public int Value { get; set; } = 0;
    public string Name { get; set; } = "test";
    
    public int GetValue() => Value;
    public string GetName() => Name;
}

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("signup")]
    public IActionResult SignupWithEmailPassword(AuthRequest request)
    {
        AuthProvider auth = Engine.Auth;
        try
        {
            AuthToken re = auth.SignupWithEmailPassword(request);
            
            return StatusCode(200, re);
            // return StatusCode(200, JsonConvert.SerializeObject(re));
        } catch (EmailExistedException)
        {
            return StatusCode(400, "EMAIL_EXISTED");
        } catch (InvalidPasswordException)
        {
            return StatusCode(400, "PASSWORD_TOO_SHORT");
        } catch (MalformedEmailException)
        {
            return StatusCode(400, "MALFORMED_EMAIL");
        } catch (Exception)
        {
            return StatusCode(500, "BUG_ENCOUNTERED");
        }
    }
    [HttpPost("login")]
    public IActionResult LoginWithEmailPassword(AuthRequest request)
    {
        AuthProvider authProvide = Engine.Auth;
        try
        {
            AuthToken re = authProvide.LoginWithEmailPassword(request);

            return StatusCode(200, re);
            // return StatusCode(200, JsonConvert.SerializeObject(re));
        }
        catch (InvalidLoginCredentialException)
        {
            return StatusCode(400, "INCORRECT_EMAIL_OR_PASSWORD");
        }
    }
    [HttpGet("fetch_profile")]
    public IActionResult FetchUserProfile()
    {
        AuthProvider auth = Engine.Auth;
        IHeaderDictionary headers = Request.Headers;
        string? authorizationHeader = headers.Authorization;
        authorizationHeader ??= "";
        if (!authorizationHeader.StartsWith("Bearer "))
            return StatusCode(401, "NOT_ENOUGH_PERMISSION");
        string authorization = authorizationHeader[7..];
        try
        {
            var rawProfile = auth.GetUserByIdToken(new AuthToken
            {
                IdToken = authorization
            });
            return StatusCode(200, new Dictionary<string, string>
            {
                { "userName", rawProfile.UserLoginKey },
                { "userUUID", rawProfile.UserUuid }
            });
        }
        catch (InvalidTokenException)
        {
            return StatusCode(400, "INVALID_AUTHORIZATION");
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_ERROR");
        }
    }

    [HttpGet("test_schema")]
    public IActionResult InternalSchemaTest()
    {
        ITestSchema newObject = ITestSchema.CreateTestObject(1, "ok");
        return Ok(newObject);
        // return StatusCode(200, newObject);
    }

    [HttpPost("another_test_schema")]
    public IActionResult InternalSchemaTestNew(Dictionary<string, object> dict)
    {
        var d = dict;
        return Ok(Request.Body.ToString());
    }
}