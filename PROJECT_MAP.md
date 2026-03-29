# Карта проекта ScreenDimmer

## Корень репозитория

| Файл | Назначение |
|------|------------|
| `ScreenDimmer.csproj` | SDK-style проект: WPF + Windows Forms (трей, `Screen`), .NET 8 Windows, манифест, `ApplicationIcon` → `Assets\app.ico` |
| `app.manifest` | `requestedExecutionLevel` — запуск без повышения прав |
| `App.xaml` / `App.xaml.cs` | Точка входа WPF: mutex, хук, трей, `ShutdownMode.OnExplicitShutdown` |
| `README.md` | Краткое описание |

## Логика приложения

| Файл | Назначение |
|------|------------|
| `LowLevelKeyboardHook.cs` | `SetWindowsHookEx(WH_KEYBOARD_LL)`, реакция на отпускание F8 (`VK_F8` = 0x77) |
| `DimmerService.cs` | Список оверлей-окон по мониторам, переключение видимости |
| `OverlayWindow.xaml` (+ `.cs`) | Чёрное полноэкранное окно, `SetWindowPos`, `WS_EX_TOOLWINDOW` |
| `NotifyIconService.cs` | Контекстное меню трея (Toggle, автозапуск, Exit), подсказка |
| `TrayIconLoader.cs` | Загрузка `Assets\app.ico` для `NotifyIcon` |
| `AutostartService.cs` | Автозапуск: ярлык `ScreenDimmer.lnk` в папке пользователя **Автозагрузка** (без реестра HKLM) |

### План этапа 2 (см. [PLAN.md](PLAN.md) — пока не реализовано в коде)

| Файл / слой | Назначение (после внедрения) |
|-------------|-------------------------------|
| `MainWindow.xaml` (+ `.cs`) | Инфо-окно: тексты, тема тёмная/светлая, без крестика, сворачивание в трей; `ShowInTaskbar = false` |
| Настройки пользователя | Сохранение темы и геометрии окна (`Properties.Settings` или JSON в `%AppData%`) |
| `NotifyIconService.cs` | Дополнение: **ЛКМ** — показ/скрытие главного окна (меню трея и Exit — без изменения смысла) |
| `App.xaml.cs` | Создание `MainWindow` в коде, скрытие при старте, связка с треем |

## Ресурсы

| Путь | Назначение |
|------|------------|
| `Assets/app.ico` | Иконка приложения (exe) и трея; копируется в выходной каталог |
| `tools/rebuild-app-ico.ps1` | Сборка **валидного для GDI+** `Assets\app.ico` (иконка трея и exe) |
| `tools/png-to-ico.ps1` | Альтернатива: ICO из PNG; после генерации проверять `new Icon(path)` |
| `tools/check-icon.ps1` | Диагностика: размер ICO, `ExtractAssociatedIcon` для собранного exe (локальная проверка) |

## Сборка

- Выход: `bin/<Configuration>/net8.0-windows/ScreenDimmer.exe`
- Требуется установленный **.NET 8 SDK** на машине разработчика

## Прочее

| Путь | Назначение |
|------|------------|
| `.github/workflows/ci.yml` | CI (если настроен) |
| `.cursor/rules/` | Правила Cursor для репозитория |
