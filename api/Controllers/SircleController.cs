using api.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;


namespace api.Controllers
{
    /*
    [Route("api/[controller]")]
    [ApiController]
    public class SircleController : ControllerBase
    {
        //private readonly IUnitCircle _unitCircle;
        UnitCircle jtn = new UnitCircle();

        public SircleController() { }

        [HttpPost]
        public List<Tuple<double, double>> CreateNewSircle( double radius, double longitude, double latitude)
        {
            //var answer = _unitCircle.CreateCircle(radius, longitude, latitude);
            var answer = jtn.CreateCircle(radius, longitude, latitude);

            return answer;
        }
    }
    */
}
