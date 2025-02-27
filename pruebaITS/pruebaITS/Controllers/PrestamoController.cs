using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using pruebaITS.Models;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace pruebaITS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrestamoController : Controller
    {
        private readonly string con;
        private readonly DbHelper dbHelper;

        public PrestamoController(IConfiguration configuration)
        {
            con = configuration.GetConnectionString("conexion");
            dbHelper = new DbHelper(con);
        }

        [HttpGet]
        public IActionResult Get()
        {
            string query = "SELECT * FROM Prestamos";
            try
            {
                return Ok(dbHelper.GetDataFilters(query, dbHelper.MapPrestamo, 0));
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }

        }

        [HttpGet("{id}")]
        public IActionResult GetPrestamo(int id)
        {
            string query = "SELECT * FROM Prestamos WHERE Id = @Id";
            try
            {
                var prestamos = dbHelper.GetDataFilters(query, dbHelper.MapPrestamo, id);
                if (prestamos.Any())
                    return Ok(prestamos.First());
                else
                    return NotFound(new { mensaje = "Prestamo no encontrado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] Prestamo prestamo)
        {
            prestamo.FechaPrestamo = DateTime.Now;

            prestamo.FechaDevolucion = CalcularFechas(prestamo.FechaPrestamo, 7);
            try
            {
                using (SqlConnection connection = new(con))
                {
                    connection.Open();

                    if (!IsCopiaDisponible(prestamo.CopiaId))
                    {
                        return BadRequest(new { mensaje = "La copia no está disponible" });
                    }

                    using (SqlCommand cmd = new("INSERT INTO Prestamos (CopiaId, CodigoFiscal, FechaPrestamo, FechaDevolucion)" +
                        " VALUES (@CopiaId, @CodigoFiscal, @FechaPrestamo, @FechaDevolucion)", connection))
                    {

                        cmd.Parameters.AddWithValue("@CopiaId", prestamo.CopiaId);
                        cmd.Parameters.AddWithValue("@CodigoFiscal", prestamo.CodigoFiscal);
                        cmd.Parameters.AddWithValue("@FechaPrestamo", prestamo.FechaPrestamo);
                        cmd.Parameters.AddWithValue("@FechaDevolucion", prestamo.FechaDevolucion);
                        cmd.ExecuteNonQuery();
                    }

                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }
            return StatusCode(201, new { mensaje = "Préstamo realizado con éxito" });
        }

        [HttpPut("{id}")]
        public IActionResult Put([FromBody] Prestamo prestamo, int id)
        {

            prestamo.FechaDevolucion = CalcularFechas(prestamo.FechaPrestamo, 7);
            string queryCopia = "SELECT * FROM Prestamos WHERE Id = @Id";
            try
            {
                var copiaIdAnterior = dbHelper.GetDataFilters(queryCopia, dbHelper.MapPrestamo, id).FirstOrDefault();

                if (copiaIdAnterior != null)
                {
                    if (prestamo.CopiaId != copiaIdAnterior.CopiaId)
                    {
                        if (!IsCopiaDisponible(prestamo.CopiaId))
                        {
                            return BadRequest(new { mensaje = "La copia no está disponible" });
                        }
                    }
                    if (prestamo.CopiaId > 0)
                    {
                        using (SqlConnection connection = new(con))
                        {
                            connection.Open();
                            using (var transaction = connection.BeginTransaction())
                            {
                                try
                                {
                                    if (prestamo.CopiaId != copiaIdAnterior.CopiaId)
                                    {
                                        using (SqlCommand cmd = new("UPDATE Copias SET Disponible = 1 WHERE Id = @CopiaId", connection, transaction))
                                        {
                                            cmd.Parameters.AddWithValue("@CopiaId", copiaIdAnterior.CopiaId);
                                            cmd.ExecuteNonQuery();
                                        }
                                        using (SqlCommand cmd = new("UPDATE Copias SET Disponible = 0 WHERE Id = @CopiaId", connection, transaction))
                                        {
                                            cmd.Parameters.AddWithValue("@CopiaId", prestamo.CopiaId);
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    using (SqlCommand cmd = new("UPDATE Prestamos SET CopiaId = @CopiaId, CodigoFiscal = @CodigoFiscal," +
                                        " FechaPrestamo = @FechaPrestamo, FechaDevolucion = @FechaDevolucion WHERE Id = @Id", connection, transaction))
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
                                    transaction.Commit();
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
                                }
                            }
                        }
                        return Ok(new { mensaje = "Prestamo actualizado con éxito" });
                    }
                }
                else
                    return NotFound(new { mensaje = "Préstamo no encontrado para actualizar" });
            }
            catch (Exception e)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = e.Message });
            }

            return Ok(new { mensaje = "Préstamo actualizado con éxito" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            string query = "SELECT * FROM Prestamos WHERE Id = @Id";
            string queryCopia = "SELECT * FROM Copias WHERE Id = @Id";
            try
            {
                var prestamoDel = dbHelper.GetDataFilters(query, dbHelper.MapPrestamo, id);
                if (prestamoDel.Any())
                {
                    var copiaLib = dbHelper.GetDataFilters(queryCopia, dbHelper.MapCopia, prestamoDel.FirstOrDefault().CopiaId);

                    using (SqlConnection connection = new(con))
                    {
                        connection.Open();
                        if (copiaLib.Any())
                        {
                            using (SqlCommand cmd = new("UPDATE Copias SET Disponible = 1 WHERE Id = @CopiaId", connection))
                            {
                                cmd.Parameters.AddWithValue("@CopiaId", copiaLib.First().Id);
                                cmd.ExecuteNonQuery();
                            }
                        }
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
                }
                else
                    return NotFound(new { mensaje = "No se ha encontrado el prestamo" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
            }

            return NoContent();
        }

        [HttpGet("prestamoVencido")]
        public IActionResult ObtenerPrestamosVencidos()
        {
            List<Prestamo> prestamosVencidos = new List<Prestamo>();

            using (SqlConnection connection = new(con))
            {
                connection.Open();
                using (SqlCommand cmd = new("SELECT * FROM Prestamos WHERE FechaDevolucion < @FechaActual", connection))
                {
                    cmd.Parameters.AddWithValue("@FechaActual", DateTime.Now);
                    try
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Prestamo prestamo = dbHelper.MapPrestamo(reader);
                                prestamosVencidos.Add(prestamo);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message }); ;
                    }

                }
            }
            return Ok(prestamosVencidos);
        }

        [HttpGet("proximosVencimientos")]
        public IActionResult ObtenerProximosVencimientos()
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
                    try
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Prestamo prestamo = dbHelper.MapPrestamo(reader);
                                proximosVencimientos.Add(prestamo);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { mensaje = "Error al realizar la operación", error = ex.Message });
                    }
                }
            }
            return Ok(proximosVencimientos);
        }

        private bool IsCopiaDisponible(int copiaId)
        {
            string query = "SELECT * FROM Copias WHERE Id = @Id";
            try
            {
                var copia = dbHelper.GetDataFilters(query, dbHelper.MapCopia, copiaId);
                if (copia.Any())
                    return copia.First().Disponible;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private DateTime CalcularFechas(DateTime fechaPrestamo, int dias)
        {
            DateTime fechaDevolucion = fechaPrestamo;

            while (dias > 0)
            {
                fechaDevolucion = fechaDevolucion.AddDays(1);
                if (fechaDevolucion.DayOfWeek != DayOfWeek.Saturday && fechaDevolucion.DayOfWeek != DayOfWeek.Sunday)
                    dias--;
            }

            return fechaDevolucion;
        }
    }
}
