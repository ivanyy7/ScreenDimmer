# Карта проекта ScreenDimmer

## Корень репозитория

| Файл | Назначение |
|------|------------|
| `ScreenDimmer.csproj` | SDK-style проект: WPF + Windows Forms (трей, `Screen`), .NET 8 Windows, манифест, `ApplicationIcon` → `Assets\app.ico` |
| `app.manifest` | `requestedExecutionLevel` — запуск без повышения прав |
| `App.xaml` / `App.xaml.cs` | Точка входа WPF: mutex, хук, трей, `ShutdownMode.OnExplicitShutdown`; создаёт `MainWindow` **без** `Show()` при старте (**2.5**); переключение окна из трея — `Dispatcher.BeginInvoke(ToggleMainWindowSafe)`; при показе — `Topmost` + `Activate` (**2.6** + фикс ЛКМ) |
| `README.md` | Описание приложения и краткое руководство пользователя |

## Логика приложения

| Файл | Назначение |
|------|------------|
| `LowLevelKeyboardHook.cs` | `SetWindowsHookEx(WH_KEYBOARD_LL)`, реакция на отпускание F8 (`VK_F8` = 0x77) |
| `DimmerService.cs` | Список оверлей-окон по мониторам, переключение видимости |
| `OverlayWindow.xaml` (+ `.cs`) | Чёрное полноэкранное окно, `SetWindowPos`, `WS_EX_TOOLWINDOW` |
| `NotifyIconService.cs` | Контекстное меню трея (Toggle, автозапуск, Exit); **ЛКМ** — `MouseDown` → `MainWindowToggleRequested` (не `MouseClick` — надёжнее в трее и у «скрытых» иконок) (**2.6**) |
| `TrayIconLoader.cs` | Загрузка `Assets\app.ico` для `NotifyIcon` |
| `AutostartService.cs` | Автозапуск: ярлык `ScreenDimmer.lnk` в папке пользователя **Автозагрузка** (без реестра HKLM) |
| `MainWindow.xaml` (+ `.cs`) | Инфо-окно этапа 2: хром (**2.2**), темы (**2.3**), настройки (**2.4**); `ShowInTaskbar = false`; показ не из старта приложения — **2.5** + **2.6** (см. [PLAN.md](PLAN.md)) |
| `UserSettingsData.cs` | Модель настроек: тема, Left/Top/Width/Height |
| `UserSettingsStore.cs` | JSON `%LocalAppData%\ScreenDimmer\settings.json` |

### Этап 2 (инфо-окно) — готово

Соответствует **PLAN.md** (подэтапы 2.1–2.7). Ключевые точки: `MainWindow` + `UserSettingsStore`, трей (**ЛКМ** — окно), без кнопки в панели задач, настройки в `%LocalAppData%\ScreenDimmer\settings.json`, выход только **Exit** в меню трея, один экземпляр (**mutex**), F8 и автозапуск как в MVP.

**Этап 3** (упаковка / установщик для переноса на другие ПК) — не в репозитории, **отложен** (см. **PROJECT_LOG**).

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
