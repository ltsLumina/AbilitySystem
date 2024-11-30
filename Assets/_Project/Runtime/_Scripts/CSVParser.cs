#region
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
#endregion

public static class CSVParser
{
	public static TextAsset FetchCSV()
	{
		const string url = "https://docs.google.com/spreadsheets/d/e/2PACX-1vS8vkdDCCy2djNwwfp9XsyFAsIU0Nc74Rl9kOMlD9Jr1wD-vKs2D04-91qzmdEfxc3ZTfiyf8gatdjD/pub?gid=0&single=true&output=csv";
		using UnityWebRequest request = UnityWebRequest.Get(url);

		UnityWebRequestAsyncOperation operation = request.SendWebRequest();

		while (!operation.isDone)

				// Keeps the editor some-what responsive
			Thread.Sleep(10);

		switch (request.result)
		{
			case UnityWebRequest.Result.Success: {
				Logger.Log("CSV fetched successfully.");
				var item = new TextAsset(request.downloadHandler.text);
				item.name = "Fetched CSV File*";

				return item;
			}

			case UnityWebRequest.Result.InProgress:
				Logger.Log("CSV fetch in progress...");
				break;

			case UnityWebRequest.Result.ConnectionError:
				Logger.LogError("Failed to fetch CSV: Connection error." + "\nPlease check your internet connection." + $"\n{request.error}");
				break;

			case UnityWebRequest.Result.ProtocolError:
				Logger.LogError("Failed to fetch CSV: Protocol error." + "\nThe requested URL is not a valid CSV file." + $"\n{request.error}");
				break;

			case UnityWebRequest.Result.DataProcessingError:
				Logger.LogError("Failed to fetch CSV: Data processing error.");
				break;

			default:
				Logger.LogError("Failed to fetch CSV: " + request.error);
				break;
		}

		return null;
	}

	// This method reads the CSV file and returns a list of tuples containing each row's data
	public static List<(string name, string description, string type, string duration, string target, string timing)> Parse(TextAsset obj)
	{
		if (obj == null)
		{
			Debug.LogError("CSV file not found.");
			return null;
		}

		List<(string name, string description, string type, string duration, string target, string timing)> buffDataList = new ();

		string[] rows = obj.text.Split
		(new[]
		 { "\n" }, StringSplitOptions.RemoveEmptyEntries);

		// Skip the header row and start reading data from the second row
		for (int i = 1; i < rows.Length; i++)
		{
			string row = rows[i];
			string[] columns = row.Replace(", ", "; ").Split(',');

			// Ensure the row has the correct number of columns
			if (columns.Length == 6)
			{
				string name = columns[0].Trim();
				string description = columns[1].Trim();
				string type = columns[2].Trim();
				string duration = columns[3].Trim();
				string target = FormatTarget(columns[4].Trim());
				string appliesTiming = columns[5].Trim();

				buffDataList.Add((name, description, type, duration, target, appliesTiming));
			}
			else { Debug.LogWarning("Invalid CSV row: " + row); }
		}

		return buffDataList;
	}

	// Format the target field to handle multiple values
	static string FormatTarget(string targetField)
	{
		// replace the comma with a semicolon
		targetField = targetField.Replace(", ", "; ");

		// Split the target field by ", " (with space after comma) and remove empty entries
		string[] targetParts = targetField.Split
		(new[]
		 { "; " }, StringSplitOptions.RemoveEmptyEntries);

		// Join the target parts with " & Target." to match the expected format
		return "Target." + string.Join(" | Target.", targetParts);
	}
}
