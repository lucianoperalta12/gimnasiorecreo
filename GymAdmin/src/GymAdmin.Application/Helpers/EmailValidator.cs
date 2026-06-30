using GymAdmin.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace GymAdmin.Application.Helpers;

public static class EmailValidator
{
    public static async Task<bool> IsValidEmailAsync(string email, IServiceScopeFactory scopeFactory)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch (Exception ex)
        {
            await ErrorLogHelper.LogAsync(scopeFactory, ex, "IsValidEmail", email);
            return false;
        }
    }
}
