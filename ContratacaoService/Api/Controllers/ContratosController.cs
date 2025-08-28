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
            try
            {
                var contratos = await _contratoService.ListarTodosAsync();
                return Ok(contratos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContratoDTO>> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("ID do contrato não pode ser vazio");
            }
            
            try
            {
                var contrato = await _contratoService.ObterPorIdAsync(id);
                return Ok(contrato);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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

        [HttpGet("proposta/{propostaId}")]
        public async Task<ActionResult<ContratoDTO>> GetByPropostaId(Guid propostaId)
        {
            if (propostaId == Guid.Empty)
            {
                return BadRequest("ID da proposta não pode ser vazio");
            }
            
            try
            {
                var contrato = await _contratoService.ObterPorPropostaIdAsync(propostaId);
                return Ok(contrato);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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

        [HttpGet("cpf/{cpf}")]
        public async Task<ActionResult<IEnumerable<ContratoDTO>>> GetByCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
            {
                return BadRequest("CPF não pode ser vazio");
            }
            
            try
            {
                var contratos = await _contratoService.ListarAtivosPorCpfAsync(cpf);
                return Ok(contratos);
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

        [HttpPost]
        public async Task<ActionResult<ContratoDTO>> Create(CriarContratoDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                var contrato = await _contratoService.CriarContratoAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = contrato.Id }, contrato);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/cancelar")]
        public async Task<ActionResult<ContratoDTO>> Cancel(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("ID do contrato não pode ser vazio");
            }
            
            try
            {
                var contrato = await _contratoService.CancelarContratoAsync(id);
                return Ok(contrato);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
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
    }
}