#region
using System;
using System.Collections.Generic;
using UnityEngine;
#endregion

public static class CSVParser
{
	const string filePath = "Effects"; // Path to your CSV file (Resources relative)

	// This method reads the CSV file and returns a list of tuples containing each row's data
	public static List<(string name, string description, string type, string duration, string target, string appliesTiming)> Parse()
	{
		var csvData = Resources.Load<TextAsset>(filePath);

		if (csvData == null)
		{
			Debug.LogError("CSV file not found at path: " + filePath);
			return null;
		}

		List<(string name, string description, string type, string duration, string target, string appliesTiming)> buffDataList = new ();

		string[] rows = csvData.text.Split
		(new[]
		 { "\n" }, StringSplitOptions.RemoveEmptyEntries);

		// Skip header row and start reading data from the second row
		for (int i = 1; i < rows.Length; i++)
		{
			string row = rows[i];
			string[] columns = row.Split(',');

			// Ensure the row has the correct number of columns
			if (columns.Length == 6)
			{
				string name = columns[0].Trim();
				string description = columns[1].Trim();
				string type = columns[2].Trim();
				string duration = columns[3].Trim();
				string target = columns[4].Trim();
				string appliesTiming = columns[5].Trim();

				buffDataList.Add((name, description, type, duration, target, appliesTiming));
			}
			else { Debug.LogWarning("Invalid CSV row: " + row); }
		}

		return buffDataList;
	}
}
