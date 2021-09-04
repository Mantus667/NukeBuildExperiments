# Setting up Nuke

Setting up and installing Nuke is pretty easy.

Just run `dotnet tool install Nuke.GlobalTool --global` to install it's tool.
After that you can run its setup script which asks some questions on how you want to setup Nuke.

A detailed guide can be found on the Nuke page [https://nuke.build/docs/getting-started/setup.html](here).

The basic setup at the end is a project where you define the different Tasks that you want to be able to execute in your pipeline and build files that actually execute them.

This works by executing for example the build.ps1 script and appending the task name: `.\build.ps1 pack`.
In this sample it would execute the pack task and create Nuget packages.

Have a look at the documentation of Nuke for further infos on creating tasks, using parameters in your scripts and integration with different CI systems.