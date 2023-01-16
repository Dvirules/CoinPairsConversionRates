using HtmlAgilityPack;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using OkooraAssignment.Interfaces;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;

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
                string fromCurrency = coinPairName.Substring(0, 3);
                string toCurrency = coinPairName.Substring(4, 3);

                // Parse the HTML to get the coin pair conversion rate and update date with Selenium
                var options = new ChromeOptions();
                options.AddArgument("--headless");
                var driver = new ChromeDriver(options);
                string urlTemplate = $"https://www.xe.com/currencyconverter/convert/?Amount=1&From={fromCurrency}&To={toCurrency}";
                driver.Navigate().GoToUrl(urlTemplate);
                string ConversionRateXpath = "/html/body/div[1]/div[2]/div[2]/section/div[2]/div/main/div/div[2]/div[1]/p[2]";
                string updateDateXpath = "/html/body/div[1]/div[2]/div[2]/section/div[2]/div/main/div/div[2]/div[3]/div[2]/div[2]";
                string conversionRate = driver.FindElement(By.XPath(ConversionRateXpath)).Text;
                string updateDate = driver.FindElement(By.XPath(updateDateXpath)).Text;
                Console.WriteLine($"Scraping coin pair {fromCurrency} to {toCurrency} from the website - {urlTemplate}\n");
                driver.Quit();

                return new CoinPair
                {
                    CoinPairName = coinPairName,
                    ConversionRate = double.Parse(conversionRate.Substring(0, conversionRate.IndexOf(" "))),
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
