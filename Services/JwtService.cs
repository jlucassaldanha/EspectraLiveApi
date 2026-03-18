using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SpectraLiveApi.Services;

public class JwtService
{
	private readonly string _secretKey;

	public JwtService(IConfiguration config)
	{
		_secretKey = config["SpectraLive:SecretKey"] ?? throw new ArgumentException("Chave secreta do JWT não encontrada");
	}

	public string GenerateToken(string userId, string twitchId)
	{
		var key = Encoding.ASCII.GetBytes(_secretKey);

		var claims = new[]
		{
			new Claim("userId", userId),
			new Claim("twitchId", twitchId)
		};

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Expires = DateTime.UtcNow.AddDays(7),
			SigningCredentials = new SigningCredentials(
				new SymmetricSecurityKey(key),
				SecurityAlgorithms.HmacSha256Signature
			)
		};

		var tokenHandler = new JwtSecurityTokenHandler();
		var token = tokenHandler.CreateToken(tokenDescriptor);

		return tokenHandler.WriteToken(token);
	}

	public ClaimsPrincipal? ValidateToken(string token)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_secretKey);

		try
		{
			var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = false,
				ValidateAudience = false,
				ClockSkew = TimeSpan.Zero
			}, out SecurityToken validatedToken);

			return principal;
		}
		catch
		{
			return null;
		}
	}
}