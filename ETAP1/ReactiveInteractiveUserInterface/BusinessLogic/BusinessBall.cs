
//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private double _top;
        private double _left;

        public double Top
        {
            get => _top;
            set
            {
                _top = value;
                OnPropertyChanged();
            }
        }

        public double Left
        {
            get => _left;
            set
            {
                _left = value;
                OnPropertyChanged();
            }
        }

        public IVector Velocity { get; set; }
        public double Mass { get; set; } = 1; // Domyślna masa kulki
        public double Diameter { get; set; } = 20; // Domyślna średnica kulki

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Ball(Data.IBall ball)
        {
            ball.NewPositionNotification += RaisePositionChangeEvent;
        }

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }

        public event EventHandler<IPosition>? NewPositionNotification;

        // Nowa metoda do obliczania kolizji
        public (IVector newVelocity, IVector otherNewVelocity) CalculateCollision(Ball otherBall)
        {
            double dx = otherBall.Left - this.Left;
            double dy = otherBall.Top - this.Top;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance == 0) return (this.Velocity, otherBall.Velocity);

            // Wektor normalny kolizji (od A do B)
            Vector n = new Vector(dx / distance, dy / distance);
            Vector t = new Vector(-n.y, n.x); // Wektor styczny

            // Rozkład prędkości na składowe
            double vA_n = this.Velocity.x * n.x + this.Velocity.y * n.y;
            double vA_t = this.Velocity.x * t.x + this.Velocity.y * t.y;

            double vB_n = otherBall.Velocity.x * n.x + otherBall.Velocity.y * n.y;
            double vB_t = otherBall.Velocity.x * t.x + otherBall.Velocity.y * t.y;

            // Uwzględnienie mas w obliczeniach odbicia sprężystego
            double vA_new_n = (vA_n * (this.Mass - otherBall.Mass) + 2 * otherBall.Mass * vB_n) / (this.Mass + otherBall.Mass);
            double vB_new_n = (vB_n * (otherBall.Mass - this.Mass) + 2 * this.Mass * vA_n) / (this.Mass + otherBall.Mass);

            // Nowe wektory prędkości
            Vector newAVelocity = new Vector(
                vA_new_n * n.x + vA_t * t.x,
                vA_new_n * n.y + vA_t * t.y
            );

            Vector newBVelocity = new Vector(
                vB_new_n * n.x + vB_t * t.x,
                vB_new_n * n.y + vB_t * t.y
            );

            return (newAVelocity, newBVelocity);
        }
    }
}