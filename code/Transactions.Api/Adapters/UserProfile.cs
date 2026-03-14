using AutoMapper;
using Transactions.Api.PresentationContracts;
using Libs.Auth.Models;

namespace Transactions.Api.Adapters;

public class UserProfile : Profile
{

    public UserProfile()
    {
        CreateMap<User, UserResponse>();
    }
}