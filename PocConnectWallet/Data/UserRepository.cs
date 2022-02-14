using Microsoft.EntityFrameworkCore;
using PocConnectWallet.Models;

namespace PocConnectWallet.Data
{
    public class UserRepository
    {
        public async Task SaveUser(UserData inputUser)
        {
            using (var context = new PocDbContext())
            {
                await context.Database.EnsureCreatedAsync();

                var user = await context.Users.FindAsync(inputUser.Id);
                if (user != null)
                {
                    user.Name = inputUser.Name;
                    user.Email = inputUser.Email;
                    user.PhoneNumber = inputUser.PhoneNumber;
                    user.DocumentNumber = inputUser.DocumentNumber;
                    user.WalletAddress = inputUser.WalletAddress;
                    user.WalletChain = inputUser.WalletChain;

                    context.Users.Update(user);
                }
                else
                {
                    var newUser = new UserData()
                    {
                        Id = Guid.NewGuid(),
                        Name = inputUser.Name,
                        Email = inputUser.Email,
                        PhoneNumber = inputUser.PhoneNumber,
                        DocumentNumber = inputUser.DocumentNumber,
                        WalletAddress = inputUser.WalletAddress,
                        WalletChain = inputUser.WalletChain
                    };
                    context.Users.Add(newUser);
                }

                await context.SaveChangesAsync();
            }
        }

        public async Task<UserData?> GetUserByWallet(string walletAddress, string walletChain)
        {
            using (var context = new PocDbContext())
            {
                await context.Database.EnsureCreatedAsync();

                return await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.WalletAddress.Equals(walletAddress) && u.WalletChain.Equals(walletChain));
            }
        }

        public async Task RemoveUser(Guid userId)
        {
            using (var context = new PocDbContext())
            {
                await context.Database.EnsureCreatedAsync();

                var user = await context.Users.FindAsync(userId);
                if (user != null)
                {
                    context.Users.Remove(user);
                    await context.SaveChangesAsync();
                }
            }

        }

    }
}
