using Microsoft.AspNetCore.Components;
using Nethereum.JsonRpc.Client.RpcMessages;
using Nethereum.Signer;

namespace PocConnectWallet.Components
{
	public partial class MetamaskConnectButton
	{
		[Parameter]
		public bool IsConnected { get; set; }

		[Parameter]
        public bool HasMetaMask { get; set; }

		[Parameter]
		public EventCallback OnClickEvent { get; set; }

		public string ButtonTitle
		{
			get
			{
				return HasMetaMask ? "Connect with MetaMask" : "Install MetaMask";
			}
		}
	}
}
