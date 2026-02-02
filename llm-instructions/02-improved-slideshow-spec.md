# Improved slideshow feature

The goal of this spec is to make the slideshow feature more customizable and to have a better UX. It has a lot of small improvements, but nothing major like in the original implementation.

## List of features

- add an ability to pause/start when pressing `space`
- add a very subtle animation on image change
- add settings flyout menu (an icon in the titlebar) for random image, no animation, skip collapsed folders
- add a component to show controls when you move your mouse: start/stop automatic change, previous/next picture, and go to the first picture
- in the component, show a progress bar until the next picture
- hide cursor when is not used for ~3 seconds
- component should also disappear after 3 seconds, but add a subtle ease-out animation (very short)

## Implementation details

Let's create another global VM for settings, in the future it will probably get expanded with other options. Also, for now it will be purely in memory with some defaults, but in the future we'll add a service to save changes to some configuration file.

Settings flyout menu will contain only slideshow options for now, but we need to add a title there regardless, so it can be easily extended in the future. Also, it should not close when we interact with options.

The animation on change needs to be determined manually. If it is simple to extend, we can add it as a setting and choose what feels the best, maybe even keep it there.

## Controls component

The component should be a rectangle at the center bottom (with some margin) with all the icons listed in a row: "to the first", "previous", "play/pause", "next", "to the last". Make a progress bar at the bottom, which is basically another rectangle filling until the next image, it needs to be flush with the bottom of the component. We can also try to experiment with having the entire background changing moving from left to right, but let's start with the small bar at the bottom.

The component should fade away after ~3 seconds, but it should do so independently from the cursor for extensibility.

When the user presses `space` to start/stop (even without the mouse interaction), we need to show the controls component -- this will help users understand that it is switching automatically now.

## Settings

Let's add support for these options:

- choose animation: none, and then a few options (fade, slide, zoom)
- use random image: next/previous buttons always select a random image
- skip collapsed folders: skip images from the folders which were collapsed at the time of launching the slideshow

## Implementation order

1. Add a component to the slideshow window with working button controls (play/pause, next, previous, first and last)
2. Add a progress bar flush to the bottom (we will decide if it looks good or we want to experiment more)
3. Make the component disappear if there is no mouse movement within 3 seconds (if the user is hovering it, it should not disappear)
4. Add settings flyout menu with the list of options for the slideshow component
5. Wire these options to the actual slideshow component
6. Implement animation changes on image switch in the slideshow component (and add different ones to the settings flyout)
7. Make the cursor disappear after ~3 seconds, if not hovering the controls component