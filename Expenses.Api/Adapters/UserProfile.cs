using AutoMapper;
using Expenses.Api.PresentationContracts;
using Libs.Auth.Models;

namespace Expenses.Api.Adapters;

public class UserProfile : Profile {

    public UserProfile()
    {
        CreateMap<User, UserResponse>();
    }
}