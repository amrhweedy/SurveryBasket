
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SurveyBasket.Api.Authentication;

public class JwtProvider(IOptions<JwtOptions> jwtOptions) : IJwtProvider
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public (string token, int expiresIn) GenerateToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        Claim[] claims = [

            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
            new Claim(nameof(roles) , JsonSerializer.Serialize(roles),JsonClaimValueTypes.JsonArray), // JsonClaimValueTypes.JsonArray => to make roles and permissions array of string not plain text
            new Claim(nameof(permissions) , JsonSerializer.Serialize(permissions),JsonClaimValueTypes.JsonArray)

            //new Claim(nameof(roles) , string.Join(",",roles)),  // the roles and permissions will be a string like this "admin,member"
            //new Claim(nameof(permissions) , string.Join(",",permissions)),
             ];


        // this key is responsible for encoding and decoding the token, it's a secret key
        // to generate the token we need to make encode for this key , and when the client get the token we need to decode it by using the same key to validate the token
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));

        // it tells the application what algorithm to use to sign the token and what key to use
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);


        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
            signingCredentials: signingCredentials
            );


        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);  // convert token object to string, in this line happens everything (notion)

        return (tokenString, _jwtOptions.ExpiryMinutes);


    }

    public string? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));  // we need the key to make decode the token using the same key 

        try
        {
            // note=> we dont validate the lifetime of the token it means if the token is expired or not , it will be used to extract the userId
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = symmetricSecurityKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;

            return userId;  // may be null

        }
        catch
        {
            return null;
        }

    }
}
