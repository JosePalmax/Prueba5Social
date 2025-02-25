using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using pruebaITS.Models;

namespace pruebaITS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonaController : Controller
    {
        public readonly string con;

        public PersonaController(IConfiguration configuration)
        {
            con = configuration.GetConnectionString("conexion");
        }

        [HttpGet]
        public IEnumerable<Persona> Get()
        {
            List<Persona> personas = new List<Persona>();

            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT * FROM Personas", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Persona persona= new Persona
                            {
                                CodigoFiscal = reader["CodigoFiscal"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Apellido = reader["Apellido"].ToString()
                            };
                            personas.Add(persona);
                        }
                    }
                }
            }
            return personas;
        }

        [HttpGet("{id}")]
        public IActionResult GetPersona(string id)
        {
            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT * FROM Personas WHERE CodigoFiscal = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if(reader.Read())
                        {
                            Persona persona = new Persona
                            {
                                CodigoFiscal =reader["CodigoFiscal"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Apellido = reader["Apellido"].ToString()
                            };
                            return Ok(persona);
                        }
                        else
                        {
                            return NotFound(new { mensaje = "Persona no encontrada" });
                        }
                    }
                }
            }
        }

        [HttpPost]
        public void Post([FromBody] Persona persona)
        {
            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("INSERT INTO Personas (CodigoFiscal, Nombre, Apellido) VALUES (@Id, @Nombre, @Apellido)", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", persona.Nombre);
                    cmd.Parameters.AddWithValue("@Nombre", persona.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", persona.Apellido);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put([FromBody] Persona persona, string id)
        {
            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("UPDATE Personas SET Nombre = @Nombre, Apellido = @Apellido WHERE CodigoFiscal = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Nombre", persona.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", persona.Apellido);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { mensaje = "Persona no encontrada para actualizar" });
                    }
                }
            }
            return Ok(new { mensaje = "Persona actualizada con éxito" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("DELETE FROM Personas WHERE CodigoFiscal = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { mensaje = "No se ha podido borrar la Persona" });
                    }
                }
            }
            return Ok(new { mensaje = "Persona borrada con éxito" });
        }
    }

}
