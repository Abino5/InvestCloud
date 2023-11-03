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
string retrieveRowURL = "api/numbers/";

int[][] DatasetA  = new int[10][];
int[][] DatasetB= new int[10][];

var AURLs = new List<string>();
var BURLs = new List<string>();
bool process = true;
var task = Task.Run(() => {
    int x = 0;
    while (process)
    {
        x += 1;
        var sec = x < 2 ? " Sec" : " Secs";
        Console.WriteLine("Elapsed : "+x + sec);
        Thread.Sleep(1000);
    }
});

for (var initIndex = 0; initIndex < 1000; initIndex++)
{
    var ARowURL = baseURL + retrieveRowURL + "A/row/" + initIndex;
    AURLs.Add(ARowURL);
    var BRowURL = baseURL + retrieveRowURL + "B/row/" + initIndex;
    BURLs.Add(BRowURL);
}


    using (var client = new HttpClient())
    {
        var requests = BURLs.Select
            (
                url => client.GetAsync(url)
            ).ToList();
        await Task.WhenAll(requests);

        var responses = requests.Select
            (
                task => task.Result
            );

        foreach (var r in responses)
        {
            var rowsX = await r.Content.ReadAsStringAsync();
            var rows = JsonConvert.DeserializeObject<DatasetClass>(rowsX);

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


    using (var client = new HttpClient())
    {
        //Start requests for all of them
        var requests = BURLs.Select
            (
                url => client.GetAsync(url)
            ).ToList();
        //Wait for all the requests to finish
        await Task.WhenAll(requests);

        //Get the responses
        var responses = requests.Select
            (
                task => task.Result
            );

        foreach (var r in responses)
        {
            // Extract the message body
            var rowsX = await r.Content.ReadAsStringAsync();
            var rows = JsonConvert.DeserializeObject<DatasetClass>(rowsX);

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
        Console.WriteLine("Validation Successful");
    }
    else
    {
        Console.WriteLine("Validation Failed");
    }
process = false;


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

