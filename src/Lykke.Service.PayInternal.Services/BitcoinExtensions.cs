namespace Lykke.Service.PayInternal.Services
{
    public static class BitcoinExtensions
    {
        public const int SatoshiInBtc = 100000000;

        public static decimal SatoshiToBtc(this decimal satoshi)
        {
            return satoshi / SatoshiInBtc;
        }
    }
}
