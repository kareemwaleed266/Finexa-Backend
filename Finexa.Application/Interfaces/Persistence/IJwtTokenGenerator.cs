using Finexa.Domain.Entities.Identity;

public interface IJwtTokenGenerator
{
    Task <string>  GenerateTokenAsync(AppUser user);
}