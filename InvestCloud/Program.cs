using InvestCloud;
using Newtonsoft.Json;
using System.Data;
using System.Security.Cryptography;
using System.Text;

string baseURL = "https://recruitment-test.investcloud.com/";
string retrieveRowURL = "api/numbers/";

int[][] DatasetA = new int[1000][];
int[][] DatasetB = new int[1000][];

var AURLs = new List<string>();
var BURLs = new List<string>();
var ObjA = new List<DatasetClass>();
var ObjB = new List<DatasetClass>();

bool process = true;
int timer = 0;

var task = Task.Run(() =>
{
    while (process)
    {
        timer += 1;
        Thread.Sleep(1000);
    }
});

Console.WriteLine("Generating required URLs...");

for (var initIndex = 0; initIndex < 1000; initIndex++)
{
    var ARowURL = baseURL + retrieveRowURL + "A/row/" + initIndex;
    AURLs.Add(ARowURL);
    var BRowURL = baseURL + retrieveRowURL + "B/row/" + initIndex;
    BURLs.Add(BRowURL);
}

Console.WriteLine("Initializing 1000 x 1000 datasets");
InitDatasets();

Console.WriteLine("Retrieving rows for Dataset A and mapping into local object list...");

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
        ObjA.Add(rows);

    }


}

Console.WriteLine("Retrieving rows for Dataset B and mapping into local object list...");

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
        ObjB.Add(rows);


    }


}
Console.WriteLine("Creating 2D array object from Lists...");

DatasetA = new int[ObjA[0].Value.Count][];
DatasetB = new int[ObjA[0].Value.Count][];
for (int i = 0; i < ObjA[0].Value.Count; i++)
{
    DatasetA[i] = new int[ObjA[0].Value.Count];
    DatasetB[i] = new int[ObjA[0].Value.Count];
    for (int j = 0; j < ObjA[0].Value.Count; j++)
    {
        DatasetA[i][j] = int.Parse(ObjA[i].Value[j].ToString());
        DatasetB[i][j] = int.Parse(ObjB[i].Value[j].ToString());

    }
}


Console.WriteLine("Multiplying Datasets...");

var resultMatrix = MultiplyMatrices(DatasetA, DatasetB);

Console.WriteLine("Formatting Dataset as string...");

string concatenatedResult = FormatMatrixAsString(resultMatrix);


Console.WriteLine("Calculating MD5...");

string md5Hash = CalculateMD5(concatenatedResult);

Console.WriteLine("Validating generated MD5 hash...");

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

float mins = timer / 60;
var sec = "Total elapsed time: " + timer + " secs (" + mins.ToString("0.00") + " mins)";
Console.WriteLine(sec);
Console.WriteLine("Task completed...thank you for your patience!");

//Start of defining computation functions
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

//End of defining computation functions

//Start of defining http call functions
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
static void InitDatasets()
{

    using (var client = new HttpClient())
    {
        var url = "https://recruitment-test.investcloud.com/api/numbers/init/1000";
        client.GetAsync(url);

    };
}
//End of defining validation functions
