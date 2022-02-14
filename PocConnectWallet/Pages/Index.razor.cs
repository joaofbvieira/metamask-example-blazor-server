using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Nethereum.Signer;
using PocConnectWallet.Data;
using PocConnectWallet.Models;

namespace PocConnectWallet.Pages
{
    public partial class Index : IDisposable
    {
        [Inject]
        public MetaMaskService _metamaskService { get; set; } = default!;

        [Inject]
        public UserRepository _userRepository { get; set; } = default!;

        public UserData User { get; set; } = default!;
        public bool HasMetaMask { get; set; }
        public string SelectedAddress { get; set; } = default!;
        public string SelectedChain { get; set; } = default!;
        public Chain? Chain { get; set; }
        public bool IsConnected
        {
            get
            {
                return !string.IsNullOrEmpty(this.SelectedAddress);
            }
        }
        public bool HasUserData
        {
            get
            {
                return !string.IsNullOrEmpty(User?.Name);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            SubscribeEvents();

            HasMetaMask = await _metamaskService.HasMetaMask();
            if (HasMetaMask)
                await _metamaskService.ListenToEvents();

        }

        public async Task GetSelectedAddress()
        {
            SelectedAddress = await _metamaskService.GetSelectedAddress();
            Console.WriteLine($"Address: {SelectedAddress}");
        }

        public async Task GetSelectedNetwork()
        {
            var chainInfo = await _metamaskService.GetSelectedChain();
            Chain = chainInfo.chain;

            SelectedChain = $"ChainID: {chainInfo.chainId}, Name: {chainInfo.chain}";
            Console.WriteLine($"ChainID: {chainInfo.chainId}");
        }

        public async Task HandleMetamaskButtonClick()
        {
            if (IsConnected)
                return;

            if (!HasMetaMask)
            {
                await _metamaskService.SendToMetamaskDownloadPage();
                return;
            }

            await ConnectMetaMask();
        }

        public async Task HandleFillUserDataSubmit(UserData user)
        {
            await InvokeAsync(async () =>
            {
                this.User = user;
                this.User.WalletAddress = this.SelectedAddress;
                this.User.WalletChain = this.SelectedChain;
                await SaveUser();
                await CheckIfUserExists();
                StateHasChanged();
            });
        }

        public async Task HandleNavbarRemoveUserData()
        {
            await InvokeAsync(async () =>
            {
                await _userRepository.RemoveUser(this.User.Id);
                ClearData();
                StateHasChanged();
            });
        }

        public async Task HandleNavbarLogout()
        {
            await InvokeAsync(() =>
            {
                ClearData();
                StateHasChanged();
            });
        }

        private async Task ConnectMetaMask()
        {
            if (!await _metamaskService.ConnectToMetaMask())
            {
                ClearData();
                return;
            }

            await GetSelectedAddress();
            await GetSelectedNetwork();
            await CheckIfUserExists();
        }

        private void SubscribeEvents()
        {
            MetaMaskService.AccountChangedEvent += MetaMaskService_AccountChangedEvent;
            MetaMaskService.DisconnectedEvent += MetaMaskService_DisconnectedEvent;
        }

        private void ClearData()
        {
            SelectedAddress = null;
            SelectedChain = null;
            Chain = null;
            User = default!;
        }

        private async Task MetaMaskService_AccountChangedEvent(string arg)
        {
            await GetSelectedAddress();
            StateHasChanged();
        }

        private async Task MetaMaskService_DisconnectedEvent()
        {
            await InvokeAsync(() =>
            {
                ClearData();
                StateHasChanged();
            });
        }

        private async Task CheckIfUserExists()
        {
            var user = await _userRepository.GetUserByWallet(this.SelectedAddress, this.SelectedChain);
            if (user != null)
            {
                this.User = user;
            }
        }

        private async Task SaveUser()
        {
            await _userRepository.SaveUser(this.User);
        }

        public void Dispose()
        {
            MetaMaskService.AccountChangedEvent -= MetaMaskService_AccountChangedEvent;
            MetaMaskService.DisconnectedEvent -= MetaMaskService_DisconnectedEvent;
        }
    }
}
