//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor
        internal Ball(Vector initialPosition, Vector initialVelocity, int mass)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            Mass = mass;
        }
        #endregion ctor

        #region IBall
        public event EventHandler<IVector>? NewPositionNotification;
        public IVector Velocity { get; set; }
        public int Mass { get; }
        #endregion IBall

        public Vector Position;

        #region private
        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        // Zmieniona metoda Move – tylko przesuwa kulkę bez logiki kolizji ze ścianą:
        internal void Move(Vector delta)
        {
            Position = new Vector(Position.x + delta.x, Position.y + delta.y);
            Velocity = delta;
            RaiseNewPositionChangeNotification();
        }
        #endregion private
    }
}