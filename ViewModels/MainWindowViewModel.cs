using RoomWriteEmpty.Infrastructure.Commands;
using RoomWriteEmpty.Models;
using RoomWriteEmpty.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace RoomWriteEmpty.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        private readonly IProjectLinksService _projectLinksService;

        public ObservableCollection<string> LinksNames{ get; }

        private string _selectedLink;
        public string SelectedLink
        {
            get => _selectedLink;
            set => Set(ref _selectedLink, value);  // уведомляет об изменениях с помощью OnPropertyChanged
        }

        public ICommand ApplyCommand
        { get; } // команда закрытие окна кнопкой Применить

        public MainWindowViewModel(IProjectLinksService projectLinksService)
        {
            _projectLinksService = projectLinksService;

            LinksNames = new ObservableCollection<string>(_projectLinksService.GetLinksNames());

            ApplyCommand = new LambdaCommand(OnApply); // Инициализация команды
        }
        private void OnApply(object parameter)
        {
            if (parameter is Window window)
            {
                //Передача окна через CommandParameter. RelativeSource передает ссылку на окно в метод команды.
                //Это позволяет избежать передачи Window напрямую в ViewModel, соблюдая MVVM
                window.DialogResult = true; // Устанавливаем результат диалога
                window.Close(); // Закрываем окно
            }
        }
    }
}






//private readonly IProjectLinksService _projectDataService;

//private List<string> _selectedLink;
//public List<string> SelectedLink
//{
//    get => _selectedLink;
//    set => Set(ref _selectedLink, value);
//}

//private ObservableCollection<string> _linkName;
//public ObservableCollection<string> LinksNames
//{
//    get => _linkName;
//    set => Set(ref _linkName, value);
//}

//public MainWindowViewModel(IProjectLinksService projectDataService)
//{
//    _projectDataService = projectDataService;
//    var linksNamesList = _projectDataService.GetLinksNames();
//    LinksNames = new ObservableCollection<string>(linksNamesList);
//}













//#region Заголовок окна
//private string _Title = "Тест настройки";
//public string Title
//{
//    get => _Title;
//    set => Set(ref _Title, value);
//}
//#endregion
