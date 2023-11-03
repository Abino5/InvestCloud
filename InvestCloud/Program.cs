using System;
using System.Data;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using InvestCloud;
using Newtonsoft;
using Newtonsoft.Json;

string baseURL = "https://recruitment-test.investcloud.com/";

string initDatasetURL = "api/numbers/init/1000";
string retrieveRowURL = "api/numbers/";
string retrieveColURL = "api/numbers/";

int[][] DatasetA  = new int[10][];
int[][] DatasetB= new int[10][];

for (var initIndex = 0; initIndex < 1000; initIndex++)
{

    var RowURL = retrieveRowURL + "A/row/" + initIndex;
    using (var client = new HttpClient())
    {
        var url = new Uri(String.Concat(baseURL, RowURL));
        var rows = JsonConvert.DeserializeObject<DatasetClass>(client.GetStringAsync(url).Result);
        DatasetA = new int[rows.Value.Count][];
        for (int i = 0; i < rows.Value.Count; i++)
        {
            var values = rows.Value[i];
            DatasetA[i] = new int[rows.Value.Count];
            for (int j = 0; j < rows.Value.Count; j++)
            {
                DatasetA[i][j] = int.Parse(rows.Value[j].ToString());
            }
        }

    }
}

for (var initIndex = 0; initIndex < 1000; initIndex++)
{
    Console.WriteLine("Processing : "+ initIndex);
    var RowURL = retrieveRowURL + "B/row/" + initIndex;
    using (var client = new HttpClient())
    {
        var url = new Uri(String.Concat(baseURL, RowURL));
        var rows = JsonConvert.DeserializeObject<DatasetClass>(client.GetStringAsync(url).Result);
        DatasetB = new int[rows.Value.Count][];
        for (int i = 0; i < rows.Value.Count; i++)
        {
            var values = rows.Value[i];
            DatasetB[i] = new int[rows.Value.Count];
            for (int j = 0; j < rows.Value.Count; j++)
            {
                DatasetB[i][j] = int.Parse(rows.Value[j].ToString());
            }
        }

    }
}


var resultMatrix = MultiplyMatrices(DatasetA, DatasetB);

string concatenatedResult = FormatMatrixAsString(resultMatrix);

string md5Hash = CalculateMD5(concatenatedResult);

bool validationResult = await ValidateMD5Hash(md5Hash);

if (validationResult)
{
    Console.WriteLine("Validation successful");
}
else
{
    Console.WriteLine("Validation failed");
}



static int[][] MultiplyMatrices(int[][] matrixA, int[][] matrixB)
{
    
    int rowsA = matrixA.Length;
    int colsA = matrixA[0].Length;
    int colsB = matrixB[0].Length;
    int[][] resultMatrix = new int[rowsA][];

    for (int i = 0; i < rowsA; i++)
    {
        resultMatrix[i] = new int[colsB];
        for (int j = 0; j < colsB; j++)
        {
            resultMatrix[i][j] = 0;
            for (int k = 0; k < colsA; k++)
            {
                resultMatrix[i][j] += matrixA[i][k] * matrixB[k][j];
            }
        }
    }

    return resultMatrix;
}

static string FormatMatrixAsString(int[][] matrix)
{
    
    StringBuilder sb = new StringBuilder();
    foreach (var row in matrix)
    {
        foreach (var element in row)
        {
            sb.Append(element);
        }
    }
    return sb.ToString();
}

static string CalculateMD5(string input)
{
    using (MD5 md5 = MD5.Create())
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("x2"));
        }
        return sb.ToString();
    }
}

static async Task<bool> ValidateMD5Hash(string md5Hash)
{
    
    using (var httpClient = new HttpClient())
    {
        var validationUrl = "https://recruitment-test.investcloud.com/api/numbers/validate";
        var content = new StringContent(md5Hash, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(validationUrl, content);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            return true;
        }

        return false; // Validation failed
    }
}

