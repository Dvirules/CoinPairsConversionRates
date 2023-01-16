using System.Collections.Concurrent;
using OkooraAssignment.Interfaces;

namespace OkooraAssignment.Classes
{
    internal class CoinPrinter : ICoinPrinter
    {
        private readonly ConcurrentDictionary<string, CoinPair> _coinPairsData;

        public CoinPrinter(ConcurrentDictionary<string, CoinPair> coinPairsData)
        {
            _coinPairsData = coinPairsData;
        }

        public void PrintCoins()
        {
            // Print all the coin pair data from the coinPairsData dictionary
            foreach (var coinPair in _coinPairsData)
            {
                Console.WriteLine($"Coin Pair: {coinPair.Value.CoinPairName}");
                Console.WriteLine($"Conversion Rate: {coinPair.Value.ConversionRate}");
                Console.WriteLine($"Update Date: {coinPair.Value.UpdateDate}\n");
            }
        }
    }
}
