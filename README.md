# NPGR003-23
Support for ***NPGR003 (Introduction to Computer Graphics)*** lecture.
Year 2023/24.

See [NPGR003 lecture - current info](https://cgg.mff.cuni.cz/~pepca/lectures/npgr003.current.cz.php).

## Lab plan
Every `NN-*` directory refers to an item in the lab plan
(usually you will find a programming task there). You don't have
to do all of them to pass the lab, 1/2 to 2/3 is enough (see the
points in task definitions and in the shared table).

This global `README.md` defines general rules and conditions,
see individual directories for task-specific information.

## Point table
See [this shared table](https://docs.google.com/spreadsheets/d/1QLukOcSRPa5exOYW1eUfQWY2WoMjo1menbjQIU7Gvs4/edit?usp=sharing)
for current points. Please check the due dates of each task.
The primary source of organizational information is **the shared table**, not
the GIT repository.

Contact me <pepca@cgg.mff.cuni.cz> with any suggestions, comments or
complaints. If you are in a different lab group, please contact your
lab supervisor first.

## C# source files in task directories
Some directories contain support files from the teachers. We are using the `.NET 6`
(select `.NET 6.0 (Long Term Support)` while creating a new project),
it works well on Windows, Linux and macOS.

We use `Visual Studio 2022`, the [Community](https://visualstudio.microsoft.com/vs/community/)
(free) version is good enough for all tasks.

## Solutions
Please place all your solutions in separate [solutions](solutions/README.md)
directory in the repository. **One subdirectory per task**.
You'll find short instructions there.

The naming conventions in the standard Visual Studio project creation procedures
allow you to simply copy a pilot task project to the `solutions` directory.
So if you want to extend a pilot project, just copy everything in a task directory
to the `solutions` directory, rewrite the `README.md` and you are ready
to work on your copy. To be safe, remove the `.vs`,
`obj` and `bin` subdirectories first.

Example of files and directories that should be copied to the `solutions` directory:
```
/01-AllTheColors/
/01-AllTheColors/01-AllTheColors.csproj
/01-AllTheColors/01-AllTheColors.sln
/01-AllTheColors/Program.cs
/01-AllTheColors/README.md    (should be overwritten later by README-TEMPLATE.md)
/01-AllTheColors/Properties/
/01-AllTheColors/Properties/launchSettings.json
```

Template for your documentation of the finished task is [here](solutions/README-TEMPLATE.md). Copy
the file to every directory with your solution (over the original `README.md`) and
edit all the sections.

## AI assistants
The use of **AI assistants (based on Large Language Models)** is not prohibited,
on the contrary! We welcome you to use them under two conditions:
1. you must **acknowledge** for each task that the AI assistant significantly
   helped you.
2. you must **document your use of the assistant**. For example, if you use
   ChatGPT, record the entire conversation and post a link to it.
   For more "built-in" assistants, you should write a verbal report of
   what the help looked like, how often (and how hard) you had to
   correct the machine-generated code - and if you used comments in
   the code, leave them in!

Credit bonuses/maluses for using an AI assistant are still undecided.

Over the course of the semester, some of you will have the opportunity
to write short reports and present your experiences to the rest of the
group (and you'll get extra credit for doing so).

## GIT instructions
You will work in your own **private repositories**.
You will start from our shared repository and add your own code and
documentation as you solve individual tasks.

We recommend using one of the following platforms - there are more
detailed instructions for each of them. The only bigger difference
is in the 3rd step (granting permissions to us = lab supervisors).

### GitHub
1. You have to set up a new **private repository** yourself.
2. Connect it to
our shared GIT using `git remote`. The command might look like this
```bash
$ git remote add origin https://github.com/pepcape/NPGR003-23.git
```
3. Finally, you have to give us permissions to access your private
repository, this is done using the **"Collaborator"** role.
Depending on who your lab supervisor is, you invite either
https://github.com/pepcape or
https://github.com/tomasiser.
4. If your GitHub username is a **nickname**, please email us with your
real name.

### GitLab (MFF UK server)
1. You have to set up a new **private repository** yourself.
2. Connect it to
our shared GIT using `git remote`. The command might look like this
```bash
$ git remote add origin https://github.com/pepcape/NPGR003-23.git
```
3. Finally, you have to give us permissions to access your private
repository, this is done using the **"Reporter"** role.
Depending on who your lab supervisor is, you invite either
https://gitlab.mff.cuni.cz/pelikan or
https://gitlab.mff.cuni.cz/isert.

## Notes
* If anything doesn't work well in your **Linux/macOS environment**,
  you should write me (<pepca@cgg.mff.cuni.cz>) as soon as possible.
  Of course you could report positive experience in Linux/macOS as well.
* You can work in your repositories without major restrictions.
  A recommended place for your solutions is the [solutions](solutions/README.md)
  directory. You'll find short instructions there.
* We suggest that you add a **tag** with the name of the task to the repository
  at the time of submission (for example, `01-solution`).
  See the [git tag documentation](https://git-scm.com/book/en/v2/Git-Basics-Tagging).
* Of course, it wouldn't hurt to send us an **email** to let us know that
  you've solved the task. In time, we will see if the "tag" system is sufficient.
* If you want use any third party library, do it correctly and use the
  [NuGet system](https://www.nuget.org/). Many pilot projects are
  using libraries already (e.g. [SixLabors.ImageSharp](https://www.nuget.org/packages/SixLabors.ImageSharp)),
  so learn by example.
* For all your documentation please
  use the [MarkDown (.md)](https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax)
  syntax ([another MarkDown page](https://www.markdownguide.org/basic-syntax/))
* Use the usual `README.md` in each solutions' directory (see the
  [solutions/README.md](solutions/README.md) for instructions)!
* Visual Studio 2022 supports direct **MarkDown editing** (with live
  result preview) starting from the 17.5 update
