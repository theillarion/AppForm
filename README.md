## GUI для окна авторизации

Для создания всех окон была использована графическая подсистема с открытым исходным кодом WPF. Непосредственно был использован паттерн MVVM, упрощающий динамическое изменение элементов дизайна. Стоит отметить, что почти все обработчики событий имеют асинхронное свойство, так как требуют работу с базой данных.

![image](https://user-images.githubusercontent.com/95585343/229384983-be7dd568-9a37-4b40-af0d-5dc3c87f110d.png)

Для того, чтобы пользователь не вступал в ступор, при возникновении каких-либо ошибок, был создан контейнер AuthExceptionTextBox, в котором отображалась та или иная ошибка, возникающая во время авторизации (например, при некорректности пароля).

Также был добавлен отдельный Button для авторизации сотрудников, нажатие которой меняет флаг типа модели User'a.

Была реализован функционал восстановления подсказок при переходе на другой TextBox, а именно с loginTextBox на passTextBox.

## GUI для окна регистрации

![image](https://user-images.githubusercontent.com/95585343/229385346-be2d690c-7d06-455d-9055-f4bef624afcd.png)

Реализован переход между окном авторзации и регистрации.

Обеспечена валидность даты рождения и пароля User'a. 

Аналогично оставлен AuthExceptionTextBox.

Для того, чтобы пользователь не вводил дату рождения вручную был использован элемент DatePicker, который гарантирует нам валидность введенных данных с точки зрения расположения ГГ-ММ-ДД.

## GUI для главного окна пользователя

![image](https://user-images.githubusercontent.com/95585343/229387076-dc230cbf-200b-4710-a9f6-2e330ffc02ba.png)

Задан динамиечский объект баланса.

Пользователь имеет две возможности последующей псхологической процедуры. 

Создана панель администратора:

![image](https://user-images.githubusercontent.com/95585343/230797578-08c4a95e-a2ef-4a11-aaca-0682a0ce366f.png)

После проведения первого представления проекта заказчику было решено изменит цветовую палитру. Ниже представлен новый набор окон:

![image](https://user-images.githubusercontent.com/95585343/230799420-736e2e80-4405-4a26-bac5-815d467616d5.png)
![image](https://user-images.githubusercontent.com/95585343/230799429-f3619826-770d-40b9-b9aa-f874f6d41916.png)
![image](https://user-images.githubusercontent.com/95585343/230799442-f92eb049-4782-4c4f-9f72-73e85e438f5c.png)

После обсуждения с заказчиком было принято решение изменить цветовую гамму, реализовав первый вариант из показанных выше. 
Получены следующие результаты

![image](https://user-images.githubusercontent.com/95585343/232347611-71bcd5ca-33d7-429b-89ba-36c25aa0a74b.png)
![image](https://user-images.githubusercontent.com/95585343/232347635-5b8d5ce5-65db-4007-9496-6bd46b460a6d.png)
![image](https://user-images.githubusercontent.com/95585343/232347665-4bebc0a9-f018-46fa-aec2-87878bbb891c.png)

Также была реализована представление базы данных в главном окне администратора:

![image](https://user-images.githubusercontent.com/95585343/232347710-4b559a93-9686-4544-86dd-768036064faa.png)

