VR-TVA
================

This is a VR experiment for whole and partial reports of letter
displays. This experiment is a modified and heavily extended version of
an experiment programmed by Mariusz Matya
([@miriusz6](https://github.com/miriusz6)) and Uffe Dalgas
([@UffeDalgas](https://github.com/UffeDalgas)) for their joint Bachelor
thesis project.

## Prerequisites

### Software

This experiment is programmed in C# in the [Unity](https://unity.com/)
framework. At present, due to a few outdated dependencies, the project
only runs with **Unity Editor 2020.3.6f1**. After installing Unity Hub,
you can install the editor by [clicking
here](https://unity.com/releases/editor/whats-new/2020.3.6). Do not
attempt to open the project in a different editor version as this will
likely break the code!

### Hardware

The experiment has only been tested using Meta’s **Oculus Quest 3**
headset but it will probably work with other Oculus headsets as well.

## Installation

### Optional: Download Unity project

You can download this project as a ZIP file or clone the repository to
your computer. Afterwards, you can open the project from within Unity
Hub (remember to select the appropriate Unity version).

### Install and run APK

The experiment will be packaged as an Android APK file. You can directly
build and run from within Unity (Files menu, may require further setup)
or copy the build APK from `Builds/` (or by [clicking
here](Builds/VRCTVA.apk)) to your device.

## Running the experiment(s)

If you have clicked “Build and Run” from within the Unity Editor, the
app is automatically run after installation. To run it subsequently
without opening Unity or after installing from a manually transferred
APK file, just select the app from the app menu (name: `VRCTVA`, typical
Unity logo).

### Settings screen

The first screen will prompt a subject number to be input via the
displayed virtual keyboard. You cannot reopen the keyboard, should focus
be lost. In that case, restart the app. *After* closing/confirming the
input, you can press the A and B buttons to rotate through a list of
included experimental procedures. Once set, pressing the trigger will
start the selected experiment.

### Introducing the participant

Unmount the display. Brief the participant on controller buttons and
headset handling. You can let the participant choose between the right-
or left-hand controller, both are set up to work accordingly.

Typically, pressing A/X or the trigger will continue with the next trial
or instruction screen. After any partial/whole report trial,
participants are asked to report letters. To do so, they are displayed a
virtual keyboard, on which they can operate with the controller
(selecting keys with A/X/trigger). Once they confirm/close the keyboard,
they will see feedback (during practice trials) or continue to the next
trial (in following experimental blocks).

The first block consists of 24 practice trials with immediate
single-trial feedback. After this, participants should be familiar with
the procedure. Trials in the following experimental blocks count 36
trials each and do not display trial feedback but a summary with the
percentage correct at the end of the block.

## Output

Responses, reaction times, and display durations are logged as a CSV
file of the form `Output_0000.csv`, where `0000` is the subject number.
All files are created under
`[/storage/emulated/0]/Android/data/dk.KU.VRCTVA/files/results`, where
they can be downloaded via USB.
