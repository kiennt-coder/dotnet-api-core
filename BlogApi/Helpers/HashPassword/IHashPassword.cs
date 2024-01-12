namespace BlogApi.Helpers.HashPassword;

public interface IHashPassword
{
    public string Hash(string password);
    public bool Validate(string passwordHash, string password);
}
