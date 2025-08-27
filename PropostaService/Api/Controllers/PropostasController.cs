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
            var propostas = await _propostaService.ListarTodasAsync();
            return Ok(propostas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PropostaDTO>> GetById(Guid id)
        {
            try
            {
                var proposta = await _propostaService.ObterPorIdAsync(id);
                return Ok(proposta);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<PropostaDTO>>> GetByStatus(StatusProposta status)
        {
            var propostas = await _propostaService.ListarPorStatusAsync(status);
            return Ok(propostas);
        }

        [HttpPost]
        public async Task<ActionResult<PropostaDTO>> Create(CriarPropostaDTO dto)
        {
            var proposta = await _propostaService.CriarPropostaAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = proposta.Id }, proposta);
        }

        [HttpPut("status")]
        public async Task<ActionResult<PropostaDTO>> UpdateStatus(AtualizarStatusPropostaDTO dto)
        {
            try
            {
                var proposta = await _propostaService.AtualizarStatusAsync(dto);
                return Ok(proposta);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}