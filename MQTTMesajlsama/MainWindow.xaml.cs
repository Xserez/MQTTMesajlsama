using System;
using System.Text;
using System.Windows;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;

namespace MQTTMesajlsama
{
    
    public partial class MainWindow : Window
    {
        private IMqttClient mqttClient = null!;
        public MainWindow()
        {
            InitializeComponent();
            InitializeMqttClient();
        }
        private async void InitializeMqttClient()
        {
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            var baglanti = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost") 
                // sunucu
            .Build();

            mqttClient.UseConnectedHandler(async e =>
            {
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("chat").Build());
                Dispatcher.Invoke(() => AppendMessage("sunucuya bağlandı"));
            });

            mqttClient.UseDisconnectedHandler(async e =>
            {
                await mqttClient.ReconnectAsync();
                Dispatcher.Invoke(() => AppendMessage("tekrar bağlanıyor"));
            });

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                var mesaj = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Dispatcher.Invoke(() => AppendMessage(message));
            });

            try
            {
                await mqttClient.ConnectAsync(baglanti);
            }
            catch (Exception ex)
            {
                AppendMessage($"Failed to connect to MQTT server: {ex.Message}");
            }
        }

        private async void gonderbuton_Click(object sender, RoutedEventArgs e)
        {
            var mesaj = text2.Text;
            if (!string.IsNullOrEmpty(mesaj))
            {
                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("chat")
                    .WithPayload(mesaj)
                    .WithExactlyOnceQoS()
                    .Build();

                try
                {
                    await mqttClient.PublishAsync(mqttMessage);
                    AppendMessage($"You: {mesaj}");
                    text2.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    AppendMessage($"Mesaj gönderilemedi: {ex.Message}");
                }
            }
        }

        private void AppendMessage(string message)
        {
            text1.AppendText(message + Environment.NewLine);
        }
    }
}
