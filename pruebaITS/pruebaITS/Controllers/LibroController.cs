using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using pruebaITS.Models;

namespace pruebaITS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibroController : Controller
    {
        private readonly string con;
        private readonly DbHelper dbHelper;

        public LibroController(IConfiguration configuration)
        {
            con = configuration.GetConnectionString("conexion");
            dbHelper = new DbHelper(con);
        }

        [HttpGet]
        public IActionResult Get()
        {
            string query = "SELECT * FROM Libros";
            try
            {
                return Ok(dbHelper.GetDataFilters(query, dbHelper.MapLibro, 0));
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetLibro(int id)
        {
            string query = "SELECT * FROM Libros WHERE Id = @Id";
            try
            {
                var libros = dbHelper.GetDataFilters(query, dbHelper.MapLibro, id);
                if (libros.Any())
                    return Ok(libros.First());
                else
                    return NotFound(new { mensaje = "Libro no encontrado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] Libro libro)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
            return StatusCode(201, new { mensaje = "Copia agregada con éxito." });
        }

        [HttpPut("{id}")]
        public IActionResult Put([FromBody] Libro libro, int id)
        {
            try
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
                            return NotFound(new { mensaje = "No se ha podido actualizar el Libro" });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
            return Ok(new { mensaje = "Copia editada con éxito." });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            string query = "SELECT * FROM Libros WHERE Id = @Id"; try
            {
                using (SqlConnection connection = new(con))
                {
                    connection.Open();
                    if (dbHelper.GetDataFilters(query, dbHelper.MapLibro, id).Any())
                    {
                        using (SqlCommand cmd = new("DELETE FROM Libros WHERE Id = @Id", connection))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                                return BadRequest(new { mensaje = "No se ha podido borrar el Libro" });
                        }
                    }
                    else
                        return NotFound(new { mensaje = "No se encontrado el Libro a borrar" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
            return NoContent();
        }
    }
}
