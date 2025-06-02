using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Data;

namespace BlazorApp5.Services
{
    public class ChatHubService : IAsyncDisposable
    {
        public event Action? OnConnectionStateChanged;

        private readonly NavigationManager _navigation;
        private HubConnection _hubConnection;
        private bool _isStarted;

        public HubConnection Connection => _hubConnection;
        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public ChatHubService(NavigationManager navigation)
        {
            _navigation = navigation;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigation.ToAbsoluteUri("/chathub"))
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.Closed += async (ex) =>
            {
                OnConnectionStateChanged?.Invoke();
            };

            _hubConnection.Reconnected += async (id) =>
            {
                OnConnectionStateChanged?.Invoke();
            };

            _hubConnection.Reconnecting += async (ex) =>
            {
                OnConnectionStateChanged?.Invoke();
            };

        }

        public async Task EnsureConnectedAsync(string userCode, Func<Task>? notifyUi = null)
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await _hubConnection.StartAsync();
                    OnConnectionStateChanged?.Invoke();
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"[ChatHubService] StartAsync failed: {ex.Message}");
                    // Safe to ignore if already connecting
                }
            }

            if (_hubConnection.State == HubConnectionState.Connected && !_isStarted)
            {
                await _hubConnection.SendAsync("RegisterCode", userCode);
                _isStarted = true;
            }
        }


        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
                await _hubConnection.DisposeAsync();
        }
    }
}
