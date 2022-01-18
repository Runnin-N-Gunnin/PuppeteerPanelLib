# PuppeteerPanelLib

PuppeteerPanel is a bare minimum, custom User Control that embeds puppeteer's chromium window in a Windows Forms application. Proof of concept, easily extendable.
Uses Win32 API to call SetParent, SetWindowLong & MoveWindow.

## Use Case
- Chromium and/or puppeteer-sharp testing, prototyping
- Bots, scrapers and crawlers, OCR bots++
- Experimentation, visual testing

## Features:

- Easily embed puppeteer-sharp's chromium window
- Handles proper termination of chromium
- Hides chromium from appearing in taskbar

## Example:

![Alt text](/Example.png "Screenshot")

## Requirements:

- Puppeteer-sharp (nuget/github)
- .NET Framework


## How to use:

Import 'PuppeteerPanelLib.csproj' to your solution OR simply reference the file 'PuppeteerPanelLib.dll'. Build project. Drag and drop 'PuppeteerPanel' user control from Toolbox to Form. Restart Visual Studio if the control does not show up in your Toolbox.
