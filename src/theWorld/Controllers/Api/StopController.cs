using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using TheWorld.Models;
using TheWorld.Services;
using TheWorld.ViewModels;

namespace TheWorld.Controllers.Api
{
    [Authorize]
    [Route("api/trips/{tripName}/stops")]
    public class StopController : Controller
    {
        private IWorldRepository _reposiory;
        private ILogger<StopController> _logger;
        private CoordService _coordService;

        public StopController(
            IWorldRepository repository, 
            ILogger<StopController> logger,
            CoordService coordService)
        {
            _reposiory = repository;
            _logger = logger;
            _coordService = coordService;
        }

        [HttpGet("")]
        public JsonResult Get(string tripName)
        {
            try
            {
                var results = _reposiory.GetTripByName(tripName, User.Identity.Name);

                if (results == null)
                {
                    return Json(null);
                }
                return Json(Mapper.Map<IEnumerable<StopViewModel>>(results.Stops.OrderBy(s => s.Order)));
            }
            catch (Exception ex)
            {
                
                _logger.LogError($"Failed to get stops for trip {tripName}", ex);
                Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return Json("Error occurred finding trip name");
            }
            
        }

        [HttpPost("")]
        public async Task<JsonResult> Post(string tripName, [FromBody] StopViewModel vm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Map to the Entity
                    var newStop = Mapper.Map<Stop>(vm);
                    // Looking up Geocoordinates
                    var coordResult = await _coordService.Lookup(newStop.Name);
                    if (!coordResult.Success)
                    {
                        Response.StatusCode = (int) HttpStatusCode.BadRequest;
                        Json(coordResult.Message);
                    }

                    newStop.Longitude = coordResult.Longitude;
                    newStop.Latitude = coordResult.Latitude;

                    // Save to the DB
                    _reposiory.AddStop(tripName, newStop, User.Identity.Name);
                    if (_reposiory.SaveAll())
                    {
                        Response.StatusCode = (int) HttpStatusCode.OK;
                        return Json(Mapper.Map<StopViewModel>(newStop));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save new stop", ex);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Failed to save new stop");
            }
            return Json(new { Message = "Failed", ModelState = ModelState });
        }
    }
}
