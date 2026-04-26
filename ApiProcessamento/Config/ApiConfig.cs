namespace ApiProcessamento.Config
{
    /// <summary>
    /// Classe de configuração da Api, 
    /// contendo a propriedade MaxTemperatura, que é lida do arquivo 
    /// appsettings.json e utilizada para validar os dados recebidos pela API.
    /// </summary>
    public class ApiConfig
    {
        public double MaxTemperatura { get; set; }
        public int MaxAngulo { get; set; }
    };
}
