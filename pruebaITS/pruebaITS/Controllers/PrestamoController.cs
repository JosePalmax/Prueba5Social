using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using pruebaITS.Models;

namespace pruebaITS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrestamoController : Controller
    {
        public readonly string con;

        public PrestamoController(IConfiguration configuration)
        {
            con = configuration.GetConnectionString("conexion");
        }

        [HttpGet]
        public IEnumerable<Prestamo> Get()
        {
            List<Prestamo> prestamos = new List<Prestamo>();

            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT * FROM Prestamos", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Prestamo prestamo = new Prestamo
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CopiaId = Convert.ToInt32(reader["CopiaId"]),
                                CodigoFiscal = reader["CodigoFiscal"].ToString(),
                                FechaPrestamo = Convert.ToDateTime(reader["FechaPrestamo"]),
                                FechaDevolucion = Convert.ToDateTime(reader["FechaDevolucion"])
                            };
                            prestamos.Add(prestamo);
                        }
                    }
                }
            }
            return prestamos;
        }

        [HttpGet("{id}")]
        public IActionResult GetPrestamo(int id)
        {
            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT * FROM Prestamos WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Prestamo prestamo = new Prestamo
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CopiaId = Convert.ToInt32(reader["CopiaId"]),
                                CodigoFiscal = reader["CodigoFiscal"].ToString(),
                                FechaPrestamo = Convert.ToDateTime(reader["FechaPrestamo"]),
                                FechaDevolucion = Convert.ToDateTime(reader["FechaDevolucion"])
                            };
                            return Ok(prestamo);
                        }
                        else
                        {
                            return NotFound(new { mensaje = "Préstamo no encontrado" });
                        }
                    }
                }
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] Prestamo prestamo)
        {
            prestamo.FechaPrestamo = DateTime.Now;

            prestamo.FechaDevolucion = CalcularFechas(prestamo.FechaPrestamo, 7);

            using (SqlConnection connection = new(con))
            {
                connection.Open();

                if (!IsCopiaDisponible(prestamo.CopiaId, connection))
                {
                    return BadRequest(new { mensaje = "La copia no está disponible" });
                }

                using (SqlCommand cmd = new("INSERT INTO Prestamos (CopiaId, CodigoFiscal, FechaPrestamo, FechaDevolucion) VALUES (@CopiaId, @CodigoFiscal, @FechaPrestamo, @FechaDevolucion)", connection))
                {
                    cmd.Parameters.AddWithValue("@CopiaId", prestamo.CopiaId);
                    cmd.Parameters.AddWithValue("@CodigoFiscal", prestamo.CodigoFiscal);
                    cmd.Parameters.AddWithValue("@FechaPrestamo", prestamo.FechaPrestamo);
                    cmd.Parameters.AddWithValue("@FechaDevolucion", prestamo.FechaDevolucion);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok(new { mensaje = "Préstamo realizado con éxito" });
        }

        [HttpPut("{id}")]
        public IActionResult Put([FromBody] Prestamo prestamo, int id)
        {

            prestamo.FechaDevolucion = CalcularFechas(prestamo.FechaPrestamo, 7);
            int copiaIdAnterior = 0;

            using (SqlConnection connection = new(con))
            {
                connection.Open();


                using (SqlCommand cmd = new("SELECT CopiaId FROM Prestamos WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    var result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        copiaIdAnterior = Convert.ToInt32(result);
                    }
                    else
                    {
                        return NotFound(new { mensaje = "Préstamo no encontrado para actualizar" });
                    }
                }
                if (prestamo.CopiaId != copiaIdAnterior)
                {
                    if (!IsCopiaDisponible(prestamo.CopiaId, connection))
                    {
                        return BadRequest(new { mensaje = "La copia no está disponible" });
                    }
                }
                if (copiaIdAnterior > 0)
                {
                    using (SqlCommand cmd = new("UPDATE Copias SET Disponible = 1 WHERE Id = @CopiaId", connection))
                    {
                        cmd.Parameters.AddWithValue("@CopiaId", copiaIdAnterior);
                        cmd.ExecuteNonQuery();
                    }
                }
                using (SqlCommand cmd = new("UPDATE Prestamos SET CopiaId = @CopiaId, CodigoFiscal = @CodigoFiscal, FechaPrestamo = @FechaPrestamo, FechaDevolucion = @FechaDevolucion WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@CopiaId", prestamo.CopiaId);
                    cmd.Parameters.AddWithValue("@CodigoFiscal", prestamo.CodigoFiscal);
                    cmd.Parameters.AddWithValue("@FechaPrestamo", prestamo.FechaPrestamo);
                    cmd.Parameters.AddWithValue("@FechaDevolucion", prestamo.FechaDevolucion);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { mensaje = "Préstamo no encontrado para actualizar" });
                    }
                }
            }
            return Ok(new { mensaje = "Préstamo actualizado con éxito" });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("DELETE FROM Prestamos WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { mensaje = "No se ha podido borrar el Préstamo" });
                    }
                }
            }
            return Ok(new { mensaje = "Préstamo borrado con éxito" });

        }

        [HttpGet("prestamoVencido")]
        public IEnumerable<Prestamo> ObtenerPrestamosVencidos()
        {
            List<Prestamo> prestamosVencidos = new List<Prestamo>();

            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT * FROM Prestamos WHERE FechaDevolucion < @FechaActual", connection))
                {
                    cmd.Parameters.AddWithValue("@FechaActual", DateTime.Now);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Prestamo prestamo = new Prestamo
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CopiaId = Convert.ToInt32(reader["CopiaId"]),
                                CodigoFiscal = reader["CodigoFiscal"].ToString(),
                                FechaPrestamo = Convert.ToDateTime(reader["FechaPrestamo"]),
                                FechaDevolucion = Convert.ToDateTime(reader["FechaDevolucion"])
                            };
                            prestamosVencidos.Add(prestamo);
                        }
                    }
                }
            }

            return prestamosVencidos;
        }

        [HttpGet("proximosVencimientos")]
        public IEnumerable<Prestamo> ObtenerProximosVencimientos()
        {
            List<Prestamo> proximosVencimientos = new List<Prestamo>();
            DateTime fechaActual = DateTime.Now;
            DateTime fechaLimite = CalcularFechas(fechaActual, 3);

            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT * FROM Prestamos WHERE FechaDevolucion BETWEEN @FechaActual AND @FechaLimite", connection))
                {
                    cmd.Parameters.AddWithValue("@FechaActual", fechaActual);
                    cmd.Parameters.AddWithValue("@FechaLimite", fechaLimite);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Prestamo prestamo = new Prestamo
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CopiaId = Convert.ToInt32(reader["CopiaId"]),
                                CodigoFiscal = reader["CodigoFiscal"].ToString(),
                                FechaPrestamo = Convert.ToDateTime(reader["FechaPrestamo"]),
                                FechaDevolucion = Convert.ToDateTime(reader["FechaDevolucion"])
                            };
                            proximosVencimientos.Add(prestamo);
                        }
                    }
                }
            }
            return proximosVencimientos;
        }

        private bool IsCopiaDisponible(int copiaId, SqlConnection connection)
        {
            using (SqlCommand cmd = new("SELECT Disponible FROM Copias WHERE Id = @CopiaId", connection))
            {
                cmd.Parameters.AddWithValue("@CopiaId", copiaId);

                var result = cmd.ExecuteScalar();

                if (result != null && (bool)result)
                {
                    using (SqlCommand updateCmd = new("UPDATE Copias SET Disponible = 0 WHERE Id = @CopiaId", connection))
                    {
                        updateCmd.Parameters.AddWithValue("@CopiaId", copiaId);
                        updateCmd.ExecuteNonQuery();
                    }
                    return true;
                }

                return false;
            }
        }
        private DateTime CalcularFechas(DateTime fechaPrestamo, int dias)
        {
            DateTime fechaDevolucion = fechaPrestamo.AddDays(dias);

            if (fechaDevolucion.DayOfWeek == DayOfWeek.Saturday)
            {
                fechaDevolucion = fechaDevolucion.AddDays(2);
            }
            else if (fechaDevolucion.DayOfWeek == DayOfWeek.Sunday)
            {
                fechaDevolucion = fechaDevolucion.AddDays(1);
            }

            return fechaDevolucion;
        }
    }
}
