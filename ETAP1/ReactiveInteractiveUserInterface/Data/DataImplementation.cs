//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        private readonly Dictionary<Ball, TP.ConcurrentProgramming.BusinessLogic.Ball> ballMapping = new();
        private readonly ConcurrentBag<Ball> BallsList = new();
        private readonly List<Thread> BallThreads = new();
        private readonly List<bool> BallThreadFlags = new(); // Flagi do kontrolowania wątków
        private bool Disposed = false;

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            CheckObjectDisposed();
            CheckNumberOfBalls(numberOfBalls);
            CheckBallsList();

            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                int mass = 1; // Przykładowa waga kulki
                Vector startingPosition = new(random.Next(100, 400 - 100), random.Next(100, 400 - 100));
                Vector initialVector = new Vector(
                    (random.NextDouble() - 0.5) * 5, // Prędkość X (-2.5 do 2.5)
                    (random.NextDouble() - 0.5) * 5  // Prędkość Y (-2.5 do 2.5)
                );

                Ball newBall = new(startingPosition, initialVector, 1);
                BallsList.Add(newBall);
                upperLayerHandler(startingPosition, newBall);

                // Tworzenie powiązanego obiektu logiki
                var logicBall = new TP.ConcurrentProgramming.BusinessLogic.Ball(newBall);
                ballMapping[newBall] = logicBall;

                // Flaga kontrolująca działanie wątków
                BallThreadFlags.Add(true);

                // Tworzenie i uruchamianie wątku dla kulki
                int ballIndex = i; // Potrzebne do zamknięcia zmiennej w pętli
                Thread ballThread = new Thread(() => MoveBall(newBall, ballIndex));
                BallThreads.Add(ballThread);
                ballThread.Start();
            }
        }

        private void MoveBall(Ball ball, int index)
        {
            try
            {
                Console.WriteLine($"[Wątek {index}] Start wątku dla kulki o masie {ball.Mass} na pozycji ({ball.Position.x}, {ball.Position.y})");

                while (BallThreadFlags[index])
                {
                    lock (BallsList)
                    {
                        Console.WriteLine($"[Wątek {index}] Przed ruchem: Pozycja=({ball.Position.x}, {ball.Position.y}), Prędkość=({ball.Velocity.x}, {ball.Velocity.y})");

                        // Wywołanie ruchu kulki w warstwie danych (bez obsługi odbić od ścian)
                        ball.Move(new Vector(ball.Velocity.x, ball.Velocity.y));

                        // Wywołanie logiki odbicia od ścian z warstwy logiki
                        if (ballMapping.TryGetValue(ball, out TP.ConcurrentProgramming.BusinessLogic.Ball logicBall))
                        {
                            // Ustawiamy granice planszy (np. 0, 0, 375, 395)
                            logicBall.HandleWallCollision(0, 0, 375, 395);
                        }

                        Console.WriteLine($"[Wątek {index}] Po ruchu: Pozycja=({ball.Position.x}, {ball.Position.y}), Prędkość=({ball.Velocity.x}, {ball.Velocity.y})");

                        // Inne kolizje (np. z innymi kulkami)
                        CheckCollisions(ball);
                    }

                    Thread.Sleep(50); // Symulacja czasu ruchu
                }
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine($"[Wątek {index}] Wątek został przerwany.");
            }
            finally
            {
                Console.WriteLine($"[Wątek {index}] Wątek zakończył działanie.");
            }
        }

        public void CheckBallsList()
        {
            if (BallsList == null || !BallsList.Any())
                throw new InvalidOperationException("Lista kulek jest pusta lub niezainicjalizowana.");
        }

        public void CheckNumberOfBalls(int numberOfBalls)
        {
            if (numberOfBalls <= 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfBalls), "Liczba kulek musi być większa od zera.");
        }

        public void CheckObjectDisposed()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation), "Obiekt został już zniszczony.");
        }


    }
}