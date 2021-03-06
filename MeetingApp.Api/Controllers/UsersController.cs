using MeetingApp.Api.Models;
using MeetingApp.Api.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MeetingAppContext _context;
        Hash _hash;
        LoginService _loginService = new LoginService();


        public UsersController(MeetingAppContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public IEnumerable<User> GetUser([FromQuery]string userId)
        {
            if (userId == null) { return _context.User; }

            var user = _context.User.Where(u => u.UserId == userId).FirstOrDefault();
            return new User[] { user };

        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }




        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser([FromRoute] int id, [FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Bodyに入力されたPasswordをハッシュ化
            string userPassword = user.Password;
            _hash = new Hash();
            user.Password = _hash.Encrypt(userPassword);

            _context.User.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserExists(user.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // POST: api/Users/login
        [HttpPost("{login}")]
        public async Task<IActionResult> LoginUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Bodyに入力されたpasswordをハッシュ化
            string userPassword = user.Password;
            _hash = new Hash();
            user.Password = _hash.Encrypt(userPassword);

            //bodyで与えられたuserIdとPasswordからテーブル内の一致情報を取得
            var searchedUser = _context.User.Where(u => u.UserId == user.UserId && u.Password == user.Password).FirstOrDefault();

            //Userが存在すればtokenを発行しtokenDBに保存
            if (searchedUser != null)
            {
                var token = _loginService.CreateToken();
                _context.Token.Add(token);

                try
                {
                    await _context.SaveChangesAsync();
                    return Ok(token);
                }
                catch (DbUpdateException)
                {
                    throw;
                }

            }

            //new StatusCodeResult(StatusCodes.Status404NotFound);

            ////失敗Tokenを発行
            //Token faultToken = new Token();
            //faultToken.TokenText = "failed";


            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}