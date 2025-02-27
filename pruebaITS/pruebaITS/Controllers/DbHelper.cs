using Microsoft.Data.SqlClient;
using pruebaITS.Models;

namespace pruebaITS.Controllers
{
    public class DbHelper
    {
        private readonly string con;

        private const string IdColumnnName = "Id";
        //COPIA
        private const string CopiaLibroIdColumnnName = "LibroId";
        private const string CopiaDisponibleColumnnName = "Disponible";
        //LIBRO
        private const string LibroTituloColumnnName = "Titulo";
        private const string LibroAutorColumnnName = "Autor";
        private const string LibroGeneroColumnnName = "Genero";
        //PRESTAMO
        private const string PrestamoCopiaIdColumnnName = "CopiaId";
        private const string PrestamoCodigoFiscalColumnnName = "CodigoFiscal";
        private const string PrestamoFechaPrestamoColumnnName = "FechaPrestamo";
        private const string PrestamoFechaDevolucionColumnnName = "FechaDevolucion";
        public DbHelper(string connectionString)
        {
            con = connectionString;
        }

        public IEnumerable<T> GetDataFilters<T>(string query, Func<SqlDataReader, T> mapper, int id = 0)
        {
            List<T> results = new List<T>();

            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new(query, connection))
                {
                    if (id != 0)
                        cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(mapper(reader));
                        }
                    }
                }
            }
            return results;
        }

        public Libro MapLibro(SqlDataReader reader)
        {
            return new Libro
            {
                Id = reader.GetInt32(reader.GetOrdinal(IdColumnnName)),
                Titulo = reader.GetString(reader.GetOrdinal(LibroTituloColumnnName)),
                Autor = reader.GetString(reader.GetOrdinal(LibroAutorColumnnName)),
                Genero = reader.GetString(reader.GetOrdinal(LibroGeneroColumnnName))
            };
        }
        public Copia MapCopia(SqlDataReader reader)
        {
            return new Copia
            {
                Id = reader.GetInt32(reader.GetOrdinal(IdColumnnName)),
                LibroId = reader.GetInt32(reader.GetOrdinal(CopiaLibroIdColumnnName)),
                Disponible = reader.GetBoolean(reader.GetOrdinal(CopiaDisponibleColumnnName))
            };
        }
        public Prestamo MapPrestamo(SqlDataReader reader)
        {
            return new Prestamo
            {
                Id = reader.GetInt32(reader.GetOrdinal(IdColumnnName)),
                CopiaId = reader.GetInt32(reader.GetOrdinal(PrestamoCopiaIdColumnnName)),
                CodigoFiscal = reader.GetString(reader.GetOrdinal(PrestamoCodigoFiscalColumnnName)),
                FechaPrestamo = reader.GetDateTime(reader.GetOrdinal(PrestamoFechaPrestamoColumnnName)),
                FechaDevolucion = reader.GetDateTime(reader.GetOrdinal(PrestamoFechaDevolucionColumnnName))
            };
        }
    }
}
