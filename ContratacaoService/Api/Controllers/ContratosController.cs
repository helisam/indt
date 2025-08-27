using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Services;

namespace ContratacaoService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContratosController : ControllerBase
    {
        private readonly IContratoService _contratoService;

        public ContratosController(IContratoService contratoService)
        {
            _contratoService = contratoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContratoDTO>>> GetAll()
        {
            var contratos = await _contratoService.ListarTodosAsync();
            return Ok(contratos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContratoDTO>> GetById(Guid id)
        {
            try
            {
                var contrato = await _contratoService.ObterPorIdAsync(id);
                return Ok(contrato);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("proposta/{propostaId}")]
        public async Task<ActionResult<ContratoDTO>> GetByPropostaId(Guid propostaId)
        {
            try
            {
                var contrato = await _contratoService.ObterPorPropostaIdAsync(propostaId);
                return Ok(contrato);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("cpf/{cpf}")]
        public async Task<ActionResult<IEnumerable<ContratoDTO>>> GetByCpf(string cpf)
        {
            var contratos = await _contratoService.ListarAtivosPorCpfAsync(cpf);
            return Ok(contratos);
        }

        [HttpPost]
        public async Task<ActionResult<ContratoDTO>> Create(CriarContratoDTO dto)
        {
            var contrato = await _contratoService.CriarContratoAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = contrato.Id }, contrato);
        }

        [HttpPut("{id}/cancelar")]
        public async Task<ActionResult<ContratoDTO>> Cancel(Guid id)
        {
            try
            {
                var contrato = await _contratoService.CancelarContratoAsync(id);
                return Ok(contrato);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}