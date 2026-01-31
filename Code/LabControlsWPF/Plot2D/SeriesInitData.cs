using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;

namespace LabControlsWPF.Plot2D
{
    /// <summary>
    /// Klasa identyfikatora seri danych wykresu
    /// </summary>
    public class SeriesInitData
    {
        /// <summary>Numer identyfikujący serię danych</summary>
        public int ID { get; private set; }
        /// <summary>Tytuł serii danych</summary>
        public string Title { get; private set; }
        /// <summary>Kolor seri danych</summary>
        public OxyColor Color { get; private set; }
        public SeriesType SeriesType { get; private set; }

        public SeriesInitData(int id, string title, OxyColor color, SeriesType seriesType)
        {
            ID = id;
            Title = title;
            Color = color;
            SeriesType = seriesType;
        }
    }

    public enum SeriesType
    {
        Line,
        Scatter
    }
}
