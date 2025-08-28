using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PropostaService.Api.Controllers;
using PropostaService.Application.DTOs;
using PropostaService.Application.Services;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using Xunit;
using System.ComponentModel.DataAnnotations;

namespace PropostaService.Tests.Api.Controllers
{
    public class PropostasControllerTests
    {
        private readonly Mock<IPropostaService> _mockPropostaService;
        private readonly PropostasController _controller;

        public PropostasControllerTests()
        {
            _mockPropostaService = new Mock<IPropostaService>();
            _controller = new PropostasController(_mockPropostaService.Object);
        }

        [Fact]
        public async Task GetAll_DeveRetornarTodasAsPropostas()
        {
            // Arrange
            var propostasDTO = new List<PropostaDTO>
            {
                new PropostaDTO
                {
                    Id = Guid.NewGuid(),
                    Nome = "João Silva",
                    CPF = "123.456.789-00",
                    ValorSeguro = 1000.00m,
                    Status = StatusProposta.EmAnalise,
                    DataCriacao = DateTime.Now
                },
                new PropostaDTO
                {
                    Id = Guid.NewGuid(),
                    Nome = "Maria Souza",
                    CPF = "987.654.321-00",
                    ValorSeguro = 2000.00m,
                    Status = StatusProposta.EmAnalise,
                    DataCriacao = DateTime.Now
                }
            };

            _mockPropostaService.Setup(s => s.ListarTodasAsync())
                .ReturnsAsync(propostasDTO);

            // Act
            var resultado = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<PropostaDTO>>(okResult.Value);
            Assert.Equal(propostasDTO.Count, returnValue.Count());
        }

        [Fact]
        public async Task GetAll_DeveRetornarListaVazia_QuandoNaoExistemPropostas()
        {
            // Arrange
            var listaVaziaDTO = new List<PropostaDTO>();

            _mockPropostaService.Setup(s => s.ListarTodasAsync())
                .ReturnsAsync(listaVaziaDTO);

            // Act
            var resultado = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<PropostaDTO>>(okResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task GetAll_DeveRetornarStatusCode500_QuandoOcorreExcecao()
        {
            // Arrange
            _mockPropostaService.Setup(s => s.ListarTodasAsync())
                .ThrowsAsync(new Exception("Erro ao listar propostas"));

            // Act
            var resultado = await _controller.GetAll();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetById_DeveRetornarProposta_QuandoExiste()
        {
            // Arrange
            var id = Guid.NewGuid();
            var propostaDTO = new PropostaDTO
            {
                Id = id,
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = 1000.00m,
                Status = StatusProposta.EmAnalise,
                DataCriacao = DateTime.Now
            };

            _mockPropostaService.Setup(s => s.ObterPorIdAsync(id))
                .ReturnsAsync(propostaDTO);

            // Act
            var resultado = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var returnValue = Assert.IsType<PropostaDTO>(okResult.Value);
            Assert.Equal(id, returnValue.Id);
            Assert.Equal(propostaDTO.Nome, returnValue.Nome);
            Assert.Equal(propostaDTO.CPF, returnValue.CPF);
        }

        // Este método foi removido porque já existe uma implementação anterior

        // Este método foi removido porque já existe uma implementação anterior

        [Fact]
        public async Task Create_DeveRetornarCreatedAtAction_QuandoPropostaCriadaComSucesso()
        {
            // Arrange
            var dto = new CriarPropostaDTO
            {
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = 1000.00m
            };

            var proposta = new Proposta(dto.Nome, dto.CPF, dto.ValorSeguro);
            var id = Guid.NewGuid();
            proposta.GetType().GetProperty("Id")?.SetValue(proposta, id);

            var propostaDTO = new PropostaDTO
            {
                Id = id,
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = 1000.00m,
                Status = StatusProposta.EmAnalise,
                DataCriacao = DateTime.Now
            };

            _mockPropostaService.Setup(s => s.CriarPropostaAsync(It.IsAny<CriarPropostaDTO>()))
                .ReturnsAsync(propostaDTO);

            // Act
            var resultado = await _controller.Create(dto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            var returnValue = Assert.IsType<PropostaDTO>(createdAtActionResult.Value);
            Assert.Equal(id, returnValue.Id);
            Assert.Equal(dto.Nome, returnValue.Nome);
            Assert.Equal(dto.CPF, returnValue.CPF);
            Assert.Equal(dto.ValorSeguro, returnValue.ValorSeguro);
        }

        // Este método foi removido porque já existe uma implementação anterior

        [Fact]
        public async Task Create_DeveRetornarStatusCode500_QuandoOcorreExcecaoNaoEsperada()
        {
            // Arrange
            var dto = new CriarPropostaDTO
            {
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = 1000.00m
            };

            _mockPropostaService.Setup(s => s.CriarPropostaAsync(It.IsAny<CriarPropostaDTO>()))
                .ThrowsAsync(new Exception("Erro inesperado"));

            // Act
            var resultado = await _controller.Create(dto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task UpdateStatus_DeveRetornarOk_QuandoStatusAtualizadoComSucesso()
        {
            // Arrange
            var dto = new AtualizarStatusPropostaDTO
            {
                Id = Guid.NewGuid(),
                NovoStatus = StatusProposta.Aprovada
            };

            var propostaDTO = new PropostaDTO
            {
                Id = dto.Id,
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = 1000.00m,
                Status = dto.NovoStatus,
                DataCriacao = DateTime.Now
            };

            _mockPropostaService.Setup(s => s.AtualizarStatusAsync(It.IsAny<AtualizarStatusPropostaDTO>()))
                .ReturnsAsync(propostaDTO);

            // Act
            var resultado = await _controller.UpdateStatus(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var returnValue = Assert.IsType<PropostaDTO>(okResult.Value);
            Assert.Equal(dto.Id, returnValue.Id);
            Assert.Equal(dto.NovoStatus, returnValue.Status);
        }

        [Fact]
        public async Task GetById_DeveRetornarBadRequest_QuandoIdInvalido()
        {
            // Arrange
            var idInvalido = Guid.Empty;

            // Act
            var resultado = await _controller.GetById(idInvalido);

            // Assert
            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task GetById_DeveRetornarStatusCode500_QuandoOcorreExcecao()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockPropostaService.Setup(s => s.ObterPorIdAsync(id))
                .ThrowsAsync(new Exception("Erro ao obter proposta"));

            // Act
            var resultado = await _controller.GetById(id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetById_DeveRetornarNotFound_QuandoNaoExiste()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockPropostaService.Setup(s => s.ObterPorIdAsync(id))
                .ThrowsAsync(new KeyNotFoundException("Proposta não encontrada"));

            // Act
            var resultado = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(resultado.Result);
        }

        [Fact]
        public async Task GetByStatus_DeveRetornarPropostasComStatusEspecifico()
        {
            // Arrange
            var status = StatusProposta.Aprovada;
            var propostasDTO = new List<PropostaDTO>
            {
                new PropostaDTO
                {
                    Id = Guid.NewGuid(),
                    Nome = "João Silva",
                    CPF = "123.456.789-00",
                    ValorSeguro = 1000.00m,
                    Status = status,
                    DataCriacao = DateTime.Now
                },
                new PropostaDTO
                {
                    Id = Guid.NewGuid(),
                    Nome = "Maria Souza",
                    CPF = "987.654.321-00",
                    ValorSeguro = 2000.00m,
                    Status = status,
                    DataCriacao = DateTime.Now
                }
            };

            _mockPropostaService.Setup(s => s.ListarPorStatusAsync(status))
                .ReturnsAsync(propostasDTO);
                
            // Act
            var resultado = await _controller.GetByStatus(status);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<PropostaDTO>>(okResult.Value);
            Assert.Equal(propostasDTO.Count, returnValue.Count());
        }

        [Theory]
        [InlineData(StatusProposta.Aprovada)]
        [InlineData(StatusProposta.Rejeitada)]
        [InlineData(StatusProposta.EmAnalise)]
        public async Task GetByStatus_DeveRetornarPropostasParaCadaStatus(StatusProposta status)
        {
            // Arrange
            var propostasDTO = new List<PropostaDTO>
            {
                new PropostaDTO
                {
                    Id = Guid.NewGuid(),
                    Nome = "João Silva",
                    CPF = "123.456.789-00",
                    ValorSeguro = 1000.00m,
                    Status = status,
                    DataCriacao = DateTime.Now
                }
            };

            _mockPropostaService.Setup(s => s.ListarPorStatusAsync(status))
                .ReturnsAsync(propostasDTO);

            // Act
            var resultado = await _controller.GetByStatus(status);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<PropostaDTO>>(okResult.Value);
            Assert.Equal(propostasDTO.Count, returnValue.Count());
        }

        [Fact]
        public async Task Create_DeveCriarProposta()
        {
            // Arrange
            var propostaDTO = new CriarPropostaDTO
            {
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = 1000.00m
            };

            var id = Guid.NewGuid();
            var retornoDTO = new PropostaDTO
            {
                Id = id,
                Nome = propostaDTO.Nome,
                CPF = propostaDTO.CPF,
                ValorSeguro = propostaDTO.ValorSeguro,
                Status = StatusProposta.EmAnalise,
                DataCriacao = DateTime.Now
            };

            _mockPropostaService.Setup(s => s.CriarPropostaAsync(It.IsAny<CriarPropostaDTO>()))
                .ReturnsAsync(retornoDTO);

            // Act
            var resultado = await _controller.Create(propostaDTO);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            var returnValue = Assert.IsType<PropostaDTO>(createdAtResult.Value);
            Assert.Equal(id, returnValue.Id);
            Assert.Equal(propostaDTO.Nome, returnValue.Nome);
            Assert.Equal(propostaDTO.CPF, returnValue.CPF);
            Assert.Equal(propostaDTO.ValorSeguro, returnValue.ValorSeguro);
        }

        [Fact]
        public async Task Create_DeveRetornarBadRequest_QuandoDadosInvalidos()
        {
            // Arrange
            var propostaInvalida = new CriarPropostaDTO
            {
                Nome = "", // Nome vazio
                CPF = "123", // CPF inválido
                ValorSeguro = -100 // Valor negativo
            };

            // Simula validação de modelo inválido
            _controller.ModelState.AddModelError("Nome", "O nome é obrigatório");
            _controller.ModelState.AddModelError("CPF", "CPF inválido");
            _controller.ModelState.AddModelError("ValorSeguro", "O valor do seguro deve ser positivo");

            // Act
            var resultado = await _controller.Create(propostaInvalida);

            // Assert
            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task Create_DeveRetornarStatusCode500_QuandoOcorreExcecao()
        {
            // Arrange
            var propostaDTO = new CriarPropostaDTO
            {
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = 1000.00m
            };

            _mockPropostaService.Setup(s => s.CriarPropostaAsync(It.IsAny<CriarPropostaDTO>()))
                .ThrowsAsync(new Exception("Erro ao criar proposta"));

            // Act
            var resultado = await _controller.Create(propostaDTO);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task UpdateStatus_DeveAtualizarStatus_QuandoExiste()
        {
            // Arrange
            var id = Guid.NewGuid();
            var status = StatusProposta.Aprovada;
            var propostaDto = new PropostaDTO
            {
                Id = id,
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = 1000.00m,
                Status = status,
                DataCriacao = DateTime.Now
            };

            _mockPropostaService.Setup(s => s.AtualizarStatusAsync(It.IsAny<AtualizarStatusPropostaDTO>()))
                .ReturnsAsync(propostaDto);

            // Act
            var dto = new AtualizarStatusPropostaDTO { Id = id, NovoStatus = status };
            var resultado = await _controller.UpdateStatus(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var returnValue = Assert.IsType<PropostaDTO>(okResult.Value);
            Assert.Equal(id, returnValue.Id);
            Assert.Equal(status, returnValue.Status);
        }

        [Fact]
        public async Task UpdateStatus_DeveRetornarNotFound_QuandoNaoExiste()
        {
            // Arrange
            var id = Guid.NewGuid();
            var status = StatusProposta.Aprovada;

            _mockPropostaService.Setup(s => s.AtualizarStatusAsync(It.IsAny<AtualizarStatusPropostaDTO>()))
                .ThrowsAsync(new KeyNotFoundException("Proposta não encontrada"));

            // Act
            var dto = new AtualizarStatusPropostaDTO { Id = id, NovoStatus = status };
            var resultado = await _controller.UpdateStatus(dto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task UpdateStatus_DeveRetornarBadRequest_QuandoIdInvalido()
        {
            // Arrange
            var idInvalido = Guid.Empty;
            var status = StatusProposta.Aprovada;

            // Act
            var dto = new AtualizarStatusPropostaDTO { Id = idInvalido, NovoStatus = status };
            var resultado = await _controller.UpdateStatus(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task UpdateStatus_DeveRetornarStatusCode500_QuandoOcorreExcecao()
        {
            // Arrange
            var id = Guid.NewGuid();
            var status = StatusProposta.Aprovada;

            _mockPropostaService.Setup(s => s.AtualizarStatusAsync(It.IsAny<AtualizarStatusPropostaDTO>()))
                .ThrowsAsync(new Exception("Erro ao atualizar status"));

            // Act
            var dto = new AtualizarStatusPropostaDTO { Id = id, NovoStatus = status };
            var resultado = await _controller.UpdateStatus(dto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Theory]
        [InlineData(StatusProposta.Aprovada)]
        [InlineData(StatusProposta.Rejeitada)]
        public async Task UpdateStatus_DevePermitirAtualizarParaDiferentesStatus(StatusProposta novoStatus)
        {
            // Arrange
            var id = Guid.NewGuid();
            var propostaDto = new PropostaDTO
            {
                Id = id,
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = 1000.00m,
                Status = novoStatus,
                DataCriacao = DateTime.Now
            };

            _mockPropostaService.Setup(s => s.AtualizarStatusAsync(It.IsAny<AtualizarStatusPropostaDTO>()))
                .ReturnsAsync(propostaDto);

            // Act
            var dto = new AtualizarStatusPropostaDTO { Id = id, NovoStatus = novoStatus };
            var resultado = await _controller.UpdateStatus(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var returnValue = Assert.IsType<PropostaDTO>(okResult.Value);
            Assert.Equal(novoStatus, returnValue.Status);
        }

        [Fact]
        public async Task GetByStatus_DeveRetornarListaVazia_QuandoNaoExistemPropostasComStatus()
        {
            // Arrange
            var status = StatusProposta.Rejeitada;
            var listaVazia = new List<PropostaDTO>();

            _mockPropostaService.Setup(s => s.ListarPorStatusAsync(status))
                .ReturnsAsync(listaVazia);

            // Act
            var resultado = await _controller.GetByStatus(status);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<PropostaDTO>>(okResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task GetByStatus_DeveRetornarStatusCode500_QuandoOcorreExcecao()
        {
            // Arrange
            var status = StatusProposta.EmAnalise;

            _mockPropostaService.Setup(s => s.ListarPorStatusAsync(status))
                .ThrowsAsync(new Exception("Erro ao listar propostas por status"));

            // Act
            var resultado = await _controller.GetByStatus(status);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task Create_DeveValidarModelState()
        {
            // Arrange
            var propostaDTO = new CriarPropostaDTO
            {
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = 1000.00m
            };

            // Adiciona erro de validação ao ModelState
            _controller.ModelState.AddModelError("Nome", "O nome é obrigatório");

            // Act
            var resultado = await _controller.Create(propostaDTO);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateStatus_DeveValidarModelState()
        {
            // Arrange
            var dto = new AtualizarStatusPropostaDTO
            {
                Id = Guid.NewGuid(),
                NovoStatus = StatusProposta.Aprovada
            };

            // Adiciona erro de validação ao ModelState
            _controller.ModelState.AddModelError("Id", "O ID é obrigatório");

            // Act
            var resultado = await _controller.UpdateStatus(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Create_DeveRetornarBadRequest_QuandoValorSeguroNegativo()
        {
            // Arrange
            var propostaDTO = new CriarPropostaDTO
            {
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = -100.00m // Valor negativo
            };

            _mockPropostaService.Setup(s => s.CriarPropostaAsync(It.IsAny<CriarPropostaDTO>()))
                .ThrowsAsync(new ArgumentException("O valor do seguro deve ser positivo"));

            // Act
            var resultado = await _controller.Create(propostaDTO);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("valor", badRequestResult.Value!.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}