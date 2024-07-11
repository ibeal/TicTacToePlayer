using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Json;

namespace TicTacToePlayer
{
    public class TicTacToeBot
    {
        private HttpClient host;
        public string name;
        private bool isLandmindGame;
        private bool madeFirstMove = false;
        private int[][][] winningSets = new[]
        {
            new[]
            {
                new[] {0, 0},
                new[] {0, 1},
                new[] {0, 2}
            },
            new[]
            {
                new[] {1, 0},
                new[] {1, 1},
                new[] {1, 2}
            },
            new[]
            {
                new[] {2, 0},
                new[] {2, 1},
                new[] {2, 2}
            },
            new[]
            {
                new[] {0, 0},
                new[] {1, 0},
                new[] {2, 0}
            },
            new[]
            {
                new[] {0, 1},
                new[] {1, 1},
                new[] {2, 1}
            },
            new[]
            {
                new[] {0, 2},
                new[] {1, 2},
                new[] {2, 2}
            },
            new[]
            {
                new[] {0, 0},
                new[] {1, 1},
                new[] {2, 2}
            },
            new[]
            {
                new[] {0, 2},
                new[] {1, 1},
                new[] {2, 0}
            },
        };
        private int[][] corners = new[]
        {
            new[] {0,0},
            new[] {0,2},
            new[] {2,0},
            new[] {2,2},
        };
        private int[][] sides = new[]
        {
            new[] {0,1},
            new[] {1,0},
            new[] {1,2},
            new[] {2,1},
        };
        private MindSweeper MindSweeper = new MindSweeper();
        public TicTacToeBot(string name, bool landmindGame = false)
        {
            host = new();
            host.BaseAddress = new("https://gamemanager20230712151202.azurewebsites.net/");
            //host.BaseAddress = new("https://localhost:7046/");
            this.name = name;
            this.isLandmindGame = landmindGame;
        }

        private async Task<TicTacToeGame?> InterpretResponse(HttpResponseMessage response)
        {
            var s = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            TicTacToeGame? game = JsonSerializer.Deserialize<TicTacToeGame>(s, 
                new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return game;
        }

        private Landmine? PlaceLandmine()
        {
            if (isLandmindGame)
            {
                Random rand = new Random(618);
                int[] coord = new int[] { 1, 1 };
                if (rand.Next(10) > 9)
                {
                    MindSweeper.ourLandmineStrategy = LPref.Corner;
                    int i = rand.Next(4);
                    coord = corners[i];
                }
                return new Landmine { Coordinate = coord };
            }
            return new Landmine();
        }

        public async Task<TicTacToeGame?> GetGameStatus(string roomCode)
        {
            var temp = await host.GetAsync("GetGameStatus/" + roomCode);
            Console.WriteLine("GetGameStatus");
            return await InterpretResponse(temp);
        }

        public async Task<TicTacToeGame> GenerateGame()
        {
            Landmine? l = PlaceLandmine();
            var temp = await host.PostAsJsonAsync("GenerateGame/" + name, l);
            Console.WriteLine("GenerateGame");
            return await InterpretResponse(temp);
        }

        public async Task<TicTacToeGame?> JoinGame(string roomCode)
        {
            Landmine? l = PlaceLandmine();
            var temp = await host.PutAsJsonAsync($"JoinGame/{roomCode}/{name}", l);
            Console.WriteLine("JoinGame");
            return await InterpretResponse(temp);
        }

        public async Task<TicTacToeGame?> SetMove(string roomCode, PlayerMove move)
        {
            var temp = await host.PostAsJsonAsync($"SetMove/{roomCode}/{name}", move);
            Console.WriteLine("SetMove");
            return await InterpretResponse(temp);
        }

        public async Task<TicTacToeGame?> Continue(string roomCode)
        {
            Landmine? l = PlaceLandmine();
            if (madeFirstMove && !isLandmindGame)
            {
                host.PostAsJsonAsync($"Continue/{roomCode}/{name}", l);
            }
            var temp = await host.PostAsJsonAsync($"Continue/{roomCode}/{name}", l);
            Console.WriteLine("Continue");
            madeFirstMove = false;
            return await InterpretResponse(temp);
        }

        public async Task MakeMove(TicTacToeGame game)
        {
            char gamepiece = 'O';
            if (game.PlayerXName == name)
            {
                gamepiece = 'X';
            }
            PlayerMove m = GetNewMove(game.GameBoard, gamepiece);
            await SetMove(game.RoomCode, m);
        }

        private PlayerMove GetNewMove(char[][] GameBoard, char gamePiece)
        {
            PlayerMove playerMove = new();
            if (isLandmindGame)
            {
                playerMove.Coordinate =
                    TakeOppositeCorner(GameBoard, gamePiece)
                    ?? TakeRandomCorner(GameBoard, gamePiece)
                    ?? TakeRandomSide(GameBoard, gamePiece)
                    ?? MakeRandomMove(GameBoard);
            }
            else
            {
                playerMove.Coordinate = 
                    MakeFirstMove(GameBoard, gamePiece) 
                    ?? MakeWinningMove(GameBoard, gamePiece)
                    ?? BlockWinningMove(GameBoard, gamePiece)
                    ?? TakeCenter(GameBoard, gamePiece)
                    ?? BlockCornerSplitWithSide(GameBoard, gamePiece)
                    ?? SetupDouble(GameBoard, gamePiece)
                    ?? TakeRandomCorner(GameBoard, gamePiece)
                    ?? TakeRandomSide(GameBoard, gamePiece)
                    ?? MakeRandomMove(GameBoard);
            }
            playerMove.DoesTauntOpponent = true;
            return playerMove;
        }

        private int[]? TakeOppositeCorner(char[][] GameBoard, char gamePiece)
        {
            foreach (int[] corner in corners)
            {
                if(GameBoard[corner[0]][corner[1]] == gamePiece)
                {
                    int cornerCoord1 = Math.Abs(corner[0] + 2 - 4);
                    int cornerCoord2 = Math.Abs(corner[0] + 2 - 4);
                    if(GameBoard[cornerCoord1][cornerCoord2] == '\0')
                    {
                        Console.WriteLine("TakeOppositeCorner");
                        return new int[] { cornerCoord1, cornerCoord2 };
                    }
                }
            }
            return null;
        }
        private int[]? MakeFirstMove(char[][] GameBoard, char gamePiece)
        {

            //if first move
            if (GameBoard.All(c => c.All(g => g == '\0')))
            {
                madeFirstMove = true;
                Console.WriteLine("MakeFirstMove");
                return TakeRandomCorner(GameBoard, gamePiece);
            }
            return null;
        }

        private int[]? MakeWinningMove(char[][] GameBoard, char gamePiece)
        {
            foreach (int[][] set in winningSets)
            {
                int ourPieces = 0;
                int[]? empty = null;
                foreach (int[] point in set)
                {
                    char val = GameBoard[point[0]][point[1]];
                    if (val == '\0')
                    {
                        empty = point;
                    }
                    else if (val == gamePiece)
                    {
                        ourPieces++;
                    }
                }
                if (ourPieces == 2 && empty != null)
                {
                    Console.WriteLine("MakeWinningMove");
                    return empty;
                }
            }
            return null;
        }

        private int[]? BlockWinningMove(char[][] GameBoard, char gamePiece)
        {
            foreach (int[][] set in winningSets)
            {
                int ourPieces = 0;
                int[]? empty = null;
                foreach (int[] point in set)
                {
                    char val = GameBoard[point[0]][point[1]];
                    if (val == '\0')
                    {
                        empty = point;
                    }
                    else if (val != gamePiece)
                    {
                        ourPieces++;
                    }
                }
                if (ourPieces == 2 && empty != null)
                {
                    Console.WriteLine("BlockWinningMove");
                    return empty;
                }
            }
            return null;
        }

        private int[]? SetupDouble(char[][] GameBoard, char gamePiece)
        {
            if (isLandmindGame)
            {
                return null;
            }
            char opponentGamepiece = gamePiece == 'X' ? 'O' : 'X';
            List<int[][]> availableWins = new List<int[][]>();
            foreach (int[][] winningSet in winningSets)
            {
                if (winningSet.All(p => GameBoard[p[0]][p[1]] != opponentGamepiece))
                {
                    availableWins.Add(winningSet);
                }
            }

            Dictionary<int[], int> emptyPointScores = new Dictionary<int[], int>();
            foreach (int[][] set in availableWins)
            {
                foreach (int[] point in set)
                {
                    if (GameBoard[point[0]][point[1]] == '\0')
                    {
                        if (emptyPointScores.ContainsKey(point))
                        {
                            emptyPointScores[point]++;
                        }
                        else
                        {
                            emptyPointScores.Add(point, 1);
                        }
                    }
                }
            }

            return emptyPointScores.Where(kv => kv.Value > 1).Select(kv => kv.Key).FirstOrDefault();
        }
        private int[] TakeCenter(char[][] GameBoard, char gamePiece)
        {//take center if they start and take anything but center
            if (GameBoard[1][1] == '\0' && !isLandmindGame)
            {
                Console.WriteLine("TakeCenter");
                return new int[] { 1, 1 };
            }
            return null;
        }
        private int[]? BlockCornerSplitWithSide(char[][] GameBoard, char gamePiece)
        {
            char theirPiece = gamePiece == 'X' ? 'O' : 'X';
            // if they took opposing corners and we took center, take side
            if (AreAllEqualTo(GameBoard, sides, '\0') && GameBoard[1][1] == gamePiece &&
                ((GameBoard[0][0] == theirPiece && GameBoard[2][2] == theirPiece) ||
                (GameBoard[0][2] == theirPiece && GameBoard[2][0] == theirPiece)
                ))
            {
                Console.WriteLine("BlockCornerSplitWithSide");
                return TakeRandomSide(GameBoard, gamePiece);
            }
            return null;
        }
        private int[] MakeRandomMove(char[][] GameBoard)
        {
            List<int[]> possibleCoordinates = new();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (GameBoard[i][j] == '\0')
                    {
                        possibleCoordinates.Add(new int[] { i, j });
                    }
                }
            }
            //This is where we select and send the coordinate for our next move
            Random rnd = new(DateTime.UtcNow.Microsecond);
            int num = rnd.Next(possibleCoordinates.Count);

            Console.WriteLine("MakeRandomMove");
            return possibleCoordinates[num];
        }
        private int[]? TakeRandomCorner(char[][] GameBoard, char gamePiece)
        {
            List<int[]> possibleCoordinates = new();
            foreach (var corner in corners)
            {
                if (GameBoard[corner[0]][corner[1]] == '\0')
                {
                    possibleCoordinates.Add(new int[] { corner[0], corner[1] });
                }
            }

            if (!possibleCoordinates.Any())
            {
                return null;
            }
            //This is where we select and send the coordinate for our next move
            Random rnd = new(DateTime.UtcNow.Microsecond);
            int num = rnd.Next(possibleCoordinates.Count);

            Console.WriteLine("TakeRandomCorner");
            return possibleCoordinates[num];
        }

        private int[]? TakeRandomSide(char[][] GameBoard, char gamePiece)
        {
            List<int[]> possibleCoordinates = new();
            foreach (var side in sides)
            {
                if (GameBoard[side[0]][side[1]] == '\0')
                {
                    possibleCoordinates.Add(new int[] { side[0], side[1] });
                }
            }

            if (!possibleCoordinates.Any())
            {
                return null;
            }

            //This is where we select and send the coordinate for our next move
            Random rnd = new(DateTime.UtcNow.Microsecond);
            int num = rnd.Next(possibleCoordinates.Count);

            Console.WriteLine("TakeRandomSide");
            return possibleCoordinates[num];
        }

        private bool AreAllEqualTo(char[][] GameBoard, IEnumerable<int[]>points, char gamepiece)
        {
            return points.All(g => GameBoard[g[0]][g[1]] == gamepiece);
        }
    }
}
