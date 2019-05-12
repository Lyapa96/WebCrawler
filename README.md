# WebCrawler

Программа переходит по ссылкам на сайтах WEB 1.0 и сохраняет эти страницы на диск.

## Запуск
`WebCrawler.exe -u https://www.debian.org/` - обязательным параметром является стартовый url, с которого начинается поиск

`WebCrawler.exe -u https://www.debian.org/ -d 5 -c 1000 -p C:\Users\Results` - дополнительно можно передать глубину обхода `-d`, количество ссылок для скачивания `-c` (важная особенность - это не количество загруженных, а именно скачанных, т.к. на сайтах Web 1.0 есть ссылки, которые уже давно не работют), путь до директории в которую нужно сохранить страницы `-p`

## Вывод на консоль
Программа регулярно выводит id потока, который завершил скачивание страницы. 

Причины завершения:
* Указанная глюбина будет достигнута 
* Количество скачанных ссылок будет равно заданному
* В очереди не останется ссылок

После любого из вышеуказанных событий на экран будет выведена краткая информация о состоянии программы на момент завершения.

## Идея реализации
Для каждой ссылки создается [задача](https://docs.microsoft.com/ru-ru/dotnet/api/system.threading.tasks.task?view=netframework-4.8), которая должна будет скачать страницу, сохранить её на диск, достать из страницы все новые ссылки и вернуть их.

Задача не является потоком. Задачами управляет TaskSceduler, который в свою очередь использует пулл потоков. Вполне нормально, что один и тот же поток выполняет несколько задач. Благодяря визуализации, можно увидеть, какой поток какую задачу выполнял.

Результатом задач является массив новых ссылок для страницы. Последний шаг - дождаться выполнения всех задач, смержить результаты, и сложить все в очередь.

Данная реализация не требует блокировок. 
