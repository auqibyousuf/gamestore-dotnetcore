using System;
using GameStore.Backend.Dtos;

namespace GameStore.Backend.ViewModels;

public class GameIndexViewModel
{
    public List<GameListDto> Games { get; set; } = [];
    public CreateGameDto CreateGame { get; set; } = new();
}
