using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Ellipse = Windows.UI.Xaml.Shapes.Ellipse;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudioCreation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Scenario7_SpatialAudio : Page
    {
        private Ellipse emitter_circle;
        private RelativePanel audio_canvas;

        public Scenario7_SpatialAudio()
        {
            this.InitializeComponent();

            // Visualization object references
            emitter_circle = FindName("Emitter") as Ellipse;
            if(emitter_circle == null)
            {
                throw new NullReferenceException("Emitter ellipse geometry not found");
            }
            audio_canvas = FindName("AudioCanvas") as RelativePanel;
            if (audio_canvas == null)
            {
                throw new NullReferenceException("Audio Canvas not found");
            }
        }

        private void OnUp(object sender, RoutedEventArgs e)
        {
            var margin = emitter_circle.Margin;
            margin.Bottom = margin.Bottom + 10;
            emitter_circle.Margin = margin;
        }

        private void OnDown(object sender, RoutedEventArgs e)
        {
            var margin = emitter_circle.Margin;
            margin.Bottom = margin.Bottom - 10;
            emitter_circle.Margin = margin;
        }

        private void OnRight(object sender, RoutedEventArgs e)
        {
            var margin = emitter_circle.Margin;
            margin.Right = margin.Right - 10;
            emitter_circle.Margin = margin;
        }

        private void OnLeft(object sender, RoutedEventArgs e)
        {
            var margin = emitter_circle.Margin;
            margin.Right = margin.Right + 10;
            emitter_circle.Margin = margin;
        }
    }
}
