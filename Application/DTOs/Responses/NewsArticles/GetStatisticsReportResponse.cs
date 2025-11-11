namespace Application.DTOs.Responses.NewsArticles
{
	public class GetStatisticsReportResponse
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int TotalArticlesCreated { get; set; }
		public int TotalCategories { get; set; }

		// Additional counts for inactive entities
		public int InactiveCategoriesCount { get; set; }
		public int InactiveArticlesCount { get; set; }

		public IEnumerable<DailyStatistic> DailyBreakdown { get; set; } = Enumerable.Empty<DailyStatistic>();

		public IEnumerable<StatisticBreakdown> CategoryBreakdown { get; set; } = Enumerable.Empty<StatisticBreakdown>();
		public IEnumerable<StatisticBreakdown> AuthorBreakdown { get; set; } = Enumerable.Empty<StatisticBreakdown>();
	}

	public class DailyStatistic
	{
		public DateTime Date { get; set; }
		public int TotalArticles { get; set; }
		public int ActiveArticles { get; set; }
		public int InactiveArticles { get; set; }
	}

	public class StatisticBreakdown
	{
		public int ItemId { get; set; }
		public string ItemName { get; set; } = string.Empty;
		public int TotalArticles { get; set; }
		public double Percentage { get; set; }
	}
}
