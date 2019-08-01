using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository campRepository;
        private readonly IMapper mapper;

        public CampsController(ICampRepository campRepository, IMapper mapper)
        {
            this.campRepository = campRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            //try
            //{
                Camp[] results = await this.campRepository.GetAllCampsAsync(includeTalks);
                CampModel[] models = this.mapper.Map<CampModel[]>(results);
                return models;

            //}
            //catch (Exception)
            //{
            //    return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            //}
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

                CampModel models = this.mapper.Map<CampModel>(result);
                return models;

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
    }
}
