# ScheduleBot
Телеграм бот, который отправляет расписание БГК и следит за его обновлением.  
Опробуйте его прямо сейчас: https://t.me/BSC_ScheduleBot

## Как собрать и запустить:
__Для компиляции бота необходимо иметь Docker и Docker compose.__
1. Скачиваем исходники.
2. Создаем `.env` файл следующего содержания и заполняем его своими значениями:  
__Свою временную зону ищите в списке: https://en.wikipedia.org/wiki/List_of_tz_database_time_zones__
```
# Код часового пояса из базы TimeZone
TZ=ВВЕДИТЕ_TIMEZONE_КОД

# Переменные окружения бота
ADMIN_TELEGRAM_ID=ВВЕДИТЕ_СВОЙ_TELEGRAM_ID
BOT_API_TOKEN=ВВЕДИТЕ_ТОКЕН_БОТА
CLOUD_CONVERT_API_TOKEN=ВВЕДИТЕ_ТОКЕН_CLOUDCONVERT
# Бот проверяет расписание каждый день не постоянно, а на заданном промежутке веремени (от 0 до 23 часов)
SCHEDULE_CHECK_TIME_START=ВВЕДИТЕ_ЧАС_С_КОТОРОГО_НАЧНЕТСЯ_ПРОВЕРКА_РАСПИСАНИЯ
SCHEDULE_CHECK_TIME_END=ВВЕДИТЕ_ЧАС_НА_КОТОРОМ_ЗАКОНЧИТСЯ_ПРОВЕРКА_РАСПИСАНИЯ
# Между проверками есть интервал 
TIME_BETWEEN_CHECKS_IN_MILLISECONDS=ВВЕДИТЕ_ВРЕМЯ_МЕЖДУ_ПРОВЕРКАМИ_РАСПИСАНИЯ_В_МИЛЛИСЕКУНДАХ
```
3. В терминале из папки проекта выполняем команду `Docker compose run --build`.

## Как развернуть бота на сервере:
1. Устанавливаем Docker.
2. Создаем файл `docker-compose.yaml` с содержимым:
```
services:
  schedulebot:
    container_name: schedulebot
    image: neitralov/schedulebot
    depends_on: 
      - db
    env_file:
      - .env
    volumes:
      - ./ScheduleBot/Data:/app/Data
      - ./ScheduleBot/Logs:/app/Logs
```
3. Создаем `.env` файл следующего содержания и заполняем его своими значениями:  
__Свою временную зону ищите в списке: https://en.wikipedia.org/wiki/List_of_tz_database_time_zones__
```
# Код часового пояса из базы TimeZone
TZ=ВВЕДИТЕ_TIMEZONE_КОД

# Переменные окружения бота
ADMIN_TELEGRAM_ID=ВВЕДИТЕ_СВОЙ_TELEGRAM_ID
BOT_API_TOKEN=ВВЕДИТЕ_ТОКЕН_БОТА
CLOUD_CONVERT_API_TOKEN=ВВЕДИТЕ_ТОКЕН_CLOUDCONVERT
# Бот проверяет расписание каждый день не постоянно, а на заданном промежутке веремени (от 0 до 23 часов)
SCHEDULE_CHECK_TIME_START=ВВЕДИТЕ_ЧАС_С_КОТОРОГО_НАЧНЕТСЯ_ПРОВЕРКА_РАСПИСАНИЯ
SCHEDULE_CHECK_TIME_END=ВВЕДИТЕ_ЧАС_НА_КОТОРОМ_ЗАКОНЧИТСЯ_ПРОВЕРКА_РАСПИСАНИЯ
# Между проверками есть интервал 
TIME_BETWEEN_CHECKS_IN_MILLISECONDS=ВВЕДИТЕ_ВРЕМЯ_МЕЖДУ_ПРОВЕРКАМИ_РАСПИСАНИЯ_В_МИЛЛИСЕКУНДАХ
```
4. Выполняем команду `docker compose run -d`.

## Используемые прямые зависимости
- Microsoft.EntityFrameworkCore 6.0.13
- Microsoft.EntityFrameworkCore.Sqlite 6.0.13
- CloudConvert.API 1.1.1
- NLog 5.0.1
- Telegram.Bot 18.0.0
