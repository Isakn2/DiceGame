using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Spectre.Console;

namespace NonTransitiveDiceGame
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: No dice configurations provided.");
                Console.WriteLine("Example: dotnet run -- 2,2,4,4,9,9 6,8,1,1,8,6 7,5,3,7,5,3");
                return;
            }

            if (args.Length < 3)
            {
                Console.WriteLine("Error: You must specify at least 3 dice configurations.");
                Console.WriteLine("Example: dotnet run -- 2,2,4,4,9,9 6,8,1,1,8,6 7,5,3,7,5,3");
                return;
            }

            try
            {
                List<Dice> diceList = new List<Dice>();
                foreach (var arg in args)
                {
                    try
                    {
                        diceList.Add(new Dice(arg));
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($"Error in dice configuration '{arg}': {ex.Message}");
                        return;
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Error in dice configuration '{arg}': Non-integer value detected.");
                        return;
                    }
                }

                // Check for duplicate dice configurations
                var duplicateDice = diceList.GroupBy(d => string.Join(",", d.Faces))
                                            .Where(g => g.Count() > 1)
                                            .Select(g => g.Key)
                                            .ToList();
                if (duplicateDice.Any())
                {
                    Console.WriteLine("Error: Duplicate dice configurations detected:");
                    foreach (var dice in duplicateDice)
                    {
                        Console.WriteLine($"- {dice}");
                    }
                    return;
                }

                Game game = new Game(diceList);
                game.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    class Dice
    {
        public int[] Faces { get; private set; }

        public Dice(string input)
        {
            // Check if the input is null or empty
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Dice configuration cannot be null or empty.");
            }

            // Split the input by commas and parse into integers
            string[] faceStrings = input.Split(',');
            Faces = new int[faceStrings.Length];

            for (int i = 0; i < faceStrings.Length; i++)
            {
                // Try to parse each face value
                if (!int.TryParse(faceStrings[i], out int faceValue))
                {
                    throw new ArgumentException($"Invalid face value: '{faceStrings[i]}'. Dice faces must be integers.");
                }

                // Check if the face value is positive
                if (faceValue <= 0)
                {
                    throw new ArgumentException($"Invalid face value: '{faceStrings[i]}'. Dice faces must be positive integers.");
                }

                Faces[i] = faceValue;
            }

            // Check the number of faces
            if (Faces.Length < 4 || Faces.Length > 20)
            {
                throw new ArgumentException($"Invalid number of faces: {Faces.Length}. Dice must have between 4 and 20 faces.");
            }
        }

        public int Roll(FairRandomGenerator fairRandom)
        {
            return Faces[fairRandom.Generate(0, Faces.Length - 1)];
        }
    }

    class FairRandomGenerator
    {
        public int Generate(int min, int max)
        {
            byte[] key = new byte[32];
            RandomNumberGenerator.Fill(key);
            int computerValue = RandomNumberGenerator.GetInt32(min, max + 1);
            string hmac = ComputeHMAC(key, computerValue);

            Console.WriteLine($"HMAC: {hmac}");
            Console.WriteLine($"Select a number between {min} and {max}:");

            int userValue;
            while (!int.TryParse(Console.ReadLine(), out userValue) || userValue < min || userValue > max)
            {
                Console.WriteLine("Invalid input. Enter a number in the given range.");
            }

            int result = (computerValue + userValue) % (max - min + 1);
            Console.WriteLine($"Computer value: {computerValue} (Key: {Convert.ToBase64String(key)})");
            return result;
        }

        private string ComputeHMAC(byte[] key, int message)
        {
            using (var hmac = new HMACSHA256(key))
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message.ToString());
                byte[] hash = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    class ProbabilityCalculator
    {
        public static double CalculateWinProbability(Dice dice1, Dice dice2)
        {
            int wins = 0;
            int total = dice1.Faces.Length * dice2.Faces.Length;

            foreach (var face1 in dice1.Faces)
            {
                foreach (var face2 in dice2.Faces)
                {
                    if (face1 > face2) wins++;
                }
            }

            return (double)wins / total;
        }
    }

   class HelpTableRenderer
    {
        public static void Render(List<Dice> diceList)
        {
            // Create a new table with custom styling
            var table = new Table()
                .Title("Probability of the win for the user")
                .BorderColor(Color.DarkBlue) // Dark border for contrast
                .Border(TableBorder.Square) // Solid border style
                .Centered();

            // Add columns with dark text colors
            table.AddColumn(new TableColumn("[bold darkblue]User dice \\ Computer dice[/]").Centered());
            foreach (var dice in diceList)
            {
                table.AddColumn(new TableColumn($"[bold darkblue]{string.Join(",", dice.Faces)}[/]").Centered());
            }

            // Add rows with dark text colors
            for (int i = 0; i < diceList.Count; i++)
            {
                var row = new List<string>();
                row.Add($"[bold darkblue]{string.Join(",", diceList[i].Faces)}[/]"); // Row header

                for (int j = 0; j < diceList.Count; j++)
                {
                    if (i == j)
                    {
                        row.Add("[bold darkred]- (0.3333)[/]");
                    }
                    else
                    {
                        double probability = ProbabilityCalculator.CalculateWinProbability(diceList[i], diceList[j]);
                        row.Add($"[bold black]{probability:F4}[/]");
                    }
                }

                table.AddRow(row.ToArray());
            }

            // Render the table
            AnsiConsole.Write(table);
        }
    }

    class Game
    {
        private List<Dice> DiceList;
        private FairRandomGenerator FairRandom;
        public Game(List<Dice> diceList)
        {
            DiceList = diceList;
            FairRandom = new FairRandomGenerator();
        }

        public void Start()
        {
            Console.WriteLine("Let's determine who makes the first move.");
            int firstMove = FairRandom.Generate(0, 1);
            Console.WriteLine($"First player determined by fair randomness: {firstMove}");

            Console.WriteLine("Choose your dice:");
            for (int i = 0; i < DiceList.Count; i++)
            {
                Console.WriteLine($"{i} - {string.Join(",", DiceList[i].Faces)}");
            }

            Console.WriteLine("X - Exit");
            Console.WriteLine("? - Help");
            Console.WriteLine("HINT: USE: '?' to get the probability table");

            int playerChoice;
            while (true)
            {
                Console.Write("Your selection: ");
                string input = Console.ReadLine() ?? string.Empty;

                if (input?.ToUpper() == "X")
                {
                    Console.WriteLine("Exiting the game...");
                    return;
                }
                else if (input == "?")
                {
                    HelpTableRenderer.Render(DiceList);
                    continue;
                }

                if (int.TryParse(input, out playerChoice) && playerChoice >= 0 && playerChoice < DiceList.Count)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid selection. Try again.");
                }
            }

            List<int> availableChoices = Enumerable.Range(0, DiceList.Count).Where(i => i != playerChoice).ToList();
            int computerChoice = availableChoices[new Random().Next(availableChoices.Count)];

            Console.WriteLine($"Computer chooses: {string.Join(",", DiceList[computerChoice].Faces)}");

            int userRoll = DiceList[playerChoice].Roll(FairRandom);
            int computerRoll = DiceList[computerChoice].Roll(FairRandom);

            Console.WriteLine($"Your roll: {userRoll}");
            Console.WriteLine($"Computer roll: {computerRoll}");

            if (userRoll > computerRoll)
            {
                Console.WriteLine("You win!");
            }
            else if (userRoll < computerRoll)
            {
                Console.WriteLine("Computer wins!");
            }
            else
            {
                Console.WriteLine("It's a tie!");
            }
        }
    }
}