using SDKTemplate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
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
        private MainPage rootPage;

        private AudioGraph graph;
        private AudioFrameInputNode frameInputNode;
        private AudioDeviceOutputNode deviceOutput;

        // For spatial sound
        //the emitter shape. using omnidirectional in this example.
        private AudioNodeEmitterShape emitterShape;
        // the decay moddel. using natural for this example.
        private AudioNodeEmitterDecayModel decayModel = AudioNodeEmitterDecayModel.CreateNatural(.1, 1, 10, 100);
        private AudioNodeEmitterSettings settings = AudioNodeEmitterSettings.None;
        // The emitter.
        private AudioNodeEmitter emitter;

        public double theta = 0;
        private AudioDeviceOutputNode deviceOutputNode;

        private Ellipse emitter_circle;
        private RelativePanel audio_canvas;

        public Scenario7_SpatialAudio()
        {
            this.InitializeComponent();

            emitterShape = AudioNodeEmitterShape.CreateOmnidirectional();
            emitter = new AudioNodeEmitter(emitterShape, decayModel, settings);

            // Emitter initial position in physical units is (0, 20, 0) but visualization circle position is (0, 200) [see XAML]
            emitter.Position = new System.Numerics.Vector3(0, 20, 0);

            // Visualization object references
            emitter_circle = FindName("Emitter") as Ellipse;
            if (emitter_circle == null)
            {
                throw new NullReferenceException("Emitter ellipse geometry not found");
            }
            audio_canvas = FindName("AudioCanvas") as RelativePanel;
            if (audio_canvas == null)
            {
                throw new NullReferenceException("Audio Canvas not found");
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
            await CreateAudioGraph();
            frameInputNode.Start();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (frameInputNode != null)
            {
                frameInputNode.Stop();
                frameInputNode.Dispose();
            }
            if (graph != null)
            {
                graph.Dispose();
            }
        }

        unsafe private AudioFrame GenerateAudioData(uint samples)
        {
            // Buffer size is (number of samples) * (size of each sample)
            // We choose to generate single channel (mono) audio. For multi-channel, multiply by number of channels
            uint bufferSize = samples * sizeof(float);
            AudioFrame frame = new Windows.Media.AudioFrame(bufferSize);

            using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
            using (IMemoryBufferReference reference = buffer.CreateReference())
            {
                byte* dataInBytes;
                uint capacityInBytes;
                float* dataInFloat;

                // Get the buffer from the AudioFrame
                ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacityInBytes);

                // Cast to float since the data we are generating is float
                dataInFloat = (float*)dataInBytes;

                float freq = 1000; // choosing to generate frequency of 1kHz
                float amplitude = 0.3f;
                int sampleRate = (int)graph.EncodingProperties.SampleRate;
                double sampleIncrement = (freq * (Math.PI * 2)) / sampleRate;

                // Generate a 1kHz sine wave and populate the values in the memory buffer
                for (int i = 0; i < samples; i++)
                {
                    double sinValue = amplitude * Math.Sin(theta);
                    dataInFloat[i] = (float)sinValue;
                    theta += sampleIncrement;
                }
            }

            return frame;
        }

        private async Task CreateAudioGraph()
        {
            // Create an AudioGraph with default settings
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media);
            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);

            if (result.Status != AudioGraphCreationStatus.Success)
            {
                // Cannot create graph
                rootPage.NotifyUser(String.Format("AudioGraph Creation Error because {0}", result.Status.ToString()), NotifyType.ErrorMessage);
                return;
            }

            graph = result.Graph;

            // Create a device output node
            CreateAudioDeviceOutputNodeResult deviceOutputNodeResult = await graph.CreateDeviceOutputNodeAsync();
            if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // Cannot create device output node
                rootPage.NotifyUser(String.Format("Audio Device Output unavailable because {0}", deviceOutputNodeResult.Status.ToString()), NotifyType.ErrorMessage);
            }

            deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;
            rootPage.NotifyUser("Device Output Node successfully created", NotifyType.StatusMessage);

            // Create the FrameInputNode at the same format as the graph, except explicitly set mono.
            AudioEncodingProperties nodeEncodingProperties = graph.EncodingProperties;
            nodeEncodingProperties.ChannelCount = 1;
            frameInputNode = graph.CreateFrameInputNode(nodeEncodingProperties, emitter);
            frameInputNode.AddOutgoingConnection(deviceOutputNode);

            // Initialize the Frame Input Node in the stopped state
            frameInputNode.Stop();

            // Hook up an event handler so we can start generating samples when needed
            // This event is triggered when the node is required to provide data
            frameInputNode.QuantumStarted += node_QuantumStarted;

            // Start the graph since we will only start/stop the frame input node
            graph.Start();
        }

        private void OnUp(object sender, RoutedEventArgs e)
        {
            var margin = emitter_circle.Margin;
            margin.Bottom = margin.Bottom + 10;
            emitter_circle.Margin = margin;

            var pos = emitter.Position;
            pos.Z += PixelsToMeters(10);
            emitter.Position = pos;
        }

        private void OnDown(object sender, RoutedEventArgs e)
        {
            var margin = emitter_circle.Margin;
            margin.Bottom = margin.Bottom - 10;
            emitter_circle.Margin = margin;

            var pos = emitter.Position;
            pos.Z -= PixelsToMeters(10);
            emitter.Position = pos;
        }

        private void OnRight(object sender, RoutedEventArgs e)
        {
            var margin = emitter_circle.Margin;
            margin.Right = margin.Right - 10;
            emitter_circle.Margin = margin;

            var pos = emitter.Position;
            pos.X += PixelsToMeters(10);
            emitter.Position = pos;
        }

        private void OnLeft(object sender, RoutedEventArgs e)
        {
            var margin = emitter_circle.Margin;
            margin.Right = margin.Right + 10;
            emitter_circle.Margin = margin;

            var pos = emitter.Position;
            pos.X -= PixelsToMeters(10);
            emitter.Position = pos;
        }

        private int PixelsToMeters(int pixels)
        {
            return pixels / 10;
        }

        private void node_QuantumStarted(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            // GenerateAudioData can provide PCM audio data by directly synthesizing it or reading from a file.
            // Need to know how many samples are required. In this case, the node is running at the same rate as the rest of the graph
            // For minimum latency, only provide the required amount of samples. Extra samples will introduce additional latency.
            uint numSamplesNeeded = (uint)args.RequiredSamples;

            if (numSamplesNeeded != 0)
            {
                AudioFrame audioData = GenerateAudioData(numSamplesNeeded);
                frameInputNode.AddFrame(audioData);
            }
        }
    }
}
