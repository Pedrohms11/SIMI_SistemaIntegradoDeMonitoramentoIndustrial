using SensorInterface.Command;
using Shared;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Input;

namespace SensorInterface.ViewModel
{
    /// <summary>
    /// Main ViewModel responsável por gerenciar os dados de sensores e comandos da interface
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<double> Temperaturas { get; set; }
        public ObservableCollection<int> Angulos { get; set; }

        public ICommand CarregarSensoresCommand { get; }

        /// <summary>
        /// MainViewModel construtor que inicializa as coleções de temperaturas e ângulos,
        /// e configura o comando para carregar os dados dos sensores a partir da API.
        /// </summary>
        public MainViewModel()
        {
            Temperaturas = new ObservableCollection<double>();
            Angulos = new ObservableCollection<int>();
            CarregarSensoresCommand = new RelayCommand(CarregarSensores);
        }
        /// <summary>
        ///  CarregarSensores é um método assíncrono que faz uma requisição HTTP GET para a API de sensores,
        /// </summary>
        private async void CarregarSensores()
        {
            var http = new HttpClient();
            var dados = await http.GetFromJsonAsync<List<SensorData>>(
                "https://localhost:7157/api/v1/sensores");

            Temperaturas.Clear();
            
            foreach (var temp in dados)
            {
                Temperaturas.Add(temp.Temperatura);
            }

            Angulos.Clear();
            foreach (var Ang in dados)
            {
                Angulos.Add(Ang.Angulo);
                
            }
        }
    }
}

