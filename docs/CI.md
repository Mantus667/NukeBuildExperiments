# Running CI tasks

As this roject is hosted on Github I decided to use Github Actions to run different workflows, for example to have proper CI.

For this project I want to run my tests on different triggers. These triggers are pushes to different branches like 'develop', 'main and different feature branches. Also for merge requests to the 'develop'-branch tests should be run.

As everything is build with nuke we have to make sure that we run the right task with Nuke. The step in the workflow yaml looks like this:

```
- name: Run tests'
  run: sudo ./build.cmd Test
```

We run the build.cmd provided by Nuke setting the task we want to run. In this case "Test".
One issue can happen when the script should be run like this. Github possibly complains about missing execution rights on the file. To fix that there are two additional steps that have to be executed beforehand:

```
- name: Make build.cmd executeable
  run: chmod +x ./build.cmd
- name: Make build.sh executeable
  run: chmod +x ./build.sh
```