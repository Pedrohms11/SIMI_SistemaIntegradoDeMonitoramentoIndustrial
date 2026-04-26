using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// SensorData é uma classe de modelo que representa os dados coletados por um sensor, 
    /// incluindo temperatura, ângulo e timestamp.
    /// </summary>
    public class SensorData
    {
        public int Id { get; set; }
        public double Temperatura { get; set; }
        public int Angulo { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
        