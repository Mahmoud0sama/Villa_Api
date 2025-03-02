using Villa_Web.Models.Dto;

namespace Villa_Web.Services.IServices
{
    public interface IAuthService
    {
        Task<T> LoginAsync<T>(LoginRequestDTO obj);
        Task<T> RegisterAsync<T>(RegisterationRequestDTO obj);
    }
}
