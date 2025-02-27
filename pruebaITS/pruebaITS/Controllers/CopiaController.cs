using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using pruebaITS.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace pruebaITS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CopiaController : Controller
    {
        private readonly string con;
        private readonly DbHelper dbHelper;

        public CopiaController(IConfiguration configuration)
        {
            con = configuration.GetConnectionString("conexion");
            dbHelper = new DbHelper(con);
        }

        [HttpGet]
        public IActionResult Get()
        {
            string query = "SELECT * FROM Copias";

            try
            {
                return Ok(dbHelper.GetDataFilters(query, dbHelper.MapCopia, 0));
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetCopia(int id)
        {
            string query = "SELECT * FROM Copias WHERE Id = @Id";
            try
            {
                var copias = dbHelper.GetDataFilters(query, dbHelper.MapCopia, id);
                if (copias.Any())
                    return Ok(copias.First());
                else
                    return NotFound(new { mensaje = "Copia no encontrada" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] Copia copia)
        {
            string queryLibros = "SELECT * FROM Libros WHERE Id = @Id";

            try
            {
                if (dbHelper.GetDataFilters(queryLibros, dbHelper.MapLibro, copia.LibroId).Any())
                {
                    using (SqlConnection connection = new(con))
                    {
                        connection.Open();
                        using (SqlCommand cmd = new("INSERT INTO Copias (LibroId, Disponible) VALUES (@LibroId, @EstaDisponible)", connection))
                        {
                            cmd.Parameters.AddWithValue("@LibroId", copia.LibroId);
                            cmd.Parameters.AddWithValue("@EstaDisponible", copia.Disponible);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                    return NotFound(new { mensaje = "No se ha encontrado el Libro que intenta referenciar" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
            return StatusCode(201, new { mensaje = "Copia agregada con éxito." });
        }

        [HttpPut("{id}")]
        public IActionResult Put([FromBody] Copia copia, int id)
        {
            string queryLibros = "SELECT * FROM Libros WHERE Id = @Id";
            try
            {
                if (dbHelper.GetDataFilters(queryLibros, dbHelper.MapLibro, copia.LibroId).Any())
                {
                    using (SqlConnection connection = new(con))
                    {
                        connection.Open();
                        using (SqlCommand cmd = new("UPDATE Copias SET LibroId = @LibroId, Disponible = @Disponible WHERE Id = @Id", connection))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.Parameters.AddWithValue("@LibroId", copia.LibroId);
                            cmd.Parameters.AddWithValue("@Disponible", copia.Disponible);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                return NotFound(new { mensaje = "No se ha podido actualizar la copia" });
                            }
                        }
                    }
                }
                else
                    return NotFound(new { mensaje = "No se ha encontrado el Libro que intenta referenciar" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
            return Ok(new { mensaje = "Copia actualizada con éxito" });

        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            string query = "SELECT * FROM Copias WHERE Id = @Id";
            try
            {
                if (dbHelper.GetDataFilters(query, dbHelper.MapCopia, id).Any())
                {
                    using (SqlConnection connection = new(con))
                    {
                        connection.Open();
                        using (SqlCommand cmd = new("DELETE FROM Copias WHERE Id = @Id", connection))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                return BadRequest(new { mensaje = "No se ha podido borrar la Copia" });
                            }
                        }
                    }
                }
                else
                    return NotFound(new { mensaje = "No se ha encontrado la copia" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
            return NoContent();
        }

        [HttpGet("getCopiasDisponibles")]
        public IActionResult GetDisponibles()
        {
            string query = "SELECT * FROM Copias WHERE Disponible = 1";
            try
            {
                return Ok(dbHelper.GetDataFilters(query, dbHelper.MapCopia, 0));
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
        }

        [HttpGet("getCopiasNoDisponibles")]
        public IActionResult GetNoDisponibles()
        {
            string query = "SELECT * FROM Copias WHERE Disponible = 0";
            try
            {
                return Ok(dbHelper.GetDataFilters(query, dbHelper.MapCopia, 0));
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
        }
    }
}