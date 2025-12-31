using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace CardGame.Core.GameEngine
{
    // Система мгновенного обновления в стиле RFOnline
    public class GameEngine
    {
        private static GameEngine _instance;
        public static GameEngine Instance => _instance ??= new GameEngine();

        private readonly List<WeakReference> _subscribers = new List<WeakReference>();
        private readonly DispatcherTimer _updateTimer;
        private bool _isUpdating;

        // События для мгновенного реагирования
        public event Action OnGameStateChanged;
        public event Action OnPlayerDataChanged;
        public event Action OnBoardStateChanged;
        public event Action OnHandChanged;
        public event Action OnManaChanged;
        public event Action OnHealthChanged;

        private GameEngine()
        {
            // Таймер для периодических обновлений (как в RFOnline)
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16), // ~60 FPS
                IsEnabled = true
            };

            _updateTimer.Tick += (s, e) => ProcessUpdates();
        }

        // Метод для принудительного мгновенного обновления
        public void ForceImmediateUpdate(string reason = "")
        {
            Debug.WriteLine($"ForceImmediateUpdate вызван: {reason}");

            Application.Current?.Dispatcher.Invoke(() =>
            {
                try
                {
                    OnGameStateChanged?.Invoke();
                    OnPlayerDataChanged?.Invoke();
                    OnBoardStateChanged?.Invoke();
                    OnHandChanged?.Invoke();
                    OnManaChanged?.Invoke();
                    OnHealthChanged?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка в ForceImmediateUpdate: {ex}");
                }
            });
        }

        // Пулл-обновление (как в RFOnline)
        private void ProcessUpdates()
        {
            if (_isUpdating) return;

            _isUpdating = true;
            try
            {
                // Очистка мёртвых подписчиков
                _subscribers.RemoveAll(wr => !wr.IsAlive);

                // Уведомление всех живых подписчиков
                foreach (var subscriber in _subscribers)
                {
                    if (subscriber.Target is IGameUpdateListener listener && subscriber.IsAlive)
                    {
                        try
                        {
                            listener.OnGameUpdate();
                        }
                        catch { /* игнорируем */ }
                    }
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        // Регистрация слушателей
        public void RegisterListener(IGameUpdateListener listener)
        {
            _subscribers.Add(new WeakReference(listener));
        }

        public void UnregisterListener(IGameUpdateListener listener)
        {
            _subscribers.RemoveAll(wr => wr.Target == listener);
        }

        // Запуск/остановка движка
        public void Start()
        {
            _updateTimer.Start();
            Debug.WriteLine("GameEngine запущен");
        }

        public void Stop()
        {
            _updateTimer.Stop();
            Debug.WriteLine("GameEngine остановлен");
        }
    }

    // Интерфейс для объектов, которые нужно обновлять
    public interface IGameUpdateListener
    {
        void OnGameUpdate();
    }
}