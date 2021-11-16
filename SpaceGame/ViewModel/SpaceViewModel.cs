using Microsoft.Win32;
using SpaceGame.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SpaceGame.ViewModel
{
    class SpaceViewModel : ViewModelBase
    {

        public SpaceModel model;
        public PlayerField player;

        public bool pausePressed = false;
        public bool simpleGameStart = true;

        public ObservableCollection<GameObjectField> fields;

        public DispatcherTimer targetMoveTimer;
        public DispatcherTimer targetCreateTimer;

        public Dictionary<Target, TargetField> targets;

        #region Delegates
        public DelegateCommand LeftKeyDown { get; set; }
        public DelegateCommand RightKeyDown { get; set; }
        public DelegateCommand PKeyDown { get; set; }
        public DelegateCommand StartButtonPressed { get; set; }
        public DelegateCommand PauseButtonPressed { get; set; }
        public DelegateCommand OpenFilePressed { get; set; }
        public DelegateCommand NewGameButtonPressed { get; set; }


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
            Fields = new ObservableCollection<GameObjectField>();

            targetMoveTimer = new DispatcherTimer();
            targetMoveTimer.Tick += targetMoveTimerTick;
            targetMoveTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);

            targetCreateTimer = new DispatcherTimer();
            targetCreateTimer.Tick += targetCreateTimerTick;
            targetCreateTimer.Interval = new TimeSpan(0, 0, 0, 1);
            //StartGame();

            //TargetField targetField = new TargetField(50, 50, 100, 100);
            //Fields.Add(targetField);
        }


        public void StartGame() {

            //Fields = new ObservableCollection<GameObjectField>();

            model = new SpaceModel(new Persistance.Persistance());

            Fields.Clear();
            //Fields.Remove(player);

            model.PlayerChanged += new EventHandler<Player>(PlayerChangedEventhandler);
            model.TargetChanged += new EventHandler<Target>(TargetChangedEventHandler);

            targets = new Dictionary<Target, TargetField>();

            pausePressed = false;

            targetMoveTimer.Start();

         
            targetCreateTimer.Start();

            model.StartGame(570, 760, 55, 48, 60, 50);
            
           


        }

        public void pause() {
            if (!pausePressed)
            {
                pausePressed = true;
                targetMoveTimer.Stop();
                targetCreateTimer.Stop();
                model.PlayerChanged -= new EventHandler<Player>(PlayerChangedEventhandler);
                model.TargetChanged -= new EventHandler<Target>(TargetChangedEventHandler);
            }
            else
            {
                pausePressed = false;
                targetMoveTimer.Start();
                targetCreateTimer.Start();
                model.PlayerChanged += new EventHandler<Player>(PlayerChangedEventhandler);
                model.TargetChanged += new EventHandler<Target>(TargetChangedEventHandler);
            }
        }

        #region Timer Tick Handlers

        public void targetMoveTimerTick(object sender, EventArgs e)
        {
            model.moveTargets();
        }

        public void targetCreateTimerTick(object sender, EventArgs e)
        {
            model.createTarget();
        }

        #endregion

        #region EventHandlers

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
                fields.Add(player);
            }
            else
            {
                player.PositionX = component.PositionX;
            }

        }

        #endregion

        #region Delegate Handlers
        public void LeftKeyDownHandler(object p)
        {
            model.moveLeft();
        }

        public void RightKeyDownHandler(object p)
        {
            model.moveRight();
        }

        public void PKeyDownHandler(object p){
            pause();
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
            string filePath = string.Empty;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {

                filePath = openFileDialog.FileName;
            }

            model = new SpaceModel(new Persistance.Persistance());
            model.FileName = filePath;
            if (filePath != null && filePath != "")
            {
                model.loadGame();
                StartGame();
            }
            else
            {
                simpleGameStart = true;
            }
        }

        #endregion

        #region Properties

        public ObservableCollection<GameObjectField> Fields
        {
            get { return fields; }
            set { fields = value; }
        }

        #endregion

    }
}
