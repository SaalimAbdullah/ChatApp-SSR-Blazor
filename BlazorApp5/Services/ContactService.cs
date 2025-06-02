// Services/ContactService.cs
using BlazorApp5.Data;
using BlazorApp5.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class ContactService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    public ContactService(IDbContextFactory<ApplicationDbContext> dbFactory, UserManager<ApplicationUser> userManager)
    {
        _dbFactory = dbFactory;
        _userManager = userManager;
    }

    public async Task<List<Contact>> GetContactsAsync(ApplicationUser currentUser)
    {
        await using var dbContext = _dbFactory.CreateDbContext();

        return await dbContext.Contacts
            .Where(c => c.OwnerUserId == currentUser.Id)
            .Select(c => new
            {
                Contact = c,
                LastMessage = dbContext.Messages
                    .Where(m =>
                        (m.SenderCode == currentUser.UserCode && m.ReceiverCode == c.ContactUserCode) ||
                        (m.SenderCode == c.ContactUserCode && m.ReceiverCode == currentUser.UserCode))
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.SentAt)
                    .FirstOrDefault()
            })
            .OrderByDescending(x => x.LastMessage)
            .Select(x => x.Contact)
            .ToListAsync();

    }

    public async Task<bool> AddContactByCodeAsync(ApplicationUser currentUser, string contactCode, string? displayName = null)
    {
        await using var dbContext = _dbFactory.CreateDbContext();

        if (contactCode == currentUser.UserCode)
            return false;

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserCode == contactCode);
        if (user==null)
            return false;

        var alreadyExists = await dbContext.Contacts.AnyAsync(c =>
            c.OwnerUserId == currentUser.Id && c.ContactUserCode == contactCode);

        if (alreadyExists)
            return false;

        var contact = new Contact
        {
            OwnerUserId = currentUser.Id,
            ContactUserCode = contactCode,
            ContactDisplayName = displayName ?? user.UserName
        };

        dbContext.Contacts.Add(contact);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveContactAsync(ApplicationUser currentUser, string contactCode)
    {
        await using var dbContext = _dbFactory.CreateDbContext();

        var contact = await dbContext.Contacts.FirstOrDefaultAsync(c =>
            c.OwnerUserId == currentUser.Id && c.ContactUserCode == contactCode);

        if (contact == null)
            return false;

        dbContext.Contacts.Remove(contact);
        try
        {
            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another process deleted it, ignore gracefully
            return false;
        }
    }

    public async Task<bool> UpdateNicknameAsync(ApplicationUser currentUser, string contactCode, string newNickname)
    {
        await using var dbContext = _dbFactory.CreateDbContext();

        var contact = await dbContext.Contacts.FirstOrDefaultAsync(c =>
            c.OwnerUserId == currentUser.Id && c.ContactUserCode == contactCode);

        if (contact == null) return false;

        contact.ContactDisplayName = newNickname;
        await dbContext.SaveChangesAsync();
        return true;
    }



}