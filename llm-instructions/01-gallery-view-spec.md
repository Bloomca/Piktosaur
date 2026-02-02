# Gallery view (Slideshow) feature

This feature adds a possibility to play current folder in a slideshow format, automatically switching to the next picture after a certain period of time.

## Constraints

This feature will have a limited scope for this implementation for simplicity:

- it will open a new borderless window (current app window won't be affected)
- there will be no initial customizing options
- there won't be an option to skip any pictures, it will just iterate through all pictures in the folder recursively
- no animation on image change for now

## Implementation details

The gallery view status (open/closed) will be kept in the AppStateVM, but the actual window status will have a separate VM which will be instantiated in the constructor.

The window should have the following features:

- borderless full screen (cover the taskbar)
- pressing `escape` will close the window
- pressing `space` will pause the slideshow
- pressing `left` or `right` arrows will move back or forward
- a picture will change every 5 seconds

For simplicity, it will not have an overlay for the controls, that will be added separately.

To launch it, there will be a button in the titlebar, before other buttons. If the slideshow is active, the button will change from "play" to "stop", which will close the slideshow. When we start the slideshow, currently selected image will be the starting point (which is located under `SelectedImagePath` field in `AppStateVM`). If it is `null`, then just start from the first one.

### Image data service

To share image data between `ImagesListVM` and the gallery VM, we'll introduce a new service (e.g., `ImageQueryService`) that owns the image discovery logic and caches results.

Responsibilities:

- Execute folder queries based on `SelectedQuery` from `AppStateVM`
- Own the `ObservableCollection` of image file data (progressive loading adds items as folders are scanned)
- Expose the collection to consumers (main window binds directly to it)
- Notify subscribers when the list changes (e.g., after a new query or refresh)

Both `ImagesListVM` and the gallery VM will depend on this service rather than managing their own image lists. The service will be registered as a singleton.

**Snapshot model for slideshow:**

When the slideshow opens, it copies the current file paths from the service's collection into its own list. This means:

- The slideshow is independent of query changes in the main window
- If the user changes the query while slideshow is running, the slideshow continues with its original list
- Thumbnails (which consume memory) are not copied—only file path data
- If scanning is still in progress when the slideshow opens, it gets whatever's loaded at that moment
- Closing the slideshow releases its snapshot; the service can freely clear/repopulate for new queries

This approach enables future extensibility:

- A "disabled images" feature can be implemented at the service level, filtering out disabled images before exposing the list
- Any new views that need image data can simply depend on the same service
- Sorting/filtering logic can be centralized

## Implementation order

1. Create a new service `ImageQueryService`
2. Migrate `ImagesListVM` to use it
3. Add a new button for the slideshow in the titlebar
4. Open a new window when clicking on the button (just show the selected image)
5. Make the window full screen (borderless, covering taskbar)
6. Make it so that pressing `Escape` closes the slideshow
7. Automatically switch to the next image every 5 seconds
8. Add pausing when pressing `Space`
9. When the slideshow is open, change the titlebar button to a "stop" icon; pressing it closes the slideshow

## Edge cases

- if there are no images, the button will be greyed out
- if there is one image, the slideshow opens normally
- once we reach the end, we just loop it back to the beginning
- if image fails to load, we won't do anything for now (so the screen will be just dark, I guess)

## Resolved questions

**Image data sharing:** Resolved by introducing `ImageQueryService` (see Implementation details above).

**Main window behavior:** The main window stays visible in the background when the gallery opens. User can alt-tab back if needed, and closing the gallery returns focus naturally.