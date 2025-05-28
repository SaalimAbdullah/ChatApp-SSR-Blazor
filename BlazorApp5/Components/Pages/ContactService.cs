// Services/ContactService.cs
using BlazorApp5.Data;
using BlazorApp5.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class ContactService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
{
    public async Task<List<Contact>> GetContactsAsync(ApplicationUser currentUser)
    {
        return await dbContext.Contacts
            .Where(c => c.OwnerUserId == currentUser.Id)
            .ToListAsync();
    }

    public async Task<bool> AddContactByCodeAsync(ApplicationUser currentUser, string contactCode, string? displayName = null)
    {
        if (contactCode == currentUser.UserCode)
            return false;

        var userExists = await dbContext.Users.AnyAsync(u => u.UserCode == contactCode);
        if (!userExists)
            return false;

        var alreadyExists = await dbContext.Contacts.AnyAsync(c =>
            c.OwnerUserId == currentUser.Id && c.ContactUserCode == contactCode);

        if (alreadyExists)
            return false;

        var contact = new Contact
        {
            OwnerUserId = currentUser.Id,
            ContactUserCode = contactCode,
            ContactDisplayName = displayName ?? contactCode
        };

        dbContext.Contacts.Add(contact);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
