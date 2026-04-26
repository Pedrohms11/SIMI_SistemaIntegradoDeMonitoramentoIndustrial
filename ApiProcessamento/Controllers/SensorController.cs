using ApiProcessamento.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared;
using System.Security.Cryptography.X509Certificates;

namespace ApiProcessamento.Controllers
{
    /// <summary>
    /// Controller responsável pelo gerenciamento de leituras de sensores de temperatura e ângulo
    /// </summary>
    [ApiController]
    [Route("api/v1/sensores")]
    [Produces("application/json")]
    public class SensorController : ControllerBase
    {
        private static readonly List<SensorData> dados = new();
        private readonly ApiConfig _apiConfig;

        /// <summary>
        /// Construtor do controller de sensores com injeção de configuração
        /// </summary>
        /// <param name="apiConfig">Configurações da API (limites de temperatura e ângulo)</param>
        public SensorController(IOptions<ApiConfig> apiConfig)
        {
            _apiConfig = apiConfig.Value;
        }

        /// <summary>
        /// Endpoint para receber e validar dados de leitura de sensores
        /// </summary>
        /// <param name="sensor">Objeto contendo os dados do sensor (temperatura, ângulo, timestamp, etc)</param>
        /// <returns>Retorna status 200 se os dados são válidos, ou BadRequest/StatusCode com mensagem de erro</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Receber([FromBody] SensorData sensor)
        {
            // 1. Validação de modelo nulo
            if (sensor == null)
            {
                return BadRequest(new { mensagem = "Dados do sensor não foram fornecidos", erro = "Solicitação inválida" });
            }

            try
            {
                // 2. Validação de timestamp
                if (sensor.Timestamp == default(DateTime))
                {
                    sensor.Timestamp = DateTime.UtcNow;
                }

                // 3. Validação para datas futuras
                if (sensor.Timestamp > DateTime.UtcNow.AddMinutes(5))
                {
                    return BadRequest(new { mensagem = "Data/Hora não pode ser futura", campo = "Timestamp", valor = sensor.Timestamp });
                }

                // 4. Validação para datas muito antigas
                if (sensor.Timestamp < DateTime.UtcNow.AddYears(-1))
                {
                    return BadRequest(new { mensagem = "Data/Hora muito antiga (máximo 1 ano atrás)", campo = "Timestamp", valor = sensor.Timestamp });
                }

                // 5. Validação de temperatura usando configuração do appsettings.json
                if (sensor.Temperatura > _apiConfig.MaxTemperatura)
                {
                    return BadRequest(new
                    {
                        mensagem = $"Temperatura acima do limite máximo permitido",
                        campo = "Temperatura",
                        valor = sensor.Temperatura,
                        limiteMaximo = _apiConfig.MaxTemperatura
                    });
                }

                // 6. Validação de ângulo usando configuração do appsettings.json
                if (sensor.Angulo > _apiConfig.MaxAngulo)
                {
                    return BadRequest(new
                    {
                        mensagem = $"Ângulo acima do limite máximo permitido",
                        campo = "Angulo",
                        valor = sensor.Angulo,
                        limiteMaximo = _apiConfig.MaxAngulo
                    });
                }

                // Validação opcional: limites mínimos (adicione ao ApiConfig se necessário)
                if (sensor.Temperatura < -50)
                {
                    return BadRequest(new
                    {
                        mensagem = "Temperatura abaixo do limite mínimo físico (-50°C)",
                        campo = "Temperatura",
                        valor = sensor.Temperatura,
                        limiteMinimo = -50
                    });
                }

                if (sensor.Angulo < 0)
                {
                    return BadRequest(new
                    {
                        mensagem = "Ângulo abaixo do limite mínimo físico (0 graus)",
                        campo = "Angulo",
                        valor = sensor.Angulo,
                        limiteMinimo = 0
                    });
                }

                // Dados válidos - adiciona à lista
                dados.Add(sensor);

                return Ok(new
                {
                    mensagem = "Dados recebidos com sucesso",
                    timestamp = sensor.Timestamp,
                    totalRegistros = dados.Count,
                    limitesAplicados = new
                    {
                        temperaturaMax = _apiConfig.MaxTemperatura,
                        anguloMax = _apiConfig.MaxAngulo
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro interno no processamento dos dados",
                    erro = ex.Message,
                    detalhe = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Endpoint para listar todos os dados de sensores com opção de filtro por período
        /// </summary>
        /// <param name="dataInicio">Data inicial para filtro (opcional)</param>
        /// <param name="dataFim">Data final para filtro (opcional)</param>
        /// <returns>Lista de leituras de sensores dentro do período especificado</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Listar([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
        {
            try
            {
                var query = dados.AsEnumerable();

                if (dataInicio.HasValue)
                {
                    query = query.Where(d => d.Timestamp >= dataInicio.Value);
                }

                if (dataFim.HasValue)
                {
                    query = query.Where(d => d.Timestamp <= dataFim.Value);
                }

                var resultado = query.OrderByDescending(d => d.Timestamp).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao listar dados", erro = ex.Message });
            }
        }
    }
}




