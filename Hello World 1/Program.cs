using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;



namespace Hello_World_1
{
	// This program shows the results of avg, min & max per Quarter, per year (2015 - 2019) of Megawatts usage.
	// For Day-ahead demand forecast in the region of California (CAL) in https://www.eia.gov/.
	// you can see both results in UTC Time & Local Time.

	class Program
	{
		/// <summary>
		/// To see this information you need access to an Api Key, you may find it in the Solution Explorer in Properties/Settings/apiKey. 
		/// You may change this key if desire by going to this link https://www.eia.gov/opendata/register.php. register to acquire a new key 
		/// and change the one in Properties/Settings/apiKey.
		/// </summary>

		static void Main(string[] _)

		/// Here is the call to access the information in UTC Time or Local Time.
		/// The url in use will return UTC Time results, to change to Local Time, use Local Time url below:
		/// Local Time url: http://api.eia.gov/series/?api_key={apiKey}&series_id=EBA.CAL-ALL.DF.HL&out=xml
		/// UTC Time url: http://api.eia.gov/series/?api_key={apiKey}&series_id=EBA.CAL-ALL.DF.H&out=xml
		{
			string apiKey = Properties.Settings.Default.apiKey;
			var url = $"http://api.eia.gov/series/?api_key={apiKey}&series_id=EBA.CAL-ALL.DF.H&out=xml";
			var client = new System.Net.Http.HttpClient();
			var payload = client.GetStringAsync(url).Result;
			var results = Parse(payload);
			Interpret(results);
		}

		public static (int year, int quarter) QuarterYear(DateTime day)
		{

			/// <summary>
			/// Year 1: [January, March]
			/// Year 2: [April, June]
			/// Year 3: [July, September]
			/// Year 4: [October, December]
			/// </summary>
			/// 

			var year = day.Year;
			switch (day.Month)
			{
				case 1:
				case 2:
				case 3:
					return (year, 1);
				case 4:
				case 5:
				case 6:
					return (year, 2);
				case 7:
				case 8:
				case 9:
					return (year, 3);
				default:
					return (year, 4);
			}
		}

		public static void Interpret(IEnumerable<Data> data)
		{
			// The different results being: avg, min, max per Quarter, per Year

			foreach (var group in data.GroupBy(r => QuarterYear(r.Time)))
			{
				var values = group.Select(g => g.Value).ToList();
				var avg = values.Average();

				var (min, max) = (values.Min(), values.Max());
				//string interpolation
				Console.WriteLine($"For quarter {group.Key}: avg = {avg:f2}, min = {min:f2}, max = {max:f2}");
			}
		}
		public static IEnumerable<Data> Parse(string payload)
		{

			/// Getting Values and Formats

			var styles = System.Globalization.DateTimeStyles.None;
			var formatProvider = default(IFormatProvider);
			var doc = XDocument.Parse(payload);
			foreach (var row in doc.Root.Element("series").Element("row").Element("data").Elements("row"))
			{
				DateTime time;
				decimal value;
				var formats = new[] { "yyyyMMddTHHzz", "yyyyMMddTHHK" };
				if (!DateTime.TryParseExact(row.Element("date").Value, formats, formatProvider, styles, out time))
					continue;
				if (!decimal.TryParse(row.Element("value").Value, out value))
					continue;
				var newValue = new Data
				{
					Time = time,
					Value = value
				};
				yield return newValue;
			}
		}
		public struct Data
		{
			public DateTime Time { get; set; }
			public decimal Value { get; set; }
		}
	}
}


