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
BOT_API_TOKEN=ВВЕДИТЕ_ТОКЕН_БОТА
CLOUD_CONVERT_API_TOKEN=ВВЕДИТЕ_ТОКЕН_CLOUDCONVERT

# Переменные окружения postgreSQL
POSTGRES_USER=admin
POSTGRES_PASSWORD=admin
POSTGRES_DB=ScheduleBotDB
```
3. В терминале из папки проекта выполняем команду `Docker compose run --build`.

## Как развернуть бота на сервере:
1. Устанавливаем Docker.
2. Создаем файл `docker-compose.yaml` с содержимым:
```
services:
  db:
    container_name: db
    image: postgres:14.4
    env_file:
      - .env
    volumes:
      - ./ScheduleBot/PostgresData/:/var/lib/postgresql/data
    ports:
      - "5432:5432"
  
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
BOT_API_TOKEN=ВВЕДИТЕ_ТОКЕН_БОТА
CLOUD_CONVERT_API_TOKEN=ВВЕДИТЕ_ТОКЕН_CLOUDCONVERT

# Переменные окружения postgreSQL
POSTGRES_USER=admin
POSTGRES_PASSWORD=admin
POSTGRES_DB=ScheduleBotDB
```
4. Выполняем команду `docker compose run -d`.

## Используемые прямые зависимости
- Microsoft.EntityFrameworkCore 6.0.7
- Npgsql.EntityFrameworkCore.PostgreSQL 6.0.5
- CloudConvert.API 1.1.1
- NLog 5.0.1
- Telegram.Bot 18.0.0
