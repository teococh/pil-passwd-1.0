using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using pil_passwd_1._0.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;


namespace pil_passwd_1._0.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        [Authorize]
        public IActionResult Secured()
        {

            return View();
        }
        [HttpGet("login")]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost("login")]
        public async Task<IActionResult> Validate(string username, string password, string returnUrl)
        {

            string loginData = GetData("SELECT user, password FROM [dbo].[login] WHERE [user]='" + username + "' AND [password]='" + password + "'");
            if (loginData != "")
            {
                ViewData["ReturnUrl"] = returnUrl;
                List<Claim> claims = new()
                {
                    new Claim("username", username),
                    new Claim(ClaimTypes.NameIdentifier, username)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Secured", "Home");
                }
            }
            TempData["Error"] = "Usuario o contraseña no validos";

            return View("Login");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("Secured");
        }

        // Conexión con la base de datos
        [HttpPost]
        public string[] DbConection()
        {

            string[] sql = { "SELECT * FROM dbo.passwords FOR JSON AUTO;", "SELECT * FROM dbo.Clientes FOR JSON AUTO;", "SELECT * FROM dbo.Plataformas FOR JSON AUTO;" };
            string[] res = new string[3];
            for (int i = 0; i < 3; i++)
            {
                res[i] = GetData(sql[i]);
            }

            return res;
        }

        [HttpPost]
        public bool AñadirCliente(string nombre)
        {
            string sql = "insert into dbo.Clientes(Nombre) values ('" + nombre + "')";
            return DbPost(sql);
        }

        [HttpPost]
        public bool AñadirPlataforma(string nombre)
        {
            string sql = "insert into dbo.Plataformas(Nombre) values ('" + nombre + "')";
            return DbPost(sql);
        }

        [HttpPost]
        public bool AñadirPassword(int idCliente, int idPlataforma, string usuario, string passwd, string url1, string comentario)
        {
            string sql = "insert into dbo.Passwords(idPlataforma, idCliente, usuario, contraseña, url, comentario) values (" + idPlataforma + "," + idCliente + ", '" + usuario + "', '" + passwd + "', '" + url1 + "', '" + comentario + "')";
            return DbPost(sql);
        }
        [HttpPost]
        public bool EditarPassword(int idCliente, int idPlataforma, string usuario, string passwd, string url1, string comentario)
        {
            string sql = "UPDATE [dbo].[Passwords] SET [idPlataforma] = "+idPlataforma+", [usuario] = '"+usuario+"', [contraseña] = '"+passwd+"', [comentario] = '"+comentario+"', [url] = '"+url1+"' WHERE Id = '"+idCliente+"'";
            return DbPost(sql);
        }

        [HttpPost]
        public bool EliminarPass(int id)
        {
            string sql = "DELETE FROM [dbo].[Passwords] WHERE Id = "+id;
            return DbPost(sql);
        }

        [HttpPost]
        public string Buscar(string x)
        {
            
            string sql = "SELECT * FROM dbo.Passwords WHERE(usuario like '%" + x + "%') OR(contraseña like '%" + x + "%') OR(url like '%" + x + "%') OR(comentario like '%" + x + "%') FOR JSON AUTO";
;
            string res = GetData(sql);
            Console.Write(res);
            return res;
        }





        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }







        /* ------------------------ Conexiones con la base de datos --------------------------- */


        private static string GetData(string sql)
        {
            string respuesta = "";
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "localhost,1433";
                builder.UserID = "sa";
                builder.Password = "12#adios";
                builder.InitialCatalog = "master";


                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                respuesta = (reader.GetString(0));
                            }

                        }
                    }
                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            return respuesta;


        }



        public bool DbPost(string sql)
        {

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "localhost,1433";
                builder.UserID = "sa";
                builder.Password = "12#adios";
                builder.InitialCatalog = "master";


                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sql, connection);
                    _ = command.ExecuteNonQuery();
                    connection.Close();
                }
                return true;
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }


        private bool DbLogin(string sql)
        {
            string respuesta = "";
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "localhost,1433";
                builder.UserID = "sa";
                builder.Password = "12#adios";
                builder.InitialCatalog = "master";
                


                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                respuesta += reader.GetString(0);
                            }

                        }
                    }
                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            if(respuesta == "")
            {
                return false;
            }
            else
            {
                return true;
            }


        }


    }
}
