using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace MicrowaveApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.  
    /// </summary>
    public partial class MainWindow
    {
        // Need te be done at the start of the application.
        public MainWindow()
        {
            InitializeComponent();
            FillRecipesBar();
        }

        private string _unconvertedTime = "00:00";
        // changes the unconverted time to a time in seconds.
        private int ConvertedTimeInSeconds()
        {
            double time = 0;
            try
            {
                string[] array = _unconvertedTime.Split(':');
                time = Convert.ToInt64(array[0]) * 60;
                time = time + Convert.ToInt64(array[1]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return Convert.ToInt32(time);
        }
        private int _counter;
        private Boolean _door;
        private Boolean _power;
        private Boolean _started;

        // Sends all the buttons to the right. 
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            // Converting sender to an object that can be used.
            Button b = (Button)sender;
            string s = b.Name;
            string a = b.Content.ToString();

            switch (s)
            {
                case "btnDoor":
                    DoorToggle();
                    break;
                case "btnPower":
                    PowerToggle();
                    break;
                case "btnReset":
                    ResetMicrowave();
                    break;
                case "btnRecipe":
                    RecipeWindow();
                    break;
                case "btnStartStop":
                    UseMicrowave();
                    break;
                case "btn0":
                case "btn1":
                case "btn2":
                case "btn3":
                case "btn4":
                case "btn5":
                case "btn6":
                case "btn7":
                case "btn8":
                case "btn9":
                    ChangeTimeNumbers(Convert.ToChar(a));
                    break;
            }
        }
        // Sets the unconverted time in de preferred format.
        private void ChangeTimeNumbers(char number)
        {
            
            char[] numbers = _unconvertedTime.ToCharArray();
            switch (_counter)
            {
                case 0:
                    numbers[4] = number;
                    break;
                case 1:
                    numbers[3] = number;
                    break;
                case 2:
                    numbers[1] = number;
                    break;
                case 3:
                    numbers[0] = number;
                    break;
            }

            _counter++;
            if (_counter >= 4)
            {
                numbers[4] = '0';
                numbers[3] = '0';
                numbers[1] = '0';
                numbers[0] = '6';
            }
            _unconvertedTime = "";
            for (int i = 0; i <= 4; i++)
            {
                _unconvertedTime = _unconvertedTime + numbers[i];
            }
            SetTime();
        }

        // Starts or stops the microwave.
        private void UseMicrowave()
        {
            if (_power && _door == false)
            {
                if (_started)
                {
                    StopMicrowave();
                }
                else
                {
                    StartMicrowave();
                }
            }
            else if (_power == false)
            {
                ResetMicrowave();
            }
            else if (_door)
            {
                StopMicrowave();
            }

        }
        // Resets the timer and microwave.
        private void ResetMicrowave()
        {
            StopMicrowave();
            _unconvertedTime = "00:00";
            _counter = 0;
            SetTime();
        }

        // Stops the microwave.
        private void StopMicrowave()
        {
            _started = false;
            if (_door == false)
                DoorColor("#FFF4F4F5", "#FFD4C8C8");
            btnStartStop.Content = "Start";
            _unconvertedTime = tbTimer.Text;
            try
            {
                _timer.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
        // Starts the microwave.
        private void StartMicrowave()
        {
            _started = true;
            DoorColor("#FFF4F4F5", "#FF4500");
            btnStartStop.Content = "Stop";
            Countdown();
        }

        // Sets the power on or off, light and timer. 
        private void PowerToggle()
        {
            if (_power == false)
            {
                _power = true;
                SetTime();
                if (_door)
                {
                    DoorColor("#FFEE4B", "#FFF4F4F5");
                }
            }
            else
            {
                _power = false;
                tbTimer.Text = "";
                if (_door)
                {
                    DoorColor("#FFD4C8C8", "#FFF4F4F5");
                }
                UseMicrowave();
            }
        }
        // Writes out the unconverted time
        private void SetTime()
        {
            if(_power)
                tbTimer.Text = _unconvertedTime;
        }

        // Makes the door appearance open or closed.
        private void DoorToggle()
        {
            if (_power)
            {
                if (_door == false)
                {
                    _door = true;
                    DoorColor("#FFEE4B", "#FFF4F4F5");
                    DoorOpen();
                    UseMicrowave();
                }
                else
                {
                    _door = false;
                    DoorColor("#FFF4F4F5", "#FFD4C8C8");
                    DoorClose();
                }
            }
            else
            {
                if (_door == false)
                {
                    _door = true;
                    DoorColor("#FFD4C8C8", "#FFF4F4F5");
                    DoorOpen();
                }
                else
                {
                    _door = false;
                    DoorColor("#FFF4F4F5", "#FFD4C8C8");
                    DoorClose();
                }
            }
        }

        // Open a new window for recipe GUI.
        private void RecipeWindow()
        {
            RecipesWindow recipe = new RecipesWindow();

            //When the new window gives an event for RecipeUpdate, refill the recipe bar with all existing recipes.
            recipe.RecipeUpdate += FillRecipesBar; 
            recipe.Show();
        }


        // Fills the combobox with the recipe name from the json file.
        private void FillRecipesBar()
        {
            cbRecipe.Items.Clear();
            using (StreamReader r = new StreamReader("recipes.json"))
            {
                string json = r.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                foreach (var item in array)
                {
                    cbRecipe.Items.Add(item.name);
                }
            }
        }

        // When the function doorColor gets two variables change color from door.
        private void DoorColor(string hexCode1, string hexCode2)
        {
            rDoor.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(hexCode1));
            rWindow.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(hexCode2));
            rWindow.Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom("#000"));
        }

        // Changes the time on the timer to the selected recipe time.
        private void CbRecipe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (StreamReader r = new StreamReader("recipes.json"))
            {
                string json = r.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                string target = cbRecipe.SelectedItem.ToString();
                foreach (var item in array)
                {
                    if (item.name == target)
                    {
                        _unconvertedTime = item.time;
                    }
                }
                
            }
            SetTime();
        }

        DispatcherTimer _timer;
        TimeSpan _time;
        // Counts down in seconds.
        public void Countdown()
        {
            _time = TimeSpan.FromSeconds(ConvertedTimeInSeconds());

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                tbTimer.Text = _time.ToString(@"mm\:ss");
                if (_time == TimeSpan.Zero)
                {
                    _timer.Stop();
                    StopMicrowave();
                }
                _time = _time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);

            _timer.Start();
        }
        // Opens the door.
        private void DoorOpen()
        {
            rWindow.Height = 351;
            rWindow.Width = 55;
            rWindow.Margin = new Thickness(13, 20, 0, 0);

        }
        // Closes the door.
        private void DoorClose()
        {
            rWindow.Height = 250;
            rWindow.Width = 450;
            rWindow.Margin = new Thickness(71, 71, 0, 0);
        }
    }
}
