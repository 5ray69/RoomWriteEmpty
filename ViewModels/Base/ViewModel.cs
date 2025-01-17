using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RoomWriteEmpty.ViewModels.Base
{
    public class ViewModel : INotifyPropertyChanged
    {
        // видео Шмачилина часть1 29минута
        // Чтобы наша VM «автоматически» обновляла View, требуется реализовать интерфейс INotifyPropertyChange.
        // Именно посредством него View получает уведомления, что во VM что-то изменилось и требуется обновить данные.
        public event PropertyChangedEventHandler PropertyChanged;  // Объявили событие.

        //средство для генерации события, чтобы все наследники смогли им воспользоваться
        //[CallerMemberName] string PropertyName = null компилятор автоматически подставит в PropertyName имя метода
        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)  //передали в этот метод имя свойства и сгенерировали внутри события
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        //Метод Set будет обновлять значение свойства для которого определено поле, 
        //в котором это свойство хранит свои данные.
        //Задача этого метода разрешить кольцевые изменения свойств, которые могут возникать
        // - чтоб небыло кольцевого зацикливания и переполнения стека
        //ref T field это ссылка на поле свойства
        //T value это новое значение, которое мы хотим установить
        //PropertyName это будет имя свойства, которое мы передадим в OnPropertyChanged
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            //если значение поля соответствует тому значению, на которое мы
            //хотим изменить это поле, то мы ничего не делаем
            if (Equals(field, value)) return false;
            field = value; //если значение изменилось, то обновляем поле и генерируем OnPropertyChanged
            OnPropertyChanged(PropertyName);
            return true;
        }
    }
}


//using System.ComponentModel;
//using System.Runtime.CompilerServices;

//namespace RoomWriteEmpty.ViewModels.Base
//{
//    public class ViewModel : INotifyPropertyChanged
//    {
//        public event PropertyChangedEventHandler PropertyChanged;

//        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
//        }
//        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
//        {
//            if (Equals(field, value)) return false;
//            field = value;
//            OnPropertyChanged(PropertyName);
//            return true;
//        }
//    }
//}