//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TP.ConcurrentProgramming.Presentation.Model;  
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using static System.Net.Mime.MediaTypeNames;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        public ICommand ClearCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand CloseCommand { get; }

        private InlineCommand _startCommand;
        private InlineCommand _clearCommand;

        private int _numberOfBalls;
        public int NumberOfBalls
        {
            get => _numberOfBalls;
            set
            {
                _numberOfBalls = value;
                _startCommand?.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

        public Action CloseAction { get; set; }

        private IDisposable Observer;
        private ModelAbstractApi ModelLayer;
        private bool Disposed = false;

        public MainWindowViewModel() : this(ModelAbstractApi.CreateModel()) { }

        public MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        {
            ModelLayer = modelLayerAPI ?? ModelAbstractApi.CreateModel();
            Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));

            _clearCommand = new InlineCommand(_ => ClearBalls(), _ => true);
            ClearCommand = _clearCommand;

            _startCommand = new InlineCommand(
                _ => Start(NumberOfBalls),
                _ => NumberOfBalls > 0 && NumberOfBalls <= 20
            );
            StartCommand = _startCommand;

            CloseCommand = new InlineCommand(_ => CloseAction?.Invoke(), _ => true);
        }

        public void Start(int numberOfBalls)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            ModelLayer.Start(numberOfBalls);
        }

        public void ClearBalls()
        {
            ModelLayer.Clear();
            Balls.Clear();
            _clearCommand?.RaiseCanExecuteChanged();
        }

        public void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Balls.Clear();
                    Observer?.Dispose();
                    ModelLayer.Dispose();
                }
                Disposed = true;
            }
        }
    }

    public class InlineCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public InlineCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (_ => true);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
