using AutoMapper;
using Transactions.Api.PresentationContracts;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Domain.Models;

namespace Transactions.Api.Adapters;

public class ModuleProfile : Profile
{

    public ModuleProfile()
    {
        CreateMap<Module, ModuleResponse>();
        CreateMap<ModuleForm, Module>();
    }
}