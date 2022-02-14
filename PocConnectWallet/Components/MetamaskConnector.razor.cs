using Microsoft.AspNetCore.Components;
using Nethereum.JsonRpc.Client.RpcMessages;
using Nethereum.Signer;
using PocConnectWallet.Models;

namespace PocConnectWallet.Components
{
    public partial class MetamaskConnector : IDisposable
    {
        [Inject]
        public MetaMaskService _metamaskService { get; set; } = default!;

        public bool HasMetaMask { get; set; }
        public string? SelectedAddress { get; set; }
        public string? SelectedChain { get; set; }
        public string? TransactionCount { get; set; }
        public string? SignedData { get; set; }
        public string? SignedDataV4 { get; set; }
        public string? RpcResult { get; set; }
        public Chain? Chain { get; set; }

        protected override async Task OnInitializedAsync()
        {
            SubscribeEvents();
            
            HasMetaMask = await _metamaskService.HasMetaMask();
            if (HasMetaMask)
                await _metamaskService.ListenToEvents();

            if (await _metamaskService.IsConnected())
            {
                await GetSelectedAddress();
                await GetSelectedNetwork();
            }
        }

        private async Task MetaMaskService_ChainChangedEvent((long, Chain) arg)
        {
            await GetSelectedNetwork();
            StateHasChanged();
        }

        private async Task MetaMaskService_AccountChangedEvent(string arg)
        {
            await GetSelectedAddress();
            StateHasChanged();
        }

        private async Task MetaMaskService_DisconnectedEvent()
        {
            await Task.FromResult(() =>
            {
                ClearData();
                return true;
            });
            
            StateHasChanged();
        }

        public async Task ConnectMetaMask()
        {
            if (!await _metamaskService.ConnectToMetaMask())
                ClearData();

            await GetSelectedAddress();
            await GetSelectedNetwork();
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

            SelectedChain = $"ChainID: {chainInfo.chainId}, Name: {chainInfo.chain.ToString()}";
            Console.WriteLine($"ChainID: {chainInfo.chainId}");
        }

        public async Task GetTransactionCount()
        {
            var transactionCount = await _metamaskService.GetTransactionCount();
            TransactionCount = $"Transaction count: {transactionCount}";
        }

        public async Task SignData(string label, string value)
        {
            try
            {
                var result = await _metamaskService.SignTypedData("test label", "test value");
                SignedData = $"Signed: {result}";
            }
            catch (Exception ex)
            {
                SignedData = $"Exception: {ex}";
            }
        }

        public async Task SignDataV4()
        {
            try
            {
                var chainInfo = await _metamaskService.GetSelectedChain();

                var data = new TypedDataPayload<Message>
                {
                    Domain = new Domain
                    {
                        Name = "AAA",
                        Version = "1",
                        ChainId = chainInfo.chainId
                    },
                    Types = new Dictionary<string, TypeMemberValue[]>
                    {
                        ["EIP712Domain"] = new[]
                        {
                    new TypeMemberValue { Name = "name", Type = "string" },
                    new TypeMemberValue { Name = "version", Type = "string" },
                    new TypeMemberValue { Name = "chainId", Type = "uint256" }
                },
                        ["Message"] = new[]
                        {
                    new TypeMemberValue { Name = "contents", Type = "string" }
                }
                    },
                    PrimaryType = "Message",
                    Message = new Message
                    {
                        contents = "Salut"
                    }
                };

                var result = await _metamaskService.SignTypedDataV4(data.ToJson());

                SignedDataV4 = $"Signed: {result}";
            }
            catch (Exception ex)
            {
                SignedDataV4 = $"Exception: {ex}";
            }
        }

        public async Task GenericRpc()
        {
            var result = await _metamaskService.RequestAccounts();
            RpcResult = $"RPC result: {result}";
        }

        public async Task GetBalance()
        {
            var address = await _metamaskService.GetSelectedAddress();
            var result = await _metamaskService.GetBalance(address);
            RpcResult = $"Balance result: {result} wei";
        }

        private void ClearData()
        {
            SelectedAddress = null;
            SelectedChain = null;
            TransactionCount = null;
            SignedData = null;
            SignedDataV4 = null;
            RpcResult = null;
            Chain = null;
        }

        private void SubscribeEvents()
        {
            MetaMaskService.AccountChangedEvent += MetaMaskService_AccountChangedEvent;
            MetaMaskService.ChainChangedEvent += MetaMaskService_ChainChangedEvent;
            MetaMaskService.DisconnectedEvent += MetaMaskService_DisconnectedEvent;
        }

        public void Dispose()
        {
            MetaMaskService.AccountChangedEvent -= MetaMaskService_AccountChangedEvent;
            MetaMaskService.ChainChangedEvent -= MetaMaskService_ChainChangedEvent;
            MetaMaskService.DisconnectedEvent -= MetaMaskService_DisconnectedEvent;
        }
    }
}
