<div id="header" align="center">
  <img src="https://media.giphy.com/media/v1.Y2lkPWVjZjA1ZTQ3cWZmcnBmYmxvbDFrczYwa3F1eDY4dnlseDM0bHNtbnZ2YWU1bm85aSZlcD12MV9zdGlja2Vyc19zZWFyY2gmY3Q9dHM/ulZ7gQQz9jwZzv224n/giphy.gif" width="200"/>
</div>

<div id="badges" align="center">
  <a href="https://vk.com/gym_bosss" target="_blank">
    <img src="https://img.shields.io/badge/VK-0077FF?style=for-the-badge&logo=vk&logoColor=white" alt="VK Badge"/>
  </a>
  <a href="https://t.me/ultra_chelik" target="_blank">
    <img src="https://img.shields.io/badge/Telegram-26A5E4?style=for-the-badge&logo=telegram&logoColor=white" alt="Telegram Badge"/>
  </a>
</div>

<div id="counter" align="center">
  <img src="https://komarev.com/ghpvc/?username=GYMBOSSS&style=flat-square&color=blue" alt=""/>
</div>

<div align="center">
  <img src="https://media.giphy.com/media/v1.Y2lkPWVjZjA1ZTQ3dmoxOWRlNnJkMTd2aHp6eWFpenJkOTlhZDhueDhvMnFjMGt6c2FtcSZlcD12MV9naWZzX3JlbGF0ZWQmY3Q9Zw/SWoSkN6DxTszqIKEqv/giphy.gif" height="300" width="600">
</div>

---

# 💬 TCP-Chat - Легковесный чат на C#

<div align="center">

**TCP-Chat** - это высокопроизводительный консольный чат с поддержкой приватных сообщений и системой пользователей

[![Version](https://img.shields.io/badge/Version-1.0.0-brightgreen?style=for-the-badge)](https://github.com/GYMBOSSS/TCP-Chat)
[![C#](https://img.shields.io/badge/C%23-8.0+-purple?style=for-the-badge&logo=csharp)](https://dotnet.microsoft.com)
[![.NET](https://img.shields.io/badge/.NET-6.0-blue?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey?style=for-the-badge)](https://github.com/GYMBOSSS/TCP-Chat)

</div>

## ✨ Особенности

### 🔧 Технические возможности
- **Многопользовательский TCP-чат** с поддержкой множества клиентов
- **Приватные сообщения** между пользователями
- **Система списка пользователей** в реальном времени
- **Автосохранение имени пользователя** между сессиями
- **Кроссплатформенность** - работает на Windows, Linux, macOS

### 💻 Производительность
- **Асинхронные операции** ввода-вывода
- **Минимальные задержки** при передаче сообщений
- **Эффективное использование памяти**
- **Стабильное соединение** с обработкой разрывов

### 🛡 Надежность
- **Обработка исключений** на всех уровнях
- **Graceful shutdown** при отключении
- **Защита от потери данных**
- **Валидация ввода пользователя**

## 🚀 Быстрый старт

### Предварительные требования
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) или выше
- Windows/Linux/macOS

### Запуск сервераbash
# Клонирование репозитория
git clone https://github.com/GYMBOSSS/TCP-Chat.git
cd TCP-Chat

# Запуск сервера
dotnet run --project Server

### Запуск клиента
bash
# В отдельном терминале
dotnet run --project Client

## 🎮 Использование

### Основные команды чата

| Команда | Описание |
|---------|-----------|
| `/help` | Показать список команд |
| `/users` | Показать подключенных пользователей |
| `/chat` | Начать приватный чат |
| `/back` | Выйти из приватного чата |
| `/exit` | Отключиться от сервера |

### Пример сессии
> Добро пожаловать в TCP-Chat!
> Введите имя пользователя: Глеб
> Подключение к серверу 127.0.0.1:8080...
> Успешное подключение!

[СИСТЕМА] Глеб присоединился к чату
> Привет всем!
Иван: Привет, Глеб!
Мария: Добро пожаловать!

> /users
Подключенные пользователи:
    0. Иван
    1. Мария
    2. Глеб

> /chat
> Выберите пользователя (введите номер): 1
> Начат чат с Марией
> Привет, как дела?
Мария: Привет! Все отлично, работаю над проектом.
> /back
> Чат завершен

## 🏗 Архитектура

### Серверная часть
csharp
public class Server
{
    // Многопоточное управление пользователями
    // Асинхронное принятие подключений
    // Широковещательная рассылка сообщений
    // Обработка приватных чатов
}

### Клиентская часть
csharp
public class Client  
{
    // Двунаправленная асинхронная связь
    // Обработка пользовательского ввода
    // Отображение сообщений в реальном времени
    // Управление приватными сессиями
}

## 📊 Производительность

- **Поддержка 100+ одновременных пользователей**
- **Задержка менее 10мс** между клиентами в одной сети
- **Минимальное использование CPU** в режиме ожидания
- **Автоматическое восстановление** при сетевых сбоях

## 🛠 Технологии

- **Язык**: C# 8.0+
- **Платформа**: .NET 6.0
- **Сетевой протокол**: TCP/IP
- **Кодировка**: UTF-8
- **Архитектура**: Асинхронная/многопоточная

## 🔧 Разработка

### Сборка проекта
bash
dotnet build

### Запуск тестов
bash
dotnet test

### Создание релиза
bash
dotnet publish -c Release -r win-x64 --self-contained

## 🤝 Участие в разработке

Мы приветствуем вклад в развитие TCP-Chat! 

1. Форкните репозиторий
2. Создайте ветку для функции (`git checkout -b feature/AmazingFeature`)
3. Закоммитьте изменения (`git commit -m 'Add AmazingFeature'`)
4. Запушьте в ветку (`git push origin feature/AmazingFeature`)
5. Откройте Pull Request

## 🐛 Отчет об ошибках

Нашли ошибку? [Создайте issue](https://github.com/GYMBOSSS/TCP-Chat/issues) с подробным описанием:

- Шаги для воспроизведения
- Ожидаемое поведение
- Фактическое поведение
- Версия .NET и ОС

## 📝 Лицензия

Этот проект распространяется под лицензией MIT. Подробнее см. в файле `LICENSE`.

## 📞 Контакты

<div align="center">

**Александр** - .NET Developer

- 🔭 Работаю над созданием эффективных сетевых приложений
- 🌱 Изучаю высоконагруженные системы на C#
- 💡 Развиваю open-source проекты для сообщества
- 📫 Как со мной связаться:

[![VK](https://img.shields.io/badge/VK-0077FF?style=for-the-badge&logo=vk&logoColor=white)](https://vk.com/gym_bosss)
[![Telegram](https://img.shields.io/badge/Telegram-26A5E4?style=for-the-badge&logo=telegram&logoColor=white)](https://t.me/ultra_chelik)

</div>

---

<div align="center">

### 🚀 Присоединяйтесь к развитию TCP-Chat!

**Вместе мы создадим надежный и производительный чат для сообществ!**

*"Простота и надежность - ключ к успешному сетевому приложению"*

</div>
