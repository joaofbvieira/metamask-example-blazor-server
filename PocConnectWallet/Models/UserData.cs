using System.ComponentModel.DataAnnotations;

namespace PocConnectWallet.Models
{
    public class UserData
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;
        public string? DocumentNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string WalletAddress { get; set; } = default!;
        public string WalletChain { get; set; } = default!;
    }
}
