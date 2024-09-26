using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Newtonsoft.Json;
using StackExchange.Redis;
using web_app_performance.Model;

namespace web_app_performance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private ConnectionMultiplexer redis;

        [HttpGet]
        public async Task<IActionResult> GetUsuario()
        {
            string key = "getUsuario";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyExpireAsync(key, TimeSpan.FromSeconds(10));
            string user = await db.StringGetAsync(key);

            if(!string.IsNullOrEmpty(user))
                {
                    return Ok(user);
                }



            string connectionString = "Server=localhost;Database=sys;User=root;Password=123;";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            string query = "select Id, Nome, Email from usuarios;";
            var usuarios = await connection.QueryAsync<Usuario>(query);
            string usuariosJson = JsonConvert.SerializeObject(usuarios);
            await db.StringSetAsync(key, usuariosJson);

            return Ok(usuarios);
        }
    }
}
