using AutoMapper;
using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.Models;

namespace Expenses.Api.Adapters;

public class ModuleProfile : Profile
{

    public ModuleProfile()
    {
        CreateMap<Module, ModuleResponse>();
        CreateMap<ModuleForm, Module>();
    }
}