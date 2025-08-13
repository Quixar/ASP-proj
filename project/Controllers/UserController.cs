using project.Services.Kdf;

namespace project.Controllers;

public class UserController(IKdfService kdfService)
{
    private readonly IKdfService _kdfService;
}