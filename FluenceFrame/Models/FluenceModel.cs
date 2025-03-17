using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FluenceFrame.Models
{
    public class FluenceModel
    {
        public float[,] Fluence { get; set; }
        public Point Origin { get; set; }
        public FluenceModel()
        {

        }
    }
}
