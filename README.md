<h2>Структура файлів</h2>
Програма реалізує механізм передачі даних трьома різними способами: Long Polling, Server Sent Events та Websocket</br>
Основними частинами програми є :
<ul>
  <li>Web api контролери, що знаходяться в папці Controllers</li>
  <li>Js клієнтські додатки, що знаходяться в папці wwwroot/js</li>
  <li>Html вигляди сторінок, що знаходяться в папці Pages/MethodPages</li>
</ul>
Також у архіві присутні додаткові файли
<ul>
  <li>Сервіс підключення до БД SourceDataService в папці Services</li>
  <li>Стиль сайту site.css в папці wwwroot/css</li>
  <li>Додаткові скрипти сайту site.js в папці wwwroot/js</li>
  <li>Файли для оновлення таблиці з наявними даними<br>
          - HomeController в папці Controllers<br>
          - Частковий вигляд showAllData в папці Views/Home
  </li>
  <li>Скрипт постійного оновлення даних в БД в папці db</li>
  <li>Представлення таблиці БД у вигляді c# об'єкту ExampleDataModel в папці Models</li>
</ul>
Частина файлів була згенерована програмою автоматично
<h2>Основні принципи роботи коду</h2>
Залежно від обраного користувачем метода, сайт починає відображати дані.<br>
Розглянемо основні принципи роботи на прикладі SSE:<br>
Робота починається з клієнтського js скрипту
<pre>
  <code>
    ﻿url = "/sse";
    var eventSource = new EventSource(url);
  </code>
</pre>
За вказаним url надсилається певний запит. За допомоги маршрутизації у програмі, запит надходить до відповідного контролера,
в нашому випадку це<code>SSEController</code>, а саме метод<code> public async Task SeeConnection(CancellationToken cancToken)</code><br>
Переходячи до цієї функції:
<pre>
  <code>
Response.Headers.Add("Content-Type", "text/event-stream");

using (SQLdependency = new SqlTableDependency<ExampleDataModel>(connectionString, "DataTable"))
{
  logger.LogInformation("Dependency has been established, sending message in order to update table");
  await Response.WriteAsync($"data: update table request" + "\n\n");
  Response.Body.Flush();
  </code>
</pre>
Після необхідних налаштувань в блоці using створюється об'єкт SqlTableDependency. Цей клас є частиною бібліотеки SqlTableDependency і використовується для встановлення прослуховування
бази даних. Одразу після створення об'єкта, назад до клієнта відправляється запит на оновлення таблиці з усіма даними, це робиться, щоб не втратити деякі записи до БД, котрі могли надійти поки підключення встановлювалося.
<pre>
  <code>
    String updateMessage ;
    SQLdependency.OnChanged += (sender, e) =>
      {
        updateMessage = JsonSerializer.Serialize(e.Entity);
      
        Response.WriteAsync($"data: {updateMessage}" + "\n\n");
        Response.Body.FlushAsync();
      };
    </code>
</pre>
Далі визначається спеціальна функція OnChanged, котра буде спрацьовувати кожен раз, коли до таблиці з БД вносяться зміни. В тілі цієї функції новий об'єкт формується та надсилається клієнту.
<pre>
  <code>
    SQLdependency.Start();

    while (!cancToken.IsCancellationRequested){
        Thread.Sleep(2000);
    };

    SQLdependency.Stop();
  </code>
</pre>
Після проведення усіх налаштувань прослуховування відкривається за допомоги Start() і триває до тих пір, поки через якусь спеціальну подію код не дійде до Stop().<br>
"Спеціальна подія" є різною для всіх методів:
<ul>
    <li>Для Long Polling події є дві<br>1)CancellationToken клієнта стає недійсним(клієнт покидає сторінку)<br>2)CancellationToken часу стає недійсним(час виконання запиту сплив)</li>
    <li>Для Server Sent Events подією є відключення від оновлень за допомоги CancellationToken(клієнт покидає сторінку)</li>
    <li>Для WebSocket подією є надіслане клієнтом повідомлення про роз'єднання<br>У випадку коли відключення клієнта відбулось непомітно, сервер тримає зв'язок ще дві хвилини, після чого самостійно обриває з'єднання
</li>
</ul>
Надіслане сервером повідомлення приходить назад до клієнта, де воно обробляється та виводиться на сайт:
<pre>
  <code>
    eventSource.onmessage = (message) => {
    tableUpdate();
    eventSource.onmessage = (message) => {
        let object = JSON.parse(message.data);
        entityUpdate(object);
    }
};
 </code>
</pre>
Коли повідомлення надійде до клієнта перший раз, воно викличе функцію оновлення таблиці на сайті та замінить визначення onmessage для подальшої обробки відповідей.<br><br>
У випадку з Long Polling, принцип дії трохи інший. Long Polling  може надати тільки одну відповідь на один запит, тому в цьому алгоритмі клієнт постійно буде надсилати нові
запити, після отримання відповідей на старі
