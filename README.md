# Non-Transitive Dice Game

## Overview
This project implements a **Non-Transitive Dice Game** in C#. The game allows users to play with a set of dice configurations, where the dice exhibit non-transitive properties (e.g., Dice A beats Dice B, Dice B beats Dice C, but Dice C beats Dice A). The game includes features like fair random number generation, probability tables, and user-friendly error handling.

---

## Features
- **Fair Random Number Generation**: Uses cryptographic methods to ensure fairness.
- **Probability Table**: Displays the probability of one dice winning against another.
- **Error Handling**: Provides clear error messages for invalid inputs.
- **Interactive Gameplay**: Allows users to select dice, roll them, and determine the winner.

---

## Requirements
- **.NET SDK**: Ensure you have the .NET SDK installed. Download it from [here](https://dotnet.microsoft.com/download).
- **Spectre.Console**: The project uses the Spectre.Console library for rendering tables. It will be installed automatically via NuGet.

---

## How to Run the Game

### 1. Clone the Repository
```bash
git clone https://github.com/your-username/non-transitive-dice-game.git
cd non-transitive-dice-game
```

### 2. Run the Game
Use the `dotnet run` command to start the game. Provide at least 3 dice configurations as command-line arguments.

#### Example Commands:
  ```bash
  dotnet run -- 2,2,4,4,9,9 1,1,6,6,8,8 3,3,5,5,7,7
  ```

- **Help Table**:
  Run the game with 3 dice and enter `?` when prompted to display the probability table.

- **Incorrect Inputs**:
  - No dice:
    ```bash
    dotnet run --
    ```
  - 2 dice:
    ```bash
    dotnet run -- 2,2,4,4,9,9 6,8,1,1,8,6
    ```
  - Invalid number of sides:
    ```bash
    dotnet run -- 2,2,4 6,8,1,1,8,6 7,5,3,7,5,3
    ```
  - Non-integer value:
    ```bash
    dotnet run -- 2,2,4,four,9,9 6,8,1,1,8,6 7,5,3,7,5,3
    ```

## Acknowledgments
- **Spectre.Console**: Used for rendering beautiful tables in the console.
- **.NET**: The framework used to build this application.

---

## Contact
For questions or feedback, feel free to reach out:
- **Name**: Isaac Silva
- **Email**: isak.silva81@gmail.com
- **GitHub**: [Isakn2](https://github.com/Isakn2