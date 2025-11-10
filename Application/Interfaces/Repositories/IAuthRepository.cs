using Domain.Entities;

namespace Application.Interfaces.Repositories
{
	public interface IAuthRepository
	{
		string GenerateJwtToken(SystemAccount user, string role);
	}
}
