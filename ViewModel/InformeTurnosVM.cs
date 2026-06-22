namespace SistemaSaludGoya.ViewModel
{
    public class InformeTurnosVM
    {
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public int TotalTurnos { get; set; }
        public int TurnosAtendidos { get; set; }
        public int TurnosCancelados { get; set; }
        public int TurnosAusentes { get; set; }
    }
}