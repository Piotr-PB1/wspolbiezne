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

namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
    #region ctor

    public DataImplementation()
    {
      MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(50)); //zamiana FromMilliseconds z 1000 na 50
    }

        #endregion ctor

        #region DataAbstractAPI
        public override void Clear()
        {
            BallsList.Clear();
            MoveTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            MoveTimer?.Change(60, 60);
            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                Vector startingPosition = new(random.Next(100, 400 - 100), random.Next(100, 400 - 100));
                //dodanie initialVecotr
                Vector initialVector = new Vector(
                    (random.NextDouble() - 0.5) * 5, // Prędkość X (-2.5 do 2.5)
                    (random.NextDouble() - 0.5) * 5  // Prędkość Y (-2.5 do 2.5)
                );
                
                Ball newBall = new(startingPosition, initialVector);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);
            }
        }

    #endregion DataAbstractAPI

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          MoveTimer.Dispose();
          BallsList.Clear();
        }
        Disposed = true;
      }
      else
        throw new ObjectDisposedException(nameof(DataImplementation));
    }

    public override void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

        #endregion IDisposable

        #region private

        private void CheckCollisions()
        {
            for (int i = 0; i < BallsList.Count; i++)
            {
                Ball ball1 = BallsList[i];
                for (int j = i + 1; j < BallsList.Count; j++)
                {
                    Ball ball2 = BallsList[j];
                    if (CheckCollision(ball1, ball2))
                    {
                        ResolveCollision(ball1, ball2);
                    }
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
            // Wektor normalny kolizji (od A do B)
            double dx = b.Position.x - a.Position.x;
            double dy = b.Position.y - a.Position.y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance == 0) return;

            Vector n = new Vector(dx / distance, dy / distance);
            Vector t = new Vector(-n.y, n.x); // Wektor styczny

            // Rozkład prędkości na składowe
            double vA_n = a.Velocity.x * n.x + a.Velocity.y * n.y;
            double vA_t = a.Velocity.x * t.x + a.Velocity.y * t.y;

            double vB_n = b.Velocity.x * n.x + b.Velocity.y * n.y;
            double vB_t = b.Velocity.x * t.x + b.Velocity.y * t.y;

            // Wymiana składowych normalnych (dla m1 = m2 = 1)
            double vA_new_n = vB_n;
            double vB_new_n = vA_n;

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



        //private bool disposedValue;
        private bool Disposed = false;

    private readonly Timer MoveTimer;
    private Random RandomGenerator = new();
    private List<Ball> BallsList = [];

        private void Move(object? x)
        {
            foreach (Ball item in BallsList)
            {
                item.Move(new Vector(item.Velocity.x, item.Velocity.y));
            }
            CheckCollisions();
            //item.Move(new Vector((RandomGenerator.NextDouble() - 0.5) * 10, (RandomGenerator.NextDouble() - 0.5) * 10));
        }

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
    {
      returnBallsList(BallsList);
    }

    [Conditional("DEBUG")]
    internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
    {
      returnNumberOfBalls(BallsList.Count);
    }

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}