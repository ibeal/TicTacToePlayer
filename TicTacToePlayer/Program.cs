using System.Net.Http.Headers;
using System.Text.Json;
using TicTacToePlayer;

Console.WriteLine("howzit");
Console.WriteLine("Please enter your name:");
string name = Console.ReadLine();
if (string.IsNullOrEmpty(name))
{
    name = "Idiot";
}
Console.WriteLine($"Welcome, {name}");
Console.WriteLine("Is this a landmine game?(y/n)");
string isLandmineString = Console.ReadLine();

if (isLandmineString.ToLower() == "y")
{
    Console.WriteLine("Playing a land mine game");
}

var bot = new TicTacToeBot(name, isLandmineString.ToLower() == "y");

string? ValidRoomCode = null;
do
{
    Console.WriteLine("Enter a room code (a blank entry will generate a new room):");
    string roomCode = Console.ReadLine();
    if (string.IsNullOrEmpty(roomCode))
    {
        var room = await bot.GenerateGame();
        Console.WriteLine($"A game was created under room code {room.RoomCode}");
        ValidRoomCode = room.RoomCode;
    }
    else
    {
        var room = await bot.JoinGame(roomCode);
        if (room != null)
        {
            ValidRoomCode = room.RoomCode;
        }
        else
        {
            Console.WriteLine("Invalid room code");
        };
    }
} while (ValidRoomCode is null);


// Create player class here

while (true)
{
    var game = await bot.GetGameStatus(ValidRoomCode);
    int counter = 0;
    while (game.currentGameStatus == CurrentGameStatus.WaitingForOpponent)
    {
        await Task.Delay(421);
        game = await bot.GetGameStatus(ValidRoomCode);
        if (counter++ == 5)
        {
            await bot.Continue(ValidRoomCode);
            counter = 0;
        }
    }
    while (game.currentGameStatus != CurrentGameStatus.GameOver && game.currentGameStatus != CurrentGameStatus.WaitingForOpponent)
    {
        if ((game.currentGameStatus == CurrentGameStatus.PlayerXTurn && game.PlayerXName == bot.name) ||
            (game.currentGameStatus is CurrentGameStatus.PlayerOTurn && game.PlayerOName == bot.name))
        {
            await bot.MakeMove(game);
        }
        else
        {
            await Task.Delay(400);
        }
        game = await bot.GetGameStatus(ValidRoomCode);
    }

    await Task.Delay(1521);
    Console.WriteLine(JsonSerializer.Serialize(game));
    if (await bot.Continue(ValidRoomCode) is null)
    {
        break;
    }

}




