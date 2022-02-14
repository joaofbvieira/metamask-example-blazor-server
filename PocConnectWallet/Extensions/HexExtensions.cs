﻿namespace PocConnectWallet.Extensions
{
    public static class HexExtensions
    {
        public static long HexToInt(this string hexString)
        {
            if (hexString.StartsWith("0x"))
                hexString = hexString[2..];

            return long.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        }
    }
}
