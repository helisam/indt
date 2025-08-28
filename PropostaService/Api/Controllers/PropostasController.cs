using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PropostaService.Application.DTOs;
using PropostaService.Application.Services;
using PropostaService.Domain.Enums;

namespace PropostaService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropostasController : ControllerBase
    {
        private readonly IPropostaService _propostaService;

        public PropostasController(IPropostaService propostaService)
        {
            _propostaService = propostaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PropostaDTO>>> GetAll()
        {
            try
            {
                var propostas = await _propostaService.ListarTodasAsync();
                return Ok(propostas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PropostaDTO>> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var proposta = await _propostaService.ObterPorIdAsync(id);
                return Ok(proposta);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<PropostaDTO>>> GetByStatus(StatusProposta status)
        {
            try
            {
                var propostas = await _propostaService.ListarPorStatusAsync(status);
                return Ok(propostas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<PropostaDTO>> Create(CriarPropostaDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var proposta = await _propostaService.CriarPropostaAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = proposta.Id }, proposta);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("status")]
        public async Task<ActionResult<PropostaDTO>> UpdateStatus(AtualizarStatusPropostaDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.Id == Guid.Empty)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var proposta = await _propostaService.AtualizarStatusAsync(dto);
                return Ok(proposta);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Proposta não encontrada");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}