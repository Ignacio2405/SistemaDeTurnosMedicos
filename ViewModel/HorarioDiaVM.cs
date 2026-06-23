using SistemaSaludGoya.ViewModel;

public class HorarioDiaVM
{
    public DayOfWeek DiaSemana { get; set; }
    public string NombreDia { get; set; } = "";
    public bool Activo { get; set; }

    public List<HorarioBloqueVM> Bloques { get; set; } = new();
}