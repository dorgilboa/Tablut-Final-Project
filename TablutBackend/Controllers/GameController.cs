using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TablutBackend.DAL;
using TablutBackend.Models;

namespace TablutBackend.Controllers
{
    public class GameController : ApiController
    {
        // GET api/<controller>
        public long Get()
        {
            return GameDAL.CreateGame();
        }

        // POST api/<controller> responsible for the AI turn by color that he gets... optional turn number delivered.
        // returns list of two (or more) numbers -> from and to, captures positions or 102 / 101 if match ended.
        public List<int> Post([FromBody] dynamic data)
        {
            return GameDAL.SetAITurn((long)data.id, (string)data.color);
        }

        // PUT api/<controller>/id responsible for managing client's turn request and updating according to given data.
        // returns list of captures positions.
        public List<int> Put(long id, [FromBody] dynamic data)
        {
            return GameDAL.SetTurn(id, (int)data.from, (int)data.to, (string)data.color);
        }
    }
}