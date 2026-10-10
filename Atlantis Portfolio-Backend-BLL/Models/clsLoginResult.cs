using Atlantis_Portfolio_Backend_DAL.Entities;

namespace Atlantis_Portfolio_Backend_BLL.Models;

public class clsLoginResult
{
    public enOperationResult Result { get; set; }
    public User? User { get; set; }

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }
}