using System;
namespace InfiniteQuoteGuesser.Models
{
	public class QuoteDetailed
	{
		//Model to store relevant details related to retrieved quote
		public string quote { get; set; }
		public string author { get; set; }
		public string category { get; set; }

		public QuoteDetailed()
		{
		}
	}
}

