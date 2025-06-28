namespace Lab12.Models
{
    public class Grade
    {
        public int GradeId { get; set; }
        public string Nombre { get; set; }
        public int Descripcion { get; set; }
        public bool Activo { get; set; } = false;
    }
}
