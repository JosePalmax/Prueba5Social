using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using pruebaITS.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace pruebaITS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibroController : Controller
    {
        public readonly string con;

        public LibroController(IConfiguration configuration)
        {
            con = configuration.GetConnectionString("conexion");
        }

        [HttpGet]
        public IEnumerable<Libro> Get()
        {
            List<Libro> libros = new List<Libro>();

            using (SqlConnection connection = new (con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT * FROM Libros", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Libro libro = new Libro { 
                                Id = Convert.ToInt32(reader["Id"]),
                                Titulo = reader["Titulo"].ToString(),
                                Autor = reader["Autor"].ToString(),
                                Genero = reader["Genero"].ToString(),
                            };
                            libros.Add(libro);
                        }
                    }
                }
            }
            return libros;
        }

        [HttpGet("{id}")]
        public IActionResult GetLibro(int id)
        {
            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT * FROM Libros WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Libro libro = new Libro
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Titulo = reader["Titulo"].ToString(),
                                Autor = reader["Autor"].ToString(),
                                Genero = reader["Genero"].ToString(),
                            };
                            return Ok(libro);
                        }
                        else
                        {
                            return NotFound(new { mensaje = "Libro no encontrado" });
                        }
                    }
                }
            }
        }

        [HttpPost]
        public void Post([FromBody] Libro libro)
        {
            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("INSERT INTO Libros (Titulo, Autor, Genero) VALUES (@Titulo, @Autor, @Genero)", connection))
                {
                    cmd.Parameters.AddWithValue("@Titulo", libro.Titulo);
                    cmd.Parameters.AddWithValue("@Autor", libro.Autor);
                    cmd.Parameters.AddWithValue("@Genero", libro.Genero);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put([FromBody] Libro libro, int id)
        {
            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("UPDATE Libros SET Titulo = @Titulo, Autor = @Autor, Genero = @Genero WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Titulo", libro.Titulo);
                    cmd.Parameters.AddWithValue("@Autor", libro.Autor);
                    cmd.Parameters.AddWithValue("@Genero", libro.Genero);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { mensaje = "No se ha podido actualizar el Libro" });
                    }
                }
            }
            return Ok(new { mensaje = "Libro actualizado con éxito" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("DELETE FROM Libros WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { mensaje = "No se ha podido borrar el Libro" });
                    }
                }
            }
            return Ok(new { mensaje = "Libro borrado con éxito" });
        }

    }
}
