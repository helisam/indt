using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ContratacaoService.Api.Controllers;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Services;

namespace ContratacaoService.Tests.Api.Controllers
{
    public class ContratosControllerTests
    {
        private readonly Mock<IContratoService> _mockService;
        private readonly ContratosController _controller;

        public ContratosControllerTests()
        {
            _mockService = new Mock<IContratoService>();
            _controller = new ContratosController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_DeveRetornarOkComListaDeContratos()
        {
            // Arrange
            var contratos = new List<ContratoDTO>
            {
                new ContratoDTO { Id = Guid.NewGuid(), Nome = "João" },
                new ContratoDTO { Id = Guid.NewGuid(), Nome = "Maria" }
            };

            _mockService.Setup(s => s.ListarTodosAsync())
                .ReturnsAsync(contratos);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<ContratoDTO>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }

        [Fact]
        public async Task GetById_QuandoContratoExiste_DeveRetornarOkComContrato()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contrato = new ContratoDTO { Id = id, Nome = "João" };

            _mockService.Setup(s => s.ObterPorIdAsync(id))
                .ReturnsAsync(contrato);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ContratoDTO>(okResult.Value);
            Assert.Equal(id, returnValue.Id);
        }

        [Fact]
        public async Task GetById_QuandoContratoNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockService.Setup(s => s.ObterPorIdAsync(id))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetByPropostaId_QuandoContratoExiste_DeveRetornarOkComContrato()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var contrato = new ContratoDTO { Id = Guid.NewGuid(), PropostaId = propostaId, Nome = "João" };

            _mockService.Setup(s => s.ObterPorPropostaIdAsync(propostaId))
                .ReturnsAsync(contrato);

            // Act
            var result = await _controller.GetByPropostaId(propostaId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ContratoDTO>(okResult.Value);
            Assert.Equal(propostaId, returnValue.PropostaId);
        }

        [Fact]
        public async Task GetByPropostaId_QuandoContratoNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            var propostaId = Guid.NewGuid();

            _mockService.Setup(s => s.ObterPorPropostaIdAsync(propostaId))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetByPropostaId(propostaId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetByCpf_DeveRetornarOkComListaDeContratos()
        {
            // Arrange
            var cpf = "12345678900";
            var contratos = new List<ContratoDTO>
            {
                new ContratoDTO { Id = Guid.NewGuid(), CPF = cpf, Nome = "João" }
            };

            _mockService.Setup(s => s.ListarAtivosPorCpfAsync(cpf))
                .ReturnsAsync(contratos);

            // Act
            var result = await _controller.GetByCpf(cpf);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<ContratoDTO>>(okResult.Value);
            Assert.Single(returnValue);
            Assert.Equal(cpf, returnValue.First().CPF);
        }

        [Fact]
        public async Task Create_DeveRetornarCreatedAtActionComContrato()
        {
            // Arrange
            var dto = new CriarContratoDTO
            {
                PropostaId = Guid.NewGuid(),
                Nome = "João Silva",
                CPF = "12345678900",
                ValorSeguro = 1000.50m,
                DuracaoMeses = 12
            };

            var contratoId = Guid.NewGuid();
            var contratoDto = new ContratoDTO
            {
                Id = contratoId,
                PropostaId = dto.PropostaId,
                Nome = dto.Nome,
                CPF = dto.CPF,
                ValorSeguro = dto.ValorSeguro
            };

            _mockService.Setup(s => s.CriarContratoAsync(dto))
                .ReturnsAsync(contratoDto);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(ContratosController.GetById), createdAtActionResult.ActionName);
            Assert.Equal(contratoId, createdAtActionResult.RouteValues["id"]);
            var returnValue = Assert.IsType<ContratoDTO>(createdAtActionResult.Value);
            Assert.Equal(contratoId, returnValue.Id);
        }

        [Fact]
        public async Task Cancel_QuandoContratoExiste_DeveRetornarOkComContratoCancelado()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contrato = new ContratoDTO { Id = id, Ativo = false };

            _mockService.Setup(s => s.CancelarContratoAsync(id))
                .ReturnsAsync(contrato);

            // Act
            var result = await _controller.Cancel(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ContratoDTO>(okResult.Value);
            Assert.Equal(id, returnValue.Id);
            Assert.False(returnValue.Ativo);
        }

        [Fact]
        public async Task Cancel_QuandoContratoNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockService.Setup(s => s.CancelarContratoAsync(id))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Cancel(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAll_QuandoOcorreExcecao_DeveRetornarStatusCode500()
        {
            // Arrange
            _mockService.Setup(s => s.ListarTodosAsync())
                .ThrowsAsync(new Exception("Erro ao listar contratos"));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetById_QuandoIdInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetById_QuandoOcorreExcecao_DeveRetornarStatusCode500()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockService.Setup(s => s.ObterPorIdAsync(id))
                .ThrowsAsync(new Exception("Erro ao obter contrato"));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetByPropostaId_QuandoIdInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var propostaId = Guid.Empty;

            // Act
            var result = await _controller.GetByPropostaId(propostaId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetByPropostaId_QuandoOcorreExcecao_DeveRetornarStatusCode500()
        {
            // Arrange
            var propostaId = Guid.NewGuid();

            _mockService.Setup(s => s.ObterPorPropostaIdAsync(propostaId))
                .ThrowsAsync(new Exception("Erro ao obter contrato por proposta"));

            // Act
            var result = await _controller.GetByPropostaId(propostaId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetByCpf_QuandoCpfInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var cpf = "123"; // CPF inválido

            _mockService.Setup(s => s.ListarAtivosPorCpfAsync(cpf))
                .ThrowsAsync(new ArgumentException("CPF inválido"));

            // Act
            var result = await _controller.GetByCpf(cpf);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_QuandoDadosInvalidos_DeveRetornarBadRequest()
        {
            // Arrange
            var dto = new CriarContratoDTO
            {
                PropostaId = Guid.NewGuid(),
                Nome = "", // Nome inválido
                CPF = "12345678900",
                ValorSeguro = 1000.50m,
                DuracaoMeses = 12
            };

            _mockService.Setup(s => s.CriarContratoAsync(dto))
                .ThrowsAsync(new ArgumentException("Nome inválido"));

            // Act
            var result = await _controller.Create(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_QuandoModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var dto = new CriarContratoDTO(); // DTO vazio
            _controller.ModelState.AddModelError("Nome", "Nome é obrigatório");

            // Act
            var result = await _controller.Create(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Cancel_QuandoIdInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            var result = await _controller.Cancel(id);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Cancel_QuandoContratoJaCancelado_DeveRetornarBadRequest()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockService.Setup(s => s.CancelarContratoAsync(id))
                .ThrowsAsync(new InvalidOperationException("Contrato já está cancelado"));

            // Act
            var result = await _controller.Cancel(id);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}