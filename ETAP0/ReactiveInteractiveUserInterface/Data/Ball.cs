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

    internal Ball(Vector initialPosition, Vector initialVelocity)
    {
      Position = initialPosition;
      Velocity = initialVelocity;
    }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;

    public IVector Velocity { get; set; }

    #endregion IBall

    public Vector Position;

    #region private

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

        //modyfikacja całego move, aby piłki mogły się odbijać od ścianek
        internal void Move(Vector delta)
        {
            double newX = Position.x + delta.x;
            double newY = Position.y + delta.y;

            // Sprawdź granice i odwróć prędkość przy kolizji
            if (newX < 0 || newX > 375)
            {
                delta = delta with { x = -delta.x }; // Odbicie w osi X
            }
            if (newY < 0 || newY > 395)
            {
                delta = delta with { y = -delta.y }; // Odbicie w osi Y
            }

            // Zastosuj nową pozycję
            Position = new Vector(Position.x + delta.x, Position.y + delta.y);
            Velocity = delta; // Aktualizuj prędkość po odbiciu
            RaiseNewPositionChangeNotification();
        }
        #endregion private
    }
}