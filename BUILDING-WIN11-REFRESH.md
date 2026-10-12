# EarTrumpet: Win11 visual refresh build

This tree = upstream EarTrumpet + Fluent/Win11 XAML restyle + a GitHub Actions
workflow that builds `EarTrumpet-Win11-Setup.exe` (a normal Windows installer)
on GitHub's Windows runners and publishes it as a GitHub Release.
No local toolchain needed.

## Build it (first time)

1. Create an empty private repo on GitHub (no README).
2. From this folder:

   ```
   git init -b master
   git add -A
   git commit -m "EarTrumpet Win11 refresh"
   git tag 2.3.1
   git remote add origin https://github.com/YOURNAME/eartrumpet-win11.git
   git push -u origin master --tags
   ```

3. GitHub -> Actions tab -> wait for the run to finish (~5-10 min).
4. Repo home page -> Releases (right sidebar) -> download `EarTrumpet-Win11-Setup.exe`.

## Build it again (repo already exists)

Copy these over your existing checkout, then commit and push:

- `.github/workflows/build.yml`
- `installer/EarTrumpet.iss`

Or in the repo on GitHub: Actions -> Build EarTrumpet -> Run workflow.
Every successful run adds a new release (`build-<number>`), so old installers stay available.

## Installing

- Run the Setup.exe. It installs per-user (no admin prompt), adds a Start menu
  entry, and offers "start when I sign in" (on by default) and a desktop shortcut (off).
- Uninstall from Settings -> Apps like any other program.
- Uninstall the Microsoft Store version first. Both versions use the same
  instance lock, so if the Store one is running (or starts at login), this build
  quietly exits at launch.
- Unsigned exe: SmartScreen prompts once (More info -> Run anyway).
- Settings don't carry over from the Store version; this is a separate unpackaged app.

## Notes

- To pick up upstream updates later: pull upstream, re-apply the visual
  changes (App.xaml, UI/Mutable.xaml, UI/Views/FlyoutWindow.xaml,
  UI/Views/SettingsWindow.xaml, plus HintPath additions in EarTrumpet.csproj),
  then push again.
- The `git tag` sets the version number (GitVersion reads it at build time).
