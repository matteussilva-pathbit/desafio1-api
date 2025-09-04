using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IAuthService
    {
        Task<string> SignUpAsync(string nome, string email, string username, string senha, CancellationToken ct);
        Task<string> LoginAsync(string usernameOrEmail, string senha, CancellationToken ct);
    }
}
