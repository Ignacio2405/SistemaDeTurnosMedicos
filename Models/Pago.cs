namespace SistemaSaludGoya.Models;

public class Pago
{
    public int IdPago { get; set; }

    public int IdTurno { get; set; }

    public decimal Monto { get; set; }

    public DateTime FechaPago { get; set; }

    public string MetodoPago { get; set; } = null!;

    public bool Pagado { get; set; }

    public Turno Turno { get; set; } = null!;
}
