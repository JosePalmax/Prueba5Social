using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using pruebaITS.Models;

namespace pruebaITS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CopiaController : Controller
    {
        public readonly string con;

        public CopiaController(IConfiguration configuration)
        {
            con = configuration.GetConnectionString("conexion");
        }

        [HttpGet]
        public IEnumerable<Copia> Get()
        {
            List<Copia> copias = new List<Copia>();

            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT * FROM Copias", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Copia copia = new Copia
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LibroId = Convert.ToInt32(reader["LibroId"]),
                                Disponible = Convert.ToBoolean(reader["Disponible"])
                            };
                            copias.Add(copia);
                        }
                    }
                }
            }
            return copias;
        }

        [HttpGet("{id}")]
        public IActionResult GetCopia(int id)
        {
            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT * FROM Copias WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Copia copia = new Copia
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LibroId = Convert.ToInt32(reader["LibroId"]),
                                Disponible = Convert.ToBoolean(reader["Disponible"])
                            };
                            return Ok(copia);
                        }
                        else
                        {
                            return NotFound(new { mensaje = "Copia no encontrada" });
                        }
                    }
                }
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] Copia copia)
        {
            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT COUNT(*) FROM Libros WHERE Id = @LibroId", connection))
                {
                    cmd.Parameters.AddWithValue("@LibroId", copia.LibroId);

                    int libroCount = (int)cmd.ExecuteScalar();

                    if (libroCount == 0)
                    {
                        return NotFound(new { mensaje = "Libro no encontrado." });
                    }
                }

                using (SqlCommand cmd = new("INSERT INTO Copias (LibroId, Disponible) VALUES (@LibroId, @EstaDisponible)", connection))
                {
                    cmd.Parameters.AddWithValue("@LibroId", copia.LibroId);
                    cmd.Parameters.AddWithValue("@EstaDisponible", copia.Disponible);

                    cmd.ExecuteNonQuery();
                }
            }

            return Ok(new { mensaje = "Copia agregada con éxito." });
        }

        [HttpPut("{id}")]
        public IActionResult Put([FromBody] Copia copia, int id)
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
            return Ok(new { mensaje = "Copia actualizada con éxito" });

        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
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
                        return NotFound(new { mensaje = "No se ha podido borrar la Copia" });
                    }
                }
            }
            return Ok(new { mensaje = "Copia borrada con éxito" });
        }

        [HttpGet("getCopiasDisponibles")]
        public IEnumerable<Copia> GetDisponibles()
        {
            List<Copia> copias = new List<Copia>();

            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT * FROM Copias WHERE Disponible = 1", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Copia copia = new Copia
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LibroId = Convert.ToInt32(reader["LibroId"]),
                                Disponible = Convert.ToBoolean(reader["Disponible"])
                            };
                            copias.Add(copia);
                        }
                    }
                }
            }
            return copias;
        }

        [HttpGet("getCopiasNoDisponibles")]
        public IEnumerable<Copia> GetNoDisponibles()
        {
            List<Copia> copias = new List<Copia>();

            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT * FROM Copias WHERE Disponible = 0", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Copia copia = new Copia
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LibroId = Convert.ToInt32(reader["LibroId"]),
                                Disponible = Convert.ToBoolean(reader["Disponible"])
                            };
                            copias.Add(copia);
                        }
                    }
                }
            }
            return copias;
        }
    }
}
