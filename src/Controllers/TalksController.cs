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

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository campRepository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public TalksController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.campRepository = campRepository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                Talk[] results = await this.campRepository.GetTalksByMonikerAsync(moniker, true);
                return this.mapper.Map<TalkModel[]>(results);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                Talk result = await this.campRepository.GetTalkByMonikerAsync(moniker, id, true);

                if (result == null)
                {
                    return NotFound();
                }

                return this.mapper.Map<TalkModel>(result);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CampModel>> Post(string moniker, TalkModel model)
        {
            try
            {
                Camp camp = await this.campRepository.GetCampAsync(moniker);

                if (camp == null)
                {
                    return BadRequest("Camp does not exist");
                }

                Talk talk = this.mapper.Map<Talk>(model);
                Speaker speaker = await this.campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);

                if (speaker == null)
                {
                    return BadRequest("Speaker could not be found");
                }

                talk.Camp = camp;
                talk.Speaker = speaker;

                this.campRepository.Add(talk);

                if (await this.campRepository.SaveChangesAsync())
                {
                    var url = this.linkGenerator.GetPathByAction(HttpContext, "Get", values: new { moniker, id = talk.TalkId });
                    return Created(url, this.mapper.Map<TalkModel>(talk));
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel model)
        {
            try
            {
                Talk talk = await this.campRepository.GetTalkByMonikerAsync(moniker, id, true);

                if (talk == null)
                {
                    return NotFound("Talk not found.");
                }

                if (model.Speaker != null)
                {
                    Speaker speaker = await this.campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }

                this.mapper.Map(model, talk);

                if (await this.campRepository.SaveChangesAsync())
                {
                    return this.mapper.Map<TalkModel>(talk);
                }

                return BadRequest("Fail to update database");

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id)
        {
            try
            {
                Talk talk = await this.campRepository.GetTalkByMonikerAsync(moniker, id);

                if (talk == null)
                {
                    return NotFound("Talk not found.");
                }

                this.campRepository.Delete(talk);

                if (await this.campRepository.SaveChangesAsync())
                {
                    return Ok();
                }

                return BadRequest("Fail to delete database");

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
    }
}
