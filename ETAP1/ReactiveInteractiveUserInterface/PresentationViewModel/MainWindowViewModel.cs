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
using TP.ConcurrentProgramming.Presentation.Model;  
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using static System.Net.Mime.MediaTypeNames;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{


    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        public RelayCommand ClearCommand { get; }
        private int _numberOfBalls;
        public int NumberOfBalls
        {
            get => _numberOfBalls;
            set
            {
                _numberOfBalls = value;
                RaisePropertyChanged();
                StartCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand StartCommand { get; }

        public void ClearBalls()
        {
            ModelLayer.Clear();
            Balls.Clear();
            ClearCommand.RaiseCanExecuteChanged();
        }

        public RelayCommand CloseCommand { get; }

        public Action CloseAction { get; set; }

        public MainWindowViewModel() : this(ModelAbstractApi.CreateModel()) 
        {
        }

        public MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        {
            ClearCommand = new RelayCommand(
            () => ClearBalls()
            );

            CloseCommand = new RelayCommand(() => CloseAction?.Invoke());   //zamknij button

            ModelLayer = modelLayerAPI ?? ModelAbstractApi.CreateModel();
            Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
            StartCommand = new RelayCommand(
                () => Start(NumberOfBalls),
                () => NumberOfBalls > 0 && NumberOfBalls <= 20
            );

        }

        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

        public void Start(int numberOfBalls)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            ModelLayer.Start(numberOfBalls);
        }

        private IDisposable Observer = null;
        private ModelAbstractApi ModelLayer;
        private bool Disposed = false;

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

        public void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}