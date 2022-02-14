export function hasMetaMask() {
    return window.ethereum !== undefined;
}

export function isConnected() {
    return window.ethereum !== undefined && (ethereum.selectedAddress != undefined || ethereum.selectedAddress != null);
}

export async function checkMetaMask() {
    if (!hasMetaMask())
        throw "No MetaMask installed";

    if (ethereum.selectedAddress === null || ethereum.selectedAddress === undefined) {
        try {
            await requestAccounts();
        } catch (e) {
            throw "User denied";
        }
    }
    else {
        console.log("Selected: " + ethereum.selectedAddress);
    }
}

export async function requestAccounts() {
    var result = await ethereum.request({
        method: 'eth_requestAccounts'
    });

    console.log(result);
}

export async function getSelectedAddress() {
    await checkMetaMask();
    return ethereum.selectedAddress;
}

export async function getSelectedChain() {
    await checkMetaMask();
    var result = await ethereum.request({
        method: 'eth_chainId'
    });
    return result;
}

export async function getTransactionCount() {
    await checkMetaMask();

    var result = await ethereum.request({
        method: 'eth_getTransactionCount',
        params:
            [
                ethereum.selectedAddress,
                'latest'
            ]

    });
    return result;
}

export async function signTypedData(label, value) {
    await checkMetaMask();

    const msgParams = [
        {
            type: 'string', // Valid solidity type
            name: label,
            value: value
        }
    ]

    try {
        var result = await ethereum.request({
            method: 'eth_signTypedData',
            params:
                [
                    msgParams,
                    ethereum.selectedAddress
                ]
        });

        return result;
    } catch (error) {
        // User denied account access...
        throw "UserDenied"
    }
}

export async function signTypedDataV4(typedData) {
    await checkMetaMask();

    try {
        var result = await ethereum.request({
            method: 'eth_signTypedData_v4',
            params:
                [
                    ethereum.selectedAddress,
                    typedData
                ],
            from: ethereum.selectedAddress
        });

        return result;
    } catch (error) {
        // User denied account access...
        throw "UserDenied"
    }
}

export async function sendTransaction(to, value, data) {
    await checkMetaMask();

    const transactionParameters = {
        to: to,
        from: ethereum.selectedAddress,
        value: value,
        data: data
    };

    try {
        var result = await ethereum.request({
            method: 'eth_sendTransaction',
            params: [transactionParameters]
        });

        return result;
    } catch (error) {
        if (error.code == 4001) {
            throw "UserDenied"
        }
        throw error;
    }
}

export async function genericRpc(method, params) {
    await checkMetaMask();

    var result = await ethereum.request({
        method: method,
        params: params
    });

    return result;
}

export async function listenToChangeEvents() {
    if (hasMetaMask()) {
        ethereum.on("accountsChanged", function (accounts) {
            if (accounts.length > 0) {
                DotNet.invokeMethodAsync('PocConnectWallet', 'MetaMaskServiceOnAccountsChanged', accounts[0]);
            }
            else {
                DotNet.invokeMethodAsync('PocConnectWallet', 'MetaMaskServiceOnDisconnected');
            }
        });

        ethereum.on("chainChanged", function (chainId) {
            DotNet.invokeMethodAsync('PocConnectWallet', 'MetaMaskServiceOnChainChanged', chainId);
        });
    }
}

export async function goToMetamaskDownloadPage() {
    var userAgent = navigator.userAgent;
    var downloadUrl = null;

    if (userAgent.match(/chrome|chromium|crios/i)) {
        downloadUrl = 'https://chrome.google.com/webstore/detail/nkbihfbeogaeaoehlefnkodbefgpgknn';
    } else if (userAgent.match(/firefox|fxios/i)) {
        downloadUrl = 'https://addons.mozilla.org/en-US/firefox/addon/ether-metamask/';
    } else if (userAgent.match(/edg/i)) {
        downloadUrl = 'https://microsoftedge.microsoft.com/addons/detail/metamask/ejbalbakoplchlghecdalmeeeajnimhm';
    } else {
        if (window.navigator.brave) {
            downloadUrl = 'https://chrome.google.com/webstore/detail/nkbihfbeogaeaoehlefnkodbefgpgknn';
        }
        else {
            downloadUrl = 'https://metamask.io/download';
        }
    }

    window.open(downloadUrl, '_blank').focus();
}