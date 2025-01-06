namespace PlaygroundMediator.Features.Tokens.Handlers.Commands
{
    public interface ITokenService
    {
        string GenerateToken(
            string userName,
            IEnumerable<string> roles,
            IEnumerable<string> permissions
        );
    }
}
