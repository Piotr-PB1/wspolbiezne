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
        private readonly ConcurrentBag<Ball> BallsList = new();
        private readonly List<Thread> BallThreads = new();
        private readonly List<bool> BallThreadFlags = new(); // Flagi do kontrolowania wątków
        private bool Disposed = false;

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
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

                Ball newBall = new(startingPosition, initialVector);
                BallsList.Add(newBall);
                upperLayerHandler(startingPosition, newBall);

                // Flaga kontrolująca działanie wątku
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
                        ball.Move(new Vector(ball.Velocity.x, ball.Velocity.y));
                        Console.WriteLine($"[Wątek {index}] Po ruchu: Pozycja=({ball.Position.x}, {ball.Position.y}), Prędkość=({ball.Velocity.x}, {ball.Velocity.y})");

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

        private void CheckCollisions(Ball ball)
        {
            foreach (var otherBall in BallsList)
            {
                if (ball != otherBall && CheckCollision(ball, otherBall))
                {
                    ResolveCollision(ball, otherBall);
                }
            }
        }

        private bool CheckCollision(Ball a, Ball b)
        {
            double dx = a.Position.x - b.Position.x;
            double dy = a.Position.y - b.Position.y;
            double distanceSquared = dx * dx + dy * dy;
            return distanceSquared < (20 * 20); // Średnica kulki to 20, więc promień 10
        }

        private void ResolveCollision(Ball a, Ball b)
        {
            double dx = b.Position.x - a.Position.x;
            double dy = b.Position.y - a.Position.y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance == 0) return;

            // Wektor normalny kolizji (od A do B)
            Vector n = new Vector(dx / distance, dy / distance);
            Vector t = new Vector(-n.y, n.x); // Wektor styczny

            // Rozkład prędkości na składowe
            double vA_n = a.Velocity.x * n.x + a.Velocity.y * n.y;
            double vA_t = a.Velocity.x * t.x + a.Velocity.y * t.y;

            double vB_n = b.Velocity.x * n.x + b.Velocity.y * n.y;
            double vB_t = b.Velocity.x * t.x + b.Velocity.y * t.y;

            // Uwzględnienie mas w obliczeniach odbicia sprężystego
            double vA_new_n = (vA_n * (a.Mass - b.Mass) + 2 * b.Mass * vB_n) / (a.Mass + b.Mass);
            double vB_new_n = (vB_n * (b.Mass - a.Mass) + 2 * a.Mass * vA_n) / (a.Mass + b.Mass);

            // Nowe wektory prędkości
            Vector newAVelocity = new Vector(
                vA_new_n * n.x + vA_t * t.x,
                vA_new_n * n.y + vA_t * t.y
            );

            Vector newBVelocity = new Vector(
                vB_new_n * n.x + vB_t * t.x,
                vB_new_n * n.y + vB_t * t.y
            );

            a.Velocity = newAVelocity;
            b.Velocity = newBVelocity;

            // Korekcja pozycji (opcjonalnie, aby uniknąć zakleszczenia)
            double overlap = 20 - distance; // 20 to średnica kulki
            if (overlap > 0)
            {
                double adjust = overlap * 0.5;
                a.Position = new Vector(a.Position.x - n.x * adjust, a.Position.y - n.y * adjust);
                b.Position = new Vector(b.Position.x + n.x * adjust, b.Position.y + n.y * adjust);
            }
        }

        public override void Clear()
        {
            for (int i = 0; i < BallThreadFlags.Count; i++)
            {
                BallThreadFlags[i] = false;
            }

            foreach (var thread in BallThreads)
            {
                if (thread.IsAlive)
                {
                    thread.Join();
                }
            }

            BallsList.Clear();
            BallThreads.Clear();
            BallThreadFlags.Clear();
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Clear();
                }
                Disposed = true;
            }
        }
    }
}