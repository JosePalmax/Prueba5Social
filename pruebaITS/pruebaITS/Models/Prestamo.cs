namespace pruebaITS.Models
{
    public class Prestamo
    {
        public int Id { get; set; }
        public int CopiaId { get; set; }
        public string CodigoFiscal { get; set; } = string.Empty;
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaDevolucion { get; set; }
    }
}
