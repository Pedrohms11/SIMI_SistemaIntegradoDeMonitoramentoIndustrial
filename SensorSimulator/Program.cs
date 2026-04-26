using Shared;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
/// <summary>
/// Programa principal do simulador de sensores que envia dados de temperatura e ângulo para a API.
/// </summary>
var http = new HttpClient();
int index = 0;

while (true)
{
    var sensor = new SensorData
    {
        Id = index,
        Temperatura = new Random().Next(20, 100),
        Angulo = new Random().Next(0,360),
        Timestamp = DateTime.Now
    };

    var response = await http.PostAsJsonAsync("https://localhost:7157/api/v1/sensores", sensor);

    if (!response.IsSuccessStatusCode)
    {
        var erro = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Erro: {response.StatusCode} - {erro}");
    }
    else
    {
        Console.WriteLine($"Enviado: {sensor.Temperatura}");
        Console.WriteLine($"Enviado Angulo: {sensor.Angulo}");
    }

    await Task.Delay(2000);
    index++;
}