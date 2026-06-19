namespace Application.Abstractions;

public interface IAuthCookieService
{
    void WriteAccessToken(string token);
    void ClearAccessToken();
}
