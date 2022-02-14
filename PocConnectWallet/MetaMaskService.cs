using Microsoft.JSInterop;
using Nethereum.Signer;
using System;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using PocConnectWallet.Extensions;
using Nethereum.JsonRpc.Client.RpcMessages;

namespace PocConnectWallet
{
    public class MetaMaskService : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        public static event Func<string, Task>? AccountChangedEvent;
        public static event Func<(long, Chain), Task>? ChainChangedEvent;
        public static event Func<Task>? DisconnectedEvent;

        public MetaMaskService(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => LoadScripts(jsRuntime).AsTask());
        }

        public ValueTask<IJSObjectReference> LoadScripts(IJSRuntime jsRuntime)
        {
            return jsRuntime.InvokeAsync<IJSObjectReference>("import", "./metaMaskJsInterop.js");
        }

        public async ValueTask<bool> HasMetaMask()
        {
            var module = await moduleTask.Value;

            try
            {
                return await module.InvokeAsync<bool>("hasMetaMask");
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async ValueTask<bool> ConnectToMetaMask()
        {
            var module = await moduleTask.Value;

            try
            {
                await module.InvokeVoidAsync("checkMetaMask");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async ValueTask<bool> IsConnected()
        {
            var module = await moduleTask.Value;

            try
            {
                return await module.InvokeAsync<bool>("isConnected");
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async ValueTask<string> GetSelectedAddress()
        {
            var module = await moduleTask.Value;
            try
            {
                return await module.InvokeAsync<string>("getSelectedAddress");
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public async ValueTask ListenToEvents()
        {
            var module = await moduleTask.Value;
            try
            {
                await module.InvokeVoidAsync("listenToChangeEvents");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async ValueTask<(long chainId, Chain chain)> GetSelectedChain()
        {
            var module = await moduleTask.Value;
            try
            {
                string chainHex = await module.InvokeAsync<string>("getSelectedChain", null);
                return ChainHexToChainResponse(chainHex);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async ValueTask<long> GetTransactionCount()
        {
            var module = await moduleTask.Value;
            try
            {
                var result = await module.InvokeAsync<JsonElement>("getTransactionCount");
                var resultString = result.GetString();
                if (resultString != null)
                {
                    long intValue = resultString.HexToInt();
                    return intValue;
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async ValueTask<string> SignTypedData(string label, string value)
        {
            var module = await moduleTask.Value;
            try
            {
                return await module.InvokeAsync<string>("signTypedData", label, value);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async ValueTask<string> SignTypedDataV4(string typedData)
        {
            var module = await moduleTask.Value;
            try
            {
                return await module.InvokeAsync<string>("signTypedDataV4", typedData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async ValueTask<string> SendTransaction(string to, BigInteger weiValue, string? data = null)
        {
            var module = await moduleTask.Value;
            try
            {
                string hexValue = "0x" + weiValue.ToString("X");
                return await module.InvokeAsync<string>("sendTransaction", to, hexValue, data);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async ValueTask<dynamic> GenericRpc(string method, params dynamic?[]? args)
        {
            var module = await moduleTask.Value;
            try
            {
                return await module.InvokeAsync<dynamic>("genericRpc", method, args);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> RequestAccounts()
        {
            var result = await GenericRpc("eth_requestAccounts");

            return result.ToString();
        }

        public async Task<long> GetBalance(string address, string block = "latest")
        {
            var result = await GenericRpc("eth_getBalance", address, block);

            string hex = result.ToString();

            return hex.HexToInt();
        }

        public async Task SendToMetamaskDownloadPage()
        {
            var module = await moduleTask.Value;
            try
            {
                await module.InvokeVoidAsync("goToMetamaskDownloadPage");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JSInvokable("MetaMaskServiceOnAccountsChanged")]
        public static async Task OnAccountsChanged(string selectedAccount)
        {
            if (AccountChangedEvent != null)
            {
                await AccountChangedEvent.Invoke(selectedAccount);
            }
        }

        [JSInvokable("MetaMaskServiceOnChainChanged")]
        public static async Task OnChainChanged(string chainhex)
        {
            if (ChainChangedEvent != null)
            {
                await ChainChangedEvent.Invoke(ChainHexToChainResponse(chainhex));
            }
        }

        [JSInvokable("MetaMaskServiceOnDisconnected")]
        public static async Task OnDisconnected()
        {
            if (DisconnectedEvent != null)
            {
                await DisconnectedEvent.Invoke();
            }
        }

        private static (long chainId, Chain chain) ChainHexToChainResponse(string chainHex)
        {
            long chainId = chainHex.HexToInt();
            return (chainId, (Chain)chainId);
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}
