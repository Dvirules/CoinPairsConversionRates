This project uses Selenium to web scrape one of the largest financial websites, pulls out from it the conversion rates of specefied coin pairs (e.g. ILS-USD), stores the data in a concurrent dictionary and prints the result.
All happens asynchronously and concurrently, with parallelism on multi threads.
Please make sure you have Chrome installed, as the scraper agent uses it.