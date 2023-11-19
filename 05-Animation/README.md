# Task 05-Animation
Your task is to animate a mandala[s] created in the previous task
[04-Mandala](../04-Mandala/README.md).

For the **definition of a mandala**, ideas and inspiration see
the previous task.

## Animation
For this `05-Animation` task, you'll create a sequence of raster images
on disk, that will eventually be encoded as a video file.

We won't accept mandala animations based only on color changes, a non-trivial
shape metamorphosis/transition must be involved!

You may find the [page on easing functions](https://easings.net/) useful.

# Pilot project
There is a VS pilot project that shows how to create a sequence of PNG
images. All output images are stored in the `anim` subdirectory,
where you will find a batch `encode.bat` for your convenience.

Please consider the pilot project as a demonstration of how to use
the `string.Format()` function to format the filenames of individual frames.

# Your solution
Please place your solution in a separate [solutions](solutions/README.md)
directory in the repository. You'll find short instructions there.

In this assignment, in addition to creating the implementation, you must
**create a video file and upload it to one of the public platforms**
(YouTube, Vimeo, Google Drive...). Don't add your video files into
a GIT repository!

# Encoding a video file with FFmpeg
Let's assume you already have individual frames stored on your hard drive. Now you need
to encode a video file from them. MPEG-4/H.264 encoding is preferred,
envelope could be AVI file format, MP4 or MKV.

We recommend the command-line program
[FFmpeg](https://ffmpeg.org/)
with many of options and able to use many popular video and audio codecs
(actually there is a static-linked version of the utility with all the codecs included).
A binary package for Windows is available
[from this page](https://ffmpeg.org/download.html).
By executing a single line command you will be able to encode a video file
from a sequence of raster images.

A typical command-line for [ffmpeg](https://ffmpeg.org/) video encoding looks like this
```bash
ffmpeg -framerate 30 -i out%04d.png -f avi -vcodec msmpeg4v2 -q:v 2 -y out.avi
```
All available arguments will be listed after you type `ffmpeg -h`, for the list of codecs
you can use `ffmpeg -formats` (our recommendation - use the `msmpeg4v2` codec, which
is installed on every Windows machine by default).

Important arguments
* `-i <input-mask>` C-like format string defining all the frame files
* `-f <file-format>` file format (aka envelope)
* `-vcodec <codec-name>` defines the most important part = which codec to use for a video stream
* `-q:v <q>` defines the quality of the video encoding (higher values mean lower quality & bitrate)
* `-y <output-file>` output file (`-y` means that ffmpeg will automatically rewrite the old file)

For advanced settings try the [FFmpeg wiki](https://trac.ffmpeg.org/wiki), special page about
creating a video from individual frames is called [Slideshow](https://trac.ffmpeg.org/wiki/Slideshow).

# Launch date
**Monday 13 November 2023**
(Don't work on the solution before this date)

# Deadline
See the shared [point table](https://docs.google.com/spreadsheets/d/1QLukOcSRPa5exOYW1eUfQWY2WoMjo1menbjQIU7Gvs4/edit?usp=sharing).

# Credit points
**Basic solution: 7 points**
* all generated frames must be circular symmetric to resemble a mandala (see the previous task `04-Mandala`)
* image size in pixels via arguments
* output filename mask specified in an argument
* C# program creates animation frames as a sequence of PNG images on disk
* encoding of the video-file (e.g. using the `FFmpeg` utility)
* publishing of the video on some public platform (YouTube, Vimeo, Google Drive...)
* documentation in the `README.md` file (including definition of all command
  line arguments and URL of the uploaded video)

**Bonus points: up to 7 more points**
* invention, animation invention
* visual appeal
* cyclic animation (plays smoothly in the "auto-repeat" mode)

## Use of AI assistant
Using an AI assistant in this task will be very difficult. Sure, you can use
it for advice on how to plug in the libraries you need, or how to draw
a geometric shape using the `ImageSharp` library, but don't expect it to
give you meaningful advice on how to draw or animate a mandala.

If you could get him to give some useful advice on that, I'd be very
interested.

# Example
You can find many relevant geometric drawings using this
[Google search](https://www.google.com/search?q=mandala+image).
