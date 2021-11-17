using Microsoft.Win32;
using SpaceGame.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace SpaceGame.ViewModel
{
    class SpaceViewModel : ViewModelBase
    {

        #region Functions

        private String gameTime;
        private String lifeNumber;

        public SpaceModel model;
        public PlayerField player;

        public bool pausePressed = true;
        public bool simpleGameStart = true;

        public ObservableCollection<GameObjectField> fields;

        public DispatcherTimer targetMoveTimer;
        public DispatcherTimer targetCreateTimer;
        public DispatcherTimer sppedUpTimer;
        public DispatcherTimer ellapsedTimer;

        public Dictionary<Target, TargetField> targets;

        private int speedUpTimerTick = 1000;

        #endregion

        #region Delegates
        public DelegateCommand LeftKeyDown { get; set; }
        public DelegateCommand RightKeyDown { get; set; }
        public DelegateCommand PKeyDown { get; set; }
        public DelegateCommand StartButtonPressed { get; set; }
        public DelegateCommand PauseButtonPressed { get; set; }
        public DelegateCommand OpenFilePressed { get; set; }
        public DelegateCommand NewGameButtonPressed { get; set; }
        public DelegateCommand SaveGamePressed { get; set; }


        #endregion

        public SpaceViewModel()
        {
           
            LeftKeyDown = new DelegateCommand(LeftKeyDownHandler);
            RightKeyDown = new DelegateCommand(RightKeyDownHandler);
            PKeyDown = new DelegateCommand(PKeyDownHandler);
            StartButtonPressed = new DelegateCommand(StartButtonPressedHandler);
            NewGameButtonPressed = new DelegateCommand(StartButtonPressedHandler);
            PauseButtonPressed = new DelegateCommand(PauseButtonPressedHandler);
            OpenFilePressed = new DelegateCommand(OpenFilePressedHandler);
            SaveGamePressed = new DelegateCommand(SaveGameButtonClicked);
            Fields = new ObservableCollection<GameObjectField>();

            targetMoveTimer = new DispatcherTimer();
            targetMoveTimer.Tick += targetMoveTimerTick;
            targetMoveTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);

            targetCreateTimer = new DispatcherTimer();
            targetCreateTimer.Tick += targetCreateTimerTick;
            targetCreateTimer.Interval = new TimeSpan(0, 0, 0, 1);

            sppedUpTimer = new DispatcherTimer();
            sppedUpTimer.Tick += speedUpTimer;
            sppedUpTimer.Interval = new TimeSpan(0, 0, 0, 2);

            ellapsedTimer = new DispatcherTimer();
            ellapsedTimer.Tick += ellapsedTimeTick;
            ellapsedTimer.Interval = new TimeSpan(0, 0, 0, 1);

            model = new SpaceModel(new Persistance.Persistance());
            targets = new Dictionary<Target, TargetField>();

            GameTime = "0";
            LifeNumber = "0";

        }

        #region Functions

        public void StartGame() {

            pausePressed = false;
            speedUpTimerTick = 1000;

            Fields.Clear();

            if (simpleGameStart){
                model = new SpaceModel(new Persistance.Persistance());

                model.PlayerChanged += new EventHandler<Player>(PlayerChangedEventhandler);
                model.TargetChanged += new EventHandler<Target>(TargetChangedEventHandler);
                model.LifeChanged += new EventHandler<int>(LifeChangedHandler);
                model.GameOver += new EventHandler(GameOverEventHandler);

                targetCreateTimer.Interval = new TimeSpan(0, 0, 0, 1);

                model.StartGame(570, 760, 50, 43, 55, 45);
            }
            else {

                model.PlayerChanged += new EventHandler<Player>(PlayerChangedEventhandler);
                model.TargetChanged += new EventHandler<Target>(TargetChangedEventHandler);
                model.LifeChanged += new EventHandler<int>(LifeChangedHandler);
                model.GameOver += new EventHandler(GameOverEventHandler);

                targetCreateTimer.Interval = new TimeSpan(0, 0, 0, 0, model.TargetCreateTimer);
                speedUpTimerTick = model.TargetCreateTimer;

                model.initializeGame();
                simpleGameStart = true;
            }

            targetMoveTimer.Start();
            targetCreateTimer.Start();
            ellapsedTimer.Start();
            sppedUpTimer.Start();

        }

        public void pause() {
            if (!pausePressed)
            {
                pausePressed = true;
                targetMoveTimer.Stop();
                targetCreateTimer.Stop();
                ellapsedTimer.Stop();
                sppedUpTimer.Stop();
                model.PlayerChanged -= new EventHandler<Player>(PlayerChangedEventhandler);
                model.TargetChanged -= new EventHandler<Target>(TargetChangedEventHandler);
            }
            else
            {
                pausePressed = false;
                targetMoveTimer.Start();
                targetCreateTimer.Start();
                ellapsedTimer.Start();
                sppedUpTimer.Stop();
                model.PlayerChanged += new EventHandler<Player>(PlayerChangedEventhandler);
                model.TargetChanged += new EventHandler<Target>(TargetChangedEventHandler);
            }
        }

        #endregion

        #region Timer Tick Handlers

        public void targetMoveTimerTick(object sender, EventArgs e)
        {
            model.moveTargets();
            
        }

        public void ellapsedTimeTick(object sender, EventArgs e)
        {
            model.GameTimeSeconds += 1;
            GameTime = model.GameTimeSeconds.ToString();
            OnPropertyChanged("GameTime");
        }


        public void targetCreateTimerTick(object sender, EventArgs e)
        {
            model.createTarget();
        }

        public void speedUpTimer(object sender, EventArgs e)
        {
            //targetCreateTimer.Stop();
            if (speedUpTimerTick > 200) {
                speedUpTimerTick = speedUpTimerTick - 100;
                model.TargetCreateTimer = speedUpTimerTick;
            }
            targetCreateTimer.Interval = new TimeSpan(0, 0, 0, 0, speedUpTimerTick);
            //targetCreateTimer.Start();
        }

        #endregion

        #region Event Handlers

        
        public void GameOverEventHandler(object sender, EventArgs eventArgs)
        {
            pause();
            if (MessageBox.Show("A jateknak vége. Kezd új játékot?", "Sudoku", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
            }
            else {
                StartGame();
            }
        }
        public void LifeChangedHandler(object sender, int lifeNumber)
        {
            LifeNumber = lifeNumber.ToString();
            OnPropertyChanged("LifeNumber");
        }

        public void TargetChangedEventHandler(object sender, Target target) {
            if (targets.ContainsKey(target))
            {
                targets[target].PositionX = target.PositionX;
                targets[target].PositionY = target.PositionY;
                if (target.status == "DELETE")
                {
                    Fields.Remove(targets[target]);
                    targets.Remove(target);
                }
            }
            else
            {
                TargetField targetField = new TargetField(target.Width, target.Height, target.PositionX, target.PositionY);
                Fields.Add(targetField);
                targets.Add(target, targetField);
            }
        }

        public void PlayerChangedEventhandler(object sender, Player component)
        {
            if (!fields.Contains(player))
            {
                player = new PlayerField(component.Width, component.Height, component.PositionX, component.PositionY);
                Fields.Add(player);
            }
            else
            {
                player.PositionX = component.PositionX;
            }

        }

        #endregion

        #region Delegate Handlers

        private async void SaveGameButtonClicked(object p)
        {
            //TODO ellapsed time
            //model.GameTimeSeconds = (int)(stopper.ElapsedMilliseconds / 1000);

            pause();

            string gameStatusJson = model.saveGame();

            string fileText = gameStatusJson;

            SaveFileDialog odf = new SaveFileDialog();
            if (odf.ShowDialog() == true)
            {
                await File.WriteAllTextAsync(odf.FileName, gameStatusJson);
            }   

            pause();

        }

        public void LeftKeyDownHandler(object p)
        {
            if (!pausePressed) {
                model.moveLeft();
            }
        }

        public void RightKeyDownHandler(object p)
        {
            if (!pausePressed){
                model.moveRight();
            }
        }

        public void PKeyDownHandler(object p){
            //pause();
        }

        public void StartButtonPressedHandler(object p) {
            StartGame();
        }

        public void PauseButtonPressedHandler(object p)
        {
            pause();
        }

        public void OpenFilePressedHandler(object p)
        {
            if (!pausePressed) {
                pause();
            }

            simpleGameStart = false;

            string filePath = string.Empty;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }

            if (filePath != null && filePath != "")
            {
                model = new SpaceModel(new Persistance.Persistance());
                model.FileName = filePath;
                model.loadGame();
                StartGame();
            }
            else
            {
                simpleGameStart = true;
                pause();
            }
        }

        #endregion

        #region Properties

        public ObservableCollection<GameObjectField> Fields
        {
            get { return fields; }
            set { fields = value; }
        }

        public string GameTime
        {
            get { return gameTime; }
            set { gameTime = value; }
        }

        public string LifeNumber
        {
            get { return lifeNumber; }
            set { lifeNumber = value; }
        }

        #endregion

    }
}
