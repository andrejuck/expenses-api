using Libs.Auth.Models.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Transactions.Api.Adapters;
using Transactions.Api.Application;
using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models;
using Transactions.Tests.TestHelpers;

namespace Transactions.Tests.Applications;

/// <summary>
/// Covers Documentation/[Feature] Transactions Manager/BDD - Acceptance Criteria/AC - Cards in Home Page.md,
/// specifically the Admin's ability to create and delete modules.
/// </summary>
[TestFixture]
public class ModuleApplicationTests
{
    private IModuleRepository _repository = null!;
    private IModuleAdapter _adapter = null!;
    private Libs.Api.ErrorHandling.IErrorService _errorService = null!;
    private ModuleApplication _sut = null!;

    private readonly Guid _adminId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IModuleRepository>();
        _adapter = new ModuleAdapter();
        _errorService = ErrorServiceTestFactory.CreateStateful();

        _sut = new ModuleApplication(
            _repository,
            _adapter,
            _errorService,
            Substitute.For<ILogger<ModuleApplication>>(),
            Options.Create(new CustomClaimSettings()));
    }

    private static ModuleForm ValidForm() => new() { Name = "Transactions Manager", AllowedRoles = ["GeneralUser"] };

    [Test]
    public async Task Given_a_unique_module_name_When_creating_a_module_Then_it_is_created()
    {
        _repository.FindByNameAsync("Transactions Manager").Returns((Module?)null);

        await _sut.CreateModuleAsync(ValidForm(), _adminId);

        await _repository.Received(1).AddAsync(Arg.Is<Module>(m => m.Name == "Transactions Manager" && m.CreatedBy == _adminId));
    }

    [Test]
    public async Task Given_a_module_name_that_already_exists_When_creating_a_module_Then_creation_is_rejected()
    {
        _repository.FindByNameAsync("Transactions Manager").Returns(new Module("Transactions Manager", _adminId));

        await _sut.CreateModuleAsync(ValidForm(), _adminId);

        await _repository.DidNotReceive().AddAsync(Arg.Any<Module>());
    }

    [Test]
    public async Task Given_a_module_that_does_not_exist_When_deleting_Then_nothing_happens()
    {
        _repository.FindByIdAsync(Arg.Any<Guid>()).Returns((Module?)null);

        await _sut.DeleteByIdAsync(Guid.NewGuid(), _adminId);

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Module>());
    }

    [Test]
    public async Task Given_an_existing_module_When_an_admin_deletes_it_Then_it_is_marked_as_deleted()
    {
        var module = new Module("Transactions Manager", _adminId);
        _repository.FindByIdAsync(module.Id).Returns(module);

        await _sut.DeleteByIdAsync(module.Id, _adminId);

        Assert.That(module.DeletedAt, Is.Not.Null);
        await _repository.Received(1).UpdateAsync(module);
    }
}
