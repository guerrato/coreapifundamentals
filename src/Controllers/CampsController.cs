using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository campRepository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.campRepository = campRepository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                Camp[] results = await this.campRepository.GetAllCampsAsync(includeTalks);
                return this.mapper.Map<CampModel[]>(results);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                Camp result = await this.campRepository.GetCampAsync(moniker);

                if (result == null)
                {
                    return NotFound();
                }

                return this.mapper.Map<CampModel>(result);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                Camp[] results = await this.campRepository.GetAllCampsByEventDate(theDate, includeTalks);
                if (!results.Any())
                {
                    return NotFound();
                }

                return this.mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                Camp exists = await this.campRepository.GetCampAsync(model.Moniker);

                if (exists != null)
                {
                    return BadRequest("Moniker in use");
                }

                var location = this.linkGenerator.GetPathByAction("Get", "Camps", new { model = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                Camp camp = this.mapper.Map<Camp>(model);

                this.campRepository.Add(camp);

                if (await this.campRepository.SaveChangesAsync())
                {
                    return Created($"/api/camps/{camp.Moniker}", this.mapper.Map<CampModel>(camp));
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
        {
            try
            {
                Camp stored = await this.campRepository.GetCampAsync(moniker);

                if (stored == null)
                {
                    return NotFound($"Cold not find the camp with moniker {moniker}");
                }

                this.mapper.Map(model, stored);

                if(await this.campRepository.SaveChangesAsync())
                {
                    return this.mapper.Map<CampModel>(stored);
                }

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<ActionResult> Delete(string moniker)
        {
            try
            {
                Camp stored = await this.campRepository.GetCampAsync(moniker);

                if (stored == null)
                {
                    return NotFound($"Cold not find the camp with moniker {moniker}");
                }

                this.campRepository.Delete(stored);

                if (await this.campRepository.SaveChangesAsync())
                {
                    return Ok();
                }

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }
    }
}
