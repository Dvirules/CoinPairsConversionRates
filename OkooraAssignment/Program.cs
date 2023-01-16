using OkooraAssignment.Classes;

class Program
{
    static void Main()
    {
        Console.WriteLine("Starting data reading...\n");

        string[] coinPairsToRead = { "USD-ILS", "GBP-EUR", "EUR-JPY", "EUR-USD" };
        DataReader reader = new DataReader(coinPairsToRead);

        reader.ReadData();

        CoinPrinter printer = new CoinPrinter(reader.coinPairsData);
        printer.PrintCoins();
    }
}
