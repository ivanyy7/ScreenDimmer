# Карта проекта ScreenDimmer

## Корень репозитория

| Файл | Назначение |
|------|------------|
| `ScreenDimmer.csproj` | SDK-style проект: WPF + Windows Forms (трей, `Screen`), .NET 8 Windows, манифест |
| `app.manifest` | `requestedExecutionLevel` — запуск без повышения прав |
| `App.xaml` / `App.xaml.cs` | Точка входа WPF: mutex, хук, трей, `ShutdownMode.OnExplicitShutdown` |
| `README.md` | Краткое описание |

## Логика приложения

| Файл | Назначение |
|------|------------|
| `LowLevelKeyboardHook.cs` | `SetWindowsHookEx(WH_KEYBOARD_LL)`, реакция на отпускание F8 (`VK_F8` = 0x77) |
| `DimmerService.cs` | Список оверлей-окон по мониторам, переключение видимости |
| `OverlayWindow.xaml` (+ `.cs`) | Чёрное полноэкранное окно, `SetWindowPos`, `WS_EX_TOOLWINDOW` |
| `NotifyIconService.cs` | Контекстное меню трея и подсказка |

## Сборка

- Выход: `bin/<Configuration>/net8.0-windows/ScreenDimmer.exe`
- Требуется установленный **.NET 8 SDK** на машине разработчика

## Прочее

| Путь | Назначение |
|------|------------|
| `.github/workflows/ci.yml` | CI (если настроен) |
| `.cursor/rules/` | Правила Cursor для репозитория |
