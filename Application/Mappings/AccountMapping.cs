using Application.DTOs.Requests.Accounts;
using Application.DTOs.Responses.Accounts;
using Domain.Entities;
using Mapster;

namespace Application.Mappings
{
	public class AccountMapping : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<SystemAccount, GetResponse>()
				.Map(dest => dest.AccountId, src => src.AccountId)
				.Map(dest => dest.AccountName, src => src.AccountName)
				.Map(dest => dest.AccountEmail, src => src.AccountEmail)
				.Map(dest => dest.AccountRole, src => src.AccountRole);

			config.NewConfig<CreateRequest, SystemAccount>()
				.Map(dest => dest.AccountName, src => src.AccountName)
				.Map(dest => dest.AccountEmail, src => src.AccountEmail)
				.Map(dest => dest.AccountPassword, src => src.Password)
				.Map(dest => dest.AccountRole, src => src.AccountRole);

			config.NewConfig<UpdateRequest, SystemAccount>()
				.IgnoreNullValues(true)
				.Map(dest => dest.AccountName, src => src.AccountName)
				.Map(dest => dest.AccountEmail, src => src.AccountEmail)
				.Map(dest => dest.AccountPassword, src => src.Password)
				.Map(dest => dest.AccountRole, src => src.AccountRole);

			config.NewConfig<UpdateProfileRequest, SystemAccount>()
				.IgnoreNullValues(true)
				.Map(dest => dest.AccountName, src => src.AccountName)
				.Map(dest => dest.AccountEmail, src => src.AccountEmail)
				.Map(dest => dest.AccountPassword, src => src.Password);
		}
	}
}
