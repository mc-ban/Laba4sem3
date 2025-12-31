using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CardGame.Core.GameState;
using CardGame.Core.Models;
using CardGame.GUI.ViewModels;

namespace CardGame.GUI
{
    public partial class GameBoardWindow : Window
    {
        private GameBoardViewModel _viewModel;
        private bool _isDragging = false;
        private Point _dragStart;
        private Border _dragElement;
        private Border _dragClone;

        private readonly Dictionary<Border, Brush> _originalBorders = new Dictionary<Border, Brush>();

        public GameBoardWindow()
        {
            InitializeComponent();
            _viewModel = new GameBoardViewModel();
            DataContext = _viewModel;
        }

        public void InitializeGame(GameState gameState)
        {
            _viewModel.InitializeGame(gameState);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
        }

        // Убедитесь, что эти методы вызывают ViewModel методы:
        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var border = sender as Border;
                if (border?.Tag is ICard card)
                {
                    // ГЛАВНОЕ: выбираем карту сразу
                    _viewModel.ExecuteSelectCard(card);

                    // Начинаем перетаскивание
                    _isDragging = true;
                    _dragStart = e.GetPosition(this);
                    _dragElement = border;
                    border.CaptureMouse();
                    CreateDragClone(border, card);
                }
            }
        }
        private void Card_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _dragClone != null)
            {
                var currentPos = e.GetPosition(this);

                Canvas.SetLeft(_dragClone, currentPos.X - _dragClone.Width / 2);
                Canvas.SetTop(_dragClone, currentPos.Y - _dragClone.Height / 2);

                var dropPos = e.GetPosition(DropZone);
                bool isOverDropZone = dropPos.X >= 0 && dropPos.X <= DropZone.ActualWidth &&
                                      dropPos.Y >= 0 && dropPos.Y <= DropZone.ActualHeight;

                if (isOverDropZone)
                {
                    DropZone.Opacity = 0.8;
                    DropZone.BorderBrush = Brushes.LimeGreen;
                    DropZone.Background = new SolidColorBrush(Color.FromArgb(80, 50, 255, 50));
                }
                else
                {
                    DropZone.Opacity = 0.5;
                    DropZone.BorderBrush = (Brush)FindResource("GoldBrush");
                    DropZone.Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                }
            }
        }

        private void Card_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _dragElement?.ReleaseMouseCapture();

                var dropPos = e.GetPosition(DropZone);
                bool isOverDropZone = dropPos.X >= 0 && dropPos.X <= DropZone.ActualWidth &&
                                      dropPos.Y >= 0 && dropPos.Y <= DropZone.ActualHeight;

                if (isOverDropZone && _viewModel.SelectedCard != null)
                {
                    // Вызов команды игры карты - ВСЯ логика в ViewModel
                    if (_viewModel.PlayCardCommand.CanExecute(_viewModel.SelectedCard))
                    {
                        // ГЛАВНОЕ: Вызываем команду - она СРАЗУ обновит UI
                        _viewModel.PlayCardCommand.Execute(_viewModel.SelectedCard);

                        // НЕМЕДЛЕННО обновляем визуальное состояние
                        // Карта уже должна исчезнуть из руки и появиться на поле

                        // Очищаем выделение
                        _viewModel.ExecuteClearSelection();
                    }
                }

                ClearDrag();
                ResetDropZone();
            }
        }
        private void ResetDropZone()
        {
            DropZone.Opacity = 0.5;
            DropZone.BorderBrush = (Brush)FindResource("GoldBrush");
            DropZone.Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
        }
  
        private void Creature_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag is CreatureCard creature)
            {
                _viewModel.ExecuteSelectCreature(creature);

                if (_viewModel.PlayerBoard.Contains(creature))
                {
                    HighlightElement(border, Brushes.Cyan);
                }
                else if (_viewModel.OpponentBoard.Contains(creature))
                {
                    HighlightElement(border, Brushes.Red);
                }
            }
        }

        private void CreateDragClone(Border original, ICard card)
        {
            var canvas = new Canvas
            {
                ClipToBounds = false,
                IsHitTestVisible = false
            };

            _dragClone = new Border
            {
                Width = original.Width * 1.2,
                Height = original.Height * 1.2,
                Background = original.Background?.Clone(),
                BorderBrush = Brushes.Gold,
                BorderThickness = new Thickness(3),
                CornerRadius = new CornerRadius(10),
                Opacity = 0.9
            };

            _dragClone.Effect = new DropShadowEffect
            {
                Color = Colors.Gold,
                BlurRadius = 25,
                Opacity = 0.9,
                ShadowDepth = 0
            };

            var content = new StackPanel();
            var text = new TextBlock
            {
                Text = card.Name,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15)
            };

            text.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 5,
                ShadowDepth = 2
            };

            content.Children.Add(text);
            _dragClone.Child = content;

            canvas.Children.Add(_dragClone);

            Canvas.SetLeft(_dragClone, _dragStart.X - _dragClone.Width / 2);
            Canvas.SetTop(_dragClone, _dragStart.Y - _dragClone.Height / 2);

            var overlay = new Grid
            {
                Background = Brushes.Transparent,
                ClipToBounds = false,
                IsHitTestVisible = false
            };
            overlay.Children.Add(canvas);

            var mainGrid = this.Content as Grid;
            if (mainGrid != null)
            {
                mainGrid.Children.Add(overlay);
                Panel.SetZIndex(overlay, 9999);
            }
        }

        private void ClearDrag()
        {
            if (_dragClone?.Parent is Canvas canvas && canvas.Parent is Grid overlay)
            {
                var mainGrid = this.Content as Grid;
                if (mainGrid != null && mainGrid.Children.Contains(overlay))
                {
                    mainGrid.Children.Remove(overlay);
                }
            }

            _dragClone = null;
            _dragElement = null;
        }

        private void HighlightElement(Border border, Brush color)
        {
            if (!_originalBorders.ContainsKey(border))
            {
                _originalBorders[border] = border.BorderBrush;
            }

            border.BorderBrush = color;
            border.BorderThickness = new Thickness(3);

            border.Effect = new DropShadowEffect
            {
                Color = ((SolidColorBrush)color).Color,
                BlurRadius = 20,
                Opacity = 0.7,
                ShadowDepth = 0
            };
        }

        private void ResetHighlight(Border border)
        {
            if (_originalBorders.ContainsKey(border))
            {
                border.BorderBrush = _originalBorders[border];
                border.BorderThickness = new Thickness(2);

                border.Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 20,
                    Opacity = 0.9,
                    ShadowDepth = 5
                };

                _originalBorders.Remove(border);
            }
        }

        private void ResetAllHighlights()
        {
            var bordersToReset = new List<Border>(_originalBorders.Keys);
            foreach (var border in bordersToReset)
            {
                ResetHighlight(border);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.Escape:
                    if (_viewModel.BackToMenuCommand.CanExecute(null))
                        _viewModel.BackToMenuCommand.Execute(null);
                    break;

                case Key.Space:
                case Key.Enter:
                    if (_viewModel.EndTurnCommand.CanExecute(null))
                        _viewModel.EndTurnCommand.Execute(null);
                    break;

                case Key.A:
                    if (_viewModel.AttackCommand.CanExecute(null))
                        _viewModel.AttackCommand.Execute(null);
                    break;

                case Key.D:
                    if (_viewModel.DirectAttackCommand.CanExecute(null))
                        _viewModel.DirectAttackCommand.Execute(null);
                    break;

                case Key.P:
                    if (_viewModel.PlayCardCommand.CanExecute(_viewModel.SelectedCard))
                        _viewModel.PlayCardCommand.Execute(_viewModel.SelectedCard);
                    break;

                case Key.C:
                    if (_viewModel.ClearSelectionCommand.CanExecute(null))
                    {
                        ResetAllHighlights();
                        _viewModel.ClearSelectionCommand.Execute(null);
                    }
                    break;
            }
        }
    }
}