using Application.DTOs.Responses.Bases;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Extensions
{
	public static class JwtConfig
	{
		public static IServiceCollection AddJWTServices(this IServiceCollection services, IConfiguration configuration)
		{
			var key = configuration["Authentication:Key"] ?? throw new ArgumentNullException("Authentication:Key not found in configuration");
			var issuer = configuration["Authentication:Issuer"] ?? throw new ArgumentNullException("Authentication:Issuer not found in configuration");
			var audience = configuration["Authentication:Audience"] ?? throw new ArgumentNullException("Authentication:Audience not found in configuration");

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.Events = new JwtBearerEvents
				{
					OnChallenge = context =>
					{
						// suppress the default WWW-Authenticate header handling and return a JSON body
						context.HandleResponse();

						var body = new BaseResponse<string>(
							message: "Missing or invalid token",
							statusCode: Domain.Enums.StatusCodes.Unauthorized,
							data: null
						);

						context.Response.StatusCode = (int)Domain.Enums.StatusCodes.Unauthorized;
						context.Response.ContentType = "application/json";
						return context.Response.WriteAsJsonAsync(body);
					},
					OnForbidden = context =>
					{
						var body = new BaseResponse<string>(
							message: "You are not authorized to access this resource",
							statusCode: Domain.Enums.StatusCodes.Forbidden,
							data: null
						);

						context.Response.StatusCode = (int)Domain.Enums.StatusCodes.Forbidden;
						context.Response.ContentType = "application/json";
						return context.Response.WriteAsJsonAsync(body);
					}
				};

				options.RequireHttpsMetadata = false;
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = issuer,
					ValidAudience = audience,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
					ClockSkew = TimeSpan.Zero
				};
			});

			return services;
		}
	}
}
