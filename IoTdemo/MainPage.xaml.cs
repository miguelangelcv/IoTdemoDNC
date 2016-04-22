using System;
using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace IoTdemo
{
    public sealed partial class MainPage : Page
    {
        // Variables para definir los puertos de los pines a usar
        const int PIN_LED = 5;
        const int PIN_BTN = 16;
        const int PIN_VERDE = 20;
        const int PIN_ROJO = 21;

        // Controladora GPIO
        GpioController gpio;

        // Pines a usar
        GpioPin pin;
        GpioPin pinPush;
        GpioPin pinVerde, pinRojo;

        // Describe los posibles valores de un pin (High y Low)
        GpioPinValue pinValue;

        // Temporizador
        DispatcherTimer timer;

        public MainPage()
        {
            this.InitializeComponent();

            /* Creamos el temporizador que vamos a usar para el led rojo
               cambie de estado cada 2 segundos */
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            /* Creamos el evento que se llevará a cabo al
               cumplirse el intervalo de tiempo que hemos indicado  */
            timer.Tick += Timer_Tick;

            // Inicializamos puertos y controladora GPIO
            InitGPIO();

            if (pin != null)
                timer.Start();
        }

        private void InitGPIO()
        {
            // Asignamos la controladora GPIO de la Raspberry Pi a gpio
            gpio = GpioController.GetDefault();

            // Asignamos el pin que va a usar el led rojo
            pin = gpio.OpenPin(PIN_LED);
            // Configuramos el puerto como salida
            pin.SetDriveMode(GpioPinDriveMode.Output);
            // Encendemos el led
            pinValue = GpioPinValue.Low;
            pin.Write(pinValue);

            // Asignamos los pines a usar por el led de dos colores
            pinVerde = gpio.OpenPin(PIN_VERDE);
            pinRojo = gpio.OpenPin(PIN_ROJO);
            // Establecemos ambos pines como salida
            pinVerde.SetDriveMode(GpioPinDriveMode.Output);
            pinRojo.SetDriveMode(GpioPinDriveMode.Output);
            // Led de dos colores apagado
            pinVerde.Write(GpioPinValue.Low);
            pinRojo.Write(GpioPinValue.Low);

            // Asignamos el puerto al pulsador
            pinPush = gpio.OpenPin(PIN_BTN);
            // Comprobamos si las resistencias de entrada del pulsador estan soportadas
            if (pinPush.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                pinPush.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                pinPush.SetDriveMode(GpioPinDriveMode.Input);

            // Set a debounce timeout to filter out switch bounce noise from a button press
            // Establezca un tiempo de espera para filtrar hacia el ruido de rebote del pulsador
            pinPush.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            // Creamos un evento para controlar cuando el voton cambia de estado (pulsado o no pulsado)
            pinPush.ValueChanged += buttonPin_ValueChanged;
        }


        private void Timer_Tick(object sender, object e)
        {
            // Cambiamos el estado del led
            if (pinValue == GpioPinValue.High)
            {
                pinValue = GpioPinValue.Low;
                pin.Write(pinValue);
            }
            else
            {
                pinValue = GpioPinValue.High;
                pin.Write(pinValue);
            }
        }

        private void buttonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            // Random para seleccionar un color al azar
            Random r = new Random();
            int color = r.Next(3);

            // Si el boton esta pulsado
            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                Debug.WriteLine("Button Pressed");
                // Establece el color según el valor obtenido en el random
                switch (color)
                {
                    default:
                        pinVerde.Write(GpioPinValue.Low);
                        pinRojo.Write(GpioPinValue.Low);
                        break;
                    case 0: // Verde
                        pinVerde.Write(GpioPinValue.High);
                        pinRojo.Write(GpioPinValue.Low);
                        break;
                    case 1: // Rojo
                        pinVerde.Write(GpioPinValue.Low);
                        pinRojo.Write(GpioPinValue.High);
                        break;
                    case 2:
                        pinVerde.Write(GpioPinValue.High);
                        pinRojo.Write(GpioPinValue.High);
                        break;
                }
            }
            else
            {
                Debug.WriteLine("Button Released");
                // Una vez soltado el boton, apagamos el led
                pinVerde.Write(GpioPinValue.Low);
                pinRojo.Write(GpioPinValue.Low);
            }
        }
    }
}