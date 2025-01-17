Основной файл электрика связан с файлом архитектора. Работа со связанным файлом.
Объекты электрика находятся в основном файле. Помещения находятся в связанном файле.

Задача – определить в каком помещении находится объект электрика и переписать значения параметров помещения, в котором он находится, в параметры объекта электрика (имя помещения, номер помещения и номер квартиры).
Сравниваются помещения и объекты электрика одного и того же этажа. Линии границ помещения и тестовая линия _projectLine приводятся к одной и той же высотной отметке.

try catch в ExternalCommand используется для показа предупреждений пользователю.

В окно вытягиваются связи, имеющиеся в проекте, и пользователь выбирает на какую из связей должен работать код.

Так как объекты электрика преимущественно находятся в стенах, в полах, в потолках, то границы помещения извлекаются по серединам стен.
Для определения находится ли объект в помещении у объекта извлекается точка размещения семейства Location и строится длинная линия _projectLine, длиной 100метров. Это тестовая линия, пересечения которой мы считаем с пересечениями линий границ помещения.


Нахождение точки размещения семейства.
Если у семейства нет точки принадлежности помещению, то извлекается точка размещения семейства. У тех семейств, которые размещаются на лиинии, извлекается линия. И точка размещения, и линия размещения преобразуются в прямую длиной 100метров _projectLine которая расположна под углом 3 градуса, у LocationCurve по отношению к исходному направлению, а LocationPoint по отношению к вертикали. Предполагается, что самый длинный коридор в здании будет 60 метров, потому берется линия длиннее самого длинного в здании помещения. А поворот в 3 градуса обеспечит непараллельность с линиями границ помещения во избежание некоторых возможных ситуаций при наложении/совпадении тестовой линии с линией границы помещения.


CountIntersectionsWithPolygon – подсчитывает количество пересечений.

Основная идея алгоритма.

Если тестовая линия _projectLine пересекла линии помещения четное число раз, то объект находится вне помещения.
Если тестовая линия _projectLine пересекла линии помещения НЕчетное число раз, то объект находится в помещении.

Нюансы алгоритма.

Если начальная или конечная точка тестовой линии _projectLine находится на вершине многоугольника из линий помещения, то объект в помещении.
Если начальная или конечная точка тестовой линии _projectLine находится на линии границы помещения, то объект в помещении.

Начало линии всегда внутри помещения, из которого линия исходит.

AllObjectsElectric возвращает все объекты электрика, находящиеся в проекте.

BordersRoom возвращает линии на высотной отметке помещения, созданные из границ помещения, извлеченных по серединам стен. Таким образом, мы перевели решение задачи в 2D-плоскость.

IsLocated возвращает истину или ложь, определяя находится ли объект в помещении - подсчитывает количество значений, четное или нечетное в коллекции.

LevelAnyObject возвращает уровень у любого объекта электрика.


LevelCache возвращает один раз коллекцию уровней на разные случаи использования.

ElevationDouble возвращает уровень, над которым находится высотная отметка, double, объекта.

ObjectToLine возвращает тестовую линию _projectLine созданную на основании точки или линии размещения объекта.

Не все классы используются в коде, они оставлены с целью фиксации достигнутых некоторых идей, например, работы с помещениями.

![рис 1](https://github.com/user-attachments/assets/aa707f43-80f5-4d9e-a418-d70f98919348)

![рис 2](https://github.com/user-attachments/assets/85cb77f0-7b16-42ba-8a6d-572b333cec6d)




