
# Streaming Tilemap

This demo implements an "infinite" tilemap that uses _chunks_ (instead of a single file) that make up a tilemap. In other words, the tilemap is not made of a giant 2-dimensional array but small parts that represent portions of visible tiles. This approach has the potential of consuming less memory than a single rectangular array of tile data.

Chunk management is based on camera position where the chunks are dynamically loaded and discarded in a way that only the immediate surrounding chunks are kept in memory (thus the term _"streaming"_). While this demo asynchronously loads chunks from disk, we could easily be streaming them over the network, too. There are no loading breaks, everything will be loaded in the background. The observer (or _player_) will get the impression of seamless, infinite map.

This demo uses [Veldrid](https://github.com/mellinoe/veldrid), a cross-platform graphics API abstraction for **.NET Core** that supports various graphics backends including **Direct3D11**, **Vulkan**, **Metal**, and **OpenGL**. In theory, this demo (the _Engine_ part) runs on **Windows**, **MacOS**, and **Linux** however this demo only contains a configuration for Windows. For other platforms you only need to change the entry point that bootstraps the game. For more information, see demo code on _Veldrid Samples_ repository.

This demo consists of the **Editor** (WPF application, Windows only) and the **Engine** (Veldrid powered cross-platform "game"). The game itself features an example map that contains a handful of chunks, and you'll be controlling the camera.

> **NOTE:**  
> The Editor is Windows only, but the chunk format is very simple and you can eaily create them manually as well. See section _"Chunk format"_ for more information.

## Requirements

- Visual Studio 2017 (Community edition will do)
- .NET Framework 4.7.2
- .NET Core 2.1 SDK and runtime

The source code is written in C# 7.3.

> **TIP:**  
> In Visual Studio Installer app, select workloads **".NET desktop development"** and **".NET Core cross-platform development"**.

## Building & Running

1. Open the Solution in Visual Studio and Rebuild it (this should run _Nuget Restore_).
2. Set **Engine** as the Startup project and run it.

The game will open an SDL2 window (centered on screen). The window fits exactly one chunk (which is 9x9 tiles in size). Tile size is 32x32 pixels, so window ends up being 288x288 pixels. These can be configured in the **Configuration.cs** file. The chunk size is of uneven tiles because the possible player sprite would occupy the middle of the screen. If you want to edit the tiles' appearance, just modify the **tileset.png** file.

Tileset size is not limited, but it should respect the tile size intervals (32 pixels in this case). Also, the atlas texture should be square, and its width & height be a _power of two_. If you need a bigger atlas - that can fit more tiles - a 1024x1024 would do fine for example. For best compatibility, make sure to save the file with 96.0 DPI.

To change the graphics backend, edit **Program.cs** and specify it in:
```csharp
window.Run(GraphicsBackend.Direct3D11);
```

> **NOTE:**  
> `Metal` is only available on a MacOS and `Direct3D11` is only available on Windows. `Vulkan` should work on all platforms (as long as you have relatively modern a GPU and up-to-date drivers), but uses more memory than the other options (dunno why, could be a Veldrid thing). `OpenGL` is not necessarily as smooth as the more modern options.

> **NOTE:**  
> The demo uses pre-compiled shaders (\*.hlsl.bytes, \*.spv, and \*.metallib).  
> - To compile HLSL, start the _Developer Command Prompt for VS 2017_ and run the **compile-hlsl.cmd**. If you don't have `fxc`, install the DirectX SDK.
> - To compile Vulkan, run **compile-spirv.cmd**. You'll need the Vulkan SDK.

> **TIP:**  
> Veldrid also supports SPIRV, so you could first compile GLSL/HLSL to SPIRV format and then import your shaders from SPIRV. The _Veldrid Samples_ repository has more examples how to load SPIRV.

### Engine controls

- ⬅️➡️⬆️⬇️ Move camera
- Hold **SHIFT** to move faster
- **Q** to reset camera position back to chunk (0, 0)

### Some caveats

Don't do the same mistakes than I did and waste time on these things:

- When creating the GraphicsDevice, set `preferDepthRangeZeroToOne: false` or else your **OpenGL** backend will not render anything.
- When creating the GraphicsDevice, set `preferStandardClipSpaceYDirection: false` or else your **Vulkan** backend will render everything vertically mirrored.
- When creating the GraphicsDevice, set `swapchainDepthFormat: null` and the pipeline's `DepthStencilState = DepthStencilStateDescription.Disabled` or else it's easy to select the wrong _depth test_ and end up rendering nothing.
- Use `PrimitiveTopology.TriangleList` instead of `PrimitiveTopology.TriangleStrip` - the latter just isn't suitable for a tilemap scenario.

## Editor

The Editor is a standard WPF application using the MVVM design pattern (with no dependencies to any MVVM mini frameworks). **BindingSource** implements the `INotifyPropertyChanged` which offers the databinding capability. The **ViewModel** base class is derived from it.

The application spawns multiple windows:

1. The **MainWindow** contains the actual tile editor. It draws 3x3 chunks, and camera can be moved (a chunk at a time) via the _arrow keys_.
2. The **Tileset** tool window that acts as _palette_; user can select the Foreground brush (left click) and the Background brush (right click). Painting on the MainWindow happens with the same respective mouse buttons. This behavior is similar to most painting/drawing applications.
3. The **MapBrowser** tool window allows user to create and delete tilemaps, as well as choose the currently active map. This window sets up some FileSystemMonitors that detect changes to the **Maps** folder structure and refresh the UI automatically. The Map browser also features a _minimap preview_ that visualizes the _shape_ of chunks within the world. **White** blocks are chunks, **Black** blocks contain no chunks, and the (0, 0) origin chunk is visualized in **Red**.

 Their view models share information via the **Context** object. The Context also provides access to common _sevices_ such as the **AssetManager**.

Currently the **AssetManager** provides loading and saving functionality for chunks. Basically it handles chunk file reading and writing. The outside facing API is simply "give me tile data from these chunk coordinates" and "save this chunk's tile data". Chunk data is requested when the camera moves (and the chunks need to be repopulated), and saved on each _Paint_ action. Currently saving is not throttled and everything operates in real-time. Chunks are rather small (9x9) so this is not that big of a problem, but there's definitely some room for improvement.

### Main view

There's a 3x3 `UniformGrid` (according to which the window is automatically sized). Each cell contains their own designated view model, representing the chunk's data. The 9 **ChunkViewModels** are created once and stay static after that - only their content gets updated when the camera moves (or a Paint/Erase occurs).

Mouse events for painting and erasing are handled in the view's code-behind (I don't use any _Behavior_ magic in this app), and then delegated to the view model.

When the _focus_ is on the main window, move the camera with the arrow keys ⬅️➡️⬆️⬇️.

#### Chunks

To render chunk data (tiles), this application uses **WriteableBitmaps** that can be used as _ImageSources_ for WPF `Image` elements. For blitting bitmap regions from one bitmap to another, I use the [WriteableBitmapEx](https://www.nuget.org/packages/WriteableBitmapEx) library. Each chunk has a 9x9 array for tiles, and for each tile we blit the appropriate 32x32 region from the tileset bitmap to the chunk's bitmap.

The tile array holds 1-based index values to the tileset. Zero means empty (= no tile) whereas 1 means the first tile in the tileset. For zero values we just present a black rectangle.

### Tilesets

The tileset is an atlas texture that contains each tile arranged in a grid. The top-left tile corresponds index 0, its first right neighbor 1 etc. Tile indices flow like text, wrapping onto the next "line":

<table align="center">
    <tr>
        <td align="center">0</td>
        <td align="center">1</td>
        <td align="center">2</td>
        <td align="center">3</td>
    </tr>
    <tr>
        <td align="center">4</td>
        <td align="center">5</td>
        <td align="center">6</td>
        <td align="center">7</td>
    </tr>
    <tr>
        <td align="center">8</td>
        <td align="center">9</td>
        <td align="center">10</td>
        <td align="center">11</td>
    </tr>
    <tr>
        <td align="center">12</td>
        <td align="center">13</td>
        <td align="center">14</td>
        <td align="center">15</td>
    </tr>
</table>

**Tileset.cs** contains some helper methods for calculating the 0-based index from the given row and column as well as determining the row and column that corresponds the given index. The Editor version of Tileset.cs contains the loaded bitmap, and is able to blit tiles to another bitmaps. The Engine version doesn't contain the texture itself, and is mainly used for tile index calculations.

Tile index is assumed to be zero-based everywhere else except in the chunk's tile array where zero means _"no data"_ i.e. just empty black rectangle. Number 1 refers to the first tile in the tileset. The reason for this is purely the serialization format, which is an _unsigned 16-bit integer_, or `ushort`.

The Editor spawns a separate tool window that contains the tileset from which the user can select the foreground and background brushes using the left and right mouse buttons, respectively. Current brushes are stored in the shared **Context** (from where the **MainViewModel** can access it).

Every bitmap surface - where tiles are shown - is basically a `WriteableBitmap`, even if the bitmap itself wouldn't be changed. The **WriteableBitmapEx** library only provides extension methods for WriteableBitmaps.

> **TIP:**  
> If you get strange exceptions from the **WriteableBitmapEx** library during blitting, debug the bitmap's size and DPI values; my Paint.NET once saved the PNG file with DPI 95.9... instead of exactly 96.0 DPI which caused unexpected size-effects and an exception. I re-saved the image in another application and it fixed the problem.

### Maps

The tilemaps are stored in the **"Maps"** folder that will then contain sub folders for the individual maps. Each individual map folder contains a number of .chunk files. The chunk filenames follow a naming convention `x,y.chunk` where both **x** and **y** represent chunk coordinates. For example, the origin chunk's filename would be **"0,0.chunk"** and one directly to the north-west would be **"-1,-1.chunk"** (the Y-axis grows downward on screen).

The files are written realtime when the user paints on them. The **Map browser** (tool window) sets a **FileSystemWatcher** that monitors the map folders, and updates the map list on the UI as needed.

> **CAUTION:**  
> The editor always creates a **Default** map. During application startup this folder is cleared so it's an empty workspace every time. Be sure to create a new map and edit that instead so that your work will not be lost.

The **Clean** button in the map list will simply remove chunks that are completely empty. You may have touched a chunk previously, but later cleared it back to zero. In this case the chunk file would still exist even though it contains no meaningful data. It's a good practice to run a Clean operation before copying the chunk files to, say, the Engine's map folder.

## Chunk format

A chunk contains 9x9=81 tiles. Each tile is encoded as an `ushort` and takes 2 bytes. There are no line breaks i.e. all tiles are consecutive. Value zero means "no data" and is rendered as a black rectangle. Any other value indicates the tile index. For example, number 1 refers to the first tile in the tileset texture.

The chunk file only contains the raw tile data - the coordinates are inferred from the filename. Therefore the exact file size for each chunk is 81x2=162 bytes.

The small chunk size would enable streaming over network scenarios, too.

## Engine

The Engine is a .NET Core application and uses [Veldrid](https://github.com/mellinoe/veldrid). The published application contains assemblies from [ImageSharp](https://github.com/SixLabors/ImageSharp), [SharpDX](https://github.com/sharpdx/SharpDX), and [SDL2](https://www.libsdl.org/). The app spawns a simple 288x288 game window, which is just enough to fit a _chunkful_ of tiles in it. You can move the camera with the arrow keys ⬅️➡️⬆️⬇️, speed it up by holding **SHIFT**, and reset the camera with **Q**.

- The .chunk files are stored as **Content**, and copied to the **Maps/Overworld** folder
- The tileset texture "tileset.png" is stored as **Content**, and copied to the **Assets** folder
- The shaders are stored as **Embedded resource**, and therefore compiled into the assembly. All shader files start with "Basic" and then specify whether they're for vertex or fragment stages. This naming convention is important. HLSL, Metal, and Vulkan shaders are included both as source code and pre-compiled forms. GLSL and GLSLES are source-only

If you need to make changes to the shaders, you must re-compile them. I've included **compile-hlsl.cmd** and **compile-spirv.cmd** for compiling the HLSL and Vulkan shaders, respectively.

The current default is **Direct3D11**, but you can change the preferred graphics backend in **Program.cs**.

### Game window and the Game base class

The hosting window and the game logic are separate classes, but a game's lifetime is tied to that of the window's. Basically the window must exist first, and then the game instance is attached to the window.

```csharp
var window = new GameWindow("Window title");
var game = new MyGame(window);
window.Run(GraphicsBackend.Direct3D11);
```

To create a game class, simply derive it from the `Game` base class:

```csharp
public class MyGame : Game
{
    public SampleGame(IGameWindow window) : base(window)
    {
    }

    protected override void CreateResources(ResourceFactory factory)
    {
    }

    protected override void FreeResources()
    {
    }
    
    protected override void Draw(float deltaSeconds)
    {
    }

    protected override void Update(float deltaSeconds)
    {
    }

    protected override void OnKeyDown(KeyEvent state)
    {
        if (state.Key == Key.Escape)
        {
            this.Exit();
        }
    }
}
```

The `Game` base class gets the window instance, subscribes to some of its events, and will then call the virtual methods at appropriate times:

- `CreateResources` will be called first. Use the passed factory to create any graphics _resources_ (buffers, layouts, resource sets, pipelines etc.)
- `Update` will be called once per frame, just before `Draw`. You can check player input, update physics, and other gameplay related things here.
- `Draw` will be called once per frame. Update shader buffers and issue the rendering commands here. Finally, swap the _backbuffer_.
- `FreeResources`will be called when the window has closed and any remaining resources need to be released. You'd typically store all resources created by the `ResourceFactory` into class fields, and then Dispose them in this method.

This demo uses an SDL2 window (also OpenTK is an option). There's a special value we can use in place of X and Y coordinates, `Sdl2Native.SDL_WINDOWPOS_CENTERED`, that allows us to center the window. This is handy because the .NET Core BCL doesn't provide a way to enumerate display devices (and to calculate the centered position).

The Window object itself doesn't provide many useful properties (apart from `Resizable = false` that is used in this demo), but should you need more advanced window controls, check out the **Sdl2Native** class:

```csharp
Sdl2Native.SDL_SetWindowFullscreen(window.SdlWindowHandle, SDL_FullscreenMode.FullScreenDesktop);
Sdl2Native.SDL_MaximizeWindow(window.SdlWindowHandle);
```

### Handling input

The GameWindow takes a snapshot of key and mouse events once per frame. These events are handled by the **InputTracker** that keeps the global key and mouse state i.e. what keys and buttons are currently being held down and what buttons received a _Down_ event during the most recent update. This class is static, and therefore easily accessible from everywhere. However, it's not thread-safe. You should process its state in the game's `Update` method.

> **TIP:**  
> Mouse position is also accessible from the InputTracker.

### Chunk management

The _World_ contains _chunks_ that make up the map. Within the world there's also a _Camera_ (that the user can move with the arrow keys). X coordinates grow to the right, and Y coordinates grow downwards (meaning that when the camera moves up, its Y coordinate decreases). Both coordinates can go negative. In fact, the camera starts at position (0, 0) and you can then move to any direction.

Camera coordinates are in pixels. During the _Update_, camera movement speed is proportional to the screen FPS so that the actual graphics performance doesn't have any impact on the observed scrolling speed on screen; it'll take the same amount of time to scroll from one location to another regarless of what FPS the game is running. This means that camera coordinates are of `double` type.

Each frame, we calculate which chunk is directly at the camera's location. This is the main chunk we're interested in because it'll determine which other chunks are loaded or discarded. Ideally, we want to keep enough surrounding chunks loaded in the world so that when the camera moves there's always something visible that scrolls into view. This gives the impression of seamless "infinite" map with no loading breaks.

The **ChunkManager** keeps a cache of _active_ chunks in the world. "Active" here means that it's close enough to the camera's chunk, and is either still loading or has finished loading tile data (we load the data asynchronously). Whenever the camera position is updated, the ChunkManager will go through its cache and throw away chunks that are too far away from the camera. Then it'll see what chunks are near the camera and load any that are not yet in the cache.

Chunks are _pooled_ so that we don't create new tile arrays every time we add a chunk to the cache. Instead we withdraw an unused chunk from the pool, and then start loading its data. When chunks are discarded, they're simply returned to the pool. This technique avoids creating GC garbage.

When rendering, the game asks the ChunkManager for tile data of a certain chunk (based on chunk position). If no chunk exists at those coordinates, it returns just an empty array, full of zeroes, that gets rendered as all-black. Otherwise the chunk's real tile data is returned (this can still be full of zeroes if the chunk hasn't finished loading).

Chunks that need to be loaded are queued to **ChunkProcessor** that runs the queue in a separate thread. No loading job is added if the same chunk (for the same location) is already queued.

Also ChunkProcessor is notified about the camera position. This is to prevent unnecessary loading jobs that could occur when the chunk whose data should be loaded next has already been discarded for being too far away from the camera. Those jobs will just be skipped.

The loading queue uses a `BlockingCollection<T>` that implements the producer-consumer pattern. Although the current demo doesn't really use true parallelism, this solution will scale if needed. We could easily have 4 _processor_ threads for loading multiple chunks simultaneously (for example, over the network).

Loaded data gets written (from a stream) to a _staging array_. Once complete, the staging array's contents gets _copied_ to the chunk's _back buffer_, and the staging array can then be re-used for the next loading job.

Yes, each chunk actually has two tile arrays, one of which is in use (and being rendered from), and the other being a temporary place-holder for new data. This is because chunk loading is asynchronous and otherwise we could accidentally end up updating the array while it's being used for rendering. This might only have 1 frame worth of visual artifacts - which might not be such a big deal - but imagine if your game had collision detection that depends on data integrity (during the _Update_ phase). Then partial updates can have more serious consequences.

So just at the beginning of _Update_ we tell all cached chunks who have obtained data to their back buffer, to flip it. Essentially the back buffer then becomes the current buffer and vice versa. This happens in the main thread.

### Some caveats

Usually you calculate chunk/tile coordinates based on camera position simply by dividing an integer by the size. For example:

```csharp
int tileSize = 32;
int cameraX;

var tileXUnderCamera = cameraX / tileSize;
```

This works as long as all coordinates are >=0. To also cover negative numbers, `Math.Floor` is required:

```csharp
var tileXUnderCamera = (int)Math.Floor((double)cameraX / tileSize)
```

So you convert `int`→`double`, divide, take `Floor` and then convert back to `int`. Yuck.

What we'd really need is a _floor division_ like `a//b` in Python, but C# doesn't provide one out-of-the-box. So I added helper methods `DivFloor` ad its corresponding `ModFloor` in **Utility.cs**. Those operate with both positive and negative integers.

## How to: Load editor maps in the demo

Let's say you have created a map named **"test"**. You'll find the chunk files in **Editor\bin\Debug\Maps\test**.

Before copying the files, run the **Clean** operation from the Map browser's list. This will remove unused chunk files.

Then simply copy all the .chunk files to **Engine\bin\Debug\netcoreapp2.1\Maps\Overworld**. Make sure you use the same tileset image you used with the Editor (both the Editor and Engine projects contain one).

> **NOTE:**  
> When running from Visual Studio, chunk files are copied to the output folder when the app is built. This operation will naturally overwrite the chunk files in the target folder. To prevent an unintended overwrite, include the chunk files in the project, mark them as **Content**, and set `Copy to Output Directory=Copy always`.