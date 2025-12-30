# Code Style

This repo follows standard modern C#/.NET conventions.

## General

- Prefer readable names over short names.
- Keep methods small and single-purpose.
- Avoid adding new dependencies unless clearly justified.

## Nullability

- Nullable reference types are enabled (`<Nullable>enable</Nullable>`).
- Use `?` for optional references and guard appropriately.

## WPF guidance

- UI structure stays in XAML.
- Code-behind is fine for this small utility, but if features grow, consider extracting services and/or adopting MVVM.

## Logging

- Prefer `Debug.WriteLine(...)` for non-fatal sampling issues.
- Avoid noisy logging loops in release builds.
