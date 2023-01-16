using HtmlAgilityPack;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using OkooraAssignment.Interfaces;

namespace OkooraAssignment.Classes
{
    internal class DataReader : IDataReader
    {
        // The list of coin pairs to pull data for
        private readonly string[] _coinPairs;
        // Concurrent dictionary to store the coin pairs data
        public ConcurrentDictionary<string, CoinPair> coinPairsData = new ConcurrentDictionary<string, CoinPair>();

        public DataReader(string[] coinPairs)
        {
            _coinPairs = coinPairs;
        }

        public void ReadData()
        {
            // Dataflow block to fetch coin pairs data
            var fetchData = new TransformBlock<string, CoinPair>(async coinPairName =>
            {
                var web = new HtmlWeb();
                web.UserAgent = "Mozilla/5.0 (Windows NT 10.0;Win64) AppleWebkit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36";
                string fromCurrency = coinPairName.Substring(0, 3);
                string toCurrency = coinPairName.Substring(4, 3);
                string urlTemplate = $"https://www.xe.com/currencyconverter/convert/?Amount=1&From={fromCurrency}&To={toCurrency}";
                var doc = await web.LoadFromWebAsync(urlTemplate);
                Console.WriteLine($"Concurrently reading the conversion rate from {fromCurrency} to {toCurrency} from the website " +
                $"{urlTemplate}\n");

                // Parse the HTML to get the coin pair conversion rate and update date
                string conversionValueXpath = "/html/body/div[1]/div[2]/div[2]/section/div[2]/div/main/div/div[2]/div[1]/p[2]/text()[1]";
                string updateDateXpath = "/html/body/div[1]/div[2]/div[2]/section/div[2]/div/main/div/div[2]/div[3]/div[2]/div[2]";
                var conversionRate = double.Parse(doc.DocumentNode.SelectSingleNode(conversionValueXpath).InnerText);
                var updateDate = doc.DocumentNode.SelectSingleNode(updateDateXpath).InnerText;

                return new CoinPair
                {
                    CoinPairName = coinPairName,
                    ConversionRate = conversionRate,
                    UpdateDate = updateDate.Substring(updateDate.IndexOf("updated") + 8)
                };
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded });

            // Dataflow block to add each coin pair data to the dictionary
            var addData = new ActionBlock<CoinPair>(coinPair =>
            {
                if (coinPairsData.TryAdd(coinPair.CoinPairName, coinPair))
                    Console.WriteLine($"Added {coinPair.CoinPairName} to the dictionary.\n");
            });

            // Link all the dataflow blocks together
            fetchData.LinkTo(addData);

            foreach (var coinPairName in _coinPairs)
            {
                fetchData.Post(coinPairName);
            }

            fetchData.Complete();

            fetchData.Completion.Wait();

            addData.Complete();
        }
    }
}
