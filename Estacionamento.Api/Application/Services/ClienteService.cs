using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IClienteService
{
    Task<IEnumerable<ClienteResponseDto>> ObterTodosAsync(bool apenasAtivos = true);
    Task<ClienteResponseDto?> ObterPorIdAsync(int id);
    Task<ClienteResponseDto> CriarAsync(CriarClienteDto dto);
    Task<ClienteResponseDto?> AtualizarAsync(int id, AtualizarClienteDto dto);
    Task<bool> DesativarAsync(int id);
    Task<Cliente> ObterOuCriarPorTelefoneAsync(string nome, string telefone, string? email, string? placa, string? modelo, string? cor);
}

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<IEnumerable<ClienteResponseDto>> ObterTodosAsync(bool apenasAtivos = true)
    {
        var clientes = await _clienteRepository.ObterTodosAsync(apenasAtivos);
        return clientes.Select(MapToResponse);
    }

    public async Task<ClienteResponseDto?> ObterPorIdAsync(int id)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id);
        return cliente == null ? null : MapToResponse(cliente);
    }

    public async Task<ClienteResponseDto> CriarAsync(CriarClienteDto dto)
    {
        var cliente = new Cliente
        {
            Nome = dto.Nome,
            Telefone = dto.Telefone,
            Email = dto.Email,
            Cpf = dto.Cpf,
            PlacaVeiculo = dto.PlacaVeiculo,
            ModeloVeiculo = dto.ModeloVeiculo,
            CorVeiculo = dto.CorVeiculo
        };

        var criado = await _clienteRepository.CriarAsync(cliente);
        return MapToResponse(criado);
    }

    public async Task<ClienteResponseDto?> AtualizarAsync(int id, AtualizarClienteDto dto)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id);
        if (cliente == null) return null;

        if (dto.Nome != null) cliente.Nome = dto.Nome;
        if (dto.Telefone != null) cliente.Telefone = dto.Telefone;
        if (dto.Email != null) cliente.Email = dto.Email;
        if (dto.Cpf != null) cliente.Cpf = dto.Cpf;
        if (dto.PlacaVeiculo != null) cliente.PlacaVeiculo = dto.PlacaVeiculo;
        if (dto.ModeloVeiculo != null) cliente.ModeloVeiculo = dto.ModeloVeiculo;
        if (dto.CorVeiculo != null) cliente.CorVeiculo = dto.CorVeiculo;

        var atualizado = await _clienteRepository.AtualizarAsync(cliente);
        return MapToResponse(atualizado);
    }

    public async Task<bool> DesativarAsync(int id)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id);
        if (cliente == null) return false;

        cliente.Ativo = false;
        await _clienteRepository.AtualizarAsync(cliente);
        return true;
    }

    public async Task<Cliente> ObterOuCriarPorTelefoneAsync(
        string nome, string telefone, string? email, string? placa, string? modelo, string? cor)
    {
        var existente = await _clienteRepository.ObterPorTelefoneAsync(telefone);
        if (existente != null) return existente;

        var novo = new Cliente
        {
            Nome = nome,
            Telefone = telefone,
            Email = email,
            PlacaVeiculo = placa,
            ModeloVeiculo = modelo,
            CorVeiculo = cor
        };

        return await _clienteRepository.CriarAsync(novo);
    }

    private static ClienteResponseDto MapToResponse(Cliente c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        Telefone = c.Telefone,
        Email = c.Email,
        Cpf = c.Cpf,
        PlacaVeiculo = c.PlacaVeiculo,
        ModeloVeiculo = c.ModeloVeiculo,
        CorVeiculo = c.CorVeiculo,
        DataCadastro = c.DataCadastro,
        Ativo = c.Ativo,
        TotalReservas = c.Reservas?.Count ?? 0
    };
}
