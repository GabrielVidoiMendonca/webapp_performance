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
        [HttpPost]

        public async Task<IActionResult> PostUsuario([FromBody] Usuario usuario)
        {
            string connectionString = "Server=localhost;Database=sys;User=root;Password=123;";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "INSERT INTO usuarios(Nome,Email) VALUES (@nome,@email)";
            await connection.ExecuteAsync(sql, usuario);

            //apaga o cache
            string key = "getUsuario";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            return Ok(usuario);

        }

        [HttpPut]
        public async Task<IActionResult> PutUsuario([FromBody] Usuario usuario)
        {
            string connectionString = "Server=localhost;Database=sys;User=root;Password=123";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "UPDATE usuarios SET Nome = @nome, Email = @email WHERE Id = @id";
            await connection.ExecuteAsync(sql, usuario);

            //apaga o cache
            string key = "getUsuario";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            return Ok();

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string connectionString = "Server=localhost;Database=sys;User=root;Password=123";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "DELETE FROM usuarios WHERE id = @id";
            await connection.ExecuteAsync(sql, new { id });

            //apaga o cache
            string key = "getusuario";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            return Ok();

        }
    }
}
