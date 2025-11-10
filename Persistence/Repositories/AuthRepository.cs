using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Persistence.Repositories
{
	public class AuthRepository : IAuthRepository
	{
		private readonly IGenericRepository<SystemAccount> _accountRepository;
		private readonly IConfiguration _configuration;
		private readonly string _secretKey;
		private readonly string _issuer;
		private readonly string _audience;

		public AuthRepository(IGenericRepository<SystemAccount> accountRepository, IConfiguration configuration)
		{
			_accountRepository = accountRepository;
			_configuration = configuration;
			_secretKey = _configuration["Authentication:Key"]
				?? throw new ArgumentNullException("Authentication:Key not found in configuration");
			_issuer = _configuration["Authentication:Issuer"]
				?? throw new ArgumentNullException("Authentication:Issuer not found in configuration");
			_audience = _configuration["Authentication:Audience"]
				?? throw new ArgumentNullException("Authentication:Audience not found in configuration");
		}

		public string GenerateJwtToken(SystemAccount account, string role)
		{
			ArgumentNullException.ThrowIfNull(account);
			if (string.IsNullOrWhiteSpace(role)) role = AccountRoles.Lecturer.ToString();

			var claims = new List<Claim>
			{
				new(JwtRegisteredClaimNames.Sub, account.AccountId.ToString()),
				new("id", account.AccountId.ToString()),
				new("email", account.AccountEmail ?? string.Empty),
				new("role", role),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _issuer,
				audience: _audience,
				claims: claims,
				notBefore: DateTime.UtcNow,
				expires: DateTime.UtcNow.AddHours(8),
				signingCredentials: creds
			);

			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
			return tokenString;
		}
	}
}
