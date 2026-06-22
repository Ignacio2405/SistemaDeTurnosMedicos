namespace SistemaSaludGoya.ViewModel;

public class GuardarHorariosDTO
{
    public List<HorarioDiaVM> HorariosDefecto { get; set; } = new();
    public List<HorarioExcepcionVM> Excepciones { get; set; } = new();
}